import { Component, inject, Input } from '@angular/core';
import { CurrencyPipe } from '@angular/common';
import { ProductModel } from '../Models/product';
import { ProductService } from '../Services/Product-Service';

@Component({
  selector: 'app-product',
  imports: [CurrencyPipe],
  templateUrl: './product.html',
  styleUrl: './product.css'
})
export class Product {
@Input() product:ProductModel|null = new ProductModel();
private productService = inject(ProductService);

constructor(){
    // this.productService.getProduct(1).subscribe(
    //   {
    //     next:(data)=>{
     
    //       this.product = data as ProductModel;
    //       console.log(this.product)
    //     },
    //     error:(err)=>{
    //       console.log(err)
    //     },
    //     complete:()=>{
    //       console.log("All done");
    //     }
    //   })
}

}