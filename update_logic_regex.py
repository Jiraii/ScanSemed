import sys
import re

with open('web-frontend-source/src/app/app.component.ts', 'r', encoding='utf8') as f:
    content = f.read()

# 1. Add resetForm method and successTimeout variable
if 'successTimeout: any;' not in content:
    index = content.find('toastTimeout: any;')
    if index != -1:
        content = content[:index] + 'toastTimeout: any;\n  successTimeout: any;\n' + content[index + len('toastTimeout: any;'):]

if 'resetForm()' not in content:
    index2 = content.find('handleScan(basketId: string) {')
    reset_logic = '''
  resetForm() {
    this.patientName = '-';
    this.basketNo = '-';
    this.rfidCode = '-';
    this.hn = '-';
    this.vn = '-';
    this.drugs = [];
    this.patientInfo = null;
    this.loadPatientTime = null;
    this.checkStockTime = null;
    this.sendSemedTime = null;
    this.dispenseStatus = 'idle';
    this.isProcessing = false;
    this.cdr.detectChanges();
  }

'''
    content = content[:index2] + reset_logic + content[index2:]

# 2. Modify handleScan
target_handleScan = '''handleScan(basketId: string) {
    const nowMs = new Date().getTime();'''
new_handleScan = '''handleScan(basketId: string) {
    if (this.successTimeout) {
        clearTimeout(this.successTimeout);
        this.successTimeout = null;
        this.resetForm();
    }
    const nowMs = new Date().getTime();'''
content = content.replace(target_handleScan, new_handleScan)

# 3. Modify onDispense success block using Regex to be safe
# We look for:
# this.dispensedHistory.unshift({ id: this.basketNo, rfidCode: this.rfidCode, patientName: this.patientName, time: new Date() });
# ... until ...
# }, 3000);

pattern = re.compile(r'this\.dispensedHistory\.unshift.*?\}, 3000\);', re.DOTALL)
new_dispense = '''// โชว์สถานะสำเร็จค้างไว้ 3 วินาที
          this.cdr.detectChanges();

          if (this.successTimeout) clearTimeout(this.successTimeout);

          this.successTimeout = setTimeout(() => {
              // 1. นำรายการปัจจุบันเก็บลงประวัติ
              this.dispensedHistory.unshift({ id: this.basketNo, rfidCode: this.rfidCode, patientName: this.patientName, time: new Date() });
              this.dispensedHistory = this.dispensedHistory.slice(0, 10);
              
              // 2. เคลียร์ State และ Form ทั้งหมดให้ว่าง
              this.resetForm();

              // 3. ดึงคิวถัดไปมาทำ (ถ้ามี)
              if (this.queue.length > 0) {
                const nextBasket = this.queue.shift();
                this.cdr.detectChanges();
                this.handleScan(nextBasket);
              }
              this.successTimeout = null;
          }, 3000);'''

content = re.sub(pattern, new_dispense, content)

with open('web-frontend-source/src/app/app.component.ts', 'w', encoding='utf8') as f:
    f.write(content)
print("Updated app.component.ts success logic via Regex")
