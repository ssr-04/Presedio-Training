import { CommonModule } from '@angular/common';
import { AfterViewInit, Component, ElementRef, inject, OnInit, ViewChild } from '@angular/core';
import { FormBuilder, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { Store } from '@ngrx/store';
import { UserModel } from '../models/user.model';
import { BehaviorSubject, combineLatest, debounceTime, distinctUntilChanged, fromEvent, map, Observable, startWith, tap } from 'rxjs';
import { bannedWordsValidator } from '../validators/bannedWordsValidator';
import { passwordStrengthValidator } from '../validators/passwordStrengthValidator';
import { passwordMatchValidator } from '../validators/passwordMatchValidator';
import { selectAllUsers } from '../store/users/users.selectors';
import { addUser } from '../store/users/users.actions';

@Component({
  selector: 'app-user-management',
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './user-management.html',
  styleUrl: './user-management.css'
})

export class UserManagement implements OnInit, AfterViewInit{

  private fb = inject(FormBuilder);
  private store = inject(Store);

  userForm!: FormGroup;
  roles: UserModel['role'][] = ['Admin', 'User', 'Guest'];

  private searchTerm$ = new BehaviorSubject<string>('');
  private roleFilter$ = new BehaviorSubject<string>('all');

  filteredUsers$!: Observable<UserModel[]>;

  @ViewChild('searchInput') searchInput!: ElementRef<HTMLInputElement>;

  showToast = false;
  toastMessage = '';

  ngOnInit(): void {
    this.initializeForm();
    this.initializeFilteredUsers();
  }
  ngAfterViewInit(): void {
    fromEvent(this.searchInput.nativeElement, 'keyup')
      .pipe(
        debounceTime(300),
        map(event => (event.target as HTMLInputElement).value.trim()),
        distinctUntilChanged(),
        tap(term => this.searchTerm$.next(term))
      )
      .subscribe();
  }

  private initializeForm(): void {
    this.userForm = this.fb.group({
      username: ['', [Validators.required, bannedWordsValidator(['admin', 'root'])]],
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, passwordStrengthValidator()]],
      confirmPassword: ['', Validators.required],
      role: ['User', Validators.required]
    }, {
      validators: passwordMatchValidator('password', 'confirmPassword')
    });
  }

   private initializeFilteredUsers(): void {
    const allUsers$ = this.store.select(selectAllUsers);

    this.filteredUsers$ = combineLatest([
      allUsers$,
      this.searchTerm$.pipe(startWith('')),
      this.roleFilter$.pipe(startWith('all')),
    ]).pipe(
      map(([users, term, role]) => {
        const lowerCaseTerm = term.toLowerCase();
        return users.filter(user => {
          const matchesTerm = user.username.toLowerCase().includes(lowerCaseTerm) || user.email.toLowerCase().includes(lowerCaseTerm);
          const matchesRole = role === 'all' || user.role === role;
          return matchesTerm && matchesRole;
        });
      })
    );
  }

  onRoleFilterChange(event: Event): void {
    const role = (event.target as HTMLSelectElement).value;
    this.roleFilter$.next(role);
  }

  onSubmit(): void {
    if (this.userForm.invalid) {
      this.userForm.markAllAsTouched();
      return;
    }
    
    const { confirmPassword, ...userData } = this.userForm.value;
    this.store.dispatch(addUser({ user: userData }));
    
    this.triggerToast('User added successfully!');
    this.userForm.reset({ role: 'User' });
  }
  
  private triggerToast(message: string): void {
    this.toastMessage = message;
    this.showToast = true;
    setTimeout(() => this.showToast = false, 3000);
  }

  get username() { return this.userForm.get('username'); }
  get email() { return this.userForm.get('email'); }
  get password() { return this.userForm.get('password'); }
  get confirmPassword() { return this.userForm.get('confirmPassword'); }

}
