import { Component, inject, OnInit, signal } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { CommonModule, Location } from '@angular/common';
import { UserService } from '../../../services/user';
import { UserModel } from '../../../Models/user.model';
import { switchMap } from 'rxjs';
import { Loader } from "../../shared/loader/loader";

@Component({
  selector: 'app-user-detail',
  imports: [Loader, CommonModule],
  templateUrl: './user-detail.html',
  styleUrl: './user-detail.css'
})

export class UserDetail implements OnInit{
  
  private route = inject(ActivatedRoute);
  private userService = inject(UserService);
  private location = inject(Location); // For the "back" button

  user = signal<UserModel | null>(null);
  isLoading = signal<boolean>(true);
  errorMessage = signal<string | null>(null);

  ngOnInit(): void {
    this.route.paramMap.pipe(
      switchMap(params => {
        const id = params.get('id');
        if (!id) {
          this.isLoading.set(false);
          this.errorMessage.set('User ID is missing from the URL.');
          return [];
        }
        this.isLoading.set(true);
        return this.userService.getUserById(id);
      })
    ).subscribe({
      next: (userData) => {
        this.user.set(userData);
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error('Error fetching user details:', err);
        this.errorMessage.set('Could not find the user.');
        this.isLoading.set(false);
      }
    });
  }

  goBack(): void {
    this.location.back();
  }

}
