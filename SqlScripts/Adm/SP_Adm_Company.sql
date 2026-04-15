-- =============================================================================
-- SP_Adm_sl_Company — Seleciona empresas com filtros opcionais e JOIN com Address
-- =============================================================================
IF OBJECT_ID('SP_Adm_sl_Company', 'P') IS NOT NULL DROP PROCEDURE SP_Adm_sl_Company;
GO
CREATE PROCEDURE SP_Adm_sl_Company
    @CompanyId       INT            = NULL,
    @Name            VARCHAR(500)   = NULL,
    @TaxID           VARCHAR(50)    = NULL,
    @GroupCompanyId  INT            = NULL,
    @ParentCompanyId INT            = NULL,
    @token_usuario   NVARCHAR(MAX)  = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        c.CompanyId,
        c.AddressId,
        c.GroupCompanyId,
        c.Name,
        c.TaxID,
        c.LogoUrl,
        c.DhCreate,
        c.DhUpdate,
        c.Log,
        c.ParentCompanyId,
        a.City,
        a.State
    FROM Company c
    LEFT JOIN Address a ON a.AddressId = c.AddressId
    WHERE
        (@CompanyId       IS NULL OR c.CompanyId       = @CompanyId)
        AND (@Name        IS NULL OR c.Name             LIKE '%' + @Name + '%')
        AND (@TaxID       IS NULL OR c.TaxID            = @TaxID)
        AND (@GroupCompanyId  IS NULL OR c.GroupCompanyId  = @GroupCompanyId)
        AND (@ParentCompanyId IS NULL OR c.ParentCompanyId = @ParentCompanyId)
    ORDER BY c.Name;
END
GO
GRANT EXECUTE ON SP_Adm_sl_Company TO S360sys;
GO

-- =============================================================================
-- SP_Adm_in_Company — Insere uma nova empresa
-- =============================================================================
IF OBJECT_ID('SP_Adm_in_Company', 'P') IS NOT NULL DROP PROCEDURE SP_Adm_in_Company;
GO
CREATE PROCEDURE SP_Adm_in_Company
    @AddressId       INT            = NULL,
    @GroupCompanyId  INT            = NULL,
    @Name            VARCHAR(500)   ,
    @TaxID           VARCHAR(50)    = NULL,
    @LogoUrl         VARCHAR(500)   = NULL,
    @ParentCompanyId INT            = NULL,
    @token_usuario   NVARCHAR(MAX)  = NULL
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO Company (AddressId, GroupCompanyId, Name, TaxID, LogoUrl, DhCreate, DhUpdate, ParentCompanyId)
    VALUES (@AddressId, @GroupCompanyId, @Name, @TaxID, @LogoUrl, GETDATE(), GETDATE(), @ParentCompanyId);

    DECLARE @NewId INT = SCOPE_IDENTITY();

    SELECT
        c.CompanyId,
        c.AddressId,
        c.GroupCompanyId,
        c.Name,
        c.TaxID,
        c.LogoUrl,
        c.DhCreate,
        c.DhUpdate,
        c.ParentCompanyId,
        a.City,
        a.State
    FROM Company c
    LEFT JOIN Address a ON a.AddressId = c.AddressId
    WHERE c.CompanyId = @NewId;
END
GO
GRANT EXECUTE ON SP_Adm_in_Company TO S360sys;
GO

-- =============================================================================
-- SP_Adm_up_Company — Atualiza uma empresa existente
-- =============================================================================
IF OBJECT_ID('SP_Adm_up_Company', 'P') IS NOT NULL DROP PROCEDURE SP_Adm_up_Company;
GO
CREATE PROCEDURE SP_Adm_up_Company
    @CompanyId       INT            ,
    @AddressId       INT            = NULL,
    @GroupCompanyId  INT            = NULL,
    @Name            VARCHAR(500)   = NULL,
    @TaxID           VARCHAR(50)    = NULL,
    @LogoUrl         VARCHAR(500)   = NULL,
    @ParentCompanyId INT            = NULL,
    @token_usuario   NVARCHAR(MAX)  = NULL
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE Company
    SET AddressId       = @AddressId,
        GroupCompanyId  = @GroupCompanyId,
        Name            = @Name,
        TaxID           = @TaxID,
        LogoUrl         = @LogoUrl,
        DhUpdate        = GETDATE(),
        ParentCompanyId = @ParentCompanyId
    WHERE CompanyId = @CompanyId;

    SELECT
        c.CompanyId,
        c.AddressId,
        c.GroupCompanyId,
        c.Name,
        c.TaxID,
        c.LogoUrl,
        c.DhCreate,
        c.DhUpdate,
        c.ParentCompanyId,
        a.City,
        a.State
    FROM Company c
    LEFT JOIN Address a ON a.AddressId = c.AddressId
    WHERE c.CompanyId = @CompanyId;
END
GO
GRANT EXECUTE ON SP_Adm_up_Company TO S360sys;
GO

-- =============================================================================
-- SP_Adm_dl_Company — Remove uma empresa
-- =============================================================================
IF OBJECT_ID('SP_Adm_dl_Company', 'P') IS NOT NULL DROP PROCEDURE SP_Adm_dl_Company;
GO
CREATE PROCEDURE SP_Adm_dl_Company
    @CompanyId     INT            ,
    @token_usuario NVARCHAR(MAX)  = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DELETE FROM Company WHERE CompanyId = @CompanyId;

    SELECT @CompanyId AS CompanyId, 'deleted' AS Status;
END
GO
GRANT EXECUTE ON SP_Adm_dl_Company TO S360sys;
GO
