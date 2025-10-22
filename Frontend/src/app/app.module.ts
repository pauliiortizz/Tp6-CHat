import { BrowserModule } from '@angular/platform-browser';
import { NgModule, LOCALE_ID } from '@angular/core'; // Importar LOCALE_ID
import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { EmployeeComponent } from './product/product.component';
import { HttpClientModule } from '@angular/common/http';
import { DatePipe } from '@angular/common'; // Importar DatePipe y registerLocaleData
import { FormsModule } from "@angular/forms";
import { AddemployeeComponent } from './addproduct/addproduct.component';




@NgModule({
  declarations: [
    AppComponent,
    EmployeeComponent,
    AddemployeeComponent
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    HttpClientModule,
    FormsModule
  ],
  providers: [
    DatePipe
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
