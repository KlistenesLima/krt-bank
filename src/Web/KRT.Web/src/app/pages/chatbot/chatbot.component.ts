import { Component, OnInit, ViewChild, ElementRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';

interface ChatMessage { text: string; isUser: boolean; suggestions?: string[]; timestamp: Date; }

@Component({
  selector: 'app-chatbot',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './chatbot.component.html',
  styleUrls: ['./chatbot.component.scss']
})
export class ChatbotComponent implements OnInit {
  @ViewChild('chatBody') chatBody!: ElementRef;
  messages: ChatMessage[] = [];
  inputText = '';
  isTyping = false;
  quickSuggestions: string[] = [];

  constructor(private http: HttpClient) {}

  ngOnInit(): void {
    this.messages.push({ text: 'Ola! Sou o assistente do KRT Bank. Como posso ajudar?', isUser: false,
      suggestions: ['Ver saldo', 'Fazer Pix', 'Ajuda'], timestamp: new Date() });
    this.http.get<string[]>(`${environment.apiUrl}/chatbot/suggestions`).subscribe(s => this.quickSuggestions = s);
  }

  send(): void {
    if (!this.inputText.trim()) return;
    const msg = this.inputText.trim();
    this.messages.push({ text: msg, isUser: true, timestamp: new Date() });
    this.inputText = '';
    this.isTyping = true;
    this.scrollToBottom();

    this.http.post<any>(`${environment.apiUrl}/chatbot/message`, { message: msg }).subscribe({
      next: (data) => {
        setTimeout(() => {
          this.messages.push({ text: data.response, isUser: false, suggestions: data.suggestions, timestamp: new Date() });
          this.isTyping = false;
          this.scrollToBottom();
        }, 500 + Math.random() * 500);
      },
      error: () => {
        this.messages.push({ text: 'Desculpe, tive um problema. Tente novamente.', isUser: false, timestamp: new Date() });
        this.isTyping = false;
      }
    });
  }

  sendSuggestion(text: string): void { this.inputText = text; this.send(); }

  scrollToBottom(): void {
    setTimeout(() => { if (this.chatBody) this.chatBody.nativeElement.scrollTop = this.chatBody.nativeElement.scrollHeight; }, 100);
  }
}