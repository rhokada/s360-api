IF OBJECT_ID('SP_Adm_sl_SurveyType', 'P') IS NOT NULL DROP PROCEDURE SP_Adm_sl_SurveyType;
GO
IF OBJECT_ID('SP_Adm_in_SurveyType', 'P') IS NOT NULL DROP PROCEDURE SP_Adm_in_SurveyType;
GO
IF OBJECT_ID('SP_Adm_up_SurveyType', 'P') IS NOT NULL DROP PROCEDURE SP_Adm_up_SurveyType;
GO
IF OBJECT_ID('SP_dl_SurveyType', 'P') IS NOT NULL DROP PROCEDURE SP_dl_SurveyType;
GO
IF OBJECT_ID('SP_Adm_SurveyType', 'P') IS NOT NULL DROP PROCEDURE SP_Adm_SurveyType;
GO

CREATE PROCEDURE SP_Adm_SurveyType
    @TypeRequest    VARCHAR(10)     ,
    @SurveyTypeId   INT             = NULL,
    @SurveyTypeCd   VARCHAR(50)     = NULL,
    @Name           VARCHAR(100)    = NULL,
    @token_usuario  NVARCHAR(MAX)   = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF @TypeRequest = 'SELECT'
    BEGIN
        SELECT
            SurveyTypeId,
            SurveyTypeCd,
            Name,
            DhUpdate,
            Log
        FROM SurveyType
        WHERE
            (@SurveyTypeId  IS NULL OR SurveyTypeId = @SurveyTypeId)
            AND (@SurveyTypeCd IS NULL OR SurveyTypeCd = @SurveyTypeCd)
            AND (@Name      IS NULL OR Name         LIKE '%' + @Name + '%');
    END
    ELSE IF @TypeRequest = 'INSERT'
    BEGIN
        INSERT INTO SurveyType
        (
            SurveyTypeCd, Name
        )
        VALUES
        (
            @SurveyTypeCd, @Name
        );

        SELECT SCOPE_IDENTITY() AS SurveyTypeId;
    END
    ELSE IF @TypeRequest = 'UPDATE'
    BEGIN
        UPDATE SurveyType SET
            SurveyTypeCd    = @SurveyTypeCd,
            Name            = @Name,
            DhUpdate        = GETDATE()
        WHERE SurveyTypeId = @SurveyTypeId;
    END
    ELSE IF @TypeRequest = 'DELETE'
    BEGIN
        DELETE FROM SurveyType WHERE SurveyTypeId = @SurveyTypeId;
    END
END
GO

GRANT EXECUTE ON [SP_Adm_SurveyType] TO S360sys;
GO
