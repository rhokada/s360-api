-- =============================================================================
-- DROP das procedures individuais de Company
-- =============================================================================
IF OBJECT_ID('SP_Adm_sl_Company', 'P') IS NOT NULL DROP PROCEDURE SP_Adm_sl_Company;
GO
IF OBJECT_ID('SP_Adm_in_Company', 'P') IS NOT NULL DROP PROCEDURE SP_Adm_in_Company;
GO
IF OBJECT_ID('SP_Adm_up_Company', 'P') IS NOT NULL DROP PROCEDURE SP_Adm_up_Company;
GO
IF OBJECT_ID('SP_Adm_dl_Company', 'P') IS NOT NULL DROP PROCEDURE SP_Adm_dl_Company;
GO

-- =============================================================================
-- SP_Adm_Company — CRUD unificado de empresas controlado por @TypeRequest
-- =============================================================================
IF OBJECT_ID('SP_Adm_Company', 'P') IS NOT NULL DROP PROCEDURE SP_Adm_Company;
GO
CREATE PROCEDURE SP_Adm_Company
    @TypeRequest     VARCHAR(10)    ,   -- 'SELECT' | 'INSERT' | 'UPDATE' | 'DELETE'
    @CompanyId       INT            = NULL,
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

    IF @TypeRequest = 'SELECT'
    BEGIN
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
    ELSE IF @TypeRequest = 'INSERT'
    BEGIN
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
    ELSE IF @TypeRequest = 'UPDATE'
    BEGIN
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
    ELSE IF @TypeRequest = 'DELETE'
    BEGIN
        DELETE FROM Company WHERE CompanyId = @CompanyId;

        SELECT @CompanyId AS CompanyId, 'deleted' AS Status;
    END
END
GO
GRANT EXECUTE ON SP_Adm_Company TO S360sys;
GO
