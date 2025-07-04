export interface PaymentHistoryItem {
  id: string; // Razorpay Payment ID
  orderId: string;
  amount: number;
  customerName: string;
  email: string;
  contact: string;
  status: 'Success' | 'Failure' | 'Cancelled';
  timestamp: Date;
}