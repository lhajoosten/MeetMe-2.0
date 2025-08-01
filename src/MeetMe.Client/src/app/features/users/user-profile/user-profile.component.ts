import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { User } from '../../../shared/models';

@Component({
  selector: 'app-user-profile',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  templateUrl: './user-profile.component.html',
  styleUrl: './user-profile.component.scss'
})
export class UserProfileComponent implements OnInit {
  profileForm: FormGroup;
  user: User | null = null;
  isLoading = false;
  errorMessage = '';
  successMessage = '';

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router
  ) {
    this.profileForm = this.fb.group({
      firstName: ['', [Validators.required]],
      lastName: ['', [Validators.required]],
      email: ['', [Validators.required, Validators.email]],
      phoneNumber: [''],
      bio: ['']
    });
  }

  ngOnInit(): void {
    this.user = this.authService.getCurrentUser();
    if (this.user) {
      this.profileForm.patchValue({
        firstName: this.user.firstName,
        lastName: this.user.lastName,
        email: this.user.email,
        phoneNumber: this.user.phoneNumber || '',
        bio: this.user.bio || ''
      });
    }
  }

  onSubmit(): void {
    if (this.profileForm.valid) {
      this.isLoading = true;
      this.errorMessage = '';
      this.successMessage = '';

      // TODO: Implement user update service
      // this.userService.updateProfile(this.profileForm.value).subscribe({
      //   next: (user) => {
      //     this.user = user;
      //     this.successMessage = 'Profile updated successfully!';
      //     this.isLoading = false;
      //   },
      //   error: (error) => {
      //     this.errorMessage = error.error?.message || 'Failed to update profile';
      //     this.isLoading = false;
      //   }
      // });

      // Temporary simulation
      setTimeout(() => {
        this.successMessage = 'Profile updated successfully!';
        this.isLoading = false;
      }, 1000);
    }
  }

  resetForm(): void {
    if (this.user) {
      this.profileForm.patchValue({
        firstName: this.user.firstName,
        lastName: this.user.lastName,
        email: this.user.email,
        phoneNumber: this.user.phoneNumber || '',
        bio: this.user.bio || ''
      });
    }
    this.errorMessage = '';
    this.successMessage = '';
  }

  uploadAvatar(): void {
    // TODO: Implement avatar upload
    console.log('Avatar upload functionality to be implemented');
  }

  changePassword(): void {
    // TODO: Navigate to change password component
    this.router.navigate(['/auth/change-password']);
  }

  downloadData(): void {
    // TODO: Implement data download
    console.log('Data download functionality to be implemented');
  }

  deleteAccount(): void {
    if (confirm('Are you sure you want to delete your account? This action cannot be undone.')) {
      // TODO: Implement account deletion
      console.log('Account deletion functionality to be implemented');
    }
  }
}
