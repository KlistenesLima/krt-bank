import { Injectable } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';

@Injectable({ providedIn: 'root' })
export class NotificationService {
  constructor(private snackBar: MatSnackBar) {}

  success(message: string) {
    this.snackBar.open(message, 'OK', {
      duration: 4000,
      panelClass: ['snackbar-success'], // Vamos estilizar no CSS
      horizontalPosition: 'center',
      verticalPosition: 'top'
    });
  }

  error(message: string) {
    this.snackBar.open(message, 'FECHAR', {
      duration: 5000,
      panelClass: ['snackbar-error'],
      horizontalPosition: 'center',
      verticalPosition: 'top'
    });
  }
}
