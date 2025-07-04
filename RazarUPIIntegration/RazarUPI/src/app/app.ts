import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { PaymentFormComponent } from './components/payment-form/payment-form';
import { PaymentHistory } from './components/payment-history/payment-history';

@Component({
  selector: 'app-root',
  imports: [PaymentHistory, PaymentFormComponent],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
  protected title = 'RazarUPI';
}
