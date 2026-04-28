-- ============================================================
-- Script de criação do schema do Hangfire no SQL Server
-- Versão compatível com Hangfire.SqlServer 1.8.x (schema v9)
--
-- Instruções para o DBA:
--   1. Execute este script conectado com um usuário que tenha
--      permissão de CREATE TABLE e CREATE SCHEMA no banco.
--   2. Após executar, o usuário da aplicação (ex: S360sys) terá
--      permissões de leitura/escrita no schema HangFire.
--   3. Execute apenas uma vez por banco de dados.
-- ============================================================

-- ------------------------------------------------------------
-- 1. Schema
-- ------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'HangFire')
    EXEC ('CREATE SCHEMA [HangFire]');
GO

-- ------------------------------------------------------------
-- 2. Tabela de controle de versão do schema
-- ------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[HangFire].[Schema]') AND type = 'U')
BEGIN
    CREATE TABLE [HangFire].[Schema] (
        [Version] [int] NOT NULL,
        CONSTRAINT [PK_HangFire_Schema] PRIMARY KEY CLUSTERED ([Version] ASC)
    );
    INSERT INTO [HangFire].[Schema] ([Version]) VALUES (9);
    PRINT 'Tabela HangFire.Schema criada.';
END
ELSE
    PRINT 'Tabela HangFire.Schema já existe.';
GO

-- ------------------------------------------------------------
-- 3. Job
-- ------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[HangFire].[Job]') AND type = 'U')
BEGIN
    CREATE TABLE [HangFire].[Job] (
        [Id]             [bigint]        IDENTITY(1,1) NOT NULL,
        [StateId]        [bigint]        NULL,
        [StateName]      [nvarchar](20)  NULL,
        [InvocationData] [nvarchar](max) NOT NULL,
        [Arguments]      [nvarchar](max) NOT NULL,
        [CreatedAt]      [datetime]      NOT NULL,
        [ExpireAt]       [datetime]      NULL,
        [NextExecution]  [datetime]      NULL,
        CONSTRAINT [PK_HangFire_Job] PRIMARY KEY CLUSTERED ([Id] ASC)
    );

    CREATE NONCLUSTERED INDEX [IX_HangFire_Job_StateName]
        ON [HangFire].[Job] ([StateName] ASC);

    CREATE NONCLUSTERED INDEX [IX_HangFire_Job_ExpireAt]
        ON [HangFire].[Job] ([ExpireAt] ASC);

    PRINT 'Tabela HangFire.Job criada.';
END
ELSE
    PRINT 'Tabela HangFire.Job já existe.';
GO

-- ------------------------------------------------------------
-- 4. State
-- ------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[HangFire].[State]') AND type = 'U')
BEGIN
    CREATE TABLE [HangFire].[State] (
        [Id]        [bigint]        IDENTITY(1,1) NOT NULL,
        [JobId]     [bigint]        NOT NULL,
        [Name]      [nvarchar](20)  NOT NULL,
        [Reason]    [nvarchar](100) NULL,
        [CreatedAt] [datetime]      NOT NULL,
        [Data]      [nvarchar](max) NULL,
        CONSTRAINT [PK_HangFire_State] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_HangFire_State_Job] FOREIGN KEY ([JobId])
            REFERENCES [HangFire].[Job] ([Id]) ON DELETE CASCADE
    );

    CREATE NONCLUSTERED INDEX [IX_HangFire_State_JobId]
        ON [HangFire].[State] ([JobId] ASC);

    PRINT 'Tabela HangFire.State criada.';
END
ELSE
    PRINT 'Tabela HangFire.State já existe.';
GO

-- ------------------------------------------------------------
-- 5. JobParameter
-- ------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[HangFire].[JobParameter]') AND type = 'U')
BEGIN
    CREATE TABLE [HangFire].[JobParameter] (
        [Id]    [bigint]        IDENTITY(1,1) NOT NULL,
        [JobId] [bigint]        NOT NULL,
        [Name]  [nvarchar](40)  NOT NULL,
        [Value] [nvarchar](max) NULL,
        CONSTRAINT [PK_HangFire_JobParameter] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_HangFire_JobParameter_Job] FOREIGN KEY ([JobId])
            REFERENCES [HangFire].[Job] ([Id]) ON DELETE CASCADE
    );

    CREATE NONCLUSTERED INDEX [IX_HangFire_JobParameter_JobIdAndName]
        ON [HangFire].[JobParameter] ([JobId] ASC, [Name] ASC);

    PRINT 'Tabela HangFire.JobParameter criada.';
