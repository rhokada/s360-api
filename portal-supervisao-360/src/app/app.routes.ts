import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  {
    path: '',
    redirectTo: '/home',
    pathMatch: 'full'
  },
  {
    path: 'login',
    loadComponent: () =>
      import('./pages/login/login.component').then(m => m.LoginComponent)
  },
  {
    path: 'select-role',
    loadComponent: () =>
      import('./pages/select-role/select-role.component').then(m => m.SelectRoleComponent),
    canActivate: [authGuard]
  },
  {
    path: 'home',
    loadComponent: () =>
      import('./pages/home/home.component').then(m => m.HomeComponent),
    canActivate: [authGuard]
  },
  {
    path: 'avaliacoes',
    loadComponent: () =>
      import('./pages/avaliacoes/avaliacoes.component').then(m => m.AvaliacoesComponent),
    canActivate: [authGuard]
  },
  {
    path: 'questionario',
    loadComponent: () =>
      import('./pages/questionario/questionario.component').then(m => m.QuestionarioComponent),
    canActivate: [authGuard]
  },
  {
    path: 'resultados',
    loadComponent: () =>
      import('./pages/resultados/resultados.component').then(m => m.ResultadosComponent),
    canActivate: [authGuard]
  },
  {
    path: 'pendencias',
    loadComponent: () =>
      import('./pages/pendencias/pendencias.component').then(m => m.PendenciasComponent),
    canActivate: [authGuard]
  },
  {
    path: 'trocar-senha',
    loadComponent: () =>
      import('./pages/trocar-senha/trocar-senha.component').then(m => m.TrocarSenhaComponent),
    canActivate: [authGuard]
  },
  {
    path: 'suporte',
    loadComponent: () =>
      import('./pages/suporte/suporte.component').then(m => m.SuporteComponent),
    canActivate: [authGuard]
  },
  {
    path: 'survey-type',
    loadComponent: () =>
      import('./pages/survey-type/survey-type.component').then(m => m.SurveyTypeComponent),
    canActivate: [authGuard],
    data: { slug: 'survey-type' }
  },
  {
    path: 'survey',
    loadComponent: () =>
      import('./pages/survey/survey.component').then(m => m.SurveyComponent),
    canActivate: [authGuard],
    data: { slug: 'survey' }
  },
  {
    path: 'question',
    loadComponent: () =>
      import('./pages/question/question.component').then(m => m.QuestionComponent),
    canActivate: [authGuard],
    data: { slug: 'question' }
  },
  {
    path: 'indicadores',
    loadComponent: () =>
      import('./pages/indicadores/indicadores.component').then(m => m.IndicadoresComponent),
    canActivate: [authGuard]
  },
  {
    path: 'indicadores-notas',
    loadComponent: () =>
      import('./pages/indicadores-notas/indicadores-notas.component').then(m => m.IndicadoresNotasComponent),
    canActivate: [authGuard]
  },
  {
    path: 'unauthorized',
    loadComponent: () =>
      import('./pages/unauthorized/unauthorized.component').then(m => m.UnauthorizedComponent),
    canActivate: [authGuard]
  },
  {
    path: 'adm',
    canActivate: [authGuard],
    children: [
      {
        path: 'company',
        loadComponent: () => import('./pages/adm/adm-company/adm-company.component').then(m => m.AdmCompanyComponent),
        data: { slug: 'adm-company' }
      },
      {
        path: 'company/:companyId/dept',
        loadComponent: () => import('./pages/adm/adm-company-dept/adm-company-dept.component').then(m => m.AdmCompanyDeptComponent),
        data: { slug: 'adm-company' }
      },
      {
        path: 'company/:companyId/dept/:companyDeptId/users',
        loadComponent: () => import('./pages/adm/adm-dept-user/adm-dept-user.component').then(m => m.AdmDeptUserComponent),
        data: { slug: 'adm-company' }
      },
      {
        path: 'user',
        loadComponent: () => import('./pages/adm/adm-user/adm-user.component').then(m => m.AdmUserComponent),
        data: { slug: 'adm-user' }
      },
      {
        path: 'hierarchy',
        loadComponent: () => import('./pages/adm/adm-hierarchy/adm-hierarchy.component').then(m => m.AdmHierarchyComponent),
        data: { slug: 'adm-hierarchy' }
      },
      {
        path: 'hierarchy-team',
        loadComponent: () => import('./pages/adm/adm-hierarchy-team/adm-hierarchy-team.component').then(m => m.AdmHierarchyTeamComponent),
        data: { slug: 'adm-hierarchy' }
      },
      {
        path: 'importacao-profissionais',
        loadComponent: () => import('./pages/adm/importacao-profissionais/importacao-profissionais.component').then(m => m.ImportacaoProfissionaisComponent),
        data: { slug: 'adm-importacao' }
      },
      {
        path: 'importacao-sallers',
        loadComponent: () => import('./pages/adm/importacao-sallers/importacao-sallers.component').then(m => m.ImportacaoSallersComponent),
        data: { slug: 'adm-importacao-sallers' }
      },
      {
        path: 'customers',
        loadComponent: () => import('./pages/adm/adm-customer/adm-customer.component').then(m => m.AdmCustomerComponent),
        data: { slug: 'adm-customer' }
      },
      {
        path: 'customers/:customerId/sellers',
        loadComponent: () => import('./pages/adm/adm-customer-seller/adm-customer-seller.component').then(m => m.AdmCustomerSellerComponent),
        data: { slug: 'adm-customer' }
      },
      {
        path: 'user-permissions',
        loadComponent: () => import('./pages/adm/adm-user-permissions/adm-user-permissions.component').then(m => m.AdmUserPermissionsComponent),
        data: { slug: 'adm-user' }
      },
      {
        path: 'page',
        loadComponent: () => import('./pages/adm/adm-page/adm-page.component').then(m => m.AdmPageComponent),
        data: { slug: 'adm-page' }
      },
      {
        path: 'role',
        loadComponent: () => import('./pages/adm/adm-role/adm-role.component').then(m => m.AdmRoleComponent),
        data: { slug: 'adm-role' }
      },
      {
        path: 'role/:roleId/permissions',
        loadComponent: () => import('./pages/adm/adm-role-permission/adm-role-permission.component').then(m => m.AdmRolePermissionComponent),
        data: { slug: 'adm-role' }
      },
      {
        path: 'ai-prompt',
        loadComponent: () => import('./pages/adm/adm-ai-prompt/adm-ai-prompt.component').then(m => m.AdmAiPromptComponent),
        data: { slug: 'adm-ai-prompt' }
      },
      {
        path: 'transcricao-audio',
        loadComponent: () => import('./pages/adm/adm-transcricao-audio/adm-transcricao-audio.component').then(m => m.AdmTranscricaoAudioComponent),
        data: { slug: 'adm-transcricao-audio' }
      }
    ]
  },
  {
    path: '**',
    redirectTo: '/home'
  }
];
