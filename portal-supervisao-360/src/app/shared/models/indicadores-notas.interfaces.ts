export interface DashNotasRow {
  submittedAnswerDetailId: number;
  idItem:        string | null;
  tipoItem:      string | null;
  comData:       string | null;
  texto:         string | null;
  audioArquivo:  string | null;
  imagemArquivo: string | null;
}

export interface NotaJoinRow {
  submittedAnswerDetailId: number;
  questao:       string | null;
  metrica:       string | null;
  texto:         string;
  audioArquivo:  string | null;
  imagemArquivo: string | null;
}

export interface NotasFilterState {
  tiposQuestionario: string[];
  supervisores:      string[];
  datas:             string[];
  vendedores:        number[];
  metricas:          string[];
  questoes:          string[];
}
