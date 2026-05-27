import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../core/services/auth.service';
import { AdmHomeService, HomeIndicador, HomeIndicadoresData } from '../../core/services/adm-home.service';
import { User } from '../../shared/models/interfaces';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.scss']
})
export class HomeComponent implements OnInit {
  currentUser: User | null = null;
  indicadores: HomeIndicadoresData | null = null;
  loading = false;
  errorMessage = '';

  constructor(
    private authService: AuthService,
    private admHomeService: AdmHomeService
  ) {}

  ngOnInit(): void {
    this.currentUser = this.authService.getUser();
    this.loadData();
  }

  loadData(): void {
    this.loading = true;
    this.errorMessage = '';
    this.admHomeService.getHomeData().subscribe({
      next: data => {
        this.indicadores = data;
        this.loading = false;
      },
      error: err => {
        this.errorMessage = err.message;
        this.loading = false;
      }
    });
  }

  get cards(): HomeIndicador[] {
    if (!this.indicadores) return [];
    return Object.values(this.indicadores).filter((v): v is HomeIndicador => v != null);
  }

  getGreeting(): string {
    const hour = new Date().getHours();
    if (hour < 12) return 'Bom dia';
    if (hour < 18) return 'Boa tarde';
    return 'Boa noite';
  }

  firstName(): string {
    return (this.currentUser?.name ?? 'Usuário').split(' ')[0];
  }
}
