import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { UsersService } from '../../../core/services/users.service';
import { User, UpdateUserRequest } from '../../../shared/models';
import { IconComponent } from '../../../shared/components/icon/icon.component';

@Component({
  selector: 'app-user-profile',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule, IconComponent],
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
    private usersService: UsersService,
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
    if (this.profileForm.valid && this.user) {
      this.isLoading = true;
      this.errorMessage = '';
      this.successMessage = '';

      const updateRequest: UpdateUserRequest = {
        firstName: this.profileForm.value.firstName,
        lastName: this.profileForm.value.lastName,
        email: this.profileForm.value.email,
        phoneNumber: this.profileForm.value.phoneNumber,
        bio: this.profileForm.value.bio
      };

      this.usersService.updateProfile(this.user.id, updateRequest).subscribe({
        next: (user) => {
          this.user = user;
          // Update the user in auth service
          this.authService.updateCurrentUser(user);
          this.successMessage = 'Profile updated successfully!';
          this.isLoading = false;
        },
        error: (error) => {
          this.errorMessage = error.error?.message || 'Failed to update profile';
          this.isLoading = false;
        }
      });
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
    const input = document.createElement('input');
    input.type = 'file';
    input.accept = 'image/*';
    input.onchange = (event: any) => {
      const file = event.target.files[0];
      if (file && this.user) {
        this.isLoading = true;
        this.usersService.uploadProfilePicture(this.user.id, file).subscribe({
          next: (user) => {
            this.user = user;
            this.authService.updateCurrentUser(user);
            this.successMessage = 'Profile picture updated successfully!';
            this.isLoading = false;
          },
          error: (error) => {
            this.errorMessage = error.error?.message || 'Failed to upload profile picture';
            this.isLoading = false;
          }
        });
      }
    };
    input.click();
  }

  changePassword(): void {
    this.router.navigate(['/users/change-password']);
  }

  downloadData(): void {
    if (this.user) {
      this.usersService.exportUserData(this.user.id).subscribe({
        next: (blob) => {
          const url = window.URL.createObjectURL(blob);
          const a = document.createElement('a');
          a.href = url;
          a.download = `${this.user?.firstName}_${this.user?.lastName}_data.json`;
          document.body.appendChild(a);
          a.click();
          document.body.removeChild(a);
          window.URL.revokeObjectURL(url);
        },
        error: (error) => {
          this.errorMessage = error.error?.message || 'Failed to download data';
        }
      });
    }
  }

  deleteAccount(): void {
    if (confirm('Are you sure you want to delete your account? This action cannot be undone.')) {
      if (this.user) {
        this.isLoading = true;
        this.usersService.deleteAccount(this.user.id).subscribe({
          next: () => {
            this.authService.logout();
            this.router.navigate(['/auth/login']);
          },
          error: (error) => {
            this.errorMessage = error.error?.message || 'Failed to delete account';
            this.isLoading = false;
          }
        });
      }
    }
  }
}
