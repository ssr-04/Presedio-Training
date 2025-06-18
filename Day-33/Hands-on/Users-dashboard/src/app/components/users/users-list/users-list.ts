import { Component, computed, HostListener, inject, OnInit, signal } from '@angular/core';
import { UserService } from '../../../services/user';
import { UserFiltersModel, UserModel } from '../../../Models/user.model';
import { FilterMenuComponent } from "../filter-menu/filter-menu";
import { UserCard } from "../user-card/user-card";

@Component({
  selector: 'app-users-list',
  imports: [FilterMenuComponent, UserCard],
  templateUrl: './users-list.html',
  styleUrl: './users-list.css'
})

export class UsersList implements OnInit{
  private userService = inject(UserService);

  // State signals
  allUsers = signal<UserModel[]>([]);
  isLoading = signal<boolean>(false);
  hasMoreUsers = signal<boolean>(true);
  filters = signal<UserFiltersModel>({});

  // Pagination
  private limit: number = 300;
  private skip = signal<number>(0);

  // Filtering
  filteredUsers = computed(() => {
  const users = this.allUsers();
  const currentFilters = this.filters();
  console.log(currentFilters);
  if(Object.keys(currentFilters).length === 0){
    return users;
  }
  return users.filter(user => {
    return Object.entries(currentFilters).every(([key, value]) => {
      if (!value) return true; 
      switch(key) {
        case 'gender':
          return user.gender.toLowerCase() === value.toLowerCase();
        case 'state':
          return user.address.state.toLowerCase() === value.toLowerCase();
        case 'university':
          return user.university.toLowerCase() === value.toLowerCase();
        case 'department':
          return user.company.department.toLowerCase() === value.toLowerCase();
        default:
          return true;
      }
    });
  });
});

  // Options for filter dropdown
  uniqueStates = computed(() => [...new Set(this.allUsers().map(u => u.address.state))].sort());
  uniqueUniversities = computed(() => [...new Set(this.allUsers().map(u => u.university))].sort());
  uniqueDepartments = computed(() => [...new Set(this.allUsers().map(u => u.company.department))].sort());

  ngOnInit(): void {
    this.loadUsers();
  }

  loadUsers(loadMore = true): void {
    if(this.isLoading() || !this.hasMoreUsers()){
      return;
    }
    this.isLoading.set(true);
    this.userService.getUsers(this.filters(), this.limit, this.skip()).subscribe(response => {
      if(loadMore) {
        // Appends new users to current list
        this.allUsers.update(current => [...current, ...response.users]);
      } else {
        // Replaces existing user list with filtered results
        this.allUsers.set(response.users);
      }
      // If loaded all users, disable further loading
      if(this.allUsers().length >= response.total){
        this.hasMoreUsers.set(false);
      }
      this.isLoading.set(false);
    });
  }

  onFilterChange(newFilters: UserFiltersModel): void {
    // Resets filters, pagination and user list before loading fresh data.
    this.filters.set(newFilters);
    this.skip.set(0); 
    this.allUsers.set([]); 
    this.hasMoreUsers.set(true); 
    this.loadUsers(false); 
  }

  @HostListener('window:scroll')
  OnScroll(): void {
    if(!this.isLoading() && this.hasMoreUsers() &&
      window.innerHeight + window.scrollY >= document.body.offsetHeight - 300){
      this.skip.update(val => val + this.limit);
      this.loadUsers(true);
    }
  }
}