import sys

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

# 2. Modify handleScan to clear successTimeout
target_handleScan = '''handleScan(basketId: string) {
    const nowMs = new Date().getTime();'''
new_handleScan = '''handleScan(basketId: string) {
    if (this.successTimeout) {
        clearTimeout(this.successTimeout);
        this.successTimeout = null;
        // If interrupted during the 3s success screen, force reset before accepting new
        this.resetForm();
    }
    const nowMs = new Date().getTime();'''
content = content.replace(target_handleScan, new_handleScan)

# 3. Modify onDispense success block
target_dispense = '''          this.dispensedHistory.unshift({ id: this.basketNo, rfidCode: this.rfidCode, patientName: this.patientName, time: new Date() });
          this.dispensedHistory = this.dispensedHistory.slice(0, 10);
          this.isProcessing = false;
          this.cdr.detectChanges();

          setTimeout(() => {

              if (this.queue.length > 0) {
                // դ Ҥáҷӵ
                const nextBasket = this.queue.shift();
                this.dispenseStatus = 'idle';
                this.cdr.detectChanges();
                this.handleScan(nextBasket);
              } else {
                // դ Ѻ˹ҧ
                this.dispenseStatus = 'idle';
                this.cdr.detectChanges();
              }
          }, 3000);'''

new_dispense = '''          // โชว์สถานะสำเร็จค้างไว้ 3 วินาที (ยังไม่เคลียร์ UI)
          this.cdr.detectChanges();

          // เคลียร์ Timer เก่าทิ้ง ป้องกันซ้อนทับ
          if (this.successTimeout) clearTimeout(this.successTimeout);

          this.successTimeout = setTimeout(() => {
              // 1. นำรายการปัจจุบันเก็บลงประวัติ
              this.dispensedHistory.unshift({ id: this.basketNo, rfidCode: this.rfidCode, patientName: this.patientName, time: new Date() });
              this.dispensedHistory = this.dispensedHistory.slice(0, 10);
              
              // 2. เคลียร์ State และ Form ทั้งหมดให้ว่าง
              this.resetForm();

              // 3. ปลดล็อกระบบให้พร้อมรับตะกร้าใหม่
              this.isProcessing = false; 
              
              // 4. ดึงคิวถัดไปมาทำ (ถ้ามี)
              if (this.queue.length > 0) {
                const nextBasket = this.queue.shift();
                this.cdr.detectChanges();
                this.handleScan(nextBasket);
              }
              this.successTimeout = null;
          }, 3000);'''

content = content.replace(target_dispense, new_dispense)

with open('web-frontend-source/src/app/app.component.ts', 'w', encoding='utf8') as f:
    f.write(content)
print("Updated app.component.ts success logic")
