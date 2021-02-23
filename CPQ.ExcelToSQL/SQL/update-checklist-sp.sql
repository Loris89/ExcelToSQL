USE [DBFEUNICO]
GO

CREATE PROCEDURE updateChecklistFromExcel
(
	@productId UNIQUEIDENTIFIER = 'A4D90C16-2530-4ACB-96C0-920DCB1C969D',
	@legalNature NVARCHAR(100) = N'PG',
	@role NVARCHAR(100) = NULL,
	@rowNum NVARCHAR(100) = NULL,
	@legalForm NVARCHAR(100) = NULL,
	@ciae NVARCHAR(100) = NULL,
	@sae NVARCHAR(100) = NULL,
	@idDocList NVARCHAR(100) = NULL	
)

AS
BEGIN

IF OBJECT_ID('tempdb..#tmpInput1') IS NOT NULL DROP TABLE #tmpInput1

create table #tmpInput1 (docId NVARCHAR(100), mandatory bit);
insert into #tmpInput1 values('18',IIF('f'='o',1,0))
insert into #tmpInput1 values('10',IIF('f'='o',1,0))
insert into #tmpInput1 values('45',IIF('f'='o',1,0))
insert into #tmpInput1 values('8',IIF('f'='o',1,0))

declare @sql NVARCHAR (MAX) -- Contiene la query da eseguire

set @sql = 'IF OBJECT_ID(''tempdb..#tmp_tab1'') IS NOT NULL DROP TABLE #tmp_tab1

