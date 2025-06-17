import { Component, Input, Pipe, PipeTransform } from '@angular/core';
import { Product } from '../../Models/Product';
import { CommonModule } from '@angular/common';
import { HighlightPipe } from '../../pipes/highlight-pipe';
import { RouterLink } from '@angular/router';


@Component({
  selector: 'app-product-card',
  imports: [CommonModule, HighlightPipe, RouterLink],
  templateUrl: './product-card.html',
  styleUrl: './product-card.css'
})
export class ProductCard {
  @Input() product!: Product;
  @Input() searchTerm: string = '';
}
