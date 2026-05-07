import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

interface FaqItem {
  question: string;
  answer: string;
  isOpen: boolean;
}

@Component({
  selector: 'app-suporte',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './suporte.component.html',
  styleUrls: ['./suporte.component.scss']
})
export class SuporteComponent {
  whatsappNumber = '5511999999999';
  supportEmail = 'suporte@bdhelix.com.br';
  appVersion = '1.0.0';

  faqItems: FaqItem[] = [
    {
      question: 'Como sincronizar os dados do sistema?',
      answer: 'Acesse a página "Início" e clique no botão "Sincronizar Agora". O sistema irá importar todos os vendedores, clientes e questões do servidor. Certifique-se de ter conexão com a internet.',
      isOpen: false
    },
    {
      question: 'O que acontece se perder a conexão durante uma avaliação?',
      answer: 'Não se preocupe! O sistema salva automaticamente as respostas a cada 30 segundos no armazenamento local do dispositivo. Ao recuperar a conexão, você pode continuar de onde parou.',
      isOpen: false
    },
    {
      question: 'Como criar uma pendência (FUP)?',
      answer: 'Acesse a página "Pendências" e clique em "Nova Pendência". Preencha a categoria, descrição, data prevista e prioridade. As pendências podem ser vinculadas a um vendedor ou cliente específico.',
      isOpen: false
    },
    {
      question: 'Quais tipos de avaliação estão disponíveis?',
      answer: 'O sistema oferece três tipos de avaliação: Check de Rota (verificação das atividades de rota), Treinamento em Campo (avaliação durante treinamentos) e Avaliação de Mercado (análise de condições de mercado).',
      isOpen: false
    },
    {
      question: 'Como visualizar os resultados das avaliações?',
      answer: 'Acesse a página "Resultados" para ver gráficos e tabelas com todas as avaliações realizadas. Você pode filtrar por vendedor, tipo de avaliação e período de datas.',
      isOpen: false
    },
    {
      question: 'Esqueci minha senha. O que fazer?',
      answer: 'Na tela de login, clique em "Esqueci a senha" e informe seu email cadastrado. Uma senha temporária será enviada para o seu email. Após entrar, recomendamos trocar a senha em "Trocar Senha".',
      isOpen: false
    },
    {
      question: 'Por quanto tempo fico logado no sistema?',
      answer: 'A sessão é controlada por um token JWT com prazo de expiração definido pelo servidor. Quando o token expirar, você será redirecionado para a tela de login automaticamente.',
      isOpen: false
    }
  ];

  toggleFaq(index: number): void {
    this.faqItems[index].isOpen = !this.faqItems[index].isOpen;
  }

  openWhatsApp(): void {
    const message = encodeURIComponent('Olá! Preciso de suporte com o Supervision 360.');
    window.open(`https://wa.me/${this.whatsappNumber}?text=${message}`, '_blank');
  }

  sendEmail(): void {
    window.location.href = `mailto:${this.supportEmail}?subject=Suporte Supervision 360`;
  }
}
