import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, BehaviorSubject } from 'rxjs';
import { tap } from 'rxjs/operators';
import { Post, Comment, CreatePostRequest, CreateCommentRequest } from '../../shared/models';
import { environment } from '../../../environments/environment';
import { NotificationService } from './notification.service';

export interface PostsFilter {
  meetingId?: string;
  authorId?: string;
  tags?: string[];
  search?: string;
  sortBy?: 'newest' | 'oldest' | 'most_liked' | 'most_commented';
  page?: number;
  pageSize?: number;
}

export interface PostsResponse {
  posts: Post[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

@Injectable({
  providedIn: 'root'
})
export class PostsService {
  private readonly baseUrl = environment.apiUrl + '/posts';
  private postsSubject = new BehaviorSubject<Post[]>([]);
  private loadingSubject = new BehaviorSubject<boolean>(false);

  constructor(
    private http: HttpClient,
    private notificationService: NotificationService
  ) {}

  get posts$(): Observable<Post[]> {
    return this.postsSubject.asObservable();
  }

  get loading$(): Observable<boolean> {
    return this.loadingSubject.asObservable();
  }

  /**
   * Get posts with filtering and pagination
   */
  getPosts(filters: PostsFilter = {}): Observable<PostsResponse> {
    this.loadingSubject.next(true);

    let params = new HttpParams();

    if (filters.meetingId) params = params.set('meetingId', filters.meetingId);
    if (filters.authorId) params = params.set('authorId', filters.authorId);
    if (filters.tags?.length) params = params.set('tags', filters.tags.join(','));
    if (filters.search) params = params.set('search', filters.search);
    if (filters.sortBy) params = params.set('sortBy', filters.sortBy);
    if (filters.page) params = params.set('page', filters.page.toString());
    if (filters.pageSize) params = params.set('pageSize', filters.pageSize.toString());

    return this.http.get<PostsResponse>(this.baseUrl, { params }).pipe(
      tap(response => {
        this.postsSubject.next(response.posts);
        this.loadingSubject.next(false);
      })
    );
  }

  getPostsByMeeting(meetingId: string): Observable<Post[]> {
    return this.http.get<Post[]>(`${this.baseUrl}/meeting/${meetingId}`);
  }

  /**
   * Get a single post by ID
   */
  getPost(id: number): Observable<Post> {
    return this.http.get<Post>(`${this.baseUrl}/${id}`);
  }

  createPost(request: CreatePostRequest): Observable<Post> {
    return this.http.post<Post>(this.baseUrl, request).pipe(
      tap(post => {
        this.notificationService.showSuccess('Success', 'Post created successfully');
        this.addPostToCache(post);
      })
    );
  }

  updatePost(id: number, content: string): Observable<Post> {
    return this.http.put<Post>(`${this.baseUrl}/${id}`, { content }).pipe(
      tap(post => {
        this.notificationService.showSuccess('Success', 'Post updated successfully');
        this.updatePostInCache(post);
      })
    );
  }

  deletePost(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`).pipe(
      tap(() => {
        this.notificationService.showSuccess('Success', 'Post deleted successfully');
        this.removePostFromCache(id);
      })
    );
  }

  /**
   * Like/unlike a post
   */
  toggleLike(postId: number): Observable<{ isLiked: boolean; likesCount: number }> {
    return this.http.post<{ isLiked: boolean; likesCount: number }>(`${this.baseUrl}/${postId}/like`, {}).pipe(
      tap(result => {
        this.updatePostLikeInCache(postId, result.isLiked, result.likesCount);
      })
    );
  }

  /**
   * Bookmark/unbookmark a post
   */
  toggleBookmark(postId: number): Observable<{ isBookmarked: boolean }> {
    return this.http.post<{ isBookmarked: boolean }>(`${this.baseUrl}/${postId}/bookmark`, {}).pipe(
      tap(result => {
        this.updatePostBookmarkInCache(postId, result.isBookmarked);
        const message = result.isBookmarked ? 'Post bookmarked' : 'Post removed from bookmarks';
        this.notificationService.showSuccess('Success', message);
      })
    );
  }

  createComment(request: CreateCommentRequest): Observable<Comment> {
    return this.http.post<Comment>(`${this.baseUrl}/${request.postId}/comments`, request);
  }

  updateComment(postId: number, commentId: number, content: string): Observable<Comment> {
    return this.http.put<Comment>(`${this.baseUrl}/${postId}/comments/${commentId}`, { content });
  }

  deleteComment(postId: number, commentId: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${postId}/comments/${commentId}`);
  }

  /**
   * Get trending posts
   */
  getTrendingPosts(limit: number = 10): Observable<Post[]> {
    return this.http.get<Post[]>(`${this.baseUrl}/trending`, {
      params: { limit: limit.toString() }
    });
  }

  /**
   * Get bookmarked posts for current user
   */
  getBookmarkedPosts(): Observable<Post[]> {
    return this.http.get<Post[]>(`${this.baseUrl}/bookmarked`);
  }

  // Cache management methods
  private addPostToCache(post: Post): void {
    const currentPosts = this.postsSubject.value;
    this.postsSubject.next([post, ...currentPosts]);
  }

  private updatePostInCache(updatedPost: Post): void {
    const currentPosts = this.postsSubject.value;
    const index = currentPosts.findIndex(p => p.id === updatedPost.id);
    if (index !== -1) {
      currentPosts[index] = updatedPost;
      this.postsSubject.next([...currentPosts]);
    }
  }

  private removePostFromCache(postId: number): void {
    const currentPosts = this.postsSubject.value;
    const filteredPosts = currentPosts.filter(p => p.id !== postId);
    this.postsSubject.next(filteredPosts);
  }

  private updatePostLikeInCache(postId: number, isLiked: boolean, likesCount: number): void {
    const currentPosts = this.postsSubject.value;
    const post = currentPosts.find(p => p.id === postId);
    if (post) {
      post.isLiked = isLiked;
      post.likes = likesCount;
      this.postsSubject.next([...currentPosts]);
    }
  }

  private updatePostBookmarkInCache(postId: number, isBookmarked: boolean): void {
    const currentPosts = this.postsSubject.value;
    const post = currentPosts.find(p => p.id === postId);
    if (post) {
      post.isBookmarked = isBookmarked;
      this.postsSubject.next([...currentPosts]);
    }
  }
}
