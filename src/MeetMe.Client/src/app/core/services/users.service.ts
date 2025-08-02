import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { User, UpdateUserRequest, ChangePasswordRequest } from '../../shared/models';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class UsersService {
  private readonly baseUrl = environment.apiUrl + '/users';

  constructor(private http: HttpClient) {}

  updateProfile(userId: string, request: UpdateUserRequest): Observable<User> {
    return this.http.put<User>(`${this.baseUrl}/${userId}`, request);
  }

  uploadProfilePicture(userId: string, file: File): Observable<User> {
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post<User>(`${this.baseUrl}/${userId}/profile-picture`, formData);
  }

  changePassword(userId: string, request: ChangePasswordRequest): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/${userId}/change-password`, request);
  }

  deleteAccount(userId: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${userId}`);
  }

  getUserById(userId: string): Observable<User> {
    return this.http.get<User>(`${this.baseUrl}/${userId}`);
  }

  getUserMeetings(userId: string): Observable<any[]> {
    return this.http.get<any[]>(`${this.baseUrl}/${userId}/meetings`);
  }

  exportUserData(userId: string): Observable<Blob> {
    return this.http.get(`${this.baseUrl}/${userId}/export`, {
      responseType: 'blob'
    });
  }
}
