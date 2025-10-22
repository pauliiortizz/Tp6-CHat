import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ModalService } from './modal.service';

@Component({
  selector: 'app-blocking-modal',
  standalone: true,
  imports: [CommonModule],
  template: `
  <div class="bc-overlay" *ngIf="data.visible">
    <div class="bc-dialog" role="dialog" aria-modal="true">
      <div class="bc-header">
        <h3>{{data.title}}</h3>
      </div>
      <div class="bc-body">{{data.message}}</div>
      <div class="bc-footer">
        <button class="btn btn-primary" (click)="close()">Cerrar</button>
      </div>
    </div>
  </div>
  `,
  styles: [
    `
    .bc-overlay { position: fixed; inset: 0; background: rgba(0,0,0,0.45); display:flex; align-items:center; justify-content:center; z-index:99999 }
    .bc-dialog { background: #fff; border-radius:8px; padding:20px; max-width:520px; width:90%; box-shadow:0 10px 30px rgba(0,0,0,0.25) }
    .bc-header h3 { margin:0 0 8px 0 }
    .bc-body { margin-bottom:16px }
    .bc-footer { text-align:right }
    `
  ]
})
export class BlockingModalComponent {
  data = { title: '', message: '', visible: false, type: 'info' } as any;
  constructor(private modal: ModalService) {
    this.modal.modal$.subscribe(v => this.data = v);
  }

  close() {
    this.modal.hide();
  }
}
