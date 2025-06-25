import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of, throwError, EMPTY } from 'rxjs';
import { RecipeList } from './recipe-list';
import { RecipeService } from '../../services/recipe';
import { Recipe } from '../../models/recipe';
import { CommonModule } from '@angular/common';
import { RecipeCard } from '../recipe-card/recipe-card';
import { DebugElement } from '@angular/core';
import { By } from '@angular/platform-browser';

describe('RecipeList', () => {
  let component: RecipeList;
  let fixture: ComponentFixture<RecipeList>;
  let mockRecipeService: jasmine.SpyObj<RecipeService>;

  const dummyRecipes: Recipe[] = [
    {
      id: 1, name: 'Test Recipe 1', ingredients: [], instructions: [],
      prepTimeMinutes: 0, cookTimeMinutes: 0, servings: 0, difficulty: 'Easy', cuisine: '',
      caloriesPerServing: 0, tags: [], userId: 0, image: 'img1.jpg', rating: 0, reviewCount: 0, mealType: []
    },
    {
      id: 2, name: 'Test Recipe 2', ingredients: [], instructions: [],
      prepTimeMinutes: 0, cookTimeMinutes: 0, servings: 0, difficulty: 'Easy', cuisine: '',
      caloriesPerServing: 0, tags: [], userId: 0, image: 'img2.jpg', rating: 0, reviewCount: 0, mealType: []
    }
  ];

  beforeEach(async () => {
    mockRecipeService = jasmine.createSpyObj('RecipeService', ['getRecipes']);

    // Setting a default successful return value for getRecipes.
    mockRecipeService.getRecipes.and.returnValue(of(dummyRecipes));

    await TestBed.configureTestingModule({
      imports: [CommonModule, RecipeList, RecipeCard],
      providers: [
        { provide: RecipeService, useValue: mockRecipeService }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(RecipeList);
    component = fixture.componentInstance;
  
    fixture.detectChanges();
  });

  

  it('should create', () => {
    expect(component).toBeTruthy();
  });

 
  it('should load recipes on ngOnInit', () => {
    expect(mockRecipeService.getRecipes).toHaveBeenCalledTimes(1);

    component.recipes$?.subscribe(recipes => {
      expect(recipes).toEqual(dummyRecipes);
    });

    const recipeCards: DebugElement[] = fixture.debugElement.queryAll(By.directive(RecipeCard));
    expect(recipeCards.length).toBe(dummyRecipes.length);

    expect(recipeCards[0].componentInstance.recipe).toEqual(dummyRecipes[0]);
  });


  it('should not display error message initially (on successful load)', () => {
    expect(component.error).toBeNull();
    const errorElement: HTMLElement = fixture.nativeElement.querySelector('.error-message');
    expect(errorElement).toBeFalsy();
  });
});