import {
  Component, OnInit, OnDestroy, AfterViewInit,
  ViewChild, ElementRef, NgZone
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { Chart, registerables } from 'chart.js';
import {
  DashRow, FilterState,
  TipoGroup, SuperGroup, DataGroup, VendGroup, PctGroup
} from '../../shared/models/indicadores.interfaces';
import { IndicadoresService } from '../../core/services/indicadores.service';

Chart.register(...registerables);

@Component({
  selector: 'app-indicadores',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './indicadores.component.html',
  styleUrls: ['./indicadores.component.scss']
})
export class IndicadoresComponent implements OnInit, AfterViewInit, OnDestroy {
  @ViewChild('barChart') barChartRef!: ElementRef<HTMLCanvasElement>;

  private destroy$ = new Subject<void>();
  private chart: Chart | null = null;
  private viewReady = false;
  private pendingChartDraw = false;

  allRows: DashRow[] = [];
  filteredRows: DashRow[] = [];
  isLoading = true;

  filter: FilterState = {
    tiposQuestionario: [],
    supervisores: [],
    datas: [],
    vendedores: [],
    grupos: [],
    metricas: []
  };

  // Grupos derivados
  tipoGroups: TipoGroup[]   = [];
  superGroups: SuperGroup[] = [];
  dataGroups: DataGroup[]   = [];
  vendGroups: VendGroup[]   = [];
  grupoGroups: PctGroup[]   = [];
  metricaGroups: PctGroup[] = [];

  // Totalizadores
  totalDatas = 0;
  totalVendedores = 0;
  totalQuestionarios = 0;
  pctSimTotal = 0;
  pctNaoTotal = 0;
  pctNRTotal = 0;

  constructor(
    private indicadoresService: IndicadoresService,
    private zone: NgZone
  ) {}

  ngOnInit(): void {
    this.indicadoresService.select()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: rows => {
          this.allRows = rows;
          this.isLoading = false;
          this.applyFilters();
          if (this.viewReady) {
            this.drawChart();
          } else {
            this.pendingChartDraw = true;
          }
        },
        error: () => { this.isLoading = false; }
      });
  }

  ngAfterViewInit(): void {
    this.viewReady = true;
    if (this.pendingChartDraw) {
      this.pendingChartDraw = false;
      this.drawChart();
    }
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
    this.chart?.destroy();
  }

  // ─── Filtros ──────────────────────────────────────────────────────────────

  toggle(field: keyof FilterState, value: any, event?: MouseEvent): void {
    const arr = this.filter[field] as any[];
    const idx = arr.indexOf(value);
    if (event?.ctrlKey || event?.metaKey) {
      if (idx >= 0) arr.splice(idx, 1);
      else arr.push(value);
    } else {
      if (idx >= 0 && arr.length === 1) {
        (this.filter[field] as any[]) = [];
      } else {
        (this.filter[field] as any[]) = [value];
      }
    }
    this.applyFilters();
  }

  isSelected(field: keyof FilterState, value: any): boolean {
    return (this.filter[field] as any[]).includes(value);
  }

  applyFilters(): void {
    this.filteredRows = this.allRows.filter(r => {
      if (this.filter.tiposQuestionario.length && !this.filter.tiposQuestionario.includes(r.tipoQuestionario ?? '')) return false;
      if (this.filter.supervisores.length    && !this.filter.supervisores.includes(r.supervisor))          return false;
      if (this.filter.datas.length           && !this.filter.datas.includes(r.data ?? ''))                 return false;
      if (this.filter.vendedores.length      && !this.filter.vendedores.includes(r.idVendedor))            return false;
      if (this.filter.grupos.length          && !this.filter.grupos.includes(r.grupo ?? ''))               return false;
      if (this.filter.metricas.length        && !this.filter.metricas.includes(r.metrica ?? ''))           return false;
      return true;
    });

    this.computeGroups();
    this.computeTotals();
    if (this.viewReady) this.drawChart();
  }

  // ─── Computações ──────────────────────────────────────────────────────────

  private computeGroups(): void {
    const rows = this.filteredRows;

    // Tipo Questionário
    const tipoMap = new Map<string, number>();
    rows.forEach(r => {
      const k = r.tipoQuestionario ?? '(sem tipo)';
      tipoMap.set(k, (tipoMap.get(k) ?? 0) + 1);
    });
    this.tipoGroups = Array.from(tipoMap.entries()).map(([value, count]) => ({ value, count }));

    // Supervisor
    const superMap = new Map<string, number>();
    rows.forEach(r => {
      const k = r.supervisor ?? '(sem supervisor)';
      superMap.set(k, (superMap.get(k) ?? 0) + 1);
    });
    this.superGroups = Array.from(superMap.entries()).map(([value, count]) => ({ value, count }));

    // Data
    const dataMap = new Map<string, number>();
    rows.forEach(r => {
      const k = r.data ? this.formatDate(r.data) : 'Sem data';
      dataMap.set(k, (dataMap.get(k) ?? 0) + 1);
    });
    this.dataGroups = Array.from(dataMap.entries())
      .sort(([a], [b]) => a.localeCompare(b))
      .map(([value, count]) => ({ value, count }));

    // Vendedor
    const vendMap = new Map<number, VendGroup>();
    rows.forEach(r => {
      if (!vendMap.has(r.idVendedor)) {
        vendMap.set(r.idVendedor, { id: r.idVendedor, codVendedor: r.codVendedor ?? '', vendedor: r.vendedor });
      }
    });
    this.vendGroups = Array.from(vendMap.values()).sort((a, b) => a.vendedor.localeCompare(b.vendedor));

    // Grupo (%SIM / %NÃO / %NR)
    this.grupoGroups = this.buildPctGroups(rows, r => r.grupo ?? '(sem grupo)');

    // Métrica
    this.metricaGroups = this.buildPctGroups(rows, r => r.metrica ?? '(sem métrica)');
  }

  private buildPctGroups(rows: DashRow[], keyFn: (r: DashRow) => string): PctGroup[] {
    const map = new Map<string, { sim: number; nao: number; nr: number; total: number }>();
    rows.forEach(r => {
      const k = keyFn(r);
      const prev = map.get(k) ?? { sim: 0, nao: 0, nr: 0, total: 0 };
      map.set(k, {
        sim:   prev.sim   + r.sim,
        nao:   prev.nao   + r.nao,
        nr:    prev.nr    + r.naoRespondido,
        total: prev.total + 1
      });
    });
    return Array.from(map.entries()).map(([label, v]) => ({
      label,
      pctSim: v.total ? Math.round(v.sim  / v.total * 1000) / 10 : 0,
      pctNao: v.total ? Math.round(v.nao  / v.total * 1000) / 10 : 0,
      pctNR:  v.total ? Math.round(v.nr   / v.total * 1000) / 10 : 0,
    }));
  }

  private computeTotals(): void {
    const rows = this.filteredRows;
    this.totalDatas         = new Set(rows.map(r => r.data)).size;
    this.totalVendedores    = new Set(rows.map(r => r.idVendedor)).size;
    this.totalQuestionarios = new Set(rows.map(r => r.idQuestionario)).size;

    const total = rows.length;
    const sumSim = rows.reduce((s, r) => s + r.sim, 0);
    const sumNao = rows.reduce((s, r) => s + r.nao, 0);
    const sumNR  = rows.reduce((s, r) => s + r.naoRespondido, 0);

    this.pctSimTotal = total ? Math.round(sumSim / total * 1000) / 10 : 0;
    this.pctNaoTotal = total ? Math.round(sumNao / total * 1000) / 10 : 0;
    this.pctNRTotal  = total ? Math.round(sumNR  / total * 1000) / 10 : 0;
  }

  // ─── Gráfico ──────────────────────────────────────────────────────────────

  private drawChart(): void {
    if (!this.barChartRef) return;

    const dataMap = new Map<string, { sim: number; nao: number; nr: number }>();
    this.filteredRows.forEach(r => {
      const k = r.data ? this.formatDate(r.data) : 'Sem data';
      const prev = dataMap.get(k) ?? { sim: 0, nao: 0, nr: 0 };
      dataMap.set(k, { sim: prev.sim + r.sim, nao: prev.nao + r.nao, nr: prev.nr + r.naoRespondido });
    });

    const labels   = Array.from(dataMap.keys()).sort();
    const simData  = labels.map(l => dataMap.get(l)!.sim);
    const naoData  = labels.map(l => dataMap.get(l)!.nao);
    const nrData   = labels.map(l => dataMap.get(l)!.nr);

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
          const rawDate = this.findRawDateByFormatted(label);
          if (rawDate) this.zone.run(() => this.toggle('datas', rawDate));
        }
      }
    });
  }

  private findRawDateByFormatted(formatted: string): string | null {
    const found = this.filteredRows.find(r => r.data && this.formatDate(r.data) === formatted);
    return found?.data ?? null;
  }

  // ─── Helpers ──────────────────────────────────────────────────────────────

  formatDate(iso: string | null): string {
    if (!iso) return 'Sem data';
    const d = new Date(iso);
    if (isNaN(d.getTime())) return iso;
    return d.toLocaleDateString('pt-BR');
  }

  clearAll(): void {
    this.filter = { tiposQuestionario: [], supervisores: [], datas: [], vendedores: [], grupos: [], metricas: [] };
    this.applyFilters();
  }
}
