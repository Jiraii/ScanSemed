const fs = require('fs');
const file = 'web-frontend-source/src/app/app.component.html';
let content = fs.readFileSync(file, 'utf8');

const targetStr = '<!-- Stock Warning Modal -->';
const index = content.indexOf(targetStr);
if (index === -1) {
    console.log('Target string not found');
    process.exit(1);
}

const newHtml = 
  <!-- ========================================== -->
  <!-- 🛑 MODERN BLOCKING MODALS                 -->
  <!-- ========================================== -->

  <!-- 1. Stock Warning Modal -->
  <div class="modern-modal-overlay" *ngIf="showStockWarning">
    <div class="modern-modal-card warning-theme">
      <div class="modal-header">
        <div class="icon-circle">!</div>
        <h2>ยาไม่เพียงพอจ่าย</h2>
      </div>
      <div class="modal-content">
        <p class="modal-desc">มียาไม่พอสำหรับตะกร้า <strong>{{ basketNo }}</strong> กรุณาตรวจสอบ:</p>
        <div class="missing-drugs-table-container">
          <table class="missing-drugs-table">
            <thead>
              <tr><th>ยา</th><th>จำนวนสั่ง</th><th>ในตู้มี</th><th>ขาด</th></tr>
            </thead>
            <tbody>
              <tr *ngFor="let m of missingDrugs">
                <td class="drug-name-cell">{{ m.name }}</td>
                <td class="text-center">{{ m.required }} {{ m.unit }}</td>
                <td class="text-center text-warning">{{ m.available }} {{ m.unit }}</td>
                <td class="text-center text-danger"><strong>{{ m.missing }}</strong> {{ m.unit }}</td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
      <div class="modal-footer">
        <button class="btn-secondary" (click)="showStockWarning = false; dispenseStatus = 'idle';">ยกเลิก</button>
        <button class="btn-confirm" (click)="onDispense()">ยืนยันทำรายการต่อ</button>
      </div>
    </div>
  </div>

  <!-- 2. No SEMED Drug Warning Modal -->
  <div class="modern-modal-overlay" *ngIf="showNoSemedDrugWarning">
    <div class="modern-modal-card warning-theme">
      <div class="modal-header" style="background-color: #fff7ed; padding-top: 30px;">
        <div class="icon-circle" style="background-color: #ea580c;">!</div>
        <h2 style="color: #ea580c;">ไม่มีรายการยาในตู้ SEMED</h2>
      </div>
      <div class="modal-content" style="padding-top: 20px;">
        <p style="font-size: 1.1rem; color: #475569;">
          ตะกร้าเลขที่ <strong>{{ basketNo }}</strong> ไม่มีคิวสั่งจ่ายยาของตู้ <strong>SEMED</strong>
        </p>
      </div>
      <div class="modal-footer">
        <button class="btn-confirm" style="background-color: #ea580c;" (click)="showNoSemedDrugWarning = false; patientName='-'; basketNo='-'; rfidCode='-';">ตกลง</button>
      </div>
    </div>
  </div>

  <!-- 3. Invalid Mapping Warning Modal -->
  <div class="modern-modal-overlay" *ngIf="showInvalidMapWarning">
    <div class="modern-modal-card error-theme">
      <div class="modal-header">
        <div class="icon-circle">✖</div>
        <h2>จับคู่ตะกร้าไม่ถูกต้อง</h2>
      </div>
      <div class="modal-content" style="padding-top: 20px;">
        <p style="font-size: 1.1rem;">
          ไม่พบข้อมูลรับรองตะกร้าเลขที่ <strong>{{ basketNo }}</strong> <br><br>
          <span style="color: #ef4444; font-weight: bold;">กรุณานำตะกร้าไปผูกคิวก่อนใช้งาน</span>
        </p>
      </div>
      <div class="modal-footer">
        <button class="btn-confirm" (click)="showInvalidMapWarning = false; patientName='-'; basketNo='-'; rfidCode='-';">ตกลง</button>
      </div>
    </div>
  </div>

  <!-- 4. Low Stock Warning Modal -->
  <div class="modern-modal-overlay" *ngIf="showLowStockWarning">
    <div class="modern-modal-card warning-theme">
      <div class="modal-header" style="background-color: #fefce8; padding-top: 30px;">
        <div class="icon-circle" style="background-color: #ca8a04;">!</div>
        <h2 style="color: #ca8a04;">เตือนยาใกล้หมดตู้</h2>
      </div>
      <div class="modal-content">
        <p style="color: #ca8a04; margin-bottom: 16px;">ยาด้านล่างนี้มีจำนวนลดลงถึงเกณฑ์ขั้นต่ำ <strong>กรุณาแจ้งเติมยาหน้าตู้</strong></p>
        <div class="missing-drugs-table-container">
          <table class="missing-drugs-table">
            <thead>
              <tr><th>รายชื่อยา</th><th>คงเหลือในตู้</th></tr>
            </thead>
            <tbody>
              <tr *ngFor="let m of lowStockDrugs">
                <td class="drug-name-cell">{{ m.name }}</td>
                <td class="text-center text-warning"><strong>{{ m.available }}</strong> {{ m.unit }}</td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
      <div class="modal-footer">
        <button class="btn-confirm" style="background-color: #ca8a04;" (click)="onDispense()">ตกลงและดำเนินการจ่ายยาต่อ</button>
      </div>
    </div>
  </div>

  <!-- 5. API/Network Error Modal -->
  <div class="modern-modal-overlay" *ngIf="showErrorModal">
    <div class="modern-modal-card error-theme">
      <div class="modal-header">
        <div class="icon-circle">✖</div>
        <h2>เกิดข้อผิดพลาดของระบบ</h2>
      </div>
      <div class="modal-content">
        <p style="color: #dc2626; margin-bottom: 12px; font-weight: 500;">
          ไม่สามารถส่งคำสั่งจ่ายยาไปยังเครื่อง SEMED ได้ กรุณาตรวจสอบ Error ด้านล่าง:
        </p>
        <div class="error-log-box">{{ errorMessage }}</div>
      </div>
      <div class="modal-footer">
        <button class="btn-confirm" (click)="closeErrorModal()">รับทราบและปิดหน้าต่าง</button>
      </div>
    </div>
  </div>

</div>

<!-- ========================================== -->
<!-- 🍞 TOAST NOTIFICATION (Modern Clean)        -->
<!-- ========================================== -->
<div class="modern-toast" *ngIf="toastMessage" [ngClass]="toastType">
  <div class="toast-icon">
    <span *ngIf="toastType === 'success'">✔</span>
    <span *ngIf="toastType === 'error'">✖</span>
    <span *ngIf="toastType === 'warning'">!</span>
    <span *ngIf="toastType === 'info'">i</span>
  </div>
  <div class="toast-body">{{ toastMessage }}</div>
</div>
;

content = content.substring(0, index) + newHtml;
fs.writeFileSync(file, content, 'utf8');
console.log('HTML updated successfully');
