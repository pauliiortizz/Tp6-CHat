import { TestBed } from '@angular/core/testing';
import { EmployeeComponent } from './product.component';
import { of, throwError } from 'rxjs';
import { EmployeeService } from '../product.service';
import { Router } from '@angular/router';
import { ToastService } from '../toast.service';

describe('EmployeeComponent', () => {
  let mockService: any;
  let mockRouter: any;
  let mockToast: any;

  beforeEach(() => {
    mockService = {
      getAllEmployee: jasmine.createSpy('getAllEmployee').and.returnValue(of([])),
      deleteEmployeeById: jasmine.createSpy('deleteEmployeeById').and.returnValue(of({}))
    };
    mockRouter = { navigate: jasmine.createSpy('navigate') };
    mockToast = { showSuccess: jasmine.createSpy('showSuccess'), showError: jasmine.createSpy('showError') };

    TestBed.configureTestingModule({
      imports: [EmployeeComponent],
      providers: [
        { provide: EmployeeService, useValue: mockService },
        { provide: Router, useValue: mockRouter },
        { provide: ToastService, useValue: mockToast }
      ]
    });
  });

  it('should create', () => {
    const fixture = TestBed.createComponent(EmployeeComponent);
    const component = fixture.componentInstance;
    expect(component).toBeTruthy();
  });

  it('deleteEmployee success should refresh list and show success modal', () => {
    const fixture = TestBed.createComponent(EmployeeComponent);
    const component = fixture.componentInstance;

    component.deleteEmployee(1);

    expect(mockService.deleteEmployeeById).toHaveBeenCalledWith(1);
    expect(mockService.getAllEmployee).toHaveBeenCalled();
    expect(mockToast.showSuccess).toHaveBeenCalled();
  });

  it('deleteEmployee error should show error modal', () => {
    mockService.deleteEmployeeById.and.returnValue(throwError(() => new Error('api error')));
    const fixture = TestBed.createComponent(EmployeeComponent);
    const component = fixture.componentInstance;

    component.deleteEmployee(2);

    expect(mockService.deleteEmployeeById).toHaveBeenCalledWith(2);
    expect(mockToast.showError).toHaveBeenCalled();
  });
});