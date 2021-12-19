using System;
using System.Collections;
using System.Web;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.Xml;
using System.Web.UI.MobileControls;
using System.Collections.Generic;
using System.Runtime.Serialization.Json;
using System.Web.Script.Services;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.Threading;

/// <summary>
/// Summary description for AjaxService
/// </summary>
[WebService(Namespace = "http://tempuri.org/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
// To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
[System.Web.Script.Services.ScriptService]
public class AjaxService : System.Web.Services.WebService {

    public AjaxService () {

        //Uncomment the following line if using designed components 
        //InitializeComponent(); 
    }

    [WebMethod(EnableSession=true)]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public List<BaseTypes> GetPriceBaseTypes(int hotelId, string FromDate, string ToDate)
    {
        List<BaseTypes> basesList;
        basesList = Agency2000Proxy.getAgencyBases(hotelId, DateTime.Parse(FromDate), DateTime.Parse(ToDate));
        return basesList;
    }

    [WebMethod(EnableSession = true)]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public HotelPrice GetHotelPrice(int hotelId, int allocationId, int baseId, string fromDateStr, string toDateStr, string isNewPrice)
    {
		string selectedMelaveDates = string.Empty;
		//Logger.Log("fromDateStr = " + fromDateStr);
		try
		{
			selectedMelaveDates = fromDateStr.Split('|')[1];
		}
		catch
		{
			selectedMelaveDates = string.Empty;
		}
		
		fromDateStr = fromDateStr.Split('|')[0];
		//Logger.Log("selectedMelaveDates = " + selectedMelaveDates);
        List<HotelPrice> price = new List<HotelPrice>();
        GovTraveller traveller = GovTraveller.LoadFromSession();
        DateTime frDate = new DateTime();
        DateTime toDate = new DateTime();
		//Sending selectedMelaveDates in message and erase it in the func Agency2000Proxy.getHotelPrice.
		isNewPrice = isNewPrice + "|" + selectedMelaveDates;
        string errorMessage = string.Empty;

        frDate = DateTime.Parse(fromDateStr);
        toDate = DateTime.Parse(toDateStr);
        price = Agency2000Proxy.getHotelPrice(hotelId, allocationId, baseId, frDate, toDate, traveller, out errorMessage, isNewPrice,null);
        
        if (price == null || price.Count == 0)
        {
            if (errorMessage.Length > 0)
            {
                throw new Exception(errorMessage);
            }
            else
            {
                throw new Exception("No prices found for this allocation.");
            }
        }
        else if (price.Count > 1)
        {
            throw new Exception("Query returns more than one (" + price.Count.ToString() + ") price for this allocation.");
        }
		
        return price[0];
    }

    [WebMethod(EnableSession = true)]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public HotelPriceOrderResult MakeOrder(int hotelId, int priceId, int allocationId, string fromDateStr, string toDateStr, string amountNetoStr, string amountBrutoStr, string zakayPaysStr, string zakaySibsudStr, string melavePaysStr, string melaveSibsudStr, int areaId, string datesSelectedStr)
    {
		Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture("en-gb");//chen - he-il
		//if (Session["Agency2000"] != null)
		//{
		//	Logger.Log(Session["Agency2000"].ToString());
		//}
		//else
		//{
		//	Logger.Log("Session is null");
		//}
			
        GovTraveller traveller = GovTraveller.LoadFromSession();
        HotelPriceOrderResult result = new HotelPriceOrderResult();
       
       
        // check if test action
        if (traveller.TravellerId.StartsWith("999"))
        {
            return new HotelPriceOrderResult() 
            {
                AgencyDocketId = "0",
                AgencyVoucherId = "0",
                OrderCompleted = false,
                OrderMessage = "זה משתמש טסט, לכן אין אפשרות לבצע הזמנה."
            };
        }

        DateTime frDate = new DateTime();
        DateTime toDate = new DateTime();

        double amountNeto = double.Parse(amountNetoStr);
        double amountBruto = double.Parse(amountBrutoStr);
        double zakayPays = double.Parse(zakayPaysStr);
        double zakaySibsud = double.Parse(zakaySibsudStr);
        // will be always zero (since 2012.02.22)
        double melavePays = double.Parse(melavePaysStr);
        double melaveSibsud = double.Parse(melaveSibsudStr);

		Logger.Log(fromDateStr + toDateStr);
        frDate = DateTime.Parse(fromDateStr);
        toDate = DateTime.Parse(toDateStr);

        traveller.TravellerPayAmount = zakayPays;
        traveller.SibsudAmount = zakaySibsud;

        if (traveller.Melave.Count > 0)
        {
            traveller.Melave[0].TravPayAmount = 0; //melavePays; //cause bug. its inserting the price in " הש' עובד "
            traveller.Melave[0].SibsudAmount = 0; //melaveSibsud;
            traveller.Melave[0].TotalAmount = melavePays;
        }
        traveller.TotalAmountBruto = amountBruto;
        traveller.TotalAmountNetto = amountNeto;
		//datesSelectedStr = (datesSelectedStr.Contains("0") ? datesSelectedStr : "" ); 
		datesSelectedStr = "";
		if (traveller.ErkevType.Contains("ZakaiBeTashlumAndMelaveLeLoTashlum"))
		{
			datesSelectedStr = datesSelectedStr.PadLeft((toDate - frDate).Days, '1');
		}
		
        traveller.StrMelaveSelectedNights = datesSelectedStr;
        
		//Logger.Log("amountNeto = " + amountNeto);
		//Logger.Log("amountBruto = " + amountBruto);
		//Logger.Log("priceId = " + priceId);
        result = Agency2000Proxy.createDocket(hotelId, priceId, allocationId, frDate, toDate, amountNeto, amountBruto, traveller, areaId);

        return result;
    }

    [WebMethod(EnableSession = true)]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public string StopProcessing() 
    {
        Session["StopProcess"] = true;
        return "ok";
    }
	
	
	[WebMethod(EnableSession = true)]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public bool DefaultFunc()
    {
        return true;
    }
	
	[WebMethod(EnableSession = true)]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public HotelPriceOrderResult MakeOrderTreatment(string iSupplierId)
    {
		Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture("en-gb");
		HotelPriceOrderResult result = new HotelPriceOrderResult();
        GovTraveller traveller = GovTraveller.LoadFromSession();
        Logger.Log("MakeOrderTreatment st");
        result = Agency2000Proxy.createDocketOnlyWithHotelPriceOrderResult(traveller);
		Logger.Log("MakeOrderTreatment en");
        return result;
    }
	
}
