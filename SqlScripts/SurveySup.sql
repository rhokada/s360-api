IF OBJECT_ID('SP_Adm_sl_SurveySup', 'P') IS NOT NULL DROP PROCEDURE SP_Adm_sl_SurveySup;
GO
IF OBJECT_ID('SP_Adm_in_SurveySup', 'P') IS NOT NULL DROP PROCEDURE SP_Adm_in_SurveySup;
GO
IF OBJECT_ID('SP_Adm_up_SurveySup', 'P') IS NOT NULL DROP PROCEDURE SP_Adm_up_SurveySup;
GO
IF OBJECT_ID('SP_dl_SurveySup', 'P') IS NOT NULL DROP PROCEDURE SP_dl_SurveySup;
GO
IF OBJECT_ID('SP_Adm_SurveySup', 'P') IS NOT NULL DROP PROCEDURE SP_Adm_SurveySup;
GO

CREATE PROCEDURE SP_Adm_SurveySup
    @TypeRequest    VARCHAR(10)     ,
    @SurveySupId    INT             = NULL,
    @SupUserId      INT             = NULL,
    @SurveyId       INT             = NULL,
    @Name           VARCHAR(100)    = NULL,
    @token_usuario  NVARCHAR(MAX)   = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF @TypeRequest = 'SELECT'
    BEGIN
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
    ELSE IF @TypeRequest = 'INSERT'
    BEGIN
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
    ELSE IF @TypeRequest = 'UPDATE'
    BEGIN
        UPDATE SurveySup SET
            SupUserId   = @SupUserId,
            SurveyId    = @SurveyId,
            Name        = @Name,
            DhUpdate    = GETDATE()
        WHERE SurveySupId = @SurveySupId;
    END
    ELSE IF @TypeRequest = 'DELETE'
    BEGIN
        DELETE FROM SurveySup WHERE SurveySupId = @SurveySupId;
    END
END
GO

GRANT EXECUTE ON [SP_Adm_SurveySup] TO S360sys;
GO
