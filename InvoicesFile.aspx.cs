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
using System.IO;
using System.Text;
using System.Xml.Linq;


public partial class InvoicesFile : System.Web.UI.Page
{
    #region Enums
    private enum DataType { txt, xml };
    private enum DateTimeFormatOption{ XMLLongFormat, XMLShortFormat, XMLReversalXMLShortFormat, XMLPeriodFormat, TXTFormat};
    private enum OutputStringFormat { Int, Decimal3Dot1, Decimal9Dot2, Decimal13Dot2 };
    private enum ServiceType { HotelOrder = 2, Tipulim };
    #endregion

    private const string valueSplitter = ";";

    protected void Page_Load(object sender, EventArgs e)
    {

        //if (!Utils.CheckSecurity(229)) Response.Redirect("AccessDenied.aspx");

        if (Page.IsPostBack)
        {           

            if ( !String.IsNullOrEmpty(Request["txtFromDate"]) && !String.IsNullOrEmpty(Request["txtToDate"]))
            {
                //DateTime fromDate = new DateTime(2012, 2, 01);
                //DateTime toDate = new DateTime(2012, 2, 29);

                DateTime fromDate = DateTime.Parse(Request["txtFromDate"]);
                DateTime toDate = DateTime.Parse(Request["txtToDate"]);
                DataType dataType = DataType.txt;
                int reportType = 0;


                if (!String.IsNullOrEmpty(Request["ddlReportType"]))
                {                    
                     reportType = ToIntParser(Request["dllReportType"]);
                }                
                if (!String.IsNullOrEmpty(Request["DataType"]))
                { 
                    int temp = ToIntParser(Request["DataType"]);
                    if( temp > -1 && temp < 2)
                    {
                        dataType = (DataType)temp;
                    }
                }

                DataSet ds1 = DAL_SQL_Helper.GetBCInvoicesForInvoicesFile(fromDate, toDate);

                if (dataType == DataType.txt)
                {
                    SendFile(GetTxtFile(ds1, reportType),  dataType);
                }
                else
                {
                    SendFile( GetXmlFile(ds1), dataType);
                }                
            }
        }
    }

    #region FileSendingUtil

    private void SendFile(StringBuilder result, DataType dataType)
    {
        Send(result, null, dataType);
    }
    private void SendFile(XDocument doc, DataType dataType)
    {
        Send(null, doc, dataType);
    }

    private void Send(StringBuilder result, XDocument doc, DataType dataType)
    {
        Response.AddHeader("content-disposition", "attachment;filename=" + GetFileName(dataType));
        if (dataType == DataType.txt)
        {
            StringWriter oStringWriter = new StringWriter();
            oStringWriter.WriteLine(result.ToString());
            Response.ContentType = "text/plain";
            Response.Clear();
            using (StreamWriter writer = new StreamWriter(Response.OutputStream, Encoding.ASCII))
            {
                writer.Write(oStringWriter.ToString());
            }           
        }
        else
        {
            Response.ContentType = "text/xml";
            Response.Clear(); 
            using (StreamWriter writer = new StreamWriter(Response.OutputStream, Encoding.UTF8))
            {
                writer.Write(doc);
            }            
        }        
        Response.End();
    }

   
    #endregion

    #region GetTxt

    private StringBuilder GetTxtFile(DataSet ds1, int reportType)
    {
        StringBuilder result = new StringBuilder();
        foreach (DataRow row1 in ds1.Tables[0].Rows)
        {
            Row1ToTxt(row1, reportType, ref result);
        }
        return result;       
    }


