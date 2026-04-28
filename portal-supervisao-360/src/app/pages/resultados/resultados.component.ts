import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup } from '@angular/forms';
import {
  NgApexchartsModule,
  ApexChart,
  ApexNonAxisChartSeries,
  ApexAxisChartSeries,
  ApexXAxis,
  ApexPlotOptions,
  ApexDataLabels,
  ApexLegend,
  ApexTooltip
} from 'ng-apexcharts';
import { ApiService } from '../../core/services/api.service';
import { StorageService, STORAGE_KEYS } from '../../core/services/storage.service';
import { AnswerData, Seller, Customer, SurveyType } from '../../shared/models/interfaces';
import { showLoading, hideLoading } from '../../shared/components/loading/loading.component';
import { format } from 'date-fns';
import { ptBR } from 'date-fns/locale';

export type DonutChartOptions = {
  series: ApexNonAxisChartSeries;
  chart: ApexChart;
  labels: string[];
  legend: ApexLegend;
  colors: string[];
  dataLabels: ApexDataLabels;
  tooltip: ApexTooltip;
};

export type BarChartOptions = {
  series: ApexAxisChartSeries;
  chart: ApexChart;
  xaxis: ApexXAxis;
  plotOptions: ApexPlotOptions;
  dataLabels: ApexDataLabels;
  colors: string[];
  tooltip: ApexTooltip;
};

@Component({
  selector: 'app-resultados',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, NgApexchartsModule],
  templateUrl: './resultados.component.html',
  styleUrls: ['./resultados.component.scss']
})
export class ResultadosComponent implements OnInit {
  filterForm!: FormGroup;
  answers: AnswerData[] = [];
  filteredAnswers: AnswerData[] = [];
  sellers: Seller[] = [];
  customers: Customer[] = [];
  isLoading = false;
  error = '';

  donutChartOptions: Partial<DonutChartOptions> = {};
  barChartOptions: Partial<BarChartOptions> = {};

  surveyTypeLabels: { [key: string]: string } = {
    'CHECK_ROTA': 'Check de Rota',
    'TREINAMENTO_CAMPO': 'Treinamento Campo',
    'AVALIACAO_MERCADO': 'Avaliação Mercado'
  };

  constructor(
    private fb: FormBuilder,
    private api: ApiService,
    private storage: StorageService
  ) {}

  ngOnInit(): void {
    this.sellers = this.storage.get<Seller[]>(STORAGE_KEYS.SELLERS) || [];
    this.customers = this.storage.get<Customer[]>(STORAGE_KEYS.CUSTOMERS) || [];

    this.filterForm = this.fb.group({
      sellerCode: [''],
      customerCode: [''],
      surveyType: [''],
      startDate: [''],
      endDate: ['']
    });

    this.initCharts();
    this.loadAnswers();
  }

  initCharts(): void {
    this.donutChartOptions = {
      series: [0, 0, 0],
      chart: {
        type: 'donut',
        height: 280,
        fontFamily: 'inherit'
      },
      labels: ['Check de Rota', 'Treinamento Campo', 'Avaliação Mercado'],
      colors: ['#2563eb', '#16a34a', '#9333ea'],
      legend: {
        position: 'bottom'
      },
      dataLabels: {
        enabled: true,
        formatter: (val: number) => `${Math.round(val)}%`
      },
      tooltip: {
        y: {
          formatter: (val: number) => `${val} avaliações`
        }
      }
    };

    this.barChartOptions = {
      series: [{ name: 'Avaliações', data: [] }],
      chart: {
        type: 'bar',
        height: 300,
        fontFamily: 'inherit',
        toolbar: { show: false }
      },
      xaxis: {
        categories: [],
        labels: { style: { fontSize: '12px' } }
      },
      plotOptions: {
        bar: {
          horizontal: false,
          borderRadius: 6,
          columnWidth: '60%'
        }
      },
      dataLabels: { enabled: false },
      colors: ['#2563eb'],
      tooltip: {
        y: { formatter: (val: number) => `${val} avaliações` }
      }
    };
  }

  loadAnswers(): void {
    this.isLoading = true;
    this.error = '';
    showLoading();

    const filters = this.filterForm.value;

    this.api.post<AnswerData[]>('/app/GetAnswersData', filters).subscribe({
      next: (data) => {
        this.answers = data || [];
        this.filteredAnswers = [...this.answers];
        this.updateCharts();
        this.isLoading = false;
        hideLoading();
      },
      error: (err: Error) => {
        this.error = err.message || 'Erro ao carregar resultados.';
        // Tenta carregar do localStorage
        const local = this.storage.get<AnswerData[]>(STORAGE_KEYS.SUBMITTED_ANSWERS) || [];
        this.answers = local as AnswerData[];
        this.filteredAnswers = [...this.answers];
        this.updateCharts();
        this.isLoading = false;
        hideLoading();
      }
    });
  }

  applyFilters(): void {
    const filters = this.filterForm.value;
    this.filteredAnswers = this.answers.filter(a => {
      if (filters.sellerCode && !a.sellerName?.toLowerCase().includes(filters.sellerCode.toLowerCase())) {
        return false;
      }
      if (filters.surveyType && a.surveyType !== filters.surveyType) {
        return false;
      }
      return true;
    });
    this.updateCharts();
  }

  clearFilters(): void {
    this.filterForm.reset();
    this.filteredAnswers = [...this.answers];
    this.updateCharts();
  }

  updateCharts(): void {
    // Donut chart: distribuição por tipo
    const typeCounts = {
      CHECK_ROTA: 0,
      TREINAMENTO_CAMPO: 0,
      AVALIACAO_MERCADO: 0
    };

    this.filteredAnswers.forEach(a => {
      if (a.surveyType in typeCounts) {
        typeCounts[a.surveyType as SurveyType]++;
      }
    });

    this.donutChartOptions = {
      ...this.donutChartOptions,
      series: [typeCounts.CHECK_ROTA, typeCounts.TREINAMENTO_CAMPO, typeCounts.AVALIACAO_MERCADO]
    };

    // Bar chart: avaliações por vendedor
    const sellerCounts: { [key: string]: number } = {};
    this.filteredAnswers.forEach(a => {
      const name = a.sellerName || 'Desconhecido';
      sellerCounts[name] = (sellerCounts[name] || 0) + 1;
    });

    const sellers = Object.keys(sellerCounts).slice(0, 10);
    const counts = sellers.map(s => sellerCounts[s]);

    this.barChartOptions = {
      ...this.barChartOptions,
      series: [{ name: 'Avaliações', data: counts }],
      xaxis: { ...this.barChartOptions.xaxis, categories: sellers }
    };
  }

  formatDate(dateStr: string): string {
    try {
      return format(new Date(dateStr), "dd/MM/yyyy", { locale: ptBR });
    } catch {
      return dateStr;
    }
  }

  getSurveyLabel(type: string): string {
    return this.surveyTypeLabels[type] || type;
  }

  getSurveyBadgeClass(type: string): string {
    const classes: { [key: string]: string } = {
      'CHECK_ROTA': 'badge-info',
      'TREINAMENTO_CAMPO': 'badge-success',
      'AVALIACAO_MERCADO': 'badge bg-purple-100 text-purple-800'
    };
    return classes[type] || 'badge-gray';
  }
}
