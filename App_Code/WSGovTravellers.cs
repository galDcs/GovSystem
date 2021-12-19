using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Services;
using System.Web.Hosting;
using System.Configuration;

/// <summary>
/// Summary description for WSGovTravellers
/// </summary>
[WebService(Namespace = "http://tempuri.org/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
// To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
// [System.Web.Script.Services.ScriptService]
public class WSGovTravellers : System.Web.Services.WebService
{

    public const string K_PATH_ORIGINAL = @"~\UploadedFilesExe\BeforeEncoding\";
    public const string K_PATH_ENCODED = @"~\UploadedFilesExe\";
	
    //public const string K_PATH_ORIGINAL = @"d:\inetpub\";
    //public const string K_PATH_ENCODED = @"d:\inetpub\";

    public WSGovTravellers()
    {
        //Uncomment the following line if using designed components 
        //InitializeComponent(); 
		string AgencyId = "00"+ConfigurationManager.AppSettings.Get("AgencyUserId"); // = "0077";
		string SystemType = ConfigurationManager.AppSettings.Get("AgencyDbType"); //= "INN";
		
		DAL_SQL.ConnStr = ConfigurationManager.AppSettings["CurrentAgencyDBConnStr"];
        DAL_SQL.ConnStr = DAL_SQL.ConnStr.Replace("^agencyId^", AgencyId);
        DAL_SQL.ConnStr = DAL_SQL.ConnStr.Replace("^systemType^", SystemType);
    }
	
	[WebMethod]
	public bool BackupGovTravellers()
	{
		bool isBackedup = false;
		
		if (DAL_SQL_Helper.BackupTravellerTable())
        {
            isBackedup = true;
        }
		
		return isBackedup;
	}	
	
    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    [WebMethod]
    public string UploadGovTravellersFile(Byte[] iDocBinaryArray, string iDocName, out bool iStatus, out string iMessage)
    {
        bool hasSucceeded = false;
		string path = string.Empty;
        string failuareMessage = string.Empty;
		string msgTmp = string.Empty;
		iStatus = false;
		iMessage = "";
		string strFilename = "GOV_Import_Travellers_log_" + String.Format("{0:yyyy_dd_MM_HH_mm_ss}", DateTime.Now) + ".txt";
		try
		{
			iDocName = strFilename;
			
			if (saveDocument(iDocBinaryArray, iDocName, K_PATH_ORIGINAL))
			{
				Logger.Log("Saved Document");
				if (isFileInHebrew(iDocName))
				{
					if(saveDocument(iDocBinaryArray, iDocName, K_PATH_ENCODED))
					{
						msgTmp = "File-Language: hebrew. Save-File-Status:true";
						Logger.Log(msgTmp);
						iMessage = msgTmp;
						hasSucceeded = true;
					}
					else
					{
						msgTmp = "File-Language: hebrew. Save-File-Status:false. -error-";
						Logger.Log(msgTmp);
						iMessage = msgTmp;
						hasSucceeded = false;
					}
				}
				else
				{
					msgTmp = "File-Language: other. Save-File-Status:unkown";
					Logger.Log(msgTmp);
					iMessage = msgTmp;
					convertToUtf8AndSaveDocument(iDocName);
					hasSucceeded = true;
				}
			}
			else
			{
				iMessage = "error Could not save document";
				hasSucceeded = false;
			}
			
			iStatus = hasSucceeded;
			if (hasSucceeded)
			{
				path = K_PATH_ENCODED + iDocName;
			}
		}
		catch(Exception ex)
		{
			Logger.Log("Upload File Failed: " + ex.Message);
		}
        return path;
    }

	
	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	    [WebMethod]
        public bool UpsertTravellers(string filePath, out string iMessage)
        {
            const char dividerChar = '#';
            bool cleanZakaut = true;
			bool isUpserted = true;
            int TotalLines, ErrorLines, GoodLines, insertrows, updaterows;
			iMessage = string.Empty;
            TotalLines = 0;
            ErrorLines = 0;
            GoodLines = 0;
            insertrows = 0;
            updaterows = 0;
			try{
				if (!string.IsNullOrEmpty(filePath))
				{
					try{
						String[] fileData = File.ReadAllLines(HostingEnvironment.MapPath(filePath), System.Text.Encoding.UTF8);
						bool isErrOnInsert = false;
		
						TotalLines = fileData.Length;
						if (!DAL_SQL_Helper.DisableAllTravellers())
						{
							iMessage = "נכשל בביטול זכאים קיימים";
							
							return false;
						}
						
						for (int i = 0; i < fileData.Length; i++)
						{
							try
							{
								string result = SaveTravellerData(fileData[i].Split(dividerChar), cleanZakaut);
								GoodLines += 1;
								if (result == "update")
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
								Logger.Log("2 - " + excp.Message + ", i = " + i + " ErrorLines = " + ErrorLines.ToString() + ", updaterows = " + updaterows.ToString() + ", insertrows = " + insertrows.ToString());
							}
						}
						
						iMessage = "TotalLines=" + TotalLines + "&Updated=" + updaterows + "&Inserted=" + insertrows + "&ErrorLines=" +ErrorLines;
					}catch(Exception ex)
					{
						isUpserted = false;
						iMessage = "Exception = " + ex.Message;
						Logger.Log(ex.Message);
					}
				}
				else
				{
					isUpserted = false;
					iMessage = "Path is Empty";
					
				}
			}catch(Exception exCl)
			{
				iMessage = exCl.Message;
				Logger.Log(exCl.Message);
			}
			
			return isUpserted;
        }

        private static string SaveTravellerData(string[] values, bool CleanZakaut)
        {
            string result = DAL_SQL_Helper.UpsertTravellerImportExe(
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
                values[9], //phone 1 param 1
                values[10], //phone 1 param 2
                values[11], //phone 2 param 1
                values[12], //phone 2 param 2
                values[13], //phone 3 param 1
                values[14], //phone 3 param 2
                CleanZakaut, //CleanZakaut
                values[4], //Simon_100
                values[25], //Nagish
                values[26], //Request_SH
                values[27] //Mahadora
                );

            return result;
        }

//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    private static bool saveDocument(Byte[] iDocBinaryArray, string iDocName, string iPath)
    {
        string strDocPath;
		
        strDocPath = iPath + iDocName;
        FileStream objfilestream = new FileStream(HostingEnvironment.MapPath(strDocPath), FileMode.Create, FileAccess.ReadWrite);

        objfilestream.Write(iDocBinaryArray, 0, iDocBinaryArray.Length);
        objfilestream.Close();

        return true;
    }

    /* aviran 25/08 - checks if the file is encoded by Windows-1255 */
    private static bool isFileInHebrew(string iStrFilename)
    {
        bool isHebrew = false;
        string sourceDirectory = HostingEnvironment.MapPath(K_PATH_ORIGINAL + iStrFilename);
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
	
	   /*aviran 25/08 - convert file to hebrew and utf 8*/
    private static void convertToUtf8AndSaveDocument(string strFilename)
    {
        string sourceDir = HostingEnvironment.MapPath(K_PATH_ORIGINAL + strFilename);
        string newDir = HostingEnvironment.MapPath(K_PATH_ENCODED + strFilename);
        string fname = strFilename;
		
        Encoding encodingType = null;
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
    }
	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	
	[WebMethod]
	public bool SendMail(string iToMail, string iMessage)
	{
		string toClient = "tomerr@flying.co.il";
		bool isSent = Utils.SendMail(toClient, iMessage);
		
		//send to agency2000m@gmail.com
		isSent = Utils.SendMail(iToMail, iMessage);
		
		return isSent;
	}
	
    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    [WebMethod]
    public int GetDocumentLen(string DocumentName)
    {
        string strdocPath;
        strdocPath = K_PATH_ORIGINAL + DocumentName;

        FileStream objfilestream = new FileStream(strdocPath, FileMode.Open, FileAccess.Read);
        int len = (int)objfilestream.Length;
        objfilestream.Close();

        return len;
    }

    [WebMethod]
    public Byte[] GetDocument(string DocumentName)
    {
		Logger.Log("In function GetDocument - DocumentName = " + DocumentName);
        string strdocPath;
        strdocPath = K_PATH_ORIGINAL + DocumentName;

        FileStream objfilestream = new FileStream(strdocPath, FileMode.Open, FileAccess.Read);
        int len = (int)objfilestream.Length;
        Byte[] documentcontents = new Byte[len];
        objfilestream.Read(documentcontents, 0, len);
        objfilestream.Close();

        return documentcontents;
    }
////////////////////////////////////////////////////////////	
	[WebMethod]
    public string IsFileExist()
    {
		string path = "D:\\inetpub\\GovSystem\\UploadedFilesExe\\";
		string[] files = System.IO.Directory.GetFiles(path, "GOV_Import_Travellers_log_" + String.Format("{0:yyyy_dd_MM}", DateTime.Now.AddDays(-1)) + "*.txt");
		if (files.Length > 0)
		{
			return "GOV_Import_Travellers_log_" + String.Format("{0:yyyy_dd_MM}", DateTime.Now.AddDays(-1)) + "*.txt true " + files;
		}
		return "GOV_Import_Travellers_log_" + String.Format("{0:yyyy_dd_MM}", DateTime.Now.AddDays(-1)) + "*.txt false";
	}
}