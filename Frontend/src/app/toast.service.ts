import { Injectable } from '@angular/core';
import { ToastrService as NgxToastrService } from 'ngx-toastr';
import { ModalService } from './blocking-modal/modal.service';

@Injectable({
  providedIn: 'root'
})
export class ToastService {
  constructor(private toastr: NgxToastrService, private modal: ModalService) {}

  showError(message: string) {
    // show blocking modal so user must acknowledge
    try {
      this.modal.showError(message);
    } catch (e) {
      try { this.toastr.error(message, 'Error'); } catch { try { alert('Error: ' + message); } catch {} }
    }
    console.error('Toast error:', message);
  }

  showSuccess(message: string) {
    try {
      this.modal.showSuccess(message);
    } catch (e) {
      try { this.toastr.success(message, 'Éxito'); } catch { try { alert('Éxito: ' + message); } catch {} }
    }
    console.log('Toast success:', message);
  }
}
