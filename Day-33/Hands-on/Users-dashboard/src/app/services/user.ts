import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { ApiUsersResponseModel, UserModel, UserFiltersModel } from '../Models/user.model';
import { Observable } from 'rxjs';

@Injectable()
export class UserService {

  private http = inject(HttpClient);
  private readonly apiUrl = 'https://dummyjson.com/users';

  constructor() { }

  getUsers(filters: UserFiltersModel, limit: number, skip: number): Observable<ApiUsersResponseModel> {
    let params = new HttpParams()
                .set('limit', limit.toString())
                .set('skip', skip.toString());

    // Note: Dummy json users doesn't support multiple filters so handling in the client side

    return this.http.get<ApiUsersResponseModel>(this.apiUrl, {params});
  }

  getUserById(id: string | number): Observable<UserModel> {
    return this.http.get<UserModel>(`${this.apiUrl}/${id}`);
  }

  addUser(user: Omit<UserModel, 'id'>): Observable<UserModel>{
    return this.http.post<UserModel>(`${this.apiUrl}/add`, user, {
      headers: {'Content-Type': 'application/json'}
    })
  }
}
