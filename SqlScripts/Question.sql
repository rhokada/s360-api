-- =============================================
-- SP_sl_Question
-- =============================================
IF OBJECT_ID('SP_sl_Question', 'P') IS NOT NULL DROP PROCEDURE SP_sl_Question;
GO

CREATE PROCEDURE SP_sl_Question
    @QuestionId         INT             = NULL,
    @IsComplement       BIT             = NULL,
    @Rank               INT             = NULL,
    @Question           VARCHAR(1000)   = NULL,
    @AnswerTypeCd       VARCHAR(50)     = NULL,
    @Group              VARCHAR(100)    = NULL,
    @Metric             VARCHAR(500)    = NULL,
    @IsFirstSurvey      BIT             = NULL,
    @IsFinalSurvey      BIT             = NULL,
    @IsCompetenceLevel  BIT             = NULL,
    @IsFinishEarly      BIT             = NULL,
    @IsStandardMetric   BIT             = NULL,
    @IsSglYesNoType     BIT             = NULL,
    @IsFeedback         BIT             = NULL,
    @token_usuario      NVARCHAR(MAX)   = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        QuestionId,
        IsComplement,
        Rank,
        Question,
        Description,
        AnswerTypeCd,
        DhCreate,
        DhUpdate,
        Log,
        [Group],
        Metric,
        IconMetric,
        IsFirstSurvey,
        IsFinalSurvey,
        IsCompetenceLevel,
        IsFinishEarly,
        IsStandardMetric,
        IsSglYesNoType,
        IsFeedback
    FROM Question
    WHERE
        (@QuestionId        IS NULL OR QuestionId        = @QuestionId)
        AND (@IsComplement  IS NULL OR IsComplement       = @IsComplement)
        AND (@Rank          IS NULL OR Rank               = @Rank)
        AND (@Question      IS NULL OR Question           LIKE '%' + @Question + '%')
        AND (@AnswerTypeCd  IS NULL OR AnswerTypeCd       = @AnswerTypeCd)
        AND (@Group         IS NULL OR [Group]            = @Group)
        AND (@Metric        IS NULL OR Metric             = @Metric)
        AND (@IsFirstSurvey     IS NULL OR IsFirstSurvey      = @IsFirstSurvey)
        AND (@IsFinalSurvey     IS NULL OR IsFinalSurvey      = @IsFinalSurvey)
        AND (@IsCompetenceLevel IS NULL OR IsCompetenceLevel  = @IsCompetenceLevel)
        AND (@IsFinishEarly     IS NULL OR IsFinishEarly      = @IsFinishEarly)
        AND (@IsStandardMetric  IS NULL OR IsStandardMetric   = @IsStandardMetric)
        AND (@IsSglYesNoType    IS NULL OR IsSglYesNoType     = @IsSglYesNoType)
        AND (@IsFeedback        IS NULL OR IsFeedback         = @IsFeedback);
END
GO

GRANT EXECUTE ON SP_sl_Question TO S360sys;
GO

-- =============================================
-- SP_cr_Question
-- =============================================
IF OBJECT_ID('SP_cr_Question', 'P') IS NOT NULL DROP PROCEDURE SP_cr_Question;
GO

CREATE PROCEDURE SP_cr_Question
    @IsComplement       BIT             NOT NULL,
    @Rank               INT             NOT NULL,
    @Question           VARCHAR(1000)   NOT NULL,
    @Description        NVARCHAR(MAX)   = NULL,
    @AnswerTypeCd       VARCHAR(50)     NOT NULL,
    @Group              VARCHAR(100)    = NULL,
    @Metric             VARCHAR(500)    = NULL,
    @IconMetric         VARCHAR(1000)   = NULL,
    @IsFirstSurvey      BIT             = NULL,
    @IsFinalSurvey      BIT             = NULL,
    @IsCompetenceLevel  BIT             = NULL,
    @IsFinishEarly      BIT             = NULL,
    @IsStandardMetric   BIT             = NULL,
    @IsSglYesNoType     BIT             = NULL,
    @IsFeedback         BIT             = NULL,
    @token_usuario      NVARCHAR(MAX)   = NULL
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO Question
    (
        IsComplement, Rank, Question, Description, AnswerTypeCd,
        DhCreate, [Group], Metric, IconMetric,
        IsFirstSurvey, IsFinalSurvey, IsCompetenceLevel,
        IsFinishEarly, IsStandardMetric, IsSglYesNoType, IsFeedback
    )
    VALUES
    (
        @IsComplement, @Rank, @Question, @Description, @AnswerTypeCd,
        GETDATE(), @Group, @Metric, @IconMetric,
        @IsFirstSurvey, @IsFinalSurvey, @IsCompetenceLevel,
        @IsFinishEarly, @IsStandardMetric, @IsSglYesNoType, @IsFeedback
    );

    SELECT SCOPE_IDENTITY() AS QuestionId;
END
GO

GRANT EXECUTE ON SP_cr_Question TO S360sys;
GO

-- =============================================
-- SP_up_Question
-- =============================================
IF OBJECT_ID('SP_up_Question', 'P') IS NOT NULL DROP PROCEDURE SP_up_Question;
GO

CREATE PROCEDURE SP_up_Question
    @QuestionId         INT             NOT NULL,
    @IsComplement       BIT             NOT NULL,
    @Rank               INT             NOT NULL,
    @Question           VARCHAR(1000)   NOT NULL,
    @Description        NVARCHAR(MAX)   = NULL,
    @AnswerTypeCd       VARCHAR(50)     NOT NULL,
    @Group              VARCHAR(100)    = NULL,
    @Metric             VARCHAR(500)    = NULL,
    @IconMetric         VARCHAR(1000)   = NULL,
    @IsFirstSurvey      BIT             = NULL,
    @IsFinalSurvey      BIT             = NULL,
    @IsCompetenceLevel  BIT             = NULL,
    @IsFinishEarly      BIT             = NULL,
    @IsStandardMetric   BIT             = NULL,
    @IsSglYesNoType     BIT             = NULL,
    @IsFeedback         BIT             = NULL,
    @token_usuario      NVARCHAR(MAX)   = NULL
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE Question SET
        IsComplement        = @IsComplement,
        Rank                = @Rank,
        Question            = @Question,
        Description         = @Description,
        AnswerTypeCd        = @AnswerTypeCd,
        DhUpdate            = GETDATE(),
        [Group]             = @Group,
        Metric              = @Metric,
        IconMetric          = @IconMetric,
        IsFirstSurvey       = @IsFirstSurvey,
        IsFinalSurvey       = @IsFinalSurvey,
        IsCompetenceLevel   = @IsCompetenceLevel,
        IsFinishEarly       = @IsFinishEarly,
        IsStandardMetric    = @IsStandardMetric,
        IsSglYesNoType      = @IsSglYesNoType,
        IsFeedback          = @IsFeedback
    WHERE QuestionId = @QuestionId;
END
GO

GRANT EXECUTE ON SP_up_Question TO S360sys;
GO

-- =============================================
-- SP_dl_Question
-- =============================================
IF OBJECT_ID('SP_dl_Question', 'P') IS NOT NULL DROP PROCEDURE SP_dl_Question;
GO

CREATE PROCEDURE SP_dl_Question
    @QuestionId     INT             NOT NULL,
    @token_usuario  NVARCHAR(MAX)   = NULL
AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM Question WHERE QuestionId = @QuestionId;
END
GO

GRANT EXECUTE ON SP_dl_Question TO S360sys;
GO
