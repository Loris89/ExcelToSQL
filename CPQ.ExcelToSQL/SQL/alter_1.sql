USE [DBFEUNICO]
GO

/****** Object:  StoredProcedure [dbo].[sp_GetDocumentCompositionAcquisitionMetadata]    Script Date: 23/02/2021 16:51:40 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO



-- =============================================
-- Author:      <Author, , Name>
-- Create Date: <Create Date, , >
-- Description: <Description, , >
-- =============================================
ALTER PROCEDURE [dbo].[sp_GetDocumentCompositionAcquisitionMetadata]
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

set @sql = 'select distinct dt.DocumentTypeDescription as [Key],
    dt.DocumentName as DocumentName,
	item.DocumentId as DocumentId,
	item.Mandatory as Mandatory,
	item.IsProduct as IsProduct,
	fil.ElectronicArchiving as ElectronicArchiving,
	meta.[Key] as [Metadatakey],
	meta.[Value] as [Metadatavalue]
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


