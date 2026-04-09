-- =============================================
-- SP_sl_SurveyType
-- =============================================
IF OBJECT_ID('SP_sl_SurveyType', 'P') IS NOT NULL DROP PROCEDURE SP_sl_SurveyType;
GO

CREATE PROCEDURE SP_sl_SurveyType
    @SurveyTypeId   INT             = NULL,
    @SurveyTypeCd   VARCHAR(50)     = NULL,
    @Name           VARCHAR(100)    = NULL,
    @token_usuario  NVARCHAR(MAX)   = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        SurveyTypeId,
        SurveyTypeCd,
        Name,
        DhUpdate,
        Log
    FROM SurveyType
    WHERE
        (@SurveyTypeId  IS NULL OR SurveyTypeId = @SurveyTypeId)
        AND (@SurveyTypeCd IS NULL OR SurveyTypeCd = @SurveyTypeCd)
        AND (@Name      IS NULL OR Name         LIKE '%' + @Name + '%');
END
GO

GRANT EXECUTE ON SP_sl_SurveyType TO S360sys;
GO

-- =============================================
-- SP_cr_SurveyType
-- =============================================
IF OBJECT_ID('SP_cr_SurveyType', 'P') IS NOT NULL DROP PROCEDURE SP_cr_SurveyType;
GO

CREATE PROCEDURE SP_cr_SurveyType
    @SurveyTypeCd   VARCHAR(50)     ,
    @Name           VARCHAR(100)    ,
    @token_usuario  NVARCHAR(MAX)   = NULL
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO SurveyType
    (
        SurveyTypeCd, Name
    )
    VALUES
    (
        @SurveyTypeCd, @Name
    );

    SELECT SCOPE_IDENTITY() AS SurveyTypeId;
END
GO

GRANT EXECUTE ON SP_cr_SurveyType TO S360sys;
GO

-- =============================================
-- SP_up_SurveyType
-- =============================================
IF OBJECT_ID('SP_up_SurveyType', 'P') IS NOT NULL DROP PROCEDURE SP_up_SurveyType;
GO

CREATE PROCEDURE SP_up_SurveyType
    @SurveyTypeId   INT             ,
    @SurveyTypeCd   VARCHAR(50)     ,
    @Name           VARCHAR(100)    ,
    @token_usuario  NVARCHAR(MAX)   = NULL
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE SurveyType SET
        SurveyTypeCd    = @SurveyTypeCd,
        Name            = @Name,
        DhUpdate        = GETDATE()
    WHERE SurveyTypeId = @SurveyTypeId;
END
GO

GRANT EXECUTE ON SP_up_SurveyType TO S360sys;
GO

-- =============================================
-- SP_dl_SurveyType
-- =============================================
IF OBJECT_ID('SP_dl_SurveyType', 'P') IS NOT NULL DROP PROCEDURE SP_dl_SurveyType;
GO

CREATE PROCEDURE SP_dl_SurveyType
    @SurveyTypeId   INT             ,
    @token_usuario  NVARCHAR(MAX)   = NULL
AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM SurveyType WHERE SurveyTypeId = @SurveyTypeId;
END
GO

GRANT EXECUTE ON SP_dl_SurveyType TO S360sys;
GO
