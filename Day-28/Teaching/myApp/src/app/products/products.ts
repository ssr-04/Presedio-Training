import { Component, OnInit } from '@angular/core';
import { Product } from "../product/product";
import { ProductModel } from '../Models/product';
import { ProductService } from '../Services/Product-Service';


@Component({
  selector: 'app-products',
  imports: [Product],
  templateUrl: './products.html',
  styleUrl: './products.css'
})
export class Products implements OnInit {
  products:ProductModel[]|undefined=undefined;
  constructor(private productService:ProductService){

  }
  ngOnInit(): void {
    this.productService.getAllProducts().subscribe(
      {
        next:(data:any)=>{
         this.products = data.products as ProductModel[];
        },
        error:(err)=>{},
        complete:()=>{}
      }
    )
  }

}