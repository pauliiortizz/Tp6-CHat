import { Component, OnInit } from '@angular/core';
import { Product } from '../product.model';
import { EmployeeService } from '../product.service';
import { Router, ActivatedRoute, Params } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ToastService } from '../toast.service';

@Component({
  selector: 'app-addemployee',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './addproduct.component.html',
  styleUrls: ['./addproduct.component.css']
})
export class AddemployeeComponent implements OnInit {
  newEmployee: Product = new Product(0, '', '', 0);
  submitBtnText: string = "Crear";
  imgLoadingDisplay: string = 'none';

  constructor(private employeeService: EmployeeService,
    private router: Router,
    private activatedRoute: ActivatedRoute,
    private toast: ToastService) {
  }

  ngOnInit(): void {
    this.activatedRoute.queryParams.subscribe(params => {
      const employeeId = params['id'];
      if(employeeId)
      this.editEmployee(employeeId);
    });
  }

  addEmployee(employee: Product) {
    const validation = this.validateAndFormat(employee);
    if (!validation.valid) {
      // show Spanish-friendly error
      const err = validation.error ? validation.error : 'Error en los datos';
      this.toast.showError(err);
      return;
    }

    // Duplicate name check (case-insensitive). For edit, ignore the same id.
    this.employeeService.getAllEmployee().subscribe(list => {
      const nameLower = (employee.name || '').toLowerCase();
      const duplicate = list.find(p => p.name?.toLowerCase() === nameLower && p.id !== employee.id);
      if (duplicate) {
        this.imgLoadingDisplay = 'none';
        this.toast.showError('No se puede usar un nombre duplicado');
        return;
      }

      // proceed with create/update after duplicate check
      this._performSave(employee);
    }, err => { this.toast.showError('Error al validar duplicados'); });
  }

  private _performSave(employee: Product) {
    // show spinner / disable button feedback
    const originalBtnText = this.submitBtnText;
    this.imgLoadingDisplay = 'inline';
    this.submitBtnText = employee.id === 0 ? 'Creating...' : 'Saving...';

    if (employee.id == 0) {
      employee.createdDate = new Date().toISOString();
      this.employeeService.createEmployee(employee).subscribe({
        next: ()=> { this.imgLoadingDisplay = 'none'; this.submitBtnText = originalBtnText; this.toast.showSuccess('Producto creado correctamente'); this.router.navigate(['/']); },
        error: e => { this.imgLoadingDisplay = 'none'; this.submitBtnText = originalBtnText; this.toast.showError('Error en la API'); }
      });
    }
    else {
      employee.createdDate = new Date().toISOString();
      this.employeeService.updateEmployee(employee).subscribe({
        next: ()=> { this.imgLoadingDisplay = 'none'; this.submitBtnText = originalBtnText; this.toast.showSuccess('Producto editado correctamente'); this.router.navigate(['/']); },
        error: e => { this.imgLoadingDisplay = 'none'; this.submitBtnText = originalBtnText; this.toast.showError('Error en la API'); }
      });
    }
  }
  private validateAndFormat(employee: Product): { valid: boolean; error?: string } {
    if (!employee || !employee.name || employee.name.trim().length === 0) return { valid: false, error: 'El nombre es obligatorio' };

    // Stock validations (align with backend 0..100 rule)
    if (employee.stock == null || Number.isNaN(employee.stock as any)) {
      return { valid: false, error: 'El stock es obligatorio' };
    }
    if (employee.stock < 0 || employee.stock > 100) {
      return { valid: false, error: 'El stock debe estar entre 0 y 100' };
    }

    let name = employee.name.replace(/\u00A0/g, ' ').trim().replace(/\s+/g, ' ');
  if (name.length < 2) return { valid: false, error: 'El nombre debe tener al menos 2 caracteres' };

  const forbidden = ['Empleado','N/A','Nombre','Anonimo','Test'];
  if (forbidden.includes(name) || forbidden.some(f => name.split(' ').includes(f))) return { valid: false, error: 'Nombre no permitido' };

    const parts = name.split(' ');
    for (const part of parts) {
      if (part.length < 1) return { valid: false, error: 'Cada parte debe tener al menos un caracter' };
      if (part.length > 100) return { valid: false, error: 'Cada parte admite hasta 100 caracteres' };
      if (/\d/.test(part)) return { valid: false, error: 'El nombre no debe contener números' };
      if (/([a-zA-Z'])\1{2,}/i.test(part)) return { valid: false, error: 'Caracteres repetidos en exceso' };
      if (!/^[\u00C0-\u017Fa-zA-Z'\-]+$/.test(part)) return { valid: false, error: 'El nombre contiene caracteres inválidos' };
    }

    // format: capitalize all given names, surname uppercase
    if (parts.length === 1) {
      employee.name = parts[0].charAt(0).toUpperCase() + parts[0].slice(1).toLowerCase();
    } else {
  const given = parts.slice(0, parts.length - 1).map((p: string) => p.charAt(0).toUpperCase() + p.slice(1).toLowerCase());
      const surname = parts[parts.length - 1].toUpperCase();
      employee.name = [...given, surname].join(' ');
    }

    return { valid: true };
  }

  editEmployee(employeeId: number) {
    this.employeeService.getEmployeeById(employeeId).subscribe(res => {
      this.newEmployee.id = res.id;
      this.newEmployee.name = res.name
      this.newEmployee.stock = (res as any).stock ?? 0;
      this.submitBtnText = "Editar";
    });
  }

  cancel() {
    this.router.navigate(['/']);
  }

}
