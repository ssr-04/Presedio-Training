import { Component, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { CommonModule, Location } from '@angular/common';
import { ProductService } from '../../services/product-service';
import { switchMap } from 'rxjs';
import { ProductDetailModel } from '../../Models/Product';
import { Loader } from "../loader/loader";

@Component({
  selector: 'app-product-detail',
  imports: [Loader, CommonModule],
  templateUrl: './product-detail.html',
  styleUrl: './product-detail.css'
})
export class ProductDetail implements OnInit{

  private route = inject(ActivatedRoute);
  private productService = inject(ProductService);
  private location = inject(Location); // for back button

  product = signal<ProductDetailModel | null>(null);
  isLoading = signal<boolean>(true);

  ngOnInit(): void {
    this.route.paramMap.pipe(
      switchMap(params => {
        const id = params.get('id');
        if(!id) {
          this.isLoading.set(false);
          alert("Invalid Product Id");
          throw new Error('Product Id is missing');
        }
        this.isLoading.set(true);
        return this.productService.getProductById(id);
      })
    ).subscribe({
      next: (productData) => {
        this.product.set(productData);
        this.isLoading.set(false)
      },
      error: (err) => {
        console.error('Error fetching product details:', err);
        this.product.set(null);
        this.isLoading.set(false);
      }
    })
  }

  goBack():void {
    this.location.back();
  }
}
