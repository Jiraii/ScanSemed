const fs = require('fs');
const file = 'electron-app/main.js';
let content = fs.readFileSync(file, 'utf8');

const targetStr = "if (rfid.length >= 10) {";
const newCode = const parts = data.split('|');
                if (parts.length >= 3 && parts[2].trim() !== '0') {
                    let rfid = data.trim();
                    rfid = rfid.replace(/[\\r\\n]/g, '').trim();
                    if (rfid.length === 16) {;

content = content.replace(targetStr, newCode);

// There's a closing brace missing because we just replaced the if statement.
// The original was: 
// if (rfid.length >= 10) {
//     if (mainWindow) {
//         mainWindow.webContents.send('scan-event', { type: 'SCAN', payload: rfid });
//     }
// }
// We replaced it, so the brackets are matched perfectly!

fs.writeFileSync(file, content, 'utf8');
console.log('RFID parser updated');
