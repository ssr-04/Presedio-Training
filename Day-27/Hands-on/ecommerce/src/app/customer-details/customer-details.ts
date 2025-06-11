import { Component, Input } from '@angular/core';

//Interface for customer
export interface Customer
{
  Id: number;
  name: string;
  email: string;
  phone: string;
  address: string;
  bio: string;
  likeCount: number;
  dislikeCount: number;
}

@Component({
  selector: 'app-customer-details',
  standalone: true, 
  imports: [], 
  templateUrl: './customer-details.html',
  styleUrl: './customer-details.css'
})
export class CustomerDetails {
  @Input() customerData!: Customer; // Input prop to receive customer obj (it gets from the parent)

  // State tracking for current user
  currentInteraction: 'none' | 'liked' | 'disliked' = 'none';

  constructor() {}

  onLike(): void {
    if (this.currentInteraction === 'liked') {
      // If already liked, then unliking it
      this.customerData.likeCount--;
      this.currentInteraction = 'none';
    } else if (this.currentInteraction === 'disliked') {
      // If disliked, switching to liked
      this.customerData.dislikeCount--;
      this.customerData.likeCount++;
      this.currentInteraction = 'liked';
    } else {
      // If none, liking it
      this.customerData.likeCount++;
      this.currentInteraction = 'liked';
    }
  }

  onDislike(): void {
    if (this.currentInteraction === 'disliked') {
      // If already disliked, undisliking it
      this.customerData.dislikeCount--;
      this.currentInteraction = 'none';
    } else if (this.currentInteraction === 'liked') {
      // If liked, switching to disliked
      this.customerData.likeCount--;
      this.customerData.dislikeCount++;
      this.currentInteraction = 'disliked';
    } else {
      // If none, disliking it
      this.customerData.dislikeCount++;
      this.currentInteraction = 'disliked';
    }
  }

}
