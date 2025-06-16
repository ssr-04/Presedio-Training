import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiResponse } from '../Models/Product';

@Injectable()
export class ProductService {

  private http = inject(HttpClient);
  private readonly apiUrl = "https://dummyjson.com/products";

  getProducts(
      searchTerm: string,
      limit: number,
      skip: number
  ):Observable<ApiResponse>{
        const params = new HttpParams()
              .set('q', searchTerm)
              .set('limit', limit)
              .set('skip', skip);
        return this.http.get<ApiResponse>(`${this.apiUrl}/search`, {params});
    
  }
  
}
