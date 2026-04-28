-- ============================================================
-- Script de criação das tabelas de importação
-- Gerado a partir do modelo: src/assets/modelo.xlsx
-- Abas: Profissionais, Clientes
-- ============================================================

-- ------------------------------------------------------------
-- Tabela de controle de importações (header de cada carga)
-- ------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DataImportProLog]') AND type = 'U')
BEGIN
  CREATE TABLE [dbo].[DataImportProLog] (
    [DataImportProLogId] INT           IDENTITY(1,1) NOT NULL,
    [FileName]       VARCHAR(200)  NOT NULL,
    [FileType]       VARCHAR(20)   NOT NULL,         -- 'PROFISSIONAL' | 'CLIENTE' | 'AMBOS'
    [Status]         VARCHAR(20)   NOT NULL DEFAULT 'PROCESSING', -- PROCESSING | COMPLETED | ERROR
    [TotalRows]      INT           NULL,
    [ProcessedRows]  INT           NULL,
    [ErrorRows]      INT           NULL,
    [UserId]         INT           NULL,
    [ErrorMessage]   NVARCHAR(MAX) NULL,
    [DhCreate]       DATETIME      NOT NULL DEFAULT GETDATE(),
    [DhUpdate]       DATETIME      NULL,
    CONSTRAINT [PK_DataImportProLog] PRIMARY KEY CLUSTERED ([DataImportProLogId] ASC)
  );
  PRINT 'Tabela DataImportProLog criada.';
END
ELSE
  PRINT 'Tabela DataImportProLog já existe.';
GO

-- ------------------------------------------------------------
-- Tabela de importação de Profissionais
-- Colunas do modelo: ID, CodProfissional, Email, Nome,
--                   Celular, Whats, CodEquipe, Vendedor, CodSuperior
-- ------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DataImportProProfissional]') AND type = 'U')
BEGIN
  CREATE TABLE [dbo].[DataImportProProfissional] (
    [DataImportProProfissionalId] INT           IDENTITY(1,1) NOT NULL,
    [DataImportProLogId]          INT           NOT NULL,
    [ID]                       VARCHAR(50)   NULL,
    [CodProfissional]          VARCHAR(50)   NULL,
    [Email]                    VARCHAR(200)  NULL,
    [Nome]                     VARCHAR(200)  NULL,
    [Celular]                  VARCHAR(20)   NULL,
    [Whats]                    VARCHAR(20)   NULL,
    [CodEquipe]                VARCHAR(50)   NULL,
    [Vendedor]                 BIT           NULL,
    [CodSuperior]              VARCHAR(50)   NULL,
    -- Controle de processamento
    [Status]                   VARCHAR(20)   NOT NULL DEFAULT 'PENDING', -- PENDING | PROCESSED | ERROR | SKIPPED
    [ErrorMessage]             NVARCHAR(MAX) NULL,
    [DhCreate]                 DATETIME      NOT NULL DEFAULT GETDATE(),
    [DhUpdate]                 DATETIME      NULL,
    CONSTRAINT [PK_DataImportProProfissional] PRIMARY KEY CLUSTERED ([DataImportProProfissionalId] ASC),
    CONSTRAINT [FK_DataImportProProfissional_DataImportProLog]
      FOREIGN KEY ([DataImportProLogId]) REFERENCES [dbo].[DataImportProLog]([DataImportProLogId])
  );

  CREATE NONCLUSTERED INDEX [IX_DataImportProProfissional_DataImportProLogId]
    ON [dbo].[DataImportProProfissional] ([DataImportProLogId]);

  CREATE NONCLUSTERED INDEX [IX_DataImportProProfissional_CodProfissional]
    ON [dbo].[DataImportProProfissional] ([CodProfissional]);

  PRINT 'Tabela DataImportProProfissional criada.';
END
ELSE
  PRINT 'Tabela DataImportProProfissional já existe.';
GO

-- ------------------------------------------------------------
-- Tabela de importação de Clientes
-- Colunas do modelo: ID, CodCliente, NomeFantasia, CNPJ, CodVendedor
-- ------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DataImportProCliente]') AND type = 'U')
BEGIN
  CREATE TABLE [dbo].[DataImportProCliente] (
    [DataImportProClienteId] INT           IDENTITY(1,1) NOT NULL,
    [DataImportProLogId]     INT           NOT NULL,
    [ID]                  VARCHAR(50)   NULL,
    [CodCliente]          VARCHAR(50)   NULL,
    [NomeFantasia]        VARCHAR(200)  NULL,
    [CNPJ]                VARCHAR(20)   NULL,
    [CodVendedor]         VARCHAR(50)   NULL,
    -- Controle de processamento
    [Status]              VARCHAR(20)   NOT NULL DEFAULT 'PENDING', -- PENDING | PROCESSED | ERROR | SKIPPED
    [ErrorMessage]        NVARCHAR(MAX) NULL,
    [DhCreate]            DATETIME      NOT NULL DEFAULT GETDATE(),
    [DhUpdate]            DATETIME      NULL,
    CONSTRAINT [PK_DataImportProCliente] PRIMARY KEY CLUSTERED ([DataImportProClienteId] ASC),
    CONSTRAINT [FK_DataImportProCliente_DataImportProLog]
      FOREIGN KEY ([DataImportProLogId]) REFERENCES [dbo].[DataImportProLog]([DataImportProLogId])
  );

  CREATE NONCLUSTERED INDEX [IX_DataImportProCliente_DataImportProLogId]
    ON [dbo].[DataImportProCliente] ([DataImportProLogId]);

  CREATE NONCLUSTERED INDEX [IX_DataImportProCliente_CodCliente]
    ON [dbo].[DataImportProCliente] ([CodCliente]);

  PRINT 'Tabela DataImportProCliente criada.';
END
ELSE
  PRINT 'Tabela DataImportProCliente já existe.';
GO

PRINT 'Script concluído.';
GO
