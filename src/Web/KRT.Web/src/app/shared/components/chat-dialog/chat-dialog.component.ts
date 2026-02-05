import { Component, ElementRef, ViewChild, AfterViewChecked, OnInit } from '@angular/core';
// CORREÇÃO: O caminho correto para sair de shared/components/chat-dialog até core/services é ../../../
import { ChatService, Message } from '../../../core/services/chat.service';

@Component({
  selector: 'app-chat-dialog',
  template: `
    <div class="chat-container">
      <div class="chat-header">
        <h3>Assistente KRT</h3>
        <button mat-icon-button (click)="close()"><mat-icon>close</mat-icon></button>
      </div>
      <div class="chat-body" #scrollMe>
        <div class="message" *ngFor="let msg of messages" [class.user]="msg.isUser">
            <div class="bubble">{{ msg.text }}</div>
        </div>
      </div>
      <div class="chat-footer">
        <input type="text" placeholder="Dúvidas?" [(ngModel)]="inputText" (keyup.enter)="send()">
        <button mat-icon-button color="primary" (click)="send()"><mat-icon>send</mat-icon></button>
      </div>
    </div>
  `,
  styles: [`
    .chat-container { width: 300px; height: 400px; background: white; border-radius: 12px; display: flex; flex-direction: column; box-shadow: 0 5px 20px rgba(0,0,0,0.2); border: 1px solid #eee; }
    .chat-header { background: var(--primary); color: white; padding: 10px; display: flex; justify-content: space-between; align-items: center; border-radius: 12px 12px 0 0; }
    .chat-header h3 { margin: 0; font-size: 1rem; }
    .chat-body { flex: 1; padding: 10px; overflow-y: auto; display: flex; flex-direction: column; gap: 8px; background: #f9f9f9; }
    .message { display: flex; width: 100%; }
    .message.user { justify-content: flex-end; }
    .bubble { padding: 8px 12px; border-radius: 10px; background: #fff; font-size: 0.9rem; max-width: 80%; box-shadow: 0 1px 2px rgba(0,0,0,0.1); }
    .user .bubble { background: var(--primary); color: white; }
    .chat-footer { padding: 8px; border-top: 1px solid #eee; display: flex; align-items: center; background: white; }
    .chat-footer input { flex: 1; padding: 8px; border: 1px solid #ddd; border-radius: 20px; outline: none; margin-right: 5px; }
  `]
})
export class ChatDialogComponent implements OnInit, AfterViewChecked {
  messages: Message[] = [];
  inputText = '';
  @ViewChild('scrollMe') private myScrollContainer!: ElementRef;

  constructor(private chatService: ChatService, private _elementRef: ElementRef) {}

  ngOnInit() {
      this.chatService.messages$.subscribe(msgs => {
          this.messages = msgs;
      });
  }

  ngAfterViewChecked() {        
      this.scrollToBottom();        
  } 

  scrollToBottom(): void {
      try {
          this.myScrollContainer.nativeElement.scrollTop = this.myScrollContainer.nativeElement.scrollHeight;
      } catch(err) { }                 
  }

  send() {
      if(!this.inputText.trim()) return;
      this.chatService.sendMessage(this.inputText);
      this.inputText = '';
  }

  close() {
      this._elementRef.nativeElement.dispatchEvent(new CustomEvent('close-chat', { bubbles: true }));
  }
}
