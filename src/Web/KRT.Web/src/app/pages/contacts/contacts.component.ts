import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';

@Component({
  selector: 'app-contacts',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './contacts.component.html',
  styleUrls: ['./contacts.component.scss']
})
export class ContactsComponent implements OnInit {
  accountId = '';
  contacts: any[] = [];
  loading = false;
  showForm = false;
  searchTerm = '';
  favoritesOnly = false;

  form = { name: '', pixKey: '', pixKeyType: 'CPF', bankName: '', nickname: '' };
  keyTypes = ['CPF', 'CNPJ', 'EMAIL', 'PHONE', 'EVP'];

  constructor(private http: HttpClient) {}

  ngOnInit(): void {
    this.accountId = localStorage.getItem('account_id') || '00000000-0000-0000-0000-000000000001';
    this.loadContacts();
  }

  loadContacts(): void {
    this.loading = true;
    let url = `${environment.apiUrl}/contacts/${this.accountId}?`;
    if (this.favoritesOnly) url += 'favoritesOnly=true&';
    if (this.searchTerm) url += `search=${this.searchTerm}`;
    this.http.get<any>(url).subscribe({
      next: (data) => { this.contacts = data.contacts; this.loading = false; },
      error: () => this.loading = false
    });
  }

  addContact(): void {
    this.http.post(`${environment.apiUrl}/contacts/${this.accountId}`, this.form).subscribe({
      next: () => {
        this.showForm = false;
        this.form = { name: '', pixKey: '', pixKeyType: 'CPF', bankName: '', nickname: '' };
        this.loadContacts();
      },
      error: (err) => alert(err.error?.error || 'Erro')
    });
  }

  toggleFavorite(id: string): void {
    this.http.post(`${environment.apiUrl}/contacts/${this.accountId}/${id}/favorite`, {}).subscribe(() => this.loadContacts());
  }

  deleteContact(id: string): void {
    if (!confirm('Remover este contato?')) return;
    this.http.delete(`${environment.apiUrl}/contacts/${this.accountId}/${id}`).subscribe(() => this.loadContacts());
  }

  search(): void { this.loadContacts(); }
}