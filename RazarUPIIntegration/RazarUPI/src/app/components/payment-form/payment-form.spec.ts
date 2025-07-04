import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { of, throwError } from 'rxjs';
import { PaymentHistoryService } from '../../services/payment-history/payment-history.service';
import { RazorpayService, RazorpayResponse, RazorpayErrorResponse } from '../../services/razorpay/razorpay.service';
import { WindowService } from '../../services/window/window.service';
import { CommonModule } from '@angular/common'; 
import { LoadingSpinner } from '../shared/loading-spinner/loading-spinner'; 
import { PaymentFormComponent, allowedUpiIdValidator, contactNumberValidator } from './payment-form';

describe('PaymentFormComponent', () => {
  let component: PaymentFormComponent;
  let fixture: ComponentFixture<PaymentFormComponent>;
  let mockRazorpayService: jasmine.SpyObj<RazorpayService>;
  let mockPaymentHistoryService: jasmine.SpyObj<PaymentHistoryService>;
  let mockWindowService: jasmine.SpyObj<WindowService>;

  // Mock data for successful payment
  const mockSuccessResponse: RazorpayResponse = {
    razorpay_payment_id: 'pay_success123',
    razorpay_order_id: 'order_success123',
    razorpay_signature: 'signature_success123'
  };

  // Mock data for failed payment
  const mockFailureError: RazorpayErrorResponse = {
    status: 'Failure',
    message: 'Payment failed due to invalid details',
    code: 'BAD_REQUEST_ERROR',
    description: 'Payment failed due to invalid details',
    metadata: {
      payment_id: 'pay_failed123',
      order_id: 'order_failed123'
    }
  };

  // Mock data for cancelled payment
  const mockCancelledError: RazorpayErrorResponse = {
    status: 'Cancelled',
    message: 'Payment cancelled by user.'
  };

  beforeEach(async () => {
    // Create spy objects for the services
    mockRazorpayService = jasmine.createSpyObj('RazorpayService', ['initiatePayment']);
    mockPaymentHistoryService = jasmine.createSpyObj('PaymentHistoryService', ['addPayment']);
    mockWindowService = jasmine.createSpyObj('WindowService', ['isRazorpayLoaded']);

    await TestBed.configureTestingModule({
      imports: [
        PaymentFormComponent, 
        ReactiveFormsModule,
        CommonModule, 
        LoadingSpinner 
      ],
      providers: [
        FormBuilder, 
        { provide: RazorpayService, useValue: mockRazorpayService },
        { provide: PaymentHistoryService, useValue: mockPaymentHistoryService },
        { provide: WindowService, useValue: mockWindowService }
      ]
    }).compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(PaymentFormComponent);
    component = fixture.componentInstance;
    mockWindowService.isRazorpayLoaded.and.returnValue(true); 
    fixture.detectChanges(); 
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  // --- ngOnInit Tests ---
  describe('ngOnInit', () => {
    it('should call checkRazorpayLoaded on initialization', () => {
      spyOn(component as any, 'checkRazorpayLoaded'); // Spy on the private method
      component.ngOnInit();
      expect((component as any).checkRazorpayLoaded).toHaveBeenCalled();
    });

    it('should set razorpayLoaded to true if SDK is loaded', () => {
      mockWindowService.isRazorpayLoaded.and.returnValue(true);
      component.ngOnInit();
      expect(component.razorpayLoaded).toBeTrue();
    });

    it('should set razorpayLoaded to false and log warning if SDK is not loaded', () => {
      mockWindowService.isRazorpayLoaded.and.returnValue(false);
      spyOn(console, 'warn'); // Spy on console.warn
      component.ngOnInit();
      expect(component.razorpayLoaded).toBeFalse();
      expect(console.warn).toHaveBeenCalledWith('Razorpay SDK not loaded. Please ensure the script is included in index.html');
    });
  });

  // --- Form Initialization Tests ---
  describe('Form Initialization', () => {
    it('should initialize the form with default values', () => {
      expect(component.paymentForm.get('amount')?.value).toBe(10);
      expect(component.paymentForm.get('customerName')?.value).toBe('Shaun Murphy');
      expect(component.paymentForm.get('email')?.value).toBe('shaun@example.com');
      expect(component.paymentForm.get('contact')?.value).toBe('9876543210');
      expect(component.paymentForm.get('upiId')?.value).toBe('success@razorpay');
    });

    it('should have all controls initialized as valid initially', () => {
      expect(component.paymentForm.valid).toBeTrue();
    });
  });

  // --- Custom Validator Tests ---
  describe('Custom Validators', () => {
    describe('allowedUpiIdValidator', () => {
      const allowedIds = ['test@upi', 'another@upi'];
      const validator = allowedUpiIdValidator(allowedIds);

      it('should return null for an allowed UPI ID', () => {
        expect(validator({ value: 'test@upi' } as any)).toBeNull();
      });

      it('should return forbiddenUpiId error for a forbidden UPI ID', () => {
        expect(validator({ value: 'forbidden@upi' } as any)).toEqual({ forbiddenUpiId: { value: 'forbidden@upi' } });
      });

      it('should return null for empty value', () => {
        expect(validator({ value: '' } as any)).toBeNull();
        expect(validator({ value: null } as any)).toBeNull();
      });
    });

    describe('contactNumberValidator', () => {
      const validator = contactNumberValidator();

      it('should return null for a valid Indian mobile number (starts with 6-9, 10 digits)', () => {
        expect(validator({ value: '9876543210' } as any)).toBeNull();
        expect(validator({ value: '6123456789' } as any)).toBeNull();
      });

      it('should return pattern error for invalid length', () => {
        expect(validator({ value: '123456789' } as any)).toEqual({ pattern: { value: '123456789' } }); // 9 digits
        expect(validator({ value: '12345678901' } as any)).toEqual({ pattern: { value: '12345678901' } }); // 11 digits
      });

      it('should return pattern error for invalid starting digit', () => {
        expect(validator({ value: '1234567890' } as any)).toEqual({ pattern: { value: '1234567890' } });
        expect(validator({ value: '5876543210' } as any)).toEqual({ pattern: { value: '5876543210' } });
      });

      it('should return null for empty value', () => {
        expect(validator({ value: '' } as any)).toBeNull();
        expect(validator({ value: null } as any)).toBeNull();
      });
    });
  });

  // --- onSubmit Tests ---
  describe('onSubmit', () => {
    beforeEach(() => {
      // Reset spies before each onSubmit test
      mockRazorpayService.initiatePayment.calls.reset();
      mockPaymentHistoryService.addPayment.calls.reset();
      component.isLoading = false;
      component.paymentResult = null;
      mockWindowService.isRazorpayLoaded.and.returnValue(true); // Ensure SDK is loaded for most tests
      fixture.detectChanges();
    });

    it('should set paymentResult and not submit if Razorpay SDK is not loaded', () => {
      mockWindowService.isRazorpayLoaded.and.returnValue(false);
      component.ngOnInit(); // Re-running ngOnInit to update razorpayLoaded
      fixture.detectChanges();

      component.onSubmit();

      expect(component.paymentResult).toEqual({
        status: 'Failure',
        message: 'Razorpay SDK not loaded. Please refresh the page and try again.'
      });
      expect(mockRazorpayService.initiatePayment).not.toHaveBeenCalled();
      expect(component.isLoading).toBeFalse();
    });


    it('should call initiatePayment with correct form data', () => {
      mockRazorpayService.initiatePayment.and.returnValue(of(mockSuccessResponse));

      const testAmount = 500;
      const testName = 'Jane Doe';
      const testEmail = 'jane@example.com';
      const testContact = '9988776655';
      const testUpiId = 'success@razorpay';

      component.paymentForm.setValue({
        amount: testAmount,
        customerName: testName,
        email: testEmail,
        contact: testContact,
        upiId: testUpiId
      });
      fixture.detectChanges();

      component.onSubmit();

      expect(mockRazorpayService.initiatePayment).toHaveBeenCalledWith(
        testAmount, testName, testEmail, testContact, testUpiId
      );
    });

    it('should handle successful payment', (done) => {
      mockRazorpayService.initiatePayment.and.returnValue(of(mockSuccessResponse));
      spyOn(console, 'log'); // Spy on console.log for success message

      component.onSubmit();

      fixture.whenStable().then(() => {
        expect(component.paymentResult).toEqual({
          status: 'Success',
          message: `Payment successful! Payment ID: ${mockSuccessResponse.razorpay_payment_id}`
        });
        expect(mockPaymentHistoryService.addPayment).toHaveBeenCalledWith(jasmine.objectContaining({
          id: mockSuccessResponse.razorpay_payment_id,
          orderId: mockSuccessResponse.razorpay_order_id,
          amount: component.paymentForm.value.amount,
          status: 'Success'
        }));
        expect(console.log).toHaveBeenCalledWith('Payment Success:', mockSuccessResponse);
        expect(component.isLoading).toBeFalse();
        done();
      });
    });

    it('should handle failed payment', (done) => {
      mockRazorpayService.initiatePayment.and.returnValue(throwError(() => mockFailureError));
      spyOn(console, 'error'); // Spy on console.error for error message

      component.onSubmit();

      fixture.whenStable().then(() => {
        expect(component.paymentResult).toEqual({
          status: mockFailureError.status,
          message: mockFailureError.message
        });
        expect(mockPaymentHistoryService.addPayment).toHaveBeenCalledWith(jasmine.objectContaining({
          id: mockFailureError.metadata?.payment_id,
          orderId: mockFailureError.metadata?.order_id,
          amount: component.paymentForm.value.amount,
          status: mockFailureError.status
        }));
        expect(console.error).toHaveBeenCalledWith('Payment Error:', mockFailureError);
        expect(component.isLoading).toBeFalse();
        done();
      });
    });

    it('should handle cancelled payment', (done) => {
      mockRazorpayService.initiatePayment.and.returnValue(throwError(() => mockCancelledError));
      spyOn(console, 'error'); // Spy on console.error for error message

      component.onSubmit();

      fixture.whenStable().then(() => {
        expect(component.paymentResult).toEqual({
          status: mockCancelledError.status,
          message: mockCancelledError.message
        });
        expect(mockPaymentHistoryService.addPayment).toHaveBeenCalledWith(jasmine.objectContaining({
          id: 'N/A', // As cancelled error might not have payment_id
          orderId: 'N/A', // As cancelled error might not have order_id
          amount: component.paymentForm.value.amount,
          status: mockCancelledError.status
        }));
        expect(console.error).toHaveBeenCalledWith('Payment Error:', mockCancelledError);
        expect(component.isLoading).toBeFalse();
        done();
      });
    });

  });

  // --- Helper Methods Tests ---
  describe('Helper Methods', () => {
    it('resetForm should reset the form to initial values and clear paymentResult', () => {
      component.paymentForm.get('amount')?.setValue(500);
      component.paymentResult = { status: 'Success', message: 'Test message' };
      fixture.detectChanges();

      component.resetForm();
      fixture.detectChanges();

      expect(component.paymentForm.get('amount')?.value).toBe(10);
      expect(component.paymentForm.get('customerName')?.value).toBe('John Doe'); // Default reset value
      expect(component.paymentForm.get('email')?.value).toBe('john.doe@example.com'); // Default reset value
      expect(component.paymentForm.get('contact')?.value).toBe('9876543210'); // Default reset value
      expect(component.paymentForm.get('upiId')?.value).toBe('success@razorpay'); // Default reset value
      expect(component.paymentResult).toBeNull();
    });

    it('setUpiId should patch the upiId form control', () => {
      const newUpiId = 'failure@razorpay';
      component.setUpiId(newUpiId);
      expect(component.paymentForm.get('upiId')?.value).toBe(newUpiId);
    });
  });
});