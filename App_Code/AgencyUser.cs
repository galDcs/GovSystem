using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;

/// <summary>
/// Summary description for AgencyUser
/// </summary>
public class AgencyUser
{
    private static string agencyAgencyIdSessionName = "agencyAgencyIdSessionName";
    private static string agencySystemTypeSessionName = "agencySystemTypeSessionName";
    private static string agencyUserIdSessionName = "agencyUserIdSessionName";
    private static string agencyUserNameSessionName = "agencyUserNameSessionName";
    private static string agencyUserPasswordSessionName = "agencyUserPasswordSessionName";


    public string AgencyId
    {
        get
        {
            if (HttpContext.Current.Session[agencyAgencyIdSessionName + HttpContext.Current.Session.SessionID] != null)
			{
                return HttpContext.Current.Session[agencyAgencyIdSessionName + HttpContext.Current.Session.SessionID].ToString();
			}
            //throw new Exception("Error: Session end.");
            return string.Empty;
        }
        set
        {
            HttpContext.Current.Session[agencyAgencyIdSessionName + HttpContext.Current.Session.SessionID] = value;
        }
    }

    public string AgencySystemType
    {
        get
        {
            if (HttpContext.Current.Session[agencySystemTypeSessionName + HttpContext.Current.Session.SessionID] != null)
                return HttpContext.Current.Session[agencySystemTypeSessionName + HttpContext.Current.Session.SessionID].ToString();
            //throw new Exception("Error: Session end.");
            return string.Empty;
        }
        set
        {
            HttpContext.Current.Session[agencySystemTypeSessionName + HttpContext.Current.Session.SessionID] = value;
        }
    }

    public string AgencyUserId
    {
        get
        {
            if (HttpContext.Current.Session[agencyUserIdSessionName + HttpContext.Current.Session.SessionID] != null)
                return HttpContext.Current.Session[agencyUserIdSessionName + HttpContext.Current.Session.SessionID].ToString();
            //throw new Exception("Error: Session end.");
            return string.Empty;
        }
        set
        {
            HttpContext.Current.Session[agencyUserIdSessionName + HttpContext.Current.Session.SessionID] = value;
        }
    }

    public string AgencyUserName
    {
        get
        {
            if (HttpContext.Current.Session[agencyUserNameSessionName + HttpContext.Current.Session.SessionID] != null)
                return HttpContext.Current.Session[agencyUserNameSessionName + HttpContext.Current.Session.SessionID].ToString();
            //throw new Exception("Error: Session end.");
            //return string.Empty;
            return ConfigurationManager.AppSettings.Get("AgencyUserName");
        }
        set
        {
            HttpContext.Current.Session[agencyUserNameSessionName + HttpContext.Current.Session.SessionID] = value;
        }
    }

    public string AgencyUserPassword
    {
        get
        {
            if (HttpContext.Current.Session[agencyUserPasswordSessionName + HttpContext.Current.Session.SessionID] != null)
                return HttpContext.Current.Session[agencyUserPasswordSessionName + HttpContext.Current.Session.SessionID].ToString();
            //throw new Exception("Error: Session end.");
            //return string.Empty;
            return ConfigurationManager.AppSettings.Get("AgencyPassword");
        }
        set
        {
            HttpContext.Current.Session[agencyUserPasswordSessionName + HttpContext.Current.Session.SessionID] = value;
        }
    }
	
}
