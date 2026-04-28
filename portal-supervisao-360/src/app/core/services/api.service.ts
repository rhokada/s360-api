import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class ApiService {
  private readonly baseUrl = environment.apiUrl;

  constructor(private http: HttpClient) {}

  private getUrl(endpoint: string): string {
    const cleanEndpoint = endpoint.startsWith('/') ? endpoint : `/${endpoint}`;
    return `${this.baseUrl}${cleanEndpoint}`;
  }

  private handleError(error: unknown): Observable<never> {
    let errorMessage = 'Erro desconhecido na requisição';

    if (error && typeof error === 'object' && 'status' in error) {
      const httpError = error as { status: number; error?: { message?: string } };
      switch (httpError.status) {
        case 400:
          errorMessage = httpError.error?.message || 'Requisição inválida';
          break;
        case 401:
          errorMessage = 'Não autorizado. Faça login novamente.';
          break;
        case 403:
          errorMessage = 'Acesso negado';
          break;
        case 404:
          errorMessage = 'Recurso não encontrado';
          break;
        case 500:
          errorMessage = 'Erro interno do servidor';
          break;
        default:
          errorMessage = `Erro HTTP: ${httpError.status}`;
      }
    }

    console.error('[ApiService] Erro:', errorMessage, error);
    return throwError(() => new Error(errorMessage));
  }

  get<T>(endpoint: string): Observable<T> {
    return this.http.get<T>(this.getUrl(endpoint)).pipe(
      catchError(err => this.handleError(err))
    );
  }

  post<T>(endpoint: string, body: unknown): Observable<T> {
    return this.http.post<T>(this.getUrl(endpoint), body).pipe(
      catchError(err => this.handleError(err))
    );
  }

  put<T>(endpoint: string, body: unknown): Observable<T> {
    return this.http.put<T>(this.getUrl(endpoint), body).pipe(
      catchError(err => this.handleError(err))
    );
  }

  delete<T>(endpoint: string): Observable<T> {
    return this.http.delete<T>(this.getUrl(endpoint)).pipe(
      catchError(err => this.handleError(err))
    );
  }

  getWithParams<T>(endpoint: string, params: HttpParams): Observable<T> {
    return this.http.get<T>(this.getUrl(endpoint), { params }).pipe(
      catchError(err => this.handleError(err))
    );
  }

  postWithHeaders<T>(endpoint: string, body: unknown, headers: HttpHeaders): Observable<T> {
    return this.http.post<T>(this.getUrl(endpoint), body, { headers }).pipe(
      catchError(err => this.handleError(err))
    );
  }
}
