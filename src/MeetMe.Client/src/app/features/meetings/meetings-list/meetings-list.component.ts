import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { MeetingsService } from '../../../core/services/meetings.service';
import { Meeting, SearchFilters } from '../../../shared/models';
import { IconComponent } from '../../../shared/components/icon/icon.component';

@Component({
  selector: 'app-meetings-list',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule, IconComponent],
  templateUrl: './meetings-list.component.html',
  styleUrl: './meetings-list.component.scss'
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
    private router: Router
  ) {}

  ngOnInit(): void {
    this.loadMeetings();
  }

  // Public method to refresh meetings list
  refreshMeetings(): void {
    this.loadMeetings();
  }

  loadMeetings(): void {
    this.isLoading = true;
    this.meetingsService.getAllMeetings(this.filters).subscribe({
      next: (response) => {
        // Handle both array response and paginated response
        this.meetings = Array.isArray(response) ? response : (response?.items || []);
        this.isLoading = false;
      },
      error: (error: any) => {
        console.error('Error loading meetings:', error);
        this.meetings = []; // Reset to empty array on error
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

  navigateToSearch(): void {
    this.router.navigate(['/search'], {
      queryParams: {
        q: this.filters.query || null,
        location: this.filters.location || null,
        public: this.filters.isPublic
      }
    });
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
