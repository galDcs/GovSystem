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
/// Summary description for GovTravellerMelave
/// </summary>
public class GovTravellerMelave
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public double TotalAmount { get; set; }
    //TotalAmount {
    //TotalAmount     get
    //TotalAmount     {
    //TotalAmount         return SibsudAmount + TravPayAmount;
    //TotalAmount     }
    //TotalAmount }
	
    public double SibsudAmount { get; set; }
    public double TravPayAmount { get; set; }
    public int RoomId { get; set; }

	public GovTravellerMelave()
	{
        RoomId = 0;
	}
}
