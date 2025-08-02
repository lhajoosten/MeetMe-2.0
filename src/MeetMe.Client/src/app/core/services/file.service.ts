import { Injectable } from '@angular/core';
import { HttpClient, HttpEvent, HttpEventType, HttpProgressEvent, HttpResponse } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface FileUploadResponse {
  id: string;
  fileName: string;
  originalName: string;
  fileSize: number;
  contentType: string;
  uploadedAt: Date;
  downloadUrl: string;
}

export interface UploadProgress {
  progress: number;
  loaded: number;
  total: number;
}

@Injectable({
  providedIn: 'root'
})
export class FileService {
  private readonly baseUrl = environment.apiUrl + '/files';

  constructor(private http: HttpClient) {}

  uploadFile(file: File, folder?: string): Observable<FileUploadResponse | UploadProgress> {
    const formData = new FormData();
    formData.append('file', file);
    if (folder) {
      formData.append('folder', folder);
    }

    return this.http.post<FileUploadResponse>(this.baseUrl + '/upload', formData, {
      reportProgress: true,
      observe: 'events'
    }).pipe(
      map(event => this.handleUploadEvent(event))
    );
  }

  uploadMultipleFiles(files: File[], folder?: string): Observable<(FileUploadResponse | UploadProgress)[]> {
    const uploads = files.map(file => this.uploadFile(file, folder));
    return new Observable(observer => {
      const results: (FileUploadResponse | UploadProgress)[] = [];
      let completed = 0;

      uploads.forEach((upload, index) => {
        upload.subscribe({
          next: (result) => {
            results[index] = result;
            observer.next([...results]);
          },
          complete: () => {
            completed++;
            if (completed === files.length) {
              observer.complete();
            }
          },
          error: (error) => observer.error(error)
        });
      });
    });
  }

  downloadFile(fileId: string): Observable<Blob> {
    return this.http.get(`${this.baseUrl}/${fileId}/download`, {
      responseType: 'blob'
    });
  }

  deleteFile(fileId: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${fileId}`);
  }

  getFileInfo(fileId: string): Observable<FileUploadResponse> {
    return this.http.get<FileUploadResponse>(`${this.baseUrl}/${fileId}`);
  }

  getMeetingFiles(meetingId: string): Observable<FileUploadResponse[]> {
    return this.http.get<FileUploadResponse[]>(`${this.baseUrl}/meeting/${meetingId}`);
  }

  getPostFiles(postId: string): Observable<FileUploadResponse[]> {
    return this.http.get<FileUploadResponse[]>(`${this.baseUrl}/post/${postId}`);
  }

  private handleUploadEvent(event: HttpEvent<FileUploadResponse>): FileUploadResponse | UploadProgress {
    switch (event.type) {
      case HttpEventType.UploadProgress:
        if (event.total) {
          const progress = Math.round(100 * event.loaded / event.total);
          return {
            progress,
            loaded: event.loaded,
            total: event.total
          };
        }
        return { progress: 0, loaded: 0, total: 0 };

      case HttpEventType.Response:
        return event.body!;

      default:
        return { progress: 0, loaded: 0, total: 0 };
    }
  }

  // Utility methods for file validation
  isImageFile(file: File): boolean {
    return file.type.startsWith('image/');
  }

  isDocumentFile(file: File): boolean {
    const documentTypes = [
      'application/pdf',
      'application/msword',
      'application/vnd.openxmlformats-officedocument.wordprocessingml.document',
      'application/vnd.ms-excel',
      'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
      'text/plain'
    ];
    return documentTypes.includes(file.type);
  }

  formatFileSize(bytes: number): string {
    if (bytes === 0) return '0 Bytes';
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
  }

  getFileIcon(file: File | FileUploadResponse): string {
    const type = 'contentType' in file ? file.contentType : file.type;

    if (type.startsWith('image/')) return 'image';
    if (type === 'application/pdf') return 'file-text';
    if (type.includes('word')) return 'file-text';
    if (type.includes('excel') || type.includes('spreadsheet')) return 'table';
    if (type.includes('powerpoint') || type.includes('presentation')) return 'presentation';
    if (type.startsWith('video/')) return 'video';
    if (type.startsWith('audio/')) return 'music';
    if (type.includes('zip') || type.includes('rar') || type.includes('archive')) return 'archive';

    return 'file';
  }
}
