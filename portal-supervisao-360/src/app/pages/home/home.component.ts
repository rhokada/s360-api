import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { forkJoin } from 'rxjs';
import { ApiService } from '../../core/services/api.service';
import { AuthService } from '../../core/services/auth.service';
import { StorageService, STORAGE_KEYS } from '../../core/services/storage.service';
import { Alternative, HomeData, Questions, RawAlternative, RawCustomer, RawQuestionsApiResponse, RawQuestionResponseItem, RawSeller, Seller, Customer, User } from '../../shared/models/interfaces';
import { showLoading, hideLoading } from '../../shared/components/loading/loading.component';
import { format } from 'date-fns';
import { ptBR } from 'date-fns/locale';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.scss']
})
export class HomeComponent implements OnInit {
  currentUser: User | null = null;
  homeData: HomeData | null = null;
  isSyncing = false;
  syncSuccess = '';
  syncError = '';
  isLoadingData = false;

  surveyTypes = [
    { key: 'CHECK_ROTA', label: 'Check de Rota', color: 'blue' },
    { key: 'TREINAMENTO_CAMPO', label: 'Treinamento Campo', color: 'green' },
    { key: 'AVALIACAO_MERCADO', label: 'Avaliação Mercado', color: 'purple' }
  ];

  constructor(
    private authService: AuthService,
    private api: ApiService,
    private storage: StorageService
  ) {}

  ngOnInit(): void {
    this.currentUser = this.authService.getUser();
    this.loadHomeData();
  }

  loadHomeData(): void {
    this.isLoadingData = true;
    showLoading();

    this.api.post<any>('/app/AppUserHomeData', {}).subscribe({
      next: (data) => {
        this.homeData = data[0] || null;
        this.isLoadingData = false;
        hideLoading();
      },
      error: (err: Error) => {
        console.error('Erro ao carregar dados da home:', err);
        // Dados mockados para desenvolvimento
        this.homeData = {
          companyName: 'Minha Empresa',
          totalAvaliacoes: 0,
          pendenciasAbertas: 0,
          recentAvaliacoes: []
        };
        this.isLoadingData = false;
        hideLoading();
      }
    });
  }

  syncData(): void {
    this.isSyncing = true;
    this.syncSuccess = '';
    this.syncError = '';
    showLoading();

    forkJoin({
      sellers: this.api.post<RawSeller[]>('/app/AppSupSellersList', { Structure: STORAGE_KEYS.SELLERS }),
      customers: this.api.post<RawCustomer[]>('/app/AppSupCustomersList', { Structure: STORAGE_KEYS.CUSTOMERS }),
      questions: this.api.post<RawQuestionsApiResponse[]>('/app/AppSupQuestionsList', { Structure: STORAGE_KEYS.QUESTIONS })
    }).subscribe({
      next: ({ sellers, customers, questions }) => {
        const mappedSellers: Seller[] = sellers.map(raw => ({
          id: raw.SellerId,
          code: raw.CompanyCodeUser,
          name: raw.Name
        }));
        this.storage.set(STORAGE_KEYS.SELLERS, mappedSellers);
        const mappedCustomers: Customer[] = customers.map(raw => ({
          id: raw.CustomerId,
          code: raw.CustomerCode,
          name: raw.CustomerName
        }));
        this.storage.set(STORAGE_KEYS.CUSTOMERS, mappedCustomers);

        const rawQuestions: RawQuestionResponseItem[] = JSON.parse(questions[0]?.RetJson ?? '[]');
        const mappedQuestions: Questions[] = rawQuestions.map(raw => {
          const alternatives: Alternative[] = (raw.alternatives ?? []).map((rawAlt: RawAlternative) => ({
            id: rawAlt.QuestionOptionId.toString(),
            text: rawAlt.text
          }));
          return {
            SurveyType: raw.SurveyType,
            id: raw.QuestionId.toString(),
            type: raw.type,
            text: raw.text,
            QuestionIsComplement: raw.QuestionIsComplement,
            QuestionGroup: raw.QuestionGroup,
            Metric: raw.Metric,
            IconMetric: raw.IconMetric,
            IsFirstSurvey: raw.IsFirstSurvey,
            IsFinalSurvey: raw.IsFinalSurvey,
            IsFinishEarly: raw.IsFinishEarly,
            IsCompetenceLevel: raw.IsCompetenceLevel,
            alternatives,
            Rank: raw.Rank
          };
        });

        this.storage.set(STORAGE_KEYS.QUESTIONS, mappedQuestions);

        this.syncSuccess = `Sincronização concluída! ${mappedSellers.length} vendedores, ${mappedCustomers.length} clientes e ${mappedQuestions.length} questões importadas.`;
        this.isSyncing = false;
        hideLoading();
        setTimeout(() => this.syncSuccess = '', 5000);
      },
      error: (err: Error) => {
        this.syncError = err.message || 'Erro ao sincronizar dados. Tente novamente.';
        this.isSyncing = false;
        hideLoading();
        setTimeout(() => this.syncError = '', 5000);
      }
    });
  }

  formatDate(dateStr: string): string {
    try {
      return format(new Date(dateStr), "dd 'de' MMM, yyyy", { locale: ptBR });
    } catch {
      return dateStr;
    }
  }

  getSurveyLabel(type: string): string {
    const found = this.surveyTypes.find(t => t.key === type);
    return found?.label || type;
  }

  getSurveyColor(type: string): string {
    const found = this.surveyTypes.find(t => t.key === type);
    return found?.color || 'gray';
  }

  getGreeting(): string {
    const hour = new Date().getHours();
    if (hour < 12) return 'Bom dia';
    if (hour < 18) return 'Boa tarde';
    return 'Boa noite';
  }
}
