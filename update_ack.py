import sys
import re

with open('electron-app/main.js', 'r', encoding='utf8') as f:
    content = f.read()

target = '''        const response = await axios.post(SEMED_SOAP_URL, soapEnvelope, {
            headers: {
                'Content-Type': 'text/xml;charset=UTF-8',
                'SOAPAction': 'urn:outpOrderDispense',
                'Connection': 'close'
            },
            timeout: 30000
        });

        res.json({ success: true, result: response.data });'''

new_code = '''        const response = await axios.post(SEMED_SOAP_URL, soapEnvelope, {
            headers: {
                'Content-Type': 'text/xml;charset=UTF-8',
                'SOAPAction': 'urn:outpOrderDispense',
                'Connection': 'close'
            },
            timeout: 30000
        });

        // ==========================================
        // 🚨 AUDIT FIX: Validate SeMed ACK/NACK (Code 0)
        // ==========================================
        let isSuccess = false;
        let semedErrorMsg = "Unknown Hardware Error";
        const codeMatch = response.data.match(/<code>(.*?)<\/code>/i);
        if (codeMatch && codeMatch[1] === '0') {
            isSuccess = true;
        } else {
            const msgMatch = response.data.match(/<message>(.*?)<\/message>/i);
            if (msgMatch) semedErrorMsg = msgMatch[1];
        }

        if (!isSuccess) {
            throw new Error(SeMed Machine Rejected: );
        }

        res.json({ success: true, result: response.data });'''

content = content.replace(target, new_code)

with open('electron-app/main.js', 'w', encoding='utf8') as f:
    f.write(content)
print("Updated ACK parsing")
