const { SerialPort } = require('serialport');
const { ReadlineParser } = require('@serialport/parser-readline');
const WebSocket = require('ws');
const express = require('express');
const cors = require('cors');
const axios = require('axios');
const http = require('http');
const https = require('https');

// ==========================================
// 1. Configuration
// ==========================================
const WS_PORT = 9000;
const API_PORT = 9001;
const BAUD_RATE = 115200;
const HOSPITAL_API_BASE_URL = 'http://192.168.34.246/apiopd'; // Updated to match C# settings

// ==========================================
// 2. WebSocket Server (For UI Communication)
// ==========================================
const wss = new WebSocket.Server({ port: WS_PORT });
let uiClients = [];

wss.on('connection', (ws) => {
    console.log('✅ Web UI Connected to Local Agent');
    uiClients.push(ws);

    ws.on('close', () => {
        console.log('❌ Web UI Disconnected');
        uiClients = uiClients.filter(client => client !== ws);
    });
});

function broadcastToUI(data) {
    uiClients.forEach(client => {
        if (client.readyState === WebSocket.OPEN) {
            client.send(JSON.stringify(data));
        }
    });
}

// ==========================================
// 3. Serial Port Auto-Discovery & Connection
// ==========================================
let activePorts = [];

async function scanAndConnectSerialPorts() {
    try {
        const ports = await SerialPort.list();
        console.log('🔍 Found COM Ports:', ports.map(p => p.path));

        ports.forEach(portInfo => {
            const path = portInfo.path;
            console.log(`🔌 Attempting to connect to ${path}...`);
            
            try {
                const port = new SerialPort({ path: path, baudRate: BAUD_RATE });
                // Assuming data ends with $ (based on C# code `#|U|ID|...|$`)
                const parser = port.pipe(new ReadlineParser({ delimiter: '$' }));

                port.on('open', () => {
                    console.log(`✅ Successfully connected to ${path}`);
                    activePorts.push(port);
                });

                parser.on('data', (data) => {
                    // Data might look like: #|U|123456789|...
                    let rawData = data.toString().trim();
                    if (rawData.startsWith('#')) {
                        rawData = rawData.substring(1); // Remove #
                        const parts = rawData.split('|');
                        if (parts.length >= 3) {
                            const basketId = parts[2]; // Index 2 is the ID in C# code
                            console.log(`📡 Scanned Basket ID: ${basketId} from ${path}`);
                            broadcastToUI({ type: 'SCAN', payload: basketId });
                        }
                    }
                });

                port.on('error', (err) => {
                    console.log(`⚠️ Error on ${path}:`, err.message);
                });

                port.on('close', () => {
                    console.log(`❌ Disconnected from ${path}`);
                });

            } catch (err) {
                console.log(`❌ Failed to open ${path}:`, err.message);
            }
        });
    } catch (err) {
        console.error('Error listing serial ports:', err);
    }
}

scanAndConnectSerialPorts();

// ==========================================
// 4. Express API Proxy (Bypass CORS) & Static Server
// ==========================================
const app = express();
app.use(cors());
app.use(express.json());

const path = require('path');
const staticPath = path.join(__dirname, '../web-frontend/dist/web-frontend/browser');
app.use(express.static(staticPath));


