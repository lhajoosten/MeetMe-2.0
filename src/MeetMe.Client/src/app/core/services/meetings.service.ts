import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  Meeting,
  CreateMeetingRequest,
  UpdateMeetingRequest,
  SearchFilters,
  PaginatedResponse
} from '../../shared/models';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class MeetingsService {
  private readonly baseUrl = environment.apiUrl + '/meetings';

  constructor(private http: HttpClient) {}

  getAllMeetings(filters?: SearchFilters): Observable<PaginatedResponse<Meeting>> {
    let params = new HttpParams();

    if (filters) {
      if (filters.query) params = params.set('query', filters.query);
      if (filters.location) params = params.set('location', filters.location);
      if (filters.startDate) params = params.set('startDate', filters.startDate);
      if (filters.endDate) params = params.set('endDate', filters.endDate);
      if (filters.isPublic !== undefined) params = params.set('isPublic', filters.isPublic.toString());
    }

    return this.http.get<PaginatedResponse<Meeting>>(this.baseUrl, { params });
  }

  getMeeting(id: string): Observable<Meeting> {
    return this.http.get<Meeting>(`${this.baseUrl}/${id}`);
  }

  createMeeting(request: CreateMeetingRequest): Observable<Meeting> {
    return this.http.post<Meeting>(this.baseUrl, request);
  }

  updateMeeting(request: UpdateMeetingRequest): Observable<Meeting> {
    return this.http.put<Meeting>(`${this.baseUrl}/${request.id}`, request);
  }

  deleteMeeting(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }

  joinMeeting(meetingId: string): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/${meetingId}/join`, {});
  }

  leaveMeeting(meetingId: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${meetingId}/leave`);
  }
}
