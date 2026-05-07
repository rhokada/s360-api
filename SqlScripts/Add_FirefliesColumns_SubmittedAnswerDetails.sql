-- Adiciona colunas de rastreamento do Fireflies.ai na tabela SubmittedAnswerDetails
-- FirefliesMeetingId: preenchido pelo webhook quando o Fireflies conclui a transcrição
-- FirefliesSubmittedAt: preenchido pelo Job de submissão para evitar reenvio

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.SubmittedAnswerDetails')
      AND name = N'FirefliesMeetingId'
)
BEGIN
    ALTER TABLE dbo.SubmittedAnswerDetails
        ADD FirefliesMeetingId   NVARCHAR(100) NULL,
            FirefliesSubmittedAt DATETIME      NULL;
END
GO
