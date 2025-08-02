import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule, FormsModule } from '@angular/forms';
import { MeetingsService } from '../../../core/services/meetings.service';
import { AttendanceService } from '../../../core/services/attendance.service';
import { AuthService } from '../../../core/services/auth.service';
import { Meeting, Attendance, AttendanceStatus } from '../../../shared/models';
import { PostListComponent } from '../../posts/post-list/post-list.component';
import { IconComponent } from '../../../shared/components/icon/icon.component';

@Component({
  selector: 'app-meeting-detail',
  standalone: true,
  imports: [CommonModule, RouterModule, ReactiveFormsModule, FormsModule, PostListComponent, IconComponent],
  templateUrl: './meeting-detail.component.html',
  styleUrl: './meeting-detail.component.scss'
})
export class MeetingDetailComponent implements OnInit {
  meeting: Meeting | null = null;
  userAttendance: Attendance | null = null;
  isLoading = false;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private meetingsService: MeetingsService,
    private attendanceService: AttendanceService,
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    const meetingId = this.route.snapshot.paramMap.get('id');
    if (meetingId) {
      this.loadMeeting(meetingId);
    }
  }

  get currentUser() {
    return this.authService.getCurrentUser();
  }

  get isOrganizer(): boolean {
    return this.meeting?.creatorId === this.currentUser?.id;
  }

  loadMeeting(id: string): void {
    this.meetingsService.getMeeting(id).subscribe({
      next: (meeting) => {
        this.meeting = meeting;
        this.loadUserAttendance(id);
      },
      error: (error) => {
        console.error('Error loading meeting:', error);
        this.router.navigate(['/meetings']);
      }
    });
  }

  loadUserAttendance(meetingId: string): void {
    if (!this.currentUser) return;

    this.attendanceService.getMeetingAttendances(meetingId).subscribe({
      next: (attendances) => {
        this.userAttendance = attendances.find(a => a.userId === this.currentUser?.id) || null;
      },
      error: (error) => {
        console.error('Error loading user attendance:', error);
        this.userAttendance = null;
      }
    });
  }

  joinMeeting(): void {
    if (!this.meeting) return;

    this.isLoading = true;
    this.attendanceService.joinMeeting(this.meeting.id).subscribe({
      next: (attendance) => {
        this.userAttendance = attendance;
        if (this.meeting) {
          // Update the attendee count since we don't have the full array
          this.meeting.attendeeCount = (this.meeting.attendeeCount || 0) + 1;
        }
        this.isLoading = false;
      },
      error: (error: any) => {
        console.error('Error joining meeting:', error);
        this.isLoading = false;
      }
    });
  }

  leaveMeeting(): void {
    if (!this.meeting || !this.userAttendance) return;

    this.isLoading = true;
    this.attendanceService.leaveMeeting(this.userAttendance.id).subscribe({
      next: () => {
        this.userAttendance = null;
        if (this.meeting) {
          // Update the attendee count
          this.meeting.attendeeCount = Math.max(0, (this.meeting.attendeeCount || 0) - 1);
        }
        this.isLoading = false;
      },
      error: (error: any) => {
        console.error('Error leaving meeting:', error);
        this.isLoading = false;
      }
    });
  }

  editMeeting(): void {
    this.router.navigate(['/meetings', this.meeting?.id, 'edit']);
  }

  deleteMeeting(): void {
    if (!this.meeting) return;

    if (confirm('Are you sure you want to delete this meeting? This action cannot be undone.')) {
      this.meetingsService.deleteMeeting(this.meeting.id).subscribe({
        next: () => {
          this.router.navigate(['/meetings']);
        },
        error: (error) => {
          console.error('Error deleting meeting:', error);
        }
      });
    }
  }

  formatDate(dateString: string): string {
    return new Date(dateString).toLocaleDateString('en-US', {
      weekday: 'long',
      year: 'numeric',
      month: 'long',
      day: 'numeric'
    });
  }

  formatTime(dateString: string): string {
    return new Date(dateString).toLocaleTimeString('en-US', {
      hour: 'numeric',
      minute: '2-digit',
      hour12: true
    });
  }
}
