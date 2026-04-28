export type SurveyType = 'CHECK_ROTA' | 'TREINAMENTO_CAMPO' | 'AVALIACAO_MERCADO';

export type QuestionType = 'SGL' | 'MLT' | 'VLR' | 'TXT';

export interface Alternative {
  id: string;
  text: string;
  value?: number;
  triggersComplementQuestion?: boolean;
}

export interface Questions {
  SurveyType: string;
  id: string;
  text: string;
  type: QuestionType;
  QuestionIsComplement: boolean;
  QuestionGroup: string;
  Metric: string;
  IconMetric: string;
  IsFirstSurvey?: boolean;
  IsFinalSurvey?: boolean;
  IsCompetenceLevel?: boolean;
  alternatives?: Alternative[];
  IsFinishEarly?: boolean;
  Rank?: number;
}

export interface QuestionResponse {
  alternativeId?: string;
  value?: string;
  text?: string;
  notes?: string;
  selectedMultipleAlternativesIds?: string[];
  selectedTexts?: string[];
  QuestionGroup?: string;
  Metric?: string;
}

export interface PartialFormState {
  customerId?: number;
  customerName?: string;
  customerCode?: string;
  sellerId?: number;
  sellerName?: string;
  sellerCode?: string;
  answers: { [questionId: string]: QuestionResponse };
  DtSurvey?: string;
  SurveyType?: string;
}

export interface FupItem {
  id: string;
  category: string;
  description: string;
  dtExpectedConclusion: string;
  dtConclusion: string | null;
  status: 'pending' | 'completed' | 'postponed';
  priority: 'low' | 'medium' | 'high';
  sellerCode?: string;
  customerCode?: string;
  postponementCount: number;
}

export interface RolePermission {
  slug: string;
  menu: string;
  icon: string;
  read: boolean;
  create: boolean;
  delete: boolean;
  alter: boolean;
}

export interface AdmRole {
  admRoleId: number;
  admRoleCd: string;
  admRoleName: string;
  rolePermissions: RolePermission[];
}

export interface User {
  id: number;
  name: string;
  email: string;
  token: string;
  roles: AdmRole[];
}

export interface RawSeller {
  SellerId: number;
  Name: string;
  CompanyCodeUser: string;
}

export interface Seller {
  id: number;
  code: string;
  name: string;
  email?: string;
}

export interface RawCustomer {
  CustomerId: number;
  CustomerCode: string;
  CustomerName: string;
  ToBeConfirmed: boolean;
}

export interface Customer {
  id: number;
  code: string;
  name: string;
  sellerId?: number;
}

export interface HomeData {
  companyName: string;
  companyLogo?: string;
  totalAvaliacoes: number;
  pendenciasAbertas: number;
  recentAvaliacoes: RecentAvaliacao[];
}

export interface RecentAvaliacao {
  id: string;
  type: SurveyType;
  sellerName: string;
  customerName?: string;
  date: string;
  score?: number;
}

export interface AnswerData {
  id: string;
  surveyType: SurveyType;
  sellerId: number;
  sellerName: string;
  customerId?: number;
  customerName?: string;
  dtSurvey: string;
  answers: { [questionId: string]: QuestionResponse };
  score?: number;
}

export interface AuthResponse {
  token: string;
  userId: number;
  name: string;
  email: string;
  roles: AdmRole[];
  roleId: number | null;
  companyId: number | null;
  password: string | null;
  msg: string | null;
}

export interface ApiResponse<T> {
  data: T;
  success: boolean;
  message?: string;
}

export interface SubmitAnswersPayload {
  SurveyType: SurveyType;
  sellerId?: number;
  sellerCode?: string;
  customerId?: number;
  customerCode?: string;
  DtSurvey: string;
  answers: { [questionId: string]: QuestionResponse };
}

export interface GetAnswersPayload {
  sellerCode?: string;
  customerCode?: string;
  surveyType?: SurveyType;
  startDate?: string;
  endDate?: string;
}

export interface ImportDataResponse {
  sellers: Seller[];
  customers: Customer[];
  questions: Questions[];
}

export interface RawAlternative {
  QuestionOptionId: number;
  text: string;
  Description?: string;
  ComplementQuestionId?: number;
  OpenMsgBox?: boolean;
  NeedNotes?: boolean;
}

export interface RawQuestionsApiResponse {
  RetJson: string;
}

export interface RawQuestionResponseItem {
  SurveyType: string;
  QuestionId: number;
  type: QuestionType;
  text: string;
  QuestionIsComplement: boolean;
  QuestionGroup: string;
  Metric: string;
  IconMetric: string;
  IsFirstSurvey?: boolean;
  IsFinalSurvey?: boolean;
  IsFinishEarly?: boolean;
  IsCompetenceLevel?: boolean;
  alternatives?: RawAlternative[];
  Rank?: number;
}
