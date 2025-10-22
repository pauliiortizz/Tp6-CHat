import { Component, OnInit } from '@angular/core';
import { EmployeeService } from '../product.service';
import { Product } from '../product.model';
import { Observable } from 'rxjs';
import { Router } from '@angular/router';
import { map } from 'rxjs/operators';
import { of } from 'rxjs';
import { CommonModule } from '@angular/common';
import { ToastService } from '../toast.service';

@Component({
  selector: 'app-employee',
  standalone: true,
  imports:[CommonModule],
  templateUrl: './product.component.html',
  styleUrls: ['./product.component.css'],
})
export class EmployeeComponent implements OnInit {
  employees: Observable<Product[]> = new Observable<Product[]>();
  imgLoadingDisplay: string = 'none';
  visibleIds: Set<number> = new Set<number>();

  constructor(
    private employeeService: EmployeeService,
    private router: Router,
    private toast: ToastService
  ) {}

  ngOnInit() {
    this.getEmployess();
  }

  getEmployess() {
    this.visibleIds.clear();
    this.employees = this.employeeService.getAllEmployee();
    // mark rows visible with slight stagger after data arrives
    this.employees.subscribe(list => {
      let delay = 0;
      for (const p of list) {
        setTimeout(() => this.visibleIds.add(p.id), delay);
        delay += 70;
      }
    });
    return this.employees;
  }

  addEmployee() {
    this.router.navigate(['/addemployee']);
  }

  deleteEmployee(id: number) {
    this.imgLoadingDisplay = 'inline';
    this.employeeService.deleteEmployeeById(id).subscribe({
      next: () => {
        this.getEmployess().subscribe(() => {
          this.imgLoadingDisplay = 'none';
          this.toast.showSuccess('Producto eliminado correctamente');
        });
      },
      error: () => {
        this.imgLoadingDisplay = 'none';
        this.toast.showError('Error al eliminar el producto');
      }
    });
  }

  editEmployee(id: number) {
    this.router.navigate(['/addemployee'], { queryParams: { id: id } });
  }

  searchItem(value: string) {
    this.employeeService.getAllEmployee().subscribe((res) => {
      this.employees = of(res);

      this.employees
        .pipe(
          map((plans) => plans.filter((results) => results.name.indexOf(value) != -1))
        )
        .subscribe((results) => {
          const productList = results.map((r) => new Product(r.id, r.name, r.createdDate, (r as any).stock ?? 0));
          this.employees = of(productList);
        });
    });
  }
}
