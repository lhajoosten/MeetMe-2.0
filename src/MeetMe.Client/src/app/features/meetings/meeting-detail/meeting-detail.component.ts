import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule, FormsModule } from '@angular/forms';
import { MeetingsService } from '../../../core/services/meetings.service';
import { PostsService } from '../../../core/services/posts.service';
import { AttendanceService } from '../../../core/services/attendance.service';
import { AuthService } from '../../../core/services/auth.service';
import { Meeting, Post, CreatePostRequest, CreateCommentRequest, AttendanceStatus } from '../../../shared/models';

@Component({
  selector: 'app-meeting-detail',
  standalone: true,
  imports: [CommonModule, RouterModule, ReactiveFormsModule, FormsModule],
  templateUrl: './meeting-detail.component.html',
  styleUrl: './meeting-detail.component.scss'
})
export class MeetingDetailComponent implements OnInit {
  meeting: Meeting | null = null;
  posts: Post[] = [];
  postForm: FormGroup;
  isLoading = false;
  isPostLoading = false;
  expandedPosts = new Set<string>();
  newCommentContent: { [postId: string]: string } = {};

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private meetingsService: MeetingsService,
    private postsService: PostsService,
    private attendanceService: AttendanceService,
    private authService: AuthService,
    private fb: FormBuilder
  ) {
    this.postForm = this.fb.group({
      content: ['', [Validators.required]]
    });
  }

  ngOnInit(): void {
    const meetingId = this.route.snapshot.paramMap.get('id');
    if (meetingId) {
      this.loadMeeting(meetingId);
      this.loadPosts(meetingId);
    }
  }

  get currentUser() {
    return this.authService.getCurrentUser();
  }

  get isOrganizer(): boolean {
    return this.meeting?.organizerId === this.currentUser?.id;
  }

  get userAttendance() {
    return this.meeting?.attendees.find(a => a.userId === this.currentUser?.id);
  }

  loadMeeting(id: string): void {
    this.meetingsService.getMeeting(id).subscribe({
      next: (meeting) => {
        this.meeting = meeting;
      },
      error: (error) => {
        console.error('Error loading meeting:', error);
        this.router.navigate(['/meetings']);
      }
    });
  }

  loadPosts(meetingId: string): void {
    this.postsService.getPostsByMeeting(meetingId).subscribe({
      next: (posts: Post[]) => {
        this.posts = posts;
      },
      error: (error: any) => {
        console.error('Error loading posts:', error);
      }
    });
  }

  joinMeeting(): void {
    if (!this.meeting) return;

    this.isLoading = true;
    this.attendanceService.joinMeeting(this.meeting.id).subscribe({
      next: (attendance) => {
        if (this.meeting) {
          this.meeting.attendees.push(attendance);
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
        if (this.meeting && this.userAttendance) {
          this.meeting.attendees = this.meeting.attendees.filter(
            a => a.id !== this.userAttendance!.id
          );
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
    // TODO: Navigate to edit meeting component
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

  createPost(): void {
    if (this.postForm.valid && this.meeting) {
      this.isPostLoading = true;

      const request: CreatePostRequest = {
        content: this.postForm.value.content,
        meetingId: this.meeting.id
      };

      this.postsService.createPost(request).subscribe({
        next: (post) => {
          this.posts.unshift(post);
          this.postForm.reset();
          this.isPostLoading = false;
        },
        error: (error) => {
          console.error('Error creating post:', error);
          this.isPostLoading = false;
        }
      });
    }
  }

  toggleComments(postId: string): void {
    if (this.expandedPosts.has(postId)) {
      this.expandedPosts.delete(postId);
    } else {
      this.expandedPosts.add(postId);
    }
  }

  addComment(postId: string): void {
    const content = this.newCommentContent[postId];
    if (!content?.trim()) return;

    const request: CreateCommentRequest = {
      content: content.trim(),
      postId: postId
    };

    this.postsService.createComment(request).subscribe({
      next: (comment) => {
        // Find the post and add the comment
        const post = this.posts.find(p => p.id === postId);
        if (post) {
          post.comments.push(comment);
        }
        this.newCommentContent[postId] = '';
      },
      error: (error: any) => {
        console.error('Error creating comment:', error);
      }
    });
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

  formatPostDate(dateString: string): string {
    return new Date(dateString).toLocaleDateString('en-US', {
      month: 'short',
      day: 'numeric',
      hour: 'numeric',
      minute: '2-digit'
    });
  }
}
