USE [DBFEUNICO]
GO

/****** Object:  StoredProcedure [dbo].[sp_GetDocumentCompositionAcquisitionSubstituteList]    Script Date: 23/02/2021 17:25:36 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


ALTER PROCEDURE [dbo].[sp_GetDocumentCompositionAcquisitionSubstituteList]
(
    @ProductId UNIQUEIDENTIFIER,
	@legalNature NVARCHAR(100) = NULL,
	@legalForm NVARCHAR(100) = NULL,
	@role NVARCHAR(100) = NULL,
	@ciae NVARCHAR(100) = NULL,
	@sae NVARCHAR(100) = NULL,
	@idDocList NVARCHAR(100) = NULL
)
AS
BEGIN
declare @sql NVARCHAR (MAX)

set @sql = 'select distinct adisub.DocumentId as SubId,
    aditgt.DocumentId as TgtId
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

		if(@idDocList IS NULL)
			set @sql += ' AND IdDocList IS NULL'
		else
			set @sql += ' AND IdDocList = ''' + @idDocList + ''''
		END
	ELSE IF(@legalNature = 'PF')
		BEGIN
			if(@role IS NULL)
				set @sql += ' AND [Role] IS NULL'
			else
				set @sql += ' AND [Role] = ''' + @role + ''''
		END

	exec sp_sqlexec @sql
    SET NOCOUNT ON	

END
GO


