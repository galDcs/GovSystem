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
/// Summary description for HotelPrice
/// </summary>
public class HotelPrice
{
    public int PriceId;
    public double PriceAmountNetto; // will ne set
    public double PriceAmountBruto; // will be set
    public double TotalAmountToShow; // amount to show to clerk
    public double TravellerPriceToPay; // 
    public double ZakayPays; // calculated amount that zakai pays
    public double ZakaySibsud; // calculated amount that zakai pays
    public double MelavePays; // calculated amount that melave pays
    public double MelaveSibsud; // calculated amount that melave pays
    public string RoomState; // not used currently (need to check at search engin the correctens)

	public HotelPrice()
	{
        PriceId = 0;
        PriceAmountNetto = 0;
        PriceAmountBruto = 0;
        TotalAmountToShow = 0;
        TravellerPriceToPay = 0;
        ZakayPays = 0;
        ZakaySibsud = 0;
        MelavePays = 0;
        MelaveSibsud = 0;
        RoomState = string.Empty;
	}

    public bool calculateAmountToShow(GovTraveller traveller)
    {
        // added at 2014.01.14 - task from excel (12) need to put 10 nis
        /* if (traveller.IsAdded5thNight && (traveller.SelectedMakat[0].ItemSKU == "027235" || traveller.SelectedMakat[0].ItemSKU == "027236"))
        {
            PriceAmountNetto = 10;
            PriceAmountBruto = 10;
            TotalAmountToShow = 10;
            ZakayPays = 0;
            ZakaySibsud = 10;
            MelavePays = 0;
            MelaveSibsud = 0;
            return true;
        } */

        ZakayPays = 0;
        ZakaySibsud = PriceAmountBruto;
        MelavePays = 0;
        MelaveSibsud = 0;

        
        TotalAmountToShow = ZakayPays + MelavePays;
        // changed at 2012.02.22
        // all amount goes to traveller, therefore need to put all all amount to traveller and zero to melave
        ZakayPays = ZakayPays + MelavePays;
        ZakaySibsud = ZakaySibsud + MelaveSibsud;
        MelavePays = 0;
        MelaveSibsud = 0;

        TotalAmountToShow = Math.Round(TotalAmountToShow, 2);
        ZakayPays = Math.Round(ZakayPays, 2);
        ZakaySibsud = Math.Round(ZakaySibsud, 2);

        if (traveller.BalanceUssage) // makat 40 and option 5+5 all payment will be on subsid, but balance will be reduced by 11
        {
            ZakaySibsud = ZakaySibsud + ZakayPays;
            ZakayPays = 0;
            TotalAmountToShow = 0;
        }
        if (traveller.IsAdded5thNight)// need to calculate 5th night price (20% from total)
        {
            double night5th = (PriceAmountBruto * 0.2); // 20%
            ZakayPays += night5th; // zakay will pay 5th night
            ZakaySibsud -= night5th; // subsid will be decreased by one night
        }
        if (traveller.makatSelected("027241") && traveller.FourOneSeven.Equals("sevenOnZakai"))// need to calculate 7 nights price (50% from total)
        {
            ZakayPays = PriceAmountBruto / 2; // zakay will pay 50%
            ZakaySibsud = PriceAmountBruto / 2; // subsid will be 50%
            TotalAmountToShow = ZakayPays;
        }
        if (traveller.ErkevType.Equals("ZakaiAndMelaveBeTashlum"))// melave be tashlum all
        {
            ZakayPays = (PriceAmountBruto * 0.2); // zakay will pay 20%
            ZakaySibsud = (PriceAmountBruto * 0.8); // subsid will be 80%
            TotalAmountToShow = ZakayPays;
        }
        return true;
    }
}
