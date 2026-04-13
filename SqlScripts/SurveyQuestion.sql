-- =============================================
-- SP_Adm_sl_SurveyQuestion
-- =============================================
IF OBJECT_ID('SP_Adm_sl_SurveyQuestion', 'P') IS NOT NULL DROP PROCEDURE SP_Adm_sl_SurveyQuestion;
GO

CREATE PROCEDURE SP_Adm_sl_SurveyQuestion
    @SurveyQuestionId   INT = NULL,
    @SurveyId           INT = NULL,
    @QuestionId         INT             = NULL,
    @token_usuario      NVARCHAR(MAX)   = NULL
AS
BEGIN
    SET NOCOUNT ON;

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
GO

GRANT EXECUTE ON SP_Adm_sl_SurveyQuestion TO S360sys;
GO

-- =============================================
-- SP_Adm_in_SurveyQuestion
-- =============================================
IF OBJECT_ID('SP_Adm_in_SurveyQuestion', 'P') IS NOT NULL DROP PROCEDURE SP_Adm_in_SurveyQuestion;
GO

CREATE PROCEDURE SP_Adm_in_SurveyQuestion
    @SurveyId   INT NOT NULL,
    @QuestionId     INT             NOT NULL,
    @token_usuario  NVARCHAR(MAX)   = NULL
AS
BEGIN
    SET NOCOUNT ON;

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
GO

GRANT EXECUTE ON SP_Adm_in_SurveyQuestion TO S360sys;
GO

-- =============================================
-- SP_Adm_up_SurveyQuestion
-- =============================================
IF OBJECT_ID('SP_Adm_up_SurveyQuestion', 'P') IS NOT NULL DROP PROCEDURE SP_Adm_up_SurveyQuestion;
GO

CREATE PROCEDURE SP_Adm_up_SurveyQuestion
    @SurveyQuestionId   INT NOT NULL,
    @SurveyId           INT NOT NULL,
    @QuestionId         INT             NOT NULL,
    @token_usuario      NVARCHAR(MAX)   = NULL
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE SurveyQuestion SET
        SurveyId    = @SurveyId,
        QuestionId  = @QuestionId,
        DhUpdate    = GETDATE()
    WHERE SurveyQuestionId = @SurveyQuestionId;
END
GO

GRANT EXECUTE ON SP_Adm_up_SurveyQuestion TO S360sys;
GO

-- =============================================
-- SP_dl_SurveyQuestion
-- =============================================
IF OBJECT_ID('SP_dl_SurveyQuestion', 'P') IS NOT NULL DROP PROCEDURE SP_dl_SurveyQuestion;
GO

CREATE PROCEDURE SP_dl_SurveyQuestion
    @SurveyQuestionId   INT             NOT NULL,
    @token_usuario      NVARCHAR(MAX)   = NULL
AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM SurveyQuestion WHERE SurveyQuestionId = @SurveyQuestionId;
END
GO

GRANT EXECUTE ON SP_dl_SurveyQuestion TO S360sys;
GO
