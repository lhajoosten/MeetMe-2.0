import { Component, Input, Output, EventEmitter, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { PostsService } from '../../../core/services/posts.service';
import { Comment } from '../../../shared/models';

@Component({
  selector: 'app-comment',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './comment.component.html',
  styleUrl: './comment.component.scss'
})
export class CommentComponent implements OnInit {
  @Input() comment!: Comment;
  @Input() currentUserId: string | null = null;
  @Output() commentUpdated = new EventEmitter<Comment>();
  @Output() commentDeleted = new EventEmitter<string>();

  isEditing = false;
  editForm: FormGroup;
  isUpdating = false;
  errorMessage = '';

  constructor(
    private fb: FormBuilder,
    private postsService: PostsService
  ) {
    this.editForm = this.fb.group({
      content: ['', [Validators.required, Validators.minLength(1), Validators.maxLength(500)]]
    });
  }

  ngOnInit(): void {
    this.editForm.patchValue({ content: this.comment.content });
  }

  get canEdit(): boolean {
    return this.currentUserId === this.comment.authorId;
  }

  get formattedDate(): string {
    return new Date(this.comment.createdAt).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  }

  startEdit(): void {
    this.isEditing = true;
    this.editForm.patchValue({ content: this.comment.content });
  }

  cancelEdit(): void {
    this.isEditing = false;
    this.editForm.patchValue({ content: this.comment.content });
    this.errorMessage = '';
  }

  saveEdit(): void {
    if (this.editForm.valid && !this.isUpdating) {
      this.isUpdating = true;
      this.errorMessage = '';

      const newContent = this.editForm.value.content.trim();

      this.postsService.updateComment(this.comment.postId, this.comment.id, newContent).subscribe({
        next: (updatedComment) => {
          this.commentUpdated.emit(updatedComment);
          this.isEditing = false;
          this.isUpdating = false;
        },
        error: (error) => {
          this.errorMessage = 'Failed to update comment. Please try again.';
          this.isUpdating = false;
          console.error('Error updating comment:', error);
        }
      });
    }
  }

  deleteComment(): void {
    if (confirm('Are you sure you want to delete this comment? This action cannot be undone.')) {
      this.postsService.deleteComment(this.comment.postId, this.comment.id).subscribe({
        next: () => {
          this.commentDeleted.emit(this.comment.id);
        },
        error: (error) => {
          this.errorMessage = 'Failed to delete comment. Please try again.';
          console.error('Error deleting comment:', error);
        }
      });
    }
  }

  get remainingChars(): number {
    const content = this.editForm.get('content')?.value || '';
    return 500 - content.length;
  }
}
