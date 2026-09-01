import {
  Component, OnInit, OnDestroy,
  ViewChild, ElementRef, NgZone
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { Location } from '@angular/common';
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
export class IndicadoresComponent implements OnInit, OnDestroy {
  @ViewChild('barChart') barChartRef!: ElementRef<HTMLCanvasElement>;

  private destroy$ = new Subject<void>();
  private chart: Chart | null = null;

  allRows: DashRow[] = [];
  filteredRows: DashRow[] = [];
  isLoading = true;

  filter: FilterState = {
    tiposQuestionario: [],
    supervisores: [],
    datas: [],
    vendedores: [],
    clientes: [], 
    grupos: [],
    metricas: []
  };

  tipoGroups: TipoGroup[]   = [];
  superGroups: SuperGroup[] = [];
  dataGroups: DataGroup[]   = [];
  vendGroups: VendGroup[]   = [];
  grupoGroups: PctGroup[]   = [];
  metricaGroups: PctGroup[] = [];
  clienteGroups: TipoGroup[] = [];

  totalDatas = 0;
  totalVendedores = 0;
  totalQuestionarios = 0;
  pctSimTotal  = 0;
  pctNaoTotal  = 0;
  pctNRTotal   = 0;
  pctJustTotal = 0;

  constructor(
    private indicadoresService: IndicadoresService,
    private zone: NgZone,
    private location: Location
  ) {}

  goBack(): void {
    this.location.back();
  }

  ngOnInit(): void {
    this.indicadoresService.select()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: rows => {
          this.allRows = rows;
          this.isLoading = false;
          this.applyFilters();
          // Canvas só entra na DOM após Angular re-renderizar o *ngIf;
          // setTimeout 0 aguarda o próximo ciclo de render para desenhar o gráfico.
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

  // ─── Filtros ──────────────────────────────────────────────────────────────

  toggle(field: keyof FilterState, value: any, event?: MouseEvent): void {
    const arr = this.filter[field] as any[];
    const idx = arr.indexOf(value);

    if (event?.ctrlKey || event?.metaKey) {
      // Ctrl+click: adiciona ou remove do multi-seleção
      if (idx >= 0) arr.splice(idx, 1);
      else arr.push(value);
    } else {
      // Click simples: troca; se já era o único selecionado, limpa
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
    // filteredRows usa todos os filtros (gráfico, totalizadores)
    this.filteredRows = this.filterRows(this.allRows, this.filter);

    // Cada card usa allRows filtrado por TODOS os campos EXCETO o seu próprio,
    // para que o card não se auto-filtre (comportamento Power BI).
    const tipoRows    = this.filterRows(this.allRows, { ...this.filter, tiposQuestionario: [] });
    const superRows   = this.filterRows(this.allRows, { ...this.filter, supervisores: [] });
    const dataRows    = this.filterRows(this.allRows, { ...this.filter, datas: [] });
    const vendRows    = this.filterRows(this.allRows, { ...this.filter, vendedores: [] });
    const clienteRows = this.filterRows(this.allRows, { ...this.filter, clientes: [] });
    const grupoRows   = this.filterRows(this.allRows, { ...this.filter, grupos: [] });
    const metricaRows = this.filterRows(this.allRows, { ...this.filter, metricas: [] });

    this.computeGroups(tipoRows, superRows, dataRows, vendRows, grupoRows, metricaRows);
    this.computeClienteGroups(clienteRows); // NOVO: monta o card CLIENTES
    this.computeMediaPonderada(vendRows);   // NOVO: média ponderada por vendedor
    this.computeTotals();
    this.drawChart();
  }

  private filterRows(rows: DashRow[], f: FilterState): DashRow[] {
    return rows.filter(r => {
      if (f.tiposQuestionario.length && !f.tiposQuestionario.includes(r.tipoQuestionario ?? '')) return false;
      if (f.supervisores.length      && !f.supervisores.includes(r.supervisor))                  return false;
      if (f.datas.length             && !f.datas.includes(this.formatDate(r.data)))              return false;
      if (f.vendedores.length        && !f.vendedores.includes(r.idVendedor))                    return false;
      if (f.clientes.length          && !f.clientes.includes(r.codCliente ?? ''))                return false;
      if (f.grupos.length            && !f.grupos.includes(r.grupo ?? ''))                       return false;
      if (f.metricas.length          && !f.metricas.includes(r.metrica ?? ''))                   return false;
      return true;
    });
  }

  // ─── Computações ──────────────────────────────────────────────────────────

  private computeGroups(
    tipoRows: DashRow[], superRows: DashRow[], dataRows: DashRow[],
    vendRows: DashRow[], grupoRows: DashRow[], metricaRows: DashRow[]
  ): void {
    const toMap = (rows: DashRow[], key: (r: DashRow) => string) => {
      const m = new Map<string, number>();
      rows.forEach(r => { const k = key(r); m.set(k, (m.get(k) ?? 0) + 1); });
      return m;
    };

    this.tipoGroups = Array.from(toMap(tipoRows, r => r.tipoQuestionario ?? '(sem tipo)').entries())
      .map(([value, count]) => ({ value, count }));

    this.superGroups = Array.from(toMap(superRows, r => r.supervisor ?? '(sem supervisor)').entries())
      .map(([value, count]) => ({ value, count }));

    // Datas: armazenar data formatada (filter.datas também usa formatada)
    this.dataGroups = Array.from(toMap(dataRows, r => this.formatDate(r.data)).entries())
      .sort(([a], [b]) => a.localeCompare(b))
      .map(([value, count]) => ({ value, count }));

    const vendMap = new Map<number, VendGroup>();
    vendRows.forEach(r => {
      if (!vendMap.has(r.idVendedor))
        vendMap.set(r.idVendedor, { id: r.idVendedor, codVendedor: r.codVendedor ?? '', vendedor: r.vendedor });
    });
    this.vendGroups = Array.from(vendMap.values()).sort((a, b) => a.vendedor.localeCompare(b.vendedor));

    this.computeMediaPonderada(vendRows);

    this.grupoGroups   = this.buildPctGroups(grupoRows,   r => r.grupo   ?? '(sem grupo)');
    this.metricaGroups = this.buildPctGroups(metricaRows, r => r.metrica ?? '(sem métrica)');
  }

  private computeClienteGroups(rows: DashRow[]): void {
    const map = new Map<string, number>();
    rows.forEach(r => {
      const c = r.codCliente?.trim() || '(sem cliente)';
      map.set(c, (map.get(c) ?? 0) + 1);
    });
    this.clienteGroups = Array.from(map.entries())
      .map(([value, count]) => ({ value, count }))
      .sort((a, b) => a.value.localeCompare(b.value));
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

  // ─── Média ponderada por vendedor ─────────────────────────────────────────
  // Usa SOMENTE respostas cuja pergunta possui weight (peso > 0).
  // Se o vendedor não tiver nenhuma resposta com weight, usa média simples.
  private computeMediaPonderada(vendRows: DashRow[]): void {
    const acc = new Map<number, {
      somaPesos: number;
      somaNotasPonderadas: number;
      total: number;
      somaNotas: number;
    }>();

    vendRows.forEach(r => {
      const nota = r.sim ? 1 : 0; // SIM=1; NÃO/JUSTIFICADO/N-R = 0
      const prev = acc.get(r.idVendedor) ?? { somaPesos: 0, somaNotasPonderadas: 0, total: 0, somaNotas: 0 };

      prev.total += 1;
      prev.somaNotas += nota;

      const peso = r.peso ?? 0; // Question.Weight, vindo da procedure como [PESO]
      if (peso > 0) {
        prev.somaPesos += peso;
        prev.somaNotasPonderadas += nota * peso;
      }

      acc.set(r.idVendedor, prev);
    });

    this.vendGroups.forEach(g => {
      const v = acc.get(g.id);
      if (!v || v.total === 0) { g.mediaPonderada = 0; return; }

      if (v.somaPesos > 0) {
        // Média ponderada: Σ(nota × peso) / Σ(peso), apenas perguntas com weight
        g.mediaPonderada = Math.round(v.somaNotasPonderadas / v.somaPesos * 1000) / 10;
      } else {
        // Fallback: média simples sobre todas as respostas do vendedor
        g.mediaPonderada = Math.round(v.somaNotas / v.total * 1000) / 10;
      }
    });
  }

  private computeTotals(): void {
    const rows = this.filteredRows;
    this.totalDatas         = new Set(rows.map(r => this.formatDate(r.data))).size;
    this.totalVendedores    = new Set(rows.map(r => r.idVendedor)).size;
    this.totalQuestionarios = new Set(rows.map(r => r.idQuestionario)).size;

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
          // labels já são datas formatadas, que é o que filter.datas armazena
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
    this.filter = { tiposQuestionario: [], supervisores: [], datas: [], vendedores: [], clientes: [], grupos: [], metricas: [] };
    this.applyFilters();
  }
}
