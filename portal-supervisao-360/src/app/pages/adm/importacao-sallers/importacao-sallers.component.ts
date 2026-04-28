import { Component, ElementRef, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import * as XLSX from 'xlsx';
import { ToastService } from '../../../core/services/toast.service';
import { environment } from '../../../../environments/environment';

interface SallersRow {
  ID: string | null;
  CodCliente: string | null;
  NomeFantasia: string | null;
  CNPJ: string | null;
  CodProfissional: string | null;
  Email: string | null;
  Nome: string | null;
  Celular: string | null;
  Whats: string | null;
  CodEquipe: string | null;
  Vendedor: string | null;
  CodSuperior: string | null;
}

type UploadStep = 'idle' | 'preview' | 'uploading' | 'done' | 'error';

@Component({
  selector: 'app-importacao-sallers',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './importacao-sallers.component.html',
  styleUrls: ['./importacao-sallers.component.scss']
})
export class ImportacaoSallersComponent {
  @ViewChild('fileInput') fileInput!: ElementRef<HTMLInputElement>;

  step: UploadStep = 'idle';
  isDragOver = false;
  selectedFile: File | null = null;
  rows: SallersRow[] = [];
  uploadResult: { success: number; errors: string[] } | null = null;
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
    link.download = 'modelo-sallers.xlsx';
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

        const sheet = wb.Sheets['Tabelao'];
        if (!sheet) {
          this.toast.error('O arquivo deve conter a aba "Tabelao".');
          this.reset();
          return;
        }

        this.rows = XLSX.utils.sheet_to_json<SallersRow>(sheet, { defval: null });
        this.step = 'preview';
      } catch {
        this.toast.error('Erro ao ler o arquivo. Verifique se é um Excel válido.');
        this.reset();
      }
    };
    reader.readAsArrayBuffer(file);
  }

  get rowsPreview(): SallersRow[] {
    return this.rows.slice(0, this.maxPreviewRows);
  }

  enviar(): void {
    if (!this.selectedFile) return;

    this.step = 'uploading';
    const formData = new FormData();
    formData.append('file', this.selectedFile, this.selectedFile.name);

    this.http.post<{ success: number; errors: string[] }>(
      `${environment.apiUrl}/DataImportSallers/ImportarPlanilha`,
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
    this.rows = [];
    this.uploadResult = null;
    this.errorMessage = '';
    if (this.fileInput?.nativeElement) {
      this.fileInput.nativeElement.value = '';
    }
  }
}
