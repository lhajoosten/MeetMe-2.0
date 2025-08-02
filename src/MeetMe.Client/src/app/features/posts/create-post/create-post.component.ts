import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { PostsService } from '../../../core/services/posts.service';
import { AuthService } from '../../../core/services/auth.service';
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

  postForm: FormGroup;
  isLoading = false;
  errorMessage = '';

  constructor(
    private fb: FormBuilder,
    private postsService: PostsService,
    private authService: AuthService
  ) {
    this.postForm = this.fb.group({
      content: ['', [Validators.required, Validators.minLength(1)]]
    });
  }

  onSubmit(): void {
    if (this.postForm.valid && this.meetingId) {
      this.isLoading = true;
      this.errorMessage = '';

      const request: CreatePostRequest = {
        content: this.postForm.value.content,
        meetingId: this.meetingId
      };

      this.postsService.createPost(request).subscribe({
        next: (post) => {
          this.postCreated.emit(post);
          this.postForm.reset();
          this.postForm.markAsUntouched();
          this.postForm.markAsPristine();
          this.isLoading = false;
        },
        error: (error) => {
          this.errorMessage = 'Failed to create post';
          this.isLoading = false;
          console.error('Error creating post:', error);
        }
      });
    }
  }
}
