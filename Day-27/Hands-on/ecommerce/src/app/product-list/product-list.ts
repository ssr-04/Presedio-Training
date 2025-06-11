import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { Component, EventEmitter, OnInit, Output } from '@angular/core';


// Interface for Product
interface Product {
  id: number;
  name: string;
  price: number;
  imageUrl: string;
  description: string;
  quantity: number; // Managed locally
}

@Component({
  selector: 'app-product-list',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './product-list.html',
  styleUrl: './product-list.css'
})

export class ProductList implements OnInit{

  @Output() addToCartEvent = new EventEmitter<number>(); //Notifies the parent on adding

  products: Product[] = [];

  constructor(private http: HttpClient){}

  ngOnInit(): void {
    //Loading data from json
    this.http.get<Product[]>('assets/products.json').subscribe(data => {
      this.products = data.map(product => ({...product, quantity : 0}));
    });
  }

  incrementQuantity(product: Product): void {
    product.quantity++;
  }

  decrementQuantity(product: Product): void {
    if (product.quantity > 0) {
      product.quantity--;
    }
  }

  onAddToCart(product: Product): void {
    if (product.quantity > 0) {
      this.addToCartEvent.emit(product.quantity); // Emits the quantity to the parent
      product.quantity = 0; // Reset quantity after adding to cart
    }
  }
}
