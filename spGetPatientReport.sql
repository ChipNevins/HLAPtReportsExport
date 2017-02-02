USE [HLA_Reports]
GO

/****** Object:  StoredProcedure [dbo].[spGetPatientReport]    Script Date: 2/1/2017 10:20:31 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- spGetPatientReport 24
CREATE procedure [dbo].[spGetPatientReport]
  @reportId integer
as
begin
	Select * from hla_report_hdr where pkreportid = @reportId order by pkreportseq;
	Select rItem.*, rItem.dtaccessiondate + 1 as dtReceived
	  --,sp.dtbleeddate sp_bleed, sp.dtaccessiondate sp_Accession
	  from hla_report_item rItem
	   --left join hla_specimen sp on rItem.fkitemsid = sp.pksid
	 where pkreportid = @reportId order by pkreportseq, nbritemseq;
	Select * from hla_report_comment where pkreportid = @reportId order by pkreportseq, nbrcommentseq;
end







GO


