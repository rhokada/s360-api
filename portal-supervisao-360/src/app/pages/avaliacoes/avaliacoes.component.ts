import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { StorageService, STORAGE_KEYS } from '../../core/services/storage.service';
import { Seller, Customer, SurveyType, PartialFormState } from '../../shared/models/interfaces';

interface SurveyCard {
  type: SurveyType;
  label: string;
  description: string;
  icon: string;
  color: string;
  bgColor: string;
  borderColor: string;
}

@Component({
  selector: 'app-avaliacoes',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './avaliacoes.component.html',
  styleUrls: ['./avaliacoes.component.scss']
})
export class AvaliacoesComponent implements OnInit {
  avaliacaoForm!: FormGroup;
  selectedSurveyType: SurveyType | null = null;
  sellers: Seller[] = [];
  customers: Customer[] = [];
  filteredSellers: Seller[] = [];
  filteredCustomers: Customer[] = [];
  selectedSeller: Seller | null = null;
  selectedCustomer: Customer | null = null;
  showSellerDropdown = false;
  showCustomerDropdown = false;
  sellerSearchText = '';
  customerSearchText = '';
  formError = '';

  surveyCards: SurveyCard[] = [
    {
      type: 'CHECK_ROTA',
      label: 'Check de Rota',
      description: 'Verificação da rota de vendas e atividades do vendedor em campo.',
      icon: 'M9 20l-5.447-2.724A1 1 0 013 16.382V5.618a1 1 0 011.447-.894L9 7m0 13l6-3m-6 3V7m6 10l4.553 2.276A1 1 0 0021 18.382V7.618a1 1 0 00-.553-.894L15 4m0 13V4m0 0L9 7',
      color: 'text-blue-600',
      bgColor: 'bg-blue-50',
      borderColor: 'border-blue-300'
    },
    {
      type: 'TREINAMENTO_CAMPO',
      label: 'Treinamento em Campo',
      description: 'Avaliação de desempenho durante treinamentos realizados em campo.',
      icon: 'M12 6.253v13m0-13C10.832 5.477 9.246 5 7.5 5S4.168 5.477 3 6.253v13C4.168 18.477 5.754 18 7.5 18s3.332.477 4.5 1.253m0-13C13.168 5.477 14.754 5 16.5 5c1.747 0 3.332.477 4.5 1.253v13C19.832 18.477 18.247 18 16.5 18c-1.746 0-3.332.477-4.5 1.253',
      color: 'text-green-600',
      bgColor: 'bg-green-50',
      borderColor: 'border-green-300'
    },
    {
      type: 'AVALIACAO_MERCADO',
      label: 'Avaliação de Mercado',
      description: 'Análise das condições de mercado, concorrência e oportunidades.',
      icon: 'M16 8v8m-4-5v5m-4-2v2m-2 4h12a2 2 0 002-2V6a2 2 0 00-2-2H6a2 2 0 00-2 2v12a2 2 0 002 2z',
      color: 'text-purple-600',
      bgColor: 'bg-purple-50',
      borderColor: 'border-purple-300'
    }
  ];

  constructor(
    private fb: FormBuilder,
    private storage: StorageService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.avaliacaoForm = this.fb.group({
      sellerSearch: [''],
      customerSearch: ['']
    });

    this.sellers = this.storage.get<Seller[]>(STORAGE_KEYS.SELLERS) || [];
    this.customers = this.storage.get<Customer[]>(STORAGE_KEYS.CUSTOMERS) || [];
    this.filteredSellers = [...this.sellers];
    this.filteredCustomers = [...this.customers];
  }

  selectSurveyType(type: SurveyType): void {
    this.selectedSurveyType = type;
    this.formError = '';
  }

  filterSellers(text: string): void {
    this.sellerSearchText = text;
    this.showSellerDropdown = text.length > 0;
    if (!text) {
      this.filteredSellers = [...this.sellers];
      return;
    }
    const lower = text.toLowerCase();
    this.filteredSellers = this.sellers.filter(s =>
      s.name.toLowerCase().includes(lower) ||
      s.code.toLowerCase().includes(lower)
    );
  }

  selectSeller(seller: Seller): void {
    this.selectedSeller = seller;
    this.sellerSearchText = `${seller.code} - ${seller.name}`;
    this.showSellerDropdown = false;
  }

  filterCustomers(text: string): void {
    this.customerSearchText = text;
    this.showCustomerDropdown = text.length > 0;
    if (!text) {
      this.filteredCustomers = [...this.customers];
      return;
    }
    const lower = text.toLowerCase();
    this.filteredCustomers = this.customers.filter(c =>
      c.name.toLowerCase().includes(lower) ||
      c.code.toLowerCase().includes(lower)
    );
  }

  selectCustomer(customer: Customer): void {
    this.selectedCustomer = customer;
    this.customerSearchText = `${customer.code} - ${customer.name}`;
    this.showCustomerDropdown = false;
  }

  clearSeller(): void {
    this.selectedSeller = null;
    this.sellerSearchText = '';
    this.filteredSellers = [...this.sellers];
    this.filteredCustomers = [...this.customers];
  }

  clearCustomer(): void {
    this.selectedCustomer = null;
    this.customerSearchText = '';
  }

  hideDropdowns(): void {
    setTimeout(() => {
      this.showSellerDropdown = false;
      this.showCustomerDropdown = false;
    }, 200);
  }

  iniciarAvaliacao(): void {
    if (!this.selectedSurveyType) {
      this.formError = 'Selecione o tipo de avaliação para continuar.';
      return;
    }

    if (!this.selectedSeller) {
      this.formError = 'Selecione um vendedor para continuar.';
      return;
    }

    this.formError = '';

    // Salva estado parcial
    const partialState: PartialFormState = {
      sellerId: this.selectedSeller.id,
      sellerName: this.selectedSeller.name,
      sellerCode: this.selectedSeller.code,
      customerId: this.selectedCustomer?.id,
      customerName: this.selectedCustomer?.name,
      customerCode: this.selectedCustomer?.code,
      answers: {},
      DtSurvey: new Date().toISOString(),
      SurveyType: this.selectedSurveyType
    };

    this.storage.set(STORAGE_KEYS.PARTIAL_ANSWERS, partialState);

    this.router.navigate(['/questionario'], {
      queryParams: { type: this.selectedSurveyType }
    });
  }

  getSurveyCard(type: SurveyType): SurveyCard | undefined {
    return this.surveyCards.find(c => c.type === type);
  }
}
