-- ============================================================
-- Tabelas de Permissões: Adm_Page, Adm_Role, Adm_RolePermission, Adm_RoleUser
-- ============================================================

IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Adm_Page]') AND type = 'U')
BEGIN
    CREATE TABLE [dbo].[Adm_Page] (
        [AdmPageId] INT           IDENTITY(1,1) NOT NULL,
        [Slug]      VARCHAR(100)  NOT NULL,
        [Menu]      VARCHAR(100)  NOT NULL,
        [Icon]      VARCHAR(100)  NULL,
        [DhCreate]  DATETIME      NOT NULL DEFAULT GETDATE(),
        [DhUpdate]  DATETIME      NULL,
        CONSTRAINT [PK_Adm_Page]   PRIMARY KEY CLUSTERED ([AdmPageId] ASC),
        CONSTRAINT [UQ_Adm_Page_Slug] UNIQUE ([Slug])
    );
    PRINT 'Tabela Adm_Page criada.';
END
ELSE
    PRINT 'Tabela Adm_Page ja existe.';
GO

IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Adm_Role]') AND type = 'U')
BEGIN
    CREATE TABLE [dbo].[Adm_Role] (
        [AdmRoleId]   INT           IDENTITY(1,1) NOT NULL,
        [AdmRoleCd]   VARCHAR(50)   NOT NULL,
        [AdmRoleName] VARCHAR(100)  NOT NULL,
        [DhCreate]    DATETIME      NOT NULL DEFAULT GETDATE(),
        [DhUpdate]    DATETIME      NULL,
        CONSTRAINT [PK_Adm_Role]    PRIMARY KEY CLUSTERED ([AdmRoleId] ASC),
        CONSTRAINT [UQ_Adm_Role_Cd] UNIQUE ([AdmRoleCd])
    );
    PRINT 'Tabela Adm_Role criada.';
END
ELSE
    PRINT 'Tabela Adm_Role ja existe.';
GO

IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Adm_RolePermission]') AND type = 'U')
BEGIN
    CREATE TABLE [dbo].[Adm_RolePermission] (
        [AdmRolePermissionId] INT      IDENTITY(1,1) NOT NULL,
        [AdmRoleId]           INT      NOT NULL,
        [AdmPageId]           INT      NOT NULL,
        [Read]                BIT      NOT NULL DEFAULT 0,
        [Create]              BIT      NOT NULL DEFAULT 0,
        [Delete]              BIT      NOT NULL DEFAULT 0,
        [Alter]               BIT      NOT NULL DEFAULT 0,
        [DhCreate]            DATETIME NOT NULL DEFAULT GETDATE(),
        [DhUpdate]            DATETIME NULL,
        CONSTRAINT [PK_Adm_RolePermission] PRIMARY KEY CLUSTERED ([AdmRolePermissionId] ASC),
        CONSTRAINT [UQ_Adm_RolePermission_RolePage] UNIQUE ([AdmRoleId], [AdmPageId]),
        CONSTRAINT [FK_Adm_RolePermission_Role] FOREIGN KEY ([AdmRoleId]) REFERENCES [dbo].[Adm_Role]([AdmRoleId]),
        CONSTRAINT [FK_Adm_RolePermission_Page] FOREIGN KEY ([AdmPageId]) REFERENCES [dbo].[Adm_Page]([AdmPageId])
    );
    PRINT 'Tabela Adm_RolePermission criada.';
END
ELSE
    PRINT 'Tabela Adm_RolePermission ja existe.';
GO

IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Adm_RoleUser]') AND type = 'U')
BEGIN
    CREATE TABLE [dbo].[Adm_RoleUser] (
        [AdmRoleUserId] INT      IDENTITY(1,1) NOT NULL,
        [AdmRoleId]     INT      NOT NULL,
        [UserId]        INT      NOT NULL,
        [DhCreate]      DATETIME NOT NULL DEFAULT GETDATE(),
        CONSTRAINT [PK_Adm_RoleUser]          PRIMARY KEY CLUSTERED ([AdmRoleUserId] ASC),
        CONSTRAINT [UQ_Adm_RoleUser_RoleUser] UNIQUE ([AdmRoleId], [UserId]),
        CONSTRAINT [FK_Adm_RoleUser_Role]      FOREIGN KEY ([AdmRoleId]) REFERENCES [dbo].[Adm_Role]([AdmRoleId]),
        CONSTRAINT [FK_Adm_RoleUser_User]      FOREIGN KEY ([UserId])    REFERENCES [dbo].[Users]([UserId])
    );
    PRINT 'Tabela Adm_RoleUser criada.';
END
ELSE
    PRINT 'Tabela Adm_RoleUser ja existe.';
GO

PRINT 'Script de permissoes concluido.';
GO
