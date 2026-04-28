import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators, AbstractControl, ValidationErrors } from '@angular/forms';
import { AuthService } from '../../core/services/auth.service';

function passwordMatchValidator(control: AbstractControl): ValidationErrors | null {
  const newPassword = control.get('newPassword');
  const confirmPassword = control.get('confirmPassword');

  if (newPassword && confirmPassword && newPassword.value !== confirmPassword.value) {
    return { passwordMismatch: true };
  }
  return null;
}

@Component({
  selector: 'app-trocar-senha',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './trocar-senha.component.html',
  styleUrls: ['./trocar-senha.component.scss']
})
export class TrocarSenhaComponent {
  passwordForm: FormGroup;
  isLoading = false;
  successMessage = '';
  errorMessage = '';
  showOldPassword = false;
  showNewPassword = false;
  showConfirmPassword = false;

  constructor(
    private fb: FormBuilder,
    private authService: AuthService
  ) {
    this.passwordForm = this.fb.group({
      oldPassword: ['', [Validators.required, Validators.minLength(4)]],
      newPassword: ['', [Validators.required, Validators.minLength(6)]],
      confirmPassword: ['', Validators.required]
    }, { validators: passwordMatchValidator });
  }

  onSubmit(): void {
    if (this.passwordForm.invalid) {
      this.passwordForm.markAllAsTouched();
      return;
    }

    const { oldPassword, newPassword } = this.passwordForm.value;

    this.isLoading = true;
    this.errorMessage = '';
    this.successMessage = '';

    this.authService.changePassword(oldPassword, newPassword).subscribe({
      next: () => {
        this.successMessage = 'Senha alterada com sucesso!';
        this.isLoading = false;
        this.passwordForm.reset();
      },
      error: (err: Error) => {
        this.errorMessage = err.message || 'Erro ao alterar a senha. Verifique a senha atual.';
        this.isLoading = false;
      }
    });
  }

  hasError(field: string, error: string): boolean {
    const control = this.passwordForm.get(field);
    return !!(control?.hasError(error) && control?.touched);
  }

  hasFormError(error: string): boolean {
    return !!(this.passwordForm.hasError(error) &&
      this.passwordForm.get('confirmPassword')?.touched);
  }
}
