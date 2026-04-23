-- =============================================================================
-- DROP das procedures individuais de DeptUser
-- =============================================================================
IF OBJECT_ID('SP_Adm_sl_DeptUser', 'P') IS NOT NULL DROP PROCEDURE SP_Adm_sl_DeptUser;
GO
IF OBJECT_ID('SP_Adm_in_DeptUser', 'P') IS NOT NULL DROP PROCEDURE SP_Adm_in_DeptUser;
GO
IF OBJECT_ID('SP_Adm_up_DeptUser', 'P') IS NOT NULL DROP PROCEDURE SP_Adm_up_DeptUser;
GO
IF OBJECT_ID('SP_Adm_dl_DeptUser', 'P') IS NOT NULL DROP PROCEDURE SP_Adm_dl_DeptUser;
GO

-- =============================================================================
-- SP_Adm_DeptUser — CRUD unificado de vínculos dept-usuário controlado por @TypeRequest
-- =============================================================================
IF OBJECT_ID('SP_Adm_DeptUser', 'P') IS NOT NULL DROP PROCEDURE SP_Adm_DeptUser;
GO
CREATE PROCEDURE SP_Adm_DeptUser
    @TypeRequest     VARCHAR(10)   ,   -- 'SELECT' | 'INSERT' | 'UPDATE' | 'DELETE'
    @DeptUserId      INT            = NULL,
    @UserId          INT            = NULL,
    @CompanyDeptId   INT            = NULL,
    @Title           VARCHAR(200)   = NULL,
    @CompanyCodeUser VARCHAR(100)   = NULL,
    @token_usuario   NVARCHAR(MAX)  = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF @TypeRequest = 'SELECT'
    BEGIN
        SELECT
            du.DeptUserId,
            du.UserId,
            u.Name      AS UserName,
            u.Email     AS UserEmail,
            du.CompanyDeptId,
            cd.Name     AS DeptName,
            c.Name      AS CompanyName,
            du.Title,
            du.CompanyCodeUser,
            du.DhUpdate,
            du.Log
        FROM DeptUser du
        INNER JOIN [User]      u  ON u.UserId       = du.UserId
        INNER JOIN CompanyDept cd ON cd.CompanyDeptId = du.CompanyDeptId
        INNER JOIN Company     c  ON c.CompanyId     = cd.CompanyId
        WHERE
            (@DeptUserId      IS NULL OR du.DeptUserId      = @DeptUserId)
            AND (@UserId           IS NULL OR du.UserId           = @UserId)
            AND (@CompanyDeptId    IS NULL OR du.CompanyDeptId    = @CompanyDeptId)
            AND (@Title            IS NULL OR du.Title            LIKE '%' + @Title + '%')
            AND (@CompanyCodeUser  IS NULL OR du.CompanyCodeUser  = @CompanyCodeUser)
        ORDER BY u.Name, cd.Name;
    END
    ELSE IF @TypeRequest = 'INSERT'
    BEGIN
        INSERT INTO DeptUser (UserId, CompanyDeptId, Title, CompanyCodeUser, DhUpdate)
        VALUES (@UserId, @CompanyDeptId, @Title, @CompanyCodeUser, GETDATE());

        DECLARE @NewId INT = SCOPE_IDENTITY();

        SELECT
            du.DeptUserId,
            du.UserId,
            u.Name  AS UserName,
            u.Email AS UserEmail,
            du.CompanyDeptId,
            cd.Name AS DeptName,
            c.Name  AS CompanyName,
            du.Title,
            du.CompanyCodeUser,
            du.DhUpdate
        FROM DeptUser du
        INNER JOIN [User]      u  ON u.UserId        = du.UserId
        INNER JOIN CompanyDept cd ON cd.CompanyDeptId = du.CompanyDeptId
        INNER JOIN Company     c  ON c.CompanyId      = cd.CompanyId
        WHERE du.DeptUserId = @NewId;
    END
    ELSE IF @TypeRequest = 'UPDATE'
    BEGIN
        UPDATE DeptUser
        SET UserId          = @UserId,
            CompanyDeptId   = @CompanyDeptId,
            Title           = @Title,
            CompanyCodeUser = @CompanyCodeUser,
            DhUpdate        = GETDATE()
        WHERE DeptUserId = @DeptUserId;

        SELECT
            du.DeptUserId,
            du.UserId,
            u.Name  AS UserName,
            u.Email AS UserEmail,
            du.CompanyDeptId,
            cd.Name AS DeptName,
            c.Name  AS CompanyName,
            du.Title,
            du.CompanyCodeUser,
            du.DhUpdate
        FROM DeptUser du
        INNER JOIN [User]      u  ON u.UserId        = du.UserId
        INNER JOIN CompanyDept cd ON cd.CompanyDeptId = du.CompanyDeptId
        INNER JOIN Company     c  ON c.CompanyId      = cd.CompanyId
        WHERE du.DeptUserId = @DeptUserId;
    END
    ELSE IF @TypeRequest = 'DELETE'
    BEGIN
        DELETE FROM DeptUser WHERE DeptUserId = @DeptUserId;

        SELECT @DeptUserId AS DeptUserId, 'deleted' AS Status;
    END
END
GO
GRANT EXECUTE ON SP_Adm_DeptUser TO S360sys;
GO
