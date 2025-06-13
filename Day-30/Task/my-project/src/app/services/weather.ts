import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { BehaviorSubject, Observable, ReplaySubject, of } from 'rxjs';
import { switchMap, catchError, tap, map, shareReplay } from 'rxjs/operators';
import { environment } from '../../environments/environment';


export interface WeatherData {
  name: string;
  temp: number;
  description: string;
  icon: string;
  humidity: number;
  windSpeed: number;
}

@Injectable()
export class WeatherService {
  private http = inject(HttpClient);

  private city$$ = new BehaviorSubject<string>('Coimbatore'); 
  private searchHistory$$ = new ReplaySubject<string[]>(1);
  private history: string[] = [];

  private error$$ = new BehaviorSubject<string | null>(null);
  public readonly error$ = this.error$$.asObservable();
  
  public readonly weather$: Observable<WeatherData | null> = this.city$$.pipe(
    switchMap(city => {
      if (!city) {
        return of(null);
      }
      
      this.error$$.next(null);

      const params = new HttpParams()
        .set('q', city)
        .set('key', environment.weatherApi.key)
        .set('aqi', 'no'); 

     
      return this.http.get<any>(environment.weatherApi.url, { params }).pipe(
        map(response => this.transformResponse(response)),
        tap(weatherData => this.updateSearchHistory(weatherData.name)),
        catchError(err => {
          console.error('API Error:', err);
          const errorMessage = err.status === 400 ? `City "${city}" not found.` : 'An error occurred fetching weather data.';
          this.error$$.next(errorMessage);
          return of(null);
        })
      );
    }),
    shareReplay(1)
  );

  public readonly searchHistory$: Observable<string[]> = this.searchHistory$$.asObservable();

  constructor() {
    this.loadHistoryFromLocalStorage();
    this.setCity(this.city$$.getValue());
  }

  public setCity(city: string): void {
    this.city$$.next(city);
  }

  
  private transformResponse(response: any): WeatherData {
    return {
      name: response.location.name,
      temp: Math.round(response.current.temp_c),
      description: response.current.condition.text,
      
      icon: `https:${response.current.condition.icon}`,
      humidity: response.current.humidity,
      
      windSpeed: Math.round(response.current.wind_kph)
    };
  }

  private updateSearchHistory(city: string): void {
    if (!this.history.includes(city)) {
      this.history.unshift(city);
      this.history = this.history.slice(0, 5);
      this.saveHistoryToLocalStorage();
      this.searchHistory$$.next([...this.history]);
    }
  }

  private loadHistoryFromLocalStorage(): void {
    const storedHistory = localStorage.getItem('weatherSearchHistory');
    if (storedHistory) {
      this.history = JSON.parse(storedHistory);
      this.searchHistory$$.next([...this.history]);
    }
  }

  private saveHistoryToLocalStorage(): void {
    localStorage.setItem('weatherSearchHistory', JSON.stringify(this.history));
  }
}