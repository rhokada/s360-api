export interface DashRow {
  submittedAnswerDetailId: number;
  idQuestionario: number;
  tipoQuestionario: string;
  data: string | null;
  supervisor: string;
  idUsuario: number;
  codVendedor: string | null;
  idVendedor: number;
  vendedor: string;
  codCliente: string | null;
  questao: string | null;
  rank: number | null;
  metricaPadrao: boolean;
  tipoSN: boolean;
  nivelCompetencia: boolean;       // NOVO: [Nível de competencia] (q.IsCompetenceLevel)
  terminoAntecipado: boolean;      // NOVO: [Término antecipado] (q.IsFinishEarly)
  feedback: boolean;               // NOVO: [Feedback] (q.IsFeedback)
  tipoCompetencia: string | null;  // NOVO: [Tipo de Competencia] — 'INICIAL' | 'FINAL'
  resposta: string | null;
  sim: number;
  nao: number;
  naoRespondido: number;
  justificado: number;
  grupo: string | null;
  metrica: string | null;
  peso: number | null; 
}

export interface FilterState {
  tiposQuestionario: string[];
  supervisores: string[];
  datas: string[];
  vendedores: number[];
  clientes: string[];  
  grupos: string[];
  metricas: string[];
}

export interface TipoGroup   { value: string; count: number; }
export interface SuperGroup  { value: string; count: number; }
export interface DataGroup   { value: string; count: number; }
export interface VendGroup   { id: number; codVendedor: string; vendedor: string; mediaPonderada?: number;}
export interface PctGroup    { label: string; pctSim: number; pctNao: number; pctNR: number; pctJust: number; }
