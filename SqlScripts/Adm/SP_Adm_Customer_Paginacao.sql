-- ============================================================
-- SP_Adm_Customer — adiciona paginação e filtro por vendedor
-- Altera apenas o bloco SELECT, sem tocar em INSERT/UPDATE/DELETE
-- ============================================================
CREATE OR ALTER PROCEDURE [dbo].[SP_Adm_Customer]
    @TypeRequest    VARCHAR(10)      ,  -- 'SELECT' | 'INSERT' | 'UPDATE' | 'DELETE'
    @CustomerId     INT             = NULL,
    @CompanyId      INT             = NULL,
    @ToBeConfirmed  BIT             = NULL,
    @CustomerCode   VARCHAR(50)     = NULL,
    @Name           VARCHAR(200)    = NULL,
    @CNPJ           VARCHAR(18)     = NULL,
    @Street         VARCHAR(200)    = NULL,
    @Street2        VARCHAR(100)    = NULL,
    @Neighborhood   VARCHAR(100)    = NULL,
    @City           VARCHAR(100)    = NULL,
    @State          VARCHAR(100)    = NULL,
    @ZipCode        VARCHAR(20)     = NULL,
    @OriginCd       VARCHAR(50)     = NULL,
    @DataImportId   INT             = NULL,
    -- novos parâmetros de filtro e paginação
    @SellerFilter   NVARCHAR(200)   = NULL,
    @PageNumber     INT             = 1,
    @PageSize       INT             = 20,
    @token_usuario  NVARCHAR(MAX)   = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF @TypeRequest = 'SELECT'
    BEGIN
        SELECT
            cu.CustomerId,
            cu.CompanyId,
            c.Name          AS CompanyName,
            cu.ToBeConfirmed,
            cu.CustomerCode,
            cu.Name,
            cu.CNPJ,
            cu.Street,
            cu.Street2,
            cu.Neighborhood,
            cu.City,
            cu.State,
            cu.ZipCode,
            cu.DhCreate,
            cu.DhUpdate,
            cu.Log,
            cu.guId,
            cu.OriginCd,
            cu.DataImportId,
            COUNT(*) OVER()  AS TotalCount
        FROM Customer cu
        INNER JOIN Company c ON c.CompanyId = cu.CompanyId
        WHERE
            (@CustomerId    IS NULL OR cu.CustomerId    =  @CustomerId)
            AND (@CompanyId     IS NULL OR cu.CompanyId     =  @CompanyId)
            AND (@Name          IS NULL OR cu.Name          LIKE '%' + @Name         + '%')
            AND (@CustomerCode  IS NULL OR cu.CustomerCode  LIKE '%' + @CustomerCode + '%')
            AND (@CNPJ          IS NULL OR cu.CNPJ          =  @CNPJ)
            AND (@City          IS NULL OR cu.City          LIKE '%' + @City         + '%')
            AND (@State         IS NULL OR cu.State         LIKE '%' + @State        + '%')
            AND (@ToBeConfirmed IS NULL OR cu.ToBeConfirmed =  @ToBeConfirmed)
            AND (@SellerFilter  IS NULL OR EXISTS (
                SELECT 1
                FROM   CustomerSeller cs
                INNER JOIN [User] u ON u.UserId = cs.SellerUserId
                WHERE  cs.CustomerId = cu.CustomerId
                  AND  (u.Name  LIKE '%' + @SellerFilter + '%'
                     OR u.Email LIKE '%' + @SellerFilter + '%')
            ))
        ORDER BY cu.Name
        OFFSET (@PageNumber - 1) * @PageSize ROWS
        FETCH NEXT @PageSize ROWS ONLY;
        RETURN;
    END

    IF @TypeRequest = 'INSERT'
    BEGIN
        INSERT INTO Customer (
            CompanyId, ToBeConfirmed, CustomerCode, Name, CNPJ,
            Street, Street2, Neighborhood, City, State, ZipCode,
            DhCreate, DhUpdate, OriginCd, DataImportId
        )
        VALUES (
            @CompanyId, ISNULL(@ToBeConfirmed, 1), @CustomerCode, @Name, @CNPJ,
            @Street, @Street2, @Neighborhood, @City, @State, @ZipCode,
            GETDATE(), GETDATE(), @OriginCd, @DataImportId
        );

        DECLARE @NewId INT = SCOPE_IDENTITY();

        SELECT
            cu.CustomerId,
            cu.CompanyId,
            c.Name          AS CompanyName,
            cu.ToBeConfirmed,
            cu.CustomerCode,
            cu.Name,
            cu.CNPJ,
            cu.Street,
            cu.Street2,
            cu.Neighborhood,
            cu.City,
            cu.State,
            cu.ZipCode,
            cu.DhCreate,
            cu.DhUpdate,
            cu.Log,
            cu.guId,
            cu.OriginCd,
            cu.DataImportId,
            1 AS TotalCount
        FROM Customer cu
        INNER JOIN Company c ON c.CompanyId = cu.CompanyId
        WHERE cu.CustomerId = @NewId;
        RETURN;
    END

    IF @TypeRequest = 'UPDATE'
    BEGIN
        UPDATE Customer
        SET CompanyId      = @CompanyId,
            ToBeConfirmed  = @ToBeConfirmed,
            CustomerCode   = @CustomerCode,
            Name           = @Name,
            CNPJ           = @CNPJ,
            Street         = @Street,
            Street2        = @Street2,
            Neighborhood   = @Neighborhood,
            City           = @City,
            State          = @State,
            ZipCode        = @ZipCode,
            OriginCd       = @OriginCd,
            DataImportId   = @DataImportId,
            DhUpdate       = GETDATE()
        WHERE CustomerId = @CustomerId;

        SELECT
            cu.CustomerId,
            cu.CompanyId,
            c.Name          AS CompanyName,
            cu.ToBeConfirmed,
            cu.CustomerCode,
            cu.Name,
            cu.CNPJ,
            cu.Street,
            cu.Street2,
            cu.Neighborhood,
            cu.City,
            cu.State,
            cu.ZipCode,
            cu.DhCreate,
            cu.DhUpdate,
            cu.Log,
            cu.guId,
            cu.OriginCd,
            cu.DataImportId,
            1 AS TotalCount
        FROM Customer cu
        INNER JOIN Company c ON c.CompanyId = cu.CompanyId
        WHERE cu.CustomerId = @CustomerId;
        RETURN;
    END

    IF @TypeRequest = 'DELETE'
    BEGIN
        DELETE FROM Customer WHERE CustomerId = @CustomerId;
        SELECT @CustomerId AS CustomerId, 'deleted' AS Status;
        RETURN;
    END
END
GO

GRANT EXECUTE ON [dbo].[SP_Adm_Customer] TO [S360sys];
GO
