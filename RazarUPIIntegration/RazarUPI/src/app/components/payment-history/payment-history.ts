import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { Observable } from 'rxjs';
import { PaymentHistoryItem } from '../../models/payment-history-item.model';
import { PaymentHistoryService } from '../../services/payment-history/payment-history.service';

@Component({
  selector: 'app-payment-history',
  imports: [CommonModule],
  templateUrl: './payment-history.html',
  styleUrl: './payment-history.css'
})
export class PaymentHistory {

  paymentHistory$!: Observable<PaymentHistoryItem[]>;

  constructor(private paymentHistoryService: PaymentHistoryService) {}

  ngOnInit(): void {
    this.paymentHistory$ = this.paymentHistoryService.getHistory();
  }

}
