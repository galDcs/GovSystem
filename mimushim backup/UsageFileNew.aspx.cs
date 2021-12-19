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
using System.Xml;

public partial class UsageFileNew : System.Web.UI.Page
{
    //private int display_counter_bad = 0;
    //private int display_counter_good = 0;
    private string delimiter = ";";
    private int lines_counter = 0;
    
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!Utils.CheckSecurity(228)) Response.Redirect("AccessDenied.aspx");
       
        if (Page.IsPostBack)
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



            int reportType = 0;

            if (Request["ddlReportType"] != null && !String.IsNullOrEmpty(Request["ddlReportType"]))
            {
                int.TryParse(Request["ddlReportType"], out reportType);
            }


            if (((Request["txtFromExitDate"] != "" && Request["txtToExitDate"] != "") ||
                   (Request["txtToTermDate"] != "" && Request["txtFromTermDate"] != ""))
                            ||

                 ((Request["txtUpdateFromDate"] != "" && Request["txtUpdateToDate"] != "") &&
                   ((Request["txtUpdateFromExitDate"] != "" && Request["txtUpdateToExitDate"] != "")
                       ||
                   (Request["txtUpdateFromTermDate"] != "" && Request["txtUpdateToTermDate"] != ""))))
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

                DataSet ds = DAL_SQL_Helper.GetUsageFileNew(fromExitDate, toExitDate, fromTermDate, toTermDate, fromUpdateExitDate, toUpdateExitDate, fromUpdateDate, toUpdateDate, fromUpdateTermDate, toUpdateTermDate);

                lines_counter = 0;
                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    Decimal sibsud, trav_pay, vat_percent, amount;
                    int mispar_tipulim, balance_nights;
                    int voucher_melavim_number, file_melavim_number; // initial values
                    int melavim_le_be_tashlum_number, melavim_be_tashlum_number; // calculated values
                    string bitul_indication, tikun_mimush, group_code, sug_eruach, service_type;
                    DateTime from_date, to_date, issue_date, critical_update_date, gov_start_makat_date, voucher_date, cancel_date;
                    string gov_indicator_id, voucher_reference, trav_remark;

                    string docket_id    = row["docket_id"].ToString();
                    string voucher_id   = row["voucher_id"].ToString();
                    string bundle_id    = row["bundle_id"].ToString();
                    voucher_reference = row["gov_connected_voucher_number"].ToString();
                    service_type = row["service_type"].ToString();
                    // B.gov_connected_voucher_number,
                    gov_indicator_id = row["gov_indicator_id"].ToString();
                    sug_eruach = Getsug_eruach(service_type, bundle_id);

                    trav_remark = (gov_indicator_id == "8") ? row["trav_remark"].ToString() : string.Empty;

                    string supplier_hotel_id = row["pay_to_supplier_id"].ToString();
                    string supplier_name = row["supplier_name"].ToString();
                    string dov_area_code = row["gov_area_code"].ToString();

                    /* aviran 28/08 - new field - name of area code - hebrew */
                    string gov_area_name = row["name_1255"].ToString();
                    string supplier_name_hebrew = row["supplier_name_hebrew"].ToString();
                    /* /aviran */

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

                    DateTime.TryParse(row["gov_voucher_cancel_date"].ToString(), out cancel_date);
                    DateTime.TryParse(row["value_date"].ToString(), out voucher_date);
                    DateTime.TryParse(row["from_date"].ToString(), out from_date);
                    DateTime.TryParse(row["to_date"].ToString(), out to_date);
                    DateTime.TryParse(row["issue_date"].ToString(), out issue_date);
                    DateTime.TryParse(row["critical_update_date"].ToString(), out critical_update_date);
                    DateTime.TryParse(row["gov_start_makat_date"].ToString(), out gov_start_makat_date);
                    Decimal.TryParse(row["subsid"].ToString(), out sibsud);
                    Decimal.TryParse(row["amount"].ToString(), out amount);
                    Decimal.TryParse(row["vat_percent"].ToString(), out vat_percent);
                    Decimal.TryParse(row["trav_pay"].ToString(), out trav_pay);

                    // added at 2013.11.18 - decided with Ilan
                    voucher_date = issue_date;
                    // added at 2014.06.23 - 2 fields
                    string gov_referral_number = row["gov_referral_number"].ToString();
                    string bc_invoice_preforma_id = row["bc_invoice_preforma_id"].ToString();
                    string invoice_id = row["invoice_id"].ToString();
                    string gov_invoice_id = row["gov_invoice_id"].ToString();

                    string melave_selected_nights = row["melave_selected_nights"].ToString();

                    if (vat_percent > 0)
                    {
                        sibsud = sibsud / (vat_percent / 100 + 1);//sibsud
                        trav_pay = trav_pay / (vat_percent / 100 + 1);
                    }

                    if (gov_makat_number != "027240")
                    {
                        trav_pay = 0;
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


                    group_code = "0";
                    bitul_indication = (row["setId"].ToString() == "2" && (row["voucher_status"].ToString() == "2" || row["service_status_id"].ToString() == "5")) ? "1" : "0";
                    tikun_mimush = "0";
                    if (row["setId"].ToString() == "2")
                    {
                        if (issue_date.Day != critical_update_date.Day || issue_date.Month != critical_update_date.Month || issue_date.Year != critical_update_date.Year)
                        {
                            tikun_mimush = "1";
                        }
                    }

                    balance_nights = nights;

                    // for export file takes voucher_id unique (see the logic inside stp)
                    // voucher_id = row["voucher_id_un"].ToString();

                    if (row["service_type"].ToString() == "2") // hotel order
                    {
                        int rooms = 0;

                        string strRooms = DAL_SQL_Helper.GetRoomsNumByBundle(Int32.Parse(bundle_id));
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


                        mispar_tipulim = 0;
                       
                        if (gov_makat_number == "027235" || gov_makat_number == "027236") // need to write secondary makat number by rooms amount and tipulim
                        {
                            string write_makat_number_hotel = string.Empty;
                            sibsud = sibsud;//sibsud is 100% 
                            write_makat_number_hotel = DAL_SQL.GetRecord("GOV_MAKATS", "WriteToAccLogFirstRoom", gov_makat_number, "MakatNumber");
                            result.Append(getFileLine(reportType, gov_docket_id, traveler_id, write_makat_number_hotel, from_date, to_date, balance_nights, mispar_tipulim, sibsud, voucher_id, bitul_indication, supplier_hotel_id, supplier_name, melavim_be_tashlum_number, melavim_le_be_tashlum_number, level, group_code, gov_area_code, docket_id, tikun_mimush, trav_pay, melave_selected_nights, critical_update_date, vat_percent, bundle_id, fifthNight_pay, clerk_login_name, voucher_date, sug_eruach, rooms, service_type, cancel_date, gov_indicator_id, voucher_reference, trav_remark, gov_referral_number, bc_invoice_preforma_id, gov_invoice_id, amount, invoice_id /*aviran->*/, gov_area_name, supplier_name_hebrew));
                            lines_counter++;
                        }
                        else
                        {
                            result.Append(getFileLine(reportType, gov_docket_id, traveler_id, gov_makat_number, from_date, to_date, nights, mispar_tipulim, sibsud, voucher_id, bitul_indication, supplier_hotel_id, supplier_name, melavim_be_tashlum_number, melavim_le_be_tashlum_number, level, group_code, gov_area_code, docket_id, tikun_mimush, trav_pay, melave_selected_nights, critical_update_date, vat_percent, bundle_id, fifthNight_pay, clerk_login_name, voucher_date, sug_eruach, rooms, service_type, cancel_date, gov_indicator_id, voucher_reference, trav_remark, gov_referral_number, bc_invoice_preforma_id, gov_invoice_id, amount, invoice_id /*aviran->*/, gov_area_name, supplier_name_hebrew));
                            lines_counter++;
                        }
                    }
                    else
                    {
                        if (row["service_type"].ToString() == "8")
                        {
                            mispar_tipulim = nights;
                            melavim_be_tashlum_number = melavim_le_be_tashlum_number = 0;
                            if (gov_makat_number == "027235" || gov_makat_number == "027236") // need to write secondary makat for tipulim
                            {
                                string write_makat_number_other = string.Empty;
                                write_makat_number_other = DAL_SQL.GetRecord("GOV_MAKATS", "WriteToAccLogTipulim", gov_makat_number, "MakatNumber");
                                result.Append(getFileLine(reportType, gov_docket_id, traveler_id, write_makat_number_other, from_date, to_date, balance_nights, mispar_tipulim, sibsud, voucher_id, bitul_indication, supplier_hotel_id, supplier_name, melavim_be_tashlum_number, melavim_le_be_tashlum_number, "4", group_code, gov_area_code, docket_id, tikun_mimush, trav_pay, melave_selected_nights, critical_update_date, vat_percent, bundle_id, fifthNight_pay, clerk_login_name, voucher_date, sug_eruach, 0, service_type, cancel_date, gov_indicator_id, voucher_reference, trav_remark, gov_referral_number, bc_invoice_preforma_id, gov_invoice_id, amount, invoice_id/*aviran->*/, gov_area_name, supplier_name_hebrew)); // other vouchers always 4 not level
                                lines_counter++;
                            }
                            else
                            {
                                result.Append(getFileLine(reportType, gov_docket_id, traveler_id, gov_makat_number, from_date, to_date, balance_nights, mispar_tipulim, sibsud, voucher_id, bitul_indication, supplier_hotel_id, supplier_name, melavim_be_tashlum_number, melavim_le_be_tashlum_number, "4", group_code, gov_area_code, docket_id, tikun_mimush, trav_pay, melave_selected_nights, critical_update_date, vat_percent, bundle_id, fifthNight_pay, clerk_login_name, voucher_date, sug_eruach, 0, service_type, cancel_date, gov_indicator_id, voucher_reference, trav_remark, gov_referral_number, bc_invoice_preforma_id, gov_invoice_id, amount, invoice_id/*aviran->*/, gov_area_name, supplier_name_hebrew)); // other vouchers always 4 not level
                                lines_counter++;
                            }
                        }
                    }
                }

                result.Append("000000000" + delimiter + lines_counter);

                StringWriter oStringWriter = new StringWriter();
                oStringWriter.WriteLine(result.ToString());
                Response.ContentType = "text/plain";
                string fileName = "Mimushim_New_" + DateTime.Today.Year.ToString("0000") + DateTime.Today.Month.ToString("00") + DateTime.Today.Day.ToString("00") + ".txt";
                Response.AddHeader("content-disposition", "attachment;filename=" + fileName);
                Response.Clear();

                //using (StreamWriter writer = new StreamWriter(Response.OutputStream, Encoding.ASCII))
                using (StreamWriter writer = new StreamWriter(Response.OutputStream, Encoding.UTF8))
                {
                    writer.Write(oStringWriter.ToString());
                }
                Response.End();
            }
        }
    }

    private string getFileLine(int reportType, string gov_docket_id, string traveler_id, string gov_makat_number, DateTime from_date, DateTime to_date,
                                int balance_nights, int mispar_tipulim, decimal sibsud, string voucher_id, string bitul_indication, string supplier_hotel_id,
                                string supplier_name, int melavim_be_tashlum_number, int melavim_le_be_tashlum_number, string level, string group_code,
                                string gov_area_code, string agn_docket_id, string tikun_mimush
                                , Decimal trav_pay, string melave_selected_nights
                                , DateTime critical_update_days
                                , Decimal vat_percent
                                , string bundle_id, string fifthNightPay, string clerk_login_name,
                                DateTime voucher_date, string sug_eruach, int rooms, string service_type,DateTime cancel_date ,string gov_indicator_id,
                                string voucher_reference, string trav_remark
        , string gov_referral_number
        , string bc_invoice_preforma_id
        , string gov_invoice_id
        , Decimal amount
		, string invoice_id
        , string gov_area_name
        ,string supplier_name_hebrew
        )
    {
        string supplier_id = "0083360021"; //const
        string ordered_by_code = "03"; //const
        
        /* aviran 28.08 - new field to suit the new format*/
        string ordered_by_name = "שיקום"; //const
        /* /aviran*/
        
        StringBuilder Line = new StringBuilder();
        Line.Append(Utils.AddAbsentSignsToTheBeginningOfTheString(tikun_mimush, 1, "0") + delimiter);//s_action_code
        Line.Append(Utils.AddAbsentSignsToTheBeginningOfTheString(voucher_id, 9, "0") + delimiter);//s_voucher_number
        /*aviran 28.08 - new field added to the file*/
        Line.Append(Utils.AddAbsentSignsToTheBeginningOfTheString(ordered_by_name, 20, " ", CutFrom.End, AddTo.End) + delimiter);//ordered_by_name - Shikum
        /* /aviran */
        Line.Append(Utils.AddAbsentSignsToTheBeginningOfTheString(ordered_by_code, 2, "0") + delimiter);//const      
        Line.Append(Utils.AddAbsentSignsToTheBeginningOfTheString(formatDatetimeToGov(voucher_date), 14, "0") + delimiter);//voucher_date
        Line.Append(Utils.AddAbsentSignsToTheBeginningOfTheString(supplier_id, 10, "0") + delimiter);//const

        Line.Append(Utils.AddAbsentSignsToTheBeginningOfTheString(gov_area_code, 2, "0") + delimiter);//regionid
        /*aviran 28.08 - new field added to the file*/
        Line.Append(Utils.AddAbsentSignsToTheBeginningOfTheString(gov_area_name, 20, " ", CutFrom.End, AddTo.End) + delimiter);//area name
        /* /aviran */
        Line.Append(Utils.AddAbsentSignsToTheBeginningOfTheString(Utils.GetLastdigit(supplier_hotel_id, 4), 5, "0") + delimiter);//supplier_hotel_id
        /*aviran 28.08 - new field added to the file*/
        Line.Append(Utils.AddAbsentSignsToTheBeginningOfTheString(supplier_name_hebrew, 30, " ", CutFrom.End, AddTo.End) + delimiter);//area name
        /* /aviran */
        
        Line.Append(Utils.AddAbsentSignsToTheBeginningOfTheString(gov_docket_id, 9, "0") + delimiter);//s_file_number
        Line.Append(Utils.AddAbsentSignsToTheBeginningOfTheString(traveler_id, 9, "0") + delimiter);//s_id_number
        Line.Append(Utils.AddAbsentSignsToTheBeginningOfTheString(rooms.ToString(), 2, "0") + delimiter);//total_rooms
        Line.Append(Utils.AddAbsentSignsToTheBeginningOfTheString(sug_eruach, 2, "0") + delimiter);//sug_eruach       ???? need fix value

        Line.Append(Utils.AddAbsentSignsToTheBeginningOfTheString(melavim_be_tashlum_number.ToString(), 1, "0") + delimiter);//s_personal_companions
        Line.Append(Utils.AddAbsentSignsToTheBeginningOfTheString(melavim_le_be_tashlum_number.ToString(), 1, "0") + delimiter);//s_org_companions

        //Line.Append(Utils.AddAbsentSignsToTheBeginningOfTheString(service_type, 6, "0") + delimiter);//s_service_number
        Line.Append(Utils.AddAbsentSignsToTheBeginningOfTheString(gov_makat_number, 6, "0") + delimiter);
        Line.Append(formatDateToGov(from_date) + delimiter);//holiday_start_date
        Line.Append(formatDateToGov(to_date) + delimiter);//holiday_end_date
        Line.Append(Utils.AddAbsentSignsToTheBeginningOfTheString(balance_nights.ToString(), 2, "0") + delimiter);//total_nights
        Line.Append(Utils.AddAbsentSignsToTheBeginningOfTheString(formatDateToGov(cancel_date),8,"0") + delimiter);//s_cancel_date         ???? need fix value
        Line.Append(Utils.AddAbsentSignsToTheBeginningOfTheString(gov_indicator_id, 3, "0") + delimiter);// ???? need fix value

        int nights = 0;
        TimeSpan ts = to_date - from_date;
        nights = Math.Abs(ts.Days);

        Line.Append(Utils.AddAbsentSignsToTheEndOfTheString(formatMelaveLeBeTashlumSelectedNightsString(melavim_le_be_tashlum_number, nights), 31, " ") + delimiter);//s_companion1_days
        Line.Append(Utils.AddAbsentSignsToTheEndOfTheString(formatMelaveBeTashlumSelectedNightsString(melave_selected_nights, melavim_be_tashlum_number, nights), 31, " ") + delimiter); ;//s_companion2_days

        Line.Append(Utils.AddAbsentSignsToTheBeginningOfTheString(voucher_reference, 9, "0") + delimiter);//s_voucher_reference     ???? need fix value

        Line.Append(Utils.AddAbsentSignsToTheBeginningOfTheString(trav_remark, 250, " ", CutFrom.End, AddTo.End) + delimiter); //trav_remark
       if (Request["ddlReportType"] == "1")
        {
			Line.Append(Utils.AddAbsentSignsToTheBeginningOfTheString(gov_referral_number, 12, "0") + delimiter);//gov_referral_number      
			Line.Append(Utils.AddAbsentSignsToTheBeginningOfTheString(invoice_id, 14, "0") + delimiter);//bc_invoice_preforma_id     
       
            Line.Append(Utils.AddAbsentSignsToTheBeginningOfTheString(gov_invoice_id, 15, "0") + delimiter);//bc_invoice_preforma_id     
        }
        // Line.Append(Utils.AddAbsentSignsToTheEndOfTheString(Utils.AddAbsentSignsToTheBeginningOfTheString(melave_selected_nights.Replace("1", melavim_be_tashlum_number != 0 ? melavim_be_tashlum_number.ToString() : "0"), balance_nights, "0") + delimiter), 31, " ")+ delimiter);;//s_companion2_days
        
        //Line.Append(Utils.AddAbsentSignsToTheBeginningOfTheString(mispar_tipulim.ToString(), 3, "0") + delimiter);
        //Line.Append(Utils.AddAbsentSignsToTheBeginningOfTheString(Math.Round(sibsud, 2).ToString(), 12, "0") + delimiter);
        //Line.Append(Utils.AddAbsentSignsToTheBeginningOfTheString(Utils.GetLastdigit(supplier_id, 4), 6, "0") + delimiter);
        //Line.Append(Utils.AddAbsentSignsToTheBeginningOfTheString(supplier_name, 20, " ") + delimiter);
        //Line.Append(Utils.AddAbsentSignsToTheBeginningOfTheString(level, 2, "0") + delimiter);
        //Line.Append(Utils.AddAbsentSignsToTheBeginningOfTheString(group_code, 2, "0") + delimiter);
        //Line.Append(Utils.AddAbsentSignsToTheBeginningOfTheString(agn_docket_id, 10, "0") + delimiter);
        //Line.Append(Utils.AddAbsentSignsToTheBeginningOfTheString(tikun_mimush, 1, "0") + delimiter);
        //Line.Append(Utils.AddAbsentSignsToTheBeginningOfTheString(Math.Round(trav_pay, 2).ToString(), 12, "0") + delimiter);

        if (reportType == 1)
        {
            //Line.Append(Utils.AddAbsentSignsToTheEndOfTheString(formatDateToGov(critical_update_days), 10, "0") + delimiter);
            //Line.Append(Utils.AddAbsentSignsToTheBeginningOfTheString(Math.Round(vat_percent, 2).ToString(), 5, "0") + delimiter);
            //Line.Append((fifthNightPay.ToLower() == "true" ? "1" : "0") + delimiter);
            //Line.Append(clerk_login_name + delimiter);
        }


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
        // commented at 2013.11.17 - Agreed with Ilan
        /*if (melavim_number == 0) {
            melave_selected_nights = string.Empty;
        }*/
        if (melave_selected_nights.Length <= 0) {
            for (int i = 0; i < nights; i++) {
                melave_selected_nights = melave_selected_nights + "0"; //melavim_number.ToString();
            }
        }
        return melave_selected_nights;
    }

    // format date from 08-Mar-12 to 20120308
    private string formatDateToGov(DateTime date)
    {
      if(date.Year==1 )
        return "00000000";
      else
        return date.Year.ToString() + date.Month.ToString("00") + date.Day.ToString("00");
    }
    // format date from 08-Mar-12 to 08032012000000
    private string formatDatetimeToGov(DateTime date)
    {
        return date.Year.ToString() + date.Month.ToString("00") + date.Day.ToString("00") + date.Hour.ToString("00") + date.Minute.ToString("00") + date.Second.ToString("00");
    }

    private string Getsug_eruach(string service_type, string bundle_id)
    {
        try
        {
            return DAL_SQL.GetRecord("[HOTELS_TO_BASES_TYPE] HTBT inner join Agency_Admin.dbo.HOTEL_ON_BASE HOB on HOB.id=HTBT.base_type_id", "top 1 isnull(HOB.gov_code, 0)", bundle_id, "bundle_id");
        }
        catch (Exception)
        {
            return "00";
        }
    }
}
