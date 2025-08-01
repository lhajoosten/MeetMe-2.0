import { Routes } from '@angular/router';

export const meetingsRoutes: Routes = [
  {
    path: '',
    loadComponent: () => import('./meetings-list/meetings-list.component').then(m => m.MeetingsListComponent)
  },
  {
    path: 'create',
    loadComponent: () => import('./create-meeting/create-meeting.component').then(m => m.CreateMeetingComponent)
  },
  {
    path: ':id',
    loadComponent: () => import('./meeting-detail/meeting-detail.component').then(m => m.MeetingDetailComponent)
  }
];
