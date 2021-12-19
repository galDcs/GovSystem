using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.IO;
using System.Xml;
using System.Text;

public partial class ConvertFile : System.Web.UI.Page
{
    private XmlDocument xmlResult = new XmlDocument();

    protected void Page_Load(object sender, EventArgs e)
    {
       // if (!Utils.CheckSecurity(246)) Response.Redirect("AccessDenied.aspx");
        
            xmlResult = new XmlDocument();
            
    }
    protected void UploadBtn_Click(object sender, CommandEventArgs e)
    {
        lblError.Text = "";
        string filePath = "";

        try
        {
            string strFilename = "GOV_ConvertFile_log_" + String.Format("{0:yyyy_dd_MM_HH_mm_ss}", DateTime.Now) + ".txt";

            if (FileUpLoad1.PostedFile != null && FileUpLoad1.PostedFile.ContentLength > 0)
            {
                // Get a reference to PostedFile object
                HttpPostedFile myFile = FileUpLoad1.PostedFile;
                filePath = Server.MapPath(@"UploadedFiles\" + strFilename);
                FileUpLoad1.SaveAs(filePath); //Response.Write(Path.GetExtension(FileUpLoad1.FileName)); Response.End();
               
                lblError.Text = "קובץ (" + FileUpLoad1.PostedFile.FileName + ") הועלה לשרת, ";
                lblError.ForeColor = System.Drawing.Color.Green;
                if (DropDownList1.SelectedIndex == 3)//referral file convert to asci
                {
                    if (Path.GetExtension(FileUpLoad1.FileName).ToLower() == ".xml")
                        ConvertReferralToAsci(filePath);
                    else
                    {
                        lblError.Text = "בחר קובץ בסיומת .xml .";
                        lblError.ForeColor = System.Drawing.Color.Red;
                    }
                    
                }
                else
                {//convert to xml
                    if (Path.GetExtension(FileUpLoad1.FileName) == ".txt")
                    {
                        ConvertFileToXml(filePath);

                        StringWriter oStringWriter = new StringWriter();
                        oStringWriter.WriteLine(xmlResult.OuterXml);
                        Response.ContentType = "text/xml";
                        string fileName = "MOD_SupplierActualVoucer_SHIKUM_83360021_mimushim_" + DateTime.Today.Year.ToString("0000") + DateTime.Today.Month.ToString("00") + "_" + DateTime.Today.Year.ToString("0000") + DateTime.Today.Month.ToString("00") + DateTime.Today.Day.ToString("00") + ".xml";
                        Response.AddHeader("content-disposition", "attachment;filename=" + fileName);
                        Response.Clear();

                        using (StreamWriter writer = new StreamWriter(Response.OutputStream, Encoding.UTF8))
                        {
                            writer.Write(oStringWriter);
                        }
                        Response.End();
                    }
                    else
                    {
                        lblError.Text = "בחר קובץ בסיומת .txt .";
                        lblError.ForeColor = System.Drawing.Color.Red;
                    }
                }
            }
            else
            {
                lblError.Text = "חסר קובץ.";
                lblError.ForeColor = System.Drawing.Color.Red;
            }

        }
        catch (Exception ex)
        {
            lblError.Text = "אירעה תקלה, לפרטים ראה בקובץ log .";//"An error occurred, for details see the log file.";
            lblError.ForeColor = System.Drawing.Color.Red;
            Logger.Log(ex.Message, ex.Source, @"Logs\ConvertFile_log_" + DateTime.Now.ToString("yyyyMMdd") + ".txt");
        }
    }
    private void ConvertFileToXml(string filePath)
    {
        int TotalLines, ErrorLines, GoodLines ;
        TotalLines = 0;
        ErrorLines = 0;
        GoodLines = 0;
        bool mimushim;
        if (DropDownList1.SelectedIndex == 1)
            mimushim = true;
        else
            mimushim = false;
        XmlElement root = xmlResult.CreateElement("MOD_SupplierActualVoucher");
        xmlResult.AppendChild(root);
        XmlElement header = xmlResult.CreateElement("MsgHeader");
        root.AppendChild(HeaderDoc(header));
        XmlElement AllLines = xmlResult.CreateElement("ActualVoucherLines");

        if (!string.IsNullOrEmpty(filePath))
        {
            // read nad insert upoaded file
            String[] fileData = File.ReadAllLines(filePath, System.Text.Encoding.ASCII);
           
            bool isErrOnInsert = false;

            TotalLines = fileData.Length;
            if (fileData.Length > 0)
            {
                root.AppendChild(AllLines);
            }
            // skips the last row that conains num rows in the file
            for (int i = 0; i < fileData.Length - 1; i++)
            {
                try
                {
                    XmlElement Line = xmlResult.CreateElement("ActualVoucherItem");
                      if(mimushim)
                        AllLines.AppendChild(SaveFileLineMimushimNewXml(Line, fileData[i].Split(';')));
                       
                    GoodLines += 1;
                   
                }
                catch (Exception excp)
                {
                    ErrorLines += 1;
                    isErrOnInsert = true;
                    Logger.Log("Row " + i + ". " + excp.Message, excp.Source, @"Logs\ConvertFile_log_" + DateTime.Now.ToString("yyyyMMdd") + ".txt");
                }
            }

            if (!isErrOnInsert)
            {
                lblError.Text = "סך כל השוברים :  " + TotalLines + ",מתוכם:" + GoodLines + "עודכנו ," + ErrorLines + "שגויים ";
                lblError.ForeColor = System.Drawing.Color.Green;
              
            }
            else
            {
                lblError.Text = "אירעה תקלה ב   " + ErrorLines +" שוברים";
                lblError.ForeColor = System.Drawing.Color.Red;
                
            }
            
        }
        else
        {
            lblError.Text = "חסר קובץ .";
            lblError.ForeColor = System.Drawing.Color.Red;
        }
       
    }
    private XmlElement SaveFileLineMimushimNewXml(XmlElement Line, string[] values)
    {
        if (values.Length < 23)
            Line.AppendChild(getXmlnode("error", "error data line"));
        else
        {
            
            Line.AppendChild(getXmlnode("ActionCode", values[0]));
            Line.AppendChild(getXmlnode("VoucherNumber", values[1]));
            Line.AppendChild(getXmlnode("OrderedByCode", values[2]));
            Line.AppendChild(getXmlnode("OrderedByName", ""));
            Line.AppendChild(getXmlnode("VoucherDate", values[3]));
            Line.AppendChild(getXmlnode("SupplierID", values[4]));
            Line.AppendChild(getXmlnode("EntitlementYear", ""));
            Line.AppendChild(getXmlnode("SupplierOrderNumber", ""));
            Line.AppendChild(getXmlnode("RegionID", values[5]));
            Line.AppendChild(getXmlnode("RegionName", ""));
            Line.AppendChild(getXmlnode("SupplierHotelID", values[6]));
            Line.AppendChild(getXmlnode("HotelName", ""));

            Line.AppendChild(getXmlnode("CustomerCode", ""));
            Line.AppendChild(getXmlnode("WorkerNumber", ""));
            Line.AppendChild(getXmlnode("PersonalNumber", ""));
            Line.AppendChild(getXmlnode("ProtfolioNumber", values[7]));
            Line.AppendChild(getXmlnode("IDCardNumber", values[8]));
            //Line.AppendChild(getXmlnode("s_file_number", values[7]));
            //Line.AppendChild(getXmlnode("s_id_number", values[8]));
            Line.AppendChild(getXmlnode("TotalRooms", values[9]));
            
            Line.AppendChild(getXmlnode("SugEruach", values[10]));
            
            Line.AppendChild(getRoomsnode(values[9]));
            Line.AppendChild(getXmlnode("PersonalCompanions", values[11]));
            Line.AppendChild(getXmlnode("OrgCompanions", values[12]));
            Line.AppendChild(getXmlnode("ServiceNumber", values[13]));

            Line.AppendChild(getXmlnode("ServiceRate", ""));
            Line.AppendChild(getXmlnode("OrgPaymeny", ""));
            Line.AppendChild(getXmlnode("WorkerPayment", ""));
            Line.AppendChild(getXmlnode("VoucherCost", ""));
            Line.AppendChild(getXmlnode("TotalPayment", ""));

            Line.AppendChild(getXmlnode("HolidayStartDate", values[14]));
            Line.AppendChild(getXmlnode("HolidayStartDay", ""));
            Line.AppendChild(getXmlnode("HolidayEndDate", values[15]));
            Line.AppendChild(getXmlnode("HolidayEndDay", ""));
            Line.AppendChild(getXmlnode("TotalNights", values[16]));
            Line.AppendChild(getXmlnode("CancelDate", values[17]));
            Line.AppendChild(getXmlnode("CancelReason", values[18]));
            Line.AppendChild(getXmlnode("Companion1Days", values[19]));
            Line.AppendChild(getXmlnode("Companion2Days", values[20]));
            Line.AppendChild(getXmlnode("VoucherReference", values[21]));
            Line.AppendChild(getXmlnode("Supplements", ""));
            Line.AppendChild(getXmlnode("Remarks", values[22]));
        }

        return Line;
    }
    private XmlElement HeaderDoc(XmlElement header)
    {
        
        header.AppendChild(getXmlnode("MsgType", "SHIKUM"));
        header.AppendChild(getXmlnode("MsgMapCode", "G32"));
        header.AppendChild(getXmlnode("MsgDocNum", "0083360021"));
        header.AppendChild(getXmlnode("MsgApkey", Apkey.Text));
        header.AppendChild(getXmlnode("MsgSender", "PILSTI"));
        header.AppendChild(getXmlnode("MsgReceiver", "SHIKUM"));
        header.AppendChild(getXmlnode("MsgCreDate", formatDateToGov(DateTime.Now)));
        header.AppendChild(getXmlnode("MsgCreTime", formattimeToGov(DateTime.Now)));
        return header;
    }
    private XmlElement getXmlnode(string name, string text)
    {
        XmlElement child = xmlResult.CreateElement(name);

        child.InnerXml = text;

        return child;
    }
    private XmlElement getRoomsnode(string totalRooms)
    {
        int countRooms = int.Parse(totalRooms);
        XmlElement rooms = xmlResult.CreateElement("Rooms");
       
        for (int i = 0; i < countRooms; i++)
        {
            XmlElement child = xmlResult.CreateElement("Room");
            child.AppendChild(getXmlnode("RoomTypeCode", ""));
            child.AppendChild(getXmlnode("RoomTypeName", ""));
            child.AppendChild(getXmlnode("NumberOfAdults", ""));
            child.AppendChild(getXmlnode("NumberOfChildren", ""));
            rooms.AppendChild(child);
        }
        return rooms;
    }
    private void SaveFileInvoicXml(ref XmlElement root, String[] fileData )
    {
        for (int i = 0; i < fileData.Length - 1; i++)
        {
            try
            {
                if (fileData[i].Split(';')[0] == "01")
                {
                }
                if (fileData[i].Split(';')[0] == "02")
                {
                }
                if (fileData[i].Split(';')[0] == "03")
                {
                }
                //XmlElement Line = xmlResult.CreateElement("line_" + i.ToString());
                //root.AppendChild(getFileLineXml(Line, reportType, gov_docket_id, traveler_id, write_makat_number_hotel, from_date, to_date, balance_nights, mispar_tipulim, sibsud, voucher_id, bitul_indication, supplier_id, supplier_name, melavim_be_tashlum_number, melavim_le_be_tashlum_number, level, group_code, gov_area_code, docket_id, tikun_mimush, trav_pay, melave_selected_nights, critical_update_date, vat_percent, bundle_id, fifthNight_pay, clerk_login_name));
                //if (mimushim)
                 //   root.AppendChild(SaveFileLineMimushimXml(Line, fileData[i].Split(';')));
                //GoodLines += 1;

            }
            catch (Exception excp)
            {
               // ErrorLines += 1;
               // isErrOnInsert = true;
                Logger.Log("Row " + i + ". " + excp.Message, excp.Source, @"Logs\ConvertFile_log_" + DateTime.Now.ToString("yyyyMMdd") + ".txt");
            }
        }
    }

    //private XmlElement SaveFileLinefirstInvoicXml(ref XmlElement root, string[] values)
    //{
        

    //    return Line;
    //}
    // format YYYYMMDD
    private string formatDateToGov(DateTime date)
    {

        return date.Year.ToString() + date.Month.ToString("00") + date.Day.ToString("00");
    }
    // format HHMMSS
    private string formattimeToGov(DateTime date)
    {
        return  date.Hour.ToString("00") + date.Minute.ToString("00") + date.Second.ToString("00");
    }
    private void ConvertReferralToAsci(string filePath)
    {
        StringBuilder result = new StringBuilder();
        string delimiter = ";";
        int TotalLines, ErrorLines, GoodLines, CheckLins;
        TotalLines = 0;
        ErrorLines = 0;
        GoodLines = 0;
        CheckLins = 0;
        string errorVoucerId = "";



        if (!string.IsNullOrEmpty(filePath))
        {


            // read nad insert upoaded file

            XmlDocument fileData = new XmlDocument();
            fileData.Load(filePath);
            XmlNode MapCode = fileData.DocumentElement.SelectSingleNode("//MsgHeader/MsgMapCode");


            if (MapCode != null && MapCode.InnerText == "G34")//check code file
            {
                XmlNodeList Vouchernodes = fileData.DocumentElement.SelectNodes("//ActualVoucherAckLines/ActualVoucherAckItem");
                bool isErrOnInsert = false;

                TotalLines = Vouchernodes.Count;

                // skips the last row that conains num rows in the file
                foreach (XmlNode node in Vouchernodes)
                {
                    try
                    {
                        StringBuilder Line = new StringBuilder();
                       
                        Line.Append(Utils.AddAbsentSignsToTheBeginningOfTheString(node.SelectSingleNode("VoucherNumber").InnerText.ToString(),9,"0") + delimiter);
                        Line.Append(Utils.AddAbsentSignsToTheBeginningOfTheString(node.SelectSingleNode("OrderedByCode").InnerText.ToString(), 2, "0") + delimiter);
                        Line.Append(Utils.AddAbsentSignsToTheBeginningOfTheString(node.SelectSingleNode("VoucherDate").InnerText.ToString(),14,"0") + delimiter);
                        Line.Append(Utils.AddAbsentSignsToTheBeginningOfTheString(node.SelectSingleNode("VoucherRcvInDate").InnerText.ToString(),8,"0") + delimiter);
                        Line.Append(Utils.AddAbsentSignsToTheBeginningOfTheString(node.SelectSingleNode("VoucherRcvInTime").InnerText.ToString(), 6, "0") + delimiter);
                        Line.Append(Utils.AddAbsentSignsToTheBeginningOfTheString(node.SelectSingleNode("SupplierID").InnerText.ToString(), 10, "0") + delimiter);
                        Line.Append(Utils.AddAbsentSignsToTheBeginningOfTheString(node.SelectSingleNode("EntitlementYear").InnerText.ToString(), 4, "0") + delimiter);
                        Line.Append(Utils.AddAbsentSignsToTheBeginningOfTheString(node.SelectSingleNode("SupplierOrderNumber").InnerText.ToString(), 10, "0") + delimiter);
                        Line.Append(Utils.AddAbsentSignsToTheBeginningOfTheString(node.SelectSingleNode("CustomerCode").InnerText.ToString(), 2, "0") + delimiter);
                        Line.Append(Utils.AddAbsentSignsToTheBeginningOfTheString(node.SelectSingleNode("ProtfolioNumber").InnerText.ToString(),9,"0") + delimiter);
                        Line.Append(Utils.AddAbsentSignsToTheBeginningOfTheString(node.SelectSingleNode("IDCardNumber").InnerText.ToString(), 9, "0") + delimiter);
                        Line.Append(Utils.AddAbsentSignsToTheBeginningOfTheString(node.SelectSingleNode("PersonalNumber").InnerText.ToString(), 10, "0") + delimiter);
                        Line.Append(Utils.AddAbsentSignsToTheBeginningOfTheString(node.SelectSingleNode("WorkerNumber").InnerText.ToString(), 8, "0") + delimiter);
                        Line.Append(Utils.AddAbsentSignsToTheBeginningOfTheString(node.SelectSingleNode("HolidayStartDate").InnerText.ToString(), 8, "0") + delimiter);
                        Line.Append(Utils.AddAbsentSignsToTheBeginningOfTheString(node.SelectSingleNode("StatusCode").InnerText.ToString(), 1, "0") + delimiter);
                        Line.Append(Utils.AddAbsentSignsToTheBeginningOfTheString(node.SelectSingleNode("ErrorCode").InnerText.ToString() , 2, "0")+ delimiter);
                        Line.Append(Utils.AddAbsentSignsToTheBeginningOfTheString(node.SelectSingleNode("ErrDesc").InnerText.ToString(), 250, " ") + delimiter);
                        Line.Append(Utils.AddAbsentSignsToTheBeginningOfTheString(node.SelectSingleNode("ReferralNumber").InnerText.ToString(), 12, "0") + delimiter);
                        Line.Append(Utils.AddAbsentSignsToTheBeginningOfTheString(node.SelectSingleNode("ReferralVersion").InnerText.ToString(), 2, "0") + delimiter);
                        
                        Line.Append(Environment.NewLine);
                        result.Append(Line);

                    }
                    catch (Exception excp)
                    {
                        ErrorLines += 1;
                        isErrOnInsert = true;
                        Logger.Log("error at row   " + excp.Message, excp.Source, @"Logs\Update_referral_number_log_" + DateTime.Now.ToString("yyyyMMdd") + ".txt");
                    }
                }

                if (!isErrOnInsert)
                {
                    lblError.Text = "סך כל השוברים :  " + TotalLines + ",מתוכם: " + GoodLines + "עודכנו   ," + ErrorLines + " שגויים ," + CheckLins + "בבדיקה   .";
                    lblError.ForeColor = System.Drawing.Color.Green;
                }
                else
                {
                    lblError.Text = "תקלה בשוברים :  " + errorVoucerId + "  ";
                    lblError.ForeColor = System.Drawing.Color.Red;

                }
                StringWriter oStringWriter = new StringWriter();
                oStringWriter.WriteLine(result.ToString());
                Response.ContentType = "text/plain";

                Response.AddHeader("content-disposition", "attachment;filename=referralG34_" + string.Format("{0:yyyyMMdd}", DateTime.Today) + ".txt");
                Response.Clear();

                using (StreamWriter writer = new StreamWriter(Response.OutputStream, Encoding.UTF8))
                {
                    writer.Write(oStringWriter.ToString());
                }
                Response.End();
            }
            else
            {
                lblError.Text = "Mapcode לא מתאים.";
                lblError.ForeColor = System.Drawing.Color.Red;
            }
        }
        else
        {
            lblError.Text = "חסר קובץ.";
            lblError.ForeColor = System.Drawing.Color.Red;
        }
        

    }

}