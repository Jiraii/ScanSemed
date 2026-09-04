import sys

with open('electron-app/main.js', 'r', encoding='utf8') as f:
    content = f.read()

targetStr = "if (rfid.length >= 10) {"
newCode = '''const parts = data.split('|');
                if (parts.length >= 3 && parts[2].trim() !== '0') {
                    let rfid_val = data.trim();
                    rfid_val = rfid_val.replace('\\r', '').replace('\\n', '').trim();
                    if (rfid_val.length === 16) {
                        const rfid = rfid_val;'''

content = content.replace(targetStr, newCode)

with open('electron-app/main.js', 'w', encoding='utf8') as f:
    f.write(content)

print('RFID parser updated successfully')
