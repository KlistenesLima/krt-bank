import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';

@Component({
  selector: 'app-monitoring',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './monitoring.component.html',
  styleUrls: ['./monitoring.component.scss']
})
export class MonitoringComponent implements OnInit, OnDestroy {
  metrics: any = null;
  refreshInterval: any;
  lastUpdated = new Date();

  constructor(private http: HttpClient) {}

  ngOnInit(): void {
    this.loadMetrics();
    this.refreshInterval = setInterval(() => this.loadMetrics(), 10000);
  }

  ngOnDestroy(): void { clearInterval(this.refreshInterval); }

  loadMetrics(): void {
    this.http.get<any>(`${environment.apiUrl}/metrics/json`).subscribe({
      next: (data) => { this.metrics = data; this.lastUpdated = new Date(); },
      error: () => {}
    });
  }
}