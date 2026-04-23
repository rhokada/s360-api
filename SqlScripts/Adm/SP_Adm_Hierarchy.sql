-- =============================================================================
-- DROP das procedures individuais de Hierarchy
-- =============================================================================
IF OBJECT_ID('SP_Adm_sl_Hierarchy', 'P') IS NOT NULL DROP PROCEDURE SP_Adm_sl_Hierarchy;
GO
IF OBJECT_ID('SP_Adm_in_Hierarchy', 'P') IS NOT NULL DROP PROCEDURE SP_Adm_in_Hierarchy;
GO
IF OBJECT_ID('SP_Adm_up_Hierarchy', 'P') IS NOT NULL DROP PROCEDURE SP_Adm_up_Hierarchy;
GO
IF OBJECT_ID('SP_Adm_dl_Hierarchy', 'P') IS NOT NULL DROP PROCEDURE SP_Adm_dl_Hierarchy;
GO

-- =============================================================================
-- SP_Adm_Hierarchy — CRUD unificado de hierarquias controlado por @TypeRequest
-- =============================================================================
IF OBJECT_ID('SP_Adm_Hierarchy', 'P') IS NOT NULL DROP PROCEDURE SP_Adm_Hierarchy;
GO
CREATE PROCEDURE SP_Adm_Hierarchy
    @TypeRequest   VARCHAR(10)    ,   -- 'SELECT' | 'INSERT' | 'UPDATE' | 'DELETE'
    @HierarchyId   INT            = NULL,
    @Name          VARCHAR(500)   = NULL,
    @token_usuario NVARCHAR(MAX)  = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF @TypeRequest = 'SELECT'
    BEGIN
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
    ELSE IF @TypeRequest = 'INSERT'
    BEGIN
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
    ELSE IF @TypeRequest = 'UPDATE'
    BEGIN
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
    ELSE IF @TypeRequest = 'DELETE'
    BEGIN
        DELETE FROM Hierarchy WHERE HierarchyId = @HierarchyId;

        SELECT @HierarchyId AS HierarchyId, 'deleted' AS Status;
    END
END
GO
GRANT EXECUTE ON SP_Adm_Hierarchy TO S360sys;
GO
