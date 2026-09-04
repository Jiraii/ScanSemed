import sys
import re

with open('electron-app/main.js', 'r', encoding='utf8') as f:
    content = f.read()

target = "const codeMatch = response.data.match(/<code>(.*?)<\/code>/i);"
replacement = "const codeMatch = response.data.match(/(?:<|&lt;)code(?:>|&gt;)(.*?)(?:<|&lt;)\/code(?:>|&gt;)/i);"

target2 = "const msgMatch = response.data.match(/<message>(.*?)<\/message>/i);"
replacement2 = "const msgMatch = response.data.match(/(?:<|&lt;)message(?:>|&gt;)(.*?)(?:<|&lt;)\/message(?:>|&gt;)/i);"

content = content.replace(target, replacement)
content = content.replace(target2, replacement2)

with open('electron-app/main.js', 'w', encoding='utf8') as f:
    f.write(content)
print("Fixed regex in main.js")
