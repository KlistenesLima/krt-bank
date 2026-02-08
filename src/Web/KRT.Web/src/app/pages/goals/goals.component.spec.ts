import { ComponentFixture, TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { GoalsComponent } from './goals.component';

describe('GoalsComponent', () => {
  let component: GoalsComponent;
  let fixture: ComponentFixture<GoalsComponent>;
  let httpMock: HttpTestingController;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [GoalsComponent, HttpClientTestingModule]
    }).compileComponents();
    fixture = TestBed.createComponent(GoalsComponent);
    component = fixture.componentInstance;
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  it('should create', () => { expect(component).toBeTruthy(); });

  it('should load goals on init', () => {
    fixture.detectChanges();
    const req = httpMock.expectOne(r => r.url.includes('/goals/'));
    req.flush({ goals: [{ id: '1', title: 'Viagem', progressPercent: 45 }], summary: { totalGoals: 1, totalSaved: 6750, totalTarget: 15000, overallProgress: 45 } });
    expect(component.goals.length).toBe(1);
    expect(component.summary.overallProgress).toBe(45);
  });
});