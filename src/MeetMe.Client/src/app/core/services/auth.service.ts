import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, tap } from 'rxjs';
import {
	LoginRequest,
	RegisterRequest,
	AuthResponse,
	User,
} from '../../shared/models';
import { environment } from '../../../environments/environment';

@Injectable({
	providedIn: 'root',
})
export class AuthService {
	private readonly TOKEN_KEY = 'meetme_token';
	private readonly USER_KEY = 'meetme_user';
	private readonly baseUrl = environment.apiUrl + '/auth';

	private currentUserSubject = new BehaviorSubject<User | null>(
		this.getUserFromStorage()
	);
	public currentUser$ = this.currentUserSubject.asObservable();

	private isAuthenticatedSubject = new BehaviorSubject<boolean>(
		this.hasValidToken()
	);
	public isAuthenticated$ = this.isAuthenticatedSubject.asObservable();

	constructor(private http: HttpClient) {}

	login(request: LoginRequest): Observable<AuthResponse> {
		return this.http
			.post<AuthResponse>(`${this.baseUrl}/login`, request)
			.pipe(tap((response) => this.handleAuthSuccess(response)));
	}

	register(request: RegisterRequest): Observable<AuthResponse> {
		return this.http
			.post<AuthResponse>(`${this.baseUrl}/register`, request)
			.pipe(tap((response) => this.handleAuthSuccess(response)));
	}

	logout(): void {
		localStorage.removeItem(this.TOKEN_KEY);
		localStorage.removeItem(this.USER_KEY);
		this.currentUserSubject.next(null);
		this.isAuthenticatedSubject.next(false);
	}

	getToken(): string | null {
		return localStorage.getItem(this.TOKEN_KEY);
	}

	getCurrentUser(): User | null {
		return this.currentUserSubject.value;
	}

	isAuthenticated(): boolean {
		return this.isAuthenticatedSubject.value;
	}

	updateCurrentUser(user: User): void {
		localStorage.setItem(this.USER_KEY, JSON.stringify(user));
		this.currentUserSubject.next(user);
	}

	private handleAuthSuccess(response: AuthResponse): void {
		localStorage.setItem(this.TOKEN_KEY, response.token);
		localStorage.setItem(this.USER_KEY, JSON.stringify(response.user));
		this.currentUserSubject.next(response.user);
		this.isAuthenticatedSubject.next(true);
	}

	private getUserFromStorage(): User | null {
		const userJson = localStorage.getItem(this.USER_KEY);
		return userJson ? JSON.parse(userJson) : null;
	}

	private hasValidToken(): boolean {
		const token = this.getToken();
		if (!token) return false;

		try {
			// Check if token is expired (basic JWT check)
			const payload = JSON.parse(atob(token.split('.')[1]));
			return payload.exp * 1000 > Date.now();
		} catch {
			return false;
		}
	}
}
