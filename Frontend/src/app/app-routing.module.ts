import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { AddemployeeComponent } from './addproduct/addproduct.component';
import { EmployeeComponent } from './product/product.component';

const routes: Routes = [
  { path: 'addemployee', component: AddemployeeComponent },
  { path: '**', component: EmployeeComponent }  
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }