using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using TreatmentEntitledService;

public partial class CancelOrder : System.Web.UI.Page
{
    private string mAccessToken { get; set; }
    private string mSupplierID = ConfigurationManager.AppSettings["supplierID"].ToString();
    private int mSupplierNumber = Convert.ToInt32(ConfigurationManager.AppSettings["supplierNumber"].ToString());
    private string mSupplierSecret = ConfigurationManager.AppSettings["supplierSecret"].ToString();

    protected void Page_Load(object sender, EventArgs e)
    {		
        string travellerDocketID = Request.QueryString["govdocketid"];
        string govOrderID = Request.QueryString["govorderid"];
        string indicationCancelReason = Request.QueryString["indication"];
        int fineDays = 0;
        int cancelReason = 0;

        switch (indicationCancelReason)
        {
            case "1":
                cancelReason = 1;
                fineDays = 1;
                break;
            case "7":
            case "10":
                cancelReason = 2;
                fineDays = 1;
                break;
            default:
                break;
        }

        cancelOrder(travellerDocketID, govOrderID, cancelReason, fineDays);
    }

    private void cancelOrder(string iDocketId, string iOrderID, int iCancleReason, int iFineDays)
    {
		System.Net.ServicePointManager.SecurityProtocol = (System.Net.SecurityProtocolType)3072;
        TreatmentEntitledServiceClient treatmentClient = new TreatmentEntitledServiceClient();
        ServiceTokenResponse tokenResponse = treatmentClient.GetTokenMoked(iDocketId, mSupplierNumber, mSupplierSecret, mSupplierID);
        string[] messages = tokenResponse.Status.ClientMessages;
        string message = messages[0];
        StringBuilder clientFailedMsgAccessT = new StringBuilder();

        if (tokenResponse.FailureCode != null)
        {
            int failure = tokenResponse.FailureCode.Id;
            foreach (string msg in tokenResponse.FailureCode.ClientMessages)
            {
                clientFailedMsgAccessT.Append(msg);
                clientFailedMsgAccessT.Append(Environment.NewLine);
            }
        }
        else
        {
            mAccessToken = tokenResponse.AccessToken;
        }

        int OrderID = int.Parse(iOrderID);
        StringBuilder clientFailedMsg = new StringBuilder();
        ServiceResponse cancelResponse = treatmentClient.CancelOrder(mAccessToken, OrderID, iFineDays, iCancleReason);
        string orderStatus = cancelResponse.Status.ClientMessages[0];

        if (cancelResponse.FailureCode != null)
        {
            int failure = cancelResponse.FailureCode.Id;
            foreach (string msg in cancelResponse.FailureCode.ClientMessages)
            {
                clientFailedMsg.Append(msg);
                clientFailedMsg.Append(Environment.NewLine);

            }
			
           // Session["ClientMessage"] = getClientErrorMessage(failure);
          //  Response.Redirect("./Default.aspx");//chen - todo pass error file
        }
        else
        {
			Logger.Log("Canceled order: " + iOrderID + " || Docket ID: " + iDocketId + " GOVOrderID "+ iCancleReason + " Fine Days: "+iFineDays);

            Session["ClientMessage"] = "ההזמנה בוטלה";
            Response.Redirect("./TravellerHistory.aspx");
        }
        treatmentClient.Close();
    }

}