-- ============================================================
-- SP_Adm_RolePermission
-- TypeRequest: SELECT | UPSERT
--   SELECT  -> returns all pages LEFT JOIN permissions for @AdmRoleId
--   UPSERT  -> insert or update a single permission
-- ============================================================
CREATE OR ALTER PROCEDURE [dbo].[SP_Adm_RolePermission]
    @TypeRequest          VARCHAR(10)   ,
    @AdmRoleId            INT           = NULL,
    @AdmPageId            INT           = NULL,
    @AdmRolePermissionId  INT           = NULL,
    @Read                 BIT           = NULL,
    @Create               BIT           = NULL,
    @Delete               BIT           = NULL,
    @Alter                BIT           = NULL,
    @token_usuario        NVARCHAR(MAX) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF @TypeRequest = 'SELECT'
    BEGIN
        SELECT
            ISNULL(rp.AdmRolePermissionId, 0)  AS AdmRolePermissionId,
            @AdmRoleId                          AS AdmRoleId,
            p.AdmPageId,
            p.Slug,
            p.Menu,
            p.Icon,
            ISNULL(rp.[Read],   0)              AS [Read],
            ISNULL(rp.[Create], 0)              AS [Create],
            ISNULL(rp.[Delete], 0)              AS [Delete],
            ISNULL(rp.[Alter],  0)              AS [Alter]
        FROM Adm_Page p
        LEFT JOIN Adm_RolePermission rp
            ON rp.AdmPageId = p.AdmPageId
            AND rp.AdmRoleId = @AdmRoleId
        ORDER BY p.Menu;
        RETURN;
    END

    IF @TypeRequest = 'UPSERT'
    BEGIN
        IF EXISTS (
            SELECT 1 FROM Adm_RolePermission
            WHERE AdmRoleId = @AdmRoleId AND AdmPageId = @AdmPageId
        )
        BEGIN
            UPDATE Adm_RolePermission
            SET
                [Read]   = ISNULL(@Read,   [Read]),
                [Create] = ISNULL(@Create, [Create]),
                [Delete] = ISNULL(@Delete, [Delete]),
                [Alter]  = ISNULL(@Alter,  [Alter]),
                DhUpdate = GETDATE()
            WHERE AdmRoleId = @AdmRoleId AND AdmPageId = @AdmPageId;

            SELECT AdmRolePermissionId FROM Adm_RolePermission
            WHERE AdmRoleId = @AdmRoleId AND AdmPageId = @AdmPageId;
        END
        ELSE
        BEGIN
            INSERT INTO Adm_RolePermission (AdmRoleId, AdmPageId, [Read], [Create], [Delete], [Alter], DhUpdate)
            VALUES (@AdmRoleId, @AdmPageId,
                    ISNULL(@Read, 0), ISNULL(@Create, 0), ISNULL(@Delete, 0), ISNULL(@Alter, 0),
                    GETDATE());

            SELECT SCOPE_IDENTITY() AS AdmRolePermissionId;
        END
        RETURN;
    END
END
GO

GRANT EXECUTE ON [dbo].[SP_Adm_RolePermission] TO [S360sys];
GO
