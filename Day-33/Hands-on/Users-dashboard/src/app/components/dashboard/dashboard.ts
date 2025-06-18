import { Component, OnInit, inject, signal, computed, effect, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Chart, registerables, ChartConfiguration } from 'chart.js';
import { UserService } from '../../services/user';
import { UserFiltersModel, UserModel } from '../../Models/user.model';
import { Loader } from "../shared/loader/loader";
import { FilterMenuComponent } from "../users/filter-menu/filter-menu";

Chart.register(...registerables);

@Component({
  selector: 'app-dashboard',
  imports: [CommonModule, Loader, FilterMenuComponent],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.css'
})
export class Dashboard implements OnInit, OnDestroy {

  private userService = inject(UserService);

  // originalUsers -> the complete dataset from the initial load.
  // allUsers -> the currently displayed (filtered) users.
  private originalUsers = signal<UserModel[]>([]);
  private allUsers = signal<UserModel[]>([]);

  filters = signal<UserFiltersModel>({});
  isLoading = signal<boolean>(true);

  // Charts
  private genderChart?: Chart;
  private ageChart?: Chart;
  private stateChart?: Chart;

  // Using originalUsers to compute filter options so that they don’t change post filtering.
  uniqueStates = computed(() => [...new Set(this.originalUsers().map(u => u.address.state))].sort());
  uniqueUniversities = computed(() => [...new Set(this.originalUsers().map(u => u.university))].sort());
  uniqueDepartments = computed(() => [...new Set(this.originalUsers().map(u => u.company.department))].sort());

  // Filtering applies to the current data (allUsers -> filtered)
  filteredUsers = computed(() => {
    const users = this.allUsers();
    const currentFilters = this.filters();
    if (Object.keys(currentFilters).length === 0) {
      return users;
    }
    return users.filter(user => {
      return Object.entries(currentFilters).every(([key, value]) => {
        if (!value) return true;
        if (key === 'gender') return user.gender.toLowerCase() === value.toLowerCase();
        if (key === 'state') return user.address.state === value;
        if (key === 'university') return user.university === value;
        if (key === 'department') return user.company.department === value;
        return true;
      });
    });
  });

  constructor() {
    // Reactively update charts -> filteredUsers changes.
    effect(() => {
      const users = this.filteredUsers();
      if (users.length > 0) {
        this.createGenderChart(users);
        this.createAgeDistributionChart(users);
        this.createStateChart(users);
      }
    });
  }

  ngOnInit(): void {
    this.userService.getUsers({}, 0, 0).subscribe({
      next: (response) => {
        // Saves the initial dataset in both signals.
        this.originalUsers.set(response.users);
        this.allUsers.set(response.users);
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error("Failed to load users for dashboard", err);
        this.isLoading.set(false);
      }
    });
  }

  ngOnDestroy(): void {
    this.genderChart?.destroy();
    this.ageChart?.destroy();
    this.stateChart?.destroy();
  }

  onFilterChange(newFilters: UserFiltersModel): void {
    this.filters.update((existing) => ({ ...existing, ...newFilters }));
    this.isLoading.set(true);
    this.userService.getUsers(this.filters(), 0, 0).subscribe({
      next: (response) => {
        // Updates only the displayed users while preserving the original dataset.
        this.allUsers.set(response.users);
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error("Failed to load filtered users", err);
        this.isLoading.set(false);
      }
    });
  }

  private createGenderChart(users: UserModel[]): void {
    const genderData = users.reduce((acc, user) => {
      acc[user.gender] = (acc[user.gender] || 0) + 1;
      return acc;
    }, {} as { [key: string]: number });
    
    const config: ChartConfiguration = {
      type: 'doughnut',
      data: {
        labels: Object.keys(genderData),
        datasets: [{
          label: 'Gender Distribution',
          data: Object.values(genderData),
          backgroundColor: ['rgba(54, 162, 235, 0.7)', 'rgba(255, 99, 132, 0.7)'],
          borderColor: ['rgba(54, 162, 235, 1)', 'rgba(255, 99, 132, 1)'],
          borderWidth: 1
        }]
      },
      options: { responsive: true, maintainAspectRatio: false }
    };

    this.renderChart('genderChart', config, 'genderChart');
  }

  private createAgeDistributionChart(users: UserModel[]): void {
    const ageGroups = { '18-30': 0, '31-40': 0, '41-50': 0, '51+': 0 };
    users.forEach(user => {
      if (user.age <= 30) ageGroups['18-30']++;
      else if (user.age <= 40) ageGroups['31-40']++;
      else if (user.age <= 50) ageGroups['41-50']++;
      else ageGroups['51+']++;
    });

    const config: ChartConfiguration = {
      type: 'bar',
      data: {
        labels: Object.keys(ageGroups),
        datasets: [{
          label: 'Users by Age Group',
          data: Object.values(ageGroups),
          backgroundColor: 'rgba(75, 192, 192, 0.7)',
          borderColor: 'rgba(75, 192, 192, 1)',
          borderWidth: 1
        }]
      },
      options: { responsive: true, maintainAspectRatio: false }
    };
    
    this.renderChart('ageChart', config, 'ageChart');
  }

  private createStateChart(users: UserModel[]): void {
    const stateData = users.reduce((acc, user) => {
      const state = user.address.state;
      acc[state] = (acc[state] || 0) + 1;
      return acc;
    }, {} as { [key: string]: number });

    // Get top 10 states for a cleaner chart.
    const topStates = Object.entries(stateData)
      .sort(([, a], [, b]) => b - a)
      .slice(0, 10);

    const config: ChartConfiguration = {
        type: 'polarArea',
        data: {
            labels: topStates.map(c => c[0]),
            datasets: [{
                label: 'Users by State',
                data: topStates.map(c => c[1]),
                backgroundColor: [
                    'rgba(255, 99, 132, 0.7)', 'rgba(75, 192, 192, 0.7)', 'rgba(255, 205, 86, 0.7)',
                    'rgba(201, 203, 207, 0.7)', 'rgba(54, 162, 235, 0.7)', 'rgba(153, 102, 255, 0.7)',
                    'rgba(255, 159, 64, 0.7)', 'rgba(23, 162, 184, 0.7)', 'rgba(40, 167, 69, 0.7)',
                    'rgba(220, 53, 69, 0.7)'
                ]
            }]
        },
        options: { responsive: true, maintainAspectRatio: false }
    };
    this.renderChart('stateChart', config, 'stateChart');
  }

  private renderChart(canvasId: string, config: ChartConfiguration, chartInstance: 'genderChart' | 'ageChart' | 'stateChart'): void {
    this[chartInstance]?.destroy();

    const canvas = document.getElementById(canvasId) as HTMLCanvasElement;
    if (!canvas) {
        // If the canvas isn’t available, wait a tick and try again.
        setTimeout(() => {
            this.renderChart(canvasId, config, chartInstance);
        }, 0);
        return;
    }

    const ctx = canvas.getContext('2d');
    if (!ctx) {
        console.error(`Failed to get 2D context from canvas element with id ${canvasId}`);
        return;
    }

    this[chartInstance] = new Chart(ctx, config);
  }
}