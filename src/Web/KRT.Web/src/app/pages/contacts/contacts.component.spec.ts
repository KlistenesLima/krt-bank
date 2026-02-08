import { ComponentFixture, TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { ContactsComponent } from './contacts.component';

describe('ContactsComponent', () => {
  let component: ContactsComponent;
  let fixture: ComponentFixture<ContactsComponent>;
  let httpMock: HttpTestingController;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ContactsComponent, HttpClientTestingModule]
    }).compileComponents();
    fixture = TestBed.createComponent(ContactsComponent);
    component = fixture.componentInstance;
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  it('should create', () => { expect(component).toBeTruthy(); });

  it('should load contacts on init', () => {
    fixture.detectChanges();
    const req = httpMock.expectOne(r => r.url.includes('/contacts/'));
    req.flush({ contacts: [{ id: '1', name: 'Maria', isFavorite: true }], total: 1 });
    expect(component.contacts.length).toBe(1);
  });

  it('should toggle favorites filter', () => {
    component.accountId = '00000000-0000-0000-0000-000000000001';
    component.favoritesOnly = true;
    component.loadContacts();
    const req = httpMock.expectOne(r => r.url.includes('favoritesOnly=true'));
    req.flush({ contacts: [], total: 0 });
    expect(req.request.url).toContain('favoritesOnly');
  });
});