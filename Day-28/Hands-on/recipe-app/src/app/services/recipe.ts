import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

@Injectable()
export class RecipeService {
  private dataUrl1 = 'assets/recipes.json';
  private dataUrl2 = 'https://dummyjson.com/recipes';
  private useLocal = false;

  constructor(private http: HttpClient) {}

  getRecipes(): Observable<any[]> {
    if(this.useLocal)
      return this.http.get<any[]>(this.dataUrl1);
    else
      return this.http.get<any[]>(this.dataUrl2);
  }
}