    private void Row1ToTxt(DataRow row1, int reportType, ref StringBuilder result)
    {
        const string FirstRowType_KodReshuma = "01";
        const string FirstRowType_KodMaarehet = "7";
        const string FirstRowType_MisparSapak = "83360021";
        const string FirstRowType_HeshbonMetakenet = "0";
        const string FirstRowType_HeshbonitZikuy = "0";

        int invoiceId = 0;
        string FirstRowType_MisparHeshbonitShelanu1,FirstRowType_MisparHeshbonitShelanu2, FirstRowType_TaarihHeshbonit, FirstRowType_ShumDarashSapak, FirstRowType_AhuzMaam,FirstRowType_ShumMaamSapak;  
        FirstRowType_MisparHeshbonitShelanu1 = FirstRowType_MisparHeshbonitShelanu2 = FirstRowType_TaarihHeshbonit = FirstRowType_ShumDarashSapak = FirstRowType_AhuzMaam = FirstRowType_ShumMaamSapak = String.Empty;

        GetMajorHeshbonitDetailsRow1(row1, OutputStringFormat.Decimal9Dot2 ,ref FirstRowType_MisparHeshbonitShelanu1, ref FirstRowType_TaarihHeshbonit, ref FirstRowType_ShumDarashSapak, ref FirstRowType_AhuzMaam, ref FirstRowType_ShumMaamSapak, out invoiceId);
        FirstRowType_MisparHeshbonitShelanu2 = FirstRowType_MisparHeshbonitShelanu1 = GetFormattedOutputString(FirstRowType_MisparHeshbonitShelanu1, 8);         
        string FirstRowType_Hearot = Utils.AddAbsentSignsToTheBeginningOfTheString("", 20, " ");
        string FirstRowType_Asmahta = Utils.AddAbsentSignsToTheBeginningOfTheString("", 35, " ");

        result.AppendFormat("{0}{1}",FirstRowType_KodReshuma, valueSplitter);
        result.AppendFormat("{0}{1}",FirstRowType_KodMaarehet, valueSplitter);
        result.AppendFormat("{0}{1}",FirstRowType_MisparSapak, valueSplitter);
        result.AppendFormat("{0}{1}",FirstRowType_MisparHeshbonitShelanu1, valueSplitter);
        result.AppendFormat("{0}{1}",GetDateTimeFormatted(FirstRowType_TaarihHeshbonit, DateTimeFormatOption.TXTFormat), valueSplitter);
        result.AppendFormat("{0}{1}",FirstRowType_ShumDarashSapak, valueSplitter);
        result.AppendFormat("{0}{1}",FirstRowType_HeshbonMetakenet, valueSplitter);
        result.AppendFormat("{0}{1}",FirstRowType_MisparHeshbonitShelanu2, valueSplitter);
        result.AppendFormat("{0}{1}",FirstRowType_HeshbonitZikuy, valueSplitter);
        result.AppendFormat("{0}{1}",FirstRowType_AhuzMaam, valueSplitter);
        result.AppendFormat("{0}{1}",FirstRowType_ShumMaamSapak, valueSplitter);
        result.AppendFormat("{0}{1}",FirstRowType_Hearot, valueSplitter);
        result.AppendFormat("{0}{1}",FirstRowType_Asmahta, valueSplitter);
        result.Append(Environment.NewLine);       
      
        Rows2ToTxt(FirstRowType_MisparHeshbonitShelanu1, invoiceId, reportType, ref result);       
    }

