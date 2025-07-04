import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators, AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';
import { finalize } from 'rxjs';
import { LoadingSpinner } from '../shared/loading-spinner/loading-spinner';
import { PaymentHistoryService } from '../../services/payment-history/payment-history.service';
import { RazorpayService, RazorpayResponse, RazorpayErrorResponse } from '../../services/razorpay/razorpay.service';
import { WindowService } from '../../services/window/window.service';

// Custom validator to allow only specific UPI IDs
export function allowedUpiIdValidator(allowedIds: string[]): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    if (!control.value) return null;
    const isAllowed = allowedIds.includes(control.value);
    return isAllowed ? null : { forbiddenUpiId: { value: control.value } };
  };
}

// Custom validator for contact number
export function contactNumberValidator(): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    if (!control.value) return null;
    const pattern = /^[6-9][0-9]{9}$/; // Indian mobile number pattern
    const isValid = pattern.test(control.value);
    return isValid ? null : { pattern: { value: control.value } };
  };
}

@Component({
  selector: 'app-payment-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, LoadingSpinner],
  templateUrl: './payment-form.html',
  styleUrl: './payment-form.css'
})
export class PaymentFormComponent implements OnInit {
  paymentForm: FormGroup;
  isLoading = false;
  paymentResult: { status: 'Success' | 'Failure' | 'Cancelled'; message: string } | null = null;
  allowedTestUpiIds = ['success@razorpay', 'failure@razorpay'];
  razorpayLoaded = false;

  constructor(
    private fb: FormBuilder,
    private razorpayService: RazorpayService,
    private paymentHistoryService: PaymentHistoryService,
    private windowService: WindowService
  ) {
    this.paymentForm = this.fb.group({
      amount: [10, [Validators.required, Validators.min(1), Validators.max(500000)]],
      customerName: ['Shaun Murphy', [Validators.required, Validators.minLength(2), Validators.maxLength(50)]],
      email: ['shaun@example.com', [Validators.required, Validators.email]],
      contact: ['9876543210', [Validators.required, contactNumberValidator()]],
      upiId: ['success@razorpay', [Validators.required, allowedUpiIdValidator(this.allowedTestUpiIds)]]
    });
  }

  ngOnInit(): void {
    this.checkRazorpayLoaded();
  }

  private checkRazorpayLoaded(): void {
    this.razorpayLoaded = this.windowService.isRazorpayLoaded();
    if (!this.razorpayLoaded) {
      console.warn('Razorpay SDK not loaded. Please ensure the script is included in index.html');
    }
  }

  onSubmit(): void {
    if (this.paymentForm.invalid) {
      this.paymentForm.markAllAsTouched();
      console.log('Form is invalid:', this.paymentForm.errors);
      return;
    }

    if (!this.razorpayLoaded) {
      this.paymentResult = {
        status: 'Failure',
        message: 'Razorpay SDK not loaded. Please refresh the page and try again.'
      };
      return;
    }

    this.isLoading = true;
    this.paymentResult = null;
    
    const formData = this.paymentForm.value;
    console.log('Form Data:', formData);

    // Validate UPI ID explicitly
    if (!this.allowedTestUpiIds.includes(formData.upiId)) {
      this.paymentResult = {
        status: 'Failure',
        message: 'Invalid UPI ID. Please select a valid test UPI ID.'
      };
      this.isLoading = false;
      return;
    }

    const { amount, customerName, email, contact, upiId } = formData;

    this.razorpayService.initiatePayment(amount, customerName, email, contact, upiId)
      .pipe(
        finalize(() => this.isLoading = false)
      )
      .subscribe({
        next: (response: RazorpayResponse) => {
          console.log('Payment Success:', response);
          this.handlePaymentSuccess(response);
        },
        error: (error: RazorpayErrorResponse) => {
          console.error('Payment Error:', error);
          this.handlePaymentFailure(error);
        }
      });
  }

  private handlePaymentSuccess(response: RazorpayResponse): void {
    const { amount, customerName, email, contact } = this.paymentForm.value;
    this.paymentResult = { 
      status: 'Success', 
      message: `Payment successful! Payment ID: ${response.razorpay_payment_id}` 
    };
    
    this.paymentHistoryService.addPayment({
      id: response.razorpay_payment_id,
      orderId: response.razorpay_order_id || 'N/A',
      amount,
      customerName,
      email,
      contact,
      status: 'Success'
    });
  }

  private handlePaymentFailure(error: RazorpayErrorResponse): void {
    const { amount, customerName, email, contact } = this.paymentForm.value;
    
    this.paymentResult = { 
      status: error.status, 
      message: error.message 
    };

    this.paymentHistoryService.addPayment({
      id: error.metadata?.payment_id || 'N/A',
      orderId: error.metadata?.order_id || 'N/A',
      amount,
      customerName,
      email,
      contact,
      status: error.status
    });
  }

  // Helper method to reset form
  resetForm(): void {
    this.paymentForm.reset({
      amount: 10,
      customerName: 'John Doe',
      email: 'john.doe@example.com',
      contact: '9876543210',
      upiId: 'success@razorpay'
    });
    this.paymentResult = null;
  }

  // Method to change UPI ID for quick testing
  setUpiId(upiId: string): void {
    this.paymentForm.patchValue({ upiId });
  }

  // Helper getters for form validation
  get amount() { return this.paymentForm.get('amount'); }
  get customerName() { return this.paymentForm.get('customerName'); }
  get email() { return this.paymentForm.get('email'); }
  get contact() { return this.paymentForm.get('contact'); }
  get upiId() { return this.paymentForm.get('upiId'); }
}