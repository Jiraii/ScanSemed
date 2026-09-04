import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClientModule, HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, FormsModule, HttpClientModule],
  templateUrl: './app.component.html',
  styleUrl: './app.component.css'
})
export class AppComponent implements OnInit {
  scanInput: string = '';
  patientName: string = '-';
  vn: string = '-';
  hn: string = '-';
  basketNo: string = '-';
  rfidCode: string = '-';
  patientInfo: any = null;
  isProcessing: boolean = false;
  
  drugs: any[] = [];
  queue: any[] = [];
  dispensedHistory: any[] = [];
  channel: string = 'L';
  menuOpen: boolean = false;
  dispenseHole: string = localStorage.getItem('dispenseHole') || '1';

  // Timeline & Status
  loadPatientTime: Date | null = null;
  checkStockTime: Date | null = null;
  sendSemedTime: Date | null = null;
  dispenseStatus: 'idle' | 'loading' | 'ready' | 'dispensing' | 'success' | 'error' = 'idle';

  constructor(private http: HttpClient, private cdr: ChangeDetectorRef) {}

  ngOnInit() {
    this.http.get<any>('/api/settings').subscribe({
      next: (data) => this.channel = data.channel || 'L',
      error: (e) => console.log('Mock or real backend not connected.')
    });

    this.connectWebSocket();
  }

  onHoleChange(event: Event) {
    const selectElement = event.target as HTMLSelectElement;
    this.dispenseHole = selectElement.value;
    localStorage.setItem('dispenseHole', this.dispenseHole);
  }

  connectWebSocket() {
    const ws = new WebSocket('ws://localhost:9000');
    ws.onmessage = (event) => {
      try {
        const data = JSON.parse(event.data);
        if (data.type === 'SCAN' && data.payload) {
          console.log("WebSocket Scan received:", data.payload);
          this.handleScan(data.payload);
        }
      } catch (e) {
        console.error("WS Parse error", e);
      }
    };
    ws.onclose = () => {
      setTimeout(() => this.connectWebSocket(), 3000);
    };
  }

    toastMessage: string | null = null;
  toastType: 'info' | 'warning' | 'success' | 'error' = 'info';
  toastTimeout: any;
  successTimeout: any;


  showToast(msg: string, type: 'info' | 'warning' | 'success' | 'error' = 'info') {
    this.toastMessage = msg;
    this.toastType = type;
    this.cdr.detectChanges();
    if (this.toastTimeout) clearTimeout(this.toastTimeout);
    this.toastTimeout = setTimeout(() => {
      this.toastMessage = null;
      this.cdr.detectChanges();
    }, 4000);
  }

  playSound(type: 'beep' | 'success' | 'error' | 'warning') {
    try {
      const AudioContext = window.AudioContext || (window as any).webkitAudioContext;
      if (!AudioContext) return;
      const ctx = new AudioContext();
      const osc = ctx.createOscillator();
      const gain = ctx.createGain();
      
      osc.connect(gain);
      gain.connect(ctx.destination);
      
      if (type === 'beep') {
        osc.type = 'sine';
        osc.frequency.setValueAtTime(800, ctx.currentTime);
        gain.gain.setValueAtTime(0.1, ctx.currentTime);
        osc.start();
        osc.stop(ctx.currentTime + 0.1);
      } else if (type === 'success') {
        osc.type = 'sine';
        osc.frequency.setValueAtTime(600, ctx.currentTime);
        osc.frequency.setValueAtTime(800, ctx.currentTime + 0.1);
        gain.gain.setValueAtTime(0.1, ctx.currentTime);
        osc.start();
        osc.stop(ctx.currentTime + 0.3);
      } else if (type === 'error' || type === 'warning') {
        osc.type = 'square';
        osc.frequency.setValueAtTime(300, ctx.currentTime);
        gain.gain.setValueAtTime(0.1, ctx.currentTime);
        osc.start();
        gain.gain.setValueAtTime(0, ctx.currentTime + 0.1);
        gain.gain.setValueAtTime(0.1, ctx.currentTime + 0.2);
        gain.gain.setValueAtTime(0, ctx.currentTime + 0.3);
        gain.gain.setValueAtTime(0.1, ctx.currentTime + 0.4);
        osc.stop(ctx.currentTime + 0.5);
      }
    } catch (e) {
      console.error('Audio play failed', e);
    }
  }

  lastScanTime: number = 0;

  
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

