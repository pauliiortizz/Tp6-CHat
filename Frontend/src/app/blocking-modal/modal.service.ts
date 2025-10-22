import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

export interface ModalData {
  title: string;
  message: string;
  visible: boolean;
  type?: 'success' | 'error' | 'info';
}

@Injectable({ providedIn: 'root' })
export class ModalService {
  private subj = new BehaviorSubject<ModalData>({ title: '', message: '', visible: false, type: 'info' });
  modal$ = this.subj.asObservable();

  show(title: string, message: string, type: 'success' | 'error' | 'info' = 'info') {
    this.subj.next({ title, message, visible: true, type });
  }

  showSuccess(message: string, title = 'Éxito') {
    this.show(title, message, 'success');
  }

  showError(message: string, title = 'Error') {
    this.show(title, message, 'error');
  }

  hide() {
    const current = this.subj.getValue();
    this.subj.next({ ...current, visible: false });
  }
}
