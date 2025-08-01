import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-meeting-detail',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="container">
      <h1>Meeting Details</h1>
      <p>Meeting detail functionality coming soon!</p>
    </div>
  `,
  styles: [`
    .container {
      max-width: 800px;
      margin: 0 auto;
      padding: 20px;
    }
  `]
})
export class MeetingDetailComponent {
}
