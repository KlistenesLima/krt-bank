import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { ChatbotComponent } from './chatbot.component';

describe('ChatbotComponent', () => {
  let component: ChatbotComponent;
  let fixture: ComponentFixture<ChatbotComponent>;
  let httpMock: HttpTestingController;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ChatbotComponent, HttpClientTestingModule]
    }).compileComponents();
    fixture = TestBed.createComponent(ChatbotComponent);
    component = fixture.componentInstance;
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  it('should create with welcome message', () => {
    fixture.detectChanges();
    httpMock.expectOne(r => r.url.includes('/chatbot/suggestions')).flush([]);
    expect(component.messages.length).toBe(1);
    expect(component.messages[0].isUser).toBeFalse();
  });

  it('should send message', fakeAsync(() => {
    fixture.detectChanges();
    httpMock.expectOne(r => r.url.includes('/chatbot/suggestions')).flush([]);
    component.inputText = 'saldo';
    component.send();
    expect(component.messages.length).toBe(2);
    expect(component.isTyping).toBeTrue();
    const req = httpMock.expectOne(r => r.url.includes('/chatbot/message'));
    req.flush({ response: 'Seu saldo...', suggestions: ['Ver extrato'], confidence: 0.92 });
    tick(1500);
    expect(component.messages.length).toBe(3);
    expect(component.isTyping).toBeFalse();
  }));

  it('should not send empty message', () => {
    fixture.detectChanges();
    httpMock.expectOne(r => r.url.includes('/chatbot/suggestions')).flush([]);
    component.inputText = '  ';
    component.send();
    expect(component.messages.length).toBe(1);
  });
});