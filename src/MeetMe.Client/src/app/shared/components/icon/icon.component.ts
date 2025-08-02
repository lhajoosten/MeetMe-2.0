import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-icon',
  standalone: true,
  imports: [CommonModule],
  template: `
    <svg
      [attr.width]="size"
      [attr.height]="size"
      [attr.viewBox]="viewBox"
      [attr.fill]="fill"
      [class]="cssClass"
    >
      <ng-container [ngSwitch]="name">
        <!-- Search -->
        <path *ngSwitchCase="'search'" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" stroke="currentColor" stroke-width="2" fill="none" stroke-linecap="round" stroke-linejoin="round"/>

        <!-- Calendar -->
        <g *ngSwitchCase="'calendar'">
          <rect x="3" y="4" width="18" height="18" rx="2" ry="2" stroke="currentColor" stroke-width="2" fill="none"/>
          <line x1="16" y1="2" x2="16" y2="6" stroke="currentColor" stroke-width="2"/>
          <line x1="8" y1="2" x2="8" y2="6" stroke="currentColor" stroke-width="2"/>
          <line x1="3" y1="10" x2="21" y2="10" stroke="currentColor" stroke-width="2"/>
        </g>

        <!-- Plus -->
        <g *ngSwitchCase="'plus'">
          <line x1="12" y1="5" x2="12" y2="19" stroke="currentColor" stroke-width="2"/>
          <line x1="5" y1="12" x2="19" y2="12" stroke="currentColor" stroke-width="2"/>
        </g>

        <!-- User -->
        <g *ngSwitchCase="'user'">
          <path d="M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2" stroke="currentColor" stroke-width="2" fill="none"/>
          <circle cx="12" cy="7" r="4" stroke="currentColor" stroke-width="2" fill="none"/>
        </g>

        <!-- Users -->
        <g *ngSwitchCase="'users'">
          <path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2" stroke="currentColor" stroke-width="2" fill="none"/>
          <circle cx="9" cy="7" r="4" stroke="currentColor" stroke-width="2" fill="none"/>
          <path d="M23 21v-2a4 4 0 0 0-3-3.87" stroke="currentColor" stroke-width="2" fill="none"/>
          <path d="M16 3.13a4 4 0 0 1 0 7.75" stroke="currentColor" stroke-width="2" fill="none"/>
        </g>

        <!-- Location -->
        <g *ngSwitchCase="'location'">
          <path d="M21 10c0 7-9 13-9 13s-9-6-9-13a9 9 0 0 1 18 0z" stroke="currentColor" stroke-width="2" fill="none"/>
          <circle cx="12" cy="10" r="3" stroke="currentColor" stroke-width="2" fill="none"/>
        </g>

        <!-- Key -->
        <g *ngSwitchCase="'key'">
          <path d="M21 2l-2 2m-7.61 7.61a5.5 5.5 0 1 1-7.778 7.778 5.5 5.5 0 0 1 7.777-7.777zm0 0L15.5 7.5m0 0l3 3L22 7l-3-3m-3.5 3.5L19 4" stroke="currentColor" stroke-width="2" fill="none"/>
        </g>

        <!-- Logout -->
        <g *ngSwitchCase="'logout'">
          <path d="M9 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h4" stroke="currentColor" stroke-width="2" fill="none"/>
          <polyline points="16,17 21,12 16,7" stroke="currentColor" stroke-width="2" fill="none"/>
          <line x1="21" y1="12" x2="9" y2="12" stroke="currentColor" stroke-width="2"/>
        </g>

        <!-- Bell -->
        <g *ngSwitchCase="'bell'">
          <path d="M18 8A6 6 0 0 0 6 8c0 7-3 9-3 9h18s-3-2-3-9" stroke="currentColor" stroke-width="2" fill="none"/>
          <path d="M13.73 21a2 2 0 0 1-3.46 0" stroke="currentColor" stroke-width="2" fill="none"/>
        </g>

        <!-- Bell Off -->
        <g *ngSwitchCase="'bell-off'">
          <path d="M13.73 21a2 2 0 0 1-3.46 0" stroke="currentColor" stroke-width="2" fill="none"/>
          <path d="M18.63 13A17.89 17.89 0 0 1 18 8" stroke="currentColor" stroke-width="2" fill="none"/>
          <path d="M6.26 6.26A5.86 5.86 0 0 0 6 8c0 7-3 9-3 9h14" stroke="currentColor" stroke-width="2" fill="none"/>
          <path d="M18 8a6 6 0 0 0-9.33-5" stroke="currentColor" stroke-width="2" fill="none"/>
          <line x1="1" y1="1" x2="23" y2="23" stroke="currentColor" stroke-width="2"/>
        </g>

        <!-- Chevron Down -->
        <polyline *ngSwitchCase="'chevron-down'" points="6,9 12,15 18,9" stroke="currentColor" stroke-width="2" fill="none"/>

        <!-- X -->
        <g *ngSwitchCase="'x'">
          <line x1="18" y1="6" x2="6" y2="18" stroke="currentColor" stroke-width="2"/>
          <line x1="6" y1="6" x2="18" y2="18" stroke="currentColor" stroke-width="2"/>
        </g>

        <!-- Check Circle -->
        <g *ngSwitchCase="'check-circle'">
          <path d="M22 11.08V12a10 10 0 1 1-5.93-9.14" stroke="currentColor" stroke-width="2" fill="none"/>
          <polyline points="22,4 12,14.01 9,11.01" stroke="currentColor" stroke-width="2" fill="none"/>
        </g>

        <!-- X Circle -->
        <g *ngSwitchCase="'x-circle'">
          <circle cx="12" cy="12" r="10" stroke="currentColor" stroke-width="2" fill="none"/>
          <line x1="15" y1="9" x2="9" y2="15" stroke="currentColor" stroke-width="2"/>
          <line x1="9" y1="9" x2="15" y2="15" stroke="currentColor" stroke-width="2"/>
        </g>

        <!-- Alert Triangle -->
        <g *ngSwitchCase="'alert-triangle'">
          <path d="M10.29 3.86L1.82 18a2 2 0 0 0 1.71 3h16.94a2 2 0 0 0 1.71-3L13.71 3.86a2 2 0 0 0-3.42 0z" stroke="currentColor" stroke-width="2" fill="none"/>
          <line x1="12" y1="9" x2="12" y2="13" stroke="currentColor" stroke-width="2"/>
          <line x1="12" y1="17" x2="12.01" y2="17" stroke="currentColor" stroke-width="2"/>
        </g>

        <!-- Info Circle -->
        <g *ngSwitchCase="'info-circle'">
          <circle cx="12" cy="12" r="10" stroke="currentColor" stroke-width="2" fill="none"/>
          <line x1="12" y1="16" x2="12" y2="12" stroke="currentColor" stroke-width="2"/>
          <line x1="12" y1="8" x2="12.01" y2="8" stroke="currentColor" stroke-width="2"/>
        </g>

        <!-- Upload -->
        <g *ngSwitchCase="'upload'">
          <path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4" stroke="currentColor" stroke-width="2" fill="none"/>
          <polyline points="17,8 12,3 7,8" stroke="currentColor" stroke-width="2" fill="none"/>
          <line x1="12" y1="3" x2="12" y2="15" stroke="currentColor" stroke-width="2"/>
        </g>

        <!-- Camera -->
        <g *ngSwitchCase="'camera'">
          <path d="M23 19a2 2 0 0 1-2 2H3a2 2 0 0 1-2-2V8a2 2 0 0 1 2-2h4l2-3h6l2 3h4a2 2 0 0 1 2 2z" stroke="currentColor" stroke-width="2" fill="none"/>
          <circle cx="12" cy="13" r="4" stroke="currentColor" stroke-width="2" fill="none"/>
        </g>

        <!-- Download -->
        <g *ngSwitchCase="'download'">
          <path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4" stroke="currentColor" stroke-width="2" fill="none"/>
          <polyline points="7,10 12,15 17,10" stroke="currentColor" stroke-width="2" fill="none"/>
          <line x1="12" y1="15" x2="12" y2="3" stroke="currentColor" stroke-width="2"/>
        </g>

        <!-- File -->
        <g *ngSwitchCase="'file'">
          <path d="M14,2H6A2,2 0 0,0 4,4V20A2,2 0 0,0 6,22H18A2,2 0 0,0 20,20V8L14,2Z" stroke="currentColor" stroke-width="2" fill="none"/>
          <polyline points="14,2 14,8 20,8" stroke="currentColor" stroke-width="2" fill="none"/>
        </g>

        <!-- Heart -->
        <g *ngSwitchCase="'heart'">
          <path d="M20.84 4.61a5.5 5.5 0 0 0-7.78 0L12 5.67l-1.06-1.06a5.5 5.5 0 0 0-7.78 7.78l1.06 1.06L12 21.23l7.78-7.78 1.06-1.06a5.5 5.5 0 0 0 0-7.78z" stroke="currentColor" stroke-width="2" fill="none"/>
        </g>

        <!-- Star -->
        <polygon *ngSwitchCase="'star'" points="12,2 15.09,8.26 22,9.27 17,14.14 18.18,21.02 12,17.77 5.82,21.02 7,14.14 2,9.27 8.91,8.26" stroke="currentColor" stroke-width="2" fill="none"/>

        <!-- Message Circle -->
        <g *ngSwitchCase="'message-circle'">
          <path d="M21 11.5a8.38 8.38 0 0 1-.9 3.8 8.5 8.5 0 0 1-7.6 4.7 8.38 8.38 0 0 1-3.8-.9L3 21l1.9-5.7a8.38 8.38 0 0 1-.9-3.8 8.5 8.5 0 0 1 4.7-7.6 8.38 8.38 0 0 1 3.8-.9h.5a8.48 8.48 0 0 1 8 8v.5z" stroke="currentColor" stroke-width="2" fill="none"/>
        </g>

        <!-- Arrow Left -->
        <polyline *ngSwitchCase="'arrow-left'" points="15,18 9,12 15,6" stroke="currentColor" stroke-width="2" fill="none"/>

        <!-- Image -->
        <g *ngSwitchCase="'image'">
          <rect x="3" y="3" width="18" height="18" rx="2" ry="2" stroke="currentColor" stroke-width="2" fill="none"/>
          <circle cx="8.5" cy="8.5" r="1.5" stroke="currentColor" stroke-width="2" fill="none"/>
          <polyline points="21,15 16,10 5,21" stroke="currentColor" stroke-width="2" fill="none"/>
        </g>

        <!-- File Text -->
        <g *ngSwitchCase="'file-text'">
          <path d="M14,2H6A2,2 0 0,0 4,4V20A2,2 0 0,0 6,22H18A2,2 0 0,0 20,20V8L14,2Z" stroke="currentColor" stroke-width="2" fill="none"/>
          <polyline points="14,2 14,8 20,8" stroke="currentColor" stroke-width="2" fill="none"/>
          <line x1="16" y1="13" x2="8" y2="13" stroke="currentColor" stroke-width="2"/>
          <line x1="16" y1="17" x2="8" y2="17" stroke="currentColor" stroke-width="2"/>
          <polyline points="10,9 9,9 8,9" stroke="currentColor" stroke-width="2" fill="none"/>
        </g>

        <!-- Table -->
        <g *ngSwitchCase="'table'">
          <rect x="3" y="3" width="18" height="18" rx="2" ry="2" stroke="currentColor" stroke-width="2" fill="none"/>
          <line x1="9" y1="9" x2="21" y2="9" stroke="currentColor" stroke-width="2"/>
          <line x1="9" y1="15" x2="21" y2="15" stroke="currentColor" stroke-width="2"/>
          <line x1="9" y1="3" x2="9" y2="21" stroke="currentColor" stroke-width="2"/>
        </g>

        <!-- Presentation -->
        <g *ngSwitchCase="'presentation'">
          <rect x="2" y="4" width="20" height="14" rx="2" ry="2" stroke="currentColor" stroke-width="2" fill="none"/>
          <line x1="8" y1="21" x2="16" y2="21" stroke="currentColor" stroke-width="2"/>
          <line x1="12" y1="18" x2="12" y2="21" stroke="currentColor" stroke-width="2"/>
        </g>

        <!-- Video -->
        <g *ngSwitchCase="'video'">
          <rect x="2" y="4" width="20" height="14" rx="2" ry="2" stroke="currentColor" stroke-width="2" fill="none"/>
          <polygon points="10,8.5 16,11 10,13.5" stroke="currentColor" stroke-width="2" fill="none"/>
        </g>

        <!-- Music -->
        <g *ngSwitchCase="'music'">
          <path d="M9 18V5l12-2v13" stroke="currentColor" stroke-width="2" fill="none"/>
          <circle cx="6" cy="18" r="3" stroke="currentColor" stroke-width="2" fill="none"/>
          <circle cx="18" cy="16" r="3" stroke="currentColor" stroke-width="2" fill="none"/>
        </g>

        <!-- Archive -->
        <g *ngSwitchCase="'archive'">
          <rect x="2" y="3" width="20" height="5" rx="1" ry="1" stroke="currentColor" stroke-width="2" fill="none"/>
          <path d="M4 8v11a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8" stroke="currentColor" stroke-width="2" fill="none"/>
          <path d="M10 12h4" stroke="currentColor" stroke-width="2" fill="none"/>
        </g>

        <!-- Default fallback -->
        <circle *ngSwitchDefault cx="12" cy="12" r="3" fill="currentColor"/>
      </ng-container>
    </svg>
  `,
  styles: [`
    :host {
      display: inline-flex;
      align-items: center;
      justify-content: center;
    }
    svg {
      flex-shrink: 0;
    }
  `]
})
export class IconComponent {
  @Input() name: string = '';
  @Input() size: number = 20;
  @Input() fill: string = 'none';
  @Input() cssClass: string = '';

  get viewBox(): string {
    return '0 0 24 24';
  }
}
