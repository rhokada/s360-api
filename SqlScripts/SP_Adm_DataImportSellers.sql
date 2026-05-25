-- ============================================================
-- Procedure CRUD unificada para DataImportSellersLog
-- ============================================================

IF OBJECT_ID('SP_Adm_DataImportSellersLog', 'P') IS NOT NULL DROP PROCEDURE SP_Adm_DataImportSellersLog;
GO
CREATE PROCEDURE SP_Adm_DataImportSellersLog
    @TypeRequest             VARCHAR(10)    ,  -- 'SELECT'|'UPDATE'|'DELETE'
    @DataImportSellersLogId  INT             = NULL,
    @FileName                VARCHAR(200)    = NULL,
    @Status                  VARCHAR(20)     = NULL,
    @TotalRows               INT             = NULL,
    @ProcessedRows           INT             = NULL,
    @ErrorRows               INT             = NULL,
    @UserId                  INT             = NULL,
    @ErrorMessage            NVARCHAR(MAX)   = NULL,
    @token_usuario           NVARCHAR(MAX)   = NULL
AS
BEGIN
    SET NOCOUNT ON;
    IF @TypeRequest = 'SELECT'
    BEGIN
        SELECT DataImportSellersLogId, FileName, Status, TotalRows, ProcessedRows, ErrorRows, UserId, ErrorMessage, DhCreate, DhUpdate
        FROM DataImportSellersLog
        WHERE (@DataImportSellersLogId IS NULL OR DataImportSellersLogId = @DataImportSellersLogId)
          AND (@Status IS NULL OR Status = @Status)
          AND (@UserId IS NULL OR UserId = @UserId)
        ORDER BY DhCreate DESC;
    END
    ELSE IF @TypeRequest = 'UPDATE'
    BEGIN
        UPDATE DataImportSellersLog SET
            Status        = ISNULL(@Status, Status),
            TotalRows     = ISNULL(@TotalRows, TotalRows),
            ProcessedRows = ISNULL(@ProcessedRows, ProcessedRows),
            ErrorRows     = ISNULL(@ErrorRows, ErrorRows),
            ErrorMessage  = ISNULL(@ErrorMessage, ErrorMessage),
            DhUpdate      = GETDATE()
        WHERE DataImportSellersLogId = @DataImportSellersLogId;
    END
    ELSE IF @TypeRequest = 'DELETE'
    BEGIN
        DELETE FROM DataImportSellersRow  WHERE DataImportSellersLogId = @DataImportSellersLogId;
        DELETE FROM DataImportSellersLog  WHERE DataImportSellersLogId = @DataImportSellersLogId;
    END
END
GO
GRANT EXECUTE ON SP_Adm_DataImportSellersLog TO S360sys;
GO

-- ============================================================
-- Procedures específicas do fluxo de importação
-- ============================================================

-- Cria o registro de log e retorna o ID
CREATE OR ALTER PROCEDURE sp_DataImportSellersLog_Create
  @FileName  VARCHAR(200),
  @UserId    INT = NULL
AS BEGIN
  SET NOCOUNT ON;
  INSERT INTO DataImportSellersLog (FileName, Status, UserId, DhCreate)
  VALUES (@FileName, 'PROCESSING', @UserId, GETDATE());
  SELECT SCOPE_IDENTITY() AS DataImportSellersLogId;
END
GO
GRANT EXECUTE ON sp_DataImportSellersLog_Create TO S360sys;
GO

-- Insere uma linha do Tabelao
CREATE OR ALTER PROCEDURE sp_DataImportSellersRow_Insert
  @DataImportSellersLogId  INT,
  @ID                  VARCHAR(50)  = NULL,
  @CodCliente          VARCHAR(50)  = NULL,
  @NomeFantasia        VARCHAR(200) = NULL,
  @CNPJ                VARCHAR(20)  = NULL,
  @CodProfissional     VARCHAR(50)  = NULL,
  @Email               VARCHAR(200) = NULL,
  @Nome                VARCHAR(200) = NULL,
  @Celular             VARCHAR(20)  = NULL,
  @CodEquipe           VARCHAR(50)  = NULL,
  @Vendedor            BIT          = NULL,
  @CodSuperior         VARCHAR(50)  = NULL,
  @NomeSupervisor      VARCHAR(200) = NULL,
  @TelefoneSupervisor  VARCHAR(20)  = NULL,
  @EmailSupervisor     VARCHAR(200) = NULL
AS BEGIN
  SET NOCOUNT ON;
  INSERT INTO DataImportSellersRow
    (DataImportSellersLogId, ID, CodCliente, NomeFantasia, CNPJ, CodProfissional,
     Email, Nome, Celular, CodEquipe, Vendedor, CodSuperior,
     NomeSupervisor, TelefoneSupervisor, EmailSupervisor, Status, DhCreate)
  VALUES
    (@DataImportSellersLogId, @ID, @CodCliente, @NomeFantasia, @CNPJ, @CodProfissional,
     @Email, @Nome, @Celular, @CodEquipe, @Vendedor, @CodSuperior,
     @NomeSupervisor, @TelefoneSupervisor, @EmailSupervisor, 'PENDING', GETDATE());
END
GO
GRANT EXECUTE ON sp_DataImportSellersRow_Insert TO S360sys;
GO

-- Finaliza a importação com os totais
CREATE OR ALTER PROCEDURE sp_DataImportSellersLog_Finalize
  @DataImportSellersLogId  INT,
  @TotalRows               INT,
  @ProcessedRows           INT,
  @ErrorRows               INT
AS BEGIN
  SET NOCOUNT ON;
  UPDATE DataImportSellersLog SET
    TotalRows     = @TotalRows,
    ProcessedRows = @ProcessedRows,
    ErrorRows     = @ErrorRows,
    Status        = 'COMPLETED',
    DhUpdate      = GETDATE()
  WHERE DataImportSellersLogId = @DataImportSellersLogId;
END
GO
GRANT EXECUTE ON sp_DataImportSellersLog_Finalize TO S360sys;
GO
