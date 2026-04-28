-- ============================================================
-- Procedure CRUD unificada para DataImportProLog
-- ============================================================

IF OBJECT_ID('SP_Adm_DataImportProLog', 'P') IS NOT NULL DROP PROCEDURE SP_Adm_DataImportProLog;
GO
CREATE PROCEDURE SP_Adm_DataImportProLog
    @TypeRequest        VARCHAR(10)     NOT NULL,  -- 'SELECT'|'INSERT'|'UPDATE'|'DELETE'
    @DataImportProLogId INT             = NULL,
    @FileName           VARCHAR(200)    = NULL,
    @FileType           VARCHAR(20)     = NULL,
    @Status             VARCHAR(20)     = NULL,
    @TotalRows          INT             = NULL,
    @ProcessedRows      INT             = NULL,
    @ErrorRows          INT             = NULL,
    @UserId             INT             = NULL,
    @ErrorMessage       NVARCHAR(MAX)   = NULL,
    @token_usuario      NVARCHAR(MAX)   = NULL
AS
BEGIN
    SET NOCOUNT ON;
    IF @TypeRequest = 'SELECT'
    BEGIN
        SELECT DataImportProLogId, FileName, FileType, Status, TotalRows, ProcessedRows, ErrorRows, UserId, ErrorMessage, DhCreate, DhUpdate
        FROM DataImportProLog
        WHERE (@DataImportProLogId IS NULL OR DataImportProLogId = @DataImportProLogId)
          AND (@Status IS NULL OR Status = @Status)
          AND (@UserId IS NULL OR UserId = @UserId)
        ORDER BY DhCreate DESC;
    END
    ELSE IF @TypeRequest = 'INSERT'
    BEGIN
        INSERT INTO DataImportProLog (FileName, FileType, Status, UserId, DhCreate)
        VALUES (@FileName, ISNULL(@FileType, 'AMBOS'), 'PROCESSING', @UserId, GETDATE());
        SELECT SCOPE_IDENTITY() AS DataImportProLogId;
    END
    ELSE IF @TypeRequest = 'UPDATE'
    BEGIN
        UPDATE DataImportProLog SET
            Status        = ISNULL(@Status, Status),
            TotalRows     = ISNULL(@TotalRows, TotalRows),
            ProcessedRows = ISNULL(@ProcessedRows, ProcessedRows),
            ErrorRows     = ISNULL(@ErrorRows, ErrorRows),
            ErrorMessage  = ISNULL(@ErrorMessage, ErrorMessage),
            DhUpdate      = GETDATE()
        WHERE DataImportProLogId = @DataImportProLogId;
    END
    ELSE IF @TypeRequest = 'DELETE'
    BEGIN
        DELETE FROM DataImportProLog WHERE DataImportProLogId = @DataImportProLogId;
    END
END
GO
GRANT EXECUTE ON SP_Adm_DataImportProLog TO S360sys;
GO

-- ============================================================
-- Procedures específicas do fluxo de importação
-- ============================================================

-- Cria o registro de log e retorna o ID
CREATE OR ALTER PROCEDURE sp_DataImportProLog_Create
  @FileName  VARCHAR(200),
  @FileType  VARCHAR(20) = 'AMBOS',
  @UserId    INT = NULL
AS BEGIN
  SET NOCOUNT ON;
  INSERT INTO DataImportProLog (FileName, FileType, Status, UserId, DhCreate)
  VALUES (@FileName, @FileType, 'PROCESSING', @UserId, GETDATE());
  SELECT SCOPE_IDENTITY() AS DataImportProLogId;
END
GO
GRANT EXECUTE ON sp_DataImportProLog_Create TO S360sys;
GO

-- Insere linha de profissional
CREATE OR ALTER PROCEDURE sp_DataImportProProfissional_Insert
  @DataImportProLogId  INT,
  @ID              VARCHAR(50)  = NULL,
  @CodProfissional VARCHAR(50)  = NULL,
  @Email           VARCHAR(200) = NULL,
  @Nome            VARCHAR(200) = NULL,
  @Celular         VARCHAR(20)  = NULL,
  @Whats           VARCHAR(20)  = NULL,
  @CodEquipe       VARCHAR(50)  = NULL,
  @Vendedor        BIT          = NULL,
  @CodSuperior     VARCHAR(50)  = NULL
AS BEGIN
  SET NOCOUNT ON;
  INSERT INTO DataImportProProfissional
    (DataImportProLogId, ID, CodProfissional, Email, Nome, Celular, Whats, CodEquipe, Vendedor, CodSuperior, Status, DhCreate)
  VALUES
    (@DataImportProLogId, @ID, @CodProfissional, @Email, @Nome, @Celular, @Whats, @CodEquipe, @Vendedor, @CodSuperior, 'PENDING', GETDATE());
END
GO
GRANT EXECUTE ON sp_DataImportProProfissional_Insert TO S360sys;
GO

-- Insere linha de cliente
CREATE OR ALTER PROCEDURE sp_DataImportProCliente_Insert
  @DataImportProLogId INT,
  @ID           VARCHAR(50)  = NULL,
  @CodCliente   VARCHAR(50)  = NULL,
  @NomeFantasia VARCHAR(200) = NULL,
  @CNPJ         VARCHAR(20)  = NULL,
  @CodVendedor  VARCHAR(50)  = NULL
AS BEGIN
  SET NOCOUNT ON;
  INSERT INTO DataImportProCliente
    (DataImportProLogId, ID, CodCliente, NomeFantasia, CNPJ, CodVendedor, Status, DhCreate)
  VALUES
    (@DataImportProLogId, @ID, @CodCliente, @NomeFantasia, @CNPJ, @CodVendedor, 'PENDING', GETDATE());
END
GO
GRANT EXECUTE ON sp_DataImportProCliente_Insert TO S360sys;
GO

-- Finaliza a importação com os totais
CREATE OR ALTER PROCEDURE sp_DataImportProLog_Finalize
  @DataImportProLogId  INT,
  @TotalRows     INT,
  @ProcessedRows INT,
  @ErrorRows     INT
AS BEGIN
  SET NOCOUNT ON;
  UPDATE DataImportProLog SET
    TotalRows     = @TotalRows,
    ProcessedRows = @ProcessedRows,
    ErrorRows     = @ErrorRows,
    Status        = 'COMPLETED',
    DhUpdate      = GETDATE()
  WHERE DataImportProLogId = @DataImportProLogId;
END
GO
GRANT EXECUTE ON sp_DataImportProLog_Finalize TO S360sys;
GO
