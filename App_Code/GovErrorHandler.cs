using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using TreatmentEntitledService;

/// <summary>
/// Summary description for GovErrorHandler
/// </summary>
public class GovErrorHandler
{
	public GovErrorHandler()
	{
		
	}

    public static string getErrorInNewOrder(ServiceNewOrderResponse orderResponse, out StringBuilder oClientFailedMsg, out string oLogMessage)
    {
        oClientFailedMsg = new StringBuilder();
        string failureMessage = string.Empty;
        int failure = orderResponse.FailureCode.Id;
        const string kPleaseReLoginMsg = "אירעה שגיאה, אנא התחברו מחדש";

        foreach (string msg in orderResponse.FailureCode.ClientMessages)
        {
            oClientFailedMsg.Append(msg);
            oClientFailedMsg.Append(Environment.NewLine);
        }

        switch (failure)
        {
            case 1:
                failureMessage = kPleaseReLoginMsg;
                oLogMessage = "שגיאת ולידציה-נתונים חסרים או לא תקינים";
                break;
            case 4:
                failureMessage = kPleaseReLoginMsg;
                oLogMessage = "Access Token אינו תקין או פג תוקף";
                break;
            case 3:
                failureMessage = kPleaseReLoginMsg;
                oLogMessage = "אירעה שגיאה";
                break;
            case 5:
                failureMessage = "שגיאה, קיימת הזמנה בתאריכים אלו";
                oLogMessage = "הזמנה כבר קיימת לזכאי בתאריכים אלו - לפי מקט תאריך התחלה ותאריך סיום";
                break;
            case 8:
                failureMessage = "זכאות זו לא קיימת בשבילך, אנא צור קשר עם אגף שיקום";
                oLogMessage = "מקט לא קיים לזכאי";

                break;
            default:
                failureMessage = kPleaseReLoginMsg;
                oLogMessage = "אירעה שגיאה שמשרד הביטחון לא טיפל בה (failure code case does not exist)";
                break;
        }

        return failureMessage;
    }
}