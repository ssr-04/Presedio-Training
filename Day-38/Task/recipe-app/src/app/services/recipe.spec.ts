import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { RecipeService } from './recipe';
import { RecipeAPIResponse, Recipe } from '../models/recipe';

describe('RecipeService', () => {
  let service: RecipeService;
  let httpTestingController: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [RecipeService]
    });
    service = TestBed.inject(RecipeService);
    httpTestingController = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpTestingController.verify(); // Ensure that no outstanding requests are uncaught.
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should retrieve recipes from the API', () => {
    const dummyRecipes: Recipe[] = [
      {
        id: 1,
        name: 'Pasta Carbonara',
        ingredients: ['Spaghetti', 'Eggs', 'Pancetta', 'Parmesan'],
        instructions: ['Cook pasta', 'Fry pancetta', 'Mix ingredients'],
        prepTimeMinutes: 15,
        cookTimeMinutes: 20,
        servings: 2,
        difficulty: 'Medium',
        cuisine: 'Italian',
        caloriesPerServing: 600,
        tags: ['dinner', 'italian'],
        userId: 1,
        image: 'carbonara.jpg',
        rating: 4.5,
        reviewCount: 100,
        mealType: ['dinner']
      },
      {
        id: 2,
        name: 'Chicken Curry',
        ingredients: ['Chicken', 'Curry powder', 'Coconut milk'],
        instructions: ['Cook chicken', 'Add spices'],
        prepTimeMinutes: 20,
        cookTimeMinutes: 30,
        servings: 4,
        difficulty: 'Medium',
        cuisine: 'Indian',
        caloriesPerServing: 500,
        tags: ['dinner', 'indian'],
        userId: 2,
        image: 'curry.jpg',
        rating: 4.0,
        reviewCount: 75,
        mealType: ['dinner']
      }
    ];

    const dummyApiResponse: RecipeAPIResponse = {
      recipes: dummyRecipes,
      total: 2,
      skip: 0,
      limit: 2
    };

    service.getRecipes().subscribe(recipes => {
      expect(recipes.length).toBe(2);
      expect(recipes).toEqual(dummyRecipes);
    });

    const req = httpTestingController.expectOne('https://dummyjson.com/recipes');
    expect(req.request.method).toBe('GET');
    req.flush(dummyApiResponse); // Provide the dummy response
  });

  it('should handle HTTP errors gracefully', () => {
    const errorMessage = 'Failed to fetch recipes';

    service.getRecipes().subscribe({
      next: () => fail('should have failed with the 404 error'),
      error: (error) => {
        expect(error.status).toBe(404);
        expect(error.statusText).toBe('Not Found');
      }
    });

    const req = httpTestingController.expectOne('https://dummyjson.com/recipes');
    req.flush('Something went wrong', { status: 404, statusText: 'Not Found' });
  });
});