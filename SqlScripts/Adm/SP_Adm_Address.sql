-- =============================================================================
-- SP_Adm_sl_Address — Seleciona endereços com filtros opcionais
-- =============================================================================
IF OBJECT_ID('SP_Adm_sl_Address', 'P') IS NOT NULL DROP PROCEDURE SP_Adm_sl_Address;
GO
CREATE PROCEDURE SP_Adm_sl_Address
    @AddressId     INT            = NULL,
    @Street        VARCHAR(500)   = NULL,
    @Neighborhood  VARCHAR(200)   = NULL,
    @City          VARCHAR(200)   = NULL,
    @State         VARCHAR(100)   = NULL,
    @ZipCode       VARCHAR(20)    = NULL,
    @Country       VARCHAR(100)   = NULL,
    @token_usuario NVARCHAR(MAX)  = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        a.AddressId,
        a.Street,
        a.Street2,
        a.Neighborhood,
        a.City,
        a.State,
        a.ZipCode,
        a.Country,
        a.DhUpdate,
        a.Log
    FROM Address a
    WHERE
        (@AddressId    IS NULL OR a.AddressId    = @AddressId)
        AND (@Street        IS NULL OR a.Street        LIKE '%' + @Street + '%')
        AND (@Neighborhood  IS NULL OR a.Neighborhood  = @Neighborhood)
        AND (@City          IS NULL OR a.City           = @City)
        AND (@State         IS NULL OR a.State          = @State)
        AND (@ZipCode       IS NULL OR a.ZipCode        = @ZipCode)
        AND (@Country       IS NULL OR a.Country        = @Country)
    ORDER BY a.City, a.Street;
END
GO
GRANT EXECUTE ON SP_Adm_sl_Address TO S360sys;
GO

-- =============================================================================
-- SP_Adm_in_Address — Insere um novo endereço
-- =============================================================================
IF OBJECT_ID('SP_Adm_in_Address', 'P') IS NOT NULL DROP PROCEDURE SP_Adm_in_Address;
GO
CREATE PROCEDURE SP_Adm_in_Address
    @Street        VARCHAR(500)   = NULL,
    @Street2       VARCHAR(500)   = NULL,
    @Neighborhood  VARCHAR(200)   = NULL,
    @City          VARCHAR(200)   = NULL,
    @State         VARCHAR(100)   = NULL,
    @ZipCode       VARCHAR(20)    = NULL,
    @Country       VARCHAR(100)   = NULL,
    @token_usuario NVARCHAR(MAX)  = NULL
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO Address (Street, Street2, Neighborhood, City, State, ZipCode, Country, DhUpdate)
    VALUES (@Street, @Street2, @Neighborhood, @City, @State, @ZipCode, @Country, GETDATE());

    SELECT
        a.AddressId,
        a.Street,
        a.Street2,
        a.Neighborhood,
        a.City,
        a.State,
        a.ZipCode,
        a.Country,
        a.DhUpdate
    FROM Address a
    WHERE a.AddressId = SCOPE_IDENTITY();
END
GO
GRANT EXECUTE ON SP_Adm_in_Address TO S360sys;
GO

-- =============================================================================
-- SP_Adm_up_Address — Atualiza um endereço existente
-- =============================================================================
IF OBJECT_ID('SP_Adm_up_Address', 'P') IS NOT NULL DROP PROCEDURE SP_Adm_up_Address;
GO
CREATE PROCEDURE SP_Adm_up_Address
    @AddressId     INT            NOT NULL,
    @Street        VARCHAR(500)   = NULL,
    @Street2       VARCHAR(500)   = NULL,
    @Neighborhood  VARCHAR(200)   = NULL,
    @City          VARCHAR(200)   = NULL,
    @State         VARCHAR(100)   = NULL,
    @ZipCode       VARCHAR(20)    = NULL,
    @Country       VARCHAR(100)   = NULL,
    @token_usuario NVARCHAR(MAX)  = NULL
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE Address
    SET Street       = @Street,
        Street2      = @Street2,
        Neighborhood = @Neighborhood,
        City         = @City,
        State        = @State,
        ZipCode      = @ZipCode,
        Country      = @Country,
        DhUpdate     = GETDATE()
    WHERE AddressId = @AddressId;

    SELECT
        a.AddressId,
        a.Street,
        a.Street2,
        a.Neighborhood,
        a.City,
        a.State,
        a.ZipCode,
        a.Country,
        a.DhUpdate
    FROM Address a
    WHERE a.AddressId = @AddressId;
END
GO
GRANT EXECUTE ON SP_Adm_up_Address TO S360sys;
GO

-- =============================================================================
-- SP_Adm_dl_Address — Remove um endereço
-- =============================================================================
IF OBJECT_ID('SP_Adm_dl_Address', 'P') IS NOT NULL DROP PROCEDURE SP_Adm_dl_Address;
GO
CREATE PROCEDURE SP_Adm_dl_Address
    @AddressId     INT            NOT NULL,
    @token_usuario NVARCHAR(MAX)  = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DELETE FROM Address WHERE AddressId = @AddressId;

    SELECT @AddressId AS AddressId, 'deleted' AS Status;
END
GO
GRANT EXECUTE ON SP_Adm_dl_Address TO S360sys;
GO
