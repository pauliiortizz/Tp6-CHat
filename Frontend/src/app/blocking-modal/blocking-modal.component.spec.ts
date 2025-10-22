import { TestBed } from '@angular/core/testing';
import { BlockingModalComponent } from './blocking-modal.component';
import { ModalService } from './modal.service';

describe('BlockingModalComponent', () => {
  it('should show and hide via ModalService', () => {
    TestBed.configureTestingModule({ imports: [BlockingModalComponent] });
    const fixture = TestBed.createComponent(BlockingModalComponent);
    const comp = fixture.componentInstance;
    const modal = TestBed.inject(ModalService);

    modal.show('Title', 'Message', 'info');
    fixture.detectChanges();
    expect((comp as any).data.visible).toBeTrue();

    modal.hide();
    fixture.detectChanges();
    expect((comp as any).data.visible).toBeFalse();
  });
});
