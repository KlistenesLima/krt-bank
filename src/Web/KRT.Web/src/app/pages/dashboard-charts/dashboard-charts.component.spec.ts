import { ComponentFixture, TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { DashboardChartsComponent } from './dashboard-charts.component';
import { environment } from '../../../environments/environment';

describe('DashboardChartsComponent', () => {
  let component: DashboardChartsComponent;
  let fixture: ComponentFixture<DashboardChartsComponent>;
  let httpMock: HttpTestingController;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DashboardChartsComponent, HttpClientTestingModule]
    }).compileComponents();
    fixture = TestBed.createComponent(DashboardChartsComponent);
    component = fixture.componentInstance;
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should set default accountId', () => {
    fixture.detectChanges();
    expect(component.accountId).toBeTruthy();
  });

  it('should load summary on init', () => {
    fixture.detectChanges();
    const req = httpMock.expectOne(r => r.url.includes('/dashboard/summary/'));
    expect(req.request.method).toBe('GET');
    req.flush({ balance: 10000, incomeThisMonth: 5000, expensesThisMonth: 3000, totalTransactions: 25 });
    expect(component.summary).toBeTruthy();
    expect(component.summary.balance).toBe(10000);
    expect(component.loading).toBeFalse();
  });

  it('should handle summary error', () => {
    fixture.detectChanges();
    const req = httpMock.expectOne(r => r.url.includes('/dashboard/summary/'));
    req.error(new ProgressEvent('error'));
    expect(component.loading).toBeFalse();
  });
});