-- =============================================================================
-- SP_Adm_sl_HierarchyTeam — Seleciona equipe hierárquica com JOINs completos
-- =============================================================================
IF OBJECT_ID('SP_Adm_sl_HierarchyTeam', 'P') IS NOT NULL DROP PROCEDURE SP_Adm_sl_HierarchyTeam;
GO
CREATE PROCEDURE SP_Adm_sl_HierarchyTeam
    @HierarchyTeamId INT            = NULL,
    @HierarchyId     INT            = NULL,
    @UserId          INT            = NULL,
    @BossId          INT            = NULL,
    @Active          BIT            = NULL,
    @token_usuario   NVARCHAR(MAX)  = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        ht.HierarchyTeamId,
        ht.HierarchyId,
        h.Name      AS HierarchyName,
        ht.UserId,
        u.Name      AS UserName,
        u.Email     AS UserEmail,
        ht.BossId,
        boss.Name   AS BossName,
        boss.Email  AS BossEmail,
        ht.Active,
        ht.DhCreate,
        ht.DhUpdate,
        du.DeptUserId,
        du.CompanyDeptId,
        du.Title,
        du.CompanyCodeUser,
        cd.Name     AS DeptName,
        c.CompanyId,
        c.Name      AS CompanyName
    FROM HierarchyTeam ht
    INNER JOIN Hierarchy h    ON h.HierarchyId = ht.HierarchyId
    INNER JOIN [User] u       ON u.UserId      = ht.UserId
    INNER JOIN [User] boss    ON boss.UserId   = ht.BossId
    OUTER APPLY (
        SELECT TOP 1
            du2.DeptUserId,
            du2.CompanyDeptId,
            du2.Title,
            du2.CompanyCodeUser
        FROM DeptUser du2
        WHERE du2.UserId = ht.UserId
        ORDER BY du2.DeptUserId
    ) du
    LEFT JOIN CompanyDept cd  ON cd.CompanyDeptId = du.CompanyDeptId
    LEFT JOIN Company c       ON c.CompanyId      = cd.CompanyId
    WHERE
        (@HierarchyTeamId IS NULL OR ht.HierarchyTeamId = @HierarchyTeamId)
        AND (@HierarchyId IS NULL OR ht.HierarchyId     = @HierarchyId)
        AND (@UserId      IS NULL OR ht.UserId           = @UserId)
        AND (@BossId      IS NULL OR ht.BossId           = @BossId)
        AND (@Active      IS NULL OR ht.Active           = @Active)
    ORDER BY boss.Name, u.Name;
END
GO
GRANT EXECUTE ON SP_Adm_sl_HierarchyTeam TO S360sys;
GO

-- =============================================================================
-- SP_Adm_in_HierarchyTeam — Insere um novo membro na equipe hierárquica
-- =============================================================================
IF OBJECT_ID('SP_Adm_in_HierarchyTeam', 'P') IS NOT NULL DROP PROCEDURE SP_Adm_in_HierarchyTeam;
GO
CREATE PROCEDURE SP_Adm_in_HierarchyTeam
    @HierarchyId   INT            ,
    @UserId        INT            ,
    @BossId        INT            ,
    @Active        BIT            = NULL,
    @token_usuario NVARCHAR(MAX)  = NULL
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO HierarchyTeam (HierarchyId, UserId, BossId, Active, DhCreate, DhUpdate)
    VALUES (@HierarchyId, @UserId, @BossId, @Active, GETDATE(), GETDATE());

    DECLARE @NewId INT = SCOPE_IDENTITY();

    SELECT
        ht.HierarchyTeamId,
        ht.HierarchyId,
        h.Name    AS HierarchyName,
        ht.UserId,
        u.Name    AS UserName,
        u.Email   AS UserEmail,
        ht.BossId,
        boss.Name AS BossName,
        boss.Email AS BossEmail,
        ht.Active,
        ht.DhCreate,
        ht.DhUpdate
    FROM HierarchyTeam ht
    INNER JOIN Hierarchy h ON h.HierarchyId = ht.HierarchyId
    INNER JOIN [User] u    ON u.UserId      = ht.UserId
    INNER JOIN [User] boss ON boss.UserId   = ht.BossId
    WHERE ht.HierarchyTeamId = @NewId;
END
GO
GRANT EXECUTE ON SP_Adm_in_HierarchyTeam TO S360sys;
GO

-- =============================================================================
-- SP_Adm_up_HierarchyTeam — Atualiza um membro da equipe hierárquica
-- =============================================================================
IF OBJECT_ID('SP_Adm_up_HierarchyTeam', 'P') IS NOT NULL DROP PROCEDURE SP_Adm_up_HierarchyTeam;
GO
CREATE PROCEDURE SP_Adm_up_HierarchyTeam
    @HierarchyTeamId INT            ,
    @HierarchyId     INT            ,
    @UserId          INT            ,
    @BossId          INT            ,
    @Active          BIT            = NULL,
    @token_usuario   NVARCHAR(MAX)  = NULL
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE HierarchyTeam
    SET HierarchyId = @HierarchyId,
        UserId      = @UserId,
        BossId      = @BossId,
        Active      = @Active,
        DhUpdate    = GETDATE()
    WHERE HierarchyTeamId = @HierarchyTeamId;

    SELECT
        ht.HierarchyTeamId,
        ht.HierarchyId,
        h.Name    AS HierarchyName,
        ht.UserId,
        u.Name    AS UserName,
        u.Email   AS UserEmail,
        ht.BossId,
        boss.Name AS BossName,
        boss.Email AS BossEmail,
        ht.Active,
        ht.DhCreate,
        ht.DhUpdate
    FROM HierarchyTeam ht
    INNER JOIN Hierarchy h ON h.HierarchyId = ht.HierarchyId
    INNER JOIN [User] u    ON u.UserId      = ht.UserId
    INNER JOIN [User] boss ON boss.UserId   = ht.BossId
    WHERE ht.HierarchyTeamId = @HierarchyTeamId;
END
GO
GRANT EXECUTE ON SP_Adm_up_HierarchyTeam TO S360sys;
GO

-- =============================================================================
-- SP_Adm_dl_HierarchyTeam — Remove um membro da equipe hierárquica
-- =============================================================================
IF OBJECT_ID('SP_Adm_dl_HierarchyTeam', 'P') IS NOT NULL DROP PROCEDURE SP_Adm_dl_HierarchyTeam;
GO
CREATE PROCEDURE SP_Adm_dl_HierarchyTeam
    @HierarchyTeamId INT            ,
    @token_usuario   NVARCHAR(MAX)  = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DELETE FROM HierarchyTeam WHERE HierarchyTeamId = @HierarchyTeamId;

    SELECT @HierarchyTeamId AS HierarchyTeamId, 'deleted' AS Status;
END
GO
GRANT EXECUTE ON SP_Adm_dl_HierarchyTeam TO S360sys;
GO
