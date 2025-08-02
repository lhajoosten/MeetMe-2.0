import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { MeetingsService } from '../../../core/services/meetings.service';
import { Meeting, UpdateMeetingRequest } from '../../../shared/models';
import { IconComponent } from '../../../shared/components/icon/icon.component';

@Component({
  selector: 'app-edit-meeting',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule, IconComponent],
  templateUrl: './edit-meeting.component.html',
  styleUrl: './edit-meeting.component.scss'
})
export class EditMeetingComponent implements OnInit {
  meetingForm: FormGroup;
  meeting: Meeting | null = null;
  isLoading = false;
  errorMessage = '';

  constructor(
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
    private meetingsService: MeetingsService
  ) {
    this.meetingForm = this.fb.group({
      title: ['', [Validators.required]],
      description: ['', [Validators.required]],
      location: ['', [Validators.required]],
      startDate: ['', [Validators.required]],
      endDate: ['', [Validators.required]],
      maxAttendees: [''],
      isPublic: [true]
    });
  }

  ngOnInit(): void {
    const meetingId = this.route.snapshot.paramMap.get('id');
    if (meetingId) {
      this.loadMeeting(meetingId);
    }
  }

  loadMeeting(id: string): void {
    this.isLoading = true;
    this.meetingsService.getMeeting(id).subscribe({
      next: (meeting) => {
        this.meeting = meeting;
        this.populateForm(meeting);
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Error loading meeting:', error);
        this.router.navigate(['/meetings']);
      }
    });
  }

  populateForm(meeting: Meeting): void {
    // Format dates for datetime-local inputs
    const startDate = new Date(meeting.startDateTime).toISOString().slice(0, 16);
    const endDate = new Date(meeting.endDateTime).toISOString().slice(0, 16);

    this.meetingForm.patchValue({
      title: meeting.title,
      description: meeting.description,
      location: meeting.location,
      startDate: startDate,
      endDate: endDate,
      maxAttendees: meeting.maxAttendees || '',
      isPublic: meeting.isPublic
    });
  }

  onSubmit(): void {
    if (this.meetingForm.valid && this.meeting) {
      this.isLoading = true;
      this.errorMessage = '';

      const request: UpdateMeetingRequest = {
        id: this.meeting.id,
        title: this.meetingForm.value.title,
        description: this.meetingForm.value.description,
        location: this.meetingForm.value.location,
        startDate: this.meetingForm.value.startDate,
        endDate: this.meetingForm.value.endDate,
        maxAttendees: this.meetingForm.value.maxAttendees || undefined,
        isPublic: this.meetingForm.value.isPublic
      };

      this.meetingsService.updateMeeting(request).subscribe({
        next: (meeting) => {
          this.router.navigate(['/meetings', meeting.id]);
        },
        error: (error) => {
          this.errorMessage = error.error?.message || 'Failed to update meeting';
          this.isLoading = false;
        }
      });
    }
  }

  goBack(): void {
    if (this.meeting) {
      this.router.navigate(['/meetings', this.meeting.id]);
    } else {
      this.router.navigate(['/meetings']);
    }
  }

  deleteMeeting(): void {
    if (this.meeting && confirm('Are you sure you want to delete this meeting? This action cannot be undone.')) {
      this.meetingsService.deleteMeeting(this.meeting.id).subscribe({
        next: () => {
          this.router.navigate(['/meetings']);
        },
        error: (error) => {
          this.errorMessage = error.error?.message || 'Failed to delete meeting';
        }
      });
    }
  }
}
