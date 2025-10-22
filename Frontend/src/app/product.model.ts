export class Employee {
  constructor(
    public id: number,
    public name: string,
    public createdDate?: string // Asegúrate de que sea un string para evitar errores con `Date`
  ) {}
}

// Product kept for semantic use in the UI; same shape as Employee
export class Product extends Employee {
  public stock: number = 0;

  constructor(id: number, name: string, createdDate?: string, stock: number = 0) {
    super(id, name, createdDate);
    this.stock = stock;
  }
}
