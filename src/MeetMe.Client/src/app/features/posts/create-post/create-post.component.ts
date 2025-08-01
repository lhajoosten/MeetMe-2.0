import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { PostsService } from '../../../core/services/posts.service';
import { Post, CreatePostRequest } from '../../../shared/models';

@Component({
  selector: 'app-create-post',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './create-post.component.html',
  styleUrl: './create-post.component.scss'
})
export class CreatePostComponent {
  @Input() meetingId!: string;
  @Output() postCreated = new EventEmitter<Post>();
  @Output() cancelled = new EventEmitter<void>();

  postForm: FormGroup;
  isSubmitting = false;
  errorMessage = '';

  constructor(
    private fb: FormBuilder,
    private postsService: PostsService
  ) {
    this.postForm = this.fb.group({
      content: ['', [Validators.required, Validators.minLength(1), Validators.maxLength(1000)]]
    });
  }

  onSubmit(): void {
    if (this.postForm.valid && !this.isSubmitting) {
      this.isSubmitting = true;
      this.errorMessage = '';

      const request: CreatePostRequest = {
        content: this.postForm.value.content.trim(),
        meetingId: this.meetingId
      };

      this.postsService.createPost(request).subscribe({
        next: (post) => {
          this.postCreated.emit(post);
          this.postForm.reset();
          this.isSubmitting = false;
        },
        error: (error) => {
          this.errorMessage = 'Failed to create post. Please try again.';
          this.isSubmitting = false;
          console.error('Error creating post:', error);
        }
      });
    }
  }

  onCancel(): void {
    this.postForm.reset();
    this.errorMessage = '';
    this.cancelled.emit();
  }

  get contentControl() {
    return this.postForm.get('content');
  }

  get remainingChars(): number {
    const content = this.contentControl?.value || '';
    return 1000 - content.length;
  }
}
