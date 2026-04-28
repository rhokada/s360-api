import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable, tap } from 'rxjs';
import { jwtDecode } from 'jwt-decode';
import { ApiService } from './api.service';
import { StorageService, STORAGE_KEYS } from './storage.service';
import { AdmRole, AuthResponse, User } from '../../shared/models/interfaces';
import { environment } from '../../../environments/environment';

interface JwtPayload {
  exp: number;
  iat: number;
  sub?: string;
  nameid?: string;
  email?: string;
  role?: string;
  unique_name?: string;
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  currentUser$ = new BehaviorSubject<User | null>(null);
  selectedRole$ = new BehaviorSubject<AdmRole | null>(null);

  constructor(
    private api: ApiService,
    private storage: StorageService
  ) {
    this.loadUserFromStorage();
  }

  private loadUserFromStorage(): void {
    const userData = this.storage.get<User>(STORAGE_KEYS.USER_DATA);
    if (userData && !this.isTokenExpired()) {
      this.currentUser$.next(userData);
      const selectedRole = this.storage.get<AdmRole>(STORAGE_KEYS.SELECTED_ROLE);
      if (selectedRole) {
        this.selectedRole$.next(selectedRole);
      }
    } else if (userData) {
      this.logout();
    }
  }

  login(userName: string, password: string): Observable<AuthResponse> {
    const payload = {
      userName,
      password,
      AppId: environment.appId
    };

    return this.api.post<AuthResponse>('/users/authenticate-adm', payload).pipe(
      tap((response: AuthResponse) => {
        const user: User = {
          id: response.userId,
          name: response.name,
          email: response.email,
          token: response.token,
          roles: response.roles ?? []
        };
        this.storage.setString(STORAGE_KEYS.TOKEN, response.token);
        this.storage.set(STORAGE_KEYS.USER_DATA, user);
        this.currentUser$.next(user);

        // Se tiver apenas uma role, selecioná-la automaticamente
        if (user.roles?.length === 1) {
          this.selectRole(user.roles[0]);
        }
      })
    );
  }

  selectRole(role: AdmRole): void {
    this.storage.set(STORAGE_KEYS.SELECTED_ROLE, role);
    this.selectedRole$.next(role);
  }

  getSelectedRole(): AdmRole | null {
    return this.selectedRole$.getValue();
  }

  getSelectedRolePermissions() {
    return this.getSelectedRole()?.rolePermissions ?? [];
  }

  hasRoutePermission(slug: string): boolean {
    const permissions = this.getSelectedRolePermissions();
    const permission = permissions.find(p => p.slug === slug);
    if (!permission) return false;
    return permission.read || permission.create || permission.delete || permission.alter;
  }

  logout(): void {
    this.storage.remove(STORAGE_KEYS.TOKEN);
    this.storage.remove(STORAGE_KEYS.USER_DATA);
    this.storage.remove(STORAGE_KEYS.SELECTED_ROLE);
    this.currentUser$.next(null);
    this.selectedRole$.next(null);
  }

  isLoggedIn(): boolean {
    const token = this.getToken();
    return !!token && !this.isTokenExpired();
  }

  isTokenExpired(): boolean {
    const token = this.getToken();
    if (!token) return true;

    try {
      const decoded = jwtDecode<JwtPayload>(token);
      const currentTime = Math.floor(Date.now() / 1000);
      return decoded.exp < currentTime;
    } catch {
      return true;
    }
  }

  getToken(): string | null {
    return this.storage.getString(STORAGE_KEYS.TOKEN);
  }

  getUser(): User | null {
    return this.currentUser$.getValue();
  }

  changePassword(oldPassword: string, newPassword: string): Observable<unknown> {
    const user = this.getUser();
    return this.api.post('/users/ChangePassword', {
      email: user?.email,
      oldPassword,
      newPassword
    });
  }

  recoverPassword(email: string): Observable<unknown> {
    return this.api.post('/users/NewTempPassword', {
      username: email,
      AppId: environment.appId
    });
  }

  getPermissions(): Observable<unknown> {
    return this.api.post('/users/GetPermissions', {
      AppId: environment.appId
    });
  }

  rememberEmail(email: string): void {
    this.storage.setString(STORAGE_KEYS.REMEMBER_EMAIL, email);
  }

  getRememberedEmail(): string | null {
    return this.storage.getString(STORAGE_KEYS.REMEMBER_EMAIL);
  }

  forgetEmail(): void {
    this.storage.remove(STORAGE_KEYS.REMEMBER_EMAIL);
  }
}
