import sys

css_append = '''

/* ==========================================
   TOUCHSCREEN 15.6" FULL HD SCALING & LAYOUT
   ========================================== */
.drugs-table td {
    padding: 18px 12px !important;
    font-size: 22px !important;
    font-weight: 700 !important;
    height: 60px !important; /* Row height >= 60px */
}
.drugs-table th {
    padding: 16px 12px !important;
    font-size: 20px !important;
}
.drugs-card {
    max-height: 45vh !important; /* Limit height so it doesn't push bottom section away */
}
.rfid-number {
    font-size: 64px !important;
    font-weight: 900 !important;
    margin: 10px 0 !important;
}
.patient-name {
    font-size: 36px !important;
    font-weight: 800 !important;
    margin-bottom: 12px !important;
}
.badge-vn, .badge-hn {
    font-size: 20px !important;
    padding: 8px 16px !important;
}
.qty-badge {
    font-size: 24px !important;
    padding: 6px 16px !important;
}
.dashboard-container {
    height: 100vh !important;
    display: flex !important;
    flex-direction: column !important;
    overflow: hidden !important;
}
.main-content {
    flex: 0 0 auto !important; /* Don't grow, take needed space */
}
.bottom-section {
    flex: 1 1 auto !important; /* Take remaining space */
    min-height: 0 !important;
    display: grid !important;
    grid-template-columns: 1fr 1fr !important;
    gap: 20px !important;
    padding: 10px 20px 20px 20px !important;
}
.queue-area, .history-area {
    display: flex !important;
    flex-direction: column !important;
    height: 100% !important;
    padding: 15px !important;
}
.queue-list, .history-list {
    flex: 1 1 auto !important;
    overflow-y: auto !important;
    min-height: 0 !important;
}
.empty-queue, .empty-history {
    font-size: 20px !important;
    padding: 40px !important;
}
.queue-header h3, .history-header h3 {
    font-size: 22px !important;
    margin-bottom: 15px !important;
}
'''

with open('web-frontend-source/src/app/app.component.css', 'a', encoding='utf8') as f:
    f.write(css_append)
print("Updated app.component.css")
