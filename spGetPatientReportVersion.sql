USE [HLA_Reports]
GO

/****** Object:  StoredProcedure [dbo].[spGetPatientReportVersion]    Script Date: 2/1/2017 10:19:43 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO



CREATE procedure [dbo].[spGetPatientReportVersion]
  @reportId integer,
  @reportSeq integer
as
begin
	Select * from hla_report_hdr where pkreportid = @reportId and pkReportSeq = @reportSeq;
	Select * from hla_report_item where pkreportid = @reportId and pkReportSeq = @reportSeq
	  order by nbritemseq;
	Select * from hla_report_comment where pkreportid = @reportId and pkReportSeq = @reportSeq
	  order by nbrcommentseq;
end


GO