// Proxy for getting patient/order details
app.post('/api/proxy/packagemaster', async (req, res) => {
    try {
        const { basketid } = req.body;
        console.log(`\n\n=== API Request for Basket: ${basketid} ===`);
        const response = await axios.post(`${HOSPITAL_API_BASE_URL}/packagemaster/order/semed`, 
            { basketid: basketid },
            { 
                headers: { 'User-Agent': 'BDSender', 'Accept': '*/*' },
                timeout: 30000 // Increased to 30 seconds for slow HIS API
            }
        );
        
        // --- OPTIMIZATION: Cut out unused heavy data to speed up frontend ---
        try {
            if (response.data && response.data.data && response.data.data.length > 0) {
                const firstData = response.data.data[0];
                
                // 1. Remove labs completely
                if (firstData.labs) {
                    delete firstData.labs;
                }
                
                // 2. Clean up drugs array
                if (firstData.drugs && Array.isArray(firstData.drugs)) {
                    firstData.drugs = firstData.drugs.map(d => ({
                        orderitemcode: d.orderitemcode,
                        orderitemname: d.orderitemname,
                        orderqty: d.orderqty, Strength: d.Strength, firmname: d.firmname,
                        qty: d.qty,
                        orderunitcode: d.orderunitcode,
                        orderunitdesc: d.orderunitdesc,
                        itemiddosage: d.itemiddosage || d.dosage
                    }));
                }
                
                // 3. Clean up packagemaster
                if (firstData.packagemaster && Array.isArray(firstData.packagemaster)) {
                    firstData.packagemaster = firstData.packagemaster
                        .filter(pm => pm && pm.shelfzone === 'SE-MED')
                        .map(pm => ({
                            hn: pm.hn,
                            patientname: pm.patientname,
                            sex: pm.sex,
                            patientdob: pm.patientdob,
                            prescriptionno: pm.prescriptionno, prescriptionno_sup: pm.prescriptionno_sup,
                            qn: pm.qn,
                            regisdatetime: pm.regisdatetime,
                            vn: pm.vn,
                            wardcode: pm.wardcode,
                            wardname: pm.wardname, doctorcode: pm.doctorcode, doctorname: pm.doctorname,
                            basketno: pm.basketno,
                            _id: pm._id,
                            
                            orderitemcode: pm.orderitemcode,
                            orderitemname: pm.orderitemname,
                            orderqty: pm.orderqty, Strength: pm.Strength, firmname: pm.firmname,
                            qty: pm.qty,
                            orderunitcode: pm.orderunitcode,
                            orderunitdesc: pm.orderunitdesc,
                            shelfzone: pm.shelfzone,
                            seqrun: pm.seqrun,
                            seq: pm.seq,
                            seqmax: pm.seqmax
                        }));
                }
            }
        } catch (trimError) {
            console.error('Trim optimization error, bypassing:', trimError.message);
        }
        
        res.json(response.data);
    } catch (error) {
        console.error('API Error:', error.message);
        res.status(200).json({ status: 500, error: 'HIS API Timeout or Error', data: [] });
    }
});

// Proxy for getting SEMED stock
app.post('/api/proxy/getsemedstock', async (req, res) => {
    try {
        const { drugcode } = req.body; // array of drug codes
        const response = await axios.post(`${HOSPITAL_API_BASE_URL}/dih/getsemedstock`, {
            drugcode: drugcode
        }, { timeout: 10000 });
        res.json(response.data);
    } catch (error) {
        console.error('API Error (getsemedstock):', error.message);
        res.status(500).json({ status: 500, error: 'Failed to fetch stock from hospital API' });
    }
});

