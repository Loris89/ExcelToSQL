CREATE TYPE [dbo].[Docs] AS TABLE(
  IdDoc nvarchar(100),
  Mandatory bit,
  SubstituteId nvarchar(100)
)

GO
CREATE PROCEDURE test
	@docs Docs READONLY,
	@productId UNIQUEIDENTIFIER = 'A4D90C16-2530-4ACB-96C0-920DCB1C969D',
	@legalNature NVARCHAR(100) = N'PG',
	@role NVARCHAR(100) = NULL,
	@rowNum NVARCHAR(100) = NULL,
	@legalForm NVARCHAR(100) = NULL,
	@ciae NVARCHAR(100) = NULL,
	@sae NVARCHAR(100) = NULL,
	@idDocList NVARCHAR(100) = NULL	
AS
BEGIN

DECLARE 
@sql NVARCHAR(MAX)

--SELECT * FROM @docs Per test da console app per il passaggio dei documenti da considerare

set @sql = 
'
	IF OBJECT_ID(''DBFEUNICO..#tmp_result'') IS NOT NULL DROP TABLE #tmp_result

	SELECT * 
	into #tmp_tab1
	FROM TBL_Stub_AcquisitionDocumentItem

	SELECT ''ciao1'' as query
	into #tmp_result

	INSERT INTO #tmp_result(query)
	SELECT ''ciao2''

	SELECT * FROM #tmp_result
'

exec sp_sqlexec @sql

END