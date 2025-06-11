import { Component } from '@angular/core';
import { First } from "./first/first";

@Component({
  selector: 'app-root',
  templateUrl: './app.html',
  styleUrl: './app.css',
  imports: [First]
})
export class App {
  protected title = 'myApp';
}