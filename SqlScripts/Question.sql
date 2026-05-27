IF OBJECT_ID('SP_Adm_sl_Question', 'P') IS NOT NULL DROP PROCEDURE SP_Adm_sl_Question;
GO
IF OBJECT_ID('SP_Adm_in_Question', 'P') IS NOT NULL DROP PROCEDURE SP_Adm_in_Question;
GO
IF OBJECT_ID('SP_Adm_up_Question', 'P') IS NOT NULL DROP PROCEDURE SP_Adm_up_Question;
GO
IF OBJECT_ID('SP_dl_Question', 'P') IS NOT NULL DROP PROCEDURE SP_dl_Question;
GO
IF OBJECT_ID('SP_Adm_Question', 'P') IS NOT NULL DROP PROCEDURE SP_Adm_Question;
GO

CREATE PROCEDURE SP_Adm_Question
    @TypeRequest        VARCHAR(10)     ,
    @QuestionId         INT             = NULL,
    @IsComplement       BIT             = NULL,
    @Rank               INT             = NULL,
    @Question           VARCHAR(1000)   = NULL,
    @Description        NVARCHAR(MAX)   = NULL,
    @AnswerTypeCd       VARCHAR(50)     = NULL,
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
    @SurveyTypeId       INT             = NULL,
    @token_usuario      NVARCHAR(MAX)   = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF @TypeRequest = 'SELECT'
    BEGIN
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
            AND (@IsFeedback        IS NULL OR IsFeedback         = @IsFeedback)
            AND (@SurveyTypeId IS NULL OR EXISTS (
                SELECT 1 FROM SurveyQuestion sq
                JOIN Survey s ON sq.SurveyId = s.SurveyId
                WHERE sq.QuestionId = Question.QuestionId
                  AND s.SurveyTypeId = @SurveyTypeId
            ));
    END
    ELSE IF @TypeRequest = 'INSERT'
    BEGIN
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
    ELSE IF @TypeRequest = 'UPDATE'
    BEGIN
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
    ELSE IF @TypeRequest = 'DELETE'
    BEGIN
        DELETE FROM Question WHERE QuestionId = @QuestionId;
    END
END
GO

GRANT EXECUTE ON [SP_Adm_Question] TO S360sys;
GO