  handleScan(basketId: string) {
    const now = Date.now();
    if (now - this.lastScanTime < 1000) return;
    this.lastScanTime = now;

    if (!basketId) return;
    const cleanId = basketId.trim();

    if ((this.basketNo === cleanId || this.rfidCode === cleanId) && this.dispenseStatus !== 'idle') {
      return;
    }

    if (this.queue.includes(cleanId)) {
      this.playSound('warning');
      this.showToast('ตะกร้า ' + cleanId + ' อยู่ในคิวแล้ว', 'warning');
      return;
    }

    const nowMs = new Date().getTime();
    const recentlyDispensed = this.dispensedHistory.some(h => 
      (h.id === cleanId || (h.rfidCode && h.rfidCode === cleanId)) && 
      (nowMs - (h.time ? h.time.getTime() : 0) < 120000)
    );
    
    if (recentlyDispensed) {
      this.playSound('error');
      this.showToast('ตะกร้า ' + cleanId + ' นี้ส่งจ่ายยาสำเร็จไปแล้ว', 'error');
      return;
    }

    if (this.dispenseStatus !== 'idle' || this.isProcessing) {
      this.queue.push(cleanId);
      this.playSound('beep');
      this.showToast('เพิ่มตะกร้า ' + cleanId + ' ลงคิวลำดับที่ ' + this.queue.length, 'info');
      this.cdr.detectChanges();
      return;
    }
    
    this.playSound('beep');
    this.fetchPatientData(cleanId);
  }

  onScan() {
    if (this.scanInput.trim()) {
      this.handleScan(this.scanInput);
      this.scanInput = '';
    }
  }

  fetchPatientData(basketId: string) {
    if (this.isProcessing) return;
    
    // Check history (max 10 recent scans)
    if (this.dispensedHistory.some(h => h.id === basketId || (h.rfidCode && h.rfidCode === basketId))) {
        return;
    }
    
    this.isProcessing = true;
    this.basketNo = basketId;
    this.rfidCode = basketId;
    this.patientName = 'กำลังดึงข้อมูล...';
    this.vn = '-';
    this.hn = '-';
    this.patientInfo = null; // Clear previous basket data
    this.drugs = [];
    this.loadPatientTime = null;
    this.checkStockTime = null;
    this.sendSemedTime = null;
    this.dispenseStatus = 'loading';
    this.cdr.detectChanges();

    this.http.post<any>('/api/proxy/packagemaster', { basketid: basketId }).subscribe({
      next: (res) => {
        if (res && res.data && res.data.length > 0) {
          const firstData = res.data[0];
          const packagemaster = firstData.packagemaster || [];
          const allDrugs = firstData.drugs || [];
          
          if (packagemaster.length > 0) {
            this.patientInfo = packagemaster[0];
            this.patientName = this.patientInfo.patientname || 'ไม่ระบุชื่อ';
            this.vn = this.patientInfo.vn || '-';
            this.hn = this.patientInfo.hn || '-';
            this.basketNo = this.patientInfo.basketno || this.patientInfo.basketname || basketId;
            
            this.loadPatientTime = new Date();

            const semedDrugs = packagemaster.filter((d:any) => d.shelfzone === 'SE-MED');
            
            const uniqueDrugs = new Map();
            semedDrugs.forEach((d:any) => {
              if (!uniqueDrugs.has(d.orderitemcode)) {
                const drugDetails = allDrugs.find((x:any) => x.orderitemcode === d.orderitemcode);
                uniqueDrugs.set(d.orderitemcode, {
                  code: d.orderitemcode,
                  name: d.orderitemname,
                  qty: d.orderqty || d.qty,
                  unit: drugDetails ? drugDetails.orderunitcode : d.orderunitcode,
                  shelfzone: d.shelfzone,
                  Strength: d.Strength || 'N/A'
                });
              }
            });
            this.drugs = Array.from(uniqueDrugs.values());
            
            if (this.drugs.length === 0) {
                this.showNoSemedDrugWarning = true;
                this.dispenseStatus = 'idle';
                this.isProcessing = false;
            } else {
                this.checkStock(this.drugs);
            }
          } else {
            this.patientName = 'ไม่พบข้อมูลตะกร้า';
            this.dispenseStatus = 'idle';
            this.showInvalidMapWarning = true;
            this.isProcessing = false;
          }
        } else {
            this.patientName = 'ไม่พบข้อมูลตะกร้า';
            this.dispenseStatus = 'idle';
            this.showInvalidMapWarning = true;
            this.isProcessing = false;
        }
      },
      error: (e) => {
        console.error(e);
        this.dispenseStatus = 'idle';
        this.isProcessing = false;
      }
    });
  }

