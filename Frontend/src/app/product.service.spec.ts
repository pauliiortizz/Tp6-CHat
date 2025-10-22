import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { EmployeeService } from './product.service';
import { Employee, Product } from './product.model';
import { DatePipe } from '@angular/common';

describe('EmployeeService', () => {
  let service: EmployeeService;
  let httpMock: HttpTestingController;
  let datePipe: DatePipe;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [
        EmployeeService,
        DatePipe
      ]
    });

    service = TestBed.inject(EmployeeService);
    httpMock = TestBed.inject(HttpTestingController);
    datePipe = TestBed.inject(DatePipe);
  });

  afterEach(() => {
    httpMock.verify();
  });



  it('should retrieve all employees', () => {
  const today = new Date();
  const expectedDateTime = datePipe.transform(today, 'dd/MM/yyyy HH:mm:ss', undefined) ?? '';

    const dummyEmployees: Employee[] = [
      new Employee(1, 'John Doe', expectedDateTime),
      new Employee(2, 'Jane Smith', expectedDateTime)
    ];

    service.getAllEmployee().subscribe(employees => {
      expect(employees.length).toBe(2);
      employees.forEach((employee, index) => {
        console.log('Employee createdDate:', employee.createdDate ?? '');
        console.log('Dummy employee createdDate:', dummyEmployees[index].createdDate ?? '');

        expect(employee.createdDate).toEqual(expectedDateTime);
      });
    });

  const req = httpMock.expectOne(`${service.apiUrlEmployee}`);
    expect(req.request.method).toBe('GET');
    req.flush(dummyEmployees);
  });


  it('should call GET by id with correct URL', () => {
    service.getEmployeeById(42).subscribe();
    const req = httpMock.expectOne(`${service.apiUrlEmployee}/42`);
    expect(req.request.method).toBe('GET');
  });

  it('should POST to create with correct URL and body', () => {
    const p = new Product(0, 'John DOE', '', 10);
    service.createEmployee(p).subscribe();
    const req = httpMock.expectOne(`${service.apiUrlEmployee}`);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual(p);
  });

  it('should PUT to update with correct URL and body', () => {
    const p = new Product(5, 'John DOE', '', 20);
    service.updateEmployee(p).subscribe();
    const req = httpMock.expectOne(`${service.apiUrlEmployee}`);
    expect(req.request.method).toBe('PUT');
    expect(req.request.body).toEqual(p);
  });

  it('should DELETE with correct URL', () => {
    service.deleteEmployeeById(99).subscribe();
    const req = httpMock.expectOne(`${service.apiUrlEmployee}/99`);
    expect(req.request.method).toBe('DELETE');
  });

});