-- ============================================================
-- Script de criação das tabelas de importação de Sallers
-- Modelo: src/assets/modelo.xlsx  Aba: Tabelao
-- Colunas: ID, CodCliente, NomeFantasia, CNPJ, CodProfissional,
--          Email, Nome, Celular, Whats, CodEquipe, Vendedor,
--          CodSuperior, NomeSupervisor, TelefoneSupervisor, EmailSupervisor
-- ============================================================

-- ------------------------------------------------------------
-- Tabela de controle de importações (header de cada carga)
-- ------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DataImportSallersLog]') AND type = 'U')
BEGIN
  CREATE TABLE [dbo].[DataImportSallersLog] (
    [DataImportSallersLogId] INT           IDENTITY(1,1) NOT NULL,
    [FileName]               VARCHAR(200)  NOT NULL,
    [Status]                 VARCHAR(20)   NOT NULL DEFAULT 'PROCESSING', -- PROCESSING | COMPLETED | ERROR
    [TotalRows]              INT           NULL,
    [ProcessedRows]          INT           NULL,
    [ErrorRows]              INT           NULL,
    [UserId]                 INT           NULL,
    [ErrorMessage]           NVARCHAR(MAX) NULL,
    [DhCreate]               DATETIME      NOT NULL DEFAULT GETDATE(),
    [DhUpdate]               DATETIME      NULL,
    CONSTRAINT [PK_DataImportSallersLog] PRIMARY KEY CLUSTERED ([DataImportSallersLogId] ASC)
  );
  PRINT 'Tabela DataImportSallersLog criada.';
END
ELSE
  PRINT 'Tabela DataImportSallersLog já existe.';
GO

-- ------------------------------------------------------------
-- Tabela de linhas importadas do Tabelao
-- Colunas do modelo: ID, CodCliente, NomeFantasia, CNPJ,
--   CodProfissional, Email, Nome, Celular, CodEquipe,
--   Vendedor, CodSuperior, NomeSupervisor, TelefoneSupervisor,
--   EmailSupervisor
-- ------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DataImportSallersRow]') AND type = 'U')
BEGIN
  CREATE TABLE [dbo].[DataImportSallersRow] (
    [DataImportSallersRowId] INT           IDENTITY(1,1) NOT NULL,
    [DataImportSallersLogId] INT           NOT NULL,
    [ID]                     VARCHAR(50)   NULL,
    [CodCliente]             VARCHAR(50)   NULL,
    [NomeFantasia]           VARCHAR(200)  NULL,
    [CNPJ]                   VARCHAR(20)   NULL,
    [CodProfissional]        VARCHAR(50)   NULL,
    [Email]                  VARCHAR(200)  NULL,
    [Nome]                   VARCHAR(200)  NULL,
    [Celular]                VARCHAR(20)   NULL,
    [CodEquipe]              VARCHAR(50)   NULL,
    [Vendedor]               BIT           NULL,
    [CodSuperior]            VARCHAR(50)   NULL,
    [NomeSupervisor]         VARCHAR(200)  NULL,
    [TelefoneSupervisor]     VARCHAR(20)   NULL,
    [EmailSupervisor]        VARCHAR(200)  NULL,
    -- Controle de processamento
    [Status]                 VARCHAR(20)   NOT NULL DEFAULT 'PENDING', -- PENDING | PROCESSED | ERROR | SKIPPED
    [ErrorMessage]           NVARCHAR(MAX) NULL,
    [DhCreate]               DATETIME      NOT NULL DEFAULT GETDATE(),
    [DhUpdate]               DATETIME      NULL,
    CONSTRAINT [PK_DataImportSallersRow] PRIMARY KEY CLUSTERED ([DataImportSallersRowId] ASC),
    CONSTRAINT [FK_DataImportSallersRow_Log]
      FOREIGN KEY ([DataImportSallersLogId]) REFERENCES [dbo].[DataImportSallersLog]([DataImportSallersLogId])
  );

  CREATE NONCLUSTERED INDEX [IX_DataImportSallersRow_LogId]
    ON [dbo].[DataImportSallersRow] ([DataImportSallersLogId]);

  CREATE NONCLUSTERED INDEX [IX_DataImportSallersRow_CodProfissional]
    ON [dbo].[DataImportSallersRow] ([CodProfissional]);

  CREATE NONCLUSTERED INDEX [IX_DataImportSallersRow_CodCliente]
    ON [dbo].[DataImportSallersRow] ([CodCliente]);

  PRINT 'Tabela DataImportSallersRow criada.';
END
ELSE
BEGIN
  -- Adiciona colunas novas caso a tabela já exista
  IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[DataImportSallersRow]') AND name = 'Whats')
    ALTER TABLE [dbo].[DataImportSallersRow] DROP COLUMN [Whats];

  IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[DataImportSallersRow]') AND name = 'NomeSupervisor')
    ALTER TABLE [dbo].[DataImportSallersRow] ADD [NomeSupervisor] VARCHAR(200) NULL;

  IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[DataImportSallersRow]') AND name = 'TelefoneSupervisor')
    ALTER TABLE [dbo].[DataImportSallersRow] ADD [TelefoneSupervisor] VARCHAR(20) NULL;

  IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[DataImportSallersRow]') AND name = 'EmailSupervisor')
    ALTER TABLE [dbo].[DataImportSallersRow] ADD [EmailSupervisor] VARCHAR(200) NULL;

  PRINT 'Tabela DataImportSallersRow atualizada com novas colunas.';
END
GO

PRINT 'Script concluído.';
GO
