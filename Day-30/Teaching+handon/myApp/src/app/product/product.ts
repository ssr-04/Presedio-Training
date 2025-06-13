import { Component, EventEmitter, inject, Input, Output } from '@angular/core';
import { CurrencyPipe } from '@angular/common';
import { ProductModel } from '../Models/product-model';
import { ProductService } from '../Services/product.service';

@Component({
  selector: 'app-product',
  imports: [CurrencyPipe],
  templateUrl: './product.html',
  styleUrl: './product.css'
})
export class Product {

@Input() product:ProductModel|null = new ProductModel();
@Input() isLoggedIn: boolean = false;
@Output() addToCart:EventEmitter<number> = new EventEmitter<number>();

handleBuyClick(pid:number|undefined){
  if(pid && this.isLoggedIn)
  {
      this.addToCart.emit(pid);
  }
}

constructor(){
}

}