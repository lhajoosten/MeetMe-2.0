import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Attendance } from '../../shared/models';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class AttendanceService {
  private readonly baseUrl = environment.apiUrl + '/attendances';

  constructor(private http: HttpClient) {}

  joinMeeting(meetingId: string): Observable<Attendance> {
    return this.http.post<Attendance>(this.baseUrl, { meetingId });
  }

  leaveMeeting(attendanceId: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${attendanceId}`);
  }

  updateAttendanceStatus(attendanceId: string, status: string): Observable<Attendance> {
    return this.http.put<Attendance>(`${this.baseUrl}/${attendanceId}`, { status });
  }

  getMeetingAttendances(meetingId: string): Observable<Attendance[]> {
    return this.http.get<Attendance[]>(`${environment.apiUrl}/meetings/${meetingId}/attendances`);
  }

  getUserAttendances(userId?: string): Observable<Attendance[]> {
    const url = userId
      ? `${environment.apiUrl}/users/${userId}/attendances`
      : `${environment.apiUrl}/users/me/attendances`;
    return this.http.get<Attendance[]>(url);
  }
}
