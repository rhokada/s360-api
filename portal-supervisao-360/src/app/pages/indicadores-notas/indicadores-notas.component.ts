import {
  Component, OnInit, OnDestroy,
  ViewChild, ElementRef, NgZone
} from '@angular/core';
import { CommonModule, Location } from '@angular/common';
import { Subject, forkJoin } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { Chart, registerables } from 'chart.js';
import { DashRow, TipoGroup, SuperGroup, DataGroup, VendGroup, PctGroup } from '../../shared/models/indicadores.interfaces';
import { DashNotasRow, NotaJoinRow, NotasFilterState } from '../../shared/models/indicadores-notas.interfaces';
import { IndicadoresService } from '../../core/services/indicadores.service';
import { IndicadoresNotasService } from '../../core/services/indicadores-notas.service';

Chart.register(...registerables);

@Component({
  selector: 'app-indicadores-notas',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './indicadores-notas.component.html',
  styleUrls: ['./indicadores-notas.component.scss']
})
export class IndicadoresNotasComponent implements OnInit, OnDestroy {
  @ViewChild('barChart') barChartRef!: ElementRef<HTMLCanvasElement>;

  private destroy$ = new Subject<void>();
  private chart: Chart | null = null;

  allRows:  DashRow[]    = [];
  allNotas: DashNotasRow[] = [];
  filteredRows: DashRow[] = [];
  isLoading = true;

  filter: NotasFilterState = {
    tiposQuestionario: [], supervisores: [], datas: [], vendedores: [], metricas: [], questoes: []
  };

  tipoGroups:    TipoGroup[]  = [];
  superGroups:   SuperGroup[] = [];
  dataGroups:    DataGroup[]  = [];
  vendGroups:    VendGroup[]  = [];
  metricaGroups: PctGroup[]   = [];

  pctSimTotal  = 0;
  pctNaoTotal  = 0;
  pctNRTotal   = 0;
  pctJustTotal = 0;

  questaoNotasRows:  NotaJoinRow[] = [];
  feedbackFinalRows: NotaJoinRow[] = [];

  constructor(
    private indicadoresService: IndicadoresService,
    private indicadoresNotasService: IndicadoresNotasService,
    private location: Location,
    private zone: NgZone
  ) {}

