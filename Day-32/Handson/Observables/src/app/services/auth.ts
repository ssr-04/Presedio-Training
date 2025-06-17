import { HttpClient, HttpHeaders } from '@angular/common/http';
import { inject, Injectable, signal } from '@angular/core';
import { Router } from '@angular/router';
import { AuthResponse, User } from '../Models/Product';
import { catchError, map, Observable, of, tap } from 'rxjs';

@Injectable()
export class AuthService {

  private http = inject(HttpClient);
  private router = inject(Router);

  private readonly apiUrl = 'https://dummyjson.com/auth';
  private readonly tokenKey = 'authToken';

  currentUser = signal<User|null|undefined>(undefined);

  constructor() { 
    this.loadInitialUser();
  }

  private loadInitialUser() {
    const token  = this.getToken();
    if(token) {
      this.verifyToken(token).subscribe();
    } else {
      this.currentUser.set(null);
    }
  }

  getToken() : string | null {
    return localStorage.getItem(this.tokenKey);
  }

  login(credentials: {username: string, password: string}): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.apiUrl}/login`, credentials).pipe(
      tap(response => {
        localStorage.setItem(this.tokenKey, response.accessToken);
        const {accessToken, ...user} = response;
        this.currentUser.set(user);
      })
    );
  }

  logout(): void {
    localStorage.removeItem(this.tokenKey);
    this.currentUser.set(null);
    this.router.navigate(['/login']);
  }

  verifyToken(token: string): Observable<User | null> {
    const headers = new HttpHeaders().set('Authorization', `Bearer ${token}`);
    return this.http.get<User>(`${this.apiUrl}/me`, {headers}).pipe(
      tap(user => this.currentUser.set(user)),
      catchError(() => {
        this.logout();
        return of(null);
      })
    )
  }

  isLoggedIn(): Observable<boolean> {
    const token = this.getToken();
    if(!token)
    {
      return of(false);
    }
    return this.verifyToken(token).pipe(
      map(user => !!user) // converts to boolean -> !something returns false of it, while !!returns boolean equvalent
    )
  }
}
