import { ComponentFixture, TestBed } from '@angular/core/testing';
import { RecipeCard } from './recipe-card';
import { Recipe } from '../../models/recipe';
import { CommonModule, NgOptimizedImage } from '@angular/common';

describe('RecipeCard', () => {
  let component: RecipeCard;
  let fixture: ComponentFixture<RecipeCard>;

  const mockRecipe: Recipe = {
    id: 1,
    name: 'Test Recipe Name', 
    ingredients: ['Ingredient 1', 'Ingredient 2'],
    instructions: ['Step 1', 'Step 2'],
    prepTimeMinutes: 10,
    cookTimeMinutes: 20,
    servings: 4,
    difficulty: 'Easy', // To Match ngClass logic
    cuisine: 'Test Cuisine',
    caloriesPerServing: 300,
    tags: ['quick', 'healthy'],
    userId: 123,
    image: 'https://via.placeholder.com/320x640',
    rating: 4.8,
    reviewCount: 50,
    mealType: ['lunch']
  };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CommonModule, NgOptimizedImage, RecipeCard], 
    }).compileComponents();

    fixture = TestBed.createComponent(RecipeCard);
    component = fixture.componentInstance;
    component.recipe = mockRecipe; 
    fixture.detectChanges(); 
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should display the recipe image', () => {
    const imgElement: HTMLImageElement = fixture.nativeElement.querySelector('img'); 
    expect(imgElement).toBeTruthy();
    expect(imgElement.src).toContain(mockRecipe.image);
    expect(imgElement.alt).toContain(mockRecipe.name);
  });

  it('should display the recipe name', () => {
    const nameElement: HTMLElement = fixture.nativeElement.querySelector('h3.text-xl.font-bold.text-gray-800');
    expect(nameElement).toBeTruthy();
    expect(nameElement.textContent).toContain(mockRecipe.name);
  });

  it('should display cuisine', () => {
    const cuisineElement: HTMLElement = fixture.nativeElement.querySelector('div > span.font-semibold.uppercase.tracking-wider');
    expect(cuisineElement).toBeTruthy();
    expect(cuisineElement.textContent).toContain(mockRecipe.cuisine);
  });

  it('should display difficulty with correct styling', () => {
    const difficultyElement: HTMLElement = fixture.nativeElement.querySelector('div > span.px-2.py-1.rounded-full');
    expect(difficultyElement).toBeTruthy();
    expect(difficultyElement.textContent).toContain(mockRecipe.difficulty);

    // Testing ngClass application
    if (mockRecipe.difficulty === 'Easy') {
      expect(difficultyElement.classList).toContain('bg-green-100');
      expect(difficultyElement.classList).toContain('text-green-800');
    }
  });

  it('should display rating and review count', () => {
    const ratingElement: HTMLElement = fixture.nativeElement.querySelector('div.flex.items-center.mt-auto.text-yellow-500 > span.font-bold.text-gray-700');
    const reviewCountElement: HTMLElement = fixture.nativeElement.querySelector('div.flex.items-center.mt-auto.text-yellow-500 > span.text-sm.text-gray-500.ml-2');

    expect(ratingElement).toBeTruthy();
    expect(reviewCountElement).toBeTruthy();

    expect(ratingElement.textContent).toContain(mockRecipe.rating.toFixed(1)); 
    expect(reviewCountElement.textContent).toContain(`(${mockRecipe.reviewCount} reviews)`);
  });

});