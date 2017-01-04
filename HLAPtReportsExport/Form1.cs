using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.Configuration;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;


namespace HLAPtReportsExport
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // read data from user control, retrieve pk's for this date
            // to be done
            // call builder with one pk
            int reportYear, reportPk;
            int nbrReportsWritten = 0;
            string reportName;
            if (Int32.TryParse(textBox1.Text, out reportYear))
            {
                string CS = ConfigurationManager.ConnectionStrings["HLA_DBCS"].ConnectionString;
                DataSet dsReportPks = new DataSet();
                using (SqlConnection conPk = new SqlConnection(CS))
                {
                    SqlDataAdapter da = new SqlDataAdapter("spGetPatientReportPkForYear", conPk);
                    da.SelectCommand.CommandType = CommandType.StoredProcedure;
                    da.SelectCommand.Parameters.AddWithValue("@yearId", reportYear);
                    da.Fill(dsReportPks);
                    foreach (DataRow dr in dsReportPks.Tables[0].Rows)
                    {
                        if (cbTop20.Checked & (++nbrReportsWritten > 20)) break;
                        Int32.TryParse(dr.ItemArray[0].ToString(), out reportPk);
                        reportName = dr["strreportname"].ToString();
                        BuildReport(reportName, reportPk);
                    }
                }
            }
            else
            {
                Console.WriteLine("Cannot convert input value to Year");
                return;
            }
            MessageBox.Show("Complete");


        }

        private void BuildReport(string reportName, int aReportPk)
        {
            string CS = ConfigurationManager.ConnectionStrings["HLA_DBCS"].ConnectionString;
            DateTime dt;
            string fileName;
            DataSet ds = new DataSet();

            using (SqlConnection con = new SqlConnection(CS))
            {
                SqlDataAdapter da = new SqlDataAdapter("spGetPatientReport", con);
                da.SelectCommand.CommandType = CommandType.StoredProcedure;
                da.SelectCommand.Parameters.AddWithValue("@reportId", aReportPk);
                da.Fill(ds);
            }
            ds.Tables[0].TableName = "ReportHdrs";
            ds.Tables[1].TableName = "ReportItems";
            ds.Tables[2].TableName = "ReportComments";

            // Get R# to create the document name
            fileName = "PK" + aReportPk.ToString();
            foreach (DataRow dr in ds.Tables["ReportItems"].Rows)
            {
                if (dr["strRelationToPt"].ToString().ToUpper().Contains("PATIENT"))
                {
                    if (dr["strrefnum"].ToString() !="")
                    {
                        fileName = "R" + dr["strrefnum"].ToString();
                    }

                }

            }
            using (Document document = new Document())
            {
                string myPath = ConfigurationManager.AppSettings["Path"];
                PdfWriter.GetInstance(document, new FileStream(myPath + fileName + ".pdf", FileMode.OpenOrCreate));
                document.Open();
                iTextSharp.text.Font font5 = iTextSharp.text.FontFactory.GetFont(FontFactory.HELVETICA, 5);
                iTextSharp.text.Font font6 = iTextSharp.text.FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 6);
                iTextSharp.text.Font font14 = iTextSharp.text.FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14);
                document.Add(new Paragraph(fileName, font14));

                foreach (DataRow drRep in ds.Tables["ReportHdrs"].Rows)
                {
                    // Header table
                    PdfPTable tableRep = new PdfPTable(7);
                    tableRep.SpacingBefore = 14;
                    tableRep.SetWidths(new int[] { 12, 70, 30, 40, 12, 12, 8 });
                    tableRep.AddCell(new Phrase("Version", font6));
                    tableRep.AddCell(new Phrase("Report Name", font6));
                    tableRep.AddCell(new Phrase("Provider Name", font6));
                    tableRep.AddCell(new Phrase("Hospital Name", font6));
                    tableRep.AddCell(new Phrase("Original Date", font6));
                    tableRep.AddCell(new Phrase("Updated Date", font6));
                    tableRep.AddCell(new Phrase("PK", font6));
                    tableRep.AddCell(new Phrase(drRep["pkReportSeq"].ToString(), font5));
                    tableRep.AddCell(new Phrase(drRep["strreportname"].ToString(), font5));
                    tableRep.AddCell(new Phrase(drRep["strProviderName"].ToString(), font5));
                    tableRep.AddCell(new Phrase(drRep["strHospitalName"].ToString(), font5));
                    tableRep.AddCell(new Phrase(DateString(drRep["dtoriginaldate"].ToString()), font5));
                    tableRep.AddCell(new Phrase(DateString(drRep["dtupdateddate"].ToString()), font5));
                    tableRep.AddCell(new Phrase(drRep["pkReportId"].ToString(), font5));
                    document.Add(tableRep);

                    // Items table
                    PdfPTable tableItem = new PdfPTable(11);
                    tableItem.SetWidths(new int[] { 30, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10 });
                    tableItem.SpacingAfter = 5;
                    tableItem.AddCell(new Phrase("Name", font6));
                    tableItem.AddCell(new Phrase("Relation", font6));
                    tableItem.AddCell(new Phrase("R#", font6));
                    tableItem.AddCell(new Phrase("Coll. Date", font6));
                    tableItem.AddCell(new Phrase("Recv'd Date", font6));
                    tableItem.AddCell(new Phrase("A", font6));
                    tableItem.AddCell(new Phrase("B", font6));
                    tableItem.AddCell(new Phrase("C", font6));
                    tableItem.AddCell(new Phrase("DRB1", font6));
                    tableItem.AddCell(new Phrase("DRB345", font6));
                    tableItem.AddCell(new Phrase("DQB1", font6));
                    foreach (DataRow drItem in ds.Tables["ReportItems"].Rows)
                    {
                        if (drRep.ItemArray[1].ToString() == drItem.ItemArray[1].ToString())
                        {
                            tableItem.AddCell(new Phrase(drItem["strPersonname"].ToString(), font5));
                            tableItem.AddCell(new Phrase(drItem["strRelationToPt"].ToString(), font5));
                            tableItem.AddCell(new Phrase(drItem["strRefNum"].ToString(), font5));
                            if (drItem["sp_bleed"].ToString() == "")
                            {
                                tableItem.AddCell(new Phrase(" ", font5));
                            }
                            else
                            {
                                dt = DateTime.Parse(drItem["sp_bleed"].ToString());
                                tableItem.AddCell(new Phrase(dt.ToShortDateString(), font5));
                            }
                            if (drItem["sp_accession"].ToString() == "")
                            {
                                tableItem.AddCell(new Phrase(" ", font5));
                            }
                            else
                            {
                                dt = DateTime.Parse(drItem["sp_accession"].ToString());
                                tableItem.AddCell(new Phrase(dt.ToShortDateString(), font5));
                            }
                            tableItem.AddCell(new Phrase(drItem["strA1"].ToString(), font5));
                            tableItem.AddCell(new Phrase(drItem["strB1"].ToString(), font5));
                            tableItem.AddCell(new Phrase(drItem["strCw1"].ToString(), font5));
                            tableItem.AddCell(new Phrase(drItem["strDrb11"].ToString(), font5));
                            tableItem.AddCell(new Phrase(drItem["strDRB3451"].ToString(), font5));
                            tableItem.AddCell(new Phrase(drItem["strDqb11"].ToString(), font5));
                            tableItem.AddCell(new Phrase(" ", font5));
                            tableItem.AddCell(new Phrase(" ", font5));
                            tableItem.AddCell(new Phrase(" ", font5));
                            tableItem.AddCell(new Phrase(" ", font5));
                            tableItem.AddCell(new Phrase(" ", font5));
                            tableItem.AddCell(new Phrase(drItem["strA2"].ToString(), font5));
                            tableItem.AddCell(new Phrase(drItem["strB2"].ToString(), font5));
                            tableItem.AddCell(new Phrase(drItem["strCw2"].ToString(), font5));
                            tableItem.AddCell(new Phrase(drItem["strDrb12"].ToString(), font5));
                            tableItem.AddCell(new Phrase(drItem["strDRB3452"].ToString(), font5));
                            tableItem.AddCell(new Phrase(drItem["strDqb12"].ToString(), font5));
                        }
                    }
                    document.Add(tableItem);

                    PdfPTable tableComment = new PdfPTable(2);
                    tableComment.SetWidths(new int[] { 10, 100 });
                    tableComment.SpacingAfter = 20;
                    tableComment.AddCell(new Phrase("Date", font6));
                    tableComment.AddCell(new Phrase("Comment", font6));
                    foreach (DataRow drComment in ds.Tables["ReportComments"].Rows)
                    {
                        if (drRep.ItemArray[1].ToString() == drComment.ItemArray[1].ToString())
                        {
                            tableComment.AddCell(new Phrase(DateString(drComment["dtCommentDate"].ToString()), font5));
                            tableComment.AddCell(new Phrase(drComment["strCommentText"].ToString(), font5));
                        }
                    }
                    document.Add(tableComment);
                }
                document.Close();
            }
            listBox1.Items.Add("Report for " + aReportPk.ToString() + " complete.");
        }

        private string DateString(string s)
        {
            DateTime dt;
            if (s != "")
            {
                dt = DateTime.Parse(s);
                return dt.ToShortDateString();
            }
            else
            {
                return " ";
            }
        }
    }



}

