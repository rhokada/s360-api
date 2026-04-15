-- =============================================================================
-- SP_Adm_sl_CompanyDept — Seleciona departamentos com JOINs em Company e Address
-- =============================================================================
IF OBJECT_ID('SP_Adm_sl_CompanyDept', 'P') IS NOT NULL DROP PROCEDURE SP_Adm_sl_CompanyDept;
GO
CREATE PROCEDURE SP_Adm_sl_CompanyDept
    @CompanyDeptId INT            = NULL,
    @CompanyId     INT            = NULL,
    @Name          VARCHAR(500)   = NULL,
    @token_usuario NVARCHAR(MAX)  = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        cd.CompanyDeptId,
        cd.CompanyId,
        c.Name      AS CompanyName,
        cd.AddressId,
        cd.Name,
        cd.ProfitCenter,
        cd.CostCenter,
        cd.DhUpdate,
        cd.Log,
        a.City,
        a.State
    FROM CompanyDept cd
    INNER JOIN Company c  ON c.CompanyId  = cd.CompanyId
    LEFT JOIN  Address a  ON a.AddressId  = cd.AddressId
    WHERE
        (@CompanyDeptId IS NULL OR cd.CompanyDeptId = @CompanyDeptId)
        AND (@CompanyId IS NULL OR cd.CompanyId     = @CompanyId)
        AND (@Name      IS NULL OR cd.Name          LIKE '%' + @Name + '%')
    ORDER BY c.Name, cd.Name;
END
GO
GRANT EXECUTE ON SP_Adm_sl_CompanyDept TO S360sys;
GO

-- =============================================================================
-- SP_Adm_in_CompanyDept — Insere um novo departamento
-- =============================================================================
IF OBJECT_ID('SP_Adm_in_CompanyDept', 'P') IS NOT NULL DROP PROCEDURE SP_Adm_in_CompanyDept;
GO
CREATE PROCEDURE SP_Adm_in_CompanyDept
    @CompanyId     INT            ,
    @AddressId     INT            = NULL,
    @Name          VARCHAR(500)   ,
    @ProfitCenter  VARCHAR(100)   = NULL,
    @CostCenter    VARCHAR(100)   = NULL,
    @token_usuario NVARCHAR(MAX)  = NULL
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO CompanyDept (CompanyId, AddressId, Name, ProfitCenter, CostCenter, DhUpdate)
    VALUES (@CompanyId, @AddressId, @Name, @ProfitCenter, @CostCenter, GETDATE());

    DECLARE @NewId INT = SCOPE_IDENTITY();

    SELECT
        cd.CompanyDeptId,
        cd.CompanyId,
        c.Name  AS CompanyName,
        cd.AddressId,
        cd.Name,
        cd.ProfitCenter,
        cd.CostCenter,
        cd.DhUpdate,
        a.City,
        a.State
    FROM CompanyDept cd
    INNER JOIN Company c ON c.CompanyId = cd.CompanyId
    LEFT JOIN  Address a ON a.AddressId = cd.AddressId
    WHERE cd.CompanyDeptId = @NewId;
END
GO
GRANT EXECUTE ON SP_Adm_in_CompanyDept TO S360sys;
GO

-- =============================================================================
-- SP_Adm_up_CompanyDept — Atualiza um departamento existente
-- =============================================================================
IF OBJECT_ID('SP_Adm_up_CompanyDept', 'P') IS NOT NULL DROP PROCEDURE SP_Adm_up_CompanyDept;
GO
CREATE PROCEDURE SP_Adm_up_CompanyDept
    @CompanyDeptId INT            ,
    @CompanyId     INT            ,
    @AddressId     INT            = NULL,
    @Name          VARCHAR(500)   = NULL,
    @ProfitCenter  VARCHAR(100)   = NULL,
    @CostCenter    VARCHAR(100)   = NULL,
    @token_usuario NVARCHAR(MAX)  = NULL
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE CompanyDept
    SET CompanyId    = @CompanyId,
        AddressId    = @AddressId,
        Name         = @Name,
        ProfitCenter = @ProfitCenter,
        CostCenter   = @CostCenter,
        DhUpdate     = GETDATE()
    WHERE CompanyDeptId = @CompanyDeptId;

    SELECT
        cd.CompanyDeptId,
        cd.CompanyId,
        c.Name  AS CompanyName,
        cd.AddressId,
        cd.Name,
        cd.ProfitCenter,
        cd.CostCenter,
        cd.DhUpdate,
        a.City,
        a.State
    FROM CompanyDept cd
    INNER JOIN Company c ON c.CompanyId = cd.CompanyId
    LEFT JOIN  Address a ON a.AddressId = cd.AddressId
    WHERE cd.CompanyDeptId = @CompanyDeptId;
END
GO
GRANT EXECUTE ON SP_Adm_up_CompanyDept TO S360sys;
GO

-- =============================================================================
-- SP_Adm_dl_CompanyDept — Remove um departamento
-- =============================================================================
IF OBJECT_ID('SP_Adm_dl_CompanyDept', 'P') IS NOT NULL DROP PROCEDURE SP_Adm_dl_CompanyDept;
GO
CREATE PROCEDURE SP_Adm_dl_CompanyDept
    @CompanyDeptId INT            ,
    @token_usuario NVARCHAR(MAX)  = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DELETE FROM CompanyDept WHERE CompanyDeptId = @CompanyDeptId;

    SELECT @CompanyDeptId AS CompanyDeptId, 'deleted' AS Status;
END
GO
GRANT EXECUTE ON SP_Adm_dl_CompanyDept TO S360sys;
GO
