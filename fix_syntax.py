import sys

with open('electron-app/main.js', 'r', encoding='utf8') as f:
    content = f.read()

content = content.replace("throw new Error(SeMed Machine Rejected: );", "throw new Error(`SeMed Machine Rejected: ${semedErrorMsg}`);")

with open('electron-app/main.js', 'w', encoding='utf8') as f:
    f.write(content)
print("Fixed syntax error")
