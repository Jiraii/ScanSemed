import sys

with open('web-frontend-source/src/app/app.component.ts', 'r', encoding='utf8') as f:
    content = f.read()

index = content.find('handleScan(basketId: string) {')
if index != -1:
    content = content[:index] + 'lastScanTime: number = 0;\n\n  ' + content[index:]

    # Now add debounce inside handleScan
    target = 'handleScan(basketId: string) {\n    if (!basketId) return;'
    content = content.replace(target, 'handleScan(basketId: string) {\n    const now = Date.now();\n    if (now - this.lastScanTime < 1000) return;\n    this.lastScanTime = now;\n\n    if (!basketId) return;')

with open('web-frontend-source/src/app/app.component.ts', 'w', encoding='utf8') as f:
    f.write(content)
print('TS updated successfully')
