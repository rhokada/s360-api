IF OBJECT_ID('SP_Adm_sl_QuestionOption', 'P') IS NOT NULL DROP PROCEDURE SP_Adm_sl_QuestionOption;
GO
IF OBJECT_ID('SP_Adm_in_QuestionOption', 'P') IS NOT NULL DROP PROCEDURE SP_Adm_in_QuestionOption;
GO
IF OBJECT_ID('SP_Adm_up_QuestionOption', 'P') IS NOT NULL DROP PROCEDURE SP_Adm_up_QuestionOption;
GO
IF OBJECT_ID('SP_dl_QuestionOption', 'P') IS NOT NULL DROP PROCEDURE SP_dl_QuestionOption;
GO
IF OBJECT_ID('SP_Adm_QuestionOption', 'P') IS NOT NULL DROP PROCEDURE SP_Adm_QuestionOption;
GO

CREATE PROCEDURE SP_Adm_QuestionOption
    @TypeRequest            VARCHAR(10)     ,
    @QuestionOptionId       INT             = NULL,
    @QuestionId             INT             = NULL,
    @ComplementQuestionId   INT             = NULL,
    @Rank                   INT             = NULL,
    @OptionCd               VARCHAR(50)     = NULL,
    @Description            VARCHAR(255)    = NULL,
    @OpenMsgBox             BIT             = NULL,
    @NeedNotes              BIT             = NULL,
    @token_usuario          NVARCHAR(MAX)   = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF @TypeRequest = 'SELECT'
    BEGIN
        SELECT
            QuestionOptionId,
            QuestionId,
            ComplementQuestionId,
            Rank,
            OptionCd,
            Description,
            DhCreate,
            DhUpdate,
            Log,
            OpenMsgBox,
            NeedNotes
        FROM QuestionOption
        WHERE
            (@QuestionOptionId      IS NULL OR QuestionOptionId     = @QuestionOptionId)
            AND (@QuestionId        IS NULL OR QuestionId           = @QuestionId)
            AND (@ComplementQuestionId IS NULL OR ComplementQuestionId = @ComplementQuestionId)
            AND (@Rank              IS NULL OR Rank                 = @Rank)
            AND (@OptionCd          IS NULL OR OptionCd             = @OptionCd)
            AND (@Description       IS NULL OR Description          LIKE '%' + @Description + '%')
            AND (@OpenMsgBox        IS NULL OR OpenMsgBox           = @OpenMsgBox)
            AND (@NeedNotes         IS NULL OR NeedNotes            = @NeedNotes);
    END
    ELSE IF @TypeRequest = 'INSERT'
    BEGIN
        INSERT INTO QuestionOption
        (
            QuestionId, ComplementQuestionId, Rank, OptionCd,
            Description, DhCreate, OpenMsgBox, NeedNotes
        )
        VALUES
        (
            @QuestionId, @ComplementQuestionId, @Rank, @OptionCd,
            @Description, GETDATE(), @OpenMsgBox, @NeedNotes
        );

        SELECT SCOPE_IDENTITY() AS QuestionOptionId;
    END
    ELSE IF @TypeRequest = 'UPDATE'
    BEGIN
        UPDATE QuestionOption SET
            QuestionId              = @QuestionId,
            ComplementQuestionId    = @ComplementQuestionId,
            Rank                    = @Rank,
            OptionCd                = @OptionCd,
            Description             = @Description,
            DhUpdate                = GETDATE(),
            OpenMsgBox              = @OpenMsgBox,
            NeedNotes               = @NeedNotes
        WHERE QuestionOptionId = @QuestionOptionId;
    END
    ELSE IF @TypeRequest = 'DELETE'
    BEGIN
        DELETE FROM QuestionOption WHERE QuestionOptionId = @QuestionOptionId;
    END
END
GO

GRANT EXECUTE ON [SP_Adm_QuestionOption] TO S360sys;
GO
