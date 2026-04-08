-- =============================================
-- SP_sl_QuestionOption
-- =============================================
IF OBJECT_ID('SP_sl_QuestionOption', 'P') IS NOT NULL DROP PROCEDURE SP_sl_QuestionOption;
GO

CREATE PROCEDURE SP_sl_QuestionOption
    @QuestionOptionId       INT = NULL,
    @QuestionId             INT = NULL,
    @ComplementQuestionId   INT = NULL,
    @Rank                   INT = NULL,
    @OptionCd               VARCHAR(50)  = NULL,
    @Description            VARCHAR(255) = NULL,
    @OpenMsgBox             BIT = NULL,
    @NeedNotes              BIT = NULL
AS
BEGIN
    SET NOCOUNT ON;

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

GO

-- =============================================
-- SP_cr_QuestionOption
-- =============================================
IF OBJECT_ID('SP_cr_QuestionOption', 'P') IS NOT NULL DROP PROCEDURE SP_cr_QuestionOption;
GO

CREATE PROCEDURE SP_cr_QuestionOption
    @QuestionId             INT             ,
    @ComplementQuestionId   INT             = NULL,
    @Rank                   INT             ,
    @OptionCd               VARCHAR(50)     ,
    @Description            VARCHAR(255)    = NULL,
    @OpenMsgBox             BIT             = NULL,
    @NeedNotes              BIT             = NULL
AS
BEGIN
    SET NOCOUNT ON;

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
GO

GO

-- =============================================
-- SP_up_QuestionOption
-- =============================================
IF OBJECT_ID('SP_up_QuestionOption', 'P') IS NOT NULL DROP PROCEDURE SP_up_QuestionOption;
GO

CREATE PROCEDURE SP_up_QuestionOption
    @QuestionOptionId       INT             ,
    @QuestionId             INT             ,
    @ComplementQuestionId   INT             = NULL,
    @Rank                   INT             ,
    @OptionCd               VARCHAR(50)     ,
    @Description            VARCHAR(255)    = NULL,
    @OpenMsgBox             BIT             = NULL,
    @NeedNotes              BIT             = NULL
AS
BEGIN
    SET NOCOUNT ON;

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
GO

GO

-- =============================================
-- SP_dl_QuestionOption
-- =============================================
IF OBJECT_ID('SP_dl_QuestionOption', 'P') IS NOT NULL DROP PROCEDURE SP_dl_QuestionOption;
GO

CREATE PROCEDURE SP_dl_QuestionOption
    @QuestionOptionId INT 
AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM QuestionOption WHERE QuestionOptionId = @QuestionOptionId;
END
GO

GO
