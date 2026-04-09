-- =============================================
-- SP_sl_SurveySup
-- =============================================
IF OBJECT_ID('SP_sl_SurveySup', 'P') IS NOT NULL DROP PROCEDURE SP_sl_SurveySup;
GO

CREATE PROCEDURE SP_sl_SurveySup
    @SurveySupId    INT             = NULL,
    @SupUserId      INT             = NULL,
    @SurveyId       INT             = NULL,
    @Name           VARCHAR(100)    = NULL,
    @token_usuario  NVARCHAR(MAX)   = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        SurveySupId,
        SupUserId,
        SurveyId,
        Name,
        DhCreate,
        DhUpdate,
        Log
    FROM SurveySup
    WHERE
        (@SurveySupId   IS NULL OR SurveySupId  = @SurveySupId)
        AND (@SupUserId IS NULL OR SupUserId     = @SupUserId)
        AND (@SurveyId  IS NULL OR SurveyId     = @SurveyId)
        AND (@Name      IS NULL OR Name         LIKE '%' + @Name + '%');
END
GO

GRANT EXECUTE ON SP_sl_SurveySup TO S360sys;
GO

-- =============================================
-- SP_cr_SurveySup
-- =============================================
IF OBJECT_ID('SP_cr_SurveySup', 'P') IS NOT NULL DROP PROCEDURE SP_cr_SurveySup;
GO

CREATE PROCEDURE SP_cr_SurveySup
    @SupUserId  INT             ,
    @SurveyId   INT             ,
    @Name           VARCHAR(100)    ,
    @token_usuario  NVARCHAR(MAX)   = NULL
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO SurveySup
    (
        SupUserId, SurveyId, Name, DhCreate
    )
    VALUES
    (
        @SupUserId, @SurveyId, @Name, GETDATE()
    );

    SELECT SCOPE_IDENTITY() AS SurveySupId;
END
GO

GRANT EXECUTE ON SP_cr_SurveySup TO S360sys;
GO

-- =============================================
-- SP_up_SurveySup
-- =============================================
IF OBJECT_ID('SP_up_SurveySup', 'P') IS NOT NULL DROP PROCEDURE SP_up_SurveySup;
GO

CREATE PROCEDURE SP_up_SurveySup
    @SurveySupId    INT             ,
    @SupUserId      INT             ,
    @SurveyId       INT             ,
    @Name           VARCHAR(100)    ,
    @token_usuario  NVARCHAR(MAX)   = NULL
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE SurveySup SET
        SupUserId   = @SupUserId,
        SurveyId    = @SurveyId,
        Name        = @Name,
        DhUpdate    = GETDATE()
    WHERE SurveySupId = @SurveySupId;
END
GO

GRANT EXECUTE ON SP_up_SurveySup TO S360sys;
GO

-- =============================================
-- SP_dl_SurveySup
-- =============================================
IF OBJECT_ID('SP_dl_SurveySup', 'P') IS NOT NULL DROP PROCEDURE SP_dl_SurveySup;
GO

CREATE PROCEDURE SP_dl_SurveySup
    @SurveySupId    INT             ,
    @token_usuario  NVARCHAR(MAX)   = NULL
AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM SurveySup WHERE SurveySupId = @SurveySupId;
END
GO

GRANT EXECUTE ON SP_dl_SurveySup TO S360sys;
GO
