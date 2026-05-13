-- ============================================================
-- SP_Adm_AiPrompt
-- TypeRequest: SELECT | INSERT | UPDATE | DELETE
-- Regra: apenas um registro ativo por AiProcessCd (Active = 1).
-- INSERT e UPDATE desativam todos os registros anteriores do mesmo
-- AiProcessCd e inserem uma nova linha versionada com Active = 1.
-- DELETE é soft delete (Active = 0).
-- ============================================================
CREATE OR ALTER PROCEDURE [dbo].[SP_Adm_AiPrompt]
    @TypeRequest   VARCHAR(10)       ,
    @AiPromptId    INT            = NULL,
    @AiProcessCd   NVARCHAR(500)  = NULL,
    @Context       NVARCHAR(4000) = NULL,
    @Prompt        VARCHAR(MAX)   = NULL,
    @Engine        NVARCHAR(1000) = NULL,
    @Log           VARCHAR(MAX)   = NULL,
    @token_usuario NVARCHAR(MAX)  = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF @TypeRequest = 'SELECT'
    BEGIN
        SELECT
            AiPromptId,
            AiProcessCd,
            Context,
            Prompt,
            Engine,
            Dhcreate,
            Active,
            Log
        FROM AiPrompt
        WHERE Active = 1
        ORDER BY AiProcessCd;
        RETURN;
    END

    IF @TypeRequest = 'INSERT'
    BEGIN
        -- Desativa todos os registros existentes do mesmo AiProcessCd
        UPDATE AiPrompt
        SET Active = 0
        WHERE AiProcessCd = @AiProcessCd;

        -- Insere nova versão ativa
        INSERT INTO AiPrompt (AiProcessCd, Context, Prompt, Engine, Dhcreate, Active, Log)
        VALUES (@AiProcessCd, @Context, @Prompt, @Engine, GETDATE(), 1, @Log);

        SELECT SCOPE_IDENTITY() AS AiPromptId;
        RETURN;
    END

    IF @TypeRequest = 'UPDATE'
    BEGIN
        DECLARE @ExistingAiProcessCd NVARCHAR(500);
        SELECT @ExistingAiProcessCd = AiProcessCd
        FROM AiPrompt
        WHERE AiPromptId = @AiPromptId;

        -- Usa o AiProcessCd enviado (pode ter sido alterado) ou mantém o atual
        DECLARE @TargetAiProcessCd NVARCHAR(500) = ISNULL(@AiProcessCd, @ExistingAiProcessCd);

        -- Desativa todos os registros do AiProcessCd de destino
        UPDATE AiPrompt
        SET Active = 0
        WHERE AiProcessCd = @TargetAiProcessCd;

        -- Insere nova versão versionada com Active = 1
        INSERT INTO AiPrompt (AiProcessCd, Context, Prompt, Engine, Dhcreate, Active, Log)
        VALUES (@TargetAiProcessCd, @Context, @Prompt, @Engine, GETDATE(), 1, @Log);

        SELECT SCOPE_IDENTITY() AS AiPromptId;
        RETURN;
    END

    IF @TypeRequest = 'DELETE'
    BEGIN
        -- Soft delete: apenas desativa o registro
        UPDATE AiPrompt
        SET Active = 0
        WHERE AiPromptId = @AiPromptId;

        SELECT @AiPromptId AS AiPromptId;
        RETURN;
    END
END
GO

GRANT EXECUTE ON [dbo].[SP_Adm_AiPrompt] TO [S360sys];
GO
