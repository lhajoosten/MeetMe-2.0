import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import {
  Meeting,
  CreateMeetingRequest,
  UpdateMeetingRequest,
  SearchFilters,
  PaginatedResponse
} from '../../shared/models';
import { environment } from '../../../environments/environment';
import { NotificationService } from './notification.service';

@Injectable({
  providedIn: 'root'
})
export class MeetingsService {
  private readonly baseUrl = environment.apiUrl + '/meetings';

  constructor(
    private http: HttpClient,
    private notificationService: NotificationService
  ) {}

  getAllMeetings(filters?: SearchFilters): Observable<PaginatedResponse<Meeting> | Meeting[]> {
    let params = new HttpParams();

    if (filters) {
      if (filters.query) params = params.set('query', filters.query);
      if (filters.location) params = params.set('location', filters.location);
      if (filters.startDate) params = params.set('startDate', filters.startDate);
      if (filters.endDate) params = params.set('endDate', filters.endDate);
      if (filters.isPublic !== undefined) params = params.set('isPublic', filters.isPublic.toString());
    }

    return this.http.get<PaginatedResponse<Meeting> | Meeting[]>(this.baseUrl, { params });
  }

  getMeeting(id: string): Observable<Meeting> {
    return this.http.get<Meeting>(`${this.baseUrl}/${id}`);
  }

  createMeeting(request: CreateMeetingRequest): Observable<string> {
    return this.http.post<string>(this.baseUrl, request).pipe(
      tap(meetingId => {
        this.notificationService.showSuccess(
          'Meeting Created',
          `Successfully created meeting`,
          `/meetings/${meetingId}`,
          'View Meeting'
        );
      })
    );
  }

  updateMeeting(request: UpdateMeetingRequest): Observable<Meeting> {
    return this.http.put<Meeting>(`${this.baseUrl}/${request.id}`, request).pipe(
      tap(meeting => {
        this.notificationService.showSuccess(
          'Meeting Updated',
          `Successfully updated "${meeting.title}"`,
          `/meetings/${meeting.id}`,
          'View Meeting'
        );
      })
    );
  }

  deleteMeeting(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`).pipe(
      tap(() => {
        this.notificationService.showInfo(
          'Meeting Deleted',
          'The meeting has been successfully deleted'
        );
      })
    );
  }

  joinMeeting(meetingId: string): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/${meetingId}/join`, {}).pipe(
      tap(() => {
        this.notificationService.showSuccess(
          'Joined Meeting',
          'You have successfully joined the meeting',
          `/meetings/${meetingId}`,
          'View Meeting'
        );
      })
    );
  }

  leaveMeeting(meetingId: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${meetingId}/leave`).pipe(
      tap(() => {
        this.notificationService.showInfo(
          'Left Meeting',
          'You have left the meeting'
        );
      })
    );
  }
}
