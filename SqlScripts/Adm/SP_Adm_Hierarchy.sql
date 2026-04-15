-- =============================================================================
-- SP_Adm_sl_Hierarchy — Seleciona hierarquias com filtros opcionais
-- =============================================================================
IF OBJECT_ID('SP_Adm_sl_Hierarchy', 'P') IS NOT NULL DROP PROCEDURE SP_Adm_sl_Hierarchy;
GO
CREATE PROCEDURE SP_Adm_sl_Hierarchy
    @HierarchyId   INT            = NULL,
    @Name          VARCHAR(500)   = NULL,
    @token_usuario NVARCHAR(MAX)  = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        h.HierarchyId,
        h.Name,
        h.DhCreate,
        h.DhUpdate,
        h.Log
    FROM Hierarchy h
    WHERE
        (@HierarchyId IS NULL OR h.HierarchyId = @HierarchyId)
        AND (@Name    IS NULL OR h.Name LIKE '%' + @Name + '%')
    ORDER BY h.Name;
END
GO
GRANT EXECUTE ON SP_Adm_sl_Hierarchy TO S360sys;
GO

-- =============================================================================
-- SP_Adm_in_Hierarchy — Insere uma nova hierarquia
-- =============================================================================
IF OBJECT_ID('SP_Adm_in_Hierarchy', 'P') IS NOT NULL DROP PROCEDURE SP_Adm_in_Hierarchy;
GO
CREATE PROCEDURE SP_Adm_in_Hierarchy
    @Name          VARCHAR(500)   ,
    @token_usuario NVARCHAR(MAX)  = NULL
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO Hierarchy (Name, DhCreate, DhUpdate)
    VALUES (@Name, GETDATE(), GETDATE());

    SELECT
        h.HierarchyId,
        h.Name,
        h.DhCreate,
        h.DhUpdate
    FROM Hierarchy h
    WHERE h.HierarchyId = SCOPE_IDENTITY();
END
GO
GRANT EXECUTE ON SP_Adm_in_Hierarchy TO S360sys;
GO

-- =============================================================================
-- SP_Adm_up_Hierarchy — Atualiza uma hierarquia existente
-- =============================================================================
IF OBJECT_ID('SP_Adm_up_Hierarchy', 'P') IS NOT NULL DROP PROCEDURE SP_Adm_up_Hierarchy;
GO
CREATE PROCEDURE SP_Adm_up_Hierarchy
    @HierarchyId   INT            ,
    @Name          VARCHAR(500)   ,
    @token_usuario NVARCHAR(MAX)  = NULL
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE Hierarchy
    SET Name     = @Name,
        DhUpdate = GETDATE()
    WHERE HierarchyId = @HierarchyId;

    SELECT
        h.HierarchyId,
        h.Name,
        h.DhCreate,
        h.DhUpdate
    FROM Hierarchy h
    WHERE h.HierarchyId = @HierarchyId;
END
GO
GRANT EXECUTE ON SP_Adm_up_Hierarchy TO S360sys;
GO

-- =============================================================================
-- SP_Adm_dl_Hierarchy — Remove uma hierarquia
-- =============================================================================
IF OBJECT_ID('SP_Adm_dl_Hierarchy', 'P') IS NOT NULL DROP PROCEDURE SP_Adm_dl_Hierarchy;
GO
CREATE PROCEDURE SP_Adm_dl_Hierarchy
    @HierarchyId   INT            ,
    @token_usuario NVARCHAR(MAX)  = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DELETE FROM Hierarchy WHERE HierarchyId = @HierarchyId;

    SELECT @HierarchyId AS HierarchyId, 'deleted' AS Status;
END
GO
GRANT EXECUTE ON SP_Adm_dl_Hierarchy TO S360sys;
GO
