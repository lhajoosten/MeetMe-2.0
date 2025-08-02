import { Component, Input, Output, EventEmitter, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FileService, FileUploadResponse, UploadProgress } from '../../../core/services/file.service';
import { IconComponent } from '../icon/icon.component';

@Component({
  selector: 'app-file-upload',
  standalone: true,
  imports: [CommonModule, IconComponent],
  templateUrl: './file-upload.component.html',
  styleUrl: './file-upload.component.scss'
})
export class FileUploadComponent implements OnInit {
  @Input() acceptedTypes: string = '*';
  @Input() maxFileSize: number = 10 * 1024 * 1024; // 10MB
  @Input() multiple: boolean = false;
  @Input() folder?: string;
  @Input() dragAndDrop: boolean = true;
  @Output() filesUploaded = new EventEmitter<FileUploadResponse[]>();
  @Output() uploadProgress = new EventEmitter<UploadProgress[]>();
  @Output() uploadError = new EventEmitter<string>();

  isDragOver = false;
  isUploading = false;
  uploadedFiles: FileUploadResponse[] = [];
  uploadProgresses: { [key: string]: UploadProgress } = {};

  constructor(private fileService: FileService) {}

  ngOnInit(): void {}

  onFileSelect(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files) {
      this.handleFiles(Array.from(input.files));
    }
  }

  onDragOver(event: DragEvent): void {
    if (!this.dragAndDrop) return;

    event.preventDefault();
    event.stopPropagation();
    this.isDragOver = true;
  }

  onDragLeave(event: DragEvent): void {
    if (!this.dragAndDrop) return;

    event.preventDefault();
    event.stopPropagation();
    this.isDragOver = false;
  }

  onDrop(event: DragEvent): void {
    if (!this.dragAndDrop) return;

    event.preventDefault();
    event.stopPropagation();
    this.isDragOver = false;

    if (event.dataTransfer?.files) {
      this.handleFiles(Array.from(event.dataTransfer.files));
    }
  }

  private handleFiles(files: File[]): void {
    const validFiles = files.filter(file => this.validateFile(file));

    if (validFiles.length === 0) {
      this.uploadError.emit('No valid files selected');
      return;
    }

    if (!this.multiple && validFiles.length > 1) {
      this.uploadError.emit('Only one file allowed');
      return;
    }

    this.uploadFiles(validFiles);
  }

  private validateFile(file: File): boolean {
    // Check file size
    if (file.size > this.maxFileSize) {
      this.uploadError.emit(`File "${file.name}" is too large. Maximum size is ${this.fileService.formatFileSize(this.maxFileSize)}`);
      return false;
    }

    // Check file type if specified
    if (this.acceptedTypes !== '*') {
      const acceptedTypes = this.acceptedTypes.split(',').map(type => type.trim());
      const isValid = acceptedTypes.some(type => {
        if (type.startsWith('.')) {
          return file.name.toLowerCase().endsWith(type.toLowerCase());
        }
        return file.type.match(type.replace('*', '.*'));
      });

      if (!isValid) {
        this.uploadError.emit(`File "${file.name}" is not an accepted file type`);
        return false;
      }
    }

    return true;
  }

  private uploadFiles(files: File[]): void {
    this.isUploading = true;
    this.uploadedFiles = [];
    this.uploadProgresses = {};

    files.forEach((file, index) => {
      const fileKey = `${file.name}_${index}`;

      this.fileService.uploadFile(file, this.folder).subscribe({
        next: (result) => {
          if ('progress' in result) {
            // Progress update
            this.uploadProgresses[fileKey] = result;
            this.uploadProgress.emit(Object.values(this.uploadProgresses));
          } else {
            // Upload complete
            this.uploadedFiles.push(result);
            delete this.uploadProgresses[fileKey];

            // Check if all uploads are complete
            if (this.uploadedFiles.length === files.length) {
              this.isUploading = false;
              this.filesUploaded.emit(this.uploadedFiles);
            }
          }
        },
        error: (error) => {
          this.isUploading = false;
          this.uploadError.emit(`Failed to upload "${file.name}": ${error.message}`);
        }
      });
    });
  }

  removeFile(index: number): void {
    this.uploadedFiles.splice(index, 1);
    this.filesUploaded.emit(this.uploadedFiles);
  }

  getFileIcon(file: FileUploadResponse): string {
    return this.fileService.getFileIcon(file);
  }

  formatFileSize(bytes: number): string {
    return this.fileService.formatFileSize(bytes);
  }
}
