import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { MeetingsService } from '../../../core/services/meetings.service';
import { AuthService } from '../../../core/services/auth.service';
import { Meeting, SearchFilters } from '../../../shared/models';

@Component({
  selector: 'app-meetings-list',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  template: `
    <div class="meetings-container">
      <header class="meetings-header">
        <div class="header-content">
          <h1>Discover Meetings</h1>
          <p>Find and join amazing meetings in your area</p>
        </div>
        <div class="header-actions">
          <button class="btn btn-primary" routerLink="/meetings/create">
            <i class="icon-plus"></i> Create Meeting
          </button>
          <button class="btn btn-secondary" (click)="logout()">
            Logout
          </button>
        </div>
      </header>

      <div class="search-filters">
        <div class="filter-group">
          <input
            type="text"
            [(ngModel)]="filters.query"
            (input)="onSearchChange()"
            placeholder="Search meetings..."
            class="search-input"
          />
        </div>
        <div class="filter-group">
          <input
            type="text"
            [(ngModel)]="filters.location"
            (input)="onSearchChange()"
            placeholder="Location"
            class="search-input"
          />
        </div>
        <div class="filter-group">
          <label class="checkbox-label">
            <input
              type="checkbox"
              [(ngModel)]="filters.isPublic"
              (change)="onSearchChange()"
            />
            Public meetings only
          </label>
        </div>
      </div>

      <div class="meetings-grid" *ngIf="!isLoading; else loadingTemplate">
        <div class="meeting-card" *ngFor="let meeting of meetings" (click)="viewMeeting(meeting.id)">
          <div class="meeting-header">
            <h3>{{ meeting.title }}</h3>
            <span class="meeting-status" [class.public]="meeting.isPublic">
              {{ meeting.isPublic ? 'Public' : 'Private' }}
            </span>
          </div>

          <p class="meeting-description">{{ meeting.description }}</p>

          <div class="meeting-meta">
            <div class="meta-item">
              <i class="icon-calendar"></i>
              <span>{{ formatDate(meeting.startDate) }}</span>
            </div>
            <div class="meta-item">
              <i class="icon-location"></i>
              <span>{{ meeting.location }}</span>
            </div>
            <div class="meta-item">
              <i class="icon-users"></i>
              <span>{{ meeting.attendees.length }} attendees</span>
            </div>
          </div>

          <div class="meeting-organizer">
            <span>Organized by {{ meeting.organizer.firstName }} {{ meeting.organizer.lastName }}</span>
          </div>
        </div>
      </div>

      <ng-template #loadingTemplate>
        <div class="loading-container">
          <div class="loading-spinner"></div>
          <p>Loading meetings...</p>
        </div>
      </ng-template>

      <div class="empty-state" *ngIf="!isLoading && meetings.length === 0">
        <h3>No meetings found</h3>
        <p>Try adjusting your search criteria or create a new meeting.</p>
        <button class="btn btn-primary" routerLink="/meetings/create">Create Your First Meeting</button>
      </div>
    </div>
  `,
  styles: [`
    .meetings-container {
      max-width: 1200px;
      margin: 0 auto;
      padding: 20px;
    }

    .meetings-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 32px;
      padding-bottom: 24px;
      border-bottom: 1px solid #e1e5e9;
    }

    .header-content h1 {
      margin: 0 0 8px 0;
      color: #333;
      font-size: 32px;
      font-weight: 700;
    }

    .header-content p {
      margin: 0;
      color: #666;
      font-size: 16px;
    }

    .header-actions {
      display: flex;
      gap: 12px;
    }

    .search-filters {
      display: flex;
      gap: 16px;
      margin-bottom: 32px;
      padding: 20px;
      background: #f8f9fa;
      border-radius: 12px;
      flex-wrap: wrap;
    }

    .filter-group {
      flex: 1;
      min-width: 200px;
    }

    .search-input {
      width: 100%;
      padding: 12px 16px;
      border: 2px solid #e1e5e9;
      border-radius: 8px;
      font-size: 16px;
      transition: border-color 0.3s ease;
      box-sizing: border-box;
    }

    .search-input:focus {
      outline: none;
      border-color: #667eea;
    }

    .checkbox-label {
      display: flex;
      align-items: center;
      gap: 8px;
      font-size: 16px;
      color: #333;
      cursor: pointer;
      margin-top: 12px;
    }

    .meetings-grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(350px, 1fr));
      gap: 24px;
    }

    .meeting-card {
      background: white;
      border-radius: 12px;
      padding: 24px;
      box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
      transition: transform 0.3s ease, box-shadow 0.3s ease;
      cursor: pointer;
      border: 2px solid transparent;
    }

    .meeting-card:hover {
      transform: translateY(-4px);
      box-shadow: 0 8px 24px rgba(0, 0, 0, 0.15);
      border-color: #667eea;
    }

    .meeting-header {
      display: flex;
      justify-content: space-between;
      align-items: start;
      margin-bottom: 16px;
    }

    .meeting-header h3 {
      margin: 0;
      color: #333;
      font-size: 20px;
      font-weight: 600;
      flex: 1;
    }

    .meeting-status {
      padding: 4px 12px;
      border-radius: 20px;
      font-size: 12px;
      font-weight: 600;
      text-transform: uppercase;
      background: #e74c3c;
      color: white;
    }

    .meeting-status.public {
      background: #27ae60;
    }

    .meeting-description {
      color: #666;
      margin-bottom: 20px;
      line-height: 1.6;
      display: -webkit-box;
      -webkit-line-clamp: 3;
      -webkit-box-orient: vertical;
      overflow: hidden;
    }

    .meeting-meta {
      display: flex;
      flex-direction: column;
      gap: 8px;
      margin-bottom: 16px;
    }

    .meta-item {
      display: flex;
      align-items: center;
      gap: 8px;
      color: #666;
      font-size: 14px;
    }

    .meta-item i {
      width: 16px;
      color: #667eea;
    }

    .meeting-organizer {
      padding-top: 16px;
      border-top: 1px solid #e1e5e9;
      color: #666;
      font-size: 14px;
    }

    .btn {
      padding: 12px 24px;
      border: none;
      border-radius: 8px;
      font-size: 16px;
      font-weight: 600;
      cursor: pointer;
      transition: background-color 0.3s ease;
      text-decoration: none;
      display: inline-flex;
      align-items: center;
      gap: 8px;
    }

    .btn-primary {
      background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
      color: white;
    }

    .btn-primary:hover {
      background: linear-gradient(135deg, #5a6fd8 0%, #6a4190 100%);
    }

    .btn-secondary {
      background: #6c757d;
      color: white;
    }

    .btn-secondary:hover {
      background: #5a6268;
    }

    .loading-container {
      text-align: center;
      padding: 60px 20px;
    }

    .loading-spinner {
      width: 40px;
      height: 40px;
      border: 4px solid #e1e5e9;
      border-top: 4px solid #667eea;
      border-radius: 50%;
      animation: spin 1s linear infinite;
      margin: 0 auto 20px;
    }

    @keyframes spin {
      0% { transform: rotate(0deg); }
      100% { transform: rotate(360deg); }
    }

    .empty-state {
      text-align: center;
      padding: 60px 20px;
    }

    .empty-state h3 {
      color: #666;
      margin-bottom: 8px;
    }

    .empty-state p {
      color: #999;
      margin-bottom: 24px;
    }

    @media (max-width: 768px) {
      .meetings-header {
        flex-direction: column;
        align-items: stretch;
        gap: 20px;
      }

      .header-actions {
        justify-content: center;
      }

      .search-filters {
        flex-direction: column;
      }

      .meetings-grid {
        grid-template-columns: 1fr;
      }
    }
  `]
})
export class MeetingsListComponent implements OnInit {
  meetings: Meeting[] = [];
  isLoading = true;
  filters: SearchFilters = {
    isPublic: true
  };

  private searchTimeout: any;

  constructor(
    private meetingsService: MeetingsService,
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    this.loadMeetings();
  }

  loadMeetings(): void {
    this.isLoading = true;
    this.meetingsService.getAllMeetings(this.filters).subscribe({
      next: (response) => {
        this.meetings = response.items;
        this.isLoading = false;
      },
      error: (error: any) => {
        console.error('Error loading meetings:', error);
        this.isLoading = false;
      }
    });
  }

  onSearchChange(): void {
    clearTimeout(this.searchTimeout);
    this.searchTimeout = setTimeout(() => {
      this.loadMeetings();
    }, 500);
  }

  viewMeeting(id: string): void {
    // Navigate to meeting detail
    window.location.href = `/meetings/${id}`;
  }

  logout(): void {
    this.authService.logout();
    window.location.href = '/auth/login';
  }

  formatDate(dateString: string): string {
    return new Date(dateString).toLocaleDateString('en-US', {
      weekday: 'short',
      year: 'numeric',
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  }
}
