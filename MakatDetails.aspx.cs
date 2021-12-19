using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Text;

public partial class MakatDetails : System.Web.UI.Page
{
    public StringBuilder mainTable = new StringBuilder();
    public StringBuilder makats = new StringBuilder();
    
    protected void Page_Load(object sender, EventArgs e)
    {

        //if (!Utils.CheckSecurity(226)) Response.Redirect("AccessDenied.aspx");

        switch (Request["action"])
        {
            case "1": if (DAL_SQL_Helper.UpdateMakatDetails(Request["MakatNumber"], Request["MakatDescription"], Request["GeneralAreas"], Request["MinNights"], Request["MaxNights"], Request["OneTimeUssage"], Request["MakatTipulim"], Request["Allow5And5Nights"], Request["OfficeRemarkForOrder"], Request["StartOrderDateFromTodayMin"], Request["StartOrderDateFromTodayMax"], Request["AllowedToAdd5NightForPay"], Request["VoucherRemark"]))
                        {
							Logger.Log(string.Format("{0} , {1} , {2} , {3} , {4} , {5} , {6} , {7} , {8} ,min =  {9} ,max =  {10} , {11} , {12} , ",Request["MakatNumber"], Request["MakatDescription"], Request["GeneralAreas"], Request["MinNights"], Request["MaxNights"], Request["OneTimeUssage"], Request["MakatTipulim"], Request["Allow5And5Nights"], Request["OfficeRemarkForOrder"], Request["StartOrderDateFromTodayMin"], Request["StartOrderDateFromTodayMax"], Request["AllowedToAdd5NightForPay"], Request["VoucherRemark"]), eLogger.DEBUG);
							
                            Response.Write("1");
                        }
                        else
                        {
                            Response.Write("0");
                        }
                      
                        Response.End(); break;
            default: GenerateMakatsTable(); break;
        }
    }

