import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { WeatherService } from '../../services/weather';

@Component({
  selector: 'app-city-search',
  imports: [FormsModule],
  templateUrl: './city-search.html',
  styleUrl: './city-search.css'
})
export class CitySearch {
  private weatherService = inject(WeatherService);
  cityName: string = '';

  search(): void {
    if (this.cityName.trim()) {
      this.weatherService.setCity(this.cityName.trim());
      this.cityName = ''; // Clear the input after search
    }
  }

}
