import sys
with open('web-frontend-source/src/app/app.component.ts', 'r', encoding='utf8') as f:
    content = f.read()

content = content.replace('const now = new Date().getTime();', 'const nowMs = new Date().getTime();')
content = content.replace('(now - (h.time ? h.time.getTime() : 0) < 120000)', '(nowMs - (h.time ? h.time.getTime() : 0) < 120000)')

with open('web-frontend-source/src/app/app.component.ts', 'w', encoding='utf8') as f:
    f.write(content)
