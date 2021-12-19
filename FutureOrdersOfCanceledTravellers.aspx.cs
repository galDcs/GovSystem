using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.Text;
using System.Configuration;
public partial class FutureOrdersOfCanceledTravellers : System.Web.UI.Page
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
		DateTime dt = DateTime.Now;
		
		string query = @"SELECT  B.docket_id      
  , B.from_date      
  , B.to_date      
  , ST.name service_name      
  , S.name supplier_name      
  , isnull(V.id,0) voucher_id      
  , V.status voucher_status  
  , (case when V.status = 1 then 'Ok' else 'Cancelled' end) voucher_status_name      
  , (T.first_name + ' ' + T.last_name) traveller_name      
  , isnull(T.id_no, '') id_no      
  , isnull(T.gov_makat_number, '') gov_makat_number      
  , isnull(T.gov_docket_id, '') gov_docket_id      
  , isnull(T.gov_start_makat_date, '') gov_start_makat_date      
  , isnull(T.gov_balance_ussage, '') gov_balance_ussage      
  , Datediff(d, B.from_date, B.to_date) as nights  
  ,T.gov_makat_number
  ,ISNULL(B.gov_order_id, '') gov_order_id
FROM    dbo.BUNDLES B       
  INNER JOIN dbo.BUNDLES_to_TRAVELLERS BTT ON B.id = BTT.bundle_id       
  INNER JOIN Agency_Admin.dbo.SERVICE_TYPES ST ON B.service_type = ST.id       
  INNER JOIN Agency_Admin.dbo.Suppliers S on S.id=b.carrier      
  INNER JOIN dbo.TRAVELLERS T ON BTT.traveller_id = T.id       
  LEFT OUTER JOIN dbo.VOUCHERS V ON B.id = V.bundle_id      
where 
T.id_no + ' ' +  T.gov_makat_number in 
(
SELECT TravellerID + ' ' + GT.ItemSKU 
FROM         Gov_TRAVELLERS GT
WHERE     (IsActive = 0)
AND GT.ItemSKU = T.gov_makat_number 
		AND GT.ItemSKU NOT in (SELECT GTT.ItemSku FROM Gov_TRAVELLERS GTT WHERE GTT.ItemSKU = GT.ItemSKU 
																				AND GTT.IsActive = 1
																				AND GTT.TravellerID = GT.TravellerID)
)
and b.from_date >= cast('" + dt.ToString("dd-MMM-yy")+ @"' as smalldatetime)
AND V.status = 1";
//Response.Write(query);
		var ds = DAL_SQL.RunSqlDataSet(query);
		var view = new StringBuilder();
		
		if (Utils.IsDataSetRowsNotEmpty(ds))
		{
			
			view.Append(@"<table id='tblResult' class='table-result cell-border hover compact stripe' >
							<thead>
								<tr>
									<th>תיק</th>
									<th>שובר</th>
									<th>סטטוס שובר</th>
									<th>שם שירות</th>
									<th>שם ספק</th>
									<th>שם זכאי</th>
									<th>תעודת זהות</th>
									<th>תיק זכאי במשהבט</th>
									<th>מקט</th>
									<th>מספר הזמנה משהבט</th>
								</tr>
							</thead>
							<tbody>");
			var trOrder = @"<tr>
								<td>@DOCKET_ID</td>
								<td>@VOUCHER_ID</td>
								<td>@voucher_status_name</td>
								<td>@service_name</td>
								<td>@supplier_name</td>
								<td>@TRAVELLER_NAME</td>
								<td>@id_no</td>
								<td>@gov_docket_id</td>
								<td>@gov_makat_number</td>
								<td>@gov_order_id</td>
							</tr>";
			foreach (DataRow row in ds.Tables[0].Rows)
            {
				view.Append(trOrder.Replace("@DOCKET_ID", row["docket_id"].ToString())
								   .Replace("@VOUCHER_ID", row["VOUCHER_ID"].ToString())
								   .Replace("@voucher_status_name", row["voucher_status_name"].ToString())
								   .Replace("@service_name", row["service_name"].ToString())
								   .Replace("@supplier_name", row["supplier_name"].ToString())
								   .Replace("@TRAVELLER_NAME", row["traveller_name"].ToString())
								   .Replace("@id_no", row["id_no"].ToString())
								   .Replace("@gov_docket_id", row["gov_docket_id"].ToString())
								   .Replace("@gov_makat_number", row["gov_makat_number"].ToString())
								   .Replace("@gov_order_id", row["gov_order_id"].ToString())
							);
			}
			view.Append("</tbody></table>");
			lbResult.Text = view.ToString();
		}
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

}