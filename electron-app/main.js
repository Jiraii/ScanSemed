const { app, BrowserWindow } = require('electron');
const path = require('path');
const fs = require('fs');
const express = require('express');
const cors = require('cors');
const axios = require('axios');
const WebSocket = require('ws');
const { SerialPort } = require('serialport');
const { ReadlineParser } = require('@serialport/parser-readline');

let mainWindow;
const API_PORT = 9001;
const WS_PORT = 9000;
const BAUD_RATE = 115200;

// Configuration fallback
let HOSPITAL_API_BASE_URL = 'http://192.168.34.246/apiopd';
let SEMED_SOAP_URL = 'http://10.35.222.66:8788/axis2/services/DIHPMPFWebservice.DIHPMPFWebserviceHttpSoap11Endpoint/';
let OUTPUT_LR = 'L';

const configPath = path.join(app.getPath('userData'), 'config.json');
function loadConfig() {
    try {
        if (fs.existsSync(configPath)) {
            const data = JSON.parse(fs.readFileSync(configPath, 'utf8'));
            if (data.HOSPITAL_API_BASE_URL) HOSPITAL_API_BASE_URL = data.HOSPITAL_API_BASE_URL;
            if (data.SEMED_SOAP_URL) SEMED_SOAP_URL = data.SEMED_SOAP_URL;
            if (data.OUTPUT_LR) OUTPUT_LR = data.OUTPUT_LR;
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
    const deptName = p.wardname || '';
    const doctCode = p.doctorcode || '';
    const doctName = p.doctorname || '';

    let drugsXml = '';
    let itemNo = 1;
    for (const d of drugs) {
        drugsXml += 
        `<Drug>
          <Code>` + (d.icode || d.code || '') + `</Code>
          <Name>` + (d.name || d.drugname || '') + `</Name>
          <Spec></Spec>
          <FirmName></FirmName>
          <Unit>` + (d.units || '') + `</Unit>
          <Alias></Alias>
          <Method></Method>
          <Type></Type>
          <Qty>` + (d.qty || '') + `</Qty>
          <note></note>
          <ItemNo>` + itemNo + `</ItemNo>
        </Drug>`;
        itemNo++;
    }

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
        const windowNo = OUTPUT_LR === 'R' ? '3,4' : '1,2';
        
        // Generate XML entirely in Node.js, no C# needed!
        const innerXml = generateSemedXml(payload, windowNo);

        const soapEnvelope = `<?xml version="1.0" encoding="utf-8"?>
<soapenv:Envelope xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/" xmlns:web="http://webservice.pmpf.dih.com">
   <soapenv:Header/>
   <soapenv:Body>
      <web:outpOrderDispense>
         <web:xml><![CDATA[` + innerXml + `]]></web:xml>
      </web:outpOrderDispense>
   </soapenv:Body>
</soapenv:Envelope>`;

        const response = await axios.post(SEMED_SOAP_URL, soapEnvelope, {
            headers: {
                'Content-Type': 'text/xml;charset=UTF-8',
                'SOAPAction': 'urn:outpOrderDispense'
            },
            timeout: 30000
        });

        res.json({ success: true, result: response.data });
    } catch (error) {
        console.error('Dispense API Error:', error.message);
        res.status(500).json({ success: false, error: error.message });
    }
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
async function scanAndConnectSerialPorts() {
    try {
        const ports = await SerialPort.list();
        ports.forEach(portInfo => {
            const portPath = portInfo.path;
            try {
                const port = new SerialPort({ path: portPath, baudRate: BAUD_RATE });
                const parser = port.pipe(new ReadlineParser({ delimiter: '$' }));
                parser.on('data', (data) => {
                    let rawData = data.toString().trim();
                    if (rawData.startsWith('#')) {
                        const parts = rawData.substring(1).split('|');
                        if (parts.length >= 3) {
                            broadcastToUI({ type: 'SCAN', payload: parts[2] });
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
        title: "Scan",
        webPreferences: {
            nodeIntegration: false,
            contextIsolation: true
        }
    });
    mainWindow.setMenuBarVisibility(false);
    mainWindow.loadURL(`http://localhost:${API_PORT}`);
    mainWindow.on('closed', function () {
        mainWindow = null;
    });
    scanAndConnectSerialPorts();
}

app.on('ready', createWindow);

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