// Proxy for sending order to SEMED (SOAP)
app.post('/api/proxy/dispense', async (req, res) => {
    try {
        let { xml, windowNo, patientInfo, drugsList } = req.body;
        
        // If frontend didn't send XML but sent raw objects, construct the XML payload
        if (!xml && patientInfo && drugsList) {
            const pad = (n) => n < 10 ? '0' + n : n;
            const formatDate = (dateString) => {
                let d;
                if (dateString) {
                    d = new Date(dateString);
                    if (isNaN(d.getTime())) d = new Date();
                } else {
                    d = new Date();
                }
                return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())} ${pad(d.getHours())}:${pad(d.getMinutes())}:${pad(d.getSeconds())}`;
            };
            
            const dob = formatDate(patientInfo.patientdob);
            const paymentDT = formatDate(patientInfo.ordercreatedate);
            
            const patientName = (patientInfo.patientname || '').replace(/[\/']/g, '');
            const hn = patientInfo.hn || '';
            const sex = patientInfo.sex || '';
            const qn = patientInfo.qn || '';
            const orderNo = patientInfo.prescriptionno_sup || patientInfo.prescriptionno || '';
            const vn = patientInfo.vn || '';
            const wardcode = patientInfo.wardcode || '';
            const wardname = patientInfo.wardname || '';
            const doctorcode = patientInfo.doctorcode || '';
            const doctorname = patientInfo.doctorname || '';
            const age = patientInfo.age || '';

            const drugsXml = drugsList.map(drug => `
        <Drug>
          <Code><![CDATA[${drug.code || ''}]]></Code>
          <Name><![CDATA[${drug.name || ''}]]></Name>
          <Spec><![CDATA[${drug.Strength || drug.spec || ''}]]></Spec>
          <FirmName><![CDATA[${drug.firmname || drug.firmName || ''}]]></FirmName>
          <Unit><![CDATA[${drug.unit || ''}]]></Unit>
          <Alias></Alias>
          <Method></Method>
          <Type></Type>
          <Qty>${drug.qty || ''}</Qty>
          <note></note>
          <ItemNo></ItemNo>
        </Drug>`).join('');

            xml = `<OutpOrderDispense>
  <Patient>
    <PatID><![CDATA[${hn}]]></PatID>
    <PatName><![CDATA[${patientName}]]></PatName>
    <Gender><![CDATA[${sex}]]></Gender>
    <Birthday>${dob}</Birthday>
    <QN><![CDATA[${qn}]]></QN>
    <AN><![CDATA[${hn}]]></AN>
    <Age><![CDATA[${age}]]></Age>
    <Identity></Identity>
    <InsuranceNo></InsuranceNo>
    <ChargeType></ChargeType>
  </Patient>
  <Prescriptions>
    <Prescription>
      <OrderNo><![CDATA[${orderNo}]]></OrderNo>
      <QN><![CDATA[${qn}]]></QN>
      <AN><![CDATA[${hn}]]></AN>
      <Ordertype></Ordertype>
      <Pharmacy>OPD</Pharmacy>
      <WindowNo>${windowNo || '1'}</WindowNo>
      <PaymentIP></PaymentIP>
      <PaymentDT>${paymentDT}</PaymentDT>
      <OutpNo></OutpNo>
      <VisitNo><![CDATA[${vn}]]></VisitNo>
      <DeptCode><![CDATA[${wardcode}]]></DeptCode>
      <DeptName><![CDATA[${wardname}]]></DeptName>
      <DoctCode><![CDATA[${doctorcode}]]></DoctCode>
      <DoctName><![CDATA[${doctorname}]]></DoctName>
      <Diagnosis></Diagnosis>
      <Drugs>${drugsXml}
      </Drugs>
    </Prescription>
  </Prescriptions>
</OutpOrderDispense>`;
        }
        
        console.log(`\n=== [REAL] Dispense Command to SEMED ===\n`);
        
        const soapUrl = 'http://10.35.222.66:8788/axis2/services/DIHPMPFWebservice.DIHPMPFWebserviceHttpSoap11Endpoint/';
        const soapEnvelope = `<?xml version="1.0" encoding="utf-8"?>
<soapenv:Envelope xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/" xmlns:web="http://webservice.pmpf.dih.com">
   <soapenv:Header/>
   <soapenv:Body>
      <web:outpOrderDispense>
         <web:xml><![CDATA[${xml}]]></web:xml>
      </web:outpOrderDispense>
   </soapenv:Body>
</soapenv:Envelope>`;

        const response = await axios.post(soapUrl, soapEnvelope, {
            headers: {
                'Content-Type': 'text/xml;charset=UTF-8',
                'SOAPAction': 'urn:outpOrderDispense'
            },
            timeout: 30000
        });
        
        res.json({ success: true, data: response.data });
    } catch (error) {
        console.error('SOAP API Error (dispense):', error.message); require('fs').writeFileSync('soap_error.log', (error.stack || error.message) + '\n\n' + JSON.stringify(error.response ? error.response.data : null));
        res.status(500).json({ status: 500, error: 'Failed to dispense via SEMED SOAP API', success: false });
    }
});

// Proxy for updating result to HIS
app.post('/api/proxy/updateresult', async (req, res) => {
    try {
        const payload = req.body;
        const response = await axios.post(`${HOSPITAL_API_BASE_URL}/dih/sendoredrdish`, payload);
        res.json(response.data);
    } catch (error) {
        console.error('API Error (updateresult):', error.message);
        res.status(500).json({ status: 500, error: 'Failed to update result to HIS' });
    }
});

// Proxy for registering basket
app.post('/api/proxy/updatebasket', async (req, res) => {
    try {
        const payload = req.body; // Array of objects
        const response = await axios.post(`${HOSPITAL_API_BASE_URL}/packagemaster/updatepackagemaster/update`, payload);
        res.json(response.data);
    } catch (error) {
        console.error('API Error (updatebasket):', error.message);
        res.status(500).json({ status: 500, error: 'Failed to update basket to HIS' });
    }
});

// SPA Fallback: Serve index.html for any unknown routes (so Angular routing works)
app.use((req, res) => {
    res.sendFile(path.join(staticPath, 'index.html'));
});

app.listen(API_PORT, () => {
    console.log(`🚀 API Proxy Server running at http://localhost:${API_PORT}`);
});