    public void GenerateMakatsTable()
    {
    DataSet GeneralAreasDS = DAL_SQL_Helper.GetAllGeneralAreas();

        mainTable.Append("<table align=center class=\"MainTable\">");
        //Add Header rows
        mainTable.Append("<tr>");
        
        mainTable.Append("<th colspan=13>");
        mainTable.Append("</th>");

        mainTable.Append("<th colspan=2>");
        mainTable.Append("מועד היציאה לנופש מתאריך ההזמנה");
        mainTable.Append("</th>");

        mainTable.Append("<th colspan=2>");
        mainTable.Append("</th>");
        mainTable.Append("</tr>");


        mainTable.Append("<tr>");

        mainTable.Append("<th>מק\"ט</th>");
        mainTable.Append("<th>תאור</th>");
        foreach (DataRow dr2 in GeneralAreasDS.Tables[0].Rows)
        {
            mainTable.Append("<th>");
            mainTable.Append(dr2["general_area_name"].ToString());
            mainTable.Append("</th>");
        }

        mainTable.Append("<th>מינימום לילות</th>");
        mainTable.Append("<th>מקסימום לילות</th>");
        mainTable.Append("<th>מימוש חד פעמי</th>");
        mainTable.Append("<th>טיפולים</th>");
        mainTable.Append("<th>5+5</th>");
        mainTable.Append("<th>הערות משרד בהזמנה</th>");
        mainTable.Append("<th>מינימום</th>");
        mainTable.Append("<th>מקסימום</th>");
        mainTable.Append("<th>השלמת לילה חמישי ע\"ח זכאי</th>");
        mainTable.Append("<th>הערות שובר</th>");

        mainTable.Append("</tr>");

        DataSet MakatDetailsDS = DAL_SQL_Helper.GetMakatDetails();
        int makatIndex = 0;
        foreach (DataRow dr1 in MakatDetailsDS.Tables[0].Rows)
        {
            
            string makatNumber = dr1["MakatNumber"].ToString();


            makats.Append("makatsArray[" + makatIndex.ToString() + "]=\"" + makatNumber+"\";");
            
            mainTable.Append("<tr align=center>");

            mainTable.Append("<td id=\"" + makatNumber + "_MakatNumber\" name=\"" + makatNumber + "_MakatNumber\">");
            mainTable.Append(makatNumber);
            mainTable.Append("</td>");

            mainTable.Append("<td>");
            mainTable.Append("<textarea rows=\"4\" cols=\"3\" id=\"" + makatNumber + "_MakatDescription\" name=\"" + makatNumber + "_MakatDescription\" >" + dr1["MakatDescription"].ToString() + "</textarea>");
            mainTable.Append("</td>");

            DataSet currentAreasForMakat = DAL_SQL_Helper.GetGeneralAreas(dr1["MakatNumber"].ToString(), "");
            foreach (DataRow dr2 in GeneralAreasDS.Tables[0].Rows)
            {
                Boolean areaFound = false;
                foreach (DataRow dr3 in currentAreasForMakat.Tables[0].Rows)
                {
                    if (dr3["general_area_id"].ToString() == dr2["general_area_id"].ToString())
                    {
                        areaFound = true;
                        break;
                    }
                }
                if (areaFound)
                {
                    mainTable.Append("<td>");
            
                    mainTable.Append("<input type=\"checkbox\" checked=\"checked\" id=\"" + makatNumber + "_GA_" + dr2["general_area_id"].ToString() + "\" name=\"" + makatNumber + "_GA_" + dr2["general_area_id"].ToString() + "\"/>");
                    mainTable.Append("</td>");
                }
                else
                {
                    mainTable.Append("<td>");
                    mainTable.Append("<input type=\"checkbox\" id=\"" + makatNumber + "_GA_" + dr2["general_area_id"].ToString() + "\" name=\"" + makatNumber + "_GA_" + dr2["general_area_id"].ToString() + "\"/>");
                    
                    mainTable.Append("</td>");
                }
            }

            mainTable.Append("<td>");
            mainTable.Append("<input type=\"text\" size=\"3\"  id=\"" + makatNumber + "_MinNights\" name=\"" + makatNumber + "_MinNights\" value=\"" + dr1["MinNights"].ToString() + "\"/>");
            mainTable.Append("</td>");

            mainTable.Append("<td>");
            mainTable.Append("<input type=\"text\" size=\"3\"  id=\"" + makatNumber + "_MaxNights\" name=\"" + makatNumber + "_MaxNights\" value=\"" + dr1["MaxNights"].ToString() + "\"/>");
            mainTable.Append("</td>");

            mainTable.Append("<td>");
            if (dr1["OneTimeUssage"].ToString().ToLower()=="true")
            {
                mainTable.Append("<input type=\"checkbox\" checked=\"checked\" id=\"" + makatNumber + "_OneTimeUssage\" name=\"" + makatNumber + "_OneTimeUssage\"/>");
            }
            else
            {
                mainTable.Append("<input type=\"checkbox\" id=\"" + makatNumber + "_OneTimeUssage\" name=\"" + makatNumber + "_OneTimeUssage\"/>");
            }
            mainTable.Append("</td>");


            mainTable.Append("<td>");
            if (dr1["MakatTipulim"].ToString().ToLower() == "true")
            {
                mainTable.Append("<input type=\"checkbox\" checked=\"checked\" id=\"" + makatNumber + "_MakatTipulim\" name=\"" + makatNumber + "_MakatTipulim\"/>");
            }
            else
            {
                mainTable.Append("<input type=\"checkbox\" id=\"" + makatNumber + "_MakatTipulim\" name=\"" + makatNumber + "_MakatTipulim\"/>");
            }
            mainTable.Append("</td>");

            mainTable.Append("<td>");
            if (dr1["Allow5And5Nights"].ToString().ToLower() == "true")
            {
                mainTable.Append("<input type=\"checkbox\" checked=\"checked\" id=\"" + makatNumber + "_Allow5And5Nights\" name=\"" + makatNumber + "_Allow5And5Nights\"/>");
            }
            else
            {
                mainTable.Append("<input type=\"checkbox\" id=\"" + makatNumber + "_Allow5And5Nights\" name=\"" + makatNumber + "_Allow5And5Nights\"/>");
            }
            mainTable.Append("</td>");

            mainTable.Append("<td>");
            mainTable.Append("<textarea rows=\"4\" cols=\"3\" id=\"" + makatNumber + "_OfficeRemarkForOrder\" name=\"" + makatNumber + "_OfficeRemarkForOrder\" >" + dr1["OfficeRemarkForOrder"].ToString() + "</textarea>");
            mainTable.Append("</td>");


            mainTable.Append("<td>");
            mainTable.Append("<input type=\"text\" size=\"4\" id=\"" + makatNumber + "_StartOrderDateFromTodayMin\" name=\"" + makatNumber + "_StartOrderDateFromTodayMin\" value=\"" + dr1["StartOrderDateFromTodayMin"].ToString() + "\"/>");
            mainTable.Append("</td>");

            mainTable.Append("<td>");
            mainTable.Append("<input type=\"text\" size=\"4\" id=\"" + makatNumber + "_StartOrderDateFromTodayMax\" name=\"" + makatNumber + "_StartOrderDateFromTodayMax\" value=\"" + dr1["StartOrderDateFromTodayMax"].ToString() + "\"/>");
            mainTable.Append("</td>");


            mainTable.Append("<td>");
            if (dr1["AllowedToAdd5NightForPay"].ToString().ToLower() == "true")
            {
                mainTable.Append("<input type=\"checkbox\" checked=\"checked\" id=\"" + makatNumber + "_AllowedToAdd5NightForPay\" name=\"" + makatNumber + "_AllowedToAdd5NightForPay\"/>");
            }
            else
            {
                mainTable.Append("<input type=\"checkbox\" id=\"" + makatNumber + "_AllowedToAdd5NightForPay\" name=\"" + makatNumber + "_AllowedToAdd5NightForPay\"/>");
            }
            mainTable.Append("</td>");

            mainTable.Append("<td>");
            mainTable.Append("<textarea rows=\"4\" cols=\"3\" id=\"" + makatNumber + "_VoucherRemark\" name=\"" + makatNumber + "_VoucherRemark\" >" + dr1["VoucherRemark"].ToString() + "</textarea>");
            mainTable.Append("</td>");

            mainTable.Append("</tr>");

            makatIndex++;
        }


        
        mainTable.Append("</table>");

        mainTable.Append("<BR><input type=\"button\" value=\"שמור\" class='button-submit' onclick=\"saveData()\"/>");
    }
}
