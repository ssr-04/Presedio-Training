import { Component, inject, signal } from '@angular/core';
import { UserService } from '../../../services/user';
import { Router } from '@angular/router';
import { UserModel } from '../../../Models/user.model';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';

@Component({
  selector: 'app-user-add',
  imports: [CommonModule, FormsModule, ReactiveFormsModule],
  templateUrl: './user-add.html',
  styleUrl: './user-add.css'
})

export class UserAdd {
  private fb = inject(FormBuilder);
  private userService = inject(UserService);
  private router = inject(Router);

  addUserForm!: FormGroup;
  isLoading = signal(false);
  successMessage = signal<string | null>(null);
  errorMessage = signal<string | null>(null);

  ngOnInit(): void {
    this.addUserForm = this.fb.group({
      firstName: ['', [Validators.required, Validators.minLength(2)]],
      lastName: ['', [Validators.required, Validators.minLength(2)]],
      email: ['', [Validators.required, Validators.email]],
      age: [null, [Validators.required, Validators.min(18), Validators.max(99)]],
      username: ['', [Validators.required, Validators.min(5)]],
      company: this.fb.group({
        name: ['', Validators.required],
        department: ['', Validators.required],
        title: ['', Validators.required],
      }),
    });
  }

  get f() {
    return this.addUserForm.controls;
  }
  get companyControls() {
    return (this.addUserForm.get('company') as FormGroup).controls;
  }

  onSubmit(): void {
    this.addUserForm.markAllAsTouched();

    if (this.addUserForm.invalid) {
      return;
    }

    this.isLoading.set(true);
    this.successMessage.set(null);
    this.errorMessage.set(null);

    this.userService.addUser(this.addUserForm.value as Omit<UserModel, 'id'>).subscribe({
      next: (newUser) => {
        this.isLoading.set(false);
        this.successMessage.set(`Successfully added user: ${newUser.firstName} (ID: ${newUser.id}). Redirecting...`);
        // Reset form
        this.addUserForm.reset();
        setTimeout(() => this.router.navigate(['/user', newUser.id]), 2500);
      },
      error: (err) => {
        this.isLoading.set(false);
        this.errorMessage.set('Failed to add user. Please try again.');
        console.error('Add user error:', err);
      }
    });
  }
}
