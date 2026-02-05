import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

export interface Message {
  text: string;
  isUser: boolean;
  time: Date;
}

@Injectable({ providedIn: 'root' })
export class ChatService {
  private messages = new BehaviorSubject<Message[]>([
    { text: 'Olá! Sou o assistente virtual KRT. Como posso ajudar?', isUser: false, time: new Date() }
  ]);
  
  messages$ = this.messages.asObservable();

  sendMessage(text: string) {
    const current = this.messages.value;
    this.messages.next([...current, { text, isUser: true, time: new Date() }]);
    setTimeout(() => { this.botReply(text); }, 1000);
  }

  private botReply(userText: string) {
      const lower = userText.toLowerCase();
      let reply = '';
      if (lower.includes('pix') || lower.includes('transferir')) {
          reply = 'Para fazer um Pix, clique no botão "Pix" na tela inicial ou acesse o menu "Minhas Chaves".';
      } else if (lower.includes('senha') || lower.includes('trocar')) {
          reply = 'Você pode alterar sua senha indo em Perfil > Segurança.';
      } else if (lower.includes('saldo') || lower.includes('dinheiro')) {
          reply = 'Seu saldo está visível no topo da Dashboard. Use o ícone de "olho" para esconder ou mostrar.';
      } else {
          reply = 'Desculpe, ainda estou aprendendo. Tente perguntar sobre "Pix", "Senha" ou "Saldo".';
      }
      const updated = this.messages.value;
      this.messages.next([...updated, { text: reply, isUser: false, time: new Date() }]);
  }
}
