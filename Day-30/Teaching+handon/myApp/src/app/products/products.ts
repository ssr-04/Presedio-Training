import { Component, inject, OnInit } from '@angular/core';
import { Product } from "../product/product";
import { ProductModel } from '../Models/product-model';
import { ProductService } from '../Services/product.service';
import { CartItem } from '../Models/cart-tem';
import { CommonModule } from '@angular/common';
import { User } from '../Models/user';
import { AuthService } from '../Services/auth.service';


@Component({
  selector: 'app-products',
  imports: [Product, CommonModule],
  templateUrl: './products.html',
  styleUrl: './products.css'
})


export class Products implements OnInit {

  products:ProductModel[]|undefined=undefined;
  cartItems:CartItem[] =[];
  cartCount:number =0;

  loggedInUser: User | null = null; 
  isLoggedIn: boolean = false; 

  private readonly SESSION_CART_KEY = 'currentCart';

  private productService = inject(ProductService); 
  private authService = inject(AuthService); 

  constructor(){

  }


  ngOnInit(): void {
    this.loggedInUser = this.authService.getUserFromLocalStorage();
    this.isLoggedIn = this.authService.isLoggedIn();
    if(this.isLoggedIn)
    {
      this.loadCartFromSessionStorage();
    }
    this.productService.getAllProducts().subscribe(
      {
        next:(data:any)=>{
         this.products = data.products as ProductModel[];
        },
        error: (err) => {
          console.error('Error fetching products:', err);
        },
        complete: () => {
          console.log("All products loaded.");
        }
      }
    )
  }

  ngOnDestroy(): void {
    // Save cart when the component is destroyed (like navigating away)
    this.saveCartToSessionStorage();
  }

  handleAddToCart(productId: number): void {
    if (!this.isLoggedIn) {
      console.warn("User is not logged in. Cannot add to cart.");
      return;
    }

    console.log("Adding product ID to cart: " + productId);
    let existingItem = this.cartItems.find(item => item.Id === productId);

    if (existingItem) {
      existingItem.Count++;
    } else {
      this.cartItems.push(new CartItem(productId, 1));
    }
    this.updateCartCount(); 
    this.saveCartToSessionStorage(); // Saving cart after every change
  }

  updateCartCount(): void {
    this.cartCount = this.cartItems.reduce((sum, item) => sum + item.Count, 0);
  }

  clearCart(): void {
    this.cartItems = [];
    this.cartCount = 0;
    sessionStorage.removeItem(this.SESSION_CART_KEY);
    console.log("Cart cleared from session storage.");
  }


  private saveCartToSessionStorage(): void {
    try {
      sessionStorage.setItem(this.SESSION_CART_KEY, JSON.stringify(this.cartItems));
      console.log('Cart saved to session storage:', this.cartItems);
    } catch (e) {
      console.error('Error saving cart to session storage:', e);
    }
  }

  private loadCartFromSessionStorage(): void {
    try {
      const storedCart = sessionStorage.getItem(this.SESSION_CART_KEY);
      if (storedCart) {
        const parsedCart: CartItem[] = JSON.parse(storedCart);
        this.cartItems = parsedCart.map(item => new CartItem(item.Id, item.Count));
        this.updateCartCount();
        console.log('Cart loaded from session storage:', this.cartItems);
      }
    } catch (e) {
      console.error('Error loading cart from session storage:', e);
      sessionStorage.removeItem(this.SESSION_CART_KEY);
    }
  }

}