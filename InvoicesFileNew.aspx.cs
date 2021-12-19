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
using System.IO;

public partial class InvoicesFileNew : System.Web.UI.Page
{
    //private int display_counter_bad = 0;
    //private int display_counter_good = 0;
    private string delimiter = ";";
    private int lines_counter = 0;

    protected void Page_Load(object sender, EventArgs e)
    {
        /*if (!Utils.CheckSecurity(239)) 
		{
			Response.Redirect("AccessDenied.aspx");
		}*/

        if (Page.IsPostBack)
        {
            
        }
		
    }

	protected void btSearch_Click(object sender, EventArgs e)
	{
		StringBuilder result = new StringBuilder();

            #region test_data_hardcoded
            //Test Data
            //DateTime fromExitDate = new DateTime(2012, 2, 1);
            //DateTime toExitDate = new DateTime(2012, 2, 29);
            //DateTime fromUpdateExitDate = new DateTime(2012, 2, 1);
            //DateTime toUpdateExitDate = new DateTime(2012, 2, 29);
            //DateTime fromUpdateDate = new DateTime(2012, 2, 1);
            //DateTime toUpdateDate = new DateTime(2012, 2, 29);
            #endregion
            int reportType = 0;

            DateTime fromExitDate = new DateTime(1900, 1, 1);
            DateTime toExitDate = new DateTime(1900, 1, 1);
            DateTime fromTermDate = new DateTime(1900, 1, 1);
            DateTime toTermDate = new DateTime(1900, 1, 1);
            DateTime fromUpdateExitDate = new DateTime(1900, 1, 1);
            DateTime toUpdateExitDate = new DateTime(1900, 1, 1);
            DateTime fromUpdateDate = new DateTime(1900, 1, 1);
            DateTime toUpdateDate = new DateTime(1900, 1, 1);
            DateTime fromUpdateTermDate = new DateTime(1900, 1, 1);
            DateTime toUpdateTermDate = new DateTime(1900, 1, 1);

            if (Request["ddlReportType"] != null && !String.IsNullOrEmpty(Request["ddlReportType"]))
            {
                int.TryParse(Request["ddlReportType"], out reportType);
            }

            if ( ((Request["txtFromExitDate"] != "" && Request["txtToExitDate"] != "") || (Request["txtToTermDate"] != "" && Request["txtFromTermDate"] != ""))
                || ((Request["txtUpdateFromDate"] != "" && Request["txtUpdateToDate"] != "") && ((Request["txtUpdateFromExitDate"] != "" && Request["txtUpdateToExitDate"] != "") || (Request["txtUpdateFromTermDate"] != "" && Request["txtUpdateToTermDate"] != ""))))
            {
                if (Request["txtFromExitDate"] != "" && Request["txtToExitDate"] != "")
                {
                    fromExitDate = DateTime.Parse(Request["txtFromExitDate"]);
                    toExitDate = DateTime.Parse(Request["txtToExitDate"]);
                }

                if (Request["txtFromTermDate"] != "" && Request["txtToTermDate"] != "")
                {
                    fromTermDate = DateTime.Parse(Request["txtFromTermDate"]);
                    toTermDate = DateTime.Parse(Request["txtToTermDate"]);
                }

                if (Request["txtUpdateFromDate"] != "" && Request["txtUpdateToDate"] != "")
                {
                    fromUpdateDate = DateTime.Parse(Request["txtUpdateFromDate"]);
                    toUpdateDate = DateTime.Parse(Request["txtUpdateToDate"]);

                    if (Request["txtUpdateFromExitDate"] != "" && Request["txtUpdateToExitDate"] != "")
                    {
                        fromUpdateExitDate = DateTime.Parse(Request["txtUpdateFromExitDate"]);
                        toUpdateExitDate = DateTime.Parse(Request["txtUpdateToExitDate"]);
                    }

                    if (Request["txtUpdateFromTermDate"] != "" && Request["txtUpdateToTermDate"] != "")
                    {
                        fromUpdateTermDate = DateTime.Parse(Request["txtUpdateFromTermDate"]);
                        toUpdateTermDate = DateTime.Parse(Request["txtUpdateToTermDate"]);
                    }
                }


                if (toUpdateDate != new DateTime(1900, 1, 1))
                {
                    toUpdateDate = toUpdateDate.AddDays(1);
                }

                DataSet ds = DAL_SQL_Helper.GetInvoicesForInvoicesFileNew(fromExitDate, toExitDate, fromTermDate, toTermDate, fromUpdateExitDate, toUpdateExitDate, fromUpdateDate, toUpdateDate, fromUpdateTermDate, toUpdateTermDate);

                lines_counter = 0;
                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    Decimal sibsud, subsid_vat, trav_pay, vat_percent, inv_amount;
                    int mispar_tipulim = 0, income_type = 0;
                    int voucher_melavim_number, file_melavim_number; // initial values
                    int melavim_le_be_tashlum_number = 0, melavim_be_tashlum_number = 0; // calculated values
                    string bitul_indication, tikun_mimush, group_code, sug_eruach, service_type;
                    DateTime from_date, to_date, issue_date, critical_update_date, gov_start_makat_date, voucher_date, cancel_date;
                    string gov_indicator_id, voucher_reference;

                    string docket_id = row["docket_id"].ToString();
                    string voucher_id = row["voucher_id"].ToString();
                    string bundle_id = row["bundle_id"].ToString();

                    voucher_reference = row["gov_referral_number"].ToString();
                    service_type = row["service_type"].ToString();
                    gov_indicator_id = row["gov_indicator_id"].ToString();
                    sug_eruach = Getsug_eruach(service_type, bundle_id);

                    string supplier_hotel_id = row["pay_to_supplier_id"].ToString();
                    string supplier_name = row["supplier_name"].ToString();
                    string dov_area_code = row["gov_area_code"].ToString();

                    string clerk_login_name = row["clerk_login_name"].ToString();
                    if (clerk_login_name.Length > 9) clerk_login_name = clerk_login_name.Substring(0, 9);

                    int agn_area_id = int.Parse(row["area_id"].ToString());
                    // gov traveller details
                    string traveler_id = Utils.AddAbsentSignsToTheBeginningOfTheString(row["id_no"].ToString(), 9, "0");
                    string gov_makat_number = Utils.AddAbsentSignsToTheBeginningOfTheString(row["gov_makat_number"].ToString(), 6, "0");
                    string gov_docket_id = Utils.AddAbsentSignsToTheBeginningOfTheString(row["gov_docket_id"].ToString(), 9, "0");

                    string gov_balance_ussage = row["gov_balance_ussage"].ToString();
                    string level = row["gov_supplier_code"].ToString();
                    string remark = row["remark"].ToString();
                    string gov_area_code = row["gov_area_code"].ToString();

                    int cinvoiceNum = 0;
                    
                    DateTime invCDate;

                    int.TryParse(row["invoice_id"].ToString(), out cinvoiceNum);
                     
                    DateTime.TryParse(row["inv_cdate"].ToString(), out invCDate);

                    string strMelave = "Select count(BTT.traveller_id) as counter,BTT.bundle_id from BUNDLES_to_TRAVELLERS AS BTT where BTT.bundle_id = " + bundle_id + " and (BTT.trav_pay=0 and BTT.subsid=0) group by BTT.bundle_id";
                    try
                    {
                        voucher_melavim_number = int.Parse(DAL_SQL.RunSql(strMelave));
                    }
                    catch (Exception)
                    {
                        voucher_melavim_number = 0;
                    }
                    file_melavim_number = int.Parse(row["tr_melave_number"].ToString());
                    string fifthNight_pay = row["fifthNight_pay"].ToString();
                    string melave_selected_nights = row["melave_selected_nights"].ToString();
                    string gov_invoice_id = row["gov_invoice_id"].ToString();

                    DateTime.TryParse(row["gov_voucher_cancel_date"].ToString(), out cancel_date);
                    DateTime.TryParse(row["value_date"].ToString(), out voucher_date);
                    DateTime.TryParse(row["from_date"].ToString(), out from_date);
                    DateTime.TryParse(row["to_date"].ToString(), out to_date);
                    DateTime.TryParse(row["issue_date"].ToString(), out issue_date);
                    DateTime.TryParse(row["critical_update_date"].ToString(), out critical_update_date);
                    DateTime.TryParse(row["gov_start_makat_date"].ToString(), out gov_start_makat_date);
                    Decimal.TryParse(row["subsid"].ToString(), out sibsud);
                    Decimal.TryParse(row["vat_percent"].ToString(), out vat_percent);
                    Decimal.TryParse(row["trav_pay"].ToString(), out trav_pay);
                    Decimal.TryParse(row["inv_amount"].ToString(), out inv_amount);
                    int.TryParse(row["income_type"].ToString(), out income_type);

                    // check if to zero vat by income type
                    if (income_type != 3 && income_type != 6) { // have vat
                        subsid_vat = sibsud - (sibsud / ((vat_percent / 100) + 1));
                    } else {
                        subsid_vat = 0;
                        vat_percent = 0;
                    }

                    TimeSpan ts = to_date - from_date;
                    int nights = ts.Days;

                    if (gov_indicator_id == "5" && nights == 5) // added at 2014.01.06 new logic
                    {
                        nights = nights - 1;
                        to_date = to_date.AddDays(-1);
                        if (melave_selected_nights.Length >= 5)
                        {
                            melave_selected_nights = melave_selected_nights.Substring(0, 4);
                        }
                    }

                    // added at 2013.11.18 - decided with Ilan
                    voucher_date = issue_date;


                    group_code = "0";
                    bitul_indication = (row["setId"].ToString() == "2" && (row["voucher_status"].ToString() == "2" || row["service_status_id"].ToString() == "5")) ? "1" : "0";
                    tikun_mimush = "0";
                    
                    int rooms = 0;
                    if (row["service_type"].ToString() == "2") { // hotel order
                        string strRooms = DAL_SQL_Helper.GetRoomsNumByBundle(Int32.Parse(bundle_id));
                        rooms = 0;
                        int.TryParse(strRooms, out rooms);
                        
                        if (gov_makat_number == "027240")
                        {
                            if (gov_balance_ussage == "1" || gov_balance_ussage.ToLower() == "true")
                            {
                                melavim_le_be_tashlum_number = 1;
                                melavim_be_tashlum_number = 0;
                            }
                            else
                            {
                                melavim_le_be_tashlum_number = 0;
                                melavim_be_tashlum_number = 1; //earlier was = voucher_melavim_number
                            }
                        }
                        else
                        {
                            melavim_be_tashlum_number = 0;
                            melavim_le_be_tashlum_number = 0;
                            Utils.MelaveTashlumDefiner(voucher_melavim_number, file_melavim_number, rooms, ref melavim_be_tashlum_number, ref melavim_le_be_tashlum_number);
                        }
                    } else {
                        rooms = 0;
                        mispar_tipulim = nights;
                        melavim_be_tashlum_number = melavim_le_be_tashlum_number = 0;

                    }

                    result.Append(getFileLine(reportType, gov_docket_id, traveler_id, gov_makat_number, from_date, to_date, mispar_tipulim, sibsud, voucher_id, bitul_indication, supplier_hotel_id, supplier_name, melavim_be_tashlum_number, melavim_le_be_tashlum_number, level, group_code, gov_area_code, docket_id, tikun_mimush, trav_pay, melave_selected_nights, critical_update_date, vat_percent, bundle_id, fifthNight_pay, clerk_login_name, voucher_date, sug_eruach, rooms, service_type, cancel_date, gov_indicator_id, voucher_reference, cinvoiceNum, invCDate, subsid_vat, gov_invoice_id, inv_amount));
                    lines_counter++;
                }

                result.Append("000000000" + delimiter + lines_counter);

                StringWriter oStringWriter = new StringWriter();
                oStringWriter.WriteLine(result.ToString());
                Response.ContentType = "text/plain";
                string fileName = "Heshboniot_" + DateTime.Today.Year.ToString("0000") + DateTime.Today.Month.ToString("00") + "_" + DateTime.Today.Year.ToString("0000") + DateTime.Today.Month.ToString("00") + DateTime.Today.Day.ToString("00") + ".txt";
                Response.AddHeader("content-disposition", "attachment;filename=" + fileName);
                Response.Clear();

                using (StreamWriter writer = new StreamWriter(Response.OutputStream, Encoding.ASCII))
                {
                    writer.Write(oStringWriter.ToString());
                }
				
				Response.End();
            }
	}
	
    private string getFileLine(int reportType, string gov_docket_id, string traveler_id, string gov_makat_number, DateTime from_date, DateTime to_date,
                                int mispar_tipulim, decimal sibsud, string voucher_id, string bitul_indication, string supplier_hotel_id,
                                string supplier_name, int melavim_be_tashlum_number, int melavim_le_be_tashlum_number, string level, string group_code,
                                string gov_area_code, string agn_docket_id, string tikun_mimush, Decimal trav_pay, string melave_selected_nights,
                                DateTime critical_update_days, Decimal vat_percent, string bundle_id, string fifthNightPay, string clerk_login_name,
                                DateTime voucher_date, string sug_eruach, int rooms, string service_type, DateTime cancel_date, string gov_indicator_id,
                                string voucher_reference, int cinvoiceNum, DateTime invCDate
        , Decimal subsid_vat
        , string gov_invoice_id
        , Decimal inv_amount
        )
    {
        const string msgType = "careinvoice";
        const string msgMapCode = "M18";
        const string supplier_id = "83695749";//Shikum
        const string msgSender = "PILSTI";
        const string msgReceiver = "SHIKUM";
        const string docType = "KR";
        string fileDate = formatDateToGov(DateTime.Now);
        string msgAppKey = string.Empty;
        string invoiceType = string.Empty;
        string strInvCDate = formatDateToGov(invCDate);
        string cinvPeriod = from_date.Year.ToString("0000") + from_date.Month.ToString("00") + "01";
        string write_makat_number = string.Empty;
        int UOM = 0;
        int msgDocNum = 0;
        int nights = 0;
        Double SuppDiscount = 0.00;
        TimeSpan ts = to_date - from_date;
        
        nights = Math.Abs(ts.Days);
        StringBuilder Line = new StringBuilder();

        if (gov_indicator_id == "1" || gov_indicator_id == "6" || gov_indicator_id == "7" || gov_indicator_id == "10") 
		{
            SuppDiscount = 50.00;
        }
		else if (gov_indicator_id == "9")
		{
			SuppDiscount = 100.00;
		}
        // getting the current number
        try {
            msgDocNum = int.Parse(DAL_SQL.RunSql("update [GOV_COUNTERS] set counter=counter+1 where name='invoice_new_file'; select counter from [GOV_COUNTERS] where name='invoice_new_file'"));
        } catch (Exception) {
            msgDocNum = 0;
        }
        
        // 8 + x(x=14) + 8 = 30
        msgAppKey = supplier_id + Utils.AddAbsentSignsToTheBeginningOfTheString(cinvoiceNum.ToString(), 14, "0") + fileDate;

        invoiceType = "395";
		string selectColumn = string.Empty;
		
        if (service_type == "2")
		{ // hotel
            UOM = 3;
			selectColumn = "WriteToAccLogFirstRoom";
        } 
		else 
		{ // other
            UOM = 27;
			selectColumn = "WriteToAccLogTipulim";
        }
		
		write_makat_number = DAL_SQL.GetRecord("GOV_MAKATS", selectColumn, gov_makat_number, "MakatNumber");
		
        if (gov_makat_number != "027240") {
            trav_pay = 0;
        }

        /*1 */Line.Append(Utils.AddAbsentSignsToTheBeginningOfTheString(msgType, 11, "0") + delimiter);//msgtype
        /*2 */Line.Append(Utils.AddAbsentSignsToTheBeginningOfTheString(msgMapCode, 3, "0") + delimiter);//msgMapCode
        /*3 */Line.Append(Utils.AddAbsentSignsToTheBeginningOfTheString(msgDocNum.ToString(), 20, "0") + delimiter);//msgMapCode
        /*4 */Line.Append(Utils.AddAbsentSignsToTheBeginningOfTheString(msgAppKey, 30, "0") + delimiter);//msgAppKey
        /*5 */Line.Append(Utils.AddAbsentSignsToTheBeginningOfTheString(msgSender, 6, "0") + delimiter);//msgSender
        /*6 */Line.Append(Utils.AddAbsentSignsToTheBeginningOfTheString(msgReceiver, 6, "0") + delimiter);//msgReceiver
        /*7 */Line.Append(Utils.AddAbsentSignsToTheBeginningOfTheString(fileDate, 8, "0") + delimiter);//msgCreDate
        /*8 */Line.Append(Utils.AddAbsentSignsToTheBeginningOfTheString("000000", 6, "0") + delimiter);//msgCreTime
        /*9 */Line.Append(Utils.AddAbsentSignsToTheBeginningOfTheString(docType, 2, "0") + delimiter);//docType
        /*10*/Line.Append(Utils.AddAbsentSignsToTheBeginningOfTheString(invoiceType, 3, "0") + delimiter);//invoiceType
        /*11*/Line.Append(Utils.AddAbsentSignsToTheBeginningOfTheString("9", 3, "0") + delimiter);//actionType
        /*12*/Line.Append(Utils.AddAbsentSignsToTheBeginningOfTheString("1", 1, "0") + delimiter);//cinvoiceRefBy
        /*13*/Line.Append(Utils.AddAbsentSignsToTheBeginningOfTheString(cinvoiceNum.ToString(), 14, "0") + delimiter);//cinvoiceNum
        /*14*/Line.Append(Utils.AddAbsentSignsToTheBeginningOfTheString(cinvPeriod, 8, "0") + delimiter);//cinvPeriod
        
        /*15*/Line.Append(Utils.AddAbsentSignsToTheBeginningOfTheString(fileDate, 8, "0") + delimiter);//msgCreationDate
        /*16*/Line.Append(Utils.AddAbsentSignsToTheBeginningOfTheString(strInvCDate, 8, "0") + delimiter);//invCDate
        /*17*/Line.Append(Utils.AddAbsentSignsToTheBeginningOfTheString("S", 1, "0") + delimiter);//LiaisonUnit
        /*18*/Line.Append(Utils.AddAbsentSignsToTheBeginningOfTheString(supplier_id, 8, "0") + delimiter);//modNumber
        /*19*/Line.Append(Utils.AddAbsentSignsToTheBeginningOfTheString(Math.Round(sibsud, 2).ToString(".00"), 14, "0") + delimiter);
        /*20*/Line.Append(Utils.AddAbsentSignsToTheBeginningOfTheString(Math.Round(vat_percent, 2).ToString(".00"), 5, "0") + delimiter);
        /*21*/Line.Append(Utils.AddAbsentSignsToTheBeginningOfTheString(Math.Round(subsid_vat, 2).ToString(".00"), 14, "0") + delimiter);
		/*22*/Line.Append(Utils.AddAbsentSignsToTheBeginningOfTheString("ILS", 3, "0") + delimiter);
        
        /*23*/Line.Append(Utils.AddAbsentSignsToTheBeginningOfTheString(voucher_reference, 12, "0") + delimiter);
        /*24*/Line.Append(Utils.AddAbsentSignsToTheBeginningOfTheString(voucher_id, 9, "0") + delimiter);
        /*25*/Line.Append(Utils.AddAbsentSignsToTheBeginningOfTheString("0", 1, "0") + delimiter);
        /*26*/Line.Append(Utils.AddAbsentSignsToTheBeginningOfTheString(traveler_id, 9, "0") + delimiter);
        /*27*/Line.Append(Utils.AddAbsentSignsToTheBeginningOfTheString(gov_docket_id, 9, "0") + delimiter);
        /*28*/Line.Append(Utils.AddAbsentSignsToTheBeginningOfTheString(write_makat_number, 6, "0") + delimiter);
        /*29*/Line.Append(Utils.AddAbsentSignsToTheBeginningOfTheString(formatDateToGov(from_date), 8, "0") + delimiter);
        /*30*/Line.Append(Utils.AddAbsentSignsToTheBeginningOfTheString(formatDateToGov(to_date), 8, "0") + delimiter);
        /*31*/Line.Append(Utils.AddAbsentSignsToTheBeginningOfTheString(nights.ToString(), 2, "0") + delimiter);
        /*32*/Line.Append(Utils.AddAbsentSignsToTheBeginningOfTheString(gov_indicator_id, 2, "0") + delimiter);
        /*33*/Line.Append(Utils.AddAbsentSignsToTheBeginningOfTheString(UOM.ToString(), 5, "0") + delimiter);
        /*34*/Line.Append(Utils.AddAbsentSignsToTheBeginningOfTheString(Math.Round(SuppDiscount, 2).ToString(".00"), 6, "0") + delimiter);
        /*35*/Line.Append(Utils.AddAbsentSignsToTheBeginningOfTheString(melavim_le_be_tashlum_number.ToString(), 1, "0") + delimiter);
        /*36*/Line.Append(Utils.AddAbsentSignsToTheBeginningOfTheString(melavim_be_tashlum_number.ToString(), 1, "0") + delimiter);
        /*37*/Line.Append(Utils.AddAbsentSignsToTheBeginningOfTheString(Math.Round(trav_pay, 2).ToString(".00"), 16, "0") + delimiter);
        /*38*/Line.Append(Utils.AddAbsentSignsToTheBeginningOfTheString(agn_docket_id, 10, "0") + delimiter);
        /*39*/Line.Append(Utils.AddAbsentSignsToTheBeginningOfTheString(supplier_hotel_id, 5, "0") + delimiter);
        /*40*/Line.Append(Utils.AddAbsentSignsToTheBeginningOfTheString(gov_area_code, 2, "0") + delimiter);
        /*41*/Line.Append(Utils.AddAbsentSignsToTheBeginningOfTheString(sug_eruach, 3, "0") + delimiter);
        /*42*/Line.Append(Utils.AddAbsentSignsToTheBeginningOfTheString(formatMelaveLeBeTashlumSelectedNightsString(melavim_le_be_tashlum_number, nights), 31, " ", CutFrom.End, AddTo.End) + delimiter);
        /*43*/Line.Append(Utils.AddAbsentSignsToTheBeginningOfTheString(formatMelaveBeTashlumSelectedNightsString(melave_selected_nights, melavim_be_tashlum_number, nights), 31, " ", CutFrom.End, AddTo.End) + delimiter);
        /*44*/Line.Append(Utils.AddAbsentSignsToTheBeginningOfTheString(Math.Round(0.00, 2).ToString(".00"), 6, "0") + delimiter); //hotelprcntholiday
         if (Request["ddlReportType"] == "1")
        {
            Line.Append(Utils.AddAbsentSignsToTheBeginningOfTheString(gov_invoice_id, 15, "0") + delimiter);//bc_invoice_preforma_id     
            Line.Append(Utils.AddAbsentSignsToTheBeginningOfTheString(Math.Round(inv_amount, 2).ToString(".00"), 10, "0") + delimiter); //hotelprcntholiday
        }
       
        /*if (reportType == 1)
        {
        }*/

        Line.Append(Environment.NewLine);
        return Line.ToString();
    }

    private string formatMelaveLeBeTashlumSelectedNightsString(int melavim_number, int nights)
    {
        string melave_selected_nights = string.Empty;
        for (int i = 0; i < nights; i++)
        {
            melave_selected_nights = melave_selected_nights + melavim_number.ToString();
        }
        return melave_selected_nights;
    }
    private string formatMelaveBeTashlumSelectedNightsString(string melave_selected_nights, int melavim_number, int nights)
    {
        if (melave_selected_nights.Length <= 0)
        {
            for (int i = 0; i < nights; i++)
            {
                melave_selected_nights = melave_selected_nights + "0";
            }
        }
        return melave_selected_nights;
    }

    // format date from 08-Mar-12 to 20120308
    private string formatDateToGov(DateTime date)
    {
        if (date.Year == 1)
            return "00000000";
        else
            return date.Year.ToString("0000") + date.Month.ToString("00") + date.Day.ToString("00");
    }
    // format date from 08-Mar-12 to 08032012000000
    private string formatDatetimeToGov(DateTime date)
    {
        return date.Year.ToString("0000") + date.Month.ToString("00") + date.Day.ToString("00") + date.Hour.ToString("00") + date.Minute.ToString("00") + date.Second.ToString("00");
    }

    private string Getsug_eruach(string service_type, string bundle_id)
    {
        try
        {
            return DAL_SQL.GetRecord("[HOTELS_TO_BASES_TYPE] HTBT inner join Agency_Admin.dbo.HOTEL_ON_BASE HOB on HOB.id=HTBT.base_type_id", "top 1 isnull(HOB.id, 0)", bundle_id, "bundle_id");
        }
        catch (Exception)
        {
            return "00";
        }
    }
}
