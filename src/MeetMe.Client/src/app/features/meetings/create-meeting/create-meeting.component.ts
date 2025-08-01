import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-create-meeting',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="container">
      <h1>Create Meeting</h1>
      <p>Create meeting functionality coming soon!</p>
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
export class CreateMeetingComponent {
}
