using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.IO;
using System.Diagnostics;

/// <summary>
/// Summary description for Logger
/// </summary>
/// 
public enum eLogger
{
    DEBUG = 0,
    PRICE_NOT_FOUND,
    MULTI_PRICES,
    CHANGED_DETAILS,
    EXCEPTION,
    EXTRA_PAY,
	UPDATED_REFERAL,
	INVOICE_XML_ERROR
}

public static class Logger
{
    public static void Log(string text, string error, string FileName)
    {
        try
        {
            string debugFileName = @"Logs\log_" + DateTime.Now.ToString("yyyy_MM_dd") + ".txt";
            //            string path = HttpContext.Current.Request.MapPath(FileName +".txt");

            //Write allways to regular log.
            if (FileName != debugFileName)
            {
                string pathDebug = HttpContext.Current.Request.MapPath(debugFileName);

                using (StreamWriter sw = File.AppendText(pathDebug))
                {
                    sw.WriteLine("===============================================");
                    sw.WriteLine();
                    sw.WriteLine(DateTime.Now.ToLongDateString());
                    sw.WriteLine(DateTime.Now.ToLongTimeString());
                    sw.WriteLine(error);
                    sw.WriteLine(text);
                }
            }

            string path = HttpContext.Current.Request.MapPath(FileName);

            using (StreamWriter sw = File.AppendText(path))
            {
                sw.WriteLine("===============================================");
                sw.WriteLine();
                sw.WriteLine(DateTime.Now.ToLongDateString());
                sw.WriteLine(DateTime.Now.ToLongTimeString());
                sw.WriteLine(error);
                sw.WriteLine(text);
            }

            
        }
        catch (Exception ex) { }
    }

    public static void EmptyLog(string text, eLogger debugLevel)
    {
        string fileName = @"Logs\";

        switch (debugLevel)
        {
            case eLogger.DEBUG:
                fileName += "UserDebugLog_";
                break;
            case eLogger.PRICE_NOT_FOUND:
                fileName += "UserPriceNotFound_";
                break;
            case eLogger.MULTI_PRICES:
                fileName += "UserMultiPrices_";
                break;
            case eLogger.EXCEPTION:
                fileName += "UserException_";
                break;
            case eLogger.CHANGED_DETAILS:
                fileName += "UserChangeDetails_";
                break;
            case eLogger.EXTRA_PAY:
                fileName += "UserExtraPay_";
                break;
			case eLogger.UPDATED_REFERAL:
                fileName += "UpdatedReferal_";
                break;
			case eLogger.INVOICE_XML_ERROR:
                fileName += "INVOICE_XML_ERROR_";
                break;
        }

        fileName += (DateTime.Now.ToString("yyyy_MM_dd") + ".txt");

        try
        {
            string path = HttpContext.Current.Request.MapPath(fileName);

            using (StreamWriter sw = File.AppendText(path))
            {
                sw.WriteLine(text);
            }
        }
        catch (Exception ex) { }
    }
    public static void Log(Exception ex)
    {
        Log(ex.Message);
    }

    public static void Log(string msg)
    {
        bool fNeedFileInfo = true;
        StackTrace st = new StackTrace(fNeedFileInfo);
        StackFrame sf = st.GetFrame(1);

        Log("File: " + sf.GetFileName() + " , Method: " + sf.GetMethod().Name + ", Line: " + sf.GetFileLineNumber(), msg, @"Logs\log_" + DateTime.Now.ToString("yyyy_MM_dd") + ".txt");
    }

    public static void Log(string msg, eLogger debugLevel)
    {
        string fileName = @"Logs\";
        
        bool fNeedFileInfo = true;
        StackTrace st = new StackTrace(fNeedFileInfo);
        StackFrame sf = st.GetFrame(1);

        switch (debugLevel)
        {
            case eLogger.DEBUG:
                fileName += "DebugLog_";
                break;
            case eLogger.PRICE_NOT_FOUND:
                fileName += "PriceNotFound_";
                break;
            case eLogger.MULTI_PRICES:
                fileName += "MultiPrices_";
                break;
            case eLogger.EXCEPTION:
                fileName += "Exception_";
                break;
            case eLogger.CHANGED_DETAILS:
                fileName += "ChangeDetails_";
                break;
			case eLogger.INVOICE_XML_ERROR:
                fileName += "INVOICE_XML_ERROR_";
                break;
        }

        fileName += (DateTime.Now.ToString("yyyy_MM_dd") + ".txt");

        Log("File: " + sf.GetFileName() + " , Method: " + sf.GetMethod().Name + ", Line: " + sf.GetFileLineNumber(), msg, fileName);
    }
}
