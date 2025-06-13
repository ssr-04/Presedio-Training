import { Component, inject } from '@angular/core';
import { AuthService } from '../Services/auth.service';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-login',
  imports: [FormsModule, CommonModule],
  templateUrl: './login.html',
  styleUrl: './login.css'
})
export class Login {
  username : string = '';
  password : string = '';
  errorMessage: string | null = null;

  private authService = inject(AuthService);
  private router = inject(Router);

  constructor() {}

  onLoginSubmit() : void {
    this.errorMessage = null; //clearing
    const user = this.authService.login(this.username, this.password);

    if(user) {
      console.log('Login suceesful!', user);
      this.router.navigate(['/products']);
    } else {
      this.errorMessage = 'Invalid username or password.';
      console.log('Login failed.');
    }
  }
}