END
ELSE
    PRINT 'Tabela HangFire.JobParameter já existe.';
GO

-- ------------------------------------------------------------
-- 6. JobQueue
-- ------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[HangFire].[JobQueue]') AND type = 'U')
BEGIN
    CREATE TABLE [HangFire].[JobQueue] (
        [Id]         [bigint]          IDENTITY(1,1) NOT NULL,
        [JobId]      [bigint]          NOT NULL,
        [Queue]      [nvarchar](50)    NOT NULL,
        [FetchedAt]  [datetime]        NULL,
        [FetchToken] [uniqueidentifier] NULL,
        CONSTRAINT [PK_HangFire_JobQueue] PRIMARY KEY CLUSTERED ([Id] ASC)
    );

    CREATE NONCLUSTERED INDEX [IX_HangFire_JobQueue_QueueAndFetchedAt]
        ON [HangFire].[JobQueue] ([Queue] ASC, [FetchedAt] ASC);

    CREATE NONCLUSTERED INDEX [IX_HangFire_JobQueue_JobIdAndQueue]
        ON [HangFire].[JobQueue] ([JobId] ASC, [Queue] ASC);

    PRINT 'Tabela HangFire.JobQueue criada.';
END
ELSE
    PRINT 'Tabela HangFire.JobQueue já existe.';
GO

-- ------------------------------------------------------------
-- 7. Server
-- ------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[HangFire].[Server]') AND type = 'U')
BEGIN
    CREATE TABLE [HangFire].[Server] (
        [Id]            [nvarchar](200) NOT NULL,
        [Data]          [nvarchar](max) NULL,
        [LastHeartbeat] [datetime]      NOT NULL,
        CONSTRAINT [PK_HangFire_Server] PRIMARY KEY CLUSTERED ([Id] ASC)
    );

    PRINT 'Tabela HangFire.Server criada.';
END
ELSE
    PRINT 'Tabela HangFire.Server já existe.';
GO

-- ------------------------------------------------------------
-- 8. Hash
-- ------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[HangFire].[Hash]') AND type = 'U')
BEGIN
    CREATE TABLE [HangFire].[Hash] (
        [Id]          [bigint]         IDENTITY(1,1) NOT NULL,
        [Key]         [nvarchar](100)  NOT NULL,
        [Field]       [nvarchar](100)  NOT NULL,
        [Value]       [nvarchar](max)  NULL,
        [ExpireAt]    [datetime2](7)   NULL,
        [UpdateCount] [int]            NOT NULL DEFAULT 0,
        CONSTRAINT [PK_HangFire_Hash] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [UK_HangFire_Hash_Key_Field] UNIQUE NONCLUSTERED ([Key] ASC, [Field] ASC)
    );

    CREATE NONCLUSTERED INDEX [IX_HangFire_Hash_ExpireAt]
        ON [HangFire].[Hash] ([ExpireAt] ASC);

    PRINT 'Tabela HangFire.Hash criada.';
END
ELSE
    PRINT 'Tabela HangFire.Hash já existe.';
GO

-- ------------------------------------------------------------
-- 9. Set
-- ------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[HangFire].[Set]') AND type = 'U')
BEGIN
    CREATE TABLE [HangFire].[Set] (
        [Id]          [bigint]        IDENTITY(1,1) NOT NULL,
        [Key]         [nvarchar](100) NOT NULL,
        [Score]       [float]         NOT NULL,
        [Value]       [nvarchar](256) NOT NULL,
        [ExpireAt]    [datetime]      NULL,
        [UpdateCount] [int]           NOT NULL DEFAULT 0,
        CONSTRAINT [PK_HangFire_Set] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [UK_HangFire_Set_KeyAndValue] UNIQUE NONCLUSTERED ([Key] ASC, [Value] ASC)
    );

    CREATE NONCLUSTERED INDEX [IX_HangFire_Set_Key]
        ON [HangFire].[Set] ([Key] ASC) INCLUDE ([Score]);

    CREATE NONCLUSTERED INDEX [IX_HangFire_Set_ExpireAt]
        ON [HangFire].[Set] ([ExpireAt] ASC);

    PRINT 'Tabela HangFire.Set criada.';