    private void Row3ToTxt(string ThirdRowType_NumberOfRows, ref StringBuilder result)
    {
        const string ThirdRowType_KodReshuma = "03";
        result.AppendFormat("{0}{1}",ThirdRowType_KodReshuma, valueSplitter);
        result.AppendFormat("{0}{1}", ThirdRowType_NumberOfRows, valueSplitter);
        result.Append(Environment.NewLine);
    }
    private void Rows2ToTxt(string SecondRowType_MisparHeshbonit, int invoiceId, int reportType, ref StringBuilder result)
    {
        DataSet ds2 = DAL_SQL_Helper.GetBCInvoicesPerformaDetailsForInvoicesFile(invoiceId);
        foreach (DataRow row2 in ds2.Tables[0].Rows)
        {
            Row2ToTxt(row2, reportType, SecondRowType_MisparHeshbonit, ref result);
        }
        Row3ToTxt(ds2.Tables[0].Rows.Count.ToString(), ref result);
    }
    private void Row2ToTxt(DataRow row2, int reportType, string SecondRowType_MisparHeshbonit, ref StringBuilder result)
    {
                const string  SecondRowType_KodReshuma = "02";
                const string SecondRowType_Ahuz = "00.00";

                string SecondRowType_VoucherId, SecondRowType_MisparTik, SecondRowType_MisparZehut, 
                       SecondRowType_Makat, SecondRowType_TaarihAthala, SecondRowType_TaarihSof, 
                       SecondRowType_MehirEhida, SecondRowType_ShumDarashSapak, SecondRowType_MisparMelavimBeTashlum, 
                       SecondRowType_MisparMelavimLeBeTashlum, SecondRowType_MisparMelavim, SecondRowType_KamutDarashSapak,
                       SecondRowType_ZihuyMalon, SecondRowType_Mezahee, SecondRowType_CriticalUpdateDate, melave_selected_nights;
              
                SecondRowType_VoucherId = SecondRowType_MisparTik = SecondRowType_MisparZehut =
                       SecondRowType_Makat = SecondRowType_TaarihAthala = SecondRowType_TaarihSof =
                       SecondRowType_MehirEhida = SecondRowType_ShumDarashSapak = SecondRowType_KamutDarashSapak =
                       SecondRowType_ZihuyMalon = SecondRowType_Mezahee = SecondRowType_CriticalUpdateDate = String.Empty;

                SecondRowType_MisparMelavimBeTashlum = SecondRowType_MisparMelavimLeBeTashlum =
                    SecondRowType_MisparMelavim = melave_selected_nights = "0";           

                GetMajorDataRow2TimeDecimalFormattedTxt(row2,
                       ref  SecondRowType_VoucherId, ref  SecondRowType_MisparTik, ref  SecondRowType_MisparZehut,
                       ref  SecondRowType_Makat, ref  SecondRowType_TaarihAthala, ref  SecondRowType_TaarihSof,
                       ref  SecondRowType_MehirEhida, ref  SecondRowType_ShumDarashSapak, ref  SecondRowType_MisparMelavimBeTashlum,
                       ref  SecondRowType_MisparMelavimLeBeTashlum, ref  SecondRowType_MisparMelavim, ref  SecondRowType_KamutDarashSapak,
                       ref  SecondRowType_ZihuyMalon, ref  SecondRowType_Mezahee, ref  SecondRowType_CriticalUpdateDate, ref  melave_selected_nights);

                string SecondRowType_Remarks = Utils.AddAbsentSignsToTheBeginningOfTheString("", 20, " ");

                result.AppendFormat("{0}{1}", SecondRowType_KodReshuma, valueSplitter);
                result.AppendFormat("{0}{1}", SecondRowType_MisparHeshbonit,  valueSplitter);
                result.AppendFormat("{0}{1}", GetFormattedOutputString( SecondRowType_VoucherId, 9 ), valueSplitter);
                result.AppendFormat("{0}{1}", GetFormattedOutputString (SecondRowType_MisparTik, 9), valueSplitter);
                result.AppendFormat("{0}{1}", GetFormattedOutputString (SecondRowType_MisparZehut, 9), valueSplitter);
                result.AppendFormat("{0}{1}", SecondRowType_Makat, valueSplitter);//already formatted
                result.AppendFormat("{0}{1}", SecondRowType_Remarks, valueSplitter);
                result.AppendFormat("{0}{1}", SecondRowType_TaarihAthala, valueSplitter);
                result.AppendFormat("{0}{1}", SecondRowType_TaarihSof, valueSplitter);
                result.AppendFormat("{0}{1}", SecondRowType_MehirEhida, valueSplitter);
                result.AppendFormat("{0}{1}", GetFormattedOutputString( SecondRowType_KamutDarashSapak, 4), valueSplitter);
                result.AppendFormat("{0}{1}", SecondRowType_ShumDarashSapak, valueSplitter);
                result.AppendFormat("{0}{1}", SecondRowType_Ahuz, valueSplitter);                
                result.AppendFormat("{0}{1}", SecondRowType_MisparMelavimLeBeTashlum, valueSplitter);// added at 2012.06.17 (position 14)                
                result.AppendFormat("{0}{1}", SecondRowType_MisparMelavim, valueSplitter);// old - commented on 2012.06.17 (will be used SecondRowType_MisparMelavimLeBeTashlum)                
                result.AppendFormat("{0}{1}", SecondRowType_MisparMelavimBeTashlum, valueSplitter);// added at 2012.06.17 (position 15)
                // old - commented on 2012.06.17 (will be used SecondRowType_MisparMelavimBeTashlum)
                //result.Append(SecondRowType_MelavimAlyadeyZakay + valueSplitter);
                result.AppendFormat("{0}{1}", GetFormattedOutputString(Utils.GetLastdigit(SecondRowType_ZihuyMalon, 4), 6), valueSplitter);
                result.AppendFormat("{0}{1}", GetFormattedOutputString (SecondRowType_Mezahee, 10),   valueSplitter); 
                if (reportType == 1)//reportType  0-usual, 1-extended 
                {
                    result.AppendFormat("{0}{1}", Utils.AddAbsentSignsToTheEndOfTheString(melave_selected_nights, 31, " "), valueSplitter);
                    result.AppendFormat("{0}{1}", SecondRowType_CriticalUpdateDate, valueSplitter);
                }
                result.Append(Environment.NewLine);             

    }
    #endregion  

    
    #region GetXML

    private XDocument GetXmlFile(DataSet ds)
    {        
        XDocument result = OpenDoc();
        foreach (DataRow row1 in ds.Tables[0].Rows)
        {
            Row1ToXML(row1, ref result);
        } 
        return result;
    }

    private XDocument OpenDoc()
    {
        return new XDocument(
            new XDeclaration("1.0", "utf-8", "yes"),
            new XElement("CareInvoice",
                new XElement("ThatMsg",
                    new XElement("MsgType", "CareInvoice"),
                    new XElement("MsgMapCode", "M18"),
                    new XElement("MsgMapVer", "10"),
                    new XElement("MsgMapForm", "XB2B-1"),
                    new XElement("MsgMapAuthor", "MOD"),
                    new XElement("MsgSecLvl", String.Empty),
                    new XElement("MsgSecKey", String.Empty),
                    new XElement("MsgMapTitle", String.Empty)))
                    );
    }

