import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Product } from './product.model';
import { map } from 'rxjs/operators';
import { DatePipe } from '@angular/common';
import { environment } from '../environments/environment'; // Importa el environment


@Injectable({
  providedIn: 'root',
})
export class EmployeeService {
  apiUrlEmployee = environment.apiUrl;  // Usa el valor de environment (ahora apunta a /api/Product)

  constructor(private http: HttpClient, private datepipe: DatePipe) {}

  private parseToDate(value: string | Date | undefined): Date | null {
    if (!value) {
      return null;
    }

    if (value instanceof Date) {
      return value;
    }

    if (typeof value === 'string') {
      const parsed = Date.parse(value);
      if (!Number.isNaN(parsed)) {
        return new Date(parsed);
      }

  // Manually parse dd/MM/yyyy HH:mm:ss since Date.parse assumes mm/dd ordering
  const match = value.match(/^(\d{2})\/(\d{2})\/(\d{4})(?:\s+(\d{2}):(\d{2}):(\d{2}))?$/);
      if (match) {
        const [, day, month, year, hour = '00', minute = '00', second = '00'] = match;
        return new Date(
          Number(year),
          Number(month) - 1,
          Number(day),
          Number(hour),
          Number(minute),
          Number(second)
        );
      }
    }

    return null;
  }

  getAllEmployee(): Observable<Product[]> {
    return this.http
      .get<Product[]>(this.apiUrlEmployee)
      .pipe(
        map((data: Product[]) =>
          data.map(
            (item: Product) =>
              new Product(
                item.id,
                item.name,
                this.formatCreatedDate(item.createdDate),
                item.stock ?? 0
              )
          )
        )
      );
  }

  private formatCreatedDate(value: string | Date | undefined): string | undefined {
    const parsedDate = this.parseToDate(value);
    if (parsedDate) {
      return this.datepipe
        .transform(parsedDate, 'dd/MM/yyyy HH:mm:ss', undefined)
        ?.toString();
    }
    return value?.toString();
  }


  getEmployeeById(employeeId: number): Observable<Product> {
    return this.http.get<Product>(`${this.apiUrlEmployee}/${employeeId}`);
  }
  createEmployee(employee: Product): Observable<Product> {
    const httpOptions = {
      headers: new HttpHeaders({ 'Content-Type': 'application/json' }),
    };
    return this.http.post<Product>(this.apiUrlEmployee, employee, httpOptions);
  }
  updateEmployee(employee: Product): Observable<Product> {
    const httpOptions = {
      headers: new HttpHeaders({ 'Content-Type': 'application/json' }),
    };
    return this.http.put<Product>(this.apiUrlEmployee, employee, httpOptions);
  }

  deleteEmployeeById(employeeid: number) {
    return this.http.delete(`${this.apiUrlEmployee}/${employeeid}`);
  }
}
