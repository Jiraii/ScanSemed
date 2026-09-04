const { app, BrowserWindow, globalShortcut } = require('electron');
const path = require('path');
const fs = require('fs');
const express = require('express');
const cors = require('cors');
const axios = require('axios');
const WebSocket = require('ws');
const { SerialPort } = require('serialport');
const { ReadlineParser } = require('@serialport/parser-readline');

// --- StateController & Queue Variables ---
let barcodeQueue = [];
let isProcessing = false;
function processNextInQueue() {
    if (isProcessing || barcodeQueue.length === 0) return;
    isProcessing = true;
    const barcode = barcodeQueue.shift();
    broadcastToUI({ type: "SCAN", payload: barcode });
}
function releaseQueue() {
    isProcessing = false;
    processNextInQueue();
}
// -----------------------------------------


let mainWindow;
const API_PORT = 9001;
const WS_PORT = 9000;
const BAUD_RATE = 115200;

// Configuration fallback
let HOSPITAL_API_BASE_URL = 'http://192.168.34.246/apiopd';
let SEMED_SOAP_URL = 'http://172.16.11.4:8788/axis2/services/DIHPMPFWebservice.DIHPMPFWebserviceHttpSoap11Endpoint/';
let OUTPUT_LR = 'L';

const configPath = path.join(app.getPath('userData'), 'config.json');

function loadConfig() {
    try {
        if (fs.existsSync(configPath)) {
            let configData = JSON.parse(fs.readFileSync(configPath, 'utf8'));
            HOSPITAL_API_BASE_URL = configData.HOSPITAL_API_BASE_URL || HOSPITAL_API_BASE_URL;
            // Update to use the one from config, but we will force it if it was the wrong 10.35.222.66 IP
            SEMED_SOAP_URL = configData.SEMED_SOAP_URL || SEMED_SOAP_URL;
            OUTPUT_LR = configData.OUTPUT_LR || OUTPUT_LR;
            
            // Auto-fix the wrong IP back to 172.16.11.4
            if (SEMED_SOAP_URL.includes('10.35.222.66')) {
                SEMED_SOAP_URL = 'http://172.16.11.4:8788/axis2/services/DIHPMPFWebservice.DIHPMPFWebserviceHttpSoap11Endpoint/';
                configData.SEMED_SOAP_URL = SEMED_SOAP_URL;
                fs.writeFileSync(configPath, JSON.stringify(configData, null, 2));
            }
        } else {
            saveConfig();
        }
    } catch(e) { console.error('Error loading config', e); }
}
function saveConfig() {
    fs.writeFileSync(configPath, JSON.stringify({
        HOSPITAL_API_BASE_URL,
        SEMED_SOAP_URL,
        OUTPUT_LR
    }, null, 4));
}
loadConfig();

// ---------------------------------------------------------
// Express Server setup
// ---------------------------------------------------------
const server = express();
server.use(cors());
server.use(express.json());

const staticPath = path.join(__dirname, 'dist', 'web-frontend', 'browser');
server.use(express.static(staticPath));

server.get('/api/settings', (req, res) => {
    res.json({ channel: OUTPUT_LR });
});

server.post('/api/settings', (req, res) => {
    try {
        OUTPUT_LR = req.body.channel || 'L';
        saveConfig();
        res.json({ success: true, channel: OUTPUT_LR });
    } catch(e) {
        res.status(500).json({ error: e.message });
    }
});

server.post('/api/proxy/packagemaster', async (req, res) => {
    try {
        const { basketid } = req.body;
        let baseUrl = HOSPITAL_API_BASE_URL;
        if(baseUrl.endsWith('/')) baseUrl = baseUrl.slice(0, -1);
        
        const response = await axios.post(baseUrl + '/packagemaster/order/semed', 
            { basketid: basketid },
            { timeout: 30000 }
        );
        res.json(response.data);
    } catch (error) {
        console.error('API Error:', error.message);
        res.status(200).json({ status: 500, error: 'HIS API Timeout or Error', data: [] });
    }
});

server.post('/api/proxy/semedstock', async (req, res) => {
    try {
        const { drugcode } = req.body;
        let baseUrl = HOSPITAL_API_BASE_URL;
        if(baseUrl.endsWith('/')) baseUrl = baseUrl.slice(0, -1);
        
        const response = await axios.post(baseUrl + '/dih/getsemedstock', 
            { drugcode: drugcode },
            { timeout: 30000 }
        );
        res.json(response.data);
    } catch (error) {
        console.error('Stock API Error:', error.message);
        res.status(200).json({ status: 500, error: 'HIS API Timeout or Error', data: [] });
    }
});

