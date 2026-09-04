import sys
import re

with open('web-frontend-source/src/app/app.component.ts', 'r', encoding='utf8') as f:
    content = f.read()

target = "windowNo: this.dispenseHole,"
replacement = "windowNo: (this.dispenseHole === 'L' || this.dispenseHole === '1') ? '1' : (this.dispenseHole === 'R' || this.dispenseHole === '2' ? '2' : '0'),"

content = content.replace(target, replacement)

with open('web-frontend-source/src/app/app.component.ts', 'w', encoding='utf8') as f:
    f.write(content)
print("Updated windowNo mapping")
