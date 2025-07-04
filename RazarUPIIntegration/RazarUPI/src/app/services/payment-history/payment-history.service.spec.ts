import { TestBed } from '@angular/core/testing';
import { PaymentHistoryService } from './payment-history.service';
import { PaymentHistoryItem } from '../../models/payment-history-item.model';

describe('PaymentHistoryService', () => {
  let service: PaymentHistoryService;
  let localStorageMock: { [key: string]: string }; // Mock object for localStorage

  // Define a mock PaymentHistoryItem for consistent testing
  const mockPayment: Omit<PaymentHistoryItem, 'timestamp'> = {
    amount: 100,
    status: 'Success',
    id : '1', 
    orderId: '123', 
    customerName:'test', 
    email:'test', 
    contact:'test'
  };

  const mockPayment2: Omit<PaymentHistoryItem, 'timestamp'> = {
    amount: 200,
    status: 'Failure',
    id : '2', 
    orderId: '1232', 
    customerName:'test2', 
    email:'test2', 
    contact:'test2'
  };

  beforeEach(() => {
    // Initialize a fresh mock localStorage before each test
    localStorageMock = {};

    // Spy on the global localStorage object and replace its methods with our mock
    spyOn(localStorage, 'getItem').and.callFake((key: string) => {
      return localStorageMock[key] || null;
    });
    spyOn(localStorage, 'setItem').and.callFake((key: string, value: string) => {
      localStorageMock[key] = value;
    });
    spyOn(localStorage, 'removeItem').and.callFake((key: string) => {
      delete localStorageMock[key];
    });

    TestBed.configureTestingModule({
      providers: [PaymentHistoryService]
    });
    service = TestBed.inject(PaymentHistoryService);
  });

  afterEach(() => {
    // Clear localStorage mock after each test to ensure isolation
    localStorageMock = {};
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  describe('constructor', () => {

    it('should initialize with an empty array if no data in localStorage', () => {
      localStorageMock['razorUpiPaymentHistory'] = ''; 

      // Re-inject the service
      service = TestBed.inject(PaymentHistoryService);

      service.getHistory().subscribe(history => {
        expect(history).toEqual([]);
      });
    });

  });

  describe('getHistory', () => {
    it('should return an Observable of the current history', (done) => {
      // Add a payment to ensure there's data
      service.addPayment(mockPayment);

      service.getHistory().subscribe(history => {
        expect(history.length).toBe(1);
        expect(history[0].orderId).toBe(mockPayment.orderId);
        done();
      });
    });
  });

  describe('addPayment', () => {
    it('should add a new payment to the history', (done) => {
      service.getHistory().subscribe(history => {
        // First, expect an empty history (initial state)
        if (history.length === 0) {
          service.addPayment(mockPayment);
        } else if (history.length === 1) {
          // Then, expect the added payment
          expect(history.length).toBe(1);
          expect(history[0].orderId).toBe(mockPayment.orderId);
          expect(history[0].timestamp).toBeInstanceOf(Date);
          done();
        } else {
          done.fail('History length unexpected');
        }
      });
    });

    it('should add the newest payment to the beginning of the array', (done) => {
      service.addPayment(mockPayment); // First payment
      service.addPayment(mockPayment2); // Second payment

      service.getHistory().subscribe(history => {
        if (history.length === 2) {
          expect(history[0].orderId).toBe(mockPayment2.orderId); // Most recent at index 0
          expect(history[1].orderId).toBe(mockPayment.orderId); // Older at index 1
          done();
        }
      });
    });

    it('should emit the updated history through the BehaviorSubject', (done) => {
      const expectedHistory: PaymentHistoryItem[] = [];
      let callCount = 0;

      service.getHistory().subscribe(history => {
        callCount++;
        if (callCount === 1) { // Initial emission from constructor
          expect(history).toEqual([]);
          service.addPayment(mockPayment);
        } else if (callCount === 2) { // Emission after addPayment
          expect(history.length).toBe(1);
          expect(history[0].orderId).toBe(mockPayment.orderId);
          done();
        }
      });
    });

  });
});