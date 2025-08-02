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
    path: ':id/edit',
    loadComponent: () => import('./edit-meeting/edit-meeting.component').then(m => m.EditMeetingComponent)
  },
  {
    path: ':id',
    loadComponent: () => import('./meeting-detail/meeting-detail.component').then(m => m.MeetingDetailComponent)
  }
];
