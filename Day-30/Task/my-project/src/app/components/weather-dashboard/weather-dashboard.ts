import { Component, inject } from '@angular/core';
import { WeatherService } from '../../services/weather';
import { CommonModule } from '@angular/common';
import { CitySearch } from "../city-search/city-search";
import { WeatherCard } from "../weather-card/weather-card";

@Component({
  selector: 'app-weather-dashboard',
  imports: [CommonModule, CitySearch, WeatherCard],
  templateUrl: './weather-dashboard.html',
  styleUrl: './weather-dashboard.css'
})
export class WeatherDashboard {
  weatherService = inject(WeatherService);

  // Exposing the observables from the service to the template
  weather$ = this.weatherService.weather$;
  error$ = this.weatherService.error$;
  searchHistory$ = this.weatherService.searchHistory$;

  // Method to handle clicks on history items
  onHistoryClick(city: string): void {
    this.weatherService.setCity(city);
  }

}
