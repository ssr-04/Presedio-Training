import { Component, inject } from '@angular/core';
import { Products } from './products/products';
import { AuthService } from './Services/auth.service';
import { RouterLink, RouterOutlet } from '@angular/router';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-root',
  templateUrl: './app.html',
  styleUrl: './app.css',
  imports: [RouterOutlet, RouterLink, CommonModule]
})
export class App {
  protected title = 'E-Commerce';
  protected authService = inject(AuthService);

  isLoggedIn(): boolean {
    return this.authService.isLoggedIn();
  }

  onLogout(): void {
    this.authService.logout();
  }
}