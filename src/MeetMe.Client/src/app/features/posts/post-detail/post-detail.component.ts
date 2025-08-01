import { Component, Input, Output, EventEmitter, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { PostsService } from '../../../core/services/posts.service';
import { AuthService } from '../../../core/services/auth.service';
import { Post, Comment, CreateCommentRequest } from '../../../shared/models';
import { CommentComponent } from '../comment/comment.component';

@Component({
  selector: 'app-post-detail',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, CommentComponent],
  templateUrl: './post-detail.component.html',
  styleUrl: './post-detail.component.scss'
})
export class PostDetailComponent implements OnInit {
  @Input() post!: Post;
  @Output() postUpdated = new EventEmitter<Post>();
  @Output() postDeleted = new EventEmitter<string>();

  currentUserId: string | null = null;
  isEditing = false;
  showComments = false;
  showCommentForm = false;

  editForm: FormGroup;
  commentForm: FormGroup;

  isUpdatingPost = false;
  isCreatingComment = false;
  errorMessage = '';

  constructor(
    private fb: FormBuilder,
    private postsService: PostsService,
    private authService: AuthService
  ) {
    this.editForm = this.fb.group({
      content: ['', [Validators.required, Validators.minLength(1), Validators.maxLength(1000)]]
    });

    this.commentForm = this.fb.group({
      content: ['', [Validators.required, Validators.minLength(1), Validators.maxLength(500)]]
    });
  }

  ngOnInit(): void {
    const currentUser = this.authService.getCurrentUser();
    this.currentUserId = currentUser?.id || null;
    this.editForm.patchValue({ content: this.post.content });
  }

  get canEditPost(): boolean {
    return this.currentUserId === this.post.authorId;
  }

  get formattedDate(): string {
    return new Date(this.post.createdAt).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  }

  startEdit(): void {
    this.isEditing = true;
    this.editForm.patchValue({ content: this.post.content });
  }

  cancelEdit(): void {
    this.isEditing = false;
    this.editForm.patchValue({ content: this.post.content });
    this.errorMessage = '';
  }

  saveEdit(): void {
    if (this.editForm.valid && !this.isUpdatingPost) {
      this.isUpdatingPost = true;
      this.errorMessage = '';

      const newContent = this.editForm.value.content.trim();

      this.postsService.updatePost(this.post.id, newContent).subscribe({
        next: (updatedPost) => {
          this.postUpdated.emit(updatedPost);
          this.isEditing = false;
          this.isUpdatingPost = false;
        },
        error: (error) => {
          this.errorMessage = 'Failed to update post. Please try again.';
          this.isUpdatingPost = false;
          console.error('Error updating post:', error);
        }
      });
    }
  }

  deletePost(): void {
    if (confirm('Are you sure you want to delete this post? This action cannot be undone.')) {
      this.postsService.deletePost(this.post.id).subscribe({
        next: () => {
          this.postDeleted.emit(this.post.id);
        },
        error: (error) => {
          this.errorMessage = 'Failed to delete post. Please try again.';
          console.error('Error deleting post:', error);
        }
      });
    }
  }

  toggleComments(): void {
    this.showComments = !this.showComments;
  }

  toggleCommentForm(): void {
    this.showCommentForm = !this.showCommentForm;
    if (this.showCommentForm) {
      this.showComments = true;
    }
  }

  onCommentSubmit(): void {
    if (this.commentForm.valid && !this.isCreatingComment) {
      this.isCreatingComment = true;
      this.errorMessage = '';

      const request: CreateCommentRequest = {
        content: this.commentForm.value.content.trim(),
        postId: this.post.id
      };

      this.postsService.createComment(request).subscribe({
        next: (comment) => {
          this.post.comments.push(comment);
          this.commentForm.reset();
          this.showCommentForm = false;
          this.isCreatingComment = false;
        },
        error: (error) => {
          this.errorMessage = 'Failed to create comment. Please try again.';
          this.isCreatingComment = false;
          console.error('Error creating comment:', error);
        }
      });
    }
  }

  onCommentUpdated(updatedComment: Comment): void {
    const index = this.post.comments.findIndex(c => c.id === updatedComment.id);
    if (index !== -1) {
      this.post.comments[index] = updatedComment;
    }
  }

  onCommentDeleted(commentId: string): void {
    this.post.comments = this.post.comments.filter(c => c.id !== commentId);
  }

  get commentContentControl() {
    return this.commentForm.get('content');
  }

  get remainingCommentChars(): number {
    const content = this.commentContentControl?.value || '';
    return 500 - content.length;
  }

  trackByCommentId(index: number, comment: Comment): string {
    return comment.id;
  }
}
