import sys

with open('web-frontend-source/src/app/app.component.ts', 'r', encoding='utf8') as f:
    content = f.read()

target = "this.errorMessage = e.message || 'Network Error / Endpoint not reachable';"
replacement = "this.errorMessage = (e.error && e.error.error) ? e.error.error : (e.message || 'Network Error / Endpoint not reachable');"

content = content.replace(target, replacement)

with open('web-frontend-source/src/app/app.component.ts', 'w', encoding='utf8') as f:
    f.write(content)
print("Updated frontend error parsing")
