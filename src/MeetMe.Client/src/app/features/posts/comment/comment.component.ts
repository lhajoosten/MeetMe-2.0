import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Comment } from '../../../shared/models';

@Component({
  selector: 'app-comment',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './comment.component.html',
  styleUrl: './comment.component.scss'
})
export class CommentComponent {
  @Input() comment!: Comment;
  @Input() canDelete = false;
  @Output() deleteComment = new EventEmitter<number>();

  onDelete(): void {
    if (confirm('Are you sure you want to delete this comment?')) {
      this.deleteComment.emit(this.comment.id);
    }
  }
}
