-- ============================================================
-- SP_Adm_TranscricaoAudio
-- TypeRequest: SELECT
-- Retorna apenas registros de SubmittedAnswerDetails que
-- possuem transcrição de áudio processada.
-- ============================================================
CREATE OR ALTER PROCEDURE [dbo].[SP_Adm_TranscricaoAudio]
    @TypeRequest   VARCHAR(10)      ,
    @token_usuario NVARCHAR(MAX) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF @TypeRequest = 'SELECT'
    BEGIN
        SELECT
            SubmittedAnswerDetailId,
            AlternativeId,
            AnswerText,
            QuestionGroup,
            Metric,
            AudioTranscription,
            AiAnalysis
        FROM SubmittedAnswerDetails
        WHERE AudioTranscription IS NOT NULL
          AND AudioTranscription <> ''
        ORDER BY SubmittedAnswerDetailId DESC;
        RETURN;
    END
END
GO

GRANT EXECUTE ON [dbo].[SP_Adm_TranscricaoAudio] TO [S360sys];
GO