select distinct dt.DocumentTypeDescription as [Key],
fil.PK_DocumentAcquisitionFilterId,
map.PK_MapAcquisitionDocumentItemFilterId,
    dt.DocumentName as DocumentName,
	item.DocumentId as DocumentId,
	item.Mandatory as Mandatory,
	item.IsProduct as IsProduct,
	--meta.[Key] as [Metadatakey],
	--meta.[Value] as [Metadatavalue],
	fil.ElectronicArchiving as ElectronicArchiving
	into #tmp_tab1
	from TBL_Stub_DocumentAcquisitionFilter fil
	inner join TBL_Stub_DocumentType dt on fil.FK_DocumentTypeId = dt.PK_DocumentTypeId
	inner join TBL_Stub_MapAcquisitionDocumentItemFilter map on map.FK_DocumentAcquisitionFilterId = fil.PK_DocumentAcquisitionFilterId
	inner join TBL_Stub_AcquisitionDocumentItem item on item.PK_AcquisitionDocumentItemId = map.FK_AcquisitionDocumentItemId 
	left join TBL_Stub_AcquisitionDocumentItemMetadata meta on meta.FK_AcquisitionDocumentItemId = item.PK_AcquisitionDocumentItemId 
	where FK_ProductId = ''' + CONVERT(NVARCHAR(100), @ProductId) + '''' +
	' and LegalNature = ''' + @legalNature  + ''' '

	IF(@legalNature = 'PG')
		BEGIN
		if(@legalForm IS NULL)
			set @sql += ' AND LegalForm IS NULL'
		else
			set @sql += ' AND LegalForm = ''' + @legalForm + ''''

		
		if(@ciae IS NULL)
			set @sql += ' AND CIAE IS NULL'
		else
			set @sql += ' AND CIAE = ''' + @ciae + ''''

		
		if(@sae IS NULL)
			set @sql += ' AND SAE IS NULL'
		else
			set @sql += ' AND SAE = ''' + @sae + ''''
		END
	ELSE IF(@legalNature = 'PF')
		BEGIN
			if(@role IS NULL)
				set @sql += ' AND [Role] IS NULL'
			else
				set @sql += ' AND [Role] = ''' + @role + ''''
		END

		set @sql += '
		declare 
		@PK_MapAcquisitionDocumentItemFilterId uniqueidentifier= null,
		@PK_DocumentAcquisitionFilterId uniqueidentifier = null,
		@DocumentId nvarchar(max) = null,
		@Mandatory	bit =null,
		@PKDocumentItemId uniqueidentifier = null,
		@PKDocumentAcquisition  uniqueidentifier = null

PRINT(''--row '+@rowNum+' ' + @legalForm + '	' + @ciae + '	' + @sae + ' '')

DECLARE db_cursor CURSOR FOR 
SELECT PK_MapAcquisitionDocumentItemFilterId,PK_DocumentAcquisitionFilterId,DocumentId,Mandatory   
FROM  #tmp_tab1

OPEN db_cursor  
FETCH NEXT FROM db_cursor INTO @PK_MapAcquisitionDocumentItemFilterId,@PK_DocumentAcquisitionFilterId,@DocumentId,@Mandatory  

WHILE @@FETCH_STATUS = 0  
BEGIN  

if not exists(select * from #tmpInput1 where trim(@DocumentId) = trim(docId) AND @Mandatory = mandatory)
begin 
--delete 
print(''DELETE FROM TBL_Stub_MapAcquisitionDocumentItemFilter WHERE PK_MapAcquisitionDocumentItemFilterId= '' + char(39) + CONVERT(NVARCHAR(100),@PK_MapAcquisitionDocumentItemFilterId) + char(39))
end

      FETCH NEXT FROM db_cursor INTO @PK_MapAcquisitionDocumentItemFilterId,@PK_DocumentAcquisitionFilterId,@DocumentId,@Mandatory   
END 

CLOSE db_cursor  
DEALLOCATE db_cursor ';	

set @sql += '
	--id tupla
	set @PKDocumentAcquisition = (select top 1 PK_DocumentAcquisitionFilterId from TBL_Stub_DocumentAcquisitionFilter fil
	where 	 LegalForm = ''' + @legalForm + ''' AND CIAE =  ''' + @ciae + ''' AND SAE = ''' + @sae + ''' AND FK_ProductId= ''A4D90C16-2530-4ACB-96C0-920DCB1C969D'' AND LegalNature = N''PG'')

IF(@PKDocumentAcquisition IS NULL)
BEGIN
set @PKDocumentAcquisition=newid();
PRINT(''INSERT INTO [dbo].[TBL_Stub_DocumentAcquisitionFilter]
    ([PK_DocumentAcquisitionFilterId]
    ,[LegalForm]
    ,[LegalNature]
    ,[Role]
    ,[CIAE]
    ,[SAE]
    ,[FK_ProductId]
    ,[FK_DocumentTypeId]
    ,[ElectronicArchiving])
VALUES
    (''+char(39) +CONVERT(NVARCHAR(100),@PKDocumentAcquisition)+char(39) +''
    ,''''' + @legalForm + ''''' 
    ,N''''PG''''
    ,NULL
    ,'''''+ @ciae +'''''
    ,''''' + @sae + '''''
    ,''''A4D90C16-2530-4ACB-96C0-920DCB1C969D''''
    ,''''169760CF-42F6-413E-BDF3-6ABD95670FEF''''
    ,NULL)
'')
END

DECLARE db_cursor CURSOR FOR 
SELECT docId, mandatory  
FROM  #tmpInput1

OPEN db_cursor  
FETCH NEXT FROM db_cursor INTO @DocumentId,@Mandatory  

WHILE @@FETCH_STATUS = 0  
BEGIN  

if not exists(select * from #tmp_tab1 where trim(DocumentId) = trim(@DocumentId) AND Mandatory = @Mandatory)
begin 
--insert doc map
if exists(select * from TBL_Stub_AcquisitionDocumentItem where trim(DocumentId) = trim(@DocumentId) AND Mandatory = @Mandatory)
begin
--get document pk
	set @PKDocumentItemId = ( select top 1 PK_AcquisitionDocumentItemId from TBL_Stub_AcquisitionDocumentItem 
    where trim(DocumentId) = trim(@DocumentId) and Mandatory = @Mandatory)'

set @sql += '
	PRINT(''
IF NOT EXISTS(SELECT 1 FROM TBL_Stub_MapAcquisitionDocumentItemFilter WHERE 
[FK_AcquisitionDocumentItemId] = ''+char(39) +CONVERT(NVARCHAR(100),@PKDocumentItemId)+char(39) +'' AND
[FK_DocumentAcquisitionFilterId] = ''+char(39) +CONVERT(NVARCHAR(100),@PKDocumentAcquisition)+char(39) +''
)
BEGIN
INSERT INTO [dbo].[TBL_Stub_MapAcquisitionDocumentItemFilter]
           ([PK_MapAcquisitionDocumentItemFilterId]
           ,[FK_AcquisitionDocumentItemId]
           ,[FK_DocumentAcquisitionFilterId])
VALUES
    (''+char(39) +CONVERT(NVARCHAR(100),newid())+char(39) +'',
	''+char(39) +CONVERT(NVARCHAR(100),@PKDocumentItemId)+char(39) +'',
	''+char(39) +CONVERT(NVARCHAR(100),@PKDocumentAcquisition)+char(39) +'')
END
GO''
)

end
else
begin
Print(''il doc '' + CONVERT(NVARCHAR(100),@DocumentId) + '' non esiste '')
end

end
	FETCH NEXT FROM db_cursor INTO @DocumentId,@Mandatory   
END 

CLOSE db_cursor  
DEALLOCATE db_cursor'

set @sql += '
--sostituti
IF OBJECT_ID(''tempdb..#tmp_tab2'') IS NOT NULL DROP TABLE #tmp_tab2

select distinct 
    aditgt.DocumentId as TgtId, adisub.DocumentId as SubId
	into #tmp_tab2
	from TBL_Stub_DocumentAcquisitionFilter fil
	inner join TBL_Stub_MapAcquisitionDocumentItemFilter map on map.FK_DocumentAcquisitionFilterId = fil.PK_DocumentAcquisitionFilterId
	inner join TBL_Stub_MapAcquisitionDocumentItemSubstituteFilter mapsub on mapsub.FK_MapAcquisitionDocumentItemFilterId = map.PK_MapAcquisitionDocumentItemFilterId
	inner join TBL_Stub_AcquisitionDocumentSubstitute ads on ads.PK_AcquisitionDocumentSubstituteId = mapsub.FK_AcquisitionDocumentSubstituteId
	inner join TBL_Stub_AcquisitionDocumentItem adisub on ads.FK_Substitute_AcquisitionDocumentItemId = adisub.PK_AcquisitionDocumentItemId
	inner join TBL_Stub_AcquisitionDocumentItem aditgt on ads.FK_Target_AcquisitionDocumentItemId = aditgt.PK_AcquisitionDocumentItemId
	where FK_ProductId = ''' + CONVERT(NVARCHAR(100), @ProductId) + '''' +
	' and LegalNature = ''' + @legalNature  + ''' '

	IF(@legalNature = 'PG')
		BEGIN
		if(@legalForm IS NULL)
			set @sql += ' AND LegalForm IS NULL'
		else
			set @sql += ' AND LegalForm = ''' + @legalForm + ''''

		
		if(@ciae IS NULL)
			set @sql += ' AND CIAE IS NULL'
		else
			set @sql += ' AND CIAE = ''' + @ciae + ''''

		
		if(@sae IS NULL)
			set @sql += ' AND SAE IS NULL'
		else
			set @sql += ' AND SAE = ''' + @sae + ''''
		END
	ELSE IF(@legalNature = 'PF')
		BEGIN
			if(@role IS NULL)
				set @sql += ' AND [Role] IS NULL'
			else
				set @sql += ' AND [Role] = ''' + @role + ''''
		END

set @sql += '
	if exists (select * from #tmp_tab2)
	begin
	Print(''Ci sono dei sostituti da controllare'')
	end'

exec sp_sqlexec @sql

END