import { CommonModule } from '@angular/common';
import { Component, inject, OnInit } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { User } from '../Models/user';
import { AuthService } from '../Services/auth.service';

@Component({
  selector: 'app-dashboard',
  imports: [CommonModule, RouterLink],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.css'
})
export class Dashboard implements OnInit{
  currentUser: User | null = null;
  lastActivity: string | null =null;
  private authService = inject(AuthService);
  private router = inject(Router);

  ngOnInit(): void {
    this.currentUser = this.authService.getUserFromLocalStorage();
    this.lastActivity = this.authService.getLastSessionActivity();
    this.authService.updateSessionActivity();
  }

  onLogout(): void {
    this.authService.logout();
    this.currentUser = null;
    this.lastActivity = null;
    this.router.navigate(['/login']);
  }
}
