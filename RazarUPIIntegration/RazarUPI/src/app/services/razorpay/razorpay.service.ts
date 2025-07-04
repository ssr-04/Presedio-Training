import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { WindowService } from '../window/window.service';

export interface RazorpayResponse {
  razorpay_payment_id: string;
  razorpay_order_id?: string;
  razorpay_signature?: string;
}

export interface RazorpayErrorResponse {
  status: 'Failure' | 'Cancelled';
  message: string;
  code?: string;
  description?: string;
  metadata?: {
    payment_id?: string;
    order_id?: string;
  };
}

@Injectable({
  providedIn: 'root'
})
export class RazorpayService {
  private readonly RAZORPAY_KEY_ID = 'rzp_test_u2zpkcOfjVym1W';

  constructor(private windowService: WindowService) {}

  initiatePayment(
    amount: number,
    customerName: string,
    email: string,
    contact: string,
    upiId: string
  ): Observable<RazorpayResponse> {
    return new Observable<RazorpayResponse>((observer) => {
      const Razorpay = this.windowService.nativeWindow.Razorpay;
      
      if (!Razorpay) {
        observer.error({
          status: 'Failure',
          message: 'Razorpay SDK is not loaded. Please ensure the Razorpay script is included.'
        } as RazorpayErrorResponse);
        return;
      }

      const options: any = {
        key: this.RAZORPAY_KEY_ID,
        amount: amount * 100, // Convert to paise
        currency: 'INR',
        name: 'RazorUPI Corp',
        description: 'Test UPI Transaction',
        image: 'https://example.com/logo.jpg',
        handler: (response: RazorpayResponse) => {
          observer.next(response);
          observer.complete();
        },
        prefill: {
          name: customerName,
          email: email,
          contact: contact,
          vpa: upiId 
        },
        theme: { 
          color: '#3399cc' 
        },
        method: {
          upi:true
        },
        modal: {
          ondismiss: () => {
            observer.error({
              status: 'Cancelled',
              message: 'Payment cancelled by user.'
            } as RazorpayErrorResponse);
            observer.complete();
          }
        }
      };


      const rzpInstance = new Razorpay(options);
      
      rzpInstance.on('payment.failed', (response: any) => {
        observer.error({
          status: 'Failure',
          message: response?.error?.description || 'Payment failed',
          code: response?.error?.code,
          description: response?.error?.description,
          metadata: response?.error?.metadata
        } as RazorpayErrorResponse);
        observer.complete();
      });

      rzpInstance.open();
    });
  }
}