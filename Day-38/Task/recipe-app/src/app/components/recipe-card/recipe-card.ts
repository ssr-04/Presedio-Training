import { Component, Input } from '@angular/core';
import { Recipe } from '../../models/recipe';
import { CommonModule, NgOptimizedImage } from '@angular/common';

@Component({
  selector: 'app-recipe-card',
  imports: [CommonModule, NgOptimizedImage],
  templateUrl: './recipe-card.html',
  styleUrl: './recipe-card.css'
})
export class RecipeCard {
  @Input({ required: true }) recipe!: Recipe;
}
