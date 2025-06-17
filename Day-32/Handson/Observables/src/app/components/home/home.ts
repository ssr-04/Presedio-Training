import { Component, HostListener, inject, OnInit, signal } from '@angular/core';
import { ProductService } from '../../services/product-service';
import { Product } from '../../Models/Product';
import { debounceTime, distinctUntilChanged, Subject, switchMap, tap } from 'rxjs';
import { ProductCard } from "../product-card/product-card";
import { Loader } from "../loader/loader";

@Component({
  selector: 'app-home',
  imports: [ProductCard, Loader],
  templateUrl: './home.html',
  styleUrl: './home.css'
})
export class Home implements OnInit{

    private productService = inject(ProductService);

    // State signals (to update only necessary state changes)
    products = signal<Product[]>([]);
    isLoading = signal<boolean>(false);
    hasMoreProducts = signal<boolean>(true);

    // Pagination and search
    private limit = 12;
    private skip = signal<number>(0);
    private searchTerm = new Subject<string>();

    currentSearchTerm = ''; // for highlighting

    ngOnInit(): void {
      this.setupSearchSubscription();
      this.searchTerm.next(''); // initially empty string
    }

    private setupSearchSubscription(): void {
      this.searchTerm.pipe(
        debounceTime(400), // wait for 400ms before request
        distinctUntilChanged(), // no request if search term is not changed
        tap((term) => {
          this.currentSearchTerm = term; // for highlighting,
          this.products.set([]); // Reset on new search
          this.skip.set(0); // reset pagination
          this.hasMoreProducts.set(true);
          this.isLoading.set(true);
        }),
        switchMap((term) => {
          return this.productService.getProducts(term, this.limit, this.skip());
        })
      ).subscribe({
        next: (response) => {
          this.products.set(response.products);
          //console.log(response);
          if(response.products.length < this.limit || this.products().length >= response.total){
            this.hasMoreProducts.set(false);
          }
          this.isLoading.set(false);
        },
        error: (err) => {
          console.error('Error fetching products', err);
          this.isLoading.set(false);
        }
      });
    }

    onSearch(event: Event): void {
      const inputElement = event.target as HTMLInputElement;
      this.searchTerm.next(inputElement.value);
    }

    @HostListener('window:scroll')
    onScroll(): void {
      console.log(!this.isLoading);
      if(this.hasMoreProducts() && window.innerHeight + window.scrollY >= document.body.offsetHeight - 200)
      {
        this.loadMore();
      }
    }

    loadMore() : void {
      this.isLoading.set(true);
      this.skip.set(this.skip() + this.limit);

      this.productService.getProducts(this.currentSearchTerm, this.limit, this.skip())
        .subscribe({
          next: (resp) => {
            if(resp.products.length > 0){
              this.products.update(currentProducts => [...currentProducts, ...resp.products]);
            }

            if(this.products().length >= resp.total){
              this.hasMoreProducts.set(false);
            }

            this.isLoading.set(false);
          },
          error: (err) => {
            console.error('Error loading more products:', err);
            this.isLoading.set(false);
          }
        })
    }
}
