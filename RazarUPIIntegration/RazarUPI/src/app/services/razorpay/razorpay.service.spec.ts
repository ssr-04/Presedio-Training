import { TestBed } from '@angular/core/testing';
import { Observable } from 'rxjs';
import { RazorpayService, RazorpayResponse, RazorpayErrorResponse } from './razorpay.service';
import { WindowService } from '../window/window.service';

describe('RazorpayService', () => {
  let service: RazorpayService;
  let windowServiceSpy: jasmine.SpyObj<WindowService>;
  let mockRazorpay: any;

  beforeEach(() => {
    // Create a spy for WindowService
    windowServiceSpy = jasmine.createSpyObj('WindowService', ['nativeWindow']);

    // Mock the global Razorpay object and its methods
    mockRazorpay = jasmine.createSpy('Razorpay').and.callFake(function(this: any, options: any) {
      this.options = options; // Store options for inspection if needed
      this.open = jasmine.createSpy('open');
      this.on = jasmine.createSpy('on').and.callFake((event: string, callback: Function) => {
        // Store event handlers for later triggering in tests
        if (event === 'payment.failed') {
          this.failedHandler = callback;
        }
      });
    });

    // Set up nativeWindow to return our mock Razorpay object
    Object.defineProperty(windowServiceSpy, 'nativeWindow', {
      get: () => ({ Razorpay: mockRazorpay })
    });

    TestBed.configureTestingModule({
      providers: [
        RazorpayService,
        { provide: WindowService, useValue: windowServiceSpy }
      ]
    });
    service = TestBed.inject(RazorpayService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  describe('initiatePayment', () => {
    const mockAmount = 100;
    const mockCustomerName = 'John Doe';
    const mockEmail = 'john.doe@example.com';
    const mockContact = '1234567890';
    const mockUpiId = 'john.doe@upi';

    it('should call Razorpay constructor with correct options', () => {
      service.initiatePayment(mockAmount, mockCustomerName, mockEmail, mockContact, mockUpiId).subscribe();

      expect(mockRazorpay).toHaveBeenCalledWith(jasmine.objectContaining({
        key: 'rzp_test_u2zpkcOfjVym1W',
        amount: mockAmount * 100, // Converted to paise
        currency: 'INR',
        name: 'RazorUPI Corp',
        description: 'Test UPI Transaction',
        prefill: {
          name: mockCustomerName,
          email: mockEmail,
          contact: mockContact,
          vpa: mockUpiId
        },
        method: {
          upi: true
        }
      }));
    });

    it('should call rzpInstance.open()', () => {
      let rzpInstanceSpy: any;
      mockRazorpay.and.callFake(function(options: any) {
        rzpInstanceSpy = {
          open: jasmine.createSpy('open'),
          on: jasmine.createSpy('on')
        };
        return rzpInstanceSpy;
      });

      service.initiatePayment(mockAmount, mockCustomerName, mockEmail, mockContact, mockUpiId).subscribe();

      expect(rzpInstanceSpy.open).toHaveBeenCalled();
    });

    it('should emit success response when handler is called', (done) => {
      const mockRazorpayResponse: RazorpayResponse = { razorpay_payment_id: 'pay_test123' };
      
      // Temporarily store the handler function passed to Razorpay constructor
      let handlerCallback: Function | undefined;
      mockRazorpay.and.callFake(function(this: any, options: any) {
        handlerCallback = options.handler;
        this.open = jasmine.createSpy('open');
        this.on = jasmine.createSpy('on');
      });

      service.initiatePayment(mockAmount, mockCustomerName, mockEmail, mockContact, mockUpiId).subscribe({
        next: (response) => {
          expect(response).toEqual(mockRazorpayResponse);
          done();
        },
        error: (err) => done.fail('Should not have errored')
      });

      // Simulate Razorpay calling the success handler
      if (handlerCallback) {
        handlerCallback(mockRazorpayResponse);
      } else {
        done.fail('Handler callback was not set');
      }
    });

    it('should emit error when payment fails', (done) => {
      const mockErrorResponse = {
        error: {
          code: 'BAD_REQUEST_ERROR',
          description: 'Payment failed due to invalid details',
          source: 'customer',
          step: 'payment_initiation',
          reason: 'input_validation_failed',
          metadata: {
            payment_id: 'pay_test_failed123',
            order_id: 'order_test_failed123'
          }
        }
      };

      let rzpInstance: any;
      mockRazorpay.and.callFake(function(options: any) {
        rzpInstance = {
          open: jasmine.createSpy('open'),
          on: jasmine.createSpy('on').and.callFake(function(this: any, event: string, callback: Function) {
            if (event === 'payment.failed') {
              this.failedHandler = callback; // Store the handler
            }
          })
        };
        return rzpInstance;
      });

      service.initiatePayment(mockAmount, mockCustomerName, mockEmail, mockContact, mockUpiId).subscribe({
        next: () => done.fail('Should have errored'),
        error: (error: RazorpayErrorResponse) => {
          expect(error.status).toEqual('Failure');
          expect(error.message).toEqual(mockErrorResponse.error.description);
          expect(error.code).toEqual(mockErrorResponse.error.code);
          expect(error.metadata).toEqual(mockErrorResponse.error.metadata);
          done();
        }
      });

      // Simulate Razorpay calling the payment.failed handler
      if (rzpInstance.on.calls.allArgs().find((args: any[]) => args[0] === 'payment.failed')) {
        const failedHandler = rzpInstance.on.calls.allArgs().find((args: any[]) => args[0] === 'payment.failed')[1];
        failedHandler(mockErrorResponse);
      } else {
        done.fail('Payment failed handler was not registered');
      }
    });

    it('should emit error when payment is cancelled by user', (done) => {
      // Temporarily store the ondismiss function passed to Razorpay constructor
      let onDismissCallback: Function | undefined;
      mockRazorpay.and.callFake(function(this: any, options: any) {
        onDismissCallback = options.modal?.ondismiss;
        this.open = jasmine.createSpy('open');
        this.on = jasmine.createSpy('on');
      });

      service.initiatePayment(mockAmount, mockCustomerName, mockEmail, mockContact, mockUpiId).subscribe({
        next: () => done.fail('Should have errored'),
        error: (error: RazorpayErrorResponse) => {
          expect(error.status).toEqual('Cancelled');
          expect(error.message).toEqual('Payment cancelled by user.');
          done();
        }
      });

      // Simulate Razorpay calling the ondismiss handler
      if (onDismissCallback) {
        onDismissCallback();
      } else {
        done.fail('OnDismiss callback was not set');
      }
    });

    it('should emit error if Razorpay SDK is not loaded', (done) => {
      // Configure nativeWindow to return no Razorpay object
      Object.defineProperty(windowServiceSpy, 'nativeWindow', {
        get: () => ({ Razorpay: undefined })
      });

      service.initiatePayment(mockAmount, mockCustomerName, mockEmail, mockContact, mockUpiId).subscribe({
        next: () => done.fail('Should have errored'),
        error: (error: RazorpayErrorResponse) => {
          expect(error.status).toEqual('Failure');
          expect(error.message).toEqual('Razorpay SDK is not loaded. Please ensure the Razorpay script is included.');
          done();
        }
      });
    });
  });
});