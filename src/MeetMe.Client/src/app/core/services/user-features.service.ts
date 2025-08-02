import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { User, Meeting, PaginatedResponse } from '../../shared/models';
import { NotificationService } from './notification.service';

export interface UserConnection {
  id: string;
  followerId: string;
  followingId: string;
  follower: User;
  following: User;
  createdAt: Date;
}

export interface UserActivity {
  id: string;
  userId: string;
  user: User;
  activityType: 'joined_meeting' | 'created_meeting' | 'posted_comment' | 'left_review';
  description: string;
  metadata: any;
  createdAt: Date;
}

export interface MeetingRecommendation {
  meeting: Meeting;
  score: number;
  reasons: string[];
}

export interface UserStats {
  meetingsCreated: number;
  meetingsJoined: number;
  postsCreated: number;
  commentsCreated: number;
  followers: number;
  following: number;
  averageRating: number;
  totalReviews: number;
}

@Injectable({
  providedIn: 'root'
})
export class UserFeaturesService {
  private readonly baseUrl = environment.apiUrl;

  constructor(
    private http: HttpClient,
    private notificationService: NotificationService
  ) {}

  // User Following/Connections
  followUser(userId: string): Observable<UserConnection> {
    return this.http.post<UserConnection>(`${this.baseUrl}/users/${userId}/follow`, {}).pipe(
      tap(connection => {
        this.notificationService.showSuccess(
          'User Followed',
          `You are now following ${connection.following.firstName} ${connection.following.lastName}`,
          `/users/${userId}`,
          'View Profile'
        );
      })
    );
  }

  unfollowUser(userId: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/users/${userId}/follow`).pipe(
      tap(() => {
        this.notificationService.showInfo(
          'User Unfollowed',
          'You have unfollowed this user'
        );
      })
    );
  }

  getFollowers(userId: string): Observable<PaginatedResponse<UserConnection>> {
    return this.http.get<PaginatedResponse<UserConnection>>(`${this.baseUrl}/users/${userId}/followers`);
  }

  getFollowing(userId: string): Observable<PaginatedResponse<UserConnection>> {
    return this.http.get<PaginatedResponse<UserConnection>>(`${this.baseUrl}/users/${userId}/following`);
  }

  isFollowing(userId: string): Observable<boolean> {
    return this.http.get<boolean>(`${this.baseUrl}/users/${userId}/is-following`);
  }

  // User Activity Feed
  getUserActivity(userId?: string): Observable<PaginatedResponse<UserActivity>> {
    const url = userId
      ? `${this.baseUrl}/users/${userId}/activity`
      : `${this.baseUrl}/users/activity/feed`;
    return this.http.get<PaginatedResponse<UserActivity>>(url);
  }

  getFollowingActivity(): Observable<PaginatedResponse<UserActivity>> {
    return this.http.get<PaginatedResponse<UserActivity>>(`${this.baseUrl}/users/activity/following`);
  }

  // Meeting Recommendations
  getMeetingRecommendations(): Observable<MeetingRecommendation[]> {
    return this.http.get<MeetingRecommendation[]>(`${this.baseUrl}/meetings/recommendations`);
  }

  getPersonalizedMeetings(): Observable<PaginatedResponse<Meeting>> {
    return this.http.get<PaginatedResponse<Meeting>>(`${this.baseUrl}/meetings/personalized`);
  }

  // User Discovery
  getSuggestedUsers(): Observable<User[]> {
    return this.http.get<User[]>(`${this.baseUrl}/users/suggestions`);
  }

  discoverUsers(interests?: string[], location?: string): Observable<PaginatedResponse<User>> {
    let params: any = {};
    if (interests?.length) params.interests = interests.join(',');
    if (location) params.location = location;

    return this.http.get<PaginatedResponse<User>>(`${this.baseUrl}/users/discover`, { params });
  }

  // User Statistics
  getUserStats(userId: string): Observable<UserStats> {
    return this.http.get<UserStats>(`${this.baseUrl}/users/${userId}/stats`);
  }

  // Meeting Reviews/Ratings
  rateMeeting(meetingId: string, rating: number, review?: string): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/meetings/${meetingId}/rate`, {
      rating,
      review
    }).pipe(
      tap(() => {
        this.notificationService.showSuccess(
          'Rating Submitted',
          'Thank you for rating this meeting!'
        );
      })
    );
  }

  getMeetingReviews(meetingId: string): Observable<any[]> {
    return this.http.get<any[]>(`${this.baseUrl}/meetings/${meetingId}/reviews`);
  }

  // Interest Management
  updateUserInterests(interests: string[]): Observable<User> {
    return this.http.put<User>(`${this.baseUrl}/users/interests`, { interests }).pipe(
      tap(() => {
        this.notificationService.showSuccess(
          'Interests Updated',
          'Your interests have been updated successfully'
        );
      })
    );
  }

  getAvailableInterests(): Observable<string[]> {
    return this.http.get<string[]>(`${this.baseUrl}/interests`);
  }

  // Saved/Bookmarked Meetings
  saveMeeting(meetingId: string): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/meetings/${meetingId}/save`, {}).pipe(
      tap(() => {
        this.notificationService.showSuccess(
          'Meeting Saved',
          'Meeting has been added to your saved list'
        );
      })
    );
  }

  unsaveMeeting(meetingId: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/meetings/${meetingId}/save`).pipe(
      tap(() => {
        this.notificationService.showInfo(
          'Meeting Removed',
          'Meeting has been removed from your saved list'
        );
      })
    );
  }

  getSavedMeetings(): Observable<PaginatedResponse<Meeting>> {
    return this.http.get<PaginatedResponse<Meeting>>(`${this.baseUrl}/meetings/saved`);
  }

  isMeetingSaved(meetingId: string): Observable<boolean> {
    return this.http.get<boolean>(`${this.baseUrl}/meetings/${meetingId}/is-saved`);
  }

  // User Blocking/Reporting
  blockUser(userId: string, reason?: string): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/users/${userId}/block`, { reason }).pipe(
      tap(() => {
        this.notificationService.showWarning(
          'User Blocked',
          'This user has been blocked and will no longer be able to interact with you'
        );
      })
    );
  }

  unblockUser(userId: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/users/${userId}/block`).pipe(
      tap(() => {
        this.notificationService.showInfo(
          'User Unblocked',
          'This user has been unblocked'
        );
      })
    );
  }

  reportUser(userId: string, reason: string, description?: string): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/users/${userId}/report`, {
      reason,
      description
    }).pipe(
      tap(() => {
        this.notificationService.showSuccess(
          'Report Submitted',
          'Thank you for reporting this user. We will review your report.'
        );
      })
    );
  }

  reportMeeting(meetingId: string, reason: string, description?: string): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/meetings/${meetingId}/report`, {
      reason,
      description
    }).pipe(
      tap(() => {
        this.notificationService.showSuccess(
          'Report Submitted',
          'Thank you for reporting this meeting. We will review your report.'
        );
      })
    );
  }
}
