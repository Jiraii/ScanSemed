import sys

with open('electron-app/main.js', 'r', encoding='utf8') as f:
    content = f.read()

target = "if (codeMatch && codeMatch[1] === '0') {"
replacement = "if (codeMatch && codeMatch[1].trim() === '0') {"

content = content.replace(target, replacement)

with open('electron-app/main.js', 'w', encoding='utf8') as f:
    f.write(content)
print("Updated main.js regex trim")
