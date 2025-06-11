import { Component } from '@angular/core';
import { Products } from './products/products';

@Component({
  selector: 'app-root',
  templateUrl: './app.html',
  styleUrl: './app.css',
  imports: [Products]
})
export class App {
  protected title = 'myApp';
}