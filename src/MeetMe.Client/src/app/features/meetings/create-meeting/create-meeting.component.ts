import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { MeetingsService } from '../../../core/services/meetings.service';
import { CreateMeetingRequest } from '../../../shared/models';
import { IconComponent } from '../../../shared/components/icon/icon.component';

@Component({
  selector: 'app-create-meeting',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule, IconComponent],
  templateUrl: './create-meeting.component.html',
  styleUrl: './create-meeting.component.scss'
})
export class CreateMeetingComponent {
  meetingForm: FormGroup;
  isLoading = false;
  errorMessage = '';
  successMessage = '';

  constructor(
    private fb: FormBuilder,
    private meetingsService: MeetingsService,
    private router: Router
  ) {
    this.meetingForm = this.fb.group({
      title: ['', [Validators.required, Validators.minLength(3)]],
      description: ['', [Validators.required, Validators.minLength(10)]],
      location: ['', [Validators.required]],
      startDate: ['', [Validators.required]],
      endDate: ['', [Validators.required]],
      maxAttendees: [null],
      isPublic: [true]
    });
  }

  onSubmit(): void {
    if (this.meetingForm.valid) {
      this.isLoading = true;
      this.errorMessage = '';

      const formValue = this.meetingForm.value;
      const request: CreateMeetingRequest = {
        title: formValue.title,
        description: formValue.description,
        location: formValue.location,
        startDate: formValue.startDate,
        endDate: formValue.endDate,
        maxAttendees: formValue.maxAttendees || undefined,
        isPublic: formValue.isPublic
      };

      this.meetingsService.createMeeting(request).subscribe({
        next: (meetingId) => {
          this.isLoading = false;
          this.successMessage = 'Meeting created successfully!';
          this.errorMessage = '';

          // Navigate to meetings list after a short delay to show success message
          setTimeout(() => {
            this.router.navigate(['/meetings']);
          }, 1500);
        },
        error: (error: any) => {
          this.isLoading = false;
          this.errorMessage = error.error?.message || 'Failed to create meeting. Please try again.';
        }
      });
    }
  }
}
