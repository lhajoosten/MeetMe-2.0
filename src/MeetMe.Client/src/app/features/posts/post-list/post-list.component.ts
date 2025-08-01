import { Component, Input, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { PostsService } from '../../../core/services/posts.service';
import { Post } from '../../../shared/models';
import { PostDetailComponent } from '../post-detail/post-detail.component';
import { CreatePostComponent } from '../create-post/create-post.component';

@Component({
  selector: 'app-post-list',
  standalone: true,
  imports: [CommonModule, RouterModule, PostDetailComponent, CreatePostComponent],
  templateUrl: './post-list.component.html',
  styleUrl: './post-list.component.scss'
})
export class PostListComponent implements OnInit {
  @Input() meetingId!: string;
  posts: Post[] = [];
  isLoading = false;
  errorMessage = '';
  showCreatePost = false;

  constructor(private postsService: PostsService) {}

  ngOnInit(): void {
    if (this.meetingId) {
      this.loadPosts();
    }
  }

  loadPosts(): void {
    this.isLoading = true;
    this.errorMessage = '';

    this.postsService.getPostsByMeeting(this.meetingId).subscribe({
      next: (posts) => {
        this.posts = posts.sort((a, b) =>
          new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime()
        );
        this.isLoading = false;
      },
      error: (error) => {
        this.errorMessage = 'Failed to load posts. Please try again.';
        this.isLoading = false;
        console.error('Error loading posts:', error);
      }
    });
  }

  onPostCreated(newPost: Post): void {
    this.posts.unshift(newPost);
    this.showCreatePost = false;
  }

  onPostUpdated(updatedPost: Post): void {
    const index = this.posts.findIndex(p => p.id === updatedPost.id);
    if (index !== -1) {
      this.posts[index] = updatedPost;
    }
  }

  onPostDeleted(postId: string): void {
    this.posts = this.posts.filter(p => p.id !== postId);
  }

  toggleCreatePost(): void {
    this.showCreatePost = !this.showCreatePost;
  }

  trackByPostId(index: number, post: Post): string {
    return post.id;
  }
}
