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

  handleScan(basketId: string) {
    if (!basketId) return;
    const cleanId = basketId.trim();

    // 1. ถ้ากำลังดำเนินการตะกร้านี้อยู่ ให้ข้ามไป (ป้องกันการสแกนซ้ำตอนที่ตะกร้าวางค้างไว้)
    if ((this.basketNo === cleanId || this.rfidCode === cleanId) && this.dispenseStatus !== 'idle') {
      console.log('Basket is already processing, ignoring scan:', cleanId);
      return;
    }

    // 2. ถ้าตะกร้านี้ไปอยู่ในคิวรอแล้ว ให้ข้ามไป
    if (this.queue.includes(cleanId)) {
      console.log('Basket is already in queue, ignoring scan:', cleanId);
      return;
    }

    // 3. ถ้าระบบไม่ว่าง ให้โยนตะกร้านี้เข้าคิว
    if (this.dispenseStatus !== 'idle') {
      console.log('System is busy, adding to queue:', cleanId);
      this.queue.push(cleanId);
      this.cdr.detectChanges();
      return;
    }

    // 3. ถ้าตะกร้านี้เพิ่งถูกส่งจ่ายยาไปภายใน 2 นาที (120,000 ms) ให้ข้ามไป
    const now = new Date().getTime();
    const recentlyDispensed = this.dispensedHistory.some(h => 
      (h.basketNo === cleanId || h.rfidCode === cleanId) && 
      (now - h.timestamp.getTime() < 120000)
    );
    
    if (recentlyDispensed) {
      console.log('Basket was recently dispensed, ignoring scan:', cleanId);
      return;
    }

    // ถ้าผ่านเงื่อนไขตรวจสอบทั้งหมด ค่อยเรียกใช้งาน
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
    if (this.dispensedHistory.some(h => h.id === basketId)) {
        return;
    }
    
    this.isProcessing = true;
    this.basketNo = basketId;
    this.rfidCode = basketId;
    this.patientName = 'กำลังดึงข้อมูล...';
    this.vn = '-';
    this.hn = '-';
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
                  unit: drugDetails ? drugDetails.orderunitcode : d.orderunitcode
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
             const available = stockItem ? stockItem.Quantity : 0;
             const packageRatio = stockItem && stockItem.packageRatio ? parseFloat(stockItem.packageRatio) : 1;
             
             const rawQty = parseFloat(d.qty);
             
             // ยัดโค้ดการคำนวณจำนวนกล่องให้เหมือน BDSender.sln
             if (stockItem && stockItem.packageRatio) {
                 d.qty = Math.floor(rawQty / packageRatio);
             }
             d.boxQty = d.qty; 
             d.unit = "Box";
             
             const requiredPills = rawQty;
             const minimum = stockItem ? stockItem.Minimum : 0;
             
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
        drugs.forEach(d => { d.boxQty = d.qty; });
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
        windowNo: this.dispenseHole,
        patientInfo: this.patientInfo,
        drugsList: this.drugs
    };
    
    this.http.post<any>('/api/proxy/dispense', payload).subscribe({
      next: (res) => {
        if (res.success) {
          this.dispenseStatus = 'success';
          this.sendSemedTime = new Date();
          
          this.dispensedHistory.unshift({
            id: this.basketNo,
            patientName: this.patientName,
            time: new Date()
          });
          this.dispensedHistory = this.dispensedHistory.slice(0, 10);
          this.isProcessing = false;
          this.cdr.detectChanges();
          
          setTimeout(() => {

              if (this.queue.length > 0) {
                // มีคิวรออยู่ เอาคิวแรกมาทำต่อ
                const nextBasket = this.queue.shift();
                this.dispenseStatus = 'idle';
                this.cdr.detectChanges();
                this.handleScan(nextBasket);
              } else {
                // ไม่มีคิวรอ กลับหน้าว่าง
                this.dispenseStatus = 'idle';
                this.cdr.detectChanges();
              }
          }, 3000);
        } else {
          this.dispenseStatus = 'error';
          this.errorMessage = res.error || 'Unknown API Error';
          this.showErrorModal = true;
          this.cdr.detectChanges();
        }
      },
      error: (e) => {
        this.dispenseStatus = 'error';
        this.errorMessage = e.message || 'Network Error / Endpoint not reachable';
        this.showErrorModal = true;
        this.cdr.detectChanges();
      }
    });
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
