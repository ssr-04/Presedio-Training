import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { map, Observable } from 'rxjs';
import { Recipe, RecipeAPIResponse } from '../models/recipe';

@Injectable({
  providedIn: 'root'
})
export class RecipeService {
  private readonly apiUrl = 'https://dummyjson.com/recipes';

  constructor(private http: HttpClient) { }

  
  getRecipes(): Observable<Recipe[]> {
    return this.http.get<RecipeAPIResponse>(this.apiUrl).pipe(
      map(response => response.recipes)
    );
  }
}