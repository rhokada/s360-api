-- ============================================================
-- SP_Adm_RoleUser
-- TypeRequest: SELECT | INSERT | DELETE
-- ============================================================
CREATE OR ALTER PROCEDURE [dbo].[SP_Adm_RoleUser]
    @TypeRequest   VARCHAR(10)   ,
    @AdmRoleUserId INT           = NULL,
    @AdmRoleId     INT           = NULL,
    @UserId        INT           = NULL,
    @token_usuario NVARCHAR(MAX) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF @TypeRequest = 'SELECT'
    BEGIN
        SELECT
            ru.AdmUserRoleId AS AdmRoleUserId,
            ru.AdmRoleId,
            r.Name AS AdmRoleName,
            r.AdmRoleCd,
            ru.UserId,
            u.Name  AS UserName,
            u.Email AS UserEmail,
            ru.DhUpdate
        FROM Adm_RoleUser ru
        INNER JOIN Adm_Role r ON r.AdmRoleId = ru.AdmRoleId
        INNER JOIN Users    u ON u.UserId    = ru.UserId
        WHERE
            (@AdmRoleId IS NULL OR ru.AdmRoleId = @AdmRoleId)
            AND (@UserId    IS NULL OR ru.UserId    = @UserId)
        ORDER BY r.Name, u.Name;
        RETURN;
    END

    IF @TypeRequest = 'INSERT'
    BEGIN
        IF NOT EXISTS (
            SELECT 1 FROM Adm_RoleUser
            WHERE AdmRoleId = @AdmRoleId AND UserId = @UserId
        )
        BEGIN
            INSERT INTO Adm_RoleUser (AdmRoleId, UserId, DhUpdate)
            VALUES (@AdmRoleId, @UserId, GETDATE());
        END

        SELECT SCOPE_IDENTITY() AS AdmRoleUserId;
        RETURN;
    END

    IF @TypeRequest = 'DELETE'
    BEGIN
        IF @AdmRoleUserId IS NOT NULL
            DELETE FROM Adm_RoleUser WHERE AdmUserRoleId = @AdmRoleUserId;
        ELSE
            DELETE FROM Adm_RoleUser WHERE AdmRoleId = @AdmRoleId AND UserId = @UserId;

        SELECT ISNULL(@AdmRoleUserId, 0) AS AdmRoleUserId;
        RETURN;
    END
END
GO

GRANT EXECUTE ON [dbo].[SP_Adm_RoleUser] TO [S360sys];
GO
