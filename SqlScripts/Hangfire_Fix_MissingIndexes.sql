-- ============================================================
-- Fix: índices ausentes no schema HangFire que causam o erro
--   "Query processor could not produce a query plan because of
--    the hints defined in this query" no ExpirationManager.
--
-- O ExpirationManager usa FORCESEEK em ExpireAt para limpar
-- registros expirados. Sem o índice, o SQL Server rejeita o plano.
--
-- Execute este script no banco onde o schema já foi criado.
-- ============================================================

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE object_id = OBJECT_ID(N'[HangFire].[Hash]')
      AND name = 'IX_HangFire_Hash_ExpireAt'
)
BEGIN
    CREATE NONCLUSTERED INDEX [IX_HangFire_Hash_ExpireAt]
        ON [HangFire].[Hash] ([ExpireAt] ASC);
    PRINT 'Índice IX_HangFire_Hash_ExpireAt criado.';
END
ELSE
    PRINT 'Índice IX_HangFire_Hash_ExpireAt já existe.';
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE object_id = OBJECT_ID(N'[HangFire].[List]')
      AND name = 'IX_HangFire_List_ExpireAt'
)
BEGIN
    CREATE NONCLUSTERED INDEX [IX_HangFire_List_ExpireAt]
        ON [HangFire].[List] ([ExpireAt] ASC);
    PRINT 'Índice IX_HangFire_List_ExpireAt criado.';
END
ELSE
    PRINT 'Índice IX_HangFire_List_ExpireAt já existe.';
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE object_id = OBJECT_ID(N'[HangFire].[Counter]')
      AND name = 'IX_HangFire_Counter_ExpireAt'
)
BEGIN
    CREATE NONCLUSTERED INDEX [IX_HangFire_Counter_ExpireAt]
        ON [HangFire].[Counter] ([ExpireAt] ASC);
    PRINT 'Índice IX_HangFire_Counter_ExpireAt criado.';
END
ELSE
    PRINT 'Índice IX_HangFire_Counter_ExpireAt já existe.';
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE object_id = OBJECT_ID(N'[HangFire].[AggregatedCounter]')
      AND name = 'IX_HangFire_AggregatedCounter_ExpireAt'
)
BEGIN
    CREATE NONCLUSTERED INDEX [IX_HangFire_AggregatedCounter_ExpireAt]
        ON [HangFire].[AggregatedCounter] ([ExpireAt] ASC);
    PRINT 'Índice IX_HangFire_AggregatedCounter_ExpireAt criado.';
END
ELSE
    PRINT 'Índice IX_HangFire_AggregatedCounter_ExpireAt já existe.';
GO

PRINT '=== Fix de índices HangFire concluído. ===';
GO