    private void Row1ToXML(DataRow row1, ref XDocument doc)
    {
        const string MODNumberValue = "83360021";         
        const string Shekel = "ILS";

        int invoiceId = 0;
        string FirstRowType_MisparHeshbonitShelanu1,  FirstRowType_TaarihHeshbonit, FirstRowType_ShumDarashSapak, FirstRowType_AhuzMaam, FirstRowType_ShumMaamSapak;
        FirstRowType_MisparHeshbonitShelanu1  = FirstRowType_TaarihHeshbonit = FirstRowType_ShumDarashSapak = FirstRowType_AhuzMaam = FirstRowType_ShumMaamSapak = String.Empty;

        GetMajorHeshbonitDetailsRow1(row1, OutputStringFormat.Decimal9Dot2, ref FirstRowType_MisparHeshbonitShelanu1, ref FirstRowType_TaarihHeshbonit, ref FirstRowType_ShumDarashSapak, ref FirstRowType_AhuzMaam, ref FirstRowType_ShumMaamSapak, out invoiceId);


        DataSet ds2 = null;
        doc.Element("CareInvoice").Add(
        new XElement("ClnvHeader",
            new XElement("DocumentType", "KG"),  //  heshbonit zikuy
            new XElement("InvoiceType", "359"),
            new XElement("ActionType", "9"),// heshbonit makor
            new XElement("OriginalInvNum", String.Empty),//no need to check hardcoded
            new XElement("NumCorrectInv", String.Empty),//no need to check hardcoded
            new XElement("CinvoiceRefBy", "1"),
            new XElement("CinvoiceNum", GetFormattedOutputString(FirstRowType_MisparHeshbonitShelanu1, 16)),//invoiceId = row1["id"]
            new XElement("BunchNum", String.Empty), //DO NOT UNDERSTAND THIS ELEMENT
            new XElement("CinvDate", GetDateTimeFormatted(FirstRowType_TaarihHeshbonit, DateTimeFormatOption.XMLLongFormat)),
            new XElement("CinvPeriod", GetDateTimeFormatted(FirstRowType_TaarihHeshbonit, DateTimeFormatOption.XMLPeriodFormat)),
            new XElement("MsgCreationDate", GetDateTimeFormatted(DateTime.Now, DateTimeFormatOption.XMLShortFormat)),
            new XElement("LiasionUnit", "5"),
            new XElement("Supplier",
                new XElement("MODNumber", GetFormattedOutputString(MODNumberValue, 10) )),
            new XElement("Cinvoice",
                new XElement("CinvTotal", GetTotalSumString(FirstRowType_ShumDarashSapak, FirstRowType_ShumMaamSapak)),
                new XElement("VATPercentage", FirstRowType_AhuzMaam),
                new XElement("VATTotal", FirstRowType_ShumMaamSapak),
                new XElement("Currency", Shekel),
                new XElement("Discount", String.Empty), //NOT FINISHED !!!!! NOT CALCULATED
                new XElement("RemTextInv", String.Empty),
                new XElement("TotalLines", GetAllLines(ref ds2, invoiceId))
                ))
            );
        Rows2ToXML(ds2, ref doc);
                  
    }

    private void Rows2ToXML(DataSet dsRows2, ref XDocument doc)
    {
        for (int i = 0; i < dsRows2.Tables[0].Rows.Count; i++)
        {
            Row2ToXML(dsRows2.Tables[0].Rows[i], i + 1, ref doc);
        }
    }