  checkStock(drugs: any[]) {
    const drugCodes = drugs.map(d => d.code);
    this.http.post<any>('/api/proxy/semedstock', { drugcode: drugCodes }).subscribe({
      next: (res) => {
        this.missingDrugs = [];
        this.lowStockDrugs = [];
        this.showStockWarning = false;
        this.showLowStockWarning = false;
        
        if (res && res.status === 200 && res.data) {
           const stockData = res.data;
           drugs.forEach(d => {
             const stockItem = stockData.find((s:any) => s.drugCode === d.code || s.Code === d.code);
             const packageRatio = stockItem && stockItem.packageRatio ? parseFloat(stockItem.packageRatio) : 1;
             const available = stockItem ? stockItem.Quantity * packageRatio : 0;
             
             const rawQty = parseFloat(d.qty);
             
             // ยัดโค้ดการคำนวณจำนวนกล่องให้เหมือน BDSender.sln
             if (stockItem && stockItem.packageRatio) {
                 d.qty = Math.floor(rawQty / packageRatio);
             }
             d.originalUnit = d.unit; d.boxQty = d.qty; 
             d.unit = "Box";
             
             const requiredPills = rawQty;
             const minimum = stockItem ? stockItem.Minimum * packageRatio : 0;
             
             if (available < requiredPills) {
               this.missingDrugs.push({
                 code: d.code,
                 name: d.name,
                 required: Math.ceil(requiredPills / packageRatio),
                 available: Math.floor(available / packageRatio),
                 missing: Math.ceil((requiredPills - available) / packageRatio),
                 unit: 'กล่อง'
               });
             } else if (available <= minimum) {
               this.lowStockDrugs.push({
                 code: d.code,
                 name: d.name,
                 available: Math.floor(available / packageRatio),
                 unit: 'กล่อง'
               });
             }
           });
        }
        
        this.checkStockTime = new Date();
        this.dispenseStatus = 'ready';
        
        if (this.missingDrugs.length > 0) {
            this.playSound('warning');
            this.showStockWarning = true;
        } else if (this.lowStockDrugs.length > 0) {
            this.showLowStockWarning = true;
        } else {
            setTimeout(() => {
                this.onDispense();
            }, 500);
        }
        
        this.cdr.detectChanges();
      },
      error: (e) => {
        console.error('Check stock error', e);
        this.missingDrugs = [];
        this.checkStockTime = new Date();
        this.dispenseStatus = 'ready';
        this.cdr.detectChanges();
        drugs.forEach(d => { d.originalUnit = d.unit; d.boxQty = d.qty; });
        setTimeout(() => {
            this.onDispense();
        }, 500);
      }
    });
  }

  onDispense() {
    if (!this.patientInfo || this.drugs.length === 0) {
        alert("ไม่มีข้อมูลยาสำหรับส่งจ่ายตู้ SEMED");
        this.isProcessing = false;
        return;
    }
    
    this.showStockWarning = false;
    this.showLowStockWarning = false;

    console.log("Dispensing...", this.patientInfo, this.drugs);
    this.dispenseStatus = 'dispensing';
    const payload = {
        windowNo: (this.dispenseHole === 'L' || this.dispenseHole === '1') ? '1' : (this.dispenseHole === 'R' || this.dispenseHole === '2' ? '2' : '0'),
        patientInfo: this.patientInfo,
        drugsList: this.drugs
    };
    
    this.http.post<any>('/api/proxy/dispense', payload).subscribe({
      next: (res) => {
        if (res.success) {
          this.playSound('success');
          this.showToast('ส่งคำสั่งจ่ายยาตะกร้า ' + this.basketNo + ' สำเร็จ', 'success');
          this.dispenseStatus = 'success';
          this.sendSemedTime = new Date();
          
          // โชว์สถานะสำเร็จค้างไว้ 3 วินาที
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
          }, 3000);
        } else {
          this.dispenseStatus = 'error';
          this.playSound('error');
          this.errorMessage = res.error || 'Unknown API Error';
          this.isProcessing = false; // Fix: Unlock system on API error
          this.showErrorModal = true;
          this.cdr.detectChanges();
        }
      },
      error: (e) => {
        this.dispenseStatus = 'error';
        this.playSound('error');
          this.errorMessage = (e.error && e.error.error) ? e.error.error : (e.message || 'Network Error / Endpoint not reachable');
        this.isProcessing = false; // Fix: Unlock system on Network error
        this.showErrorModal = true;
        this.cdr.detectChanges();
      }
    });
  }

  closeErrorModal() {
    this.showErrorModal = false;
    this.dispenseStatus = 'idle';
    this.isProcessing = false;
    
    if (this.queue.length > 0) {
      const nextBasket = this.queue.shift();
      this.cdr.detectChanges();
      this.handleScan(nextBasket);
    } else {
      this.cdr.detectChanges();
    }
  }
  
  toggleMenu(event: Event) {
    event.stopPropagation();
    this.menuOpen = !this.menuOpen;
  }
  
  setChannel(ch: string) {
    this.channel = ch;
    this.http.post('/api/settings', { channel: ch }).subscribe({
      next: () => {
        this.menuOpen = false;
        alert('บันทึกการตั้งค่าช่องจ่ายยาเรียบร้อยแล้ว');
      },
      error: () => alert('Could not save setting.')
    });
  }

  // Stock Warning State
  showStockWarning: boolean = false;
  showNoSemedDrugWarning: boolean = false;
  showInvalidMapWarning: boolean = false;
  showLowStockWarning: boolean = false;
  showErrorModal: boolean = false;
  errorMessage: string = '';
  missingDrugs: any[] = [];
  lowStockDrugs: any[] = [];
}
