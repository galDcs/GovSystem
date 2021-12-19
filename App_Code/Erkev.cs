using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Text;

/// <summary>
/// Summary description for Erkev
/// </summary>
public static class Erkev
{
    // commented by igor on 2012.02.23 - preffer to save selected makat and work with it (whole object)
    //public static string GenerateTableHtml(int escortNumber, string makat, int makatBalance)
    public static string GenerateTableHtml(GovTravellerMakat selectedMakat)
    {
        StringBuilder ret_str = new StringBuilder();

        ret_str.Append("<br/><table class='trans' id='erkevTable'>");
        ret_str.Append("<tr><td colspan='2' align='center'><h2 style='margin:0px;'>הרכב בפועל</h2></td></tr>");

        if (selectedMakat.ItemSKU != "027240")
        {
            ret_str.Append("<tr>");
            ret_str.Append("<th style='width:25px;'><input type='radio' name='erkev' value='Zakai'></th>");
            ret_str.Append("<td>זכאי</td>");
            ret_str.Append("</tr>");

            if (selectedMakat.EscortNum == 1 || selectedMakat.EscortNum == 2)
            {
                ret_str.Append("<tr>");
                ret_str.Append("<th><input type='radio' name='erkev' value='ZakaiAndMelave'></th>");
                ret_str.Append("<td>זכאי + מלווה</td>");
                ret_str.Append("</tr>");
            }
            if (selectedMakat.EscortNum == 2)
            {
                ret_str.Append("<tr>");
                ret_str.Append("<th><input type='radio' name='erkev' value='ZakaiAnd2Melavim'></th>");
                ret_str.Append("<td>זכאי + 2 מלווים</td>");
                ret_str.Append("</tr>");
            }
            /*if (selectedMakat.EscortNum == 0)
            {
                ret_str.Append("<tr>");
                ret_str.Append("<th><input type='radio' name='erkev' value='ZakaiAndMelaveBeTashlum'></th>");
                ret_str.Append("<td>זכאי + מלווה בתשלום לכל התקופה</td>");
                ret_str.Append("</tr>");
                ret_str.Append("<tr>");
                ret_str.Append("<th><input type='radio' name='erkev' value='ZakaiAndMelaveBeTashlumHelekTkufa'></th>");
                ret_str.Append("<td>זכאי + מלווה בתשלום לתקופה חלקית</td>");
                ret_str.Append("</tr>");
            }*/
            //if (selectedMakat.ItemSKU == "027241" && (selectedMakat.DaysNum - selectedMakat.UsageBalance) == 7)
            //{
            //    ret_str.Append("<tr><td colspan='2'><h3>תוספת</h3></td></tr>");
            //    ret_str.Append("<tr>");
            //    //ret_str.Append("<th style='width:25px;'><input type='radio' name='four_one_seven' value='sevenOnZakai'></th>");
			//	ret_str.Append("<th style='width:25px;'><input type='radio' name='' value='sevenOnZakai'></th>");
            //    ret_str.Append("<td>מבקש להזמין 14 לילות מתוכם 7 על חשבון הזכאי</td>");
            //    ret_str.Append("</tr>");
            //}
        }
        else // changed at 2012.02.22 - only makat *40 can have this option
        {
            bool option1Enabled = (selectedMakat.Makat40 == 1) ? false : true;
            bool option2Enabled = (selectedMakat.Makat40 == 2) ? false : true;

            ret_str.Append("<tr>");
            ret_str.Append("<th>");
            if (option1Enabled)
            {
                ret_str.Append("<input type='radio' name='erkev' value='ZakaiBeTashlumAndMelaveLeLoTashlum' />");
            }
            ret_str.Append("</th>");
            ret_str.Append("<td>זכאי בתשלום + מלווה ללא תשלום</td>");
            // commented on 2012.04.17 - to allow not only 5+5, but also 2+2,3+3,4+4 and 5+5
            //if (selectedMakat.UsageBalance >= 11)
            //{
            // commented on 2012.11.20 and created new erkev 
            //ret_str.Append("<td><input type='checkbox' id='chBalanceUssage' name='chBalanceUssage' />מעוניין לממש את היתרה עבור מלווה</td>");
            //}
            ret_str.Append("</tr>");
            // removed at 2014.01.14 by design of Ilan from mail (not excel)
            // && (selectedMakat.DaysNum - selectedMakat.UsageBalance >= 2)
            if (selectedMakat.DaysNum == 11)
            {
                ret_str.Append("<tr>");
                ret_str.Append("<th>");
                if (option2Enabled)
                {
                    ret_str.Append("<input type='radio' name='erkev' value='ZakaiAndMelaveTmuratZakaut' />");
                }
                ret_str.Append("</th>");
                ret_str.Append("<td>זכאי ומלווה שניהם ללא תשלום תמורת ניצול כפול של ימי זכאות(5+5)</td>");
                ret_str.Append("</tr>");
            }
        }
        if ((selectedMakat.ItemSKU == "027235" || selectedMakat.ItemSKU == "027236") && (selectedMakat.DaysNum - selectedMakat.UsageBalance) == 4)
        {
            ret_str.Append("<tr><td colspan='2'><h3>תוספת</h3></td></tr>");
            ret_str.Append("<tr>");
            ret_str.Append("<th><input type='radio' name='four_one_seven' value='fourNightHotel5Tipulim' data='four_one'></th>");

            ret_str.Append("<td>   4 לילות  מלון + 5 טיפולים - אחד על חשבון הזכאי(4+1)</td>");
            ret_str.Append("</tr>");
            //ret_str.Append("<tr>");
            //ret_str.Append("<th><input type='radio' name='four_one_seven' value='fiveNightHotelTipulim' data='four_one'></th>");
            //ret_str.Append("<td>   5 לילות  מלון - אחד על חשבון הזכאי ו 5 טיפולים - אחד על חשבון הזכאי(4+1)</td>");
            //ret_str.Append("</tr>");
        }

        ret_str.Append("</table>");

        return ret_str.ToString();
    }
}

public enum ErkevTypes
{
    Zakai,
    ZakaiAndMelave,
    ZakaiAnd2Melavim,
    ZakaiAndMelaveBeTashlum,
    ZakaiAndMelaveBeTashlumHelekTkufa,
    ZakaiBeTashlumAndMelaveLeLoTashlum,
    ZakaiAndMelaveTmuratZakaut

}


//fourNightHotel5Tipulim,
//fiveNightHotelTipulim,
//sevenOnZakai
