-- ============================================================
-- SP_Adm_Role
-- TypeRequest: SELECT | INSERT | UPDATE | DELETE
-- ============================================================
CREATE OR ALTER PROCEDURE [dbo].[SP_Adm_Role]
    @TypeRequest   VARCHAR(10)     ,
    @AdmRoleId     INT              = NULL,
    @AdmRoleCd     VARCHAR(50)      = NULL,
    @AdmRoleName   VARCHAR(100)     = NULL,
    @token_usuario NVARCHAR(MAX)    = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF @TypeRequest = 'SELECT'
    BEGIN
        SELECT
            AdmRoleId,
            AdmRoleCd,
            Name,
            DhUpdate
        FROM Adm_Role
        WHERE
            (@AdmRoleId   IS NULL OR AdmRoleId   = @AdmRoleId)
            AND (@AdmRoleCd   IS NULL OR AdmRoleCd   LIKE '%' + @AdmRoleCd   + '%')
            AND (@AdmRoleName IS NULL OR Name LIKE '%' + @AdmRoleName + '%')
        ORDER BY Name;
        RETURN;
    END

    IF @TypeRequest = 'INSERT'
    BEGIN
        INSERT INTO Adm_Role (AdmRoleCd, Name, DhUpdate)
        VALUES (@AdmRoleCd, @AdmRoleName, GETDATE());

        SELECT SCOPE_IDENTITY() AS AdmRoleId;
        RETURN;
    END

    IF @TypeRequest = 'UPDATE'
    BEGIN
        UPDATE Adm_Role
        SET
            AdmRoleCd   = ISNULL(@AdmRoleCd,   AdmRoleCd),
            Name = ISNULL(@AdmRoleName, Name),
            DhUpdate    = GETDATE()
        WHERE AdmRoleId = @AdmRoleId;

        SELECT @AdmRoleId AS AdmRoleId;
        RETURN;
    END

    IF @TypeRequest = 'DELETE'
    BEGIN
        DELETE FROM Adm_Role WHERE AdmRoleId = @AdmRoleId;
        SELECT @AdmRoleId AS AdmRoleId;
        RETURN;
    END
END
GO

GRANT EXECUTE ON [dbo].[SP_Adm_Role] TO [S360sys];
GO
