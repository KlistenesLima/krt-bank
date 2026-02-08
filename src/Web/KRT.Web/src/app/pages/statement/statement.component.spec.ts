import { ComponentFixture, TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { StatementComponent } from './statement.component';

describe('StatementComponent', () => {
  let component: StatementComponent;
  let fixture: ComponentFixture<StatementComponent>;
  let httpMock: HttpTestingController;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [StatementComponent, HttpClientTestingModule]
    }).compileComponents();
    fixture = TestBed.createComponent(StatementComponent);
    component = fixture.componentInstance;
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  it('should create', () => { expect(component).toBeTruthy(); });

  it('should load statement on init', () => {
    fixture.detectChanges();
    const req = httpMock.expectOne(r => r.url.includes('/statement/'));
    req.flush({ items: [{ id: '1', type: 'PIX_SENT', amount: -100 }], totalItems: 1, totalPages: 1, summary: { totalIncome: 0, totalExpenses: 100, net: -100 } });
    expect(component.transactions.length).toBe(1);
  });

  it('should apply filters', () => {
    component.accountId = '00000000-0000-0000-0000-000000000001';
    component.typeFilter = 'PIX_SENT';
    component.applyFilters();
    const req = httpMock.expectOne(r => r.url.includes('type=PIX_SENT'));
    expect(req.request.params.get('type')).toBe('PIX_SENT');
    expect(component.page).toBe(1);
    req.flush({ items: [], totalItems: 0, totalPages: 0, summary: { totalIncome: 0, totalExpenses: 0, net: 0 } });
  });

  it('should clear filters', () => {
    component.typeFilter = 'PIX_SENT';
    component.searchTerm = 'Maria';
    component.clearFilters();
    const req = httpMock.expectOne(r => r.url.includes('/statement/'));
    req.flush({ items: [], totalItems: 0, totalPages: 0, summary: {} });
    expect(component.typeFilter).toBe('');
    expect(component.searchTerm).toBe('');
  });

  it('should sort by column', () => {
    component.accountId = '00000000-0000-0000-0000-000000000001';
    component.sort('amount');
    expect(component.sortBy).toBe('amount');
    expect(component.sortOrder).toBe('desc');
    const req = httpMock.expectOne(r => r.url.includes('/statement/'));
    req.flush({ items: [], totalItems: 0, totalPages: 0, summary: {} });
    component.sort('amount');
    const req2 = httpMock.expectOne(r => r.url.includes('/statement/'));
    req2.flush({ items: [], totalItems: 0, totalPages: 0, summary: {} });
    expect(component.sortOrder).toBe('asc');
  });

  it('should paginate', () => {
    component.totalPages = 5;
    component.accountId = '00000000-0000-0000-0000-000000000001';
    component.goToPage(3);
    const req = httpMock.expectOne(r => r.url.includes('/statement/'));
    req.flush({ items: [], totalItems: 0, totalPages: 5, summary: {} });
    expect(component.page).toBe(3);
  });

  it('getTypeLabel should map correctly', () => {
    expect(component.getTypeLabel('PIX_SENT')).toBe('Pix Enviado');
    expect(component.getTypeLabel('BOLETO')).toBe('Boleto');
    expect(component.getTypeLabel('UNKNOWN')).toBe('UNKNOWN');
  });
});