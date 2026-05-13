import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { TranscricaoAudioService } from '../../../core/services/transcricao-audio.service';
import { ToastService } from '../../../core/services/toast.service';
import { TranscricaoAudioItem, TranscricaoSentence } from '../../../shared/models/adm.interfaces';

type AiAnalysisValue = string | string[] | Record<string, any>[];

@Component({
  selector: 'app-adm-transcricao-audio',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './adm-transcricao-audio.component.html',
  styleUrls: ['./adm-transcricao-audio.component.scss']
})
export class AdmTranscricaoAudioComponent implements OnInit, OnDestroy {
  records: TranscricaoAudioItem[] = [];
  isLoading    = false;
  modalVisible = false;
  activeTab: 'transcricao' | 'analise' = 'transcricao';
  selected: TranscricaoAudioItem | null = null;
  sentences: TranscricaoSentence[] = [];
  aiAnalysisData: Record<string, AiAnalysisValue> | null = null;
  speakerMap = new Map<string, number>();
  private destroy$ = new Subject<void>();

  constructor(
    private transcricaoAudioService: TranscricaoAudioService,
    private toastService: ToastService
  ) {}

  ngOnInit(): void {
    this.loadRecords();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadRecords(): void {
    this.isLoading = true;
    this.transcricaoAudioService.select().pipe(takeUntil(this.destroy$)).subscribe({
      next: (data) => { this.records = data; this.isLoading = false; },
      error: () => { this.toastService.error('Erro ao carregar transcrições.'); this.isLoading = false; }
    });
  }

  openModal(item: TranscricaoAudioItem): void {
    this.selected     = item;
    this.activeTab    = 'transcricao';
    this.speakerMap   = new Map();
    this.sentences    = this.parseTranscricao(item.audioTranscription);
    this.aiAnalysisData = this.parseAiAnalysis(item.aiAnalysis);
    this.modalVisible = true;
  }

  closeModal(): void {
    this.modalVisible = false;
    this.selected = null;
  }

  private parseTranscricao(json: string | null): TranscricaoSentence[] {
    if (!json) return [];
    try {
      const parsed = JSON.parse(json);
      const sentences: any[] = Array.isArray(parsed)
        ? parsed
        : (parsed.sentences ?? parsed.transcript ?? []);
      return sentences.map((s: any) => ({
        speakerName: s.speaker_name ?? s.speakerName ?? s.speaker ?? 'Falante',
        text:        s.text ?? s.raw_text ?? '',
        startTime:   s.start_time ?? s.startTime ?? undefined
      }));
    } catch {
      return [{ speakerName: 'Transcrição', text: json }];
    }
  }

  private parseAiAnalysis(json: string | null): Record<string, AiAnalysisValue> | null {
    if (!json) return null;
    try {
      return JSON.parse(json);
    } catch {
      return { resultado: json };
    }
  }

  getAiAnalysisKeys(): string[] {
    return this.aiAnalysisData ? Object.keys(this.aiAnalysisData) : [];
  }

  getAnalysisValue(key: string): AiAnalysisValue {
    return this.aiAnalysisData ? this.aiAnalysisData[key] : '';
  }

  isString(val: any): val is string {
    return typeof val === 'string';
  }

  isStringArray(val: any): val is string[] {
    return Array.isArray(val) && val.every(v => typeof v === 'string');
  }

  isObjectArray(val: any): val is Record<string, any>[] {
    return Array.isArray(val) && val.length > 0 && typeof val[0] === 'object';
  }

  getObjectArrayKeys(val: Record<string, any>[]): string[] {
    return val.length > 0 ? Object.keys(val[0]) : [];
  }

  isFirstSpeaker(name: string): boolean {
    if (!this.speakerMap.has(name)) {
      this.speakerMap.set(name, this.speakerMap.size);
    }
    return this.speakerMap.get(name) === 0;
  }

  truncate(text: string | null, len = 60): string {
    if (!text) return '-';
    return text.length > len ? text.substring(0, len) + '...' : text;
  }
}
