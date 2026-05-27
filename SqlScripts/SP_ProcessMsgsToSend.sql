/****** Object:  StoredProcedure [dbo].[SP_ProcessMsgsToSend] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER PROCEDURE [dbo].[SP_ProcessMsgsToSend]
AS
BEGIN
    SET NOCOUNT ON;

    PRINT 'DEBUG: Início da execução da procedure dbo.SP_ProcessMsgsToSend.';

    -- =========================================================================
    -- VARIÁVEIS
    -- =========================================================================
    DECLARE @AzureBlobBaseUrl NVARCHAR(500) = 'https://storagebdh.blob.core.windows.net/s360/';
    DECLARE @CurrentDate DATE;

    -- Questionários
    DECLARE @SubmittedAnswersId         INT;
    DECLARE @CurrentSellerId            INT;
    DECLARE @CurrentSellerCode          VARCHAR(100);
    DECLARE @CurrentDtSurvey            DATE;
    DECLARE @CurrentSurveyType          VARCHAR(100);
    DECLARE @SellerEmail                VARCHAR(255);
    DECLARE @SellerName                 VARCHAR(255);
    DECLARE @SellerPhone                VARCHAR(50);
    DECLARE @QuestionnaireDateFormatted VARCHAR(10);
    DECLARE @SurveyTypeDisplayName      NVARCHAR(100);
    DECLARE @QuestionDetailText         NVARCHAR(MAX);
    DECLARE @CustomerCode               VARCHAR(100);

    -- FUPs
    DECLARE @FupId                            INT;
    DECLARE @FupDescription                   NVARCHAR(MAX);
    DECLARE @FupDtExpectedConclusion          DATETIME;
    DECLARE @FupPriority                      VARCHAR(100);
    DECLARE @FupStatus                        VARCHAR(100);
    DECLARE @FupCustomerCode                  VARCHAR(100);
    DECLARE @FupSellerCode_FUP                VARCHAR(100);
    DECLARE @FupReminderDate                  DATETIME;
    DECLARE @FupDtInclusion                   DATETIME;
    DECLARE @FupDtConclusion                  DATETIME;
    DECLARE @FupResponsibleEmail              VARCHAR(255);
    DECLARE @FupResponsibleName               NVARCHAR(255);
    DECLARE @FupResponsiblePhone              VARCHAR(50);
    DECLARE @FupDtExpectedConclusionFormatted VARCHAR(10);
    DECLARE @FupDtConclusionFormatted         VARCHAR(10);
    DECLARE @EmailReason                      NVARCHAR(100);
    DECLARE @IsWeekendAdjusted                BIT;
    DECLARE @FupCategory                      NVARCHAR(100);
    DECLARE @FupPostponementCount             INT;
    DECLARE @FupQuestion                      NVARCHAR(1000);
    DECLARE @FupLog                           NVARCHAR(MAX);
    DECLARE @LookupUserId                     INT;

    -- Template
    DECLARE @ContractId             INT;
    DECLARE @JsonParam              NVARCHAR(MAX);
    DECLARE @EmailTemplateSubject   NVARCHAR(1000);
    DECLARE @EmailTemplateMessage   NVARCHAR(MAX);
    DECLARE @EmailTemplateFrom      VARCHAR(255);
    DECLARE @EmailTemplateNameFrom  VARCHAR(255);
    DECLARE @WhatsAppTemplateMsg    NVARCHAR(MAX);

    -- Mensagens
    DECLARE @RawSubject        NVARCHAR(1000);
    DECLARE @RawMessageContent NVARCHAR(MAX);
    DECLARE @RawMessagePlain   NVARCHAR(MAX);
    DECLARE @FinalEmailSubject NVARCHAR(1000);
    DECLARE @FinalEmailBody    NVARCHAR(MAX);
    DECLARE @FinalWhatsAppBody NVARCHAR(MAX);
    DECLARE @JsonDataEmail     NVARCHAR(MAX);
    DECLARE @JsonDataWhatsApp  NVARCHAR(MAX);

    -- Auxiliares para strip de HTML
    DECLARE @TagStart INT;
    DECLARE @TagEnd   INT;

    -- =========================================================================
    -- INICIALIZAÇÕES
    -- =========================================================================
    SET @CurrentDate = CAST(GETDATE() AS DATE);
    PRINT 'DEBUG: @CurrentDate definida como: ' + CAST(@CurrentDate AS NVARCHAR(MAX));

    -- =========================================================================
    -- CURSORES
    -- =========================================================================

    -- Cursor para Questionários
    DECLARE cur_submitted_questionnaires CURSOR LOCAL FAST_FORWARD FOR
    SELECT sa.SubmittedAnswersId, sa.SellerId, sa.SellerCode, sa.DtSurvey, sa.SurveyType
    FROM dbo.SubmittedAnswers sa
    WHERE
        NOT EXISTS (
            SELECT 1 FROM dbo.MsgToSend mts
            WHERE mts.SubmittedAnswersId = sa.SubmittedAnswersId
              AND mts.MsgRefType = 'QuestionnaireFeedback'
        )
        AND sa.SurveyType <> 'AVALIACAO_MERCADO'
        AND sa.SellerId IS NOT NULL;

    -- Cursor para lembretes de FUP
    DECLARE cur_fup_reminders CURSOR LOCAL FAST_FORWARD FOR
    WITH FupRelevantDates AS (
        SELECT
            f.FupId, f.description, f.dtExpectedConclusion, f.priority, f.status,
            f.customerCode, f.sellerCode, f.reminderDate, f.dtInclusion, f.dtConclusion,
            f.category, f.postponementCount, f.question, f.log,
            CAST(f.dtInclusion AS DATE)         AS Unadj_CreationDate,
            CAST(f.reminderDate AS DATE)         AS Unadj_ReminderDate,
            CASE WHEN f.reminderDate IS NULL AND f.dtExpectedConclusion IS NOT NULL
                 THEN CAST(DATEADD(day, -3, f.dtExpectedConclusion) AS DATE)
                 ELSE NULL END                   AS Unadj_ReminderNullExp3Days,
            CAST(f.dtExpectedConclusion AS DATE) AS Unadj_ExpectedConclusionDate,
            CASE WHEN f.dtExpectedConclusion IS NOT NULL AND f.dtExpectedConclusion < @CurrentDate
                      AND DATEDIFF(day, f.dtExpectedConclusion, @CurrentDate) > 0
                      AND (DATEDIFF(day, f.dtExpectedConclusion, @CurrentDate) % 30 = 0)
                 THEN CAST(DATEADD(day, (DATEDIFF(day, f.dtExpectedConclusion, @CurrentDate) / 30) * 30, f.dtExpectedConclusion) AS DATE)
                 ELSE NULL END                   AS Unadj_RecurringDate,
            CAST(f.dtConclusion AS DATE)         AS Unadj_CompletionDate
        FROM dbo.Fup f
        WHERE (f.active = 1 OR f.active IS NULL)
    ),
    FupHasReminders AS (
        SELECT
            frd.FupId,
            CASE WHEN EXISTS (
                SELECT 1 FROM dbo.MsgToSend mts
                WHERE mts.FupId = frd.FupId AND mts.MsgRefType = 'FUPReminder'
            ) THEN 1 ELSE 0 END AS HasBeenRemindedBefore
        FROM FupRelevantDates frd
    ),
    FupAdjustedDates AS (
        SELECT
            fd.FupId, fd.description, fd.dtExpectedConclusion, fd.priority, fd.status,
            fd.customerCode, fd.sellerCode, fd.reminderDate, fd.dtInclusion, fd.dtConclusion,
            fd.category, fd.postponementCount, fd.question, fd.log,
            fhr.HasBeenRemindedBefore,
            (CASE WHEN fd.Unadj_CreationDate IS NULL THEN NULL WHEN DATEPART(weekday,fd.Unadj_CreationDate)=1 THEN DATEADD(day,-2,fd.Unadj_CreationDate) WHEN DATEPART(weekday,fd.Unadj_CreationDate)=7 THEN DATEADD(day,-1,fd.Unadj_CreationDate) ELSE fd.Unadj_CreationDate END) AS Adj_CreationDate,
            (CASE WHEN fd.Unadj_ReminderDate IS NULL THEN NULL WHEN DATEPART(weekday,fd.Unadj_ReminderDate)=1 THEN DATEADD(day,-2,fd.Unadj_ReminderDate) WHEN DATEPART(weekday,fd.Unadj_ReminderDate)=7 THEN DATEADD(day,-1,fd.Unadj_ReminderDate) ELSE fd.Unadj_ReminderDate END) AS Adj_ReminderDate,
            (CASE WHEN fd.Unadj_ReminderNullExp3Days IS NULL THEN NULL WHEN DATEPART(weekday,fd.Unadj_ReminderNullExp3Days)=1 THEN DATEADD(day,-2,fd.Unadj_ReminderNullExp3Days) WHEN DATEPART(weekday,fd.Unadj_ReminderNullExp3Days)=7 THEN DATEADD(day,-1,fd.Unadj_ReminderNullExp3Days) ELSE fd.Unadj_ReminderNullExp3Days END) AS Adj_ReminderNullExp3Days,
            (CASE WHEN fd.Unadj_ExpectedConclusionDate IS NULL THEN NULL WHEN DATEPART(weekday,fd.Unadj_ExpectedConclusionDate)=1 THEN DATEADD(day,-2,fd.Unadj_ExpectedConclusionDate) WHEN DATEPART(weekday,fd.Unadj_ExpectedConclusionDate)=7 THEN DATEADD(day,-1,fd.Unadj_ExpectedConclusionDate) ELSE fd.Unadj_ExpectedConclusionDate END) AS Adj_ExpectedConclusionDate,
            (CASE WHEN fd.Unadj_RecurringDate IS NULL THEN NULL WHEN DATEPART(weekday,fd.Unadj_RecurringDate)=1 THEN DATEADD(day,-2,fd.Unadj_RecurringDate) WHEN DATEPART(weekday,fd.Unadj_RecurringDate)=7 THEN DATEADD(day,-1,fd.Unadj_RecurringDate) ELSE fd.Unadj_RecurringDate END) AS Adj_RecurringDate,
            CASE
                WHEN fhr.HasBeenRemindedBefore = 0 AND fd.status = 'pending' THEN 'Primeiro Lembrete'
                WHEN (CASE WHEN fd.Unadj_CreationDate IS NULL THEN NULL WHEN DATEPART(weekday,fd.Unadj_CreationDate)=1 THEN DATEADD(day,-2,fd.Unadj_CreationDate) WHEN DATEPART(weekday,fd.Unadj_CreationDate)=7 THEN DATEADD(day,-1,fd.Unadj_CreationDate) ELSE fd.Unadj_CreationDate END) = @CurrentDate THEN 'Criação'
                WHEN (CASE WHEN fd.Unadj_ReminderDate IS NULL THEN NULL WHEN DATEPART(weekday,fd.Unadj_ReminderDate)=1 THEN DATEADD(day,-2,fd.Unadj_ReminderDate) WHEN DATEPART(weekday,fd.Unadj_ReminderDate)=7 THEN DATEADD(day,-1,fd.Unadj_ReminderDate) ELSE fd.Unadj_ReminderDate END) = @CurrentDate THEN 'Lembrete Configurado'
                WHEN (CASE WHEN fd.Unadj_ReminderNullExp3Days IS NULL THEN NULL WHEN DATEPART(weekday,fd.Unadj_ReminderNullExp3Days)=1 THEN DATEADD(day,-2,fd.Unadj_ReminderNullExp3Days) WHEN DATEPART(weekday,fd.Unadj_ReminderNullExp3Days)=7 THEN DATEADD(day,-1,fd.Unadj_ReminderNullExp3Days) ELSE fd.Unadj_ReminderNullExp3Days END) = @CurrentDate THEN 'Lembrete (3 dias antes da conclusão)'
                WHEN (CASE WHEN fd.Unadj_ExpectedConclusionDate IS NULL THEN NULL WHEN DATEPART(weekday,fd.Unadj_ExpectedConclusionDate)=1 THEN DATEADD(day,-2,fd.Unadj_ExpectedConclusionDate) WHEN DATEPART(weekday,fd.Unadj_ExpectedConclusionDate)=7 THEN DATEADD(day,-1,fd.Unadj_ExpectedConclusionDate) ELSE fd.Unadj_ExpectedConclusionDate END) = @CurrentDate THEN 'Data de Conclusão Prevista'
                WHEN (CASE WHEN fd.Unadj_RecurringDate IS NULL THEN NULL WHEN DATEPART(weekday,fd.Unadj_RecurringDate)=1 THEN DATEADD(day,-2,fd.Unadj_RecurringDate) WHEN DATEPART(weekday,fd.Unadj_RecurringDate)=7 THEN DATEADD(day,-1,fd.Unadj_RecurringDate) ELSE fd.Unadj_RecurringDate END) = @CurrentDate THEN 'Lembrete Recorrente (a cada 30 dias)'
                WHEN fd.status = 'pending'
                    AND (CASE WHEN fd.Unadj_ExpectedConclusionDate IS NULL THEN NULL WHEN DATEPART(weekday,fd.Unadj_ExpectedConclusionDate)=1 THEN DATEADD(day,-2,fd.Unadj_ExpectedConclusionDate) WHEN DATEPART(weekday,fd.Unadj_ExpectedConclusionDate)=7 THEN DATEADD(day,-1,fd.Unadj_ExpectedConclusionDate) ELSE fd.Unadj_ExpectedConclusionDate END) < @CurrentDate
                    AND (DATEPART(weekday, @CurrentDate) = 2)
                    AND NOT EXISTS (
                        SELECT 1 FROM dbo.MsgToSend mts_inner
                        WHERE mts_inner.FupId = fd.FupId AND mts_inner.MsgRefType = 'FUPReminder'
                          AND CAST(mts_inner.DtCreated AS DATE) >= DATEADD(day, -7, @CurrentDate)
                    )
                THEN 'Pendência Vencida (Semanal)'
                ELSE NULL
            END AS EmailReason_Calculated,
            (CASE
                WHEN (fd.Unadj_CreationDate IS NOT NULL AND (CASE WHEN fd.Unadj_CreationDate IS NULL THEN NULL WHEN DATEPART(weekday,fd.Unadj_CreationDate)=1 THEN DATEADD(day,-2,fd.Unadj_CreationDate) WHEN DATEPART(weekday,fd.Unadj_CreationDate)=7 THEN DATEADD(day,-1,fd.Unadj_CreationDate) ELSE fd.Unadj_CreationDate END) != fd.Unadj_CreationDate) THEN 1
                WHEN (fd.Unadj_ReminderDate IS NOT NULL AND (CASE WHEN fd.Unadj_ReminderDate IS NULL THEN NULL WHEN DATEPART(weekday,fd.Unadj_ReminderDate)=1 THEN DATEADD(day,-2,fd.Unadj_ReminderDate) WHEN DATEPART(weekday,fd.Unadj_ReminderDate)=7 THEN DATEADD(day,-1,fd.Unadj_ReminderDate) ELSE fd.Unadj_ReminderDate END) != fd.Unadj_ReminderDate) THEN 1
                WHEN (fd.Unadj_ReminderNullExp3Days IS NOT NULL AND (CASE WHEN fd.Unadj_ReminderNullExp3Days IS NULL THEN NULL WHEN DATEPART(weekday,fd.Unadj_ReminderNullExp3Days)=1 THEN DATEADD(day,-2,fd.Unadj_ReminderNullExp3Days) WHEN DATEPART(weekday,fd.Unadj_ReminderNullExp3Days)=7 THEN DATEADD(day,-1,fd.Unadj_ReminderNullExp3Days) ELSE fd.Unadj_ReminderNullExp3Days END) != fd.Unadj_ReminderNullExp3Days) THEN 1
                WHEN (fd.Unadj_ExpectedConclusionDate IS NOT NULL AND (CASE WHEN fd.Unadj_ExpectedConclusionDate IS NULL THEN NULL WHEN DATEPART(weekday,fd.Unadj_ExpectedConclusionDate)=1 THEN DATEADD(day,-2,fd.Unadj_ExpectedConclusionDate) WHEN DATEPART(weekday,fd.Unadj_ExpectedConclusionDate)=7 THEN DATEADD(day,-1,fd.Unadj_ExpectedConclusionDate) ELSE fd.Unadj_ExpectedConclusionDate END) != fd.Unadj_ExpectedConclusionDate) THEN 1
                WHEN (fd.Unadj_RecurringDate IS NOT NULL AND (CASE WHEN fd.Unadj_RecurringDate IS NULL THEN NULL WHEN DATEPART(weekday,fd.Unadj_RecurringDate)=1 THEN DATEADD(day,-2,fd.Unadj_RecurringDate) WHEN DATEPART(weekday,fd.Unadj_RecurringDate)=7 THEN DATEADD(day,-1,fd.Unadj_RecurringDate) ELSE fd.Unadj_RecurringDate END) != fd.Unadj_RecurringDate) THEN 1
                ELSE 0
            END) AS IsWeekendAdjusted_Calculated
        FROM FupRelevantDates fd
        JOIN FupHasReminders fhr ON fd.FupId = fhr.FupId
    )
    SELECT
        fa.FupId, fa.description, fa.dtExpectedConclusion, fa.priority, fa.status,
        fa.customerCode, fa.sellerCode, fa.reminderDate, fa.dtInclusion, fa.dtConclusion,
        fa.category, fa.postponementCount, fa.question, fa.log,
        fa.EmailReason_Calculated AS EmailReason,
        fa.IsWeekendAdjusted_Calculated
    FROM FupAdjustedDates fa
    WHERE
        fa.status NOT IN ('completed', 'cancelled')
        AND fa.EmailReason_Calculated IS NOT NULL
        AND fa.sellerCode IS NOT NULL
        AND NOT EXISTS (
            SELECT 1 FROM dbo.MsgToSend mts
            WHERE mts.MsgRefType = 'FUPReminder'
              AND mts.FupId = fa.FupId
              AND CAST(mts.DtCreated AS DATE) = @CurrentDate
        );

    -- Cursor para conclusão de FUP
    DECLARE cur_fup_completions CURSOR LOCAL FAST_FORWARD FOR
    WITH FupRelevantDates AS (
        SELECT
            f.FupId, f.description, f.dtExpectedConclusion, f.priority, f.status,
            f.customerCode, f.sellerCode, f.dtConclusion,
            f.category, f.postponementCount, f.question, f.log,
            CAST(f.dtConclusion AS DATE) AS Unadj_CompletionDate
        FROM dbo.Fup f
        WHERE (f.active = 1 OR f.active IS NULL)
    ),
    FupAdjustedDates AS (
        SELECT
            fd.FupId, fd.description, fd.dtExpectedConclusion, fd.priority, fd.status,
            fd.customerCode, fd.sellerCode, fd.dtConclusion,
            fd.category, fd.postponementCount, fd.question, fd.log,
            fd.Unadj_CompletionDate,
            (CASE WHEN fd.Unadj_CompletionDate IS NULL THEN NULL
                  WHEN DATEPART(weekday,fd.Unadj_CompletionDate)=1 THEN DATEADD(day,-2,fd.Unadj_CompletionDate)
                  WHEN DATEPART(weekday,fd.Unadj_CompletionDate)=7 THEN DATEADD(day,-1,fd.Unadj_CompletionDate)
                  ELSE fd.Unadj_CompletionDate END) AS Adj_CompletionDate,
            (CASE WHEN fd.Unadj_CompletionDate IS NOT NULL AND (CASE WHEN fd.Unadj_CompletionDate IS NULL THEN NULL WHEN DATEPART(weekday,fd.Unadj_CompletionDate)=1 THEN DATEADD(day,-2,fd.Unadj_CompletionDate) WHEN DATEPART(weekday,fd.Unadj_CompletionDate)=7 THEN DATEADD(day,-1,fd.Unadj_CompletionDate) ELSE fd.Unadj_CompletionDate END) != fd.Unadj_CompletionDate THEN 1 ELSE 0 END) AS IsWeekendAdjusted_Calculated
        FROM FupRelevantDates fd
    )
    SELECT
        fa.FupId, fa.description, fa.dtExpectedConclusion, fa.priority, fa.status,
        fa.customerCode, fa.sellerCode, fa.dtConclusion,
        fa.category, fa.postponementCount, fa.question, fa.log,
        'Conclusão' AS EmailReason,
        fa.IsWeekendAdjusted_Calculated
    FROM FupAdjustedDates fa
    WHERE
        fa.status = 'completed'
        AND fa.dtConclusion IS NOT NULL
        AND fa.Adj_CompletionDate = @CurrentDate
        AND fa.sellerCode IS NOT NULL
        AND NOT EXISTS (
            SELECT 1 FROM dbo.MsgToSend mts
            WHERE mts.MsgRefType = 'FUPCompletion'
              AND mts.FupId = fa.FupId
              AND CAST(mts.DtCreated AS DATE) = @CurrentDate
        );

    -- =========================================================================
    -- PROCESSAMENTO DE QUESTIONÁRIOS
    -- =========================================================================
    PRINT 'DEBUG: Iniciando processamento de Questionários.';

    OPEN cur_submitted_questionnaires;
    FETCH NEXT FROM cur_submitted_questionnaires INTO
        @SubmittedAnswersId, @CurrentSellerId, @CurrentSellerCode, @CurrentDtSurvey, @CurrentSurveyType;

    WHILE @@FETCH_STATUS = 0
    BEGIN
        PRINT 'DEBUG: Processando Questionário SubmittedAnswersId: ' + ISNULL(CAST(@SubmittedAnswersId AS NVARCHAR), 'NULO');

        SET @SellerEmail = NULL; SET @SellerName = NULL; SET @SellerPhone = NULL; SET @ContractId = NULL;

        SELECT TOP 1
            @SellerEmail = U.Email,
            @SellerName  = U.Name,
            @SellerPhone = ISNULL(U.DddCell, '') + ISNULL(U.NrCell, ''),
            @ContractId  = U.ContractId
        FROM dbo.[User] U WHERE U.UserId = @CurrentSellerId;

        IF @SellerEmail IS NULL
        BEGIN
            PRINT N'AVISO: E-mail não encontrado para SellerId ' + ISNULL(CAST(@CurrentSellerId AS NVARCHAR), 'NULO') + '. Pulando.';
            FETCH NEXT FROM cur_submitted_questionnaires INTO
                @SubmittedAnswersId, @CurrentSellerId, @CurrentSellerCode, @CurrentDtSurvey, @CurrentSurveyType;
            CONTINUE;
        END;

        -- Carregar template do contrato
        SET @JsonParam = NULL;
        IF @ContractId IS NOT NULL
            SELECT TOP 1 @JsonParam = C.JsonParam FROM dbo.[Contract] C WHERE C.ContractId = @ContractId;

        SET @EmailTemplateSubject  = ISNULL(JSON_VALUE(@JsonParam, '$.MsgEmailTemplate.Subject'),   N'SuperVision360 - <<SUBJECT>>');
        SET @EmailTemplateMessage  = ISNULL(JSON_VALUE(@JsonParam, '$.MsgEmailTemplate.Message'),   N'Olá <<NAME>>,<br><br><<MESSAGE>><br><br>Caso você não tenha solicitado, pode ignorar essa mensagem.<br><br><br>Suporte SuperVision 360');
        SET @EmailTemplateFrom     = ISNULL(JSON_VALUE(@JsonParam, '$.MsgEmailTemplate.From'),      'naoresponda@supervision360.com.br');
        SET @EmailTemplateNameFrom = ISNULL(JSON_VALUE(@JsonParam, '$.MsgEmailTemplate.NameFrom'),  N'Não responda - SuperVision 360');
        SET @WhatsAppTemplateMsg   = ISNULL(JSON_VALUE(@JsonParam, '$.MsgWhasAppTemplate.Message'), N'Olá <<NAME>>, ' + CHAR(10) + '<<MESSAGE>>');

        SET @QuestionnaireDateFormatted = FORMAT(@CurrentDtSurvey, 'dd/MM/yyyy');
        SET @SurveyTypeDisplayName = CASE @CurrentSurveyType
            WHEN 'TREINAMENTO_CAMPO' THEN 'Treinamento em Campo'
            WHEN 'CHECK_ROTA'        THEN 'Check de Rota'
            ELSE ISNULL(@CurrentSurveyType, 'Tipo Desconhecido')
        END;

        SELECT @CustomerCode = CustomerCode FROM dbo.SubmittedAnswers WHERE SubmittedAnswersId = @SubmittedAnswersId;

        SET @QuestionDetailText = (
            SELECT STUFF((
                SELECT
                    N'<br><br>' +
                    N'<b>P:</b> ' + ISNULL(Q.Question, 'Questão Desconhecida') + N'<br>' +
                    N'<b>R:</b> ' +
                    CASE Q.AnswerTypeCd
                        WHEN 'TXT' THEN ISNULL(SAD.AnswerText, 'Não Respondido')
                        WHEN 'VLR' THEN ISNULL(SAD.AnswerText, 'Não Respondido')
                        WHEN 'SGL' THEN ISNULL(SGL_Desc.Description, ISNULL(SAD.AnswerText, 'Não Selecionado'))
                        WHEN 'MLT' THEN ISNULL(MLT_Desc.AggregatedDescriptions, ISNULL(SAD.AnswerText, 'Não Selecionado'))
                        ELSE ISNULL(SAD.AnswerText, 'Não Respondido')
                    END
                    + ISNULL(N'<br>  (Notas da Linha do Tempo:<br>' + TimelineFormatted.FormattedTimeline + N'<br>)', '')
                FROM dbo.SubmittedAnswerDetails SAD
                JOIN dbo.Question Q ON TRY_CAST(SAD.QuestionId AS INT) = Q.QuestionId
                OUTER APPLY (
                    SELECT TOP 1 qo.Description FROM dbo.QuestionOption qo
                    WHERE qo.QuestionId = TRY_CAST(SAD.QuestionId AS INT) AND qo.OptionCd = SAD.AlternativeId AND Q.AnswerTypeCd = 'SGL'
                ) AS SGL_Desc
                OUTER APPLY (
                    SELECT STUFF((
                        SELECT N', ' + qo.Description
                        FROM dbo.QuestionOption qo
                        JOIN OPENJSON(ISNULL(SAD.SelectedMultipleAlternativesIdsJson, '[]')) WITH (id VARCHAR(100) '$.id') AS sel ON qo.OptionCd = sel.id
                        WHERE qo.QuestionId = TRY_CAST(SAD.QuestionId AS INT) AND Q.AnswerTypeCd = 'MLT'
                        ORDER BY qo.Description FOR XML PATH(''), TYPE).value('.', 'NVARCHAR(MAX)'), 1, 2, '') AS AggregatedDescriptions
                ) AS MLT_Desc
                OUTER APPLY (
                    SELECT STUFF((
                        SELECT N'<br>' +
                            CASE
                                WHEN te.type = 'text'       THEN N'  - Texto: ' + te.text
                                WHEN te.type = 'attachment' THEN CONCAT(N'  - Anexo: <a href="', @AzureBlobBaseUrl, ad.filename, N'" >', ISNULL(ad.filename, N'Arquivo'), N'</a>')
                                ELSE N'  - Entrada desconhecida'
                            END
                        FROM OPENJSON(SAD.TimeLineJson) WITH (id VARCHAR(36), type VARCHAR(50), timestamp BIGINT, text NVARCHAR(MAX), attachment NVARCHAR(MAX) AS JSON) AS te
                        OUTER APPLY OPENJSON(te.attachment) WITH (id VARCHAR(36), type VARCHAR(50), filename NVARCHAR(255), filepath NVARCHAR(MAX)) AS ad
                        WHERE SAD.TimeLineJson IS NOT NULL AND SAD.TimeLineJson <> '[]'
                        ORDER BY te.timestamp FOR XML PATH(''), TYPE).value('.', 'NVARCHAR(MAX)'), 1, LEN(N'<br>'), '') AS FormattedTimeline
                ) AS TimelineFormatted
                WHERE SAD.SubmittedAnswersId = @SubmittedAnswersId
                ORDER BY Q.Rank
                FOR XML PATH(''), TYPE).value('.', 'NVARCHAR(MAX)'), 1, LEN(N'<br><br>'), '')
        );

        SET @RawSubject =
            N'Resumo do Questionário ' + @SurveyTypeDisplayName + ' - ' + @QuestionnaireDateFormatted +
            ' (' + ISNULL(@CurrentSellerCode, 'Vendedor Desconhecido') + ')';

        SET @RawMessageContent =
            N'Esperamos que se encontre bem!<br>' +
            N'Este é o resumo das suas respostas para o questionário "' + @SurveyTypeDisplayName +
            N'" realizado em ' + @QuestionnaireDateFormatted + N' juntamente com seu Supervisor.<br>' +
            N'Cliente: ' + ISNULL(@CustomerCode, 'N/A') + N'<br>---<br>' +
            ISNULL(@QuestionDetailText, '') +
            N'<br><br>---<br><br>' +
            N'Sua colaboração é fundamental para o nosso aprimoramento contínuo. Agradecemos a sua dedicação e contribuição!';

        -- Aplicar template de email
        SET @FinalEmailSubject = REPLACE(@EmailTemplateSubject, '<<SUBJECT>>', @RawSubject);
        SET @FinalEmailBody    = REPLACE(REPLACE(@EmailTemplateMessage, '<<NAME>>', ISNULL(@SellerName, '')), '<<MESSAGE>>', @RawMessageContent);

        -- Preparar versão plain para WhatsApp (strip de HTML)
        SET @RawMessagePlain = @RawMessageContent;
        SET @RawMessagePlain = REPLACE(@RawMessagePlain, '<br><br>', CHAR(10) + CHAR(10));
        SET @RawMessagePlain = REPLACE(@RawMessagePlain, '<br>',     CHAR(10));
        SET @RawMessagePlain = REPLACE(@RawMessagePlain, '<BR><BR>', CHAR(10) + CHAR(10));
        SET @RawMessagePlain = REPLACE(@RawMessagePlain, '<BR>',     CHAR(10));
        SET @RawMessagePlain = REPLACE(@RawMessagePlain, '<b>',  '*'); SET @RawMessagePlain = REPLACE(@RawMessagePlain, '</b>', '*');
        SET @RawMessagePlain = REPLACE(@RawMessagePlain, '<B>',  '*'); SET @RawMessagePlain = REPLACE(@RawMessagePlain, '</B>', '*');
        SET @RawMessagePlain = REPLACE(@RawMessagePlain, '<pre>',  ''); SET @RawMessagePlain = REPLACE(@RawMessagePlain, '</pre>', '');
        SET @RawMessagePlain = REPLACE(@RawMessagePlain, '</a>',  '');
        SET @TagStart = CHARINDEX('<', @RawMessagePlain);
        WHILE @TagStart > 0
        BEGIN
            SET @TagEnd = CHARINDEX('>', @RawMessagePlain, @TagStart);
            IF @TagEnd > @TagStart SET @RawMessagePlain = STUFF(@RawMessagePlain, @TagStart, @TagEnd - @TagStart + 1, ''); ELSE BREAK;
            SET @TagStart = CHARINDEX('<', @RawMessagePlain);
        END;

        -- Aplicar template de WhatsApp
        SET @FinalWhatsAppBody = REPLACE(REPLACE(@WhatsAppTemplateMsg, '<<NAME>>', ISNULL(@SellerName, '')), '<<MESSAGE>>', @RawMessagePlain);

        -- Montar JsonData
        SET @JsonDataEmail =
            N'{"To":"'       + STRING_ESCAPE(ISNULL(@SellerEmail, ''),          'json') +
            N'","ToName":"'  + STRING_ESCAPE(ISNULL(@SellerName, ''),           'json') +
            N'","Phone":"'   + STRING_ESCAPE(ISNULL(@SellerPhone, ''),          'json') +
            N'","Subject":"' + STRING_ESCAPE(ISNULL(@FinalEmailSubject, ''),    'json') +
            N'","Body":"'    + STRING_ESCAPE(ISNULL(@FinalEmailBody, ''),       'json') +
            N'","From":"'    + STRING_ESCAPE(ISNULL(@EmailTemplateFrom, ''),    'json') +
            N'","NameFrom":"'+ STRING_ESCAPE(ISNULL(@EmailTemplateNameFrom,''), 'json') + N'"}';

        SET @JsonDataWhatsApp =
            N'{"To":"'      + STRING_ESCAPE(ISNULL(@SellerEmail, ''),      'json') +
            N'","ToName":"' + STRING_ESCAPE(ISNULL(@SellerName, ''),       'json') +
            N'","Phone":"'  + STRING_ESCAPE(ISNULL(@SellerPhone, ''),      'json') +
            N'","Subject":"","Body":"' + STRING_ESCAPE(ISNULL(@FinalWhatsAppBody, ''), 'json') +
            N'","From":"","NameFrom":""}';

        INSERT INTO dbo.MsgToSend (DtCreated, Type, MsgRefType, SellerCode, SubmittedAnswersId, FupId, JsonData, DtToSend, Status, DtStatus)
        VALUES (GETDATE(), 'EMAIL',    'QuestionnaireFeedback', @CurrentSellerCode, @SubmittedAnswersId, NULL, @JsonDataEmail,    GETDATE(), 'Pending', GETDATE());

        INSERT INTO dbo.MsgToSend (DtCreated, Type, MsgRefType, SellerCode, SubmittedAnswersId, FupId, JsonData, DtToSend, Status, DtStatus)
        VALUES (GETDATE(), 'WHATSAPP', 'QuestionnaireFeedback', @CurrentSellerCode, @SubmittedAnswersId, NULL, @JsonDataWhatsApp, GETDATE(), 'Pending', GETDATE());

        PRINT 'DEBUG: Mensagens de Questionário inseridas para SubmittedAnswersId: ' + ISNULL(CAST(@SubmittedAnswersId AS NVARCHAR), 'NULO');

        FETCH NEXT FROM cur_submitted_questionnaires INTO
            @SubmittedAnswersId, @CurrentSellerId, @CurrentSellerCode, @CurrentDtSurvey, @CurrentSurveyType;
    END;

    CLOSE cur_submitted_questionnaires;
    DEALLOCATE cur_submitted_questionnaires;
    PRINT 'DEBUG: Processamento de Questionários finalizado.';

    -- =========================================================================
    -- PROCESSAMENTO DE LEMBRETES DE FUP
    -- =========================================================================
    PRINT 'DEBUG: Iniciando processamento de Pendências (FUP).';

    OPEN cur_fup_reminders;
    FETCH NEXT FROM cur_fup_reminders INTO
        @FupId, @FupDescription, @FupDtExpectedConclusion, @FupPriority, @FupStatus,
        @FupCustomerCode, @FupSellerCode_FUP, @FupReminderDate, @FupDtInclusion, @FupDtConclusion,
        @FupCategory, @FupPostponementCount, @FupQuestion, @FupLog,
        @EmailReason, @IsWeekendAdjusted;

    WHILE @@FETCH_STATUS = 0
    BEGIN
        PRINT 'DEBUG: Processando FUPId: ' + ISNULL(CAST(@FupId AS NVARCHAR), 'NULO') + ', Motivo: ' + ISNULL(@EmailReason, 'NULO');

        SET @FupResponsibleEmail = NULL; SET @FupResponsibleName = NULL; SET @FupResponsiblePhone = NULL;
        SET @ContractId = NULL; SET @LookupUserId = NULL;

        SELECT TOP 1 @LookupUserId = du.UserId
        FROM dbo.DeptUser du WHERE du.CompanyCodeUser = @FupSellerCode_FUP;

        IF @LookupUserId IS NOT NULL
            SELECT TOP 1
                @FupResponsibleEmail = U.Email,
                @FupResponsibleName  = U.Name,
                @FupResponsiblePhone = ISNULL(U.DddCell, '') + ISNULL(U.NrCell, ''),
                @ContractId          = U.ContractId
            FROM dbo.[User] U WHERE U.UserId = @LookupUserId;

        IF @FupResponsibleEmail IS NULL
        BEGIN
            PRINT N'AVISO: E-mail não encontrado para FupId: ' + ISNULL(CAST(@FupId AS NVARCHAR), 'NULO') + '. Pulando.';
            FETCH NEXT FROM cur_fup_reminders INTO
                @FupId, @FupDescription, @FupDtExpectedConclusion, @FupPriority, @FupStatus,
                @FupCustomerCode, @FupSellerCode_FUP, @FupReminderDate, @FupDtInclusion, @FupDtConclusion,
                @FupCategory, @FupPostponementCount, @FupQuestion, @FupLog,
                @EmailReason, @IsWeekendAdjusted;
            CONTINUE;
        END;

        -- Carregar template do contrato
        SET @JsonParam = NULL;
        IF @ContractId IS NOT NULL
            SELECT TOP 1 @JsonParam = C.JsonParam FROM dbo.[Contract] C WHERE C.ContractId = @ContractId;

        SET @EmailTemplateSubject  = ISNULL(JSON_VALUE(@JsonParam, '$.MsgEmailTemplate.Subject'),   N'SuperVision360 - <<SUBJECT>>');
        SET @EmailTemplateMessage  = ISNULL(JSON_VALUE(@JsonParam, '$.MsgEmailTemplate.Message'),   N'Olá <<NAME>>,<br><br><<MESSAGE>><br><br>Caso você não tenha solicitado, pode ignorar essa mensagem.<br><br><br>Suporte SuperVision 360');
        SET @EmailTemplateFrom     = ISNULL(JSON_VALUE(@JsonParam, '$.MsgEmailTemplate.From'),      'naoresponda@supervision360.com.br');
        SET @EmailTemplateNameFrom = ISNULL(JSON_VALUE(@JsonParam, '$.MsgEmailTemplate.NameFrom'),  N'Não responda - SuperVision 360');
        SET @WhatsAppTemplateMsg   = ISNULL(JSON_VALUE(@JsonParam, '$.MsgWhasAppTemplate.Message'), N'Olá <<NAME>>, ' + CHAR(10) + '<<MESSAGE>>');

        SET @FupDtExpectedConclusionFormatted = ISNULL(FORMAT(@FupDtExpectedConclusion, 'dd/MM/yyyy'), 'Não Definida');

        SET @RawSubject = N'[Lembrete de Pendência - ' + ISNULL(@FupPriority, 'Normal') + N'] ' +
            LEFT(ISNULL(@FupDescription, 'Pendência sem descrição'), 70) +
            N' (Motivo: ' + ISNULL(@EmailReason, 'N/A') + N')';
        IF LEN(ISNULL(@FupDescription, '')) > 70 SET @RawSubject = @RawSubject + N'...';

        SET @RawMessageContent =
            N'Este é um lembrete sobre uma pendência em aberto sob sua responsabilidade.<br>' +
            N'Motivo: <b>' + ISNULL(@EmailReason, 'N/A') + N'</b>' +
            CASE WHEN @IsWeekendAdjusted = 1 THEN N'<br>Data de envio antecipada devido ao fim de semana.' ELSE N'' END +
            N'<br><br>' +
            N'<b>ID da Pendência:</b> '      + ISNULL(CAST(@FupId AS NVARCHAR), 'N/A') + N'<br>' +
            N'<b>Vendedor Responsável:</b> ' + ISNULL(@FupSellerCode_FUP, 'N/A')        + N'<br>' +
            N'<b>Descrição:</b> '            + ISNULL(@FupDescription, 'N/A')            + N'<br>' +
            N'<b>Categoria:</b> '            + ISNULL(@FupCategory, 'N/A')               + N'<br>' +
            N'<b>Cliente:</b> '              + ISNULL(@FupCustomerCode, 'N/A')           + N'<br>' +
            N'<b>Conclusão Prevista:</b> '   + @FupDtExpectedConclusionFormatted         + N'<br>' +
            N'<b>Prioridade:</b> '           + ISNULL(@FupPriority, 'N/A')               + N'<br>' +
            N'<b>Status Atual:</b> '         + ISNULL(@FupStatus, 'N/A')                 + N'<br>' +
            N'<b>Qtd. de Postergações:</b> ' + ISNULL(CAST(@FupPostponementCount AS NVARCHAR), '0') + N'<br>' +
            N'<b>Questão/Item:</b> '         + ISNULL(@FupQuestion, 'N/A')               + N'<br>' +
            N'<b>Histórico:</b> <pre>'       + ISNULL(@FupLog, 'N/A')                   + N'</pre><br><br>' +
            N'Por favor, tome as ações necessárias para resolver esta pendência.';

        SET @FinalEmailSubject = REPLACE(@EmailTemplateSubject, '<<SUBJECT>>', @RawSubject);
        SET @FinalEmailBody    = REPLACE(REPLACE(@EmailTemplateMessage, '<<NAME>>', ISNULL(@FupResponsibleName, '')), '<<MESSAGE>>', @RawMessageContent);

        SET @RawMessagePlain = @RawMessageContent;
        SET @RawMessagePlain = REPLACE(@RawMessagePlain, '<br><br>', CHAR(10) + CHAR(10));
        SET @RawMessagePlain = REPLACE(@RawMessagePlain, '<br>',     CHAR(10));
        SET @RawMessagePlain = REPLACE(@RawMessagePlain, '<BR><BR>', CHAR(10) + CHAR(10));
        SET @RawMessagePlain = REPLACE(@RawMessagePlain, '<BR>',     CHAR(10));
        SET @RawMessagePlain = REPLACE(@RawMessagePlain, '<b>',  '*'); SET @RawMessagePlain = REPLACE(@RawMessagePlain, '</b>', '*');
        SET @RawMessagePlain = REPLACE(@RawMessagePlain, '<B>',  '*'); SET @RawMessagePlain = REPLACE(@RawMessagePlain, '</B>', '*');
        SET @RawMessagePlain = REPLACE(@RawMessagePlain, '<pre>',  ''); SET @RawMessagePlain = REPLACE(@RawMessagePlain, '</pre>', '');
        SET @RawMessagePlain = REPLACE(@RawMessagePlain, '</a>',   '');
        SET @TagStart = CHARINDEX('<', @RawMessagePlain);
        WHILE @TagStart > 0
        BEGIN
            SET @TagEnd = CHARINDEX('>', @RawMessagePlain, @TagStart);
            IF @TagEnd > @TagStart SET @RawMessagePlain = STUFF(@RawMessagePlain, @TagStart, @TagEnd - @TagStart + 1, ''); ELSE BREAK;
            SET @TagStart = CHARINDEX('<', @RawMessagePlain);
        END;

        SET @FinalWhatsAppBody = REPLACE(REPLACE(@WhatsAppTemplateMsg, '<<NAME>>', ISNULL(@FupResponsibleName, '')), '<<MESSAGE>>', @RawMessagePlain);

        SET @JsonDataEmail =
            N'{"To":"'       + STRING_ESCAPE(ISNULL(@FupResponsibleEmail, ''),  'json') +
            N'","ToName":"'  + STRING_ESCAPE(ISNULL(@FupResponsibleName, ''),   'json') +
            N'","Phone":"'   + STRING_ESCAPE(ISNULL(@FupResponsiblePhone, ''),  'json') +
            N'","Subject":"' + STRING_ESCAPE(ISNULL(@FinalEmailSubject, ''),    'json') +
            N'","Body":"'    + STRING_ESCAPE(ISNULL(@FinalEmailBody, ''),       'json') +
            N'","From":"'    + STRING_ESCAPE(ISNULL(@EmailTemplateFrom, ''),    'json') +
            N'","NameFrom":"'+ STRING_ESCAPE(ISNULL(@EmailTemplateNameFrom,''), 'json') + N'"}';

        SET @JsonDataWhatsApp =
            N'{"To":"'      + STRING_ESCAPE(ISNULL(@FupResponsibleEmail, ''), 'json') +
            N'","ToName":"' + STRING_ESCAPE(ISNULL(@FupResponsibleName, ''),  'json') +
            N'","Phone":"'  + STRING_ESCAPE(ISNULL(@FupResponsiblePhone, ''), 'json') +
            N'","Subject":"","Body":"' + STRING_ESCAPE(ISNULL(@FinalWhatsAppBody, ''), 'json') +
            N'","From":"","NameFrom":""}';

        INSERT INTO dbo.MsgToSend (DtCreated, Type, MsgRefType, SellerCode, SubmittedAnswersId, FupId, JsonData, DtToSend, Status, DtStatus)
        VALUES (GETDATE(), 'EMAIL',    'FUPReminder', @FupSellerCode_FUP, NULL, @FupId, @JsonDataEmail,    GETDATE(), 'Pending', GETDATE());

        INSERT INTO dbo.MsgToSend (DtCreated, Type, MsgRefType, SellerCode, SubmittedAnswersId, FupId, JsonData, DtToSend, Status, DtStatus)
        VALUES (GETDATE(), 'WHATSAPP', 'FUPReminder', @FupSellerCode_FUP, NULL, @FupId, @JsonDataWhatsApp, GETDATE(), 'Pending', GETDATE());

        PRINT N'DEBUG: Mensagens de FUP Lembrete inseridas para FupId: ' + ISNULL(CAST(@FupId AS NVARCHAR), 'NULO');

        FETCH NEXT FROM cur_fup_reminders INTO
            @FupId, @FupDescription, @FupDtExpectedConclusion, @FupPriority, @FupStatus,
            @FupCustomerCode, @FupSellerCode_FUP, @FupReminderDate, @FupDtInclusion, @FupDtConclusion,
            @FupCategory, @FupPostponementCount, @FupQuestion, @FupLog,
            @EmailReason, @IsWeekendAdjusted;
    END;

    CLOSE cur_fup_reminders;
    DEALLOCATE cur_fup_reminders;
    PRINT 'DEBUG: Processamento de Pendências (FUP) finalizado.';

    -- =========================================================================
    -- PROCESSAMENTO DE CONCLUSÃO DE FUP
    -- =========================================================================
    PRINT 'DEBUG: Iniciando processamento de Notificação de Conclusão de FUP.';

    OPEN cur_fup_completions;
    FETCH NEXT FROM cur_fup_completions INTO
        @FupId, @FupDescription, @FupDtExpectedConclusion, @FupPriority, @FupStatus,
        @FupCustomerCode, @FupSellerCode_FUP, @FupDtConclusion,
        @FupCategory, @FupPostponementCount, @FupQuestion, @FupLog,
        @EmailReason, @IsWeekendAdjusted;

    WHILE @@FETCH_STATUS = 0
    BEGIN
        PRINT 'DEBUG: Processando FUP Concluída FupId: ' + ISNULL(CAST(@FupId AS NVARCHAR), 'NULO');

        SET @FupResponsibleEmail = NULL; SET @FupResponsibleName = NULL; SET @FupResponsiblePhone = NULL;
        SET @ContractId = NULL; SET @LookupUserId = NULL;

        SELECT TOP 1 @LookupUserId = du.UserId
        FROM dbo.DeptUser du WHERE du.CompanyCodeUser = @FupSellerCode_FUP;

        IF @LookupUserId IS NOT NULL
            SELECT TOP 1
                @FupResponsibleEmail = U.Email,
                @FupResponsibleName  = U.Name,
                @FupResponsiblePhone = ISNULL(U.DddCell, '') + ISNULL(U.NrCell, ''),
                @ContractId          = U.ContractId
            FROM dbo.[User] U WHERE U.UserId = @LookupUserId;

        IF @FupResponsibleEmail IS NULL
        BEGIN
            PRINT N'AVISO: E-mail não encontrado para FUP Concluída FupId: ' + ISNULL(CAST(@FupId AS NVARCHAR), 'NULO') + '. Pulando.';
            FETCH NEXT FROM cur_fup_completions INTO
                @FupId, @FupDescription, @FupDtExpectedConclusion, @FupPriority, @FupStatus,
                @FupCustomerCode, @FupSellerCode_FUP, @FupDtConclusion,
                @FupCategory, @FupPostponementCount, @FupQuestion, @FupLog,
                @EmailReason, @IsWeekendAdjusted;
            CONTINUE;
        END;

        -- Carregar template do contrato
        SET @JsonParam = NULL;
        IF @ContractId IS NOT NULL
            SELECT TOP 1 @JsonParam = C.JsonParam FROM dbo.[Contract] C WHERE C.ContractId = @ContractId;

        SET @EmailTemplateSubject  = ISNULL(JSON_VALUE(@JsonParam, '$.MsgEmailTemplate.Subject'),   N'SuperVision360 - <<SUBJECT>>');
        SET @EmailTemplateMessage  = ISNULL(JSON_VALUE(@JsonParam, '$.MsgEmailTemplate.Message'),   N'Olá <<NAME>>,<br><br><<MESSAGE>><br><br>Caso você não tenha solicitado, pode ignorar essa mensagem.<br><br><br>Suporte SuperVision 360');
        SET @EmailTemplateFrom     = ISNULL(JSON_VALUE(@JsonParam, '$.MsgEmailTemplate.From'),      'naoresponda@supervision360.com.br');
        SET @EmailTemplateNameFrom = ISNULL(JSON_VALUE(@JsonParam, '$.MsgEmailTemplate.NameFrom'),  N'Não responda - SuperVision 360');
        SET @WhatsAppTemplateMsg   = ISNULL(JSON_VALUE(@JsonParam, '$.MsgWhasAppTemplate.Message'), N'Olá <<NAME>>, ' + CHAR(10) + '<<MESSAGE>>');

        SET @FupDtConclusionFormatted = ISNULL(FORMAT(@FupDtConclusion, 'dd/MM/yyyy'), 'Não Definida');

        SET @RawSubject = N'[FUP Concluída - ' + ISNULL(@FupCustomerCode, 'Cliente') + N'] ' +
            LEFT(ISNULL(@FupDescription, 'Pendência sem descrição'), 70);
        IF LEN(ISNULL(@FupDescription, '')) > 70 SET @RawSubject = @RawSubject + N'...';

        SET @RawMessageContent =
            N'Gostaríamos de informar que a seguinte pendência foi marcada como concluída em <b>' +
            FORMAT(@CurrentDate, 'dd/MM/yyyy') + N'</b>' +
            CASE WHEN @IsWeekendAdjusted = 1 THEN N' (Data de conclusão antecipada devido ao fim de semana).' ELSE N'.' END +
            N'<br><br>' +
            N'<b>ID da Pendência:</b> '           + ISNULL(CAST(@FupId AS NVARCHAR), 'N/A')             + N'<br>' +
            N'<b>Descrição:</b> '                 + ISNULL(@FupDescription, 'N/A')                        + N'<br>' +
            N'<b>Categoria:</b> '                 + ISNULL(@FupCategory, 'N/A')                           + N'<br>' +
            N'<b>Cliente:</b> '                   + ISNULL(@FupCustomerCode, 'N/A')                       + N'<br>' +
            N'<b>Vendedor Responsável:</b> '      + ISNULL(@FupSellerCode_FUP, 'N/A')                     + N'<br>' +
            N'<b>Concluída em:</b> '              + @FupDtConclusionFormatted                             + N'<br>' +
            N'<b>Prioridade:</b> '                + ISNULL(@FupPriority, 'N/A')                           + N'<br>' +
            N'<b>Status:</b> '                    + ISNULL(@FupStatus, 'N/A')                             + N'<br>' +
            N'<b>Qtd. de Postergações:</b> '      + ISNULL(CAST(@FupPostponementCount AS NVARCHAR), '0') + N'<br>' +
            N'<b>Questão/Item da Pendência:</b> ' + ISNULL(@FupQuestion, 'N/A')                           + N'<br>' +
            N'<b>Histórico/Log:</b> <pre>'        + ISNULL(@FupLog, 'N/A')                               + N'</pre>';

        SET @FinalEmailSubject = REPLACE(@EmailTemplateSubject, '<<SUBJECT>>', @RawSubject);
        SET @FinalEmailBody    = REPLACE(REPLACE(@EmailTemplateMessage, '<<NAME>>', ISNULL(@FupResponsibleName, '')), '<<MESSAGE>>', @RawMessageContent);

        SET @RawMessagePlain = @RawMessageContent;
        SET @RawMessagePlain = REPLACE(@RawMessagePlain, '<br><br>', CHAR(10) + CHAR(10));
        SET @RawMessagePlain = REPLACE(@RawMessagePlain, '<br>',     CHAR(10));
        SET @RawMessagePlain = REPLACE(@RawMessagePlain, '<BR><BR>', CHAR(10) + CHAR(10));
        SET @RawMessagePlain = REPLACE(@RawMessagePlain, '<BR>',     CHAR(10));
        SET @RawMessagePlain = REPLACE(@RawMessagePlain, '<b>',  '*'); SET @RawMessagePlain = REPLACE(@RawMessagePlain, '</b>', '*');
        SET @RawMessagePlain = REPLACE(@RawMessagePlain, '<B>',  '*'); SET @RawMessagePlain = REPLACE(@RawMessagePlain, '</B>', '*');
        SET @RawMessagePlain = REPLACE(@RawMessagePlain, '<pre>',  ''); SET @RawMessagePlain = REPLACE(@RawMessagePlain, '</pre>', '');
        SET @RawMessagePlain = REPLACE(@RawMessagePlain, '</a>',   '');
        SET @TagStart = CHARINDEX('<', @RawMessagePlain);
        WHILE @TagStart > 0
        BEGIN
            SET @TagEnd = CHARINDEX('>', @RawMessagePlain, @TagStart);
            IF @TagEnd > @TagStart SET @RawMessagePlain = STUFF(@RawMessagePlain, @TagStart, @TagEnd - @TagStart + 1, ''); ELSE BREAK;
            SET @TagStart = CHARINDEX('<', @RawMessagePlain);
        END;

        SET @FinalWhatsAppBody = REPLACE(REPLACE(@WhatsAppTemplateMsg, '<<NAME>>', ISNULL(@FupResponsibleName, '')), '<<MESSAGE>>', @RawMessagePlain);

        SET @JsonDataEmail =
            N'{"To":"'       + STRING_ESCAPE(ISNULL(@FupResponsibleEmail, ''),  'json') +
            N'","ToName":"'  + STRING_ESCAPE(ISNULL(@FupResponsibleName, ''),   'json') +
            N'","Phone":"'   + STRING_ESCAPE(ISNULL(@FupResponsiblePhone, ''),  'json') +
            N'","Subject":"' + STRING_ESCAPE(ISNULL(@FinalEmailSubject, ''),    'json') +
            N'","Body":"'    + STRING_ESCAPE(ISNULL(@FinalEmailBody, ''),       'json') +
            N'","From":"'    + STRING_ESCAPE(ISNULL(@EmailTemplateFrom, ''),    'json') +
            N'","NameFrom":"'+ STRING_ESCAPE(ISNULL(@EmailTemplateNameFrom,''), 'json') + N'"}';

        SET @JsonDataWhatsApp =
            N'{"To":"'      + STRING_ESCAPE(ISNULL(@FupResponsibleEmail, ''), 'json') +
            N'","ToName":"' + STRING_ESCAPE(ISNULL(@FupResponsibleName, ''),  'json') +
            N'","Phone":"'  + STRING_ESCAPE(ISNULL(@FupResponsiblePhone, ''), 'json') +
            N'","Subject":"","Body":"' + STRING_ESCAPE(ISNULL(@FinalWhatsAppBody, ''), 'json') +
            N'","From":"","NameFrom":""}';

        INSERT INTO dbo.MsgToSend (DtCreated, Type, MsgRefType, SellerCode, SubmittedAnswersId, FupId, JsonData, DtToSend, Status, DtStatus)
        VALUES (GETDATE(), 'EMAIL',    'FUPCompletion', @FupSellerCode_FUP, NULL, @FupId, @JsonDataEmail,    GETDATE(), 'Pending', GETDATE());

        INSERT INTO dbo.MsgToSend (DtCreated, Type, MsgRefType, SellerCode, SubmittedAnswersId, FupId, JsonData, DtToSend, Status, DtStatus)
        VALUES (GETDATE(), 'WHATSAPP', 'FUPCompletion', @FupSellerCode_FUP, NULL, @FupId, @JsonDataWhatsApp, GETDATE(), 'Pending', GETDATE());

        PRINT N'DEBUG: Mensagens de Conclusão FUP inseridas para FupId: ' + ISNULL(CAST(@FupId AS NVARCHAR), 'NULO');

        FETCH NEXT FROM cur_fup_completions INTO
            @FupId, @FupDescription, @FupDtExpectedConclusion, @FupPriority, @FupStatus,
            @FupCustomerCode, @FupSellerCode_FUP, @FupDtConclusion,
            @FupCategory, @FupPostponementCount, @FupQuestion, @FupLog,
            @EmailReason, @IsWeekendAdjusted;
    END;

    CLOSE cur_fup_completions;
    DEALLOCATE cur_fup_completions;
    PRINT 'DEBUG: Processamento de Notificação de Conclusão de FUP finalizado.';

    PRINT 'DEBUG: Fim da execução da procedure dbo.SP_ProcessMsgsToSend.';
END;
GO

-- executer em PROD: GRANT EXECUTE ON dbo.SP_ProcessMsgsToSend TO s360batch;
GO
