import { Component } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  template: `
    <div class="auth-container">
      <div class="auth-card">
        <h2>Join MeetMe</h2>
        <p class="subtitle">Create your account to start connecting</p>

        <form [formGroup]="registerForm" (ngSubmit)="onSubmit()">
          <div class="form-row">
            <div class="form-group">
              <label for="firstName">First Name</label>
              <input
                id="firstName"
                type="text"
                formControlName="firstName"
                class="form-control"
                [class.error]="registerForm.get('firstName')?.invalid && registerForm.get('firstName')?.touched"
                placeholder="First name"
              />
              <div class="error-message" *ngIf="registerForm.get('firstName')?.invalid && registerForm.get('firstName')?.touched">
                <small *ngIf="registerForm.get('firstName')?.errors?.['required']">First name is required</small>
              </div>
            </div>

            <div class="form-group">
              <label for="lastName">Last Name</label>
              <input
                id="lastName"
                type="text"
                formControlName="lastName"
                class="form-control"
                [class.error]="registerForm.get('lastName')?.invalid && registerForm.get('lastName')?.touched"
                placeholder="Last name"
              />
              <div class="error-message" *ngIf="registerForm.get('lastName')?.invalid && registerForm.get('lastName')?.touched">
                <small *ngIf="registerForm.get('lastName')?.errors?.['required']">Last name is required</small>
              </div>
            </div>
          </div>

          <div class="form-group">
            <label for="email">Email</label>
            <input
              id="email"
              type="email"
              formControlName="email"
              class="form-control"
              [class.error]="registerForm.get('email')?.invalid && registerForm.get('email')?.touched"
              placeholder="Enter your email"
            />
            <div class="error-message" *ngIf="registerForm.get('email')?.invalid && registerForm.get('email')?.touched">
              <small *ngIf="registerForm.get('email')?.errors?.['required']">Email is required</small>
              <small *ngIf="registerForm.get('email')?.errors?.['email']">Please enter a valid email</small>
            </div>
          </div>

          <div class="form-group">
            <label for="password">Password</label>
            <input
              id="password"
              type="password"
              formControlName="password"
              class="form-control"
              [class.error]="registerForm.get('password')?.invalid && registerForm.get('password')?.touched"
              placeholder="Create a password"
            />
            <div class="error-message" *ngIf="registerForm.get('password')?.invalid && registerForm.get('password')?.touched">
              <small *ngIf="registerForm.get('password')?.errors?.['required']">Password is required</small>
              <small *ngIf="registerForm.get('password')?.errors?.['minlength']">Password must be at least 6 characters</small>
            </div>
          </div>

          <div class="form-group">
            <label for="confirmPassword">Confirm Password</label>
            <input
              id="confirmPassword"
              type="password"
              formControlName="confirmPassword"
              class="form-control"
              [class.error]="registerForm.get('confirmPassword')?.invalid && registerForm.get('confirmPassword')?.touched"
              placeholder="Confirm your password"
            />
            <div class="error-message" *ngIf="registerForm.get('confirmPassword')?.invalid && registerForm.get('confirmPassword')?.touched">
              <small *ngIf="registerForm.get('confirmPassword')?.errors?.['required']">Please confirm your password</small>
              <small *ngIf="registerForm.get('confirmPassword')?.errors?.['passwordMismatch']">Passwords do not match</small>
            </div>
          </div>

          <div class="form-group">
            <label for="phoneNumber">Phone Number (Optional)</label>
            <input
              id="phoneNumber"
              type="tel"
              formControlName="phoneNumber"
              class="form-control"
              placeholder="Enter your phone number"
            />
          </div>

          <div class="form-group">
            <label for="bio">Bio (Optional)</label>
            <textarea
              id="bio"
              formControlName="bio"
              class="form-control"
              rows="3"
              placeholder="Tell us about yourself"
            ></textarea>
          </div>

          <button
            type="submit"
            class="btn btn-primary"
            [disabled]="registerForm.invalid || isLoading"
          >
            {{ isLoading ? 'Creating Account...' : 'Create Account' }}
          </button>

          <div class="error-message" *ngIf="errorMessage">
            <small>{{ errorMessage }}</small>
          </div>
        </form>

        <div class="auth-footer">
          <p>Already have an account? <a routerLink="/auth/login">Sign in</a></p>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .auth-container {
      min-height: 100vh;
      display: flex;
      align-items: center;
      justify-content: center;
      background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
      padding: 20px;
    }

    .auth-card {
      background: white;
      padding: 40px;
      border-radius: 12px;
      box-shadow: 0 20px 40px rgba(0, 0, 0, 0.1);
      width: 100%;
      max-width: 500px;
    }

    h2 {
      text-align: center;
      margin-bottom: 8px;
      color: #333;
      font-weight: 600;
    }

    .subtitle {
      text-align: center;
      color: #666;
      margin-bottom: 32px;
    }

    .form-row {
      display: grid;
      grid-template-columns: 1fr 1fr;
      gap: 16px;
    }

    .form-group {
      margin-bottom: 20px;
    }

    label {
      display: block;
      margin-bottom: 8px;
      color: #333;
      font-weight: 500;
    }

    .form-control {
      width: 100%;
      padding: 12px 16px;
      border: 2px solid #e1e5e9;
      border-radius: 8px;
      font-size: 16px;
      transition: border-color 0.3s ease;
      box-sizing: border-box;
      resize: vertical;
    }

    .form-control:focus {
      outline: none;
      border-color: #667eea;
    }

    .form-control.error {
      border-color: #e74c3c;
    }

    .btn {
      width: 100%;
      padding: 12px;
      border: none;
      border-radius: 8px;
      font-size: 16px;
      font-weight: 600;
      cursor: pointer;
      transition: background-color 0.3s ease;
    }

    .btn-primary {
      background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
      color: white;
    }

    .btn-primary:hover:not(:disabled) {
      background: linear-gradient(135deg, #5a6fd8 0%, #6a4190 100%);
    }

    .btn:disabled {
      opacity: 0.6;
      cursor: not-allowed;
    }

    .error-message {
      margin-top: 8px;
    }

    .error-message small {
      color: #e74c3c;
      font-size: 14px;
    }

    .auth-footer {
      text-align: center;
      margin-top: 24px;
      padding-top: 24px;
      border-top: 1px solid #e1e5e9;
    }

    .auth-footer p {
      margin: 0;
      color: #666;
    }

    .auth-footer a {
      color: #667eea;
      text-decoration: none;
      font-weight: 600;
    }

    .auth-footer a:hover {
      text-decoration: underline;
    }
  `]
})
export class RegisterComponent {
  registerForm: FormGroup;
  isLoading = false;
  errorMessage = '';

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router
  ) {
    this.registerForm = this.fb.group({
      firstName: ['', [Validators.required]],
      lastName: ['', [Validators.required]],
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]],
      confirmPassword: ['', [Validators.required]],
      phoneNumber: [''],
      bio: ['']
    }, { validators: this.passwordMatchValidator });
  }

  passwordMatchValidator(form: any) {
    const password = form.get('password');
    const confirmPassword = form.get('confirmPassword');

    if (password && confirmPassword && password.value !== confirmPassword.value) {
      confirmPassword.setErrors({ passwordMismatch: true });
      return { passwordMismatch: true };
    }

    if (confirmPassword?.errors?.['passwordMismatch']) {
      delete confirmPassword.errors['passwordMismatch'];
      if (Object.keys(confirmPassword.errors).length === 0) {
        confirmPassword.setErrors(null);
      }
    }

    return null;
  }

  onSubmit(): void {
    if (this.registerForm.valid) {
      this.isLoading = true;
      this.errorMessage = '';

      this.authService.register(this.registerForm.value).subscribe({
        next: () => {
          this.router.navigate(['/meetings']);
        },
        error: (error: any) => {
          this.isLoading = false;
          this.errorMessage = error.error?.message || 'Registration failed. Please try again.';
        }
      });
    }
  }
}
