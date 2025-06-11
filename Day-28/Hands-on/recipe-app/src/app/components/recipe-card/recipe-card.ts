import { Component, Input } from '@angular/core';
import { RecipeModel } from '../../models/Recipe';

@Component({
  selector: 'app-recipe-card',
  imports: [],
  templateUrl: './recipe-card.html',
  styleUrl: './recipe-card.css'
})
export class RecipeCard {
  @Input() recipe!: RecipeModel;
}