    private void Row2ToXML(DataRow row2, int rowNum, ref XDocument doc)
    {
        const string regularLine = "0";

        string SecondRowType_VoucherId,  SecondRowType_MisparZehut,
                      SecondRowType_Makat, SecondRowType_TaarihAthala, SecondRowType_TaarihSof,
                      SecondRowType_MehirEhida, SecondRowType_ShumDarashSapak, SecondRowType_MisparMelavimBeTashlum,
                      SecondRowType_MisparMelavimLeBeTashlum, SecondRowType_KamutDarashSapak,
                      SecondRowType_ZihuyMalon, SecondRowType_Mezahee;
        int serviceType;

        SecondRowType_VoucherId = SecondRowType_MisparZehut =
               SecondRowType_Makat = SecondRowType_TaarihAthala = SecondRowType_TaarihSof =
               SecondRowType_MehirEhida = SecondRowType_ShumDarashSapak = SecondRowType_KamutDarashSapak =
               SecondRowType_ZihuyMalon = SecondRowType_Mezahee  = String.Empty;

        SecondRowType_MisparMelavimBeTashlum = SecondRowType_MisparMelavimLeBeTashlum = "0";

        GetMajorDataRow2UnformattedXML(row2, ref  SecondRowType_VoucherId,  ref  SecondRowType_MisparZehut,
                       ref  SecondRowType_Makat, ref  SecondRowType_TaarihAthala, ref  SecondRowType_TaarihSof,
                       ref  SecondRowType_MehirEhida, ref  SecondRowType_ShumDarashSapak, ref  SecondRowType_MisparMelavimBeTashlum,
                       ref  SecondRowType_MisparMelavimLeBeTashlum,  ref  SecondRowType_KamutDarashSapak,
                       ref  SecondRowType_ZihuyMalon, ref  SecondRowType_Mezahee,  out  serviceType);      
        
      
        doc.Element("CareInvoice").Element("ClnvHeader").Add(
            new XElement ("CinvLines", 
                new XElement("Referral",
                    new XElement("LineNO", GetFormattedOutputString( rowNum.ToString(), 5) ),
                    new XElement("ReferralNum", String.Empty), // NOTE: אין לשלוח את השדה
                    new XElement("ReferralVoucher", GetFormattedOutputString(SecondRowType_VoucherId, 9)),
                    new XElement("LineType", regularLine)  ), // ATM ASSUMED THAT ALL LINES  ARE REGULAR
                new XElement("Patient",
                    new XElement("IdCard", GetFormattedOutputString(SecondRowType_MisparZehut, 9)),
                    new XElement ("PatientFileNum", String.Empty)        ),
                new XElement("Treatment",
                    new XElement("ModCatNum", GetFormattedOutputString(SecondRowType_Makat, 20)),
                    new XElement("TreatStartDate", GetDateTimeFormatted(SecondRowType_TaarihAthala, DateTimeFormatOption.XMLReversalXMLShortFormat)),
                    new XElement("TreatEndDate", GetDateTimeFormatted(SecondRowType_TaarihSof, DateTimeFormatOption.XMLReversalXMLShortFormat))),
                new XElement("ReferrTotals",
                    new XElement("Quantity", GetFormattedOutputString(SecondRowType_KamutDarashSapak, OutputStringFormat.Decimal13Dot2)), //NOT SURE THAT UNDERSTOOD CORRECTLY
                    new XElement ("UOM", GetFormattedUomValue(serviceType) ) ,
                    new XElement("UOMPrice", GetFormattedOutputString(SecondRowType_MehirEhida, OutputStringFormat.Decimal13Dot2)),
                    new XElement("ReferrTotal", GetFormattedOutputString(SecondRowType_ShumDarashSapak, OutputStringFormat.Decimal13Dot2)),
                    new XElement ("SuppDiscount", String.Empty), //NOT CALACULATED, NOT SURE ABOUT IT
                    new XElement ("SuppDiscountAmt", String.Empty), //NOT CALACULATED, NOT SURE ABOUT IT
                    new XElement ("RemTextLine", String.Empty) ), //IS THIS AN OFFICE_COMMENT? OR LEAVE EMPTY
                new XElement("AddExpance", 
                   new XElement("NumEscorts", SecondRowType_MisparMelavimBeTashlum ),
                   new XElement("NumEscortsPatient", SecondRowType_MisparMelavimLeBeTashlum),
                   new XElement("AmountPayPatient", String.Empty), // FORMULA ?  (pure_amount  + maam) - ( zakaut ) =>zakaut of traveller * days * mehirEhida + zakaut of traveller melave * misparMelavim * mehirEhida ???? 
                   new XElement("DocetNum", GetFormattedOutputString(SecondRowType_Mezahee, 10)),
                   new XElement("HotelCode", GetFormattedHotelCode(serviceType, Utils.GetLastdigit(SecondRowType_ZihuyMalon, 4))),
                   new XElement("HotelInvNo", String.Empty), //?  מה זה חשבונית נופש ומאיפה לקחת את הנתון. כנ"ל שורה מתחת
                   new XElement("HotelInvAmt",String.Empty) // 
                   )  ) );
    }

    #endregion

    #region ValueGetters 

    private string GetFileName(DataType dataType)
    {
        const string txt = "_invoicesFile.txt";
        const string xml = "_invoicesFile.xml";
        return String.Concat(string.Format("{0:yyyyMMdd}", DateTime.Today), (dataType == DataType.txt ? txt : xml));
    }

    private void GetMajorDataRow2UnformattedXML(DataRow row2, ref  string SecondRowType_VoucherId,  ref  string SecondRowType_MisparZehut,
                       ref  string SecondRowType_Makat, ref  string SecondRowType_TaarihAthala, ref  string SecondRowType_TaarihSof,
                       ref  string SecondRowType_MehirEhida, ref  string SecondRowType_ShumDarashSapak, ref  string SecondRowType_MisparMelavimBeTashlum,
                       ref  string SecondRowType_MisparMelavimLeBeTashlum,  ref  string SecondRowType_KamutDarashSapak,
                       ref  string SecondRowType_ZihuyMalon, ref  string SecondRowType_Mezahee, 
                       out  int serviceType)
    {
        string SecondRowType_MisparTik, SecondRowType_MisparMelavim, SecondRowType_CriticalUpdateDate, melave_selected_nights;
        SecondRowType_MisparTik = SecondRowType_MisparMelavim = SecondRowType_CriticalUpdateDate = melave_selected_nights = String.Empty;

        int bundleId = 0;

        GetMajorDataRow2Unformatted(row2, ref  SecondRowType_VoucherId, ref  SecondRowType_MisparTik, ref  SecondRowType_MisparZehut,
                      ref  SecondRowType_Makat, ref  SecondRowType_TaarihAthala, ref  SecondRowType_TaarihSof,
                      ref  SecondRowType_MehirEhida, ref  SecondRowType_ShumDarashSapak, ref  SecondRowType_MisparMelavimBeTashlum,
                      ref  SecondRowType_MisparMelavimLeBeTashlum, ref  SecondRowType_MisparMelavim, ref  SecondRowType_KamutDarashSapak,
                      ref  SecondRowType_ZihuyMalon, ref  SecondRowType_Mezahee, ref  SecondRowType_CriticalUpdateDate, ref  melave_selected_nights, out serviceType, out bundleId);
    }

