IF OBJECT_ID('SP_Adm_sl_Survey', 'P') IS NOT NULL DROP PROCEDURE SP_Adm_sl_Survey;
GO
IF OBJECT_ID('SP_Adm_in_Survey', 'P') IS NOT NULL DROP PROCEDURE SP_Adm_in_Survey;
GO
IF OBJECT_ID('SP_Adm_up_Survey', 'P') IS NOT NULL DROP PROCEDURE SP_Adm_up_Survey;
GO
IF OBJECT_ID('SP_dl_Survey', 'P') IS NOT NULL DROP PROCEDURE SP_dl_Survey;
GO
IF OBJECT_ID('SP_Adm_Survey', 'P') IS NOT NULL DROP PROCEDURE SP_Adm_Survey;
GO

CREATE PROCEDURE SP_Adm_Survey
    @TypeRequest    VARCHAR(10)     ,
    @SurveyId       INT             = NULL,
    @SurveyTypeId   INT             = NULL,
    @Name           VARCHAR(255)    = NULL,
    @DtIni          DATETIME        = NULL,
    @DtFin          DATETIME        = NULL,
    @token_usuario  NVARCHAR(MAX)   = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF @TypeRequest = 'SELECT'
    BEGIN
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
    ELSE IF @TypeRequest = 'INSERT'
    BEGIN
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
    ELSE IF @TypeRequest = 'UPDATE'
    BEGIN
        UPDATE Survey SET
            SurveyTypeId    = @SurveyTypeId,
            Name            = @Name,
            DtIni           = @DtIni,
            DtFin           = @DtFin,
            DhUpdate        = GETDATE()
        WHERE SurveyId = @SurveyId;
    END
    ELSE IF @TypeRequest = 'DELETE'
    BEGIN
        DELETE FROM Survey WHERE SurveyId = @SurveyId;
    END
END
GO

GRANT EXECUTE ON [SP_Adm_Survey] TO S360sys;
GO
