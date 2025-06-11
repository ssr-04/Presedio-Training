import { Component, signal } from '@angular/core';
import { RecipeModel } from '../../models/Recipe';
import { RecipeService } from '../../services/recipe';
import { RecipeCard } from "../recipe-card/recipe-card";

@Component({
  selector: 'app-recipe-list',
  imports: [RecipeCard],
  templateUrl: './recipe-list.html',
  styleUrl: './recipe-list.css'
})
export class RecipeList {
  FetchedRecipes = signal<RecipeModel[]>([]);
  local:boolean = false;

  constructor(private recipeService: RecipeService)
  {
    this.recipeService.getRecipes().subscribe({
      next: (data:any) => 
        {
          if(this.local)
          {
            console.log(data)
            this.FetchedRecipes.set(data)
          }
          else
          {
            console.log(data.recipes as RecipeModel[])
            this.FetchedRecipes.set(data.recipes as RecipeModel[])
          }
          
        },
      error: (err) => console.log("Error fetching recipes", err),
      complete: () => console.log("Fetch completed")
    });
  }
}
