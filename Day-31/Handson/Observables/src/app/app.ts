import { Component, HostListener } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { Header } from "./components/layout/header/header";

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, Header],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
  protected title = 'Observables';

  showBackToTop = false;

  @HostListener('window:scroll')
  onWindowScroll(): void {
    this.showBackToTop = window.scrollY > 400;
  }

  scrollToTop(): void {
    window.scrollTo({top: 0, behavior: 'smooth'});
  }
}
