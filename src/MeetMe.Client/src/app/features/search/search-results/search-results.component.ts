import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { MeetingsService } from '../../../core/services/meetings.service';
import { Meeting, SearchFilters } from '../../../shared/models';
import { IconComponent } from '../../../shared/components/icon/icon.component';

@Component({
  selector: 'app-search-results',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule, IconComponent],
  templateUrl: './search-results.component.html',
  styleUrl: './search-results.component.scss'
})
export class SearchResultsComponent implements OnInit {
  meetings: Meeting[] = [];
  filters: SearchFilters = {
    query: '',
    location: '',
    startDate: '',
    isPublic: true
  };
  isLoading = false;
  searchQuery = '';

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private meetingsService: MeetingsService
  ) {}

  ngOnInit(): void {
    // Get search parameters from route
    this.route.queryParams.subscribe(params => {
      if (params['q']) {
        this.filters.query = params['q'];
        this.searchQuery = params['q'];
      }
      if (params['location']) {
        this.filters.location = params['location'];
      }
      if (params['date']) {
        this.filters.startDate = params['date'];
      }
      if (params['public'] !== undefined) {
        this.filters.isPublic = params['public'] === 'true';
      }

      this.search();
    });
  }

  search(): void {
    this.isLoading = true;

    // Update URL with current filters
    this.router.navigate([], {
      relativeTo: this.route,
      queryParams: {
        q: this.filters.query || null,
        location: this.filters.location || null,
        date: this.filters.startDate || null,
        public: this.filters.isPublic
      },
      queryParamsHandling: 'merge'
    });

    this.meetingsService.getAllMeetings(this.filters).subscribe({
      next: (response) => {
        // Handle both array response and paginated response
        this.meetings = Array.isArray(response) ? response : (response?.items || []);
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Search failed:', error);
        this.meetings = [];
        this.isLoading = false;
      }
    });
  }

  viewMeeting(id: string): void {
    this.router.navigate(['/meetings', id]);
  }

  formatDate(dateString: string): string {
    return new Date(dateString).toLocaleDateString('en-US', {
      weekday: 'short',
      month: 'short',
      day: 'numeric',
      hour: 'numeric',
      minute: '2-digit'
    });
  }
}
