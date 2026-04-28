-- ============================================================
-- Procedure CRUD unificada para DataImportSallersLog
-- ============================================================

IF OBJECT_ID('SP_Adm_DataImportSallersLog', 'P') IS NOT NULL DROP PROCEDURE SP_Adm_DataImportSallersLog;
GO
CREATE PROCEDURE SP_Adm_DataImportSallersLog
    @TypeRequest             VARCHAR(10)     NOT NULL,  -- 'SELECT'|'UPDATE'|'DELETE'
    @DataImportSallersLogId  INT             = NULL,
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
        SELECT DataImportSallersLogId, FileName, Status, TotalRows, ProcessedRows, ErrorRows, UserId, ErrorMessage, DhCreate, DhUpdate
        FROM DataImportSallersLog
        WHERE (@DataImportSallersLogId IS NULL OR DataImportSallersLogId = @DataImportSallersLogId)
          AND (@Status IS NULL OR Status = @Status)
          AND (@UserId IS NULL OR UserId = @UserId)
        ORDER BY DhCreate DESC;
    END
    ELSE IF @TypeRequest = 'UPDATE'
    BEGIN
        UPDATE DataImportSallersLog SET
            Status        = ISNULL(@Status, Status),
            TotalRows     = ISNULL(@TotalRows, TotalRows),
            ProcessedRows = ISNULL(@ProcessedRows, ProcessedRows),
            ErrorRows     = ISNULL(@ErrorRows, ErrorRows),
            ErrorMessage  = ISNULL(@ErrorMessage, ErrorMessage),
            DhUpdate      = GETDATE()
        WHERE DataImportSallersLogId = @DataImportSallersLogId;
    END
    ELSE IF @TypeRequest = 'DELETE'
    BEGIN
        DELETE FROM DataImportSallersRow  WHERE DataImportSallersLogId = @DataImportSallersLogId;
        DELETE FROM DataImportSallersLog  WHERE DataImportSallersLogId = @DataImportSallersLogId;
    END
END
GO
GRANT EXECUTE ON SP_Adm_DataImportSallersLog TO S360sys;
GO

-- ============================================================
-- Procedures específicas do fluxo de importação
-- ============================================================

-- Cria o registro de log e retorna o ID
CREATE OR ALTER PROCEDURE sp_DataImportSallersLog_Create
  @FileName  VARCHAR(200),
  @UserId    INT = NULL
AS BEGIN
  SET NOCOUNT ON;
  INSERT INTO DataImportSallersLog (FileName, Status, UserId, DhCreate)
  VALUES (@FileName, 'PROCESSING', @UserId, GETDATE());
  SELECT SCOPE_IDENTITY() AS DataImportSallersLogId;
END
GO
GRANT EXECUTE ON sp_DataImportSallersLog_Create TO S360sys;
GO

-- Insere uma linha do Tabelao
CREATE OR ALTER PROCEDURE sp_DataImportSallersRow_Insert
  @DataImportSallersLogId  INT,
  @ID               VARCHAR(50)  = NULL,
  @CodCliente       VARCHAR(50)  = NULL,
  @NomeFantasia     VARCHAR(200) = NULL,
  @CNPJ             VARCHAR(20)  = NULL,
  @CodProfissional  VARCHAR(50)  = NULL,
  @Email            VARCHAR(200) = NULL,
  @Nome             VARCHAR(200) = NULL,
  @Celular          VARCHAR(20)  = NULL,
  @Whats            VARCHAR(20)  = NULL,
  @CodEquipe        VARCHAR(50)  = NULL,
  @Vendedor         BIT          = NULL,
  @CodSuperior      VARCHAR(50)  = NULL
AS BEGIN
  SET NOCOUNT ON;
  INSERT INTO DataImportSallersRow
    (DataImportSallersLogId, ID, CodCliente, NomeFantasia, CNPJ, CodProfissional,
     Email, Nome, Celular, Whats, CodEquipe, Vendedor, CodSuperior, Status, DhCreate)
  VALUES
    (@DataImportSallersLogId, @ID, @CodCliente, @NomeFantasia, @CNPJ, @CodProfissional,
     @Email, @Nome, @Celular, @Whats, @CodEquipe, @Vendedor, @CodSuperior, 'PENDING', GETDATE());
END
GO
GRANT EXECUTE ON sp_DataImportSallersRow_Insert TO S360sys;
GO

-- Finaliza a importação com os totais
CREATE OR ALTER PROCEDURE sp_DataImportSallersLog_Finalize
  @DataImportSallersLogId  INT,
  @TotalRows               INT,
  @ProcessedRows           INT,
  @ErrorRows               INT
AS BEGIN
  SET NOCOUNT ON;
  UPDATE DataImportSallersLog SET
    TotalRows     = @TotalRows,
    ProcessedRows = @ProcessedRows,
    ErrorRows     = @ErrorRows,
    Status        = 'COMPLETED',
    DhUpdate      = GETDATE()
  WHERE DataImportSallersLogId = @DataImportSallersLogId;
END
GO
GRANT EXECUTE ON sp_DataImportSallersLog_Finalize TO S360sys;
GO
