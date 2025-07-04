import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';
import { PaymentHistoryItem } from '../../models/payment-history-item.model';

@Injectable({
  providedIn: 'root'
})

export class PaymentHistoryService {

  private readonly STORAGE_KEY = 'razorUpiPaymentHistory';
  private historySubject: BehaviorSubject<PaymentHistoryItem[]>;

  constructor() {
    const history = this.getHistoryFromStorage();
    this.historySubject = new BehaviorSubject<PaymentHistoryItem[]>(history);
  }

  private getHistoryFromStorage(): PaymentHistoryItem[] {
    try {
      const storedHistory = localStorage.getItem(this.STORAGE_KEY);
      return storedHistory ? JSON.parse(storedHistory) : [];
    } catch (e) {
      console.error('Error reading from localStorage', e);
      return [];
    }
  }

  private saveHistoryToStorage(history: PaymentHistoryItem[]): void {
    try {
      localStorage.setItem(this.STORAGE_KEY, JSON.stringify(history));
    } catch (e) {
      console.error('Error saving to localStorage', e);
    }
  }

  getHistory(): Observable<PaymentHistoryItem[]> {
    return this.historySubject.asObservable();
  }

  addPayment(payment: Omit<PaymentHistoryItem, 'timestamp'>): void {
    const newPayment: PaymentHistoryItem = {
      ...payment,
      timestamp: new Date()
    };

    const currentHistory = this.historySubject.getValue();
    // Adds the newest payment to the beginning of the array
    const updatedHistory = [newPayment, ...currentHistory];
    
    this.saveHistoryToStorage(updatedHistory);
    this.historySubject.next(updatedHistory);
  }

}
