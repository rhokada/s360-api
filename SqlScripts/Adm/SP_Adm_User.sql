-- =============================================================================
-- DROP das procedures individuais de User
-- =============================================================================
IF OBJECT_ID('SP_Adm_sl_User', 'P') IS NOT NULL DROP PROCEDURE SP_Adm_sl_User;
GO
IF OBJECT_ID('SP_Adm_in_User', 'P') IS NOT NULL DROP PROCEDURE SP_Adm_in_User;
GO
IF OBJECT_ID('SP_Adm_up_User', 'P') IS NOT NULL DROP PROCEDURE SP_Adm_up_User;
GO
IF OBJECT_ID('SP_Adm_dl_User', 'P') IS NOT NULL DROP PROCEDURE SP_Adm_dl_User;
GO

-- =============================================================================
-- SP_Adm_User — CRUD unificado de usuários controlado por @TypeRequest
-- =============================================================================
IF OBJECT_ID('SP_Adm_User', 'P') IS NOT NULL DROP PROCEDURE SP_Adm_User;
GO
CREATE PROCEDURE SP_Adm_User
    @TypeRequest   VARCHAR(10)    ,   -- 'SELECT' | 'INSERT' | 'UPDATE' | 'DELETE'
    @UserId        INT            = NULL,
    @AppId         VARCHAR(100)   = NULL,
    @Name          VARCHAR(500)   = NULL,
    @Email         VARCHAR(100)   = NULL,
    @DddCell       VARCHAR(5)     = NULL,
    @NrCell        VARCHAR(20)    = NULL,
    @Active        BIT            = NULL,
    @ContractId    INT            = NULL,
    @PbiLogin      VARCHAR(100)   = NULL,
    @token_usuario NVARCHAR(MAX)  = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF @TypeRequest = 'SELECT'
    BEGIN
        SELECT
            u.UserId,
            u.AppId,
            u.Name,
            u.Email,
            u.DddCell,
            u.NrCell,
            u.Active,
            u.DhCreate,
            u.DhUpdate,
            u.ContractId,
            u.PbiLogin,
            u.Log,
            du.Title,
            du.CompanyCodeUser
        FROM [User] u
        OUTER APPLY (
            SELECT TOP 1 du2.Title, du2.CompanyCodeUser
            FROM DeptUser du2
            WHERE du2.UserId = u.UserId
            ORDER BY du2.DeptUserId
        ) du
        WHERE
            (@UserId     IS NULL OR u.UserId     = @UserId)
            AND (@AppId  IS NULL OR u.AppId      = @AppId)
            AND (@Name   IS NULL OR u.Name       LIKE '%' + @Name + '%')
            AND (@Email  IS NULL OR u.Email      LIKE '%' + @Email + '%')
            AND (@Active IS NULL OR u.Active     = @Active)
            AND (@ContractId IS NULL OR u.ContractId = @ContractId)
        ORDER BY u.Name;
    END
    ELSE IF @TypeRequest = 'INSERT'
    BEGIN
        INSERT INTO [User] (AppId, Name, Email, DddCell, NrCell, Active, DhCreate, DhUpdate, ContractId, PbiLogin)
        VALUES (@AppId, @Name, @Email, @DddCell, @NrCell, @Active, GETDATE(), GETDATE(), @ContractId, @PbiLogin);

        SELECT
            u.UserId,
            u.AppId,
            u.Name,
            u.Email,
            u.DddCell,
            u.NrCell,
            u.Active,
            u.DhCreate,
            u.DhUpdate,
            u.ContractId,
            u.PbiLogin
        FROM [User] u
        WHERE u.UserId = SCOPE_IDENTITY();
    END
    ELSE IF @TypeRequest = 'UPDATE'
    BEGIN
        UPDATE [User]
        SET
            AppId      = @AppId,
            Name       = @Name,
            Email      = @Email,
            DddCell    = @DddCell,
            NrCell     = @NrCell,
            Active     = @Active,
            DhUpdate   = GETDATE(),
            ContractId = @ContractId,
            PbiLogin   = @PbiLogin
        WHERE UserId = @UserId;

        SELECT
            u.UserId,
            u.AppId,
            u.Name,
            u.Email,
            u.DddCell,
            u.NrCell,
            u.Active,
            u.DhCreate,
            u.DhUpdate,
            u.ContractId,
            u.PbiLogin
        FROM [User] u
        WHERE u.UserId = @UserId;
    END
    ELSE IF @TypeRequest = 'DELETE'
    BEGIN
        UPDATE [User]
        SET Active   = 0,
            DhUpdate = GETDATE()
        WHERE UserId = @UserId;

        SELECT @UserId AS UserId, 'deleted' AS Status;
    END
END
GO
GRANT EXECUTE ON SP_Adm_User TO S360sys;
GO
