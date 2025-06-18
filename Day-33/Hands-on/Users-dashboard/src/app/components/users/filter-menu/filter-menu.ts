import { Component, EventEmitter, Input, Output } from '@angular/core';
import { UserFiltersModel } from '../../../Models/user.model';
import { CommonModule } from '@angular/common';
import {FormsModule} from '@angular/forms';

@Component({
  selector: 'app-filter-menu',
  imports: [CommonModule, FormsModule],
  templateUrl: './filter-menu.html',
  styleUrl: './filter-menu.css'
})

export class FilterMenuComponent {
  @Input() states: string[] = [];
  @Input() universities: string[] = [];
  @Input() departments: string[] = [];
  @Output() filtersChanged = new EventEmitter<UserFiltersModel>();

  filters: UserFiltersModel = {
    gender: '',
    state: '',
    university: '',
    department: '',
  };

  onFilterChange(): void {
    this.filtersChanged.emit(this.filters);
  }

  resetFilters(): void {
    this.filters = { gender: '', state: '', university: '', department: '' };
    this.onFilterChange();
  }
}