  ngOnInit(): void {
    forkJoin([
      this.indicadoresService.select(),
      this.indicadoresNotasService.select()
    ]).pipe(takeUntil(this.destroy$))
      .subscribe({
        next: ([rows, notas]) => {
          this.allRows  = rows;
          this.allNotas = notas;
          this.isLoading = false;
          this.applyFilters();
          setTimeout(() => this.drawChart());
        },
        error: () => { this.isLoading = false; }
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
    this.chart?.destroy();
  }

  goBack(): void { this.location.back(); }

  // ─── Filtros ──────────────────────────────────────────────────────────────

  toggle(field: keyof NotasFilterState, value: any, event?: MouseEvent): void {
    const arr = this.filter[field] as any[];
    const idx = arr.indexOf(value);
    if (event?.ctrlKey || event?.metaKey) {
      if (idx >= 0) arr.splice(idx, 1); else arr.push(value);
    } else {
      (this.filter[field] as any[]) = (idx >= 0 && arr.length === 1) ? [] : [value];
    }
    this.applyFilters();
  }

  isSelected(field: keyof NotasFilterState, value: any): boolean {
    return (this.filter[field] as any[]).includes(value);
  }

  applyFilters(): void {
    this.filteredRows = this.filterRows(this.allRows, this.filter);

    const tipoRows    = this.filterRows(this.allRows, { ...this.filter, tiposQuestionario: [] });
    const superRows   = this.filterRows(this.allRows, { ...this.filter, supervisores: [] });
    const dataRows    = this.filterRows(this.allRows, { ...this.filter, datas: [] });
    const vendRows    = this.filterRows(this.allRows, { ...this.filter, vendedores: [] });
    const metricaRows = this.filterRows(this.allRows, { ...this.filter, metricas: [] });
    // questoes não tem card próprio, mas participa do filtro global

    this.computeGroups(tipoRows, superRows, dataRows, vendRows, metricaRows);
    this.computePcts();
    this.computeJoinedRows();
    this.drawChart();
  }

  private filterRows(rows: DashRow[], f: NotasFilterState): DashRow[] {
    return rows.filter(r => {
      if (f.tiposQuestionario.length && !f.tiposQuestionario.includes(r.tipoQuestionario ?? '')) return false;
      if (f.supervisores.length      && !f.supervisores.includes(r.supervisor))                  return false;
      if (f.datas.length             && !f.datas.includes(this.formatDate(r.data)))              return false;
      if (f.vendedores.length        && !f.vendedores.includes(r.idVendedor))                    return false;
      if (f.metricas.length          && !f.metricas.includes(r.metrica ?? ''))                   return false;
      if (f.questoes.length          && !f.questoes.includes(r.questao ?? ''))                   return false;
      return true;
    });
  }

  // ─── Computações ──────────────────────────────────────────────────────────

  private computeGroups(
    tipoRows: DashRow[], superRows: DashRow[], dataRows: DashRow[],
    vendRows: DashRow[], metricaRows: DashRow[]
  ): void {
    const toMap = (rows: DashRow[], key: (r: DashRow) => string) => {
      const m = new Map<string, number>();
      rows.forEach(r => { const k = key(r); m.set(k, (m.get(k) ?? 0) + 1); });
      return m;
    };

    this.tipoGroups  = Array.from(toMap(tipoRows,  r => r.tipoQuestionario ?? '(sem tipo)').entries()).map(([value, count]) => ({ value, count }));
    this.superGroups = Array.from(toMap(superRows, r => r.supervisor       ?? '(sem supervisor)').entries()).map(([value, count]) => ({ value, count }));
    this.dataGroups  = Array.from(toMap(dataRows,  r => this.formatDate(r.data)).entries()).sort(([a], [b]) => a.localeCompare(b)).map(([value, count]) => ({ value, count }));

    const vendMap = new Map<number, VendGroup>();
    vendRows.forEach(r => {
      if (!vendMap.has(r.idVendedor))
        vendMap.set(r.idVendedor, { id: r.idVendedor, codVendedor: r.codVendedor ?? '', vendedor: r.vendedor });
    });
    this.vendGroups = Array.from(vendMap.values()).sort((a, b) => a.vendedor.localeCompare(b.vendedor));

    this.metricaGroups = this.buildPctGroups(metricaRows, r => r.metrica ?? '(sem métrica)');
  }

  private buildPctGroups(rows: DashRow[], keyFn: (r: DashRow) => string): PctGroup[] {
    const map = new Map<string, { sim: number; nao: number; nr: number; just: number; total: number }>();
    rows.forEach(r => {
      const k    = keyFn(r);
      const prev = map.get(k) ?? { sim: 0, nao: 0, nr: 0, just: 0, total: 0 };
      map.set(k, { sim: prev.sim + r.sim, nao: prev.nao + r.nao, nr: prev.nr + r.naoRespondido, just: prev.just + r.justificado, total: prev.total + 1 });
    });
    return Array.from(map.entries()).map(([label, v]) => ({
      label,
      pctSim:  v.total ? Math.round(v.sim  / v.total * 1000) / 10 : 0,
      pctNao:  v.total ? Math.round(v.nao  / v.total * 1000) / 10 : 0,
      pctNR:   v.total ? Math.round(v.nr   / v.total * 1000) / 10 : 0,
      pctJust: v.total ? Math.round(v.just / v.total * 1000) / 10 : 0,
    }));
  }

  private computePcts(): void {
    const rows    = this.filteredRows;
    const total   = rows.length;
    const sumSim  = rows.reduce((s, r) => s + r.sim, 0);
    const sumNao  = rows.reduce((s, r) => s + r.nao, 0);
    const sumNR   = rows.reduce((s, r) => s + r.naoRespondido, 0);
    const sumJust = rows.reduce((s, r) => s + r.justificado, 0);

    this.pctSimTotal  = total ? Math.round(sumSim  / total * 1000) / 10 : 0;
    this.pctNaoTotal  = total ? Math.round(sumNao  / total * 1000) / 10 : 0;
    this.pctNRTotal   = total ? Math.round(sumNR   / total * 1000) / 10 : 0;
    this.pctJustTotal = total ? Math.round(sumJust / total * 1000) / 10 : 0;
  }

  private computeJoinedRows(): void {
    // Agrupa notas por submittedAnswerDetailId, acumulando texto/audio/imagem
    const notaMap = new Map<number, { texto: string | null; audio: string | null; imagem: string | null }>();
    this.allNotas.forEach(n => {
      const prev = notaMap.get(n.submittedAnswerDetailId) ?? { texto: null, audio: null, imagem: null };
      notaMap.set(n.submittedAnswerDetailId, {
        texto:  prev.texto  || n.texto        || null,
        audio:  prev.audio  || n.audioArquivo  || null,
        imagem: prev.imagem || n.imagemArquivo || null,
      });
    });

    const questaoRows:  NotaJoinRow[] = [];
    const feedbackRows: NotaJoinRow[] = [];

    this.filteredRows.forEach(row => {
      const nota = notaMap.get(row.submittedAnswerDetailId);
      if (!nota?.texto) return;

      const joined: NotaJoinRow = {
        submittedAnswerDetailId: row.submittedAnswerDetailId,
        questao:      row.questao,
        metrica:      row.metrica,
        texto:        nota.texto,
        audioArquivo: nota.audio,
        imagemArquivo: nota.imagem,
      };

      questaoRows.push(joined);
      if ((row.metrica ?? '').toUpperCase() === 'FEEDBACK FINAL') feedbackRows.push(joined);
    });

    this.questaoNotasRows  = questaoRows;
    this.feedbackFinalRows = feedbackRows;
  }

  // ─── Gráfico ──────────────────────────────────────────────────────────────

  private drawChart(): void {
    if (!this.barChartRef?.nativeElement) return;

    const chartMap = new Map<string, { sim: number; nao: number; nr: number }>();
    this.filteredRows.forEach(r => {
      const k    = this.formatDate(r.data);
      const prev = chartMap.get(k) ?? { sim: 0, nao: 0, nr: 0 };
      chartMap.set(k, { sim: prev.sim + r.sim, nao: prev.nao + r.nao, nr: prev.nr + r.naoRespondido });
    });

    const labels  = Array.from(chartMap.keys()).sort();
    const simData = labels.map(l => chartMap.get(l)!.sim);
    const naoData = labels.map(l => chartMap.get(l)!.nao);
    const nrData  = labels.map(l => chartMap.get(l)!.nr);

    this.chart?.destroy();
    this.chart = new Chart(this.barChartRef.nativeElement, {
      type: 'bar',
      data: {
        labels,
        datasets: [
          { label: 'SIM', data: simData, backgroundColor: '#22c55e' },
          { label: 'NÃO', data: naoData, backgroundColor: '#ef4444' },
          { label: 'N/R', data: nrData,  backgroundColor: '#94a3b8' }
        ]
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        plugins: { legend: { position: 'top' } },
        scales: { x: { stacked: false }, y: { stacked: false } },
        onClick: (_evt, elements) => {
          if (!elements.length) return;
          const label = labels[elements[0].index];
          this.zone.run(() => this.toggle('datas', label));
        }
      }
    });
  }

  // ─── Helpers ──────────────────────────────────────────────────────────────

  formatDate(iso: string | null | undefined): string {
    if (!iso) return 'Sem data';
    // ISO: 2024-05-12[T...]
    const iso8601 = iso.match(/^(\d{4})-(\d{2})-(\d{2})/);
    if (iso8601) return `${iso8601[3]}/${iso8601[2]}/${iso8601[1]}`;
    // BR com ou sem hora: 12/05/2024[ 00:00:00]
    const br = iso.match(/^(\d{2}\/\d{2}\/\d{4})/);
    if (br) return br[1];
    return iso;
  }

  clearAll(): void {
    this.filter = { tiposQuestionario: [], supervisores: [], datas: [], vendedores: [], metricas: [], questoes: [] };
    this.applyFilters();
  }
}
