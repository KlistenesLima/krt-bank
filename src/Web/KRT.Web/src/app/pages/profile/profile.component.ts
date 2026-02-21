import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './profile.component.html',
  styleUrls: ['./profile.component.scss']
})
export class ProfileComponent implements OnInit {
  accountId = '';
  profile: any = null;
  activities: any[] = [];
  loading = true;
  activeSection: 'personal' | 'preferences' | 'security' | 'activity' = 'personal';
  saving = false;

  constructor(private http: HttpClient) {}

  ngOnInit(): void {
    this.accountId = localStorage.getItem('account_id') || '00000000-0000-0000-0000-000000000001';
    this.loadProfile();
    this.loadActivity();
  }

  loadProfile(): void {
    this.http.get<any>(`${environment.apiUrl}/profile/${this.accountId}`).subscribe({
      next: (data) => { this.profile = data; this.loading = false; },
      error: () => this.loading = false
    });
  }

  loadActivity(): void {
    this.http.get<any[]>(`${environment.apiUrl}/profile/${this.accountId}/activity`).subscribe(data => this.activities = data);
  }

  saveProfile(): void {
    this.saving = true;
    this.http.put(`${environment.apiUrl}/profile/${this.accountId}`, {
      name: this.profile.name, phone: this.profile.phone, address: this.profile.address
    }).subscribe({ next: () => this.saving = false, error: () => this.saving = false });
  }

  savePreferences(): void {
    this.saving = true;
    this.http.put(`${environment.apiUrl}/profile/${this.accountId}/preferences`, this.profile.preferences)
      .subscribe({ next: () => this.saving = false, error: () => this.saving = false });
  }

  saveSecurity(): void {
    this.saving = true;
    this.http.put(`${environment.apiUrl}/profile/${this.accountId}/security`, this.profile.security)
      .subscribe({ next: () => this.saving = false, error: () => this.saving = false });
  }

  getInitials(): string {
    if (!this.profile?.name) return '?';
    return this.profile.name.split(' ').filter((w: string) => w).slice(0, 2).map((w: string) => w[0]).join('').toUpperCase();
  }
}