    private void GetMajorDataRow2TimeDecimalFormattedTxt(DataRow row2,
                       ref string SecondRowType_VoucherId, ref string SecondRowType_MisparTik, ref string SecondRowType_MisparZehut,
                       ref string SecondRowType_Makat, ref string SecondRowType_TaarihAthala, ref string SecondRowType_TaarihSof,
                       ref string SecondRowType_MehirEhida, ref string SecondRowType_ShumDarashSapak, ref string SecondRowType_MisparMelavimBeTashlum,
                       ref string SecondRowType_MisparMelavimLeBeTashlum, ref string SecondRowType_MisparMelavim, ref string SecondRowType_KamutDarashSapak,
                       ref string SecondRowType_ZihuyMalon, ref string SecondRowType_Mezahee, ref string SecondRowType_CriticalUpdateDate, ref string melave_selected_nights)
    {
        const DateTimeFormatOption optionDateTime = DateTimeFormatOption.TXTFormat;
        const OutputStringFormat optionDecimal = OutputStringFormat.Decimal9Dot2;
        int serviceType, bundleId;
        serviceType = bundleId = 0;

        GetMajorDataRow2Unformatted(row2, ref  SecondRowType_VoucherId, ref  SecondRowType_MisparTik, ref  SecondRowType_MisparZehut,
                       ref  SecondRowType_Makat, ref  SecondRowType_TaarihAthala, ref  SecondRowType_TaarihSof,
                       ref  SecondRowType_MehirEhida, ref  SecondRowType_ShumDarashSapak, ref  SecondRowType_MisparMelavimBeTashlum,
                       ref  SecondRowType_MisparMelavimLeBeTashlum, ref  SecondRowType_MisparMelavim, ref  SecondRowType_KamutDarashSapak,
                       ref  SecondRowType_ZihuyMalon, ref  SecondRowType_Mezahee, ref  SecondRowType_CriticalUpdateDate, ref  melave_selected_nights, out serviceType, out bundleId);

        SecondRowType_TaarihAthala = GetDateTimeFormatted(SecondRowType_TaarihAthala, optionDateTime);
        SecondRowType_TaarihSof = GetDateTimeFormatted(SecondRowType_TaarihSof, optionDateTime);
        SecondRowType_CriticalUpdateDate = GetDateTimeFormatted(SecondRowType_CriticalUpdateDate, optionDateTime);
        SecondRowType_ShumDarashSapak = GetFormattedOutputString(SecondRowType_ShumDarashSapak, optionDecimal);
        SecondRowType_MehirEhida = GetFormattedOutputString(SecondRowType_MehirEhida, optionDecimal);

    }

    private void GetMajorDataRow2Unformatted(DataRow row2, ref string SecondRowType_VoucherId, ref string SecondRowType_MisparTik, ref string SecondRowType_MisparZehut,
                       ref string SecondRowType_Makat, ref string SecondRowType_TaarihAthala, ref string SecondRowType_TaarihSof,
                       ref string SecondRowType_MehirEhida, ref string SecondRowType_ShumDarashSapak, ref string SecondRowType_MisparMelavimBeTashlum,
                       ref string SecondRowType_MisparMelavimLeBeTashlum, ref string SecondRowType_MisparMelavim, ref string SecondRowType_KamutDarashSapak,
                       ref string SecondRowType_ZihuyMalon, ref string SecondRowType_Mezahee, ref string SecondRowType_CriticalUpdateDate, ref string melave_selected_nights, out int serviceType, out int bundleId)
    {
        SecondRowType_VoucherId = row2["voucher_id"].ToString();
        SecondRowType_MisparTik = row2["gov_docket_id"].ToString();
        SecondRowType_MisparZehut = row2["id_no"].ToString();

        SecondRowType_TaarihAthala = row2["from_date"].ToString();
        SecondRowType_TaarihSof = row2["to_date"].ToString();
        SecondRowType_MehirEhida = row2["ItemPrice"].ToString();
        SecondRowType_ShumDarashSapak = row2["pure_amount"].ToString();
        SecondRowType_MisparMelavim = row2["EscortsNumber"].ToString();
        SecondRowType_KamutDarashSapak = row2["DaysNumber"].ToString();
        SecondRowType_ZihuyMalon = row2["pay_to_supplier_id"].ToString();
        SecondRowType_Mezahee = row2["docket_id"].ToString();
        SecondRowType_CriticalUpdateDate = row2["critical_update_date"].ToString();
        melave_selected_nights = row2["melave_selected_nights"].ToString();

        serviceType = ToIntParser(row2["service_type"].ToString());
        bundleId = ToIntParser(row2["id"].ToString());

        GetCalculatedMelavimValuesAsStrings(row2["tr_melave_number"].ToString(), row2["EscortsNumber"].ToString(), row2["EscortsByZakai"].ToString(), row2["gov_makat_number"].ToString(), row2["gov_balance_ussage"].ToString(), bundleId, serviceType, ref SecondRowType_MisparMelavimBeTashlum, ref SecondRowType_MisparMelavimLeBeTashlum);
        SecondRowType_Makat = GetMakatValue(row2["gov_makat_number"].ToString(), serviceType);
    }


