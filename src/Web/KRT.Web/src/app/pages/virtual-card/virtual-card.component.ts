import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CardService, VirtualCard } from '../../core/services/card.service';

@Component({
  selector: 'app-virtual-card',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './virtual-card.component.html',
  styleUrls: ['./virtual-card.component.scss']
})
export class VirtualCardComponent implements OnInit {
  cards: VirtualCard[] = [];
  selectedCard: VirtualCard | null = null;
  showCardNumber = false;
  showCvv = false;
  creating = false;
  holderName = '';
  selectedBrand = 'Visa';
  accountId = '';

  constructor(private cardService: CardService) {}

  ngOnInit(): void {
    this.accountId = localStorage.getItem('account_id') || '';
    if (this.accountId) this.loadCards();
  }

  loadCards(): void {
    this.cardService.getCards(this.accountId).subscribe(cards => {
      this.cards = cards;
      if (cards.length > 0 && !this.selectedCard) {
        this.selectCard(cards[0]);
      }
    });
  }

  selectCard(card: VirtualCard): void {
    this.cardService.getCard(card.id).subscribe(full => {
      this.selectedCard = full;
      this.showCardNumber = false;
      this.showCvv = false;
    });
  }

  createCard(): void {
    if (!this.holderName) return;
    this.creating = true;
    this.cardService.createCard(this.accountId, this.holderName, this.selectedBrand)
      .subscribe({
        next: (card) => {
          this.cards.push(card);
          this.selectCard(card);
          this.creating = false;
          this.holderName = '';
        },
        error: () => this.creating = false
      });
  }

  toggleBlock(): void {
    if (!this.selectedCard) return;
    const action = this.selectedCard.status === 'Active'
      ? this.cardService.blockCard(this.selectedCard.id)
      : this.cardService.unblockCard(this.selectedCard.id);

    action.subscribe(() => {
      this.loadCards();
      if (this.selectedCard) this.selectCard(this.selectedCard);
    });
  }

  rotateCvv(): void {
    if (!this.selectedCard) return;
    this.cardService.rotateCvv(this.selectedCard.id).subscribe(res => {
      if (this.selectedCard) {
        this.selectedCard.cvv = res.cvv;
        this.selectedCard.cvvValid = true;
        this.showCvv = true;
      }
    });
  }

  cancelCard(): void {
    if (!this.selectedCard || !confirm('Cancelar cartao permanentemente?')) return;
    this.cardService.cancelCard(this.selectedCard.id).subscribe(() => {
      this.selectedCard = null;
      this.loadCards();
    });
  }

  updateSetting(field: string, value: boolean): void {
    if (!this.selectedCard) return;
    this.cardService.updateSettings(this.selectedCard.id, { [field]: value })
      .subscribe(() => { if (this.selectedCard) this.selectCard(this.selectedCard); });
  }

  get brandLogo(): string {
    return this.selectedCard?.brand === 'Mastercard' ? '⬤⬤' : '𝗩';
  }
}