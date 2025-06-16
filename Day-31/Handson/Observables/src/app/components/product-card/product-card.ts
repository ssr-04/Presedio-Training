import { Component, Input, Pipe, PipeTransform } from '@angular/core';
import { Product } from '../../Models/Product';
import { CommonModule } from '@angular/common';
import { HighlightPipe } from '../../pipes/highlight-pipe';


@Component({
  selector: 'app-product-card',
  imports: [CommonModule, HighlightPipe],
  templateUrl: './product-card.html',
  styleUrl: './product-card.css'
})
export class ProductCard {
  @Input() product!: Product;
  @Input() searchTerm: string = '';
}