END
ELSE
    PRINT 'Tabela HangFire.Set já existe.';
GO

-- ------------------------------------------------------------
-- 10. List
-- ------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[HangFire].[List]') AND type = 'U')
BEGIN
    CREATE TABLE [HangFire].[List] (
        [Id]       [bigint]        IDENTITY(1,1) NOT NULL,
        [Key]      [nvarchar](100) NOT NULL,
        [Value]    [nvarchar](max) NULL,
        [ExpireAt] [datetime]      NULL,
        CONSTRAINT [PK_HangFire_List] PRIMARY KEY CLUSTERED ([Id] ASC)
    );

    CREATE NONCLUSTERED INDEX [IX_HangFire_List_Key]
        ON [HangFire].[List] ([Key] ASC) INCLUDE ([ExpireAt]);

    CREATE NONCLUSTERED INDEX [IX_HangFire_List_ExpireAt]
        ON [HangFire].[List] ([ExpireAt] ASC);

    PRINT 'Tabela HangFire.List criada.';
END
ELSE
    PRINT 'Tabela HangFire.List já existe.';
GO

-- ------------------------------------------------------------
-- 11. Counter
-- ------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[HangFire].[Counter]') AND type = 'U')
BEGIN
    CREATE TABLE [HangFire].[Counter] (
        [Id]       [bigint]        IDENTITY(1,1) NOT NULL,
        [Key]      [nvarchar](100) NOT NULL,
        [Value]    [smallint]      NOT NULL,
        [ExpireAt] [datetime]      NULL,
        CONSTRAINT [PK_HangFire_Counter] PRIMARY KEY CLUSTERED ([Id] ASC)
    );

    CREATE NONCLUSTERED INDEX [IX_HangFire_Counter_Key]
        ON [HangFire].[Counter] ([Key] ASC) INCLUDE ([Value]);

    CREATE NONCLUSTERED INDEX [IX_HangFire_Counter_ExpireAt]
        ON [HangFire].[Counter] ([ExpireAt] ASC);

    PRINT 'Tabela HangFire.Counter criada.';
END
ELSE
    PRINT 'Tabela HangFire.Counter já existe.';
GO

-- ------------------------------------------------------------
-- 12. AggregatedCounter
-- ------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[HangFire].[AggregatedCounter]') AND type = 'U')
BEGIN
    CREATE TABLE [HangFire].[AggregatedCounter] (
        [Id]       [bigint]        IDENTITY(1,1) NOT NULL,
        [Key]      [nvarchar](100) NOT NULL,
        [Value]    [bigint]        NOT NULL,
        [ExpireAt] [datetime]      NULL,
        CONSTRAINT [PK_HangFire_AggregatedCounter] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [UK_HangFire_CounterAggregated_Key] UNIQUE NONCLUSTERED ([Key] ASC)
    );

    CREATE NONCLUSTERED INDEX [IX_HangFire_AggregatedCounter_ExpireAt]
        ON [HangFire].[AggregatedCounter] ([ExpireAt] ASC);

    PRINT 'Tabela HangFire.AggregatedCounter criada.';
END
ELSE
    PRINT 'Tabela HangFire.AggregatedCounter já existe.';
GO

-- ------------------------------------------------------------
-- 13. Lock
-- ------------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[HangFire].[Lock]') AND type = 'U')
BEGIN
    CREATE TABLE [HangFire].[Lock] (
        [Resource] [nvarchar](100) NOT NULL,
        [ExpireAt] [datetime]      NULL,
        CONSTRAINT [PK_HangFire_Lock] PRIMARY KEY CLUSTERED ([Resource] ASC)
    );

    PRINT 'Tabela HangFire.Lock criada.';
END
ELSE
    PRINT 'Tabela HangFire.Lock já existe.';
GO

-- ------------------------------------------------------------
-- 14. Permissões para o usuário da aplicação
--     Substitua [S360sys] pelo login da aplicação se for diferente
-- ------------------------------------------------------------
GRANT SELECT, INSERT, UPDATE, DELETE ON SCHEMA::[HangFire] TO [S360sys];
GRANT EXECUTE ON SCHEMA::[HangFire] TO [S360sys];
PRINT 'Permissões concedidas ao usuário S360sys.';
GO

PRINT '=== Schema HangFire criado com sucesso (versão 9). ===';
GO
