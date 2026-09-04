import sys
import re

with open('electron-app/main.js', 'r', encoding='utf8') as f:
    content = f.read()

target = "<ItemNo> + (index + 1) + </ItemNo>" # wait, I used template literals
content = re.sub(r'<ItemNo>\$\{index \+ 1\}<\/ItemNo>', '<ItemNo></ItemNo>', content)

with open('electron-app/main.js', 'w', encoding='utf8') as f:
    f.write(content)
print("Updated ItemNo mapping")
