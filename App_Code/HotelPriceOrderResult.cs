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
/// Summary description for HotelPriceOrderResult
/// </summary>
public class HotelPriceOrderResult
{
    public string AgencyDocketId { get; set; }
    public string AgencyVoucherId { get; set; }
    public bool OrderCompleted { get; set; }
    public string OrderMessage { get; set; }

	public HotelPriceOrderResult()
	{
        AgencyDocketId = "0";
        AgencyVoucherId = "0";
        OrderCompleted = false;
	}
}
