import { Routes } from '@angular/router';
import { PostListComponent } from './post-list/post-list.component';

export const postsRoutes: Routes = [
  {
    path: '',
    component: PostListComponent
  }
];
