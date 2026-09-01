import { Component, OnInit } from '@angular/core';
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
  patientName: string = 'นางประคอง วงษ์มณี';
  vn: string = '0525';
  hn: string = '4334277';
  basketNo: string = '62';
  rfidCode: string = 'E002230C1A748FC8';
  
  drugs: any[] = [
    { code: '1290030^100', name: 'FOLIC ACID 5 MG. *100', qty: '100', unit: 'TAB' },
    { code: '1130100^100', name: 'PROPRANOLOL HCL 10 MG.TAB *100', qty: '400', unit: 'TAB' }
  ];
  
  queue: any[] = [];
  channel: string = 'L';

  constructor(private http: HttpClient) {}

  ngOnInit() {
    this.http.get<any>('/api/settings').subscribe({
      next: (data) => this.channel = data.channel || 'L',
      error: (e) => console.log('Mock or real backend not connected.')
    });
  }

  onScan() {
    console.log("Scanned:", this.scanInput);
    this.scanInput = '';
  }

  onDispense() {
    console.log("Dispensing...");
  }
  
    menuOpen: boolean = false;
  
  toggleMenu(event: Event) {
    event.stopPropagation();
    this.menuOpen = !this.menuOpen;
  }
  
  setChannel(ch: string) {
    this.channel = ch;
    this.http.post('/api/settings', { channel: ch }).subscribe({
      next: () => {
        this.menuOpen = false;
        alert('ตั้งค่าเครื่องสำเร็จ');
      },
      error: () => alert('Could not save setting.')
    });
  }
  
  toggleChannel() {
    this.channel = this.channel === 'L' ? 'R' : 'L';
    this.http.post('/api/settings', { channel: this.channel }).subscribe({
      next: () => console.log('Channel switched to', this.channel),
      error: (e) => alert('Could not save setting.')
    });
  }
}
