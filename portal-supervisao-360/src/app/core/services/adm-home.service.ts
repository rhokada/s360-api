import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';

export interface HomeIndicador {
  Label: string;
  valor: string | number | null;
}

export interface HomeIndicadoresData {
  treinamentoCampo?: HomeIndicador;
  ultimoTreinamentoCampo?: HomeIndicador;
  checkRota?: HomeIndicador;
  ultimoCheckRota?: HomeIndicador;
  avaliacaoMercado?: HomeIndicador;
  ultimaAvaliacaoMercado?: HomeIndicador;
  vendedoresTreinamento?: HomeIndicador;
  statusUltimaCarga?: HomeIndicador;
}

@Injectable({ providedIn: 'root' })
export class AdmHomeService {
  constructor(private api: ApiService) {}

  getHomeData(): Observable<HomeIndicadoresData> {
    return this.api.get<HomeIndicadoresData>('/AdmHome/HomeData');
  }
}
