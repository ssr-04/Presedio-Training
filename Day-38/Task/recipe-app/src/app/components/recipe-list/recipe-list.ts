import { Component, OnInit } from '@angular/core';
import { Observable, EMPTY } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { Recipe } from '../../models/recipe';
import { CommonModule } from '@angular/common';
import { RecipeService } from '../../services/recipe';
import { RecipeCard } from "../recipe-card/recipe-card";


@Component({
  selector: 'app-recipe-list',
  imports: [CommonModule, RecipeCard],
  templateUrl: './recipe-list.html',
  styleUrl: './recipe-list.css'
})
export class RecipeList {
  public recipes$!: Observable<Recipe[]>;
  public error: string | null = null;

  constructor(private recipeService: RecipeService) {}

  ngOnInit(): void {
    this.recipes$ = this.recipeService.getRecipes().pipe(
      catchError((err) => {
        console.error(err);
        this.error = 'Failed to load recipes. Please try again later.';
        return EMPTY;
      })
    );
  }

}
