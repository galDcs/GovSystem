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
/// Summary description for PricesResponse
/// </summary>
public class PricesResponse
{
    public bool Success { get; set; }
    public decimal PriceNetto { get; set; }
    public decimal PriceBrutto { get; set; }
    public string Message { get; set; }

	public PricesResponse()
	{
        Success = false;
        PriceNetto = 0;
        PriceBrutto = 0;
        Message = "";
	}
}
