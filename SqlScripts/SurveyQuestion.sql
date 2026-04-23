IF OBJECT_ID('SP_Adm_sl_SurveyQuestion', 'P') IS NOT NULL DROP PROCEDURE SP_Adm_sl_SurveyQuestion;
GO
IF OBJECT_ID('SP_Adm_in_SurveyQuestion', 'P') IS NOT NULL DROP PROCEDURE SP_Adm_in_SurveyQuestion;
GO
IF OBJECT_ID('SP_Adm_up_SurveyQuestion', 'P') IS NOT NULL DROP PROCEDURE SP_Adm_up_SurveyQuestion;
GO
IF OBJECT_ID('SP_dl_SurveyQuestion', 'P') IS NOT NULL DROP PROCEDURE SP_dl_SurveyQuestion;
GO
IF OBJECT_ID('SP_Adm_SurveyQuestion', 'P') IS NOT NULL DROP PROCEDURE SP_Adm_SurveyQuestion;
GO

CREATE PROCEDURE SP_Adm_SurveyQuestion
    @TypeRequest        VARCHAR(10)     ,
    @SurveyQuestionId   INT             = NULL,
    @SurveyId           INT             = NULL,
    @QuestionId         INT             = NULL,
    @token_usuario      NVARCHAR(MAX)   = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF @TypeRequest = 'SELECT'
    BEGIN
        SELECT
            SurveyQuestionId,
            SurveyId,
            QuestionId,
            DhCreate,
            DhUpdate,
            Log
        FROM SurveyQuestion
        WHERE
            (@SurveyQuestionId  IS NULL OR SurveyQuestionId = @SurveyQuestionId)
            AND (@SurveyId      IS NULL OR SurveyId         = @SurveyId)
            AND (@QuestionId    IS NULL OR QuestionId       = @QuestionId);
    END
    ELSE IF @TypeRequest = 'INSERT'
    BEGIN
        INSERT INTO SurveyQuestion
        (
            SurveyId, QuestionId, DhCreate
        )
        VALUES
        (
            @SurveyId, @QuestionId, GETDATE()
        );

        SELECT SCOPE_IDENTITY() AS SurveyQuestionId;
    END
    ELSE IF @TypeRequest = 'UPDATE'
    BEGIN
        UPDATE SurveyQuestion SET
            SurveyId    = @SurveyId,
            QuestionId  = @QuestionId,
            DhUpdate    = GETDATE()
        WHERE SurveyQuestionId = @SurveyQuestionId;
    END
    ELSE IF @TypeRequest = 'DELETE'
    BEGIN
        DELETE FROM SurveyQuestion WHERE SurveyQuestionId = @SurveyQuestionId;
    END
END
GO

GRANT EXECUTE ON [SP_Adm_SurveyQuestion] TO S360sys;
GO
