USE [HLA_Reports]
GO

/****** Object:  StoredProcedure [dbo].[spGetPatientReportPkForYear]    Script Date: 2/1/2017 10:29:23 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO





Create Procedure [dbo].[spGetPatientReportPkForYear]
	@yearId integer
AS

begin

select distinct pkreportid
from hla_report_hdr
where year(dtOriginalDate) = @yearId

end




GO


