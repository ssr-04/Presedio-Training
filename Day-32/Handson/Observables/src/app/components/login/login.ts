import { CommonModule } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import {FormBuilder, FormGroup, ReactiveFormsModule} from '@angular/forms';
import { AuthService } from '../../services/auth';
import { Router } from '@angular/router';

@Component({
  selector: 'app-login',
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './login.html',
  styleUrl: './login.css'
})
export class Login {
  private fb = inject(FormBuilder);
  private authService = inject(AuthService);
  private router = inject(Router);

  loginForm: FormGroup;
  isLoading = signal(false);
  errorMessage = signal<string | null>(null);

  constructor(){
    console.log("hello")
    this.loginForm = this.fb.group({
      username: ['emilys'],
      password: ['emilyspass']
    });
  }

  onSubmit():void {
    if(this.loginForm.invalid) {
      return;
    }
    this.isLoading.set(true);
    this.errorMessage.set(null);

    this.authService.login(this.loginForm.value).subscribe({
      next:() => {
        this.router.navigate(['/home']);
      },
      error: (err) => {
        this.errorMessage.set('Invalid username or password. Please try again.');
        console.error('Login failed:', err);
        this.isLoading.set(false);
      },
      complete: () => {
        this.isLoading.set(false);
      }
    })
  }
}
