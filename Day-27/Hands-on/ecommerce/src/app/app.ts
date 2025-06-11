import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http'; // Needed for fetching JSON
import { CommonModule } from '@angular/common';
import { CustomerDetails, Customer } from './customer-details/customer-details'; // CustomerDetails component and Customer interface
import { ProductList } from './product-list/product-list';


@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, CustomerDetails, ProductList],
  templateUrl: './app.html',
  styleUrl: './app.css'
})

export class App implements OnInit {
  
  title = 'ecommerce';
  customers: Customer[] = []; // Array to hold customer data
  cartCount: number = 0; 

  constructor(private http: HttpClient) {}

  ngOnInit(): void {
    // Customer data is loaded from the JSON file
    this.http.get<Customer[]>('assets/customers.json').subscribe(data => {
      this.customers = data; // Assigning the loaded array of customers
    });
  }

  onProductAddedToCart(quantity: number): void {
    this.cartCount += quantity;
  }
}

