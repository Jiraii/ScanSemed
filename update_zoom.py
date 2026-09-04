import sys

with open('electron-app/main.js', 'r', encoding='utf8') as f:
    content = f.read()

targetStr = "mainWindow.loadURL(http://localhost:);"
newCode = targetStr + """
    mainWindow.webContents.on('did-finish-load', () => {
        mainWindow.webContents.setZoomFactor(1.35);
    });
"""

if "setZoomFactor" not in content:
    content = content.replace(targetStr, newCode)

with open('electron-app/main.js', 'w', encoding='utf8') as f:
    f.write(content)
print("Updated main.js")
