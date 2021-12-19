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
using System.Text.RegularExpressions;

public partial class ImportTravellers : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
       // if (!Utils.CheckSecurity(224)) Response.Redirect("AccessDenied.aspx");
    }

    protected void UploadBtn_Click(object sender, CommandEventArgs e)
    {
        lblError.Text = "";
        string filePath = "";
        pnlFileList.Visible = false;

        try
        {
            string strFilename = "GOV_Import_Travellers_log_" + String.Format("{0:yyyy_dd_MM_HH_mm_ss}", DateTime.Now) + ".txt";

            if (FileUpLoad1.PostedFile != null && FileUpLoad1.PostedFile.ContentLength > 0)
            {
                // Get a reference to PostedFile object
                HttpPostedFile myFile = FileUpLoad1.PostedFile;

                /* aviran 25/08 - the file before encodeing will be in this path UploadedFiles\BeforeEncoding */
                filePath = Server.MapPath(@"UploadedFiles\BeforeEncoding\" + strFilename);
                FileUpLoad1.SaveAs(filePath); //Response.Write(filePath); Response.End();

                /* aviran . - the final file will be in this folder*/
                string hebrewFilePath = Server.MapPath(@"UploadedFiles\" + strFilename);

                /* aviran 25/08 - convert file to utf-8 hebrew (not latin) if needed*/
                if (isFileInHebrew(strFilename) == false)
                {
                    convertFileToUtf8(strFilename);
                }
                else
                {
                    FileUpLoad1.SaveAs(hebrewFilePath); //Response.Write(filePath); Response.End();
                }

                FileToUpload.Value = filePath;

                /* aviran 25/08 - save the path in hidden element in asp:HiddenField */
                HebrewFileToUpload.Value = hebrewFilePath;

                lblError.Text = "File (" + FileUpLoad1.PostedFile.FileName + ") uploaded to server, press process button to start import.";
                lblError.ForeColor = System.Drawing.Color.Green;

                ProcessWithDisable.Visible = true;
                //ProcessWithOutDisable.Visible = true;
                //ProcessWithOutZakautClean.Visible = true;
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

    protected void ProcessWithOutDisable_Click(object sender, EventArgs e)
    {
        ProcessWithDisable.Visible = false;
        ProcessWithOutDisable.Visible = false;
        ProcessWithOutZakautClean.Visible = false;
        processData(false, true);
    }

    protected void ProcessWithDisable_Click(object sender, EventArgs e)
    {
        ProcessWithDisable.Visible = false;
        ProcessWithOutDisable.Visible = false;
        ProcessWithOutZakautClean.Visible = false;
        processData(true, true);
    }

    protected void ProcessWithOutZakautClean_Click(object sender, EventArgs e)
    {
        ProcessWithDisable.Visible = false;
        ProcessWithOutDisable.Visible = false;
        ProcessWithOutZakautClean.Visible = false;
        processData(true, false);
    }

    private bool processData(bool DisableTravellers, bool CleanZakaut)
    {
        if (!DAL_SQL_Helper.BackupTravellerTable())
        {
            lblError.Text = "אירעה תקלה בגיבוי טבלת הנוסעים .";// "An error occurred at backup Gov Traveller table.";
            lblError.ForeColor = System.Drawing.Color.Red;
            return false;
        }

        if (DisableTravellers)
        {
            if (!DAL_SQL_Helper.DisableAllTravellers())
            {
                lblError.Text = "אירעה תקלה באיתחול מצב הנוסעים .";// "An error occurred at reset travellers status.";
                lblError.ForeColor = System.Drawing.Color.Red;
                return false;
            }
        }

        Session["StopProcess"] = false;
        /*  aviran 25/08 get the hebrew file path  */
        string hebrewFile = HebrewFileToUpload.Value;

        /* aviran 25/08 - old code - draws the unencoded file*/
        //UploadFile(FileToUpload.Value, CleanZakaut);

        /* aviran 25/08 - upload the encoded file*/
        UploadFile(hebrewFile, CleanZakaut);

        lblError.Text = "התהליך הושלם .";// "Processing complete.";
        lblError.ForeColor = System.Drawing.Color.Green;
        return true;
    }



    protected void StopProcess_Click(object sender, EventArgs e)
    {
        Session["StopProcess"] = true;
        lblError.Text = "התהליך נעצר .";// "The process is stopped.";
        lblError.ForeColor = System.Drawing.Color.Green;
        ProcessWithDisable.Visible = false;
        ProcessWithOutDisable.Visible = false;
        ProcessWithOutZakautClean.Visible = false;
    }

    protected void Restore_Click(object sender, EventArgs e)
    {
        if (Checkpassword(TBpaswword.Text))
        {
            Session["StopProcess"] = true;
            if (!DAL_SQL_Helper.RestoreTravellerTable())
            {
                lblError.Text = "אירעה תקלה בשיחזור טבלת הנוסעים .";// "An error occurred at Restore Traveller table";
                lblError.ForeColor = System.Drawing.Color.Red;
            }
            else
            {
                lblError.Text = "השיחזור הושלם .";// "Restore completed.";
                lblError.ForeColor = System.Drawing.Color.Green;
            }
        }
        else
        {
            lblError.Text = "סיסמא שגויה ! ";// "Wrong password !";
            lblError.ForeColor = System.Drawing.Color.Red;
        }
        ProcessWithDisable.Visible = false;
        ProcessWithOutDisable.Visible = false;
        ProcessWithOutZakautClean.Visible = false;
    }

    private bool Checkpassword(string password)
    {
        if (password == ConfigurationManager.AppSettings.Get("RestoreFilePassword")) return true;
        return false;
    }

    /* aviran 25/08 - checks if the file is encoded by Windows-1255 */
    private bool isFileInHebrew(string iStrFilename)
    {
        bool isHebrew = false;
        string sourceDirectory = Server.MapPath(@"UploadedFiles\BeforeEncoding\" + iStrFilename);
        FileStream fileStream = new FileStream(sourceDirectory, FileMode.Open, FileAccess.ReadWrite);
        StreamReader ReadFile = new StreamReader(fileStream, System.Text.Encoding.UTF8);
        String strLine;

        while (ReadFile != null)
        {
            strLine = ReadFile.ReadLine();
            if (strLine != null)
            {
                /* aviran 25/08 - check if line is in hebrew */
                isHebrew = IsStringValidForCodePage(strLine, 1255);
            }

            ReadFile.Close();
            ReadFile = null;
            break;
        }

        return isHebrew;
    }

    /*aviran 25/08 - convert file to hebrew and utf 8*/
    private void convertFileToUtf8(string strFilename)
    {
        string sourceDir = Server.MapPath(@"UploadedFiles\BeforeEncoding\" + strFilename);
        string newDir = Server.MapPath(@"UploadedFiles\" + strFilename);
        string fname = strFilename;
        Encoding encodingType = null;

        //if (isFileInHebrew(strFilename) == true)
        //{
        //    encodingType = Encoding.UTF8;
        //}
        //else
        //{
            encodingType = Encoding.Default;
            FileStream fs = new FileStream(sourceDir, FileMode.Open, FileAccess.ReadWrite);
            StreamReader ReadFile = new StreamReader(fs, encodingType);
            FileStream fs1 = new FileStream(newDir, FileMode.OpenOrCreate, FileAccess.Write);
            StreamWriter WriteFile = new StreamWriter(fs1, Encoding.UTF8);
            String strLine;
            while (ReadFile != null)
            {
                strLine = ReadFile.ReadLine();
                if (strLine != null)
                {
                    Encoding latinEncoding = Encoding.GetEncoding("Windows-1252");
                    Encoding hebrewEncoding = Encoding.GetEncoding("Windows-1255");
                    string hebrewString;
                    byte[] latinBytes = latinEncoding.GetBytes(strLine);

                    bool isLineHebrew = IsStringValidForCodePage(strLine, 1255);
                    if (isLineHebrew == true)
                    {
                        hebrewString = strLine;
                    }
                    else
                    {
                        hebrewString = hebrewEncoding.GetString(latinBytes);
                    }
                    WriteFile.WriteLine(hebrewString);
                }
                else
                {
                    ReadFile.Close();
                    ReadFile = null;
                    WriteFile.Close();
                }
            }
        //}
    }

    /*aviran 25/08 - gets a string and code*/
    public static bool IsStringValidForCodePage(string text, int codePage)
    {
        var encoder = Encoding.GetEncoding(codePage, new EncoderExceptionFallback(), new DecoderExceptionFallback());

        try
        {
            encoder.GetBytes(text);
        }
        catch (EncoderFallbackException)
        {
            return false;
        }

        return true;
    }

    private void UploadFile(string filePath, bool CleanZakaut)
    {
        int TotalLines, ErrorLines, GoodLines, insertrows, updaterows;
        TotalLines = 0;
        ErrorLines = 0;
        GoodLines = 0;
        insertrows = 0;
        updaterows = 0;

        if (!string.IsNullOrEmpty(filePath))
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture("he-IL");

            /* aviran 24/08 - the encoding didn't work - old code */
            // read nad insert upoaded file
            //  String[] fileData = File.ReadAllLines(filePath, System.Text.Encoding.Default);

            //aviran 24/08 - read all line, encoding utf-8
            String[] fileData = File.ReadAllLines(filePath, System.Text.Encoding.UTF8);

            bool isErrOnInsert = false;

            TotalLines = fileData.Length;

            // skips the last row that conains num rows in the file
			//old zakaut file
            //for (int i = 0; i < fileData.Length - 1; i++)
			for (int i = 0; i < fileData.Length; i++)
            {
                try
                {
                    if ((bool)Session["StopProcess"])
                    {
                        break;
                    }
                    string resualt = SaveTravellerData(fileData[i].Split('#'), CleanZakaut);
                    GoodLines += 1;
                    if (resualt == "update")
                    {
                        updaterows += 1;
                    }
                    else
                    {
                        insertrows += 1;
                    }
                }
                catch (Exception excp)
                {
                    ErrorLines += 1;
                    isErrOnInsert = true;
                    Logger.Log("Row " + i + ". " + excp.Message, excp.Source, @"Logs\import_log_" + DateTime.Now.ToString("yyyyMMdd") + ".txt");
                }
            }

            if (!isErrOnInsert)
            {
                lblError.Text = "File Uploaded Successfully. " + GoodLines.ToString() + " out of " + TotalLines.ToString() + " , " + updaterows.ToString() + " lines update " + insertrows.ToString() + " lines inserted (" + ErrorLines.ToString() + " errors lines).";
                lblError.ForeColor = System.Drawing.Color.Green;
            }
            else
            {
                lblError.Text = "An error occurred while processing the file, for details see the log file. " + GoodLines.ToString() + " out of " + TotalLines.ToString() + " lines processed. Error lines: " + ErrorLines.ToString() + " ";
                lblError.ForeColor = System.Drawing.Color.Red;
                //Logger.Log(ex.Message, ex.Source, @"Logs\import_log_" + DateTime.Now.ToShortDateString() + ".txt");
            }
            if ((bool)Session["StopProcess"])
            {
                lblError.Text += "התהליך נעצר באמצע ! ";// " the process was stoped in middle ! ";
                lblError.ForeColor = System.Drawing.Color.Green;
            }
        }
        else
        {
            lblError.Text = "חסר קובץ .";// "No files to process.";
            lblError.ForeColor = System.Drawing.Color.Red;
        }
    }



    private string SaveTravellerData(string[] values, bool CleanZakaut)
    {
        /* aviran 24/08 - old code based on a different structre of file */
        /*
        string result= DAL_SQL_Helper.UpsertTravellerImport(
            values[0], // DocketId
            values[1], // TravellerID
            values[2], // FirstName
            values[3], // SecondName
            values[4], // Address
            values[5], // CityCode
            values[6], // City
            values[7], // ZipCode
            values[14], // ItemSKU
            values[15],  // ItemDescription ,
            DateTime.Parse(values[16]), // StartDate
            DateTime.Parse(values[17]), // EndDate
            DateTime.Parse(values[18]), // ReleaseDate
            int.Parse(values[19]), // DaysNum
            int.Parse(values[20]), // EscortNum ,
            int.Parse(values[21]), // Department
            int.Parse(values[22]), // Level
            int.Parse(values[23]), // Status
            values[8], //aviran - phone 1 param 1
            values[9], //aviran - phone 1 param 2
            values[10], //aviran - phone 2 param 1
            values[11], //aviran - phone 2 param 2
            values[12], //aviran - phone 3 param 1
            values[13], //aviran - phone 3 param 2
            CleanZakaut //aviran - CleanZakaut
        );*/

        /* aviran 24/08 - new file structure */
        string result = DAL_SQL_Helper.UpsertTravellerImport(
            values[0], // DocketId
            values[1], // TravellerID
            values[2], // FirstName
            values[3], // SecondName
            values[5], // Address
            values[6], // CityCode
            values[7], // City
            values[8], // ZipCode
            values[15], // ItemSKU                    
            values[16],  // ItemDescription ,
            DateTime.Parse(values[17]), // StartDate
            DateTime.Parse(values[18]), // EndDate
            DateTime.Parse(values[19]), // ReleaseDate
            int.Parse(values[20]), // DaysNum
            int.Parse(values[21]), // EscortNum       
            int.Parse(values[22]), // Department      
            int.Parse(values[23]), // Level           
            int.Parse(values[24]), // Status
            values[9], //aviran - phone 1 param 1
            values[10], //aviran - phone 1 param 2
            values[11], //aviran - phone 2 param 1
            values[12], //aviran - phone 2 param 2
            values[13], //aviran - phone 3 param 1
            values[14], //aviran - phone 3 param 2
            CleanZakaut, //aviran - CleanZakaut
            values[4], //aviran - Simon_100
            values[25], //aviran - Nagish
            values[26], //aviran - Request_SH
            values[27] //aviran - Mahadora
            );

        return result;
    }


    // NOT ACTUAL ALL FILES WILL BE UPLOADED FROM CLIENT
    /*protected void ViewServerFiles_Click(object sender, CommandEventArgs e)
    {
        GetServerFiles();
        pnlFileList.Visible = true;
    }*/
    /*private void GridDataBind(string filePath)
    {
        DataTable dt = FileToDataTable(filePath, "#");
        grid.DataSource = dt;
        grid.DataBind();
        grid.Visible = true;
    }
    protected void grid_PageIndexChanging(object sender, GridViewPageEventArgs e)
    {
        grid.PageIndex = e.NewPageIndex;
        GridDataBind(Session["currFileName" + Session.SessionID].ToString());
    }*/
    /*protected void Btn_File_OnClick(Object sender, CommandEventArgs e)
    {
        Response.Write("aaaaa");
        Response.End();

        lblError.Text = "";
        string orgFilePath = e.CommandArgument.ToString();
        string strFilename = "GOV_Import_Travellers_log_" + String.Format("{0:yyyy_dd_MM_HH_mm_ss}", DateTime.Now) + ".txt";
        string fileName = e.CommandName.ToString();
        string filePath = Server.MapPath(@"UploadedFiles\" + strFilename);
        File.Copy(orgFilePath, filePath);
        UploadFile(filePath);
    }*/
    /*public void GetServerFiles()
    {
        string dir = ConfigurationManager.AppSettings.Get("ImportTravellersFileDir"); ;
        DirectoryInfo di = new DirectoryInfo(dir);
        FileInfo[] files = di.GetFiles("*.txt", SearchOption.TopDirectoryOnly);

        if (files.Length <= 0)
        {
            lblError.Text = "There is files at server.";
        }
        else
        {
            for (int i = 0; i < files.Length; i++)
            {
                LinkButton btn = new LinkButton();
                btn.Text = files[i].Name;
                btn.Attributes.Add("title", "Click to load this file");
                btn.Command += new CommandEventHandler(Btn_File_OnClick);
                btn.CommandName = files[i].Name;
                btn.CommandArgument = files[i].FullName;
                pnlFileList.Controls.Add(btn);
                pnlFileList.Controls.Add(new LiteralControl("<br/>"));
                pnlFileList.Controls.Add(new LiteralControl("<br/>"));
            }
        }
    }*/
    /*private static DataTable FileToDataTable(string filename, string separatorChar)
    {
        HttpContext.Current.Response.Write("FileToDataTable");
        HttpContext.Current.Response.End();

        var table = new DataTable("StringLocalization");
        using (var sr = new StreamReader(filename, Encoding.Default))
        {
            string line;
            var i = 0;
            while (sr.Peek() >= 0)
            {
                line = sr.ReadLine();
                if (string.IsNullOrEmpty(line)) continue;
                var values = line.Split(new[] { separatorChar }, StringSplitOptions.None);
                var row = table.NewRow();
                for (var colNum = 0; colNum < values.Length; colNum++)
                {
                    var value = values[colNum];
                    if (i == 0)
                    {
                        table.Columns.Add(colNum.ToString(), typeof(String));
                    }
                    else
                    {
                        row[table.Columns[colNum]] = value;
                    }
                }
                if (i != 0) table.Rows.Add(row);
                i++;
            }
        }
        return table;
    }*/
}
