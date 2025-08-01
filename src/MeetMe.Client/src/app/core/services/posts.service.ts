import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Post, Comment, CreatePostRequest, CreateCommentRequest } from '../../shared/models';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class PostsService {
  private readonly baseUrl = environment.apiUrl + '/posts';

  constructor(private http: HttpClient) {}

  getPostsByMeeting(meetingId: string): Observable<Post[]> {
    return this.http.get<Post[]>(`${environment.apiUrl}/meetings/${meetingId}/posts`);
  }

  createPost(request: CreatePostRequest): Observable<Post> {
    return this.http.post<Post>(this.baseUrl, request);
  }

  updatePost(id: string, content: string): Observable<Post> {
    return this.http.put<Post>(`${this.baseUrl}/${id}`, { content });
  }

  deletePost(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }

  createComment(request: CreateCommentRequest): Observable<Comment> {
    return this.http.post<Comment>(`${this.baseUrl}/${request.postId}/comments`, request);
  }

  updateComment(postId: string, commentId: string, content: string): Observable<Comment> {
    return this.http.put<Comment>(`${this.baseUrl}/${postId}/comments/${commentId}`, { content });
  }

  deleteComment(postId: string, commentId: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${postId}/comments/${commentId}`);
  }
}
