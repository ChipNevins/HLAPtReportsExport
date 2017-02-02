USE [HLA_Reports]
GO

/****** Object:  StoredProcedure [dbo].[spGetPatientReportPkRemaining]    Script Date: 2/1/2017 10:36:01 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO






ALTER Procedure [dbo].[spGetPatientReportPkRemaining]

AS

begin

select distinct pkreportid
from hla_report_hdr
     where dtrecordadded > (getdate() - 30)

end





GO


