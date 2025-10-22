import { TestBed } from '@angular/core/testing';
import { ToastService } from './toast.service';
import { ModalService } from './blocking-modal/modal.service';
import { ToastrService, TOAST_CONFIG } from 'ngx-toastr';

describe('ToastService', () => {
  let modalSpy: any;

  beforeEach(() => {
    modalSpy = { showSuccess: jasmine.createSpy('showSuccess'), showError: jasmine.createSpy('showError') };
    TestBed.configureTestingModule({
      providers: [
        ToastService,
        { provide: ModalService, useValue: modalSpy },
  { provide: ToastrService, useValue: {} as any },
  { provide: TOAST_CONFIG, useValue: {} }
      ]
    });
  });

  it('showSuccess should call modal.showSuccess', () => {
    const svc = TestBed.inject(ToastService);
    svc.showSuccess('ok');
    expect(modalSpy.showSuccess).toHaveBeenCalled();
  });

  it('showError should call modal.showError', () => {
    const svc = TestBed.inject(ToastService);
    svc.showError('err');
    expect(modalSpy.showError).toHaveBeenCalled();
  });
});