    private void GetMajorHeshbonitDetailsRow1(DataRow row1,  OutputStringFormat optionNum ,ref string heshbonitNum, ref string heshbonitDateTime, ref string sumDemanded, ref string vatPercentage, ref string vatAmount, out int invoiceId)
    {
        heshbonitNum = row1["docket_invoice_id"].ToString();
        heshbonitDateTime = row1["cdate"].ToString();
        sumDemanded = GetFormattedOutputString(row1["pure_amount"].ToString(), optionNum);        
        vatPercentage = GetVatPercentageString(row1["vat_amount"].ToString());
        vatAmount = GetFormattedOutputString(row1["vat_amount"].ToString(), optionNum);
        invoiceId = ToIntParser(row1["id"].ToString());
    }
   
    private string GetVatPercentageString(string rowData)
    {
        const string freeMaam = "00.00";
        const string fullMaam = "16.00";
        if (!String.IsNullOrEmpty(rowData) && Convert.ToDecimal(rowData) != 0)
        {
            return fullMaam;
        }
        return freeMaam;
    }
    
    private string GetFormattedHotelCode(int serviceType, string unformattedCode)
    {
        
        if (serviceType == (int)ServiceType.HotelOrder)
        {
            return String.Empty;
        }
        return GetFormattedOutputString(unformattedCode, 5);
        
    }

    private string GetMakatValue(string rawMakatData, int serviceType)
    {
        string result;

        if (serviceType == (int)ServiceType.HotelOrder)
        {
            result = DAL_SQL.GetRecord("GOV_MAKATS", "WriteToAccLogFirstRoom", rawMakatData, "MakatNumber");
        }
        else
        {
            result = DAL_SQL.GetRecord("GOV_MAKATS", "WriteToAccLogTipulim", rawMakatData, "MakatNumber");
        }

        if (result.Length <= 0) // bad makat number, write as is
        {
            result = GetFormattedOutputString (rawMakatData, 6);  
            Logger.Log("ERROR: INVOICES FILE", "WRONG MAKAT NUMBER: " + result + " ", @"Logs\log_invoices_file_" + DateTime.Now.ToShortDateString() + ".txt");
            return result;          
        }

        return GetFormattedOutputString( result, 6);
    }

    private void GetCalculatedMelavimValuesAsStrings( string tr_melave_num, string escorts_num, string escorts_by_zakai, string gov_makat_num, string gov_ballance_usage, int bundleId, int serviceType, ref string melavim_be_tashlum, ref string melavim_le_be_tashlum)
    { 
        int tr_melavim, escNum, escNumByZakai, makatNum;
        tr_melavim = escNum = escNumByZakai = makatNum = 0;

        ParseAllStringsToInt(tr_melave_num, escorts_num, escorts_by_zakai, gov_makat_num,  out tr_melavim, out escNum, out escNumByZakai, out makatNum);

        CheckSpecialMakatLogic(tr_melavim, makatNum, gov_ballance_usage, escNum, ref  melavim_be_tashlum, ref  melavim_le_be_tashlum);
        CheckOrderTypeLogic(serviceType, tr_melavim, bundleId, ref melavim_be_tashlum, ref melavim_le_be_tashlum);  
    }

    private void CheckOrderTypeLogic(int serviceType, int tr_melavim, int bundleId, ref string melavim_be_tashlum, ref string melavim_le_be_tashlum)
    {
       
        int be_tashlum, le_be_tashlum, rooms;
        rooms = be_tashlum = le_be_tashlum = 0;

        if (serviceType == (int)ServiceType.HotelOrder) // hotel
        {
            //1. Getting rooms number 
            string strRooms = DAL_SQL_Helper.GetRoomsNumByBundle(bundleId);
            int.TryParse(strRooms, out rooms);

            //2. Getting number of  melave acc to voucher =>  מספר מלווים בשבובר
            int voucher_melavim = DAL_SQL_Helper.GetVoucherMelavim(bundleId.ToString());

            //3. Implementing melave logic             
            Utils.MelaveTashlumDefiner(voucher_melavim, tr_melavim, rooms, ref be_tashlum, ref le_be_tashlum);
            melavim_be_tashlum = be_tashlum.ToString();
            melavim_le_be_tashlum = le_be_tashlum.ToString();
        }
    }

    private void CheckSpecialMakatLogic(int tr_melavim, int makatNum, string govBallance, int escNum, ref string melavim_be_tashlum, ref string melavim_le_be_tashlum)
    {
        const int specialMakat = 27240;
       

        if (tr_melavim == 0)
        {
            if (makatNum == specialMakat &&  ( govBallance == "1" || govBallance.ToLower() == "true")  ) 
            {
                melavim_le_be_tashlum = "1";
                melavim_be_tashlum = "0";
            }
            else
            {
                melavim_le_be_tashlum = "0";
                melavim_be_tashlum = "1"; // earlier: (escNum > 1) ? "1" : escNum.ToString();
            }
        }
        else
        {
            //?
            melavim_le_be_tashlum = "1"; //earlier: escNum.ToString();
            melavim_be_tashlum = "0";
        }

    }
   
