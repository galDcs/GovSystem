using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.IO;
using System.Globalization;
using System.Threading;
using System.Text;

using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Data.SqlTypes;
 



public partial class ImportFileInvoice : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        //if (!Utils.CheckSecurity(224)) Response.Redirect("AccessDenied.aspx");
    }

    protected void UploadBtn_Click(object sender, CommandEventArgs e)
    {
        lblError.Text = "";
        string filePath = "";
        pnlFileList.Visible = false;

        try
        {
            string strFilename = "GOV_INV" + String.Format("{0:yyyy_dd_MM_HH_mm_ss}", DateTime.Now) + ".txt";

            if (FileUpLoad1.PostedFile != null && FileUpLoad1.PostedFile.ContentLength>0)
            {
                // Get a reference to PostedFile object
                HttpPostedFile myFile = FileUpLoad1.PostedFile;
                filePath = Server.MapPath(@"UploadedFiles\" + strFilename);
                FileUpLoad1.SaveAs(filePath); //Response.Write(filePath); Response.End();
                FileToUpload.Value = filePath;
                lblError.Text = "File (" + FileUpLoad1.PostedFile.FileName + ") uploaded to server, press process button to start import.";
                lblError.ForeColor = System.Drawing.Color.Green;
                
                ProcessWithDisable.Visible = true;
                 
               
                ProcessWithDisable.Font.Bold = true;

            }
        }
        catch (Exception ex)
        {
            lblError.Text = "אירעה תקלה , לפרטים ראה בקובץ הרישום .";// "An error occurred, for details see the log file.";
            lblError.ForeColor = System.Drawing.Color.Red;
            Logger.Log(ex.Message, ex.Source, @"Logs\import_log_" + DateTime.Now.ToString("yyyyMMdd") + ".txt");
        }
    }
    public  void ReadAndUpdateFile(string pathToFileRead)
    {

        string updated = "";
        if (File.Exists(pathToFileRead))
        {
            string[] ss = File.ReadAllLines(pathToFileRead);

            try
            {
                for (int i = 0; i < ss.Length; i++)
                {
                    string s = ss[i];
                    string[] linex = s.Split(';');
                    int inv_id = Convert.ToInt32(StripNonNumeric(linex[0]));
                    string gov_id = linex[1].Trim();
                    DataSet ds = DAL_SQL_Helper.GOV_UpdateInvoices(inv_id, gov_id);
                    updated += ds.GetXml().IndexOf(inv_id.ToString()) > -1 ? "<div style='color:green'>" + s + "</div>" : "<div style='color:red'>" + s + "</div>";
                }

            }catch(Exception ex){
                updated += ex.Message + "<br>";
            }
        }
        lblError.Text = updated;
    }
    [Microsoft.SqlServer.Server.SqlFunction]
    public static string StripNonNumeric(string input)
    {
        Regex regEx = new Regex(@"\D");
        return regEx.Replace(input, "");
    }  

    protected void ProcessUpdateInv_Click(object sender, EventArgs e)
    {
        ProcessWithDisable.Visible = false;

        ReadAndUpdateFile(FileToUpload.Value);
    }

    
    
   
}
