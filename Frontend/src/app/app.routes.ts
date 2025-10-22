import { Routes } from '@angular/router';
import { AddemployeeComponent } from './addproduct/addproduct.component';
import { EmployeeComponent } from './product/product.component';

export const routes: Routes = [
  { path: 'addemployee', component: AddemployeeComponent },
  { path: '**', component: EmployeeComponent },
];
