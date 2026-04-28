import { Component, ElementRef, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import * as XLSX from 'xlsx';
import { ToastService } from '../../../core/services/toast.service';
import { environment } from '../../../../environments/environment';

interface ProfissionalRow {
  ID: string | null;
  CodProfissional: string | null;
  Email: string | null;
  Nome: string | null;
  Celular: string | null;
  Whats: string | null;
  CodEquipe: string | null;
  Vendedor: string | null;
  CodSuperior: string | null;
}

interface ClienteRow {
  ID: string | null;
  CodCliente: string | null;
  NomeFantasia: string | null;
  CNPJ: string | null;
  CodVendedor: string | null;
}

interface PreviewData {
  profissionais: ProfissionalRow[];
  clientes: ClienteRow[];
}

type UploadStep = 'idle' | 'preview' | 'uploading' | 'done' | 'error';

@Component({
  selector: 'app-importacao-profissionais',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './importacao-profissionais.component.html',
  styleUrls: ['./importacao-profissionais.component.scss']
})
export class ImportacaoProfissionaisComponent {
  @ViewChild('fileInput') fileInput!: ElementRef<HTMLInputElement>;

  step: UploadStep = 'idle';
  isDragOver = false;
  selectedFile: File | null = null;
  preview: PreviewData | null = null;
  uploadResult: { success: number; errors: string[] } | null = null;
  activeTab: 'profissionais' | 'clientes' = 'profissionais';
  errorMessage = '';

  readonly modeloUrl = '/assets/modelo.xlsx';
  readonly maxPreviewRows = 5;

  constructor(
    private http: HttpClient,
    private toast: ToastService
  ) {}

  downloadModelo(): void {
    const link = document.createElement('a');
    link.href = this.modeloUrl;
    link.download = 'modelo.xlsx';
    link.click();
  }

  openFileDialog(): void {
    this.fileInput.nativeElement.click();
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files?.length) {
      this.processFile(input.files[0]);
    }
  }

  onDragOver(event: DragEvent): void {
    event.preventDefault();
    this.isDragOver = true;
  }

  onDragLeave(): void {
    this.isDragOver = false;
  }

  onDrop(event: DragEvent): void {
    event.preventDefault();
    this.isDragOver = false;
    const file = event.dataTransfer?.files[0];
    if (file) this.processFile(file);
  }

  private processFile(file: File): void {
    if (!file.name.endsWith('.xlsx') && !file.name.endsWith('.xls')) {
      this.toast.error('Apenas arquivos .xlsx ou .xls são aceitos.');
      return;
    }

    this.selectedFile = file;
    this.errorMessage = '';

    const reader = new FileReader();
    reader.onload = (e) => {
      try {
        const data = new Uint8Array(e.target!.result as ArrayBuffer);
        const wb = XLSX.read(data, { type: 'array' });

        const profSheet = wb.Sheets['Profissionais'];
        const cliSheet = wb.Sheets['Clientes'];

        if (!profSheet || !cliSheet) {
          this.toast.error('O arquivo deve conter as abas "Profissionais" e "Clientes".');
          this.reset();
          return;
        }

        const profRows = XLSX.utils.sheet_to_json<ProfissionalRow>(profSheet, { defval: null });
        const cliRows = XLSX.utils.sheet_to_json<ClienteRow>(cliSheet, { defval: null });

        this.preview = { profissionais: profRows, clientes: cliRows };
        this.step = 'preview';
      } catch {
        this.toast.error('Erro ao ler o arquivo. Verifique se é um Excel válido.');
        this.reset();
      }
    };
    reader.readAsArrayBuffer(file);
  }

  get profPreview(): ProfissionalRow[] {
    return this.preview?.profissionais.slice(0, this.maxPreviewRows) ?? [];
  }

  get cliPreview(): ClienteRow[] {
    return this.preview?.clientes.slice(0, this.maxPreviewRows) ?? [];
  }

  enviar(): void {
    if (!this.selectedFile || !this.preview) return;

    this.step = 'uploading';
    const formData = new FormData();
    formData.append('file', this.selectedFile, this.selectedFile.name);

    this.http.post<{ success: number; errors: string[] }>(
      `${environment.apiUrl}/DataImportProLog/ImportarPlanilha`,
      formData
    ).subscribe({
      next: (res) => {
        this.uploadResult = res;
        this.step = 'done';
        this.toast.success(`Importação concluída: ${res.success} registro(s) processado(s).`);
      },
      error: (err) => {
        this.errorMessage = err?.error?.message ?? 'Erro ao enviar o arquivo para o servidor.';
        this.step = 'error';
        this.toast.error('Falha na importação. Verifique os detalhes abaixo.');
      }
    });
  }

  reset(): void {
    this.step = 'idle';
    this.selectedFile = null;
    this.preview = null;
    this.uploadResult = null;
    this.errorMessage = '';
    if (this.fileInput?.nativeElement) {
      this.fileInput.nativeElement.value = '';
    }
  }
}