    private string GetFormattedUomValue(int serviceType)
    {
        const string UOMHotelStay = "3";
        const string UOMHameiMarpe = "27";

        return GetFormattedOutputString((serviceType == (int)ServiceType.HotelOrder ? UOMHotelStay : UOMHameiMarpe), 5);
    }

    private string GetAllLines(ref DataSet ds, int invoiceId)
    {
        ds = DAL_SQL_Helper.GetBCInvoicesPerformaDetailsForInvoicesFile(invoiceId);
        return GetFormattedOutputString (ds.Tables[0].Rows.Count.ToString(), 5);
    }

    
    private string GetTotalSumString(string pureSum, string maam)
    {
        Decimal sumDec, maamDec;
        sumDec = maamDec = Decimal.Zero;

        if (Decimal.TryParse(pureSum, out sumDec))
        {
            if (Decimal.TryParse(maam, out maamDec))
            {
                sumDec += maamDec;
            }

            return Utils.AddAbsentSignsToTheBeginningOfTheString(Math.Round(sumDec, 2).ToString(), 16, "0"); //format 13.2
        }

        return String.Empty;

    }
    #endregion

    #region Formatters and Parsers

    private string GetDateTimeFormatted(DateTime date, DateTimeFormatOption option)
    {
        return DateTimeFormatting(date, option);
    }

    private string GetDateTimeFormatted(string rawInput, DateTimeFormatOption option)
    {
        DateTime temp = new DateTime();
        if (DateTime.TryParse(rawInput, out temp))
        {
            return DateTimeFormatting(temp, option); 
            
        }
        return String.Empty;
    }

    private string DateTimeFormatting(DateTime date,  DateTimeFormatOption option)
    {
        switch(option)
            {
                case DateTimeFormatOption.XMLLongFormat:  return String.Format("{0}{1}{2}{3}{4}{5}", date.Day.ToString("00"), date.Month.ToString("00"), date.Year.ToString("00"), date.Hour.ToString("00"), date.Minute.ToString("00"), date.Second.ToString("00"));
                case DateTimeFormatOption.XMLShortFormat: return String.Format("{0}{1}{2}", date.Day.ToString("00"), date.Month.ToString("00"), date.Year.ToString("0000"));
                case DateTimeFormatOption.XMLReversalXMLShortFormat: return String.Format("{0}{1}{2}", date.Year.ToString("0000"), date.Month.ToString("00"), date.Day.ToString("00"));
                case DateTimeFormatOption.XMLPeriodFormat: return String.Format("01{0}{1}", date.Month.ToString("00"), date.Year.ToString("0000"));
                case DateTimeFormatOption.TXTFormat: return String.Format("{0}.{1}.{2}", date.Day.ToString("00"), date.Month.ToString("00"), date.Year.ToString("0000"));
            }
        return String.Empty;
    }

    private string GetFormattedOutputString(string str, OutputStringFormat option)
    {
        return GetFormattedString( str, option, 0 );
    }

    private string GetFormattedOutputString(string str, int numOfChars)
    {
        return GetFormattedString(str, OutputStringFormat.Int, numOfChars);
    }

    private string GetFormattedString(string str, OutputStringFormat option, int numOfCharsForInt)
    {
        const string addCharZero = "0";

        switch (option)
        {
            case OutputStringFormat.Int : return Utils.AddAbsentSignsToTheBeginningOfTheString(str, numOfCharsForInt, addCharZero);
            case OutputStringFormat.Decimal3Dot1: return GetFormattedWithDot(str, 3, 1);
            case OutputStringFormat.Decimal9Dot2: return GetFormattedWithDot(str, 9, 2);
            case OutputStringFormat.Decimal13Dot2: return GetFormattedWithDot(str, 13, 2);
        }       
        
        return String.Empty;
    }

    private string GetFormattedWithDot(string rawData, int beforeDot, int AfterDot)
    {
        const string addCharZero = "0";
        Decimal temp = Decimal.Zero;
        if (Decimal.TryParse(rawData, out temp))
        {
            return Utils.AddAbsentSignsToTheBeginningOfTheString(Math.Round(temp, AfterDot).ToString(), beforeDot + AfterDot + 1, addCharZero);
        }
        return String.Empty;
    }

    private void ParseAllStringsToInt(string tr_melave_num, string escorts_num, string escorts_by_zakai, string gov_makat_num,  out int tr_melavim, out int escNum, out int escNumByZakai, out int makatNum)
    {
        Int32.TryParse(tr_melave_num, out tr_melavim);
        Int32.TryParse(escorts_num, out escNum);
        Int32.TryParse(escorts_by_zakai, out escNumByZakai);
        Int32.TryParse(gov_makat_num, out makatNum);
         
        
    }

    private int ToIntParser(string val)
    {
        int res = 0;
        Int32.TryParse(val, out res);
        return res;
    }
    #endregion
}
