import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { PostsService } from '../../../core/services/posts.service';
import { AuthService } from '../../../core/services/auth.service';
import { Post, Comment, CreateCommentRequest } from '../../../shared/models';

@Component({
  selector: 'app-post-detail',
  standalone: true,
  imports: [CommonModule, RouterModule, ReactiveFormsModule],
  templateUrl: './post-detail.component.html',
  styleUrl: './post-detail.component.scss'
})
export class PostDetailComponent implements OnInit {
  post: Post | null = null;
  comments: Comment[] = [];
  commentForm: FormGroup;
  isLoading = false;
  isSubmittingComment = false;
  errorMessage = '';
  currentUserId: string | null = null;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private postsService: PostsService,
    private authService: AuthService
  ) {
    this.commentForm = this.fb.group({
      content: ['', [Validators.required, Validators.minLength(1)]]
    });
  }

  ngOnInit(): void {
    this.getCurrentUser();
    this.loadPost();
  }

  getCurrentUser(): void {
    const user = this.authService.getCurrentUser();
    this.currentUserId = user?.id || null;
  }

  loadPost(): void {
    const postId = this.route.snapshot.paramMap.get('id');
    if (!postId) {
      this.router.navigate(['/meetings']);
      return;
    }

    this.isLoading = true;
    this.postsService.getPost(+postId).subscribe({
      next: (post) => {
        this.post = post;
        this.comments = post.comments || [];
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Error loading post:', error);
        this.errorMessage = 'Failed to load post';
        this.isLoading = false;
      }
    });
  }

  onSubmitComment(): void {
    if (this.commentForm.valid && this.post) {
      this.isSubmittingComment = true;

      const request: CreateCommentRequest = {
        content: this.commentForm.value.content,
        postId: this.post.id
      };

      this.postsService.createComment(request).subscribe({
        next: (comment) => {
          this.comments.push(comment);
          this.commentForm.reset();
          this.isSubmittingComment = false;
        },
        error: (error) => {
          console.error('Error creating comment:', error);
          this.isSubmittingComment = false;
        }
      });
    }
  }

  onDeleteComment(commentId: number): void {
    if (this.post) {
      this.postsService.deleteComment(this.post.id, commentId).subscribe({
        next: () => {
          this.comments = this.comments.filter(c => c.id !== commentId);
        },
        error: (error) => {
          console.error('Error deleting comment:', error);
        }
      });
    }
  }

  canDeleteComment(comment: Comment): boolean {
    return this.currentUserId === comment.authorId;
  }

  canDeletePost(): boolean {
    return this.currentUserId === this.post?.authorId;
  }

  onDeletePost(): void {
    if (this.post && confirm('Are you sure you want to delete this post?')) {
      this.postsService.deletePost(this.post.id).subscribe({
        next: () => {
          this.router.navigate(['/meetings', this.post?.meetingId]);
        },
        error: (error) => {
          console.error('Error deleting post:', error);
        }
      });
    }
  }

  trackByCommentId(index: number, comment: Comment): number {
    return comment.id;
  }
}
