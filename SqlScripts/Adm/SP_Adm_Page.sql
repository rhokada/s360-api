-- ============================================================
-- SP_Adm_Page
-- TypeRequest: SELECT | INSERT | UPDATE | DELETE
-- ============================================================
CREATE OR ALTER PROCEDURE [dbo].[SP_Adm_Page]
    @TypeRequest   VARCHAR(10)      ,
    @AdmPageId     INT              = NULL,
    @Slug          VARCHAR(100)     = NULL,
    @Menu          VARCHAR(100)     = NULL,
    @Icon          VARCHAR(100)     = NULL,
    @token_usuario NVARCHAR(MAX)    = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF @TypeRequest = 'SELECT'
    BEGIN
        SELECT
            AdmPageId,
            Slug,
            Menu,
            Icon,
            DhCreate
        FROM Adm_Page
        WHERE
            (@AdmPageId IS NULL OR AdmPageId = @AdmPageId)
            AND (@Slug   IS NULL OR Slug LIKE '%' + @Slug + '%')
            AND (@Menu   IS NULL OR Menu LIKE '%' + @Menu + '%')
        ORDER BY Menu;
        RETURN;
    END

    IF @TypeRequest = 'INSERT'
    BEGIN
        INSERT INTO Adm_Page (Slug, Menu, Icon, DhCreate)
        VALUES (@Slug, @Menu, @Icon, GETDATE());

        SELECT SCOPE_IDENTITY() AS AdmPageId;
        RETURN;
    END

    IF @TypeRequest = 'UPDATE'
    BEGIN
        UPDATE Adm_Page
        SET
            Slug     = ISNULL(@Slug,  Slug),
            Menu     = ISNULL(@Menu,  Menu),
            Icon     = ISNULL(@Icon,  Icon)
        WHERE AdmPageId = @AdmPageId;

        SELECT @AdmPageId AS AdmPageId;
        RETURN;
    END

    IF @TypeRequest = 'DELETE'
    BEGIN
        DELETE FROM Adm_Page WHERE AdmPageId = @AdmPageId;
        SELECT @AdmPageId AS AdmPageId;
        RETURN;
    END
END
GO

GRANT EXECUTE ON [dbo].[SP_Adm_Page] TO [S360sys];
GO
