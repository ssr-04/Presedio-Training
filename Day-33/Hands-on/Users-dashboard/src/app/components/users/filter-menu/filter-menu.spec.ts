import { ComponentFixture, TestBed } from '@angular/core/testing';

import { FilterMenu } from './filter-menu';

describe('FilterMenu', () => {
  let component: FilterMenu;
  let fixture: ComponentFixture<FilterMenu>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [FilterMenu]
    })
    .compileComponents();

    fixture = TestBed.createComponent(FilterMenu);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
