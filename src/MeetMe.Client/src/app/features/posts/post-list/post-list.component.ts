import { Component, Input, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { PostsService } from '../../../core/services/posts.service';
import { Post } from '../../../shared/models';
import { CreatePostComponent } from '../create-post/create-post.component';

@Component({
  selector: 'app-post-list',
  standalone: true,
  imports: [CommonModule, RouterModule, CreatePostComponent],
  templateUrl: './post-list.component.html',
  styleUrl: './post-list.component.scss'
})
export class PostListComponent implements OnInit {
  @Input() meetingId!: string;
  posts: Post[] = [];
  isLoading = false;
  errorMessage = '';

  constructor(private postsService: PostsService) {}

  ngOnInit(): void {
    if (this.meetingId) {
      this.loadPosts();
    }
  }

  loadPosts(): void {
    this.isLoading = true;
    this.postsService.getPostsByMeeting(this.meetingId).subscribe({
      next: (posts) => {
        this.posts = posts;
        this.isLoading = false;
      },
      error: (error) => {
        this.errorMessage = 'Failed to load posts';
        this.isLoading = false;
        console.error('Error loading posts:', error);
      }
    });
  }

  onPostCreated(post: Post): void {
    this.posts.unshift(post);
  }

  onPostDeleted(postId: number): void {
    this.postsService.deletePost(postId).subscribe({
      next: () => {
        this.posts = this.posts.filter(p => p.id !== postId);
      },
      error: (error) => {
        console.error('Error deleting post:', error);
      }
    });
  }

  trackByPostId(index: number, post: Post): number {
    return post.id;
  }
}
