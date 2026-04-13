-- =============================================
-- SP_Adm_sl_Survey
-- =============================================
IF OBJECT_ID('SP_Adm_sl_Survey', 'P') IS NOT NULL DROP PROCEDURE SP_Adm_sl_Survey;
GO

CREATE PROCEDURE SP_Adm_sl_Survey
    @SurveyId       INT             = NULL,
    @SurveyTypeId   INT             = NULL,
    @Name           VARCHAR(255)    = NULL,
    @token_usuario  NVARCHAR(MAX)   = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        SurveyId,
        SurveyTypeId,
        Name,
        DtIni,
        DtFin,
        DhUpdate,
        Log,
        DhCreate
    FROM Survey
    WHERE
        (@SurveyId      IS NULL OR SurveyId     = @SurveyId)
        AND (@SurveyTypeId IS NULL OR SurveyTypeId = @SurveyTypeId)
        AND (@Name       IS NULL OR Name         LIKE '%' + @Name + '%');
END
GO

GRANT EXECUTE ON SP_Adm_sl_Survey TO S360sys;
GO

-- =============================================
-- SP_Adm_in_Survey
-- =============================================
IF OBJECT_ID('SP_Adm_in_Survey', 'P') IS NOT NULL DROP PROCEDURE SP_Adm_in_Survey;
GO

CREATE PROCEDURE SP_Adm_in_Survey
    @SurveyTypeId   INT             ,
    @Name           VARCHAR(255)    ,
    @DtIni          DATETIME        ,
    @DtFin          DATETIME        = NULL,
    @token_usuario  NVARCHAR(MAX)   = NULL
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO Survey
    (
        SurveyTypeId, Name, DtIni, DtFin, DhCreate
    )
    VALUES
    (
        @SurveyTypeId, @Name, @DtIni, @DtFin, GETDATE()
    );

    SELECT SCOPE_IDENTITY() AS SurveyId;
END
GO

GRANT EXECUTE ON SP_Adm_in_Survey TO S360sys;
GO

-- =============================================
-- SP_Adm_up_Survey
-- =============================================
IF OBJECT_ID('SP_Adm_up_Survey', 'P') IS NOT NULL DROP PROCEDURE SP_Adm_up_Survey;
GO

CREATE PROCEDURE SP_Adm_up_Survey
    @SurveyId       INT             ,
    @SurveyTypeId   INT             ,
    @Name           VARCHAR(255)    ,
    @DtIni          DATETIME        ,
    @DtFin          DATETIME        = NULL,
    @token_usuario  NVARCHAR(MAX)   = NULL
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE Survey SET
        SurveyTypeId    = @SurveyTypeId,
        Name            = @Name,
        DtIni           = @DtIni,
        DtFin           = @DtFin,
        DhUpdate        = GETDATE()
    WHERE SurveyId = @SurveyId;
END
GO

GRANT EXECUTE ON SP_Adm_up_Survey TO S360sys;
GO

-- =============================================
-- SP_dl_Survey
-- =============================================
IF OBJECT_ID('SP_dl_Survey', 'P') IS NOT NULL DROP PROCEDURE SP_dl_Survey;
GO

CREATE PROCEDURE SP_dl_Survey
    @SurveyId       INT             ,
    @token_usuario  NVARCHAR(MAX)   = NULL
AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM Survey WHERE SurveyId = @SurveyId;
END
GO

GRANT EXECUTE ON SP_dl_Survey TO S360sys;
GO
