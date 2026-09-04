import sys

with open('web-frontend-source/src/app/app.component.css', 'r', encoding='utf8') as f:
    content = f.read()

index = content.find('/* Toast Notifications */')
if index != -1:
    content = content[:index] # Remove old toast css

new_css = '''/* ==========================================
   TOAST NOTIFICATION (Modern Clean)
   ========================================== */
.modern-toast {
  position: fixed;
  bottom: 30px;
  right: 30px;
  display: flex;
  align-items: center;
  padding: 16px 24px;
  border-radius: 12px;
  color: #1e293b;
  background: #ffffff;
  font-size: 1.25rem;
  font-weight: 600;
  z-index: 10000;
  box-shadow: 0 10px 25px -5px rgba(0,0,0,0.1), 0 8px 10px -6px rgba(0,0,0,0.1);
  animation: slideUpFadeIn 0.4s cubic-bezier(0.16, 1, 0.3, 1);
  border-left: 6px solid #cbd5e1;
}

.modern-toast .toast-icon {
  display: flex;
  justify-content: center;
  align-items: center;
  width: 28px;
  height: 28px;
  border-radius: 50%;
  color: white;
  margin-right: 12px;
  font-size: 1rem;
}

/* ชุดสี Toast */
.modern-toast.success { border-left-color: #10b981; }
.modern-toast.success .toast-icon { background: #10b981; }

.modern-toast.error { border-left-color: #ef4444; }
.modern-toast.error .toast-icon { background: #ef4444; }

.modern-toast.warning { border-left-color: #f59e0b; }
.modern-toast.warning .toast-icon { background: #f59e0b; }

.modern-toast.info { border-left-color: #3b82f6; }
.modern-toast.info .toast-icon { background: #3b82f6; }

@keyframes slideUpFadeIn {
  from { transform: translateY(100%); opacity: 0; }
  to { transform: translateY(0); opacity: 1; }
}

/* ==========================================
   MODAL DIALOG (Hospital Clinical Style)
   ========================================== */
.modern-modal-overlay {
  position: fixed;
  top: 0; left: 0; right: 0; bottom: 0;
  background: rgba(15, 23, 42, 0.5);
  backdrop-filter: blur(4px); /* สไตล์กระจกฝ้า Glassmorphism */
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 9999;
  animation: fadeIn 0.2s ease-out;
}

.modern-modal-card {
  background: #ffffff;
  width: 90%;
  max-width: 500px; /* ยืดหยุ่นบนจอสัมผัส */
  border-radius: 16px;
  box-shadow: 0 20px 25px -5px rgba(0,0,0,0.1);
  overflow: hidden;
  animation: popIn 0.3s cubic-bezier(0.16, 1, 0.3, 1);
}

.modal-header {
  padding: 24px 24px 16px 24px;
  display: flex;
  flex-direction: column;
  align-items: center;
  text-align: center;
}

.modal-header .icon-circle {
  width: 64px;
  height: 64px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 2rem;
  color: white;
  margin-bottom: 16px;
}

.modal-header h2 {
  margin: 0;
  font-size: 1.5rem;
  color: #1e293b;
}

.modal-content {
  padding: 0 24px 24px 24px;
  text-align: center;
  color: #475569;
  font-size: 1.1rem;
}

.error-log-box {
  background: #f8fafc;
  color: #dc2626;
  padding: 12px;
  border-radius: 8px;
  border: 1px dashed #fca5a5;
  margin-top: 12px;
  font-family: monospace;
  text-align: left;
  word-break: break-all;
}

.modal-footer {
  padding: 16px 24px;
  background: #f8fafc;
  display: flex;
  justify-content: center;
  gap: 12px;
}

.btn-confirm {
  padding: 12px 32px;
  font-size: 1.25rem;
  font-weight: 600;
  color: white;
  border: none;
  border-radius: 8px;
  cursor: pointer;
  transition: transform 0.1s, filter 0.2s;
  width: 100%;
}
.btn-confirm:active { transform: scale(0.98); }

.btn-secondary {
  padding: 12px 32px;
  font-size: 1.25rem;
  font-weight: 600;
  color: #475569;
  background: #f1f5f9;
  border: none;
  border-radius: 8px;
  cursor: pointer;
  transition: transform 0.1s, filter 0.2s;
  width: 100%;
}
.btn-secondary:active { transform: scale(0.98); }

/* Theme Colors สำหรับ Modal */
.error-theme .icon-circle { background: #ef4444; }
.error-theme .btn-confirm { background: #ef4444; }
.error-theme .btn-confirm:hover { background: #dc2626; }

.warning-theme .icon-circle { background: #f59e0b; }
.warning-theme .btn-confirm { background: #f59e0b; color: #fff; }
.warning-theme .btn-confirm:hover { background: #d97706; }

@keyframes fadeIn { from { opacity: 0; } to { opacity: 1; } }
@keyframes popIn { 
  0% { transform: scale(0.9); opacity: 0; } 
  100% { transform: scale(1); opacity: 1; } 
}
'''

content = content + '\\n' + new_css
with open('web-frontend-source/src/app/app.component.css', 'w', encoding='utf8') as f:
    f.write(content)
print('CSS updated successfully')
