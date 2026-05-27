/****** Object:  StoredProcedure [dbo].[GenerateSellerQuestionnaireEmails]    Script Date: 27/05/2026 16:29:36 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE [dbo].[GenerateSellerQuestionnaireEmails]
AS
BEGIN
    SET NOCOUNT ON;

    PRINT 'DEBUG: Início da execução da procedure dbo.GenerateSellerQuestionnaireEmails.';

    -- =========================================================================
    -- TODAS AS DECLARAÇÕES DE VARIÁVEIS SÃO AGRUPADAS AQUI
    -- =========================================================================
        -- executer em PROD:
	-- GRANT EXECUTE ON dbo.GenerateSellerQuestionnaireEmails TO s360batch 

    -- Variável para a URL base do Azure Blob Storage
    DECLARE @AzureBlobBaseUrl NVARCHAR(500) = 'https://storagebdh.blob.core.windows.net/s360/';
    -- Variáveis de controle de data para FUPs
    DECLARE @CurrentDate DATE;

    -- Variáveis para Questionários
    DECLARE @SubmittedAnswersId INT;
    DECLARE @CurrentSellerId INT;
    DECLARE @CurrentSellerCode VARCHAR(100);
    DECLARE @CurrentDtSurvey DATE;
    DECLARE @CurrentSurveyType VARCHAR(100);
    DECLARE @SellerEmail VARCHAR(255);
    DECLARE @SellerName VARCHAR(255);
    DECLARE @FromEmail VARCHAR(255) = 'naoresponda@supervision360.com.br';
    DECLARE @EmailSubject NVARCHAR(1000);
    DECLARE @EmailBody NVARCHAR(MAX);
    DECLARE @QuestionnaireDateFormatted VARCHAR(10);
    DECLARE @SurveyTypeDisplayName NVARCHAR(100);
    DECLARE @QuestionDetailText NVARCHAR(MAX);
    DECLARE @CustomerCode VARCHAR(100);

    -- Variáveis para FUPs
    DECLARE @FupId INT;
    DECLARE @FupDescription NVARCHAR(MAX);
    DECLARE @FupDtExpectedConclusion DATETIME;
    DECLARE @FupPriority VARCHAR(100);
    DECLARE @FupStatus VARCHAR(100);
    DECLARE @FupCustomerCode VARCHAR(100);
    DECLARE @FupSellerCode_FUP VARCHAR(100); -- Este é o CompanyCodeUser (ou outro identificador) do FUP
    DECLARE @FupReminderDate DATETIME;
    DECLARE @FupDtInclusion DATETIME;
    DECLARE @FupDtConclusion DATETIME;
    DECLARE @FupResponsibleEmail VARCHAR(255);
    DECLARE @FupResponsibleName NVARCHAR(255);
    DECLARE @FupSubject_FUP NVARCHAR(1000);
    DECLARE @FupBody_FUP NVARCHAR(MAX);
    DECLARE @FupDtExpectedConclusionFormatted VARCHAR(10); -- Formatada para email de lembrete
    DECLARE @FupDtConclusionFormatted VARCHAR(10);        -- Formatada para email de conclusão
    DECLARE @EmailReason NVARCHAR(100); -- Variável para o motivo do e-mail
    DECLARE @IsWeekendAdjusted BIT;

    -- NOVAS VARIÁVEIS PARA OS DETALHES DO FUP
    DECLARE @FupCategory NVARCHAR(100);
    DECLARE @FupPostponementCount INT;
    DECLARE @FupQuestion NVARCHAR(1000);
    DECLARE @FupLog NVARCHAR(MAX);

    -- Variável para armazenar o UserId encontrado durante o lookup
    DECLARE @LookupUserId INT;


    -- =========================================================================
    -- INICIALIZAÇÕES DE VARIÁVEIS (APÓS TODAS AS DECLARAÇÕES DE VARIÁVEIS)
    -- =========================================================================
    SET @CurrentDate = CAST(GETDATE() AS DATE);
    -- <<<< PARA TESTE, PODE ALTERAR TEMPORARIAMENTE PARA UMA DATA ESPECÍFICA, ex: '2026-02-25'
    -- SET @CurrentDate = '2026-02-25';
    PRINT 'DEBUG: @CurrentDate definida como: ' + CAST(@CurrentDate AS NVARCHAR(MAX));


    -- =========================================================================
    -- TODAS AS DECLARAÇÕES DE CURSORES SÃO AGRUPADAS AQUI
    -- =========================================================================
    -- Cursor para Questionários
    DECLARE cur_submitted_questionnaires CURSOR LOCAL FAST_FORWARD FOR
    SELECT
        sa.SubmittedAnswersId,
        sa.SellerId,
        sa.SellerCode,
        sa.DtSurvey,
        sa.SurveyType
    FROM
        dbo.SubmittedAnswers sa
    WHERE
        NOT EXISTS (
            SELECT 1
            FROM dbo.MsgEmailToSend mse
            WHERE mse.SubmittedAnswersId = sa.SubmittedAnswersId
            AND mse.MsgType = 'QuestionnaireFeedback'
        )
        AND sa.SurveyType <> 'AVALIACAO_MERCADO'
        AND sa.SellerId IS NOT NULL -- Ignora registros de SubmittedAnswers sem SellerId
    ;

    -- Cursor para lembretes de FUP (Casos 1, 2, 3, 5 - FUPs não concluídos/cancelados)
    -- MODIFICADO PARA INCLUIR LEMBRETES SEMANAIS PARA FUPs VENCIDAS E O PRIMEIRO LEMBRETE
    DECLARE cur_fup_reminders CURSOR LOCAL FAST_FORWARD FOR
    WITH FupRelevantDates AS (
        SELECT
            f.FupId,
            f.description,
            f.dtExpectedConclusion,
            f.priority,
            f.status,
            f.customerCode,
            f.sellerCode, -- Este é o CompanyCodeUser (ou outro identificador) do FUP
            f.reminderDate,
            f.dtInclusion,
            f.dtConclusion,
            f.category,          -- Categoria
            f.postponementCount, -- Qtd. Postergações
            f.question,          -- Questão da pendência
            f.log,               -- Log da pendência
            -- Calcular datas de gatilho potenciais (NÃO AJUSTADAS para fim de semana)
            CAST(f.dtInclusion AS DATE) AS Unadj_CreationDate,
            CAST(f.reminderDate AS DATE) AS Unadj_ReminderDate,
            CASE
                WHEN f.reminderDate IS NULL AND f.dtExpectedConclusion IS NOT NULL
                THEN CAST(DATEADD(day, -3, f.dtExpectedConclusion) AS DATE)
                ELSE NULL
            END AS Unadj_ReminderNullExp3Days,
            CAST(f.dtExpectedConclusion AS DATE) AS Unadj_ExpectedConclusionDate,
            CASE
                WHEN f.dtExpectedConclusion IS NOT NULL AND f.dtExpectedConclusion < @CurrentDate -- Deve estar vencido para ser recorrente
                AND DATEDIFF(day, f.dtExpectedConclusion, @CurrentDate) > 0 -- Não ser o mesmo dia da expectativa de conclusão
                AND (DATEDIFF(day, f.dtExpectedConclusion, @CurrentDate) % 30 = 0)
                THEN CAST(DATEADD(day, (DATEDIFF(day, f.dtExpectedConclusion, @CurrentDate) / 30) * 30, f.dtExpectedConclusion) AS DATE)
                ELSE NULL
            END AS Unadj_RecurringDate,
            CAST(f.dtConclusion AS DATE) AS Unadj_CompletionDate
        FROM dbo.Fup f
        WHERE (f.active = 1 OR f.active IS NULL)
    ),
    -- Nova CTE para verificar se já houve um lembrete para a FUP
    FupHasReminders AS (
        SELECT
            frd.FupId,
            CASE WHEN EXISTS (
                SELECT 1 FROM dbo.MsgEmailToSend mse
                WHERE mse.FupId = frd.FupId
                AND mse.MsgType = 'FUPReminder'
            ) THEN 1 ELSE 0 END AS HasBeenRemindedBefore
        FROM FupRelevantDates frd
    ),
    FupAdjustedDates AS (
        SELECT
            -- Seleciona todos os campos originais do FUP e as datas não ajustadas da FupRelevantDates (fd)
            fd.FupId, fd.description, fd.dtExpectedConclusion, fd.priority, fd.status,
            fd.customerCode, fd.sellerCode, fd.reminderDate, fd.dtInclusion, fd.dtConclusion,
            fd.category, fd.postponementCount, fd.question, fd.log,
            fd.Unadj_CreationDate,
            fd.Unadj_ReminderDate,
            fd.Unadj_ReminderNullExp3Days,
            fd.Unadj_ExpectedConclusionDate,
            fd.Unadj_RecurringDate,
            fd.Unadj_CompletionDate,

            -- Seleciona a flag de "já lembrado" da FupHasReminders (fhr)
            fhr.HasBeenRemindedBefore,

            -- Calcula as Datas Ajustadas (Adj_) usando as datas não ajustadas (Unadj_)
            -- Nota: Estas colunas são calculadas aqui para serem referenciadas na próxima CTE,
            -- mas para uso interno no EmailReason_Calculated e IsWeekendAdjusted_Calculated nesta mesma CTE,
            -- a lógica completa do CASE WHEN é repetida para evitar problemas de escopo de alias.
            (CASE WHEN fd.Unadj_CreationDate IS NULL THEN NULL
                 WHEN DATEPART(weekday, fd.Unadj_CreationDate) = 1 THEN DATEADD(day, -2, fd.Unadj_CreationDate) -- Domingo -> Sexta
                 WHEN DATEPART(weekday, fd.Unadj_CreationDate) = 7 THEN DATEADD(day, -1, fd.Unadj_CreationDate) -- Sábado -> Sexta
                 ELSE fd.Unadj_CreationDate
            END) AS Adj_CreationDate,
            (CASE WHEN fd.Unadj_ReminderDate IS NULL THEN NULL
                 WHEN DATEPART(weekday, fd.Unadj_ReminderDate) = 1 THEN DATEADD(day, -2, fd.Unadj_ReminderDate)
                 WHEN DATEPART(weekday, fd.Unadj_ReminderDate) = 7 THEN DATEADD(day, -1, fd.Unadj_ReminderDate)
                 ELSE fd.Unadj_ReminderDate
            END) AS Adj_ReminderDate,
            (CASE WHEN fd.Unadj_ReminderNullExp3Days IS NULL THEN NULL
                 WHEN DATEPART(weekday, fd.Unadj_ReminderNullExp3Days) = 1 THEN DATEADD(day, -2, fd.Unadj_ReminderNullExp3Days)
                 WHEN DATEPART(weekday, fd.Unadj_ReminderNullExp3Days) = 7 THEN DATEADD(day, -1, fd.Unadj_ReminderNullExp3Days)
                 ELSE fd.Unadj_ReminderNullExp3Days
            END) AS Adj_ReminderNullExp3Days,
            (CASE WHEN fd.Unadj_ExpectedConclusionDate IS NULL THEN NULL
                 WHEN DATEPART(weekday, fd.Unadj_ExpectedConclusionDate) = 1 THEN DATEADD(day, -2, fd.Unadj_ExpectedConclusionDate)
                 WHEN DATEPART(weekday, fd.Unadj_ExpectedConclusionDate) = 7 THEN DATEADD(day, -1, fd.Unadj_ExpectedConclusionDate)
                 ELSE fd.Unadj_ExpectedConclusionDate
            END) AS Adj_ExpectedConclusionDate,
            (CASE WHEN fd.Unadj_RecurringDate IS NULL THEN NULL
                 WHEN DATEPART(weekday, fd.Unadj_RecurringDate) = 1 THEN DATEADD(day, -2, fd.Unadj_RecurringDate)
                 WHEN DATEPART(weekday, fd.Unadj_RecurringDate) = 7 THEN DATEADD(day, -1, fd.Unadj_RecurringDate)
                 ELSE fd.Unadj_RecurringDate
            END) AS Adj_RecurringDate,
            (CASE WHEN fd.Unadj_CompletionDate IS NULL THEN NULL
                 WHEN DATEPART(weekday, fd.Unadj_CompletionDate) = 1 THEN DATEADD(day, -2, fd.Unadj_CompletionDate)
                 WHEN DATEPART(weekday, fd.Unadj_CompletionDate) = 7 THEN DATEADD(day, -1, fd.Unadj_CompletionDate)
                 ELSE fd.Unadj_CompletionDate
            END) AS Adj_CompletionDate,

            -- Calcula EmailReason_Calculated (lógica completa reembutida)
            CASE
                WHEN fhr.HasBeenRemindedBefore = 0 AND fd.status = 'pending' THEN 'Primeiro Lembrete'
                WHEN (CASE WHEN fd.Unadj_CreationDate IS NULL THEN NULL WHEN DATEPART(weekday, fd.Unadj_CreationDate) = 1 THEN DATEADD(day, -2, fd.Unadj_CreationDate) WHEN DATEPART(weekday, fd.Unadj_CreationDate) = 7 THEN DATEADD(day, -1, fd.Unadj_CreationDate) ELSE fd.Unadj_CreationDate END) = @CurrentDate THEN 'Criação'
                WHEN (CASE WHEN fd.Unadj_ReminderDate IS NULL THEN NULL WHEN DATEPART(weekday, fd.Unadj_ReminderDate) = 1 THEN DATEADD(day, -2, fd.Unadj_ReminderDate) WHEN DATEPART(weekday, fd.Unadj_ReminderDate) = 7 THEN DATEADD(day, -1, fd.Unadj_ReminderDate) ELSE fd.Unadj_ReminderDate END) = @CurrentDate THEN 'Lembrete Configurado'
                WHEN (CASE WHEN fd.Unadj_ReminderNullExp3Days IS NULL THEN NULL WHEN DATEPART(weekday, fd.Unadj_ReminderNullExp3Days) = 1 THEN DATEADD(day, -2, fd.Unadj_ReminderNullExp3Days) WHEN DATEPART(weekday, fd.Unadj_ReminderNullExp3Days) = 7 THEN DATEADD(day, -1, fd.Unadj_ReminderNullExp3Days) ELSE fd.Unadj_ReminderNullExp3Days END) = @CurrentDate THEN 'Lembrete (3 dias antes da conclusão)'
                WHEN (CASE WHEN fd.Unadj_ExpectedConclusionDate IS NULL THEN NULL WHEN DATEPART(weekday, fd.Unadj_ExpectedConclusionDate) = 1 THEN DATEADD(day, -2, fd.Unadj_ExpectedConclusionDate) WHEN DATEPART(weekday, fd.Unadj_ExpectedConclusionDate) = 7 THEN DATEADD(day, -1, fd.Unadj_ExpectedConclusionDate) ELSE fd.Unadj_ExpectedConclusionDate END) = @CurrentDate THEN 'Data de Conclusão Prevista'
                WHEN (CASE WHEN fd.Unadj_RecurringDate IS NULL THEN NULL WHEN DATEPART(weekday, fd.Unadj_RecurringDate) = 1 THEN DATEADD(day, -2, fd.Unadj_RecurringDate) WHEN DATEPART(weekday, fd.Unadj_RecurringDate) = 7 THEN DATEADD(day, -1, fd.Unadj_RecurringDate) ELSE fd.Unadj_RecurringDate END) = @CurrentDate THEN 'Lembrete Recorrente (a cada 30 dias)'
                WHEN fd.status = 'pending' AND (CASE WHEN fd.Unadj_ExpectedConclusionDate IS NULL THEN NULL WHEN DATEPART(weekday, fd.Unadj_ExpectedConclusionDate) = 1 THEN DATEADD(day, -2, fd.Unadj_ExpectedConclusionDate) WHEN DATEPART(weekday, fd.Unadj_ExpectedConclusionDate) = 7 THEN DATEADD(day, -1, fd.Unadj_ExpectedConclusionDate) ELSE fd.Unadj_ExpectedConclusionDate END) < @CurrentDate
                    AND (DATEPART(weekday, @CurrentDate) = 2) -- Monday = 2 (assuming @@DATEFIRST = 7)
                    AND NOT EXISTS (
                        SELECT 1 FROM dbo.MsgEmailToSend mse_inner
                        WHERE mse_inner.FupId = fd.FupId
                        AND mse_inner.MsgType = 'FUPReminder'
                        AND CAST(mse_inner.DtCreated AS DATE) >= DATEADD(day, -7, @CurrentDate)
                    )
                THEN 'Pendência Vencida (Semanal)'
                ELSE NULL
            END AS EmailReason_Calculated,

            -- Calcula IsWeekendAdjusted_Calculated (lógica completa reembutida)
            (CASE
                WHEN (fd.Unadj_CreationDate IS NOT NULL AND (CASE WHEN fd.Unadj_CreationDate IS NULL THEN NULL WHEN DATEPART(weekday, fd.Unadj_CreationDate) = 1 THEN DATEADD(day, -2, fd.Unadj_CreationDate) WHEN DATEPART(weekday, fd.Unadj_CreationDate) = 7 THEN DATEADD(day, -1, fd.Unadj_CreationDate) ELSE fd.Unadj_CreationDate END) != fd.Unadj_CreationDate) THEN 1
                WHEN (fd.Unadj_ReminderDate IS NOT NULL AND (CASE WHEN fd.Unadj_ReminderDate IS NULL THEN NULL WHEN DATEPART(weekday, fd.Unadj_ReminderDate) = 1 THEN DATEADD(day, -2, fd.Unadj_ReminderDate) WHEN DATEPART(weekday, fd.Unadj_ReminderDate) = 7 THEN DATEADD(day, -1, fd.Unadj_ReminderDate) ELSE fd.Unadj_ReminderDate END) != fd.Unadj_ReminderDate) THEN 1
                WHEN (fd.Unadj_ReminderNullExp3Days IS NOT NULL AND (CASE WHEN fd.Unadj_ReminderNullExp3Days IS NULL THEN NULL WHEN DATEPART(weekday, fd.Unadj_ReminderNullExp3Days) = 1 THEN DATEADD(day, -2, fd.Unadj_ReminderNullExp3Days) WHEN DATEPART(weekday, fd.Unadj_ReminderNullExp3Days) = 7 THEN DATEADD(day, -1, fd.Unadj_ReminderNullExp3Days) ELSE fd.Unadj_ReminderNullExp3Days END) != fd.Unadj_ReminderNullExp3Days) THEN 1
                WHEN (fd.Unadj_ExpectedConclusionDate IS NOT NULL AND (CASE WHEN fd.Unadj_ExpectedConclusionDate IS NULL THEN NULL WHEN DATEPART(weekday, fd.Unadj_ExpectedConclusionDate) = 1 THEN DATEADD(day, -2, fd.Unadj_ExpectedConclusionDate) WHEN DATEPART(weekday, fd.Unadj_ExpectedConclusionDate) = 7 THEN DATEADD(day, -1, fd.Unadj_ExpectedConclusionDate) ELSE fd.Unadj_ExpectedConclusionDate END) != fd.Unadj_ExpectedConclusionDate) THEN 1
                WHEN (fd.Unadj_RecurringDate IS NOT NULL AND (CASE WHEN fd.Unadj_RecurringDate IS NULL THEN NULL WHEN DATEPART(weekday, fd.Unadj_RecurringDate) = 1 THEN DATEADD(day, -2, fd.Unadj_RecurringDate) WHEN DATEPART(weekday, fd.Unadj_RecurringDate) = 7 THEN DATEADD(day, -1, fd.Unadj_RecurringDate) ELSE fd.Unadj_RecurringDate END) != fd.Unadj_RecurringDate) THEN 1
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
        AND fa.sellerCode IS NOT NULL -- Garante que sellerCode não é NULL
        AND NOT EXISTS ( -- Impede múltiplos emails de FUPReminder para o mesmo FupId no mesmo dia (para qualquer motivo)
            SELECT 1 FROM dbo.MsgEmailToSend mse
            WHERE mse.MsgType = 'FUPReminder'
              AND mse.FupId = fa.FupId
              AND CAST(mse.DtCreated AS DATE) = @CurrentDate
        )
    ;

    -- Cursor para notificação de conclusão de FUP (Caso 4 - FUPs concluídos)
    DECLARE cur_fup_completions CURSOR LOCAL FAST_FORWARD FOR
    WITH FupRelevantDates AS (
        SELECT
            f.FupId,
            f.description,
            f.dtExpectedConclusion,
            f.priority,
            f.status,
            f.customerCode,
            f.sellerCode,
            f.reminderDate,
            f.dtInclusion,
            f.dtConclusion,
            f.category,
            f.postponementCount,
            f.question,
            f.log,
            CAST(f.dtInclusion AS DATE) AS Unadj_CreationDate,
            CAST(f.reminderDate AS DATE) AS Unadj_ReminderDate,
            CASE
                WHEN f.reminderDate IS NULL AND f.dtExpectedConclusion IS NOT NULL
                THEN CAST(DATEADD(day, -3, f.dtExpectedConclusion) AS DATE)
                ELSE NULL
            END AS Unadj_ReminderNullExp3Days,
            CAST(f.dtExpectedConclusion AS DATE) AS Unadj_ExpectedConclusionDate,
            CASE
                WHEN f.dtExpectedConclusion IS NOT NULL AND f.dtExpectedConclusion < @CurrentDate
                AND DATEDIFF(day, f.dtExpectedConclusion, @CurrentDate) > 0
                AND (DATEDIFF(day, f.dtExpectedConclusion, @CurrentDate) % 30 = 0)
                THEN CAST(DATEADD(day, (DATEDIFF(day, f.dtExpectedConclusion, @CurrentDate) / 30) * 30, f.dtExpectedConclusion) AS DATE)
                ELSE NULL
            END AS Unadj_RecurringDate,
            CAST(f.dtConclusion AS DATE) AS Unadj_CompletionDate
        FROM dbo.Fup f
        WHERE (f.active = 1 OR f.active IS NULL)
    ),
    FupAdjustedDates AS (
        SELECT
            fd.FupId, fd.description, fd.dtExpectedConclusion, fd.priority, fd.status,
            fd.customerCode, fd.sellerCode, fd.dtConclusion,
            fd.category, fd.postponementCount, fd.question, fd.log, -- PROPAGANDO NOVOS CAMPOS
            fd.Unadj_CompletionDate, -- Propagando a data não ajustada para uso posterior
            (CASE WHEN fd.Unadj_CompletionDate IS NULL THEN NULL
                 WHEN DATEPART(weekday, fd.Unadj_CompletionDate) = 1 THEN DATEADD(day, -2, fd.Unadj_CompletionDate)
                 WHEN DATEPART(weekday, fd.Unadj_CompletionDate) = 7 THEN DATEADD(day, -1, fd.Unadj_CompletionDate)
                 ELSE fd.Unadj_CompletionDate
            END) AS Adj_CompletionDate,
            (CASE WHEN fd.Unadj_CompletionDate IS NOT NULL AND (CASE WHEN fd.Unadj_CompletionDate IS NULL THEN NULL WHEN DATEPART(weekday, fd.Unadj_CompletionDate) = 1 THEN DATEADD(day, -2, fd.Unadj_CompletionDate) WHEN DATEPART(weekday, fd.Unadj_CompletionDate) = 7 THEN DATEADD(day, -1, fd.Unadj_CompletionDate) ELSE fd.Unadj_CompletionDate END) != fd.Unadj_CompletionDate THEN 1 ELSE 0 END) AS IsWeekendAdjusted_Calculated
        FROM FupRelevantDates fd
    )
    SELECT
        fa.FupId, fa.description, fa.dtExpectedConclusion, fa.priority, fa.status,
        fa.customerCode, fa.sellerCode, fa.dtConclusion,
        fa.category, fa.postponementCount, fa.question, fa.log,
        'Conclusão' AS EmailReason, -- Motivo fixo para conclusão
        fa.IsWeekendAdjusted_Calculated
    FROM FupAdjustedDates fa
    WHERE
        fa.status = 'completed'
        AND fa.dtConclusion IS NOT NULL
        AND fa.Adj_CompletionDate = @CurrentDate
        AND fa.sellerCode IS NOT NULL -- Garante que sellerCode não é NULL
        AND NOT EXISTS (
            SELECT 1 FROM dbo.MsgEmailToSend mse
            WHERE mse.MsgType = 'FUPCompletion'
              AND mse.FupId = fa.FupId
              AND CAST(mse.DtCreated AS DATE) = @CurrentDate
        )
    ;

    -- =========================================================================
    -- INÍCIO DA LÓGICA DE GERAÇÃO DE E-MAILS PARA QUESTIONÁRIOS
    -- =========================================================================
    PRINT 'DEBUG: Iniciando processamento de Questionários.';

    OPEN cur_submitted_questionnaires;

    FETCH NEXT FROM cur_submitted_questionnaires INTO
        @SubmittedAnswersId, @CurrentSellerId, @CurrentSellerCode, @CurrentDtSurvey, @CurrentSurveyType;

    WHILE @@FETCH_STATUS = 0
    BEGIN
        PRINT 'DEBUG: Processando Questionário SubmittedAnswersId: ' + ISNULL(CAST(@SubmittedAnswersId AS NVARCHAR(MAX)), 'NULO') + ', SellerId: ' + ISNULL(CAST(@CurrentSellerId AS NVARCHAR(MAX)), 'NULO') + ', SellerCode: ' + ISNULL(@CurrentSellerCode, 'NULO') + ', DtSurvey: ' + ISNULL(FORMAT(@CurrentDtSurvey, 'yyyy-MM-dd'), 'NULO');

        SET @SellerEmail = NULL;
        SET @SellerName = NULL;
        SELECT TOP 1 @SellerEmail = U.Email, @SellerName = U.Name
        FROM dbo.[User] U
        WHERE U.UserId = @CurrentSellerId;

        PRINT 'DEBUG: Email encontrado para SellerId ' + ISNULL(CAST(@CurrentSellerId AS NVARCHAR(MAX)), 'NULO') + ' (SellerCode: ' + ISNULL(@CurrentSellerCode, 'NULO') + '): ' + ISNULL(@SellerEmail, 'NENHUM');

        IF @SellerEmail IS NULL
        BEGIN
            PRINT N'AVISO: E-mail NÃO ENCONTRADO para o Vendedor ID: ' + ISNULL(CAST(@CurrentSellerId AS NVARCHAR(MAX)), 'NULO') + ' (SellerCode: ' + ISNULL(@CurrentSellerCode, 'NULO') + '). Pulando a geração de e-mail para SubmittedAnswersId: ' + CAST(@SubmittedAnswersId AS NVARCHAR(MAX));
            FETCH NEXT FROM cur_submitted_questionnaires INTO
                @SubmittedAnswersId, @CurrentSellerId, @CurrentSellerCode, @CurrentDtSurvey, @CurrentSurveyType;
            CONTINUE;
        END;

        SET @QuestionnaireDateFormatted = FORMAT(@CurrentDtSurvey, 'dd/MM/yyyy');
        SET @SurveyTypeDisplayName = CASE @CurrentSurveyType
                                        WHEN 'TREINAMENTO_CAMPO' THEN 'Treinamento em Campo'
                                        WHEN 'CHECK_ROTA' THEN 'Check de Rota'
                                        ELSE ISNULL(@CurrentSurveyType, 'Tipo Desconhecido')
                                     END;
        
        SELECT  @CustomerCode = CustomerCode
        	FROM SubmittedAnswers
        	WHERE SubmittedAnswersId = @SubmittedAnswersId
        	
        SET @EmailSubject = N'Resumo do Questionário ' + @SurveyTypeDisplayName + ' - ' + @QuestionnaireDateFormatted + ' (' + ISNULL(@CurrentSellerCode, 'Vendedor Desconhecido') + ')';
        SET @EmailBody = N'Prezado(a) ' + ISNULL(@SellerName, 'Vendedor(a)') + N',<br><br>'
                        + N'Esperamos que se encontre bem!<br>'
                        + N'Este é o resumo das suas respostas para o questionário "' + @SurveyTypeDisplayName + '" realizado em ' + @QuestionnaireDateFormatted + N' juntamente com seu Supervisor.<br>'
                        + N'Cliente: ' + @CustomerCode 
                        + N'<br>---<br>';

        SET @QuestionDetailText = (
            SELECT
                STUFF((
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
                    FROM
                        dbo.SubmittedAnswerDetails SAD
                    JOIN
                        dbo.Question Q ON TRY_CAST(SAD.QuestionId AS INT) = Q.QuestionId
                    OUTER APPLY (
                        SELECT TOP 1 qo.Description
                        FROM dbo.QuestionOption qo
                        WHERE qo.QuestionId = TRY_CAST(SAD.QuestionId AS INT)
                          AND qo.OptionCd = SAD.AlternativeId
                          AND Q.AnswerTypeCd = 'SGL'
                    ) AS SGL_Desc
                    OUTER APPLY (
                        SELECT STUFF((
                            SELECT N', ' + qo.Description
                            FROM dbo.QuestionOption qo
                            JOIN OPENJSON(ISNULL(SAD.SelectedMultipleAlternativesIdsJson, '[]')) WITH (id VARCHAR(100) '$.id') AS selected_ids ON qo.OptionCd = selected_ids.id
                            WHERE qo.QuestionId = TRY_CAST(SAD.QuestionId AS INT)
                              AND Q.AnswerTypeCd = 'MLT'
                            ORDER BY qo.Description
                            FOR XML PATH(''), TYPE).value('.', 'NVARCHAR(MAX)'), 1, 2, '') AS AggregatedDescriptions
                    ) AS MLT_Desc
                    OUTER APPLY ( -- Para formatar o conteúdo do TimeLineJson com links para Blob Storage
                        SELECT STUFF((
                            SELECT N'<br>' +
                                CASE
                                    WHEN timeline_entry.type = 'text' THEN N'  - Texto: ' + timeline_entry.text
                                    WHEN timeline_entry.type = 'attachment' THEN
                                        CONCAT(
                                            N'  - Anexo: <a href="',
                                            @AzureBlobBaseUrl,
                                            attachment_detail.filename,
                                            N'" >', -- Adicionado  para abrir em nova aba
                                            ISNULL(attachment_detail.filename, N'Arquivo de Anexo'),
                                            N'</a>'
                                        )
                                    ELSE N'  - Entrada desconhecida na linha do tempo'
                                END
                            FROM
                                OPENJSON(SAD.TimeLineJson)
                                WITH (
                                    id VARCHAR(36),
                                    type VARCHAR(50),
                                    timestamp BIGINT,
                                    text NVARCHAR(MAX),
                                    attachment NVARCHAR(MAX) AS JSON
                                ) AS timeline_entry
                            OUTER APPLY OPENJSON(timeline_entry.attachment)
                                WITH (
                                    id VARCHAR(36),
                                    type VARCHAR(50),
                                    filename NVARCHAR(255),
                                    filepath NVARCHAR(MAX)
                                ) AS attachment_detail
                            WHERE
                                SAD.TimeLineJson IS NOT NULL AND SAD.TimeLineJson <> '[]'
                            ORDER BY timeline_entry.timestamp
                            FOR XML PATH(''), TYPE).value('.', 'NVARCHAR(MAX)'), 1, LEN(N'<br>'), '') AS FormattedTimeline
                    ) AS TimelineFormatted
                    WHERE
                        SAD.SubmittedAnswersId = @SubmittedAnswersId
                    ORDER BY Q.Rank
                    FOR XML PATH(''), TYPE).value('.', 'NVARCHAR(MAX)'), 1, LEN(N'<br><br>'), '')
        );


        SET @EmailBody = @EmailBody + @QuestionDetailText + N'<br><br>---<br><br>'
                        + N'Sua colaboração é fundamental para o nosso aprimoramento contínuo. Agradecemos a sua dedicação e contribuição!<br>'
                        + N'Atenciosamente,<br>'
                        + N'Equipe Supervision 360';

        INSERT INTO dbo.MsgEmailToSend (
            DtCreated,
            MsgType,
            SellerCode,
            SubmittedAnswersId,
            FupId,
            [From],
            [To],
            Subject,
            Body,
            DtToSend,
            Status,
            DhUpdate,
            DtStatus
        )
        VALUES (
            GETDATE(),
            'QuestionnaireFeedback',
            @CurrentSellerCode,
            @SubmittedAnswersId,
            NULL,
            @FromEmail,
            @SellerEmail,
            @EmailSubject,
            @EmailBody,
            GETDATE(),
            'Pending',
            GETDATE(),
            GETDATE()
        );
        PRINT 'DEBUG: E-mail de Questionário INSERIDO para SubmittedAnswersId: ' + ISNULL(CAST(@SubmittedAnswersId AS NVARCHAR(MAX)), 'NULO') + ' para o destinatário: ' + ISNULL(@SellerEmail, 'NULO');

        FETCH NEXT FROM cur_submitted_questionnaires INTO
            @SubmittedAnswersId, @CurrentSellerId, @CurrentSellerCode, @CurrentDtSurvey, @CurrentSurveyType;
    END;

    CLOSE cur_submitted_questionnaires;
    DEALLOCATE cur_submitted_questionnaires;
    PRINT 'DEBUG: Processamento de Questionários finalizado.';


    -- =========================================================================
    -- INÍCIO DA LÓGICA DE GERAÇÃO DE E-MAILS PARA PENDÊNCIAS (FUP)
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
        PRINT 'DEBUG: Processando FUPId: ' + ISNULL(CAST(@FupId AS NVARCHAR(MAX)), 'NULO') + ', FupSellerCode_FUP (VARCHAR): [' + ISNULL(@FupSellerCode_FUP, 'NULO_VARCHAR') + '], Motivo: ' + ISNULL(@EmailReason, 'NULO');

        SET @FupResponsibleEmail = NULL;
        SET @FupResponsibleName = NULL;
        SET @LookupUserId = NULL; -- Reset para cada iteração

        -- ÚNICA TENTATIVA: Mapear Fup.sellerCode como CompanyCodeUser através de DeptUser
        PRINT 'DEBUG: Tentando buscar email via DeptUser.CompanyCodeUser para FupSellerCode_FUP [' + ISNULL(@FupSellerCode_FUP, 'NULO') + ']';
        SELECT TOP 1 @LookupUserId = du.UserId
        FROM dbo.DeptUser du
        WHERE du.CompanyCodeUser = @FupSellerCode_FUP;

        IF @LookupUserId IS NOT NULL
        BEGIN
            SELECT TOP 1 @FupResponsibleEmail = U.Email, @FupResponsibleName = U.Name
            FROM dbo.[User] U
            WHERE U.UserId = @LookupUserId;
            PRINT 'DEBUG: Email encontrado (via DeptUser.CompanyCodeUser) para UserId [' + CAST(@LookupUserId AS NVARCHAR(MAX)) + ']: ' + ISNULL(@FupResponsibleEmail, 'NENHUM');
        END
        ELSE
        BEGIN
            PRINT 'DEBUG: Lookup de UserId via DeptUser.CompanyCodeUser falhou para [' + ISNULL(@FupSellerCode_FUP, 'NULO') + '].';
        END;
        
        IF @FupResponsibleEmail IS NULL
        BEGIN
            PRINT N'AVISO: E-mail NÃO ENCONTRADO para o Vendedor (FUP) com CompanyCodeUser: [' + ISNULL(@FupSellerCode_FUP, 'NULO') + ']. Pulando a geração de e-mail para FupId: ' + ISNULL(CAST(@FupId AS NVARCHAR(MAX)), 'NULO');
            FETCH NEXT FROM cur_fup_reminders INTO
                @FupId, @FupDescription, @FupDtExpectedConclusion, @FupPriority, @FupStatus,
                @FupCustomerCode, @FupSellerCode_FUP, @FupReminderDate, @FupDtInclusion, @FupDtConclusion,
                @FupCategory, @FupPostponementCount, @FupQuestion, @FupLog,
                @EmailReason, @IsWeekendAdjusted;
            CONTINUE;
        END;

        SET @FupDtExpectedConclusionFormatted = ISNULL(FORMAT(@FupDtExpectedConclusion, 'dd/MM/yyyy'), 'Não Definida');

        SET @FupSubject_FUP = N'[Lembrete de Pendência - ' + ISNULL(@FupPriority, 'Normal') + N'] ' + LEFT(ISNULL(@FupDescription, 'Pendência sem descrição'), 70) + N' (Motivo: ' + ISNULL(@EmailReason, 'N/A') + N')';
        IF LEN(ISNULL(@FupDescription, '')) > 70 SET @FupSubject_FUP = @FupSubject_FUP + N'...';

        SET @FupBody_FUP = N'Prezado(a) ' + ISNULL(@FupResponsibleName, @FupSellerCode_FUP) + N',<br><br>'
                      + N'Este é um lembrete sobre uma pendência em aberto sob sua responsabilidade <br>Motivo do E-mail: <b>' + ISNULL(@EmailReason, 'N/A') + N'</b><br>'
                      + CASE WHEN @IsWeekendAdjusted = 1 THEN N' Data de envio antecipada devido ao fim de semana.' ELSE N'.' END
                      + N'<br><br>'
                      + N'<b>ID da Pendência:</b> ' + ISNULL(CAST(@FupId AS NVARCHAR(MAX)), 'N/A') + N'<br>'
                      + N'<b>Vendedor Responsável:</b> ' + ISNULL(@FupSellerCode_FUP, 'N/A') + N'<br>'
                      + N'<b>Descrição:</b> ' + ISNULL(@FupDescription, 'N/A') + N'<br>'
                      + N'<b>Categoria:</b> ' + ISNULL(@FupCategory, 'N/A') + N'<br>'
                      + N'<b>Cliente:</b> ' + ISNULL(@FupCustomerCode, 'N/A') + N'<br>'
                      + N'<b>Conclusão Prevista:</b> ' + @FupDtExpectedConclusionFormatted + N'<br>'
                      + N'<b>Prioridade:</b> ' + ISNULL(@FupPriority, 'N/A') + N'<br>'
                      + N'<b>Status Atual:</b> ' + ISNULL(@FupStatus, 'N/A') + N'<br>'
                      + N'<b>Qtd. de Postergações:</b> ' + ISNULL(CAST(@FupPostponementCount AS NVARCHAR(MAX)), '0') + N'<br>'
                      + N'<b>Questão/Item da Pendência:</b> ' + ISNULL(@FupQuestion, 'N/A') + N'<br>'
                      + N'<b>Histórico:</b> <pre>' + ISNULL(@FupLog, 'N/A') + N'</pre><br><br>'
                      + N'Por favor, tome as ações necessárias para resolver esta pendência.<br>'
                      + N'Atenciosamente,<br>'
                      + N'Equipe Supervision 360';

        INSERT INTO dbo.MsgEmailToSend (
            DtCreated,
            MsgType,
            SellerCode, -- Aqui conterá o CompanyCodeUser da FUP
            SubmittedAnswersId,
            FupId,
            [From],
            [To],
            Subject,
            Body,
            DtToSend,
            Status,
            DhUpdate,
            DtStatus
        )
        VALUES (
            GETDATE(),
            'FUPReminder',
            @FupSellerCode_FUP,
            NULL,
            @FupId,
            @FromEmail,
            @FupResponsibleEmail,
            @FupSubject_FUP,
            @FupBody_FUP,
            GETDATE(),
            'Pending',
            GETDATE(),
            GETDATE()
        );
        PRINT N'DEBUG: E-mail de Lembrete FUP INSERIDO para FupId: ' + ISNULL(CAST(@FupId AS NVARCHAR(MAX)), 'NULO') + N' para o destinatário: ' + ISNULL(@FupResponsibleEmail, 'NULO') + N' (Motivo: ' + ISNULL(@EmailReason, 'NULO') + N')';

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
    -- INÍCIO DA LÓGICA DE GERAÇÃO DE E-MAILS PARA NOTIFICAÇÃO DE CONCLUSÃO DE FUP (Caso 4)
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
        PRINT 'DEBUG: Processando FUP Concluída FupId: ' + ISNULL(CAST(@FupId AS NVARCHAR(MAX)), 'NULO') + ', FupSellerCode_FUP (VARCHAR): [' + ISNULL(@FupSellerCode_FUP, 'NULO_VARCHAR') + ']';

        SET @FupResponsibleEmail = NULL;
        SET @FupResponsibleName = NULL;
        SET @LookupUserId = NULL; -- Reset para cada iteração

        -- ÚNICA TENTATIVA: Mapear Fup.sellerCode como CompanyCodeUser através de DeptUser
        PRINT 'DEBUG: Tentando buscar email via DeptUser.CompanyCodeUser para [' + ISNULL(@FupSellerCode_FUP, 'NULO') + ']';
        SELECT TOP 1 @LookupUserId = du.UserId
        FROM dbo.DeptUser du
        WHERE du.CompanyCodeUser = @FupSellerCode_FUP;

        IF @LookupUserId IS NOT NULL
        BEGIN
            SELECT TOP 1 @FupResponsibleEmail = U.Email, @FupResponsibleName = U.Name
            FROM dbo.[User] U
            WHERE U.UserId = @LookupUserId;
            PRINT 'DEBUG: Email encontrado (via DeptUser.CompanyCodeUser) para UserId [' + CAST(@LookupUserId AS NVARCHAR(MAX)) + ']: ' + ISNULL(@FupResponsibleEmail, 'NENHUM');
        END
        ELSE
        BEGIN
            PRINT 'DEBUG: Lookup de UserId via DeptUser.CompanyCodeUser falhou para [' + ISNULL(@FupSellerCode_FUP, 'NULO') + '].';
        END;

        IF @FupResponsibleEmail IS NULL
        BEGIN
            PRINT N'AVISO: E-mail NÃO ENCONTRADO para o Vendedor (FUP Concluída) com CompanyCodeUser: [' + ISNULL(@FupSellerCode_FUP, 'NULO') + ']. Pulando a geração de e-mail para FupId: ' + ISNULL(CAST(@FupId AS NVARCHAR(MAX)), 'NULO');
            FETCH NEXT FROM cur_fup_completions INTO
                @FupId, @FupDescription, @FupDtExpectedConclusion, @FupPriority, @FupStatus,
                @FupCustomerCode, @FupSellerCode_FUP, @FupDtConclusion,
                @FupCategory, @FupPostponementCount, @FupQuestion, @FupLog,
                @EmailReason, @IsWeekendAdjusted;
            CONTINUE;
        END;

        SET @FupDtConclusionFormatted = ISNULL(FORMAT(@FupDtConclusion, 'dd/MM/yyyy'), 'Não Definida');
        SET @FupSubject_FUP = N'[FUP Concluída - ' + ISNULL(@FupCustomerCode, 'Cliente') + N'] ' + LEFT(ISNULL(@FupDescription, 'Pendência sem descrição'), 70);
        IF LEN(ISNULL(@FupDescription, '')) > 70 SET @FupSubject_FUP = @FupSubject_FUP + N'...';

        SET @FupBody_FUP = N'Prezado(a) ' + ISNULL(@FupResponsibleName, @FupSellerCode_FUP) + N',<br><br>'
                      + N'Gostaríamos de informar que a seguinte pendência foi marcada como concluída em <b>' + FORMAT(@CurrentDate, 'dd/MM/yyyy') + N'</b>'
                      + CASE WHEN @IsWeekendAdjusted = 1 THEN N' (Data de conclusão antecipada devido ao fim de semana).' ELSE N'.' END
                      + N'<br><br>'
                      + N'<b>ID da Pendência:</b> ' + ISNULL(CAST(@FupId AS NVARCHAR(MAX)), 'N/A') + N'<br>'
                      + N'<b>Descrição:</b> ' + ISNULL(@FupDescription, 'N/A') + N'<br>'
                      + N'<b>Categoria:</b> ' + ISNULL(@FupCategory, 'N/A') + N'<br>'
                      + N'<b>Cliente:</b> ' + ISNULL(@FupCustomerCode, 'N/A') + N'<br>'
                      + N'<b>Vendedor Responsável (Cód. Empresa):</b> ' + ISNULL(@FupSellerCode_FUP, 'N/A') + N'<br>'
                      + N'<b>Concluída em:</b> ' + @FupDtConclusionFormatted + N'<br>'
                      + N'<b>Prioridade:</b> ' + ISNULL(@FupPriority, 'N/A') + N'<br>'
                      + N'<b>Status:</b> ' + ISNULL(@FupStatus, 'N/A') + N'<br>'
                      + N'<b>Qtd. de Postergações:</b> ' + ISNULL(CAST(@FupPostponementCount AS NVARCHAR(MAX)), '0') + N'<br>'
                      + N'<b>Questão/Item da Pendência:</b> ' + ISNULL(@FupQuestion, 'N/A') + N'<br>'
                      + N'<b>Histórico/Log:</b> <pre>' + ISNULL(@FupLog, 'N/A') + N'</pre><br><br>'
                      + N'Atenciosamente,<br>'
                      + N'Equipe Supervision 360';

        INSERT INTO dbo.MsgEmailToSend (
            DtCreated,
            MsgType,
            SellerCode, -- Aqui conterá o identificador (CompanyCodeUser) da FUP
            SubmittedAnswersId,
            FupId,
            [From],
            [To],
            Subject,
            Body,
            DtToSend,
            Status,
            DhUpdate,
            DtStatus
        )
        VALUES (
            GETDATE(),
            'FUPCompletion',
            @FupSellerCode_FUP,
            NULL,
            @FupId,
            @FromEmail,
            @FupResponsibleEmail,
            @FupSubject_FUP,
            @FupBody_FUP,
            GETDATE(),
            'Pending',
            GETDATE(),
            GETDATE()
        );
        PRINT N'DEBUG: E-mail de Conclusão FUP INSERIDO para FupId: ' + ISNULL(CAST(@FupId AS NVARCHAR(MAX)), 'NULO') + N' para o destinatário: ' + ISNULL(@FupResponsibleEmail, 'NULO');

        FETCH NEXT FROM cur_fup_completions INTO
            @FupId, @FupDescription, @FupDtExpectedConclusion, @FupPriority, @FupStatus,
            @FupCustomerCode, @FupSellerCode_FUP, @FupDtConclusion,
            @FupCategory, @FupPostponementCount, @FupQuestion, @FupLog,
            @EmailReason, @IsWeekendAdjusted;
    END;

    CLOSE cur_fup_completions;
    DEALLOCATE cur_fup_completions;
    PRINT 'DEBUG: Processamento de Notificação de Conclusão de FUP finalizado.';

    PRINT 'DEBUG: Fim da execução da procedure dbo.GenerateSellerQuestionnaireEmails.';
END;