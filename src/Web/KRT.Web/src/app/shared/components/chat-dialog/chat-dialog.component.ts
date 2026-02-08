import { Component, ElementRef, ViewChild, AfterViewChecked, OnInit } from '@angular/core';
import { ChatService, Message } from '../../../core/services/chat.service';

@Component({
  selector: 'app-chat-dialog',
  template: `
    <div class="chat-container">
      <div class="chat-header">
        <div class="header-left">
          <div class="avatar">🤖</div>
          <div class="header-info">
            <h3>Assistente KRT</h3>
            <span class="status">Online</span>
          </div>
        </div>
        <button class="close-btn" (click)="close()">✕</button>
      </div>
      <div class="chat-body" #scrollMe>
        <div class="msg-row" *ngFor="let msg of messages" [class.user]="msg.isUser">
          <div class="avatar-sm" *ngIf="!msg.isUser">🤖</div>
          <div class="bubble" [class.user-bubble]="msg.isUser" [class.bot-bubble]="!msg.isUser">
            {{ msg.text }}
          </div>
        </div>
      </div>
      <div class="chat-footer">
        <input type="text" placeholder="Digite sua mensagem..." [(ngModel)]="inputText" (keyup.enter)="send()">
        <button class="send-btn" (click)="send()" [disabled]="!inputText.trim()">
          <mat-icon>send</mat-icon>
        </button>
      </div>
    </div>
  `,
  styles: [`
    .chat-container {
      width: 340px; height: 460px; background: #fff;
      border-radius: 16px; display: flex; flex-direction: column;
      box-shadow: 0 8px 30px rgba(0,0,0,0.18);
      overflow: hidden; font-family: 'Inter', sans-serif;
    }
    .chat-header {
      background: linear-gradient(135deg, #0052D4, #0047BB);
      color: white; padding: 14px 16px;
      display: flex; justify-content: space-between; align-items: center;
    }
    .header-left { display: flex; align-items: center; gap: 10px; }
    .avatar {
      width: 36px; height: 36px; border-radius: 50%;
      background: rgba(255,255,255,0.2); display: flex;
      align-items: center; justify-content: center; font-size: 1.2rem;
    }
    .header-info h3 { margin: 0; font-size: 0.95rem; font-weight: 600; }
    .status { font-size: 0.7rem; opacity: 0.8; }
    .close-btn {
      background: rgba(255,255,255,0.15); border: none; color: white;
      width: 28px; height: 28px; border-radius: 50%; cursor: pointer;
      font-size: 0.85rem; display: flex; align-items: center; justify-content: center;
    }
    .close-btn:hover { background: rgba(255,255,255,0.3); }

    .chat-body {
      flex: 1; padding: 16px; overflow-y: auto;
      display: flex; flex-direction: column; gap: 10px;
      background: #f5f7fb;
    }
    .msg-row { display: flex; align-items: flex-end; gap: 8px; }
    .msg-row.user { justify-content: flex-end; }
    .avatar-sm {
      width: 26px; height: 26px; border-radius: 50%;
      background: #e8ecf4; display: flex; align-items: center;
      justify-content: center; font-size: 0.75rem; flex-shrink: 0;
    }
    .bubble {
      padding: 10px 14px; font-size: 0.85rem; line-height: 1.4;
      max-width: 75%; word-wrap: break-word;
    }
    .bot-bubble {
      background: white; color: #333; border-radius: 14px 14px 14px 4px;
      box-shadow: 0 1px 3px rgba(0,0,0,0.08);
    }
    .user-bubble {
      background: linear-gradient(135deg, #0052D4, #0047BB);
      color: white; border-radius: 14px 14px 4px 14px;
    }

    .chat-footer {
      padding: 10px 12px; border-top: 1px solid #eef0f4;
      display: flex; align-items: center; gap: 8px; background: white;
    }
    .chat-footer input {
      flex: 1; padding: 10px 14px; border: 1px solid #e0e3ea;
      border-radius: 24px; outline: none; font-size: 0.85rem;
      color: #333; background: #f5f7fb;
    }
    .chat-footer input:focus { border-color: #0052D4; background: white; }
    .chat-footer input::placeholder { color: #999; }
    .send-btn {
      width: 38px; height: 38px; border-radius: 50%; border: none;
      background: linear-gradient(135deg, #0052D4, #0047BB);
      color: white; cursor: pointer; display: flex;
      align-items: center; justify-content: center;
    }
    .send-btn:disabled { opacity: 0.4; cursor: not-allowed; }
    .send-btn mat-icon { font-size: 18px; width: 18px; height: 18px; }
  `]
})
export class ChatDialogComponent implements OnInit, AfterViewChecked {
  messages: Message[] = [];
  inputText = '';
  @ViewChild('scrollMe') private myScrollContainer!: ElementRef;
  constructor(private chatService: ChatService, private _elementRef: ElementRef) {}
  ngOnInit() {
    this.chatService.messages$.subscribe(msgs => { this.messages = msgs; });
  }
  ngAfterViewChecked() { this.scrollToBottom(); }
  scrollToBottom(): void {
    try { this.myScrollContainer.nativeElement.scrollTop = this.myScrollContainer.nativeElement.scrollHeight; } catch(err) {}
  }
  send() {
    if (!this.inputText.trim()) return;
    this.chatService.sendMessage(this.inputText);
    this.inputText = '';
  }
  close() {
    this._elementRef.nativeElement.dispatchEvent(new CustomEvent('close-chat', { bubbles: true }));
  }
}
