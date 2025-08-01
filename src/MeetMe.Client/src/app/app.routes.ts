import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';

export const routes: Routes = [
	{
		path: '',
		redirectTo: '/meetings',
		pathMatch: 'full',
	},
	{
		path: 'auth',
		loadChildren: () =>
			import('./features/auth/auth.routes').then((m) => m.authRoutes),
	},
	{
		path: 'meetings',
		loadChildren: () =>
			import('./features/meetings/meetings.routes').then(
				(m) => m.meetingsRoutes
			),
		canActivate: [authGuard],
	},
	{
		path: 'posts',
		loadChildren: () =>
			import('./features/posts/posts.routes').then((m) => m.postsRoutes),
		canActivate: [authGuard],
	},
	{
		path: 'users',
		loadChildren: () =>
			import('./features/users/users.routes').then((m) => m.usersRoutes),
		canActivate: [authGuard],
	},
	{
		path: 'search',
		loadChildren: () =>
			import('./features/search/search.routes').then((m) => m.searchRoutes),
		canActivate: [authGuard],
	},
	{
		path: '**',
		redirectTo: '/meetings',
	},
];
