import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { BehaviorSubject, Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

// Serviço de loading global - pode ser importado em qualquer lugar
export const loadingState$ = new BehaviorSubject<boolean>(false);

export function showLoading(): void {
  loadingState$.next(true);
}

export function hideLoading(): void {
  loadingState$.next(false);
}

@Component({
  selector: 'app-loading',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './loading.component.html',
  styleUrls: ['./loading.component.scss']
})
export class LoadingComponent implements OnInit, OnDestroy {
  isLoading = false;
  private destroy$ = new Subject<void>();

  ngOnInit(): void {
    loadingState$.pipe(
      takeUntil(this.destroy$)
    ).subscribe(state => {
      this.isLoading = state;
    });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }
}
