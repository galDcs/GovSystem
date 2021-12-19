using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.Text;
using System.Configuration;
using TreatmentEntitledService;
public partial class GovFalseStatus : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!Page.IsPostBack)
        {
            setAgencyData();
            buildOrderView();
        }
    }

    private void buildOrderView()
    {
        var view = new StringBuilder();
        var travDocketIds = getTravellerDocketIdsOfCanceledAgencyFutureOrders();
        var accessToken = string.Empty;

        if (travDocketIds != null && travDocketIds.Count > 0)
        {
			view.Append(@"<table id='tblResult' class='table-result cell-border hover compact stripe' >
							<thead>
								<tr>
									<th>תיק</th>
									<th>Agency סטטוס</th>
									<th>מספר הזמנה משהבט</th>
									<th>סטטוס משהבט</th>
									<th>מקט</th>
									<th>מספר בקשה</th>
									<th>תאריך תחילה</th>
									<th>תאריך סיום</th>
									<th>ימים</th>
									<th>קנס</th>
									<th>מלווים</th>
								</tr>
							</thead>
							<tbody>");
									
            //foreach (var travDocketId in travDocketIds)
            for (var i = 0; i < travDocketIds.Count; i++)
            {
                //view.Append(getOrdersView(getAccessToken(travDocketId), travDocketId));
				
				view.Append(getOrdersView(getAccessToken(travDocketIds[i]), travDocketIds[i]));
            }
			
			view.Append("</tbody></table>");
        }
        else
        {
            view.Append("אין תיקים עתידיים מבוטלים");
        }
		
		lbResult.Text = view.ToString();
    }

    private List<string> getTravellerDocketIdsOfCanceledAgencyFutureOrders()
    {
        var docketIds = new List<string>();
        string query = @"SELECT distinct GT.DocketId as docketId
FROM    dbo.BUNDLES B       
  INNER JOIN dbo.BUNDLES_to_TRAVELLERS BTT ON B.id = BTT.bundle_id       
  INNER JOIN dbo.TRAVELLERS T ON BTT.traveller_id = T.id       
  inner join dbo.Gov_TRAVELLERS GT ON GT.TravellerID = T.id_no
  LEFT OUTER JOIN dbo.VOUCHERS V ON B.id = V.bundle_id      
where 
cast(b.from_date as smalldatetime) >= cast('" + DateTime.Now.Date.ToString("dd-MMM-yy") + @"' as smalldatetime)
and v.status=2
and v.service_type_id = 2
order by DocketId asc";

        DataSet ds = DAL_SQL.RunSqlDataSet(query);

        if (Utils.IsDataSetRowsNotEmpty(ds))
        {
            foreach (DataRow row in ds.Tables[0].Rows)
            {
                docketIds.Add(row["docketId"].ToString().Trim());
            }
        }
        return docketIds;
    }

    private string getAccessToken(string iDocketID)
    {
        string accessToken = string.Empty;
        try
        {
            string mSupplierSecret = "6985456", mSupplierID = "4825";
            int mSupplierNumber = 1;
            Session["DocketID"] = iDocketID;
            bool isTokenResponseSuccess = true;
            ServiceTokenResponse tokenResponse = null;
			System.Net.ServicePointManager.SecurityProtocol = (System.Net.SecurityProtocolType)3072;
            TreatmentEntitledServiceClient treatmentClient = new TreatmentEntitledServiceClient();

            tokenResponse = treatmentClient.GetTokenMoked(iDocketID, mSupplierNumber, mSupplierSecret, mSupplierID);


            StringBuilder clientFailedMsg = new StringBuilder();

            if (tokenResponse.FailureCode != null)
            {
                int failure = tokenResponse.FailureCode.Id;
                foreach (string msg in tokenResponse.FailureCode.ClientMessages)
                {
                    clientFailedMsg.Append(msg);
                    clientFailedMsg.Append(Environment.NewLine);
                }
            }
            accessToken = tokenResponse.AccessToken;
        }
        catch (Exception ex)
        {

        }

        return accessToken;
    }

    private string getOrdersView(string iAccessToken, string iDocketID)
    {
		var sb = new StringBuilder();
		if (string.IsNullOrEmpty(iAccessToken))
		{
			sb.Append("No Access Token");
		}
		else
		{
			System.Net.ServicePointManager.SecurityProtocol = (System.Net.SecurityProtocolType)3072;
        TreatmentEntitledServiceClient treatmentClient = new TreatmentEntitledServiceClient();
			ServiceEntitledResponse responseEntitled = treatmentClient.GetEntitledDetails(iAccessToken);
			string[] messages = responseEntitled.Status.ClientMessages;
			string message = messages[0];
			StringBuilder clientFailedMsg = new StringBuilder();
	
			if (responseEntitled.FailureCode != null)
			{
				int failure = responseEntitled.FailureCode.Id;
				foreach (string msg in responseEntitled.FailureCode.ClientMessages)
				{
					clientFailedMsg.Append(msg);
					clientFailedMsg.Append(Environment.NewLine);
				}
			}
			if (responseEntitled.EntitledOrders != null)
			{
				string query = @"SELECT 
								B.docket_id, 
								B.id as bundle_id, 
								V.id as voucher_id,
								V.status,
								(SELECT top 1 GT.FirstName + ' ' + GT.SecondName FROM Gov_TRAVELLERS GT WHERE GT.DocketId = @GOV_DOCKET_ID) as traveller_name
								FROM BUNDLES B
								INNER JOIN VOUCHERS V on V.bundle_id = B.id
								WHERE B.gov_order_id = @GOV_ORDER_ID";
				var trOrder = @"<tr>
									<td>@AGN_DOCKET_ID</td>
									<td class='status @AGN_STATUS_CLASS'>@AGN_STATUS_TEXT</td>
									<td>@GOV_ORDER_ID</td>
									<td class='status @GOV_STATUS_CLASS'>@GOV_STATUS_TEXT</td>
									<td>@GOV_ITEM_SKU</td>
									<td>@GOV_REQUEST_SH</td>
									<td>@GOV_FROM_DATE</td>
									<td>@GOV_TO_DATE</td>
									<td>@GOV_DAYS</td>
									<td>@GOV_FINE_DAYS</td>
									<td>@GOV_ESCORT</td>
								</tr>";
				foreach (EntitledOrder order in responseEntitled.EntitledOrders)
				{
					var agnDs = DAL_SQL.RunSqlDataSet(query
												  .Replace("@GOV_ORDER_ID", order.Id.ToString().Trim())
												  .Replace("@GOV_DOCKET_ID", iDocketID));
					if (Utils.IsDataSetRowsNotEmpty(agnDs))
					{
						var dvdr = "</td><td>";
						var fromDate = DateTime.Parse(order.StartDate.ToString());
						var row = agnDs.Tables[0].Rows[0];
						var agnStatus = row["status"].ToString() == "1";
						var govStatus = order.Status;
						
						if (fromDate >= DateTime.Now && agnStatus != govStatus)
						{						
							sb.Append(trOrder.Replace("@AGN_DOCKET_ID", row["docket_id"].ToString())
											 .Replace("@AGN_STATUS_CLASS", getStatusClass(agnStatus))
											 .Replace("@AGN_STATUS_TEXT", getStatusText(agnStatus))
											 .Replace("@GOV_ORDER_ID", order.Id.ToString())
											 .Replace("@GOV_STATUS_TEXT", getStatusText(govStatus))
											 .Replace("@GOV_STATUS_CLASS", getStatusClass(govStatus))
											 .Replace("@GOV_ITEM_SKU", order.KatalogNumber)
											 .Replace("@GOV_REQUEST_SH", order.RequestId)
											 .Replace("@GOV_FROM_DATE", order.StartDate.ToString())
											 .Replace("@GOV_TO_DATE", order.EndDate.ToString())
											 .Replace("@GOV_DAYS", order.DaysNumber.ToString())
											 .Replace("@GOV_FINE_DAYS", order.FineDays.ToString())
											 .Replace("@GOV_ESCORT", order.AccompaniedNumber.ToString())
											 );
						}
					}
					//}
				}
			}
		}
        return sb.ToString();
    }

	private string getStatusText(bool iIsActive)
	{
		return iIsActive ? "פעיל" : "מבוטל";
	}
	
	private string getStatusClass(bool iIsActive)
	{
		return iIsActive ? "active" : "disabled";
	}
	
    private void setAgencyData()
    {
        AgencyUser user = new AgencyUser()
        {
            AgencyId = Request["AgnAgencyId"],
            AgencyUserId = Request["AgnClerkId"],
            AgencySystemType = Request["AgnSystemType"],
            AgencyUserName = Request["AgnClerkName"],
            AgencyUserPassword = Request["AgnClerkPassword"]
        };
        DAL_SQL.ConnStr = ConfigurationManager.AppSettings["CurrentAgencyDBConnStr"]
                          .Replace("^agencyId^", ((user.AgencyId.Length == 1) ? "000" + user.AgencyId : "00" + user.AgencyId))
                          .Replace("^systemType^", ((user.AgencySystemType == "3") ? "INN" : "OUT"));
    }

    private bool unitTestGetDocketTest(bool showOnScreen)
    {
        var isValid = false;
        try
        {
            var docketIds = getTravellerDocketIdsOfCanceledAgencyFutureOrders();
            var view = new StringBuilder();

            if (docketIds != null && docketIds.Count > 0)
            {
                isValid = true;
                if (showOnScreen == true)
                {
                    string newTravTemplate = "<tr><td>@TRAVELLER_DOCKET_ID</td></tr>";
                    foreach (string id in docketIds)
                    {
                        view.Append(newTravTemplate.Replace("@TRAVELLER_DOCKET_ID", id));
                    }
                }
            }
            else
            {
                if (showOnScreen == true)
                {
                    view.Append("No Results");
                }
            }
            if (showOnScreen == true)
            {
                lbResult.Text = view.ToString();
            }
        }
        catch
        {

        }
        return isValid;
    }
}