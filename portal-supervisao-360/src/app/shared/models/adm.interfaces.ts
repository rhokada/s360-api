export interface AdmUser {
  userId: number;
  appId: string | null;
  name: string;
  email: string;
  dddCell: string | null;
  nrCell: string | null;
  active: boolean | null;
  dhCreate: string | null;
  dhUpdate: string | null;
  contractId: number | null;
  pbiLogin: string | null;
  title: string | null;
  companyCodeUser: string | null;
}

export interface AdmHierarchy {
  hierarchyId: number;
  name: string;
  dhCreate: string | null;
  dhUpdate: string | null;
}

export interface AdmHierarchyTeam {
  hierarchyTeamId: number;
  hierarchyId: number;
  hierarchyName: string;
  userId: number;
  userName: string;
  userEmail: string;
  bossId: number;
  bossName: string;
  bossEmail: string;
  active: boolean | null;
  dhCreate: string | null;
  dhUpdate: string | null;
  deptUserId: number | null;
  companyDeptId: number | null;
  title: string | null;
  companyCodeUser: string | null;
  deptName: string | null;
  companyId: number | null;
  companyName: string | null;
}

export interface TreeNode {
  userId: number;
  name: string;
  email: string;
  title: string | null;
  companyName: string | null;
  deptName: string | null;
  isRoot: boolean;
  members: AdmHierarchyTeam[];
  children: TreeNode[];
}

export interface AdmAddress {
  addressId: number;
  street: string | null;
  street2: string | null;
  neighborhood: string | null;
  city: string | null;
  state: string | null;
  zipCode: string | null;
  country: string | null;
  dhUpdate: string | null;
}

export interface AdmCompany {
  companyId: number;
  addressId: number | null;
  groupCompanyId: number | null;
  name: string;
  taxID: string | null;
  logoUrl: string | null;
  dhCreate: string | null;
  dhUpdate: string | null;
  parentCompanyId: number | null;
  city: string | null;
  state: string | null;
}

export interface AdmCompanyDept {
  companyDeptId: number;
  companyId: number;
  companyName: string;
  addressId: number | null;
  name: string;
  profitCenter: string | null;
  costCenter: string | null;
  dhUpdate: string | null;
  city: string | null;
  state: string | null;
}

export interface AdmDeptUser {
  deptUserId: number;
  userId: number;
  userName: string;
  userEmail: string;
  companyDeptId: number;
  deptName: string;
  companyName: string;
  title: string | null;
  companyCodeUser: string | null;
  dhUpdate: string | null;
}

export interface Customer {
  customerId: number;
  companyId: number;
  companyName: string;
  toBeConfirmed: boolean | null;
  customerCode: string | null;
  name: string;
  cnpj: string | null;
  street: string | null;
  street2: string | null;
  neighborhood: string | null;
  city: string | null;
  state: string | null;
  zipCode: string | null;
  dhCreate: string | null;
  dhUpdate: string | null;
  log: string | null;
  guId: string | null;
  originCd: string | null;
  dataImportId: number | null;
  totalCount?: number;
}

export interface CustomerSeller {
  customerSellerId: number;
  customerId: number;
  customerName: string;
  customerCode: string | null;
  sellerUserId: number;
  sellerName: string;
  sellerEmail: string;
  active: boolean;
  dhCreate: string | null;
  dhUpdate: string | null;
  log: string | null;
}

export interface AdmPage {
  admPageId: number;
  slug: string;
  menu: string;
  icon: string | null;
  dhCreate: string | null;
  dhUpdate: string | null;
}

export interface AdmRoleItem {
  admRoleId: number;
  admRoleCd: string;
  admRoleName: string;
  dhCreate: string | null;
  dhUpdate: string | null;
}

export interface AdmRolePermission {
  admRolePermissionId: number;
  admRoleId: number;
  admPageId: number;
  slug: string;
  menu: string;
  icon: string | null;
  read: boolean;
  create: boolean;
  delete: boolean;
  alter: boolean;
}

export interface AdmRoleUser {
  admRoleUserId: number;
  admRoleId: number;
  admRoleName: string;
  admRoleCd: string;
  userId: number;
  userName: string;
  userEmail: string;
  dhCreate: string | null;
}

export interface AiPromptItem {
  aiPromptId: number;
  aiProcessCd: string;
  context: string | null;
  prompt: string | null;
  engine: string | null;
  dhcreate: string | null;
  active: boolean;
  log: string | null;
}

export interface TranscricaoAudioItem {
  submittedAnswerDetailId: number;
  alternativeId: number | null;
  answerText: string | null;
  questionGroup: string | null;
  metric: string | null;
  audioTranscription: string | null;
  aiAnalysis: string | null;
}

export interface TranscricaoSentence {
  speakerName: string;
  text: string;
  startTime?: number;
}
