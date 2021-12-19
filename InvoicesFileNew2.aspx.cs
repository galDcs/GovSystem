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
        //if (!Utils.CheckSecurity(239)) Response.Redirect("AccessDenied.aspx");

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

                 
                DataSet ds = DAL_SQL_Helper.GetInvoicesForInvoicesFileNew(fromExitDate, toExitDate, fromTermDate, toTermDate, fromUpdateExitDate, toUpdateExitDate, fromUpdateDate, toUpdateDate, fromUpdateTermDate, toUpdateTermDate);

                lines_counter = 0;
                foreach (DataRow row in ds.Tables[0].Rows)
                {

                    Decimal sibsud, trav_pay, vat_percent;
                    int mispar_tipulim, balance_nights;
                    int voucher_melavim_number, file_melavim_number; // initial values
                    int melavim_le_be_tashlum_number, melavim_be_tashlum_number; // calculated values
                    string bitul_indication, tikun_mimush, group_code;
                    DateTime from_date, to_date, issue_date, critical_update_date, gov_start_makat_date;

                    string docket_id = row["docket_id"].ToString();
                    string voucher_id = row["voucher_id"].ToString();
                    string bundle_id = row["bundle_id"].ToString();

                    string supplier_id = row["pay_to_supplier_id"].ToString();
                    string supplier_name = row["supplier_name"].ToString();
                    string dov_area_code = row["gov_area_code"].ToString();


                    int agn_area_id = int.Parse(row["area_id"].ToString());
                    // gov traveller details
                    string traveler_id = Utils.AddAbsentSignsToTheBeginningOfTheString(row["id_no"].ToString(), 9, "0");
                    string gov_makat_number = Utils.AddAbsentSignsToTheBeginningOfTheString(row["gov_makat_number"].ToString(), 6, "0");
                    string gov_docket_id = Utils.AddAbsentSignsToTheBeginningOfTheString(row["gov_docket_id"].ToString(), 9, "0");

                    string gov_balance_ussage = row["gov_balance_ussage"].ToString();
                    string level = row["gov_supplier_code"].ToString();
                    string remark = row["remark"].ToString();
                    string gov_area_code = row["gov_area_code"].ToString();
                    string invoice_id = row["invoice_id"].ToString();
                    DateTime invoice_date;
                    Decimal invoice_amount; 
                    Decimal vat_amount;
                    string line_type = row["line_type"].ToString();

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


                    DateTime.TryParse(row["from_date"].ToString(), out from_date);
                    DateTime.TryParse(row["to_date"].ToString(), out to_date);
                    DateTime.TryParse(row["issue_date"].ToString(), out issue_date);
                    DateTime.TryParse(row["critical_update_date"].ToString(), out critical_update_date);
                    DateTime.TryParse(row["gov_start_makat_date"].ToString(), out gov_start_makat_date);
                    Decimal.TryParse(row["subsid"].ToString(), out sibsud);
                    Decimal.TryParse(row["vat_percent"].ToString(), out vat_percent);
                    Decimal.TryParse(row["trav_pay"].ToString(), out trav_pay);

                    DateTime.TryParse(row["invoice_date"].ToString(), out invoice_date);
                    Decimal.TryParse(row["invoice_amount"].ToString(), out invoice_amount);
                    Decimal.TryParse(row["vat_amount"].ToString(), out vat_amount);

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

                    group_code = "0";
                    bitul_indication = (row["setId"].ToString() == "2" && row["voucher_status"].ToString() == "2") ? "1" : "0";
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
                    voucher_id = row["voucher_id_un"].ToString();

                    string UOM = "0"; // yahidat mida sapak
                    if (row["service_type"].ToString() == "2") // hotel order
                    {
                        int rooms = 0;
                        UOM = "3";
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

                            result.Append(getFileLine(reportType, gov_docket_id, traveler_id, write_makat_number_hotel, from_date, to_date, balance_nights, mispar_tipulim, sibsud, voucher_id, bitul_indication, supplier_id, supplier_name, melavim_be_tashlum_number, melavim_le_be_tashlum_number, level, group_code, gov_area_code, docket_id, tikun_mimush, trav_pay, melave_selected_nights, critical_update_date, vat_percent, bundle_id, invoice_id, invoice_date, invoice_amount, vat_amount, line_type, UOM));
                            lines_counter++;
                        }
                        else
                        {
                            result.Append(getFileLine(reportType, gov_docket_id, traveler_id, gov_makat_number, from_date, to_date, nights, mispar_tipulim, sibsud, voucher_id, bitul_indication, supplier_id, supplier_name, melavim_be_tashlum_number, melavim_le_be_tashlum_number, level, group_code, gov_area_code, docket_id, tikun_mimush, trav_pay, melave_selected_nights, critical_update_date, vat_percent, bundle_id, invoice_id, invoice_date, invoice_amount, vat_amount, line_type, UOM));
                            lines_counter++;
                        }
                    }
                    else
                    {
                        if (row["service_type"].ToString() == "8")
                        {
                            UOM = "27";
                            mispar_tipulim = nights;
                            melavim_be_tashlum_number = melavim_le_be_tashlum_number = 0;
                            if (gov_makat_number == "027235" || gov_makat_number == "027236") // need to write secondary makat for tipulim
                            {
                                string write_makat_number_other = string.Empty;
                                write_makat_number_other = DAL_SQL.GetRecord("GOV_MAKATS", "WriteToAccLogTipulim", gov_makat_number, "MakatNumber");
                                result.Append(getFileLine(reportType, gov_docket_id, traveler_id, write_makat_number_other, from_date, to_date, balance_nights, mispar_tipulim, sibsud, voucher_id, bitul_indication, supplier_id, supplier_name, melavim_be_tashlum_number, melavim_le_be_tashlum_number, "4", group_code, gov_area_code, docket_id, tikun_mimush, trav_pay, melave_selected_nights, critical_update_date, vat_percent, bundle_id, invoice_id, invoice_date, invoice_amount, vat_amount, line_type, UOM)); // other vouchers always 4 not level
                                lines_counter++;
                            }
                            else
                            {
                                result.Append(getFileLine(reportType, gov_docket_id, traveler_id, gov_makat_number, from_date, to_date, balance_nights, mispar_tipulim, sibsud, voucher_id, bitul_indication, supplier_id, supplier_name, melavim_be_tashlum_number, melavim_le_be_tashlum_number, "4", group_code, gov_area_code, docket_id, tikun_mimush, trav_pay, melave_selected_nights, critical_update_date, vat_percent, bundle_id, invoice_id, invoice_date, invoice_amount, vat_amount, line_type, UOM)); // other vouchers always 4 not level
                                lines_counter++;
                            }
                        }
                    }
                }

                result.Append("000000000" + delimiter + lines_counter);

                StringWriter oStringWriter = new StringWriter();
                oStringWriter.WriteLine(result.ToString());
                Response.ContentType = "text/plain";

                Response.AddHeader("content-disposition", "attachment;filename=" + string.Format("{0:yyyyMMdd}", DateTime.Today) + "_mimushimFile.txt");
                Response.Clear();

                using (StreamWriter writer = new StreamWriter(Response.OutputStream, Encoding.ASCII))
                {
                    writer.Write(oStringWriter.ToString());
                }
                Response.End();
            }
        }
    }

    private string getFileLine(int reportType, string gov_docket_id, string traveler_id, string gov_makat_number, DateTime from_date, DateTime to_date,
                                int balance_nights, int mispar_tipulim, decimal sibsud, string voucher_id, string bitul_indication, string supplier_id,
                                string supplier_name, int melavim_be_tashlum_number, int melavim_le_be_tashlum_number, string level, string group_code,
                                string gov_area_code, string agn_docket_id, string tikun_mimush, Decimal trav_pay, string melave_selected_nights,
                                DateTime critical_update_days, Decimal vat_percent, string bundle_id,
                                string invoice_id, DateTime invoice_date, Decimal invoice_amount, Decimal vat_amount, string line_type, string UOM)
    {
        string dateFormat = "ddMMyyyy";
        gov_makat_number = (gov_makat_number.Length>5)?gov_makat_number.Substring(gov_makat_number.Length-5):gov_makat_number;

        StringBuilder Line = new StringBuilder();
        Line.Append(Utils.AddAbsentSignsToTheBeginningOfTheString(invoice_id, 9, "0") + delimiter);
        Line.Append(Utils.AddAbsentSignsToTheBeginningOfTheString(invoice_date.ToString(dateFormat), 8, "0") + delimiter);
        Line.Append(Utils.AddAbsentSignsToTheBeginningOfTheString(Math.Round(invoice_amount, 2).ToString(), 16, "0") + delimiter);
        Line.Append(Utils.AddAbsentSignsToTheBeginningOfTheString(Math.Round(vat_percent, 2).ToString(), 5, "0") + delimiter);
        Line.Append(Utils.AddAbsentSignsToTheBeginningOfTheString(Math.Round(vat_amount, 2).ToString(), 16, "0") + delimiter);
        Line.Append(Utils.AddAbsentSignsToTheBeginningOfTheString(voucher_id, 9, "0") + delimiter);
        Line.Append(Utils.AddAbsentSignsToTheBeginningOfTheString(line_type, 1, "0") + delimiter); //linetype
        Line.Append(Utils.AddAbsentSignsToTheBeginningOfTheString(traveler_id, 9, "0") + delimiter);
        Line.Append(Utils.AddAbsentSignsToTheBeginningOfTheString(gov_docket_id, 10, "0") + delimiter);//was 21      
        Line.Append(Utils.AddAbsentSignsToTheBeginningOfTheString(gov_makat_number, 6, "0") + delimiter);
        Line.Append(Utils.AddAbsentSignsToTheBeginningOfTheString(from_date.ToString(dateFormat), 8, "0") + delimiter);
        Line.Append(Utils.AddAbsentSignsToTheBeginningOfTheString(to_date.ToString(dateFormat), 8, "0") + delimiter);
        Line.Append(Utils.AddAbsentSignsToTheBeginningOfTheString(balance_nights.ToString(), 5, "0") + delimiter);
        Line.Append(Utils.AddAbsentSignsToTheBeginningOfTheString(UOM, 5, "0") + delimiter); //UOM
        Decimal UOMPrice = (sibsud != 0 && balance_nights !=0)? sibsud / balance_nights : 0;
        Line.Append(Utils.AddAbsentSignsToTheBeginningOfTheString(Math.Round(UOMPrice, 2).ToString(), 16, "0") + delimiter);
        Line.Append(Utils.AddAbsentSignsToTheBeginningOfTheString(Math.Round(sibsud, 2).ToString(), 16, "0") + delimiter);
        Line.Append(Utils.AddAbsentSignsToTheBeginningOfTheString(String.Empty, 20, " ") + delimiter); //RemTextLine
        Line.Append(Utils.AddAbsentSignsToTheBeginningOfTheString(melavim_le_be_tashlum_number.ToString(), 1, "0") + delimiter);
        Line.Append(Utils.AddAbsentSignsToTheBeginningOfTheString(melavim_be_tashlum_number.ToString(), 1, "0") + delimiter);
        Line.Append(Utils.AddAbsentSignsToTheBeginningOfTheString(Math.Round(trav_pay, 2).ToString(), 16, "0") + delimiter);
        Line.Append(Utils.AddAbsentSignsToTheBeginningOfTheString(agn_docket_id, 9, "0") + delimiter);//was 9
        Line.Append(Utils.AddAbsentSignsToTheBeginningOfTheString(Utils.GetLastdigit(supplier_id, 4), 6, "0") + delimiter);
        Line.Append(Utils.AddAbsentSignsToTheBeginningOfTheString(gov_area_code, 2, "0") + delimiter);
        Line.Append(Utils.AddAbsentSignsToTheBeginningOfTheString(level, 3, "0") + delimiter);
        Line.Append(Utils.AddAbsentSignsToTheEndOfTheString(
               (melavim_be_tashlum_number > 0 ?
                 GetExactMelaveSelectedNightsValue(melave_selected_nights, bundle_id, from_date, to_date) : melave_selected_nights)
                , 31, " ") + delimiter);
        Line.Append(Utils.AddAbsentSignsToTheBeginningOfTheString("000.00", 6, "0") + delimiter); // hotelprcntholiday TBA
        


        Line.Append(Environment.NewLine);
        return Line.ToString();
    }

    // format date from 08-Mar-12 to 08.03.2012
    private string formatDateToGov(DateTime date)
    {
        return date.Day.ToString("00") + "." + date.Month.ToString("00") + "." + date.Year.ToString();
    }

    //must be called only when there is melave_be_tashlum
    //if called with string with no "1" creates new string with "1"s and updates db. if db update fails, returns old string value
    private string GetExactMelaveSelectedNightsValue(string cur_melave_selected_nights, string bundle_id, DateTime fromDate, DateTime toDate)
    {
        if (String.IsNullOrEmpty(cur_melave_selected_nights) || (!cur_melave_selected_nights.Contains("1")))
        {
            string old_melave_selected_nights = cur_melave_selected_nights;
            cur_melave_selected_nights = BuildUpdatedMelaveSelectedNightsString(fromDate, toDate);
        }
        return cur_melave_selected_nights;

    }

    private string BuildUpdatedMelaveSelectedNightsString(DateTime from, DateTime to)
    {
        const char repetativeValue = '1';
        TimeSpan ts = to - from;
        return new String(repetativeValue, Math.Abs(ts.Days));
    }

}