// XML Generation mapping completely migrated from C# gd4lib.dll to JS!
function generateSemedXml(payload, windowNoStr) {
    const p = payload.patientInfo || {};
    const drugs = payload.drugsList || [];

    const patID = p.hn || '';
    const patName = p.patientname ? p.patientname.replace(/[']/g, '') : '';
    const gender = p.sex || '';
    
    // Format Date helper
    const formatDate = (dateStr) => {
        if (!dateStr) return new Date().toISOString().replace('T', ' ').substring(0, 19);
        const dt = new Date(dateStr);
        if (isNaN(dt)) return new Date().toISOString().replace('T', ' ').substring(0, 19);
        return dt.toISOString().replace('T', ' ').substring(0, 19);
    };

    const birthday = formatDate(p.patientdob);
    const age = p.age || '';
    const QN = p.qn || '';
    const AN = patID;
    const orderNo = p.prescriptionno_sup || p.prescriptionno || '';
    const paymentDT = formatDate(p.ordercreatedate);
    const visitNo = p.vn || '';
    const deptCode = p.wardcode || '';
    const deptName = (p.wardname || "").substring(0, 15);
    const doctCode = p.doctorcode || '';
    const doctName = (p.doctorname || "").substring(0, 15);

    let drugsXml = '';
    payload.drugsList.forEach((drug, index) => {
        drugsXml += `
    <Drug>
        <Alias></Alias>
        <Code>${drug.code || ''}</Code>
        <Name>${drug.name || ''}</Name>
        <Spec>${drug.spec || drug.Strength || "N/A"}</Spec>
        <FirmName>${drug.firmName || drug.firmname || "NKP"}</FirmName>
        <Qty>${drug.qty !== undefined && drug.qty !== null && drug.qty !== "" ? drug.qty : (drug.orderqty || "1")}</Qty>
        <Unit>${drug.unit || ''}</Unit>
        <Method></Method>
        <Type></Type>
        <note>${drug.shelfzone || ''}</note>
        <ItemNo></ItemNo>
    </Drug>`;
    });

    const xml = `<?xml version="1.0" encoding="utf-8"?>
<OutpOrderDispense xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema">
  <Patient>
    <PatID>` + patID + `</PatID>
    <PatName>` + patName + `</PatName>
    <Gender>` + gender + `</Gender>
    <Birthday>` + birthday + `</Birthday>
    <Age>` + age + `</Age>
    <Identity></Identity>
    <InsuranceNo></InsuranceNo>
    <ChargeType></ChargeType>
  </Patient>
  <Prescriptions>
    <Prescription>
      <OrderNo>` + orderNo + `</OrderNo>
      <QN>` + QN + `</QN>
      <AN>` + AN + `</AN>
      <Ordertype></Ordertype>
      <Pharmacy>OPD</Pharmacy>
      <WindowNo>` + windowNoStr + `</WindowNo>
      <PaymentIP></PaymentIP>
      <PaymentDT>` + paymentDT + `</PaymentDT>
      <OutpNo></OutpNo>
      <VisitNo>` + visitNo + `</VisitNo>
      <DeptCode>` + deptCode + `</DeptCode>
      <DeptName>` + deptName + `</DeptName>
      <DoctCode>` + doctCode + `</DoctCode>
      <DoctName>` + doctName + `</DoctName>
      <Diagnosis></Diagnosis>
      <Drugs>` + drugsXml + `</Drugs>
    </Prescription>
  </Prescriptions>
</OutpOrderDispense>`;
    return xml;
}

server.post('/api/proxy/dispense', async (req, res) => {
    try {
        let payload = req.body; 
        let rawWin = String(payload.windowNo || OUTPUT_LR || "1").trim().toUpperCase();
        let windowNo = "1";
        if (rawWin.startsWith("R") || rawWin.includes("¢ÇÒ") || rawWin === "2") windowNo = "2";
        if (windowNo.toUpperCase() === "L") windowNo = "1";
        else if (windowNo.toUpperCase() === "R") windowNo = "2";
        
        let baseUrl = HOSPITAL_API_BASE_URL;
        if(baseUrl.endsWith('/')) baseUrl = baseUrl.slice(0, -1);

        // Generate XML entirely in Node.js, no C# needed!
        const innerXml = generateSemedXml(payload, windowNo);

        // Map the payload.drugsList to the format expected by update_resultSemed in C#
        const sendoredrdishPayload = payload.drugsList.map(row => ({
            alias: "",
            code: (row.code || "").toString().replace("/", "").replace("'", ""),
            name: (row.name || "").toString().replace("/", "").replace("'", ""),
            spec: "N/A", // C# checks stock for spec, defaulting to N/A
            firmName: "NKP",
            qty: (row.qty || "").toString(),
            unit: (row.unit || "").toString(),
            method: "",
            type: "",
            note: (row.shelfzone || "").toString(),
            itemNo: ""
        }));

        try {
            // C# calls update_resultSemed(json) to notify HIS
            const hisUpdateUrl = HOSPITAL_API_BASE_URL + '/dih/sendoredrdish';
            console.log("Sending update to HIS:", hisUpdateUrl);
            await axios.post(hisUpdateUrl, sendoredrdishPayload, {
                headers: { 'Content-Type': 'application/json' },
                timeout: 10000
            });
        } catch (hisErr) {
            console.error("Warning: HIS update /dih/sendoredrdish failed:", hisErr.message);
            // We continue even if HIS fails, to make sure the machine dispenses.
        }

        const soapEnvelope = `<?xml version="1.0" encoding="utf-8"?>
<soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema">
  <soap:Body>
    <outpOrderDispense xmlns="http://webservice.pmpf.dih.com">
      <xml><![CDATA[` + innerXml + `]]></xml>
    </outpOrderDispense>
  </soap:Body>
</soap:Envelope>`;

        const response = await axios.post(SEMED_SOAP_URL, soapEnvelope, {
            headers: {
                'Content-Type': 'text/xml;charset=UTF-8',
                'SOAPAction': 'urn:outpOrderDispense',
                'Connection': 'close'
            },
            timeout: 30000
        });

        // ==========================================
        // ðŸš¨ AUDIT FIX: Validate SeMed ACK/NACK (Code 0)
        // ==========================================
        let isSuccess = false;
        let semedErrorMsg = "Unknown Hardware Error";
        console.log("[SeMed Raw Response]:", response.data);
        const codeMatch = response.data.match(/(?:<|&lt;)code(?:>|&gt;)(.*?)(?:<|&lt;)\/code(?:>|&gt;)/i);
        if (codeMatch && codeMatch[1].trim() === '0') {
            isSuccess = true;
        } else {
            const msgMatch = response.data.match(/(?:<|&lt;)message(?:>|&gt;)(.*?)(?:<|&lt;)\/message(?:>|&gt;)/i);
            if (msgMatch) semedErrorMsg = msgMatch[1];
        }

        if (!isSuccess) {
            throw new Error(`SeMed Machine Rejected: ${semedErrorMsg}`);
        }

        await new Promise(r => setTimeout(r, 3000));
        res.json({ success: true, result: response.data });
        releaseQueue(); // Process next basket
    } catch (error) {
        console.error('Dispense API Error:', error.message);
        res.status(500).json({ success: false, error: error.message });
        releaseQueue();
    }
});

server.post('/api/proxy/release_queue', (req, res) => {
    releaseQueue();
    res.json({ success: true });
});
server.use((req, res) => {
    res.sendFile(path.join(staticPath, 'index.html'));
});

server.listen(API_PORT, () => {
    console.log(`API Server running at http://localhost:${API_PORT}`);
});

// ---------------------------------------------------------
// WebSocket Server for Scanner
// ---------------------------------------------------------
const wss = new WebSocket.Server({ port: WS_PORT });
let uiClients = [];
wss.on('connection', (ws) => {
    uiClients.push(ws);
    ws.on('close', () => { uiClients = uiClients.filter(c => c !== ws); });
});
function broadcastToUI(data) {
    uiClients.forEach(client => {
        if (client.readyState === WebSocket.OPEN) {
            client.send(JSON.stringify(data));
        }
    });
}

// ---------------------------------------------------------
// Serial Port Scanner
// ---------------------------------------------------------
const appStartTime = Date.now();

async function scanAndConnectSerialPorts() {
    try {
        const ports = await SerialPort.list();
        ports.forEach(portInfo => {
            const portPath = portInfo.path;
            try {
                const port = new SerialPort({ path: portPath, baudRate: 115200 });
                const parser = port.pipe(new ReadlineParser({ delimiter: '$' }));
                parser.on('data', (data) => {
                    // Ignore data received within the first 3 seconds (flush old buffer)
                    if (Date.now() - appStartTime < 3000) return;
                    
                    let rawData = data.toString().trim();
                    if (rawData.startsWith('#')) {
                        const parts = rawData.substring(1).split('|');
                        if (parts.length >= 3 && parts[2] !== '0') {
                            const barcode = parts[2].trim();
                            if (!barcodeQueue.includes(barcode)) {
                                barcodeQueue.push(barcode);
                                processNextInQueue();
                            }
                        }

                    }
                });
            } catch (err) { }
        });
    } catch (err) { }
}

// ---------------------------------------------------------
// Electron App Window
// ---------------------------------------------------------
function createWindow() {
    mainWindow = new BrowserWindow({
        width: 1200,
        height: 800,
        title: "BDSender SEMED Dashboard",
        kiosk: false,
        fullscreen: false,
        autoHideMenuBar: true,
        webPreferences: {
            nodeIntegration: true,
            contextIsolation: false,
            zoomFactor: 1.0
        }
    });
    mainWindow.maximize();
    mainWindow.setMenuBarVisibility(false);
    mainWindow.loadURL(`http://localhost:${API_PORT}`);
    mainWindow.on('closed', function () {
        mainWindow = null;
    });
    scanAndConnectSerialPorts();
}

app.on('ready', () => {
    createWindow();
    
    // Register Emergency Shortcuts
    globalShortcut.register('CommandOrControl+Shift+I', () => {
        if (mainWindow) mainWindow.webContents.toggleDevTools();
    });
    globalShortcut.register('CommandOrControl+Shift+Q', () => {
        app.quit();
    });
});
app.on('will-quit', () => {
    globalShortcut.unregisterAll();
});

app.on('window-all-closed', function () {
    if (process.platform !== 'darwin') {
        app.quit();
    }
});
app.on('activate', function () {
    if (mainWindow === null) {
        createWindow();
    }
});












