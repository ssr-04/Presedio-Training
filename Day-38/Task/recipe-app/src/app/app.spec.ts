import { ComponentFixture, TestBed } from '@angular/core/testing';
import { App } from './app';
import { RecipeList } from './components/recipe-list/recipe-list';
import { RecipeService } from './services/recipe'; 
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { of } from 'rxjs'; // For mocking service responses

describe('App', () => {
  let fixture: ComponentFixture<App>;
  let app: App;
  let mockRecipeService: jasmine.SpyObj<RecipeService>;

  beforeEach(async () => {

    mockRecipeService = jasmine.createSpyObj('RecipeService', ['getRecipes']);
    mockRecipeService.getRecipes.and.returnValue(of([])); // Returning an empty array for simplicity

    await TestBed.configureTestingModule({
      imports: [
        App,
        RecipeList, 
        HttpClientTestingModule 
      ],
      providers: [
        { provide: RecipeService, useValue: mockRecipeService }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(App);
    app = fixture.componentInstance;
    fixture.detectChanges(); 
  });

  it('should create the app', () => {
    expect(app).toBeTruthy();
  });

  it(`should have the 'recipe-app' title`, () => {
    expect(app.title).toEqual('recipe-app');
  });

  it('should render the RecipeList component', () => {
    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('app-recipe-list')).toBeTruthy();
  });
});