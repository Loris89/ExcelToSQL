SET NUMERIC_ROUNDABORT OFF
GO
SET XACT_ABORT, ANSI_PADDING, ANSI_WARNINGS, CONCAT_NULL_YIELDS_NULL, ARITHABORT, QUOTED_IDENTIFIER, ANSI_NULLS ON
GO

BEGIN TRY
BEGIN TRANSACTION

-- AGGIUNTA DELLA NUOVA COLONNA

BEGIN
	ALTER TABLE TBL_Stub_DocumentAcquisitionFilter
	ADD IdDocList VARCHAR

	EXEC(' UPDATE TBL_Stub_DocumentAcquisitionFilter SET idDocList = 1') 
END

-- AGGIORNAMENTO STORED PROCEDURES

-- Penso tocchi wrappare le operazioni "alter" delle stored procedure in degli exec qui dentro

-- INSERIRE QUI GLI SCRIPT SQL GENERATI DAL TOOL

COMMIT TRANSACTION
PRINT 'Script concluso con successo'

END TRY
BEGIN CATCH
PRINT 'Rollback execution...'
SELECT ERROR_MESSAGE() AS ErrorMessage;
ROLLBACK
END CATCH