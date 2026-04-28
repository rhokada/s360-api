import { Injectable } from '@angular/core';

export const STORAGE_KEYS = {
  TOKEN: 'S360TokenAuth',
  USER_DATA: 'user_data',
  SELECTED_ROLE: 's360_selected_role',
  REMEMBER_EMAIL: 'rememberEmail',
  QUESTIONS: 's360_questions',
  FUPS: 's360_fups',
  SELLERS: 's360_sellers',
  CUSTOMERS: 's360_customers',
  PARTIAL_ANSWERS: 's360_partialAnswers',
  SUBMITTED_ANSWERS: 's360_submittedAnswers'
} as const;

@Injectable({
  providedIn: 'root'
})
export class StorageService {
  set(key: string, value: unknown): void {
    try {
      const serialized = JSON.stringify(value);
      localStorage.setItem(key, serialized);
    } catch (error) {
      console.error(`[StorageService] Erro ao salvar chave "${key}":`, error);
    }
  }

  get<T>(key: string): T | null {
    try {
      const item = localStorage.getItem(key);
      if (item === null) return null;
      return JSON.parse(item) as T;
    } catch (error) {
      console.error(`[StorageService] Erro ao ler chave "${key}":`, error);
      return null;
    }
  }

  getString(key: string): string | null {
    return localStorage.getItem(key);
  }

  setString(key: string, value: string): void {
    localStorage.setItem(key, value);
  }

  remove(key: string): void {
    localStorage.removeItem(key);
  }

  clear(): void {
    localStorage.clear();
  }

  /**
   * Limpa apenas as chaves do S360, preservando outras entradas
   */
  clearAppData(): void {
    Object.values(STORAGE_KEYS).forEach(key => {
      localStorage.removeItem(key);
    });
  }
}
