-- =============================================================================
-- SP_Adm_CustomerSeller — CRUD unificado de vendedor do cliente controlado por @TypeRequest
-- =============================================================================
IF OBJECT_ID('SP_Adm_CustomerSeller', 'P') IS NOT NULL DROP PROCEDURE SP_Adm_CustomerSeller;
GO
CREATE PROCEDURE SP_Adm_CustomerSeller
    @TypeRequest        VARCHAR(10)     ,  -- 'SELECT' | 'INSERT' | 'UPDATE' | 'DELETE'
    @CustomerSellerId   INT             = NULL,
    @CustomerId         INT             = NULL,
    @SellerUserId       INT             = NULL,
    @Active             BIT             = NULL,
    @token_usuario      NVARCHAR(MAX)   = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF @TypeRequest = 'SELECT'
    BEGIN
        SELECT
            cs.CustomerSellerId,
            cs.CustomerId,
            cu.Name         AS CustomerName,
            cu.CustomerCode,
            cs.SellerUserId,
            u.Name          AS SellerName,
            u.Email         AS SellerEmail,
            cs.Active,
            cs.DhCreate,
            cs.DhUpdate,
            cs.Log
        FROM CustomerSeller cs
        INNER JOIN Customer cu  ON cu.CustomerId = cs.CustomerId
        INNER JOIN [User] u     ON u.UserId      = cs.SellerUserId
        WHERE
            (@CustomerId    IS NULL OR cs.CustomerId    = @CustomerId)
            AND (@SellerUserId IS NULL OR cs.SellerUserId = @SellerUserId)
            AND (@Active      IS NULL OR cs.Active        = @Active)
        ORDER BY cu.Name, u.Name;
    END
    ELSE IF @TypeRequest = 'INSERT'
    BEGIN
        INSERT INTO CustomerSeller (
            CustomerId, SellerUserId, Active, DhCreate, DhUpdate
        )
        VALUES (
            @CustomerId, @SellerUserId, ISNULL(@Active, 1), GETDATE(), GETDATE()
        );

        DECLARE @NewId INT = SCOPE_IDENTITY();

        SELECT
            cs.CustomerSellerId,
            cs.CustomerId,
            cu.Name         AS CustomerName,
            cu.CustomerCode,
            cs.SellerUserId,
            u.Name          AS SellerName,
            u.Email         AS SellerEmail,
            cs.Active,
            cs.DhCreate,
            cs.DhUpdate,
            cs.Log
        FROM CustomerSeller cs
        INNER JOIN Customer cu  ON cu.CustomerId = cs.CustomerId
        INNER JOIN [User] u     ON u.UserId      = cs.SellerUserId
        WHERE cs.CustomerSellerId = @NewId;
    END
    ELSE IF @TypeRequest = 'UPDATE'
    BEGIN
        UPDATE CustomerSeller
        SET CustomerId    = @CustomerId,
            SellerUserId  = @SellerUserId,
            Active        = @Active,
            DhUpdate      = GETDATE()
        WHERE CustomerSellerId = @CustomerSellerId;

        SELECT
            cs.CustomerSellerId,
            cs.CustomerId,
            cu.Name         AS CustomerName,
            cu.CustomerCode,
            cs.SellerUserId,
            u.Name          AS SellerName,
            u.Email         AS SellerEmail,
            cs.Active,
            cs.DhCreate,
            cs.DhUpdate,
            cs.Log
        FROM CustomerSeller cs
        INNER JOIN Customer cu  ON cu.CustomerId = cs.CustomerId
        INNER JOIN [User] u     ON u.UserId      = cs.SellerUserId
        WHERE cs.CustomerSellerId = @CustomerSellerId;
    END
    ELSE IF @TypeRequest = 'DELETE'
    BEGIN
        DELETE FROM CustomerSeller WHERE CustomerSellerId = @CustomerSellerId;

        SELECT @CustomerSellerId AS CustomerSellerId, 'deleted' AS Status;
    END
END
GO
GRANT EXECUTE ON SP_Adm_CustomerSeller TO S360sys;
GO
