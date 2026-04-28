import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss']
})
export class LoginComponent implements OnInit {
  loginForm!: FormGroup;
  recoverForm!: FormGroup;
  isLoading = false;
  showPassword = false;
  errorMessage = '';
  successMessage = '';
  showRecoverModal = false;
  isRecovering = false;
  recoverError = '';
  recoverSuccess = '';

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router
  ) {}

  ngOnInit(): void {
    // Redirecionar se já logado
    if (this.authService.isLoggedIn()) {
      this.router.navigate(['/home']);
      return;
    }

    const rememberedEmail = this.authService.getRememberedEmail();

    this.loginForm = this.fb.group({
      userName: [rememberedEmail || '', [Validators.required, Validators.minLength(3)]],
      password: ['', [Validators.required, Validators.minLength(4)]],
      rememberEmail: [!!rememberedEmail]
    });

    this.recoverForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]]
    });
  }

  onSubmit(): void {
    if (this.loginForm.invalid) {
      this.loginForm.markAllAsTouched();
      return;
    }

    this.isLoading = true;
    this.errorMessage = '';

    const { userName, password, rememberEmail } = this.loginForm.value;

    this.authService.login(userName, password).subscribe({
      next: () => {
        if (rememberEmail) {
          this.authService.rememberEmail(userName);
        } else {
          this.authService.forgetEmail();
        }
        const user = this.authService.getUser();
        const roles = user?.roles ?? [];
        if (roles.length > 1) {
          this.router.navigate(['/select-role']);
        } else {
          this.router.navigate(['/home']);
        }
      },
      error: (err: Error) => {
        this.errorMessage = err.message || 'Usuário ou senha incorretos';
        this.isLoading = false;
      }
    });
  }

  openRecoverModal(): void {
    this.showRecoverModal = true;
    this.recoverError = '';
    this.recoverSuccess = '';
    const userName = this.loginForm.get('userName')?.value;
    if (userName && userName.includes('@')) {
      this.recoverForm.patchValue({ email: userName });
    }
  }

  closeRecoverModal(): void {
    this.showRecoverModal = false;
  }

  onRecover(): void {
    if (this.recoverForm.invalid) {
      this.recoverForm.markAllAsTouched();
      return;
    }

    this.isRecovering = true;
    this.recoverError = '';
    this.recoverSuccess = '';

    const { email } = this.recoverForm.value;

    this.authService.recoverPassword(email).subscribe({
      next: () => {
        this.recoverSuccess = 'Uma senha temporária foi enviada para seu email.';
        this.isRecovering = false;
        setTimeout(() => this.closeRecoverModal(), 3000);
      },
      error: (err: Error) => {
        this.recoverError = err.message || 'Erro ao recuperar senha. Verifique o email informado.';
        this.isRecovering = false;
      }
    });
  }

  togglePasswordVisibility(): void {
    this.showPassword = !this.showPassword;
  }

  hasError(field: string, error: string): boolean {
    const control = this.loginForm.get(field);
    return !!(control?.hasError(error) && control?.touched);
  }
}
