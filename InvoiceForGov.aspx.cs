using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;
using System.Collections.Generic;
using System.IO.Compression;
using Ionic.Zip;

public partial class InvoiceForGov : System.Web.UI.Page
{
	public bool toShow = false;
    private const char k_ParamDivider = ';';
	public string[] filesNames;
	StringBuilder sb = new StringBuilder();
	
    protected void Page_Load(object sender, EventArgs e)
    {
        setAgencyData();
    }
	
    private void setAgencyData()
    {
        AgencyUser user = new AgencyUser();
        user.AgencyId = "85";
        user.AgencySystemType = "3";
        user.AgencyUserId = "1";
        user.AgencyUserName = "Agency2000";
        user.AgencyUserPassword = "11071964";
        DAL_SQL.ConnStr = ConfigurationManager.AppSettings["CurrentAgencyDBConnStr"];
        DAL_SQL.ConnStr = DAL_SQL.ConnStr.Replace("^agencyId^", ((user.AgencyId.Length == 1) ? "000" + user.AgencyId : "00" + user.AgencyId));
        DAL_SQL.ConnStr = DAL_SQL.ConnStr.Replace("^systemType^", ((user.AgencySystemType == "3") ? "INN" : "OUT"));
    }

    protected void Upload_Click(object sender, EventArgs e)
    {
		sb.Append("<table style='direction:rtl; text-align:center;'>");
		sb.Append("<tr><th>חשבונית</th><th>מלון</th><th>מתאריך</th><th>עד תאריך</th></tr>");
        // ClAsciFile item = new ClAsciFile()
        // {
        //     FromDate = new DateTime(2017, 3, 5),
        //     Todate = new DateTime(2017, 3, 10),
        //     InnerUse_AgencyHotelId = "102"
        // };
        // DataSet ds = getPricesFromDB(item);
		string fileName = string.Empty;
        if (FileUpLoadInvoice.PostedFile != null && FileUpLoadInvoice.PostedFile.ContentLength > 0)
        {
            List<string> allLinesList = getLinesList();
            string xml = getAsciFile(allLinesList);

			string headLine = "<?xml version=\"1.0\" encoding=\"windows-1255\"?>";
			string[] splittedXml = xml.Split(new string[] { headLine }, StringSplitOptions.None );
			string xmlWithHeadLine = string.Empty;
			int i = 0;
			string pathToCareInvoice = HttpContext.Current.Request.MapPath("Logs\\CareInvoice\\");
			string pathToCareInvoiceZip = HttpContext.Current.Request.MapPath("Logs\\CareInvoiceForZip\\");
			//Delete all file in ForZip folder and zip it.
			System.IO.DirectoryInfo di = new DirectoryInfo(pathToCareInvoiceZip);
			
			foreach (FileInfo file in di.GetFiles())
			{
				file.Delete(); 
			}
			
			foreach (string oneXml in splittedXml )
			{
				if (!string.IsNullOrEmpty(oneXml))
				{
					xmlWithHeadLine = headLine + oneXml;
					fileName = filesNames[i];
					XmlDocument doc = new XmlDocument();
					doc.LoadXml(xmlWithHeadLine );

					saveFileInGovDir(doc, fileName, "", pathToCareInvoice);
					saveFileInGovDirZip(doc, fileName, "", pathToCareInvoice);
					
					i++;
				}
			}
			
			string path = HttpContext.Current.Request.MapPath("Logs\\");
			
			//.net 2.0 code
			saveZip(path, pathToCareInvoiceZip + "careInvoices.zip");
			
			using (var client = new WebClient())
			{
				client.DownloadFile(pathToCareInvoiceZip + "careInvoices.zip", "D:\\Logs\\careInvoices.zip");
			}
			
			string downloadLink = "http://web08.agency2000.co.il/GovSystem/Logs/CareInvoiceForZip/careInvoices.zip";
			toShow = true;
			//lblDownloadLink.HRef = downloadLink;
			//lblDownloadLink.Visible = true;
			//Response.AddHeader("content-disposition", "attachment;filename=" + "Logs\\careInvoices.zip");
			//Response.Clear();
			//using (StreamWriter writer = new StreamWriter(Response.OutputStream, Encoding.UTF8))
			//{
			//	writer.Write(savedZipPath );
			//}
			//
			//Response.End();
        }
		sb.Append("</table>");
		Response.Write(sb.ToString());
    }

    private List<string> getLinesList()
    {
        List<string> allLinesList = new List<string>();
        string line = string.Empty;

        // read file
        using (StreamReader inputStreamReader = new StreamReader(FileUpLoadInvoice.PostedFile.InputStream))
        {
            //getting list of all lines in the uploaded file
            while ((line = inputStreamReader.ReadLine()) != null)
            {
                //add line to list
                allLinesList.Add(line);
            }
        }

        return allLinesList;
    }

    private string getAsciFile(List<string> iAllLinesList)
    {
        List<ClAsciFile> asciFile = new List<ClAsciFile>();
        ClAsciFile item = null;
        int lineNumber = 0;
        string xmlFile = string.Empty;

        //make sure cultural is hebrew
        IFormatProvider culture = new System.Globalization.CultureInfo("he-IL", true);
		
		//Init filesNames string array.
		filesNames = new string[iAllLinesList.Count];
		int index = 0;
		
        foreach (string line in iAllLinesList)
        {
            //update current line number
            lineNumber++;
            //initiallize new ascii file
            item = new ClAsciFile();
            string[] param = line.Split(k_ParamDivider);

            try
            {
                item.LineNum = lineNumber;
                item.MsgType = param[0];
                item.MsgMapCode = param[1];
                item.MsgDocNum = param[2];
                item.MsgApkey = param[3];
                item.MsgSender = param[4];
                item.MsgReceiver = param[5];
                item.MsgCreDate = param[6];
                item.MsgCreTime = param[7];
                item.DocumentType = param[8];
                item.InvoiceType = param[9];
                item.ActionType = param[10];
                item.CinvoiceRefBy = param[11];
                item.CinvoiceNum = param[12].TrimStart('0');
                item.BunchNum = "";
                item.CinvDate = "";
                item.CinvPeriod = param[13];
                item.MsgCreationDate = param[14];
                item.InvoiceDate = param[15];
                item.LiasionUnit = param[16];
                item.MODNumber = param[17];
                item.CinvTotal = param[18];
                item.VATPercentage = param[19];
                item.VATTotal = param[20];
                item.Currency = param[21];
                item.LineNO = "";
                item.ReferralNum = param[22];
                item.ReferralVoucher = param[23];
                item.LineType = param[24];
                item.IdCard = param[25];
                item.PatientFileNum = param[26];
                item.ModCatNum = "";

                item.HotelId = getHotelId(param[38].TrimStart('0'));// tbHotel.HotelId.Value.ToString();
                item.BaseID = getBasisId(param[40]);// tbBasis.IdFile.ToString();
                item.FromDate = Convert.ToDateTime(param[28].Substring(6, 2) + "/" + param[28].Substring(4, 2) + "/" + param[28].Substring(0, 4), culture);
                item.Todate = Convert.ToDateTime(param[29].Substring(6, 2) + "/" + param[29].Substring(4, 2) + "/" + param[29].Substring(0, 4), culture);
                item.NightNum = param[30];
                item.TotalPeople = param[34];
                item.AccompaniedPay = param[35];
                item.HotelMelave1 = param[41].Trim();
                item.HotelMelave2 = param[42].Trim();
                item.CoRemTextLine1 = param[31].Trim();
				
                item.InnerUse_AgencyHotelId = param[38].TrimStart('0');
//Logger.EmptyLog("item.InnerUse_AgencyHotelId = " + item.InnerUse_AgencyHotelId, eLogger.INVOICE_XML_ERROR);
				
                item.InnerUse_AgencyBaseId = getBasisId(param[40]);
                item.InnerUse_VoucherId = param[23].TrimStart('0');
                item.InnerUse_BundleId = DAL_SQL.GetRecord("VOUCHERS", "bundle_id", item.InnerUse_VoucherId, "id");
                item.InnerUse_Erkev_Type_Name = DAL_SQL.GetRecord("BUNDLES", "erkev_type", item.InnerUse_BundleId, "id").Trim();
                item.InnerUse_Erkev_Type = getHotelRoomsTypes(item.InnerUse_Erkev_Type_Name);

                decimal PriceForCheck = 0;
                decimal CoDiscountamnt = 0;

                item.Referral = fillReferral(param, item, out PriceForCheck, out CoDiscountamnt);
                if (item.Referral == null)
                {
                    //errorPrices = errorPrices + line + ";" + PriceForCheck.ToString() + Environment.NewLine;
                    Logger.EmptyLog("item.Referral == null : " + PriceForCheck.ToString() + Environment.NewLine, eLogger.INVOICE_XML_ERROR);
                }
                else
                {
                    item.CoDiscountamnt = CoDiscountamnt.ToString("#.00");
                    if (item.CoDiscountamnt.Equals(".00"))
                        item.CoDiscountamnt = "";
                    if (item.CoRemTextLine1.Equals("00"))
                        item.CoRemTextLine1 = "";
                    xmlFile += CreateXmlFile(item);

                }

				filesNames[index] = getFileName(item.CinvoiceNum, item.InvoiceDate, index);
				index++;
				
                asciFile.Add(item);
            }
            catch (Exception ex)
            {
				Logger.EmptyLog("========================================================================", eLogger.INVOICE_XML_ERROR);
                Logger.EmptyLog(line + ex.Message , eLogger.INVOICE_XML_ERROR);
				Logger.EmptyLog("ex.StackTrace = " + ex.StackTrace, eLogger.INVOICE_XML_ERROR);
				Logger.EmptyLog("========================================================================", eLogger.INVOICE_XML_ERROR);
            }
        }

        return xmlFile;
    }

    private string getHotelId(string iGovHotelType)
    {
        string agencyHotelId;
        try
        {
            agencyHotelId = DAL_SQL.GetRecord("SUPPLIER_DETAILS", "gov_supplier_code", iGovHotelType, "supplier_id");

            return agencyHotelId;
        }
        catch (Exception)
        {
            return "00";
        }
    }

    private string getBasisId(string iGovBaseType)
    {
        string agencyBaseId;
        try
        {
            agencyBaseId = DAL_SQL.GetRecord("Agency_Admin.dbo.HOTEL_ON_BASE HOB", "HOB.gov_code", iGovBaseType, "id");

            return agencyBaseId;
        }
        catch (Exception)
        {
            return "00";
        }
    }

    private List<CLReferral> fillReferral(string[] param, ClAsciFile item, out  decimal PriceForChecktmp, out decimal CoDiscountamnt)
    {
        List<CLReferral> list = new List<CLReferral>();

        decimal priceZakai = 0;
        decimal priceZakaiTotal = 0;
        int tmp = 0;
        int LineNO = 1;
        string makat = param[27];
        string makattmp = param[27];
        DateTime StartDate = item.FromDate;
        int nightCount = 0;
        CLReferral clreferral = new CLReferral();
        decimal priceTotal = 0;
        decimal priceTmp = -1;
        decimal Price = 0;
        CoDiscountamnt = 0;
        int peoplecount;
        decimal PriceForCheck = 0;
        string NumEscortPatient = "0";
        string HotelMelave1Tmp = "";
        string HotelMelave2Tmp = "";
		decimal vatPercent = decimal.Parse(item.VATPercentage);
        Dictionary<int, DaysPrices> pricesByDay = new Dictionary<int, DaysPrices>();

        makat = makattmp = param[27];
        if (Convert.ToInt32(param[32]) == 3) //uom is hotel (hotel - 3, other - 27)
        {
            pricesByDay = fillPricesByDays(item);
			
			int zakaiAndMelaveBeTashlumId = 2330;
			decimal priceForZakaAndMelaveBeTashlum = getPriceByRoomType(pricesByDay, zakaiAndMelaveBeTashlumId, StartDate, int.Parse(item.NightNum));
			
			//If the price for zakaiAndMelaveBeTashlum saem as bruto, means that need to changes the prices to zakaiAndMelaveBeTashlum erkev. 
			if (priceForZakaAndMelaveBeTashlum == decimal.Parse(item.CinvTotal))
			{
				item.InnerUse_Erkev_Type.Clear();
				item.InnerUse_Erkev_Type.Add(zakaiAndMelaveBeTashlumId);
			}
			
            for (int i = 0; i < Convert.ToInt16(item.NightNum); i++)
            {
                makattmp = param[27];
                peoplecount = Convert.ToInt16(item.HotelMelave1.Substring(i, 1)) + Convert.ToInt16(item.HotelMelave2.Substring(i, 1));
                #region ExcelPrices
                //NOTICE:old prices (when makat was different on the weekends
                // Price = GetPriceFromExcell(item, makattmp, 1, StartDate.AddDays(nightCount), peoplecount, Convert.ToInt16(item.HotelMelave2.Substring(i, 1)), out priceZakai);
                Price = GetPriceAndPriceZakai(item, makattmp, 1, StartDate.AddDays(nightCount), peoplecount, Convert.ToInt16(item.HotelMelave2.Substring(i, 1)), pricesByDay, out priceZakai, vatPercent.ToString());

                //same as software code
                if (priceTmp == -1) // if it's the first price
                    priceTmp = Price;
                if (Price != priceTmp || makat != makattmp)
                {
                    
                    clreferral = fillReferral(param, item, makat, LineNO, nightCount, StartDate);
                    clreferral.HotelMelave1Tmp = HotelMelave1Tmp;
                    clreferral.HotelMelave2Tmp = HotelMelave2Tmp;
                    HotelMelave2Tmp = "";
                    HotelMelave1Tmp = "";

                    clreferral.ReferrTotal = priceTotal.ToString("#.00");
                    if (makat.Equals("027240") || makat.Equals("036398"))
                        clreferral.AmountPayPatient = priceZakaiTotal.ToString();
                    else
                        clreferral.AmountPayPatient = "0";
                    clreferral.UOMPrice = (priceTotal / nightCount).ToString("#.00");
                    clreferral.NumEscortPatient = NumEscortPatient;
                    list.Add(clreferral);
                    priceZakaiTotal = priceTotal = 0;
                    makat = makattmp;
                    priceTmp = Price;
                    StartDate = StartDate.AddDays(nightCount);
                    nightCount = 0;
                }
                #endregion
				

                HotelMelave1Tmp = HotelMelave1Tmp + item.HotelMelave1.Substring(i, 1);
                HotelMelave2Tmp = HotelMelave2Tmp + item.HotelMelave2.Substring(i, 1);

                priceTotal = priceTotal + Price;
                PriceForCheck = PriceForCheck + Price;
                priceZakaiTotal = priceZakaiTotal + priceZakai;
                NumEscortPatient = item.HotelMelave2.Substring(i, 1);
                nightCount++;
            }
            LineNO++;
			
            clreferral = fillReferral(param, item, makattmp, LineNO, nightCount, StartDate);


            clreferral.AmountPayPatient = priceZakaiTotal.ToString();
            clreferral.NumEscortPatient = NumEscortPatient;
            clreferral.HotelMelave1Tmp = HotelMelave1Tmp;
            clreferral.HotelMelave2Tmp = HotelMelave2Tmp;
            HotelMelave2Tmp = "";
            HotelMelave1Tmp = "";


            if (makattmp.Equals("027240") || makattmp.Equals("036398"))
                clreferral.AmountPayPatient = priceZakaiTotal.ToString();
            else
                clreferral.AmountPayPatient = "0";
            clreferral.UOMPrice = (priceTotal / nightCount).ToString("#.00");
            //Discount 
            switch (param[31])
            {
                case "01":
                case "06":
                case "07":
                case "10":
                    clreferral.SuppDiscount = "50";
                    clreferral.AmountPayPatient = (Convert.ToDecimal(clreferral.AmountPayPatient) / 2).ToString("#.00");
                    PriceForCheck = PriceForCheck / 2;

                    break;
                default:
                    clreferral.SuppDiscount = "0";
                    break;
            }
			
            clreferral.SuppDiscountAmt = (Convert.ToDecimal(clreferral.SuppDiscount) / 100 * priceTotal).ToString("#.00");
            if (clreferral.SuppDiscountAmt != "")
                CoDiscountamnt += (Convert.ToDecimal(clreferral.SuppDiscountAmt) + (Convert.ToDecimal(clreferral.SuppDiscountAmt) / 100) * Convert.ToDecimal(item.VATPercentage));
			
			
            //priceTotal = priceTotal - (vatPercent * priceTotal/(100 + vatPercent));
			clreferral.ReferrTotal = priceTotal.ToString("#.00");
            list.Add(clreferral);
        }
        else // tipulim
        {
            LineNO = 1;
            clreferral = fillReferral(param, item, makat, LineNO, Convert.ToInt16(item.NightNum), StartDate);
            decimal tipulimPrice = GetPriceTipulim(item.InnerUse_AgencyHotelId, item.Todate.AddDays(-1), item.InnerUse_BundleId);
			decimal tipulimPriceTotal;
            //tipulimPrice = decimal.Parse(param[18]) - decimal.Parse(param[20]);
            PriceForCheck = Convert.ToDecimal((tipulimPrice * Convert.ToInt16(item.NightNum)).ToString("#.00"));
            PriceForCheck = PriceForCheck * (1 + (Convert.ToDecimal(param[19]) / 100));
            tipulimPriceTotal = (tipulimPrice * Convert.ToInt16(item.NightNum));
            clreferral.AmountPayPatient = "0";
            clreferral.UOMPrice = (tipulimPrice - (vatPercent * tipulimPrice/(100 + vatPercent))).ToString("#.00");
			
			tipulimPriceTotal = tipulimPriceTotal - (vatPercent * tipulimPriceTotal/(100 + vatPercent));
			clreferral.ReferrTotal = tipulimPriceTotal.ToString("#.00");
            list.Add(clreferral);
        }
        if (Convert.ToInt32(param[32]) == 3) //hotel
        {
            PriceForChecktmp = PriceForCheck - Convert.ToDecimal(param[18]) + Convert.ToDecimal(param[20]);
        }
        else
        {
            PriceForChecktmp = PriceForCheck - Convert.ToDecimal(param[18]);
        }

        return list;
    }

    private decimal GetPriceTipulim(string iSupplierId, DateTime iDate, string iBundleId)
    {
        string addId;

        addId = getAddId(iBundleId);

        //double price = DAL_SQL_Helper.GetNettoPriceForAttraction(int.Parse(iBundleId), iSupplierId, addId, "1", iDate, iDate.AddDays(1));
		double price = DAL_SQL_Helper.GetBrutoPriceForAttraction(int.Parse(iBundleId), iSupplierId, addId, "1", iDate, iDate.AddDays(1));

        return decimal.Parse(price.ToString());
    }

	private decimal getPriceByRoomType(Dictionary<int, DaysPrices> pricesByDay, int zakaiAndMelaveBeTashlumId, DateTime fromDate, int nights)
	{
		double totalPrice = 0;
		DaysPrices dp;

        pricesByDay.TryGetValue(zakaiAndMelaveBeTashlumId, out dp);
		
		if (dp != null)
		{
			for (int i = 0; i < nights; i++)
			{
				totalPrice += dp.Bruto.GetPriceByDayOfWeek(fromDate.AddDays(i).DayOfWeek);
			}
		}
		
		return decimal.Parse(totalPrice.ToString());
	}
 

    private Dictionary<int, DaysPrices> fillPricesByDays(ClAsciFile item)
    {
        Dictionary<int, DaysPrices> pricesByDays = new Dictionary<int, DaysPrices>();
        DataSet pricesFromDB = getPricesFromDB(item);
		if (isDataSetRowsNotEmpty(pricesFromDB))
		{
			if (pricesFromDB.Tables[0].Rows != null)
			{
				foreach (DataRow priceRow in pricesFromDB.Tables[0].Rows)
				{
					DaysPrices dp = new DaysPrices()
					{
						Neto = new DaysPrices.PriceByDay()
						{
							Sunday = double.Parse(priceRow["night_netto_1"].ToString()),
							Monday = double.Parse(priceRow["night_netto_2"].ToString()),
							Tuesday = double.Parse(priceRow["night_netto_3"].ToString()),
							Wednesday = double.Parse(priceRow["night_netto_4"].ToString()),
							Thursday = double.Parse(priceRow["night_netto_5"].ToString()),
							Friday = double.Parse(priceRow["night_netto_6"].ToString()),
							Saturday = double.Parse(priceRow["night_netto_7"].ToString())
						},
						Bruto = new DaysPrices.PriceByDay()
						{
							Sunday = double.Parse(priceRow["night_bruto_1"].ToString()),
							Monday = double.Parse(priceRow["night_bruto_2"].ToString()),
							Tuesday = double.Parse(priceRow["night_bruto_3"].ToString()),
							Wednesday = double.Parse(priceRow["night_bruto_4"].ToString()),
							Thursday = double.Parse(priceRow["night_bruto_5"].ToString()),
							Friday = double.Parse(priceRow["night_bruto_6"].ToString()),
							Saturday = double.Parse(priceRow["night_bruto_7"].ToString())
						}
					};

					pricesByDays.Add(int.Parse(priceRow["hotel_room_type_id"].ToString()), dp);
				}
			}
			else
			{
				Debug.WriteLine('1');
			}
		}
		else
		{
			if (Convert.ToInt16(item.NightNum) == 1)
			{
				//sb.Append("<tr><td>" + item.CinvoiceNum.ToString() + "</td><td>" + DAL_SQL.GetRecord("Agency_Admin.dbo.SUPPLIERS", "name_1255", "id", item.InnerUse_AgencyHotelId) + "</td><td>" + item.FromDate.ToString("dd/MM/yy") + "</td><td>" + item.Todate.ToString("dd/MM/yy") + "</td></tr>");
				DaysPrices dp = new DaysPrices()
				{
					Neto = new DaysPrices.PriceByDay()
					{
						Sunday = double.Parse(item.CinvTotal),
						Monday = double.Parse(item.CinvTotal),
						Tuesday = double.Parse(item.CinvTotal),
						Wednesday = double.Parse(item.CinvTotal),
						Thursday = double.Parse(item.CinvTotal),
						Friday = double.Parse(item.CinvTotal),
						Saturday = double.Parse(item.CinvTotal)
					},
					Bruto = new DaysPrices.PriceByDay()
					{
						Sunday = double.Parse(item.CinvTotal),
						Monday = double.Parse(item.CinvTotal),
						Tuesday = double.Parse(item.CinvTotal),
						Wednesday = double.Parse(item.CinvTotal),
						Thursday = double.Parse(item.CinvTotal),
						Friday = double.Parse(item.CinvTotal),
						Saturday = double.Parse(item.CinvTotal)
					}
				};

				string docketId = DAL_SQL.GetRecord("VOUCHERS", "docket_id", item.InnerUse_VoucherId, "id");				
                item.InnerUse_Erkev_Type_Name = DAL_SQL.GetRecord("BUNDLES", "erkev_type", docketId + " and erkev_type <> '' ", "docket_id").Trim();
				item.InnerUse_Erkev_Type = getHotelRoomsTypes(item.InnerUse_Erkev_Type_Name);
				pricesByDays.Add(int.Parse(item.InnerUse_Erkev_Type[0].ToString()), dp);
				
			}
		}
		
        return pricesByDays;
    }

    private decimal GetPriceAndPriceZakai(ClAsciFile item, string makattmp, int p1, DateTime CurrentDate, int peoplecount, short p2, Dictionary<int, DaysPrices> daysPrices, out decimal priceZakai, string iVatPercant)
    {
        double PriceTotal = 0;
        DaysPrices dp = new DaysPrices();
        priceZakai = 0;

        //sums up the prices (e.g ZakaiAnd2Melavim is price for couple + price single
        foreach (int priceForErkev in item.InnerUse_Erkev_Type)
        {
            //get prices by erkev
            daysPrices.TryGetValue(priceForErkev, out dp);
            //sum the total
            
			PriceTotal += dp.Bruto.getBrutoWithoutVat(CurrentDate.DayOfWeek, iVatPercant);
			//PriceTotal += dp.Bruto.GetPriceByDayOfWeek(CurrentDate.DayOfWeek);
			//PriceTotal += dp.Neto.GetPriceByDayOfWeek(CurrentDate.DayOfWeek);
            if (item.InnerUse_Erkev_Type_Name == "ZakaiBeTashlumAndMelaveLeLoTashlum")
            {
				priceZakai += (decimal)dp.Bruto.getBrutoWithoutVat(CurrentDate.DayOfWeek, iVatPercant);
				//priceZakai += (decimal)dp.Bruto.GetPriceByDayOfWeek(CurrentDate.DayOfWeek);
				//priceZakai += (decimal)dp.Neto.GetPriceByDayOfWeek(CurrentDate.DayOfWeek);
            }
        }

        return decimal.Parse(PriceTotal.ToString());
    }

    private CLReferral fillReferral(string[] param, ClAsciFile item, string makat, int LineNO, int nightCount, DateTime TreatStartDate)
    {
        CLReferral clreferral = new CLReferral();

		if (Utils.IsWeekend(TreatStartDate))
		{
			switch(makat)
			{
				case "027237":
					makat = "036399";
					break;
				case "027238":
					makat = "036397";
					break;
				case "027240":
					makat = "036398";
					break;
				case "027241":
					makat = "036396";
					break;
				case "027242":
					makat = "036395";
					break;
				case "030772":
					makat = "036393";
					break;
				case "030773":
					makat = "036394";
					break;
			}
			//035727, 029940 stays the same.
		}
		
        clreferral.DocetNum = param[37];
        clreferral.HotelAreaCode = param[39];
        clreferral.HotelCode = getHotelId(param[38].TrimStart('0')).PadLeft(4, '0');
        clreferral.HotelMelave1 = param[41].Trim();
        clreferral.HotelMelave2 = param[42].Trim();
        clreferral.HotelPrcntHoliday = param[43];
        clreferral.HotelRamaCode = getBasisId(param[40]);
        clreferral.IdCard = param[25];
        clreferral.LineNO = LineNO.ToString().PadLeft(5, '0');
        clreferral.LineType = param[24];
        clreferral.ModCatNum = makat.ToString();
        clreferral.NumEscort = param[34];
        clreferral.PatientFileNum = param[26];
        clreferral.Quantity = nightCount.ToString();
        clreferral.ReferralNum = param[22];
        clreferral.ReferralVoucher = param[23];
        //excelcells = excelworksheet.get_Range("H10", "H10");
        //clreferral.ReferrTotal = excelcells.Value.ToString();
        DateTime TreatEndDate = TreatStartDate.AddDays(nightCount);
        clreferral.TreatEndDate = TreatEndDate.Year.ToString() + TreatEndDate.Month.ToString().PadLeft(2, '0') + TreatEndDate.Day.ToString().PadLeft(2, '0');
        clreferral.TreatStartDate = TreatStartDate.Year.ToString() + TreatStartDate.Month.ToString().PadLeft(2, '0') + TreatStartDate.Day.ToString().PadLeft(2, '0');
        clreferral.UOM = param[32]; // hotel - 3 other - 27
        return clreferral;
    }
	
	private string CreateXmlFile(ClAsciFile item)
    {
        string xml = "<?xml version=\"1.0\" encoding=\"windows-1255\"?>" +
                       "<MOD_CareInvoice>" +
                            "<MsgHeader>" +
                                 "<MsgType>" + item.MsgType + "</MsgType>" +
                                 "<MsgMapCode>" + item.MsgMapCode + "</MsgMapCode>" +
                                 "<MsgDocNum>" + item.MsgDocNum + "</MsgDocNum>" +
                                 "<MsgApkey>" + item.MsgApkey + "</MsgApkey>" +
                                 "<MsgSender>" + item.MsgSender + "</MsgSender>" +
                                 "<MsgReceiver>" + item.MsgReceiver + "</MsgReceiver>" +
                                 "<MsgCreDate>" + item.MsgCreDate + "</MsgCreDate>" +
                                 "<MsgCreTime>" + item.MsgCreTime + "</MsgCreTime>" +
                            "</MsgHeader>" +
                            "<CinvHeader>" +
                                "<DocumentType>" + item.DocumentType + "</DocumentType>" +
                                "<InvoiceType>" + item.InvoiceType + "</InvoiceType>" +
                                "<ActionType>" + item.ActionType + "</ActionType>" +
                                "<OriginaInvNum></OriginaInvNum>" +
                                "<NumCorrectInv></NumCorrectInv>" +
                                "<CinvoiceRefBy>" + item.CinvoiceRefBy + "</CinvoiceRefBy>" +
                                "<CinvoiceNum>" + item.CinvoiceNum + "</CinvoiceNum>" +
                                "<BunchNum>" + (string.IsNullOrEmpty(item.BunchNum.Trim()) ? "" : item.BunchNum.Trim()) + "</BunchNum>" +
                                "<CinvDate>" + item.CinvDate + "</CinvDate>" +
                                "<CinvPeriod>" + item.CinvPeriod + "</CinvPeriod>" +
                                "<MsgCreationDate>" + item.MsgCreationDate + "</MsgCreationDate>" +
                                "<InvoiceDate>" + item.InvoiceDate + "</InvoiceDate>" +
                                "<LiasionUnit>" + item.LiasionUnit + "</LiasionUnit>" +
                                "<Supplier>" +
                                    "<MODNumber>" + item.MODNumber + "</MODNumber>" +
                                "</Supplier>" +
                                "<Cinvoice>" +
                                    "<CinvTotal>" + item.CinvTotal + "</CinvTotal>" +
                                    "<VATPercentage>" + item.VATPercentage + "</VATPercentage>" +
                                    "<VATTotal>" + item.VATTotal + "</VATTotal>" +
                                    "<Currency>" + item.Currency + "</Currency>" +
                                    "<DiscountAmnt>" + item.CoDiscountamnt + "</DiscountAmnt>" +
                                    "<RemTextInv>" + item.CoRemTextLine1 + "</RemTextInv>" +
                                    "<TotalLines>" + item.Referral.Count().ToString().PadLeft(5, '0') + "</TotalLines>" +
                                    "</Cinvoice>" +
                                    "</CinvHeader>" +
                                    "<CinvLines>"
                                        ;
        foreach (CLReferral refItem in item.Referral)
        {
            xml += "<Referral><LineNo>" + refItem.LineNO + "</LineNo>" +
                "<ReferralNum>" + refItem.ReferralNum + "</ReferralNum>" +
                "<ReferralVoucher>" + refItem.ReferralVoucher + "</ReferralVoucher>" +
                "<LineType>" + refItem.LineType + "</LineType>" +
                "<Patient>" +
                    "<IdCard>" + refItem.IdCard + "</IdCard>" +
                    "<PatientFileNum>" + refItem.PatientFileNum + "</PatientFileNum>" +
                "</Patient>" +
                "<Treatment>" +
                   "<ModCatNum>" + refItem.ModCatNum + "</ModCatNum>" +
                   "<TreatStartDate>" + refItem.TreatStartDate + "</TreatStartDate>" +
                   "<TreatEndDate>" + refItem.TreatEndDate + "</TreatEndDate>" +
                "</Treatment>" +
                "<ReferrTotals>" +
                    "<Quantity>" + refItem.Quantity + "</Quantity>" +
                    "<UOM>" + refItem.UOM + "</UOM>" +
                    "<UOMPrice>" + refItem.UOMPrice + "</UOMPrice>" +
                    "<ReferrTotal>" + refItem.ReferrTotal + "</ReferrTotal>" +
                    "<SuppDiscount>" + refItem.SuppDiscount + "</SuppDiscount>" +
                    "<SuppDiscountAmt>" + refItem.SuppDiscountAmt + "</SuppDiscountAmt>" +
                    "<SuppRemTextLine></SuppRemTextLine>" +
                "</ReferrTotals>" +
                "<AddExpance>" +
                "<NumEscort>" + refItem.NumEscort + "</NumEscort>" +
                "<NumEscortPatient>" + refItem.NumEscortPatient + "</NumEscortPatient>" +
                "<AmountPayPatient>" + refItem.AmountPayPatient + "</AmountPayPatient>" +
                "<DocetNum>" + refItem.DocetNum + "</DocetNum>" +
                "<HotelCode>" + refItem.HotelCode + "</HotelCode>" +
                "<HotelInvNo>" + refItem.HotelInvNo + "</HotelInvNo>" +
                "<HotelInvAmnt>" + refItem.HotelInvAmnt + "</HotelInvAmnt>" +
                "<HotelAreaCode>" + refItem.HotelAreaCode + "</HotelAreaCode>" +
                "<HotelRamaCode>" + refItem.HotelRamaCode + "</HotelRamaCode>" +
                "<HotelMelave1>" + refItem.HotelMelave1Tmp + "</HotelMelave1>" +
                "<HotelMelave2>" + refItem.HotelMelave2Tmp + "</HotelMelave2>" +
                "<HotelPrcntHoliday>" + refItem.HotelPrcntHoliday + "</HotelPrcntHoliday>" +
                "</AddExpance></Referral>";


        }

        xml += "</CinvLines>" +
        "</MOD_CareInvoice>";

        return xml;
    }

    //private string CreateXmlFile(ClAsciFile item)
    //{
    //    string xml =  "<?xml version=\"1.0\" encoding=\"windows-1255\"?>"+
	//						"<CareInvoice>" +
    //                        "<ThatMsg>" +
    //                             "<MsgType>careinvoice</MsgType>" +
    //                             "<MsgMapCode>M18</MsgMapCode>" +
    //                             "<MsgMapVer>10</MsgMapVer>" +
    //                             "<MsgMapForm>XB2B-1</MsgMapForm>" +
    //                             "<MsgMapAuthor>MOD</MsgMapAuthor>" +
    //                             "<MsgSecLvl></MsgSecLvl>" +
    //                             "<MsgSecKey></MsgSecKey>" +
    //                             "<MsgMapTitle></MsgMapTitle>" +
    //                        "</ThatMsg>"+
    //                        "<CinvHeader>" +
    //                            "<DocumentType>" + item.DocumentType + "</DocumentType>" +
    //                            "<InvoiceType>" + item.InvoiceType + "</InvoiceType>" +
    //                            "<ActionType>" + item.ActionType + "</ActionType>" +
    //                            "<Originalinvnum></Originalinvnum>" +
    //                            "<NumCorrectInv></NumCorrectInv>" +
    //                            "<CinvoiceRefBy>" + item.CinvoiceRefBy + "</CinvoiceRefBy>" +
    //                            "<CinvoiceNum>" + item.CinvoiceNum + "</CinvoiceNum>" +
    //                            "<BunchNum>" + item.BunchNum + "</BunchNum>" +
    //                            "<CinvInDate>" + item.CinvDate + "</CinvInDate>" +
    //                            "<CinvPeriod>" + item.CinvPeriod + "</CinvPeriod>" +
    //                            "<MsgCreationDate>" + item.MsgCreationDate + "</MsgCreationDate>" +
    //                            "<InvoiceDate>" + item.InvoiceDate + "</InvoiceDate>" +
    //                            "<LiaisonUnit>" + item.LiasionUnit + "</LiaisonUnit>" +
    //                            "<Supplier>" +
    //                                "<MODNumber>" + item.MODNumber + "</MODNumber>" +
    //                            "</Supplier>" +
    //                            "<Cinvoice>" +
    //                                "<CinvTotal>" + item.CinvTotal + "</CinvTotal>" +
    //                                "<VATPercentage>" + item.VATPercentage + "</VATPercentage>" +
    //                                "<VATTotal>" + item.VATTotal + "</VATTotal>" +
    //                                "<Currency>" + item.Currency + "</Currency>" +
    //                                "<DiscountAmnt>" + item.CoDiscountamnt + "</DiscountAmnt>" +
    //                                "<RemTextInv>" + item.CoRemTextLine1 + "</RemTextInv>" +
    //                                "<TotalLines>" + item.Referral.Count().ToString().PadLeft(5, '0') + "</TotalLines>" +
    //                                "</Cinvoice>" +
    //                                "</CinvHeader>" +
    //                                "<CinvLines>";
    //    foreach (CLReferral refItem in item.Referral)
    //    {
    //        xml += "<Referral><LineNo>" + refItem.LineNO + "</LineNo>" +
    //            "<ReferralNum>" + refItem.ReferralNum + "</ReferralNum>" +
    //            "<ReferralVoucher>" + refItem.ReferralVoucher + "</ReferralVoucher>" +
    //            "<LineType>" + refItem.LineType + "</LineType>" +
    //            "<Patient>" +
    //                "<IdCard>" + refItem.IdCard + "</IdCard>" +
    //                "<PatientFileNum>" + refItem.PatientFileNum + "</PatientFileNum>" +
    //            "</Patient>" +
    //            "<Treatment>" +
    //               "<ModCatNum>" + refItem.ModCatNum + "</ModCatNum>" +
    //               "<TreatStartDate>" + refItem.TreatStartDate + "</TreatStartDate>" +
    //               "<TreatEndDate>" + refItem.TreatEndDate + "</TreatEndDate>" +
    //            "</Treatment>" +
    //            "<ReferrTotals>" +
    //                "<Quantity>" + refItem.Quantity + "</Quantity>" +
    //                "<UOM>" + refItem.UOM + "</UOM>" +
    //                "<UOMPrice>" + refItem.UOMPrice + "</UOMPrice>" +
    //                "<ReferrTotal>" + refItem.ReferrTotal + "</ReferrTotal>" +
    //                "<SuppDiscount>" + refItem.SuppDiscount + "</SuppDiscount>" +
    //                "<SuppDiscountAmt>" + refItem.SuppDiscountAmt + "</SuppDiscountAmt>" +
    //                "<RemTextLine></RemTextLine>" +
    //            "</ReferrTotals>" +
    //            "<AddExpance>" +
    //            "<NumEscort>" + refItem.NumEscort + "</NumEscort>" +
    //            "<NumEscortPatient>" + refItem.NumEscortPatient + "</NumEscortPatient>" +
    //            "<AmountPayPatient>" + refItem.AmountPayPatient + "</AmountPayPatient>" +
    //            "<DocetNum>" + refItem.DocetNum + "</DocetNum>" +
    //            "<HotelCode>" + refItem.HotelCode + "</HotelCode>" +
    //            "<HotelInvNo>" + refItem.HotelInvNo + "</HotelInvNo>" +
    //            "<HotelInvAmnt>" + refItem.HotelInvAmnt + "</HotelInvAmnt>" +
    //            "<HotelAreaCode>" + refItem.HotelAreaCode + "</HotelAreaCode>" +
    //            "<HotelRamaCode>" + refItem.HotelRamaCode + "</HotelRamaCode>" +
    //            "<HotelMelave1>" + refItem.HotelMelave1Tmp + "</HotelMelave1>" +
    //            "<HotelMelave2>" + refItem.HotelMelave2Tmp + "</HotelMelave2>" +
    //            "<HotelPrcntHoliday>" + refItem.HotelPrcntHoliday + "</HotelPrcntHoliday>" +
    //            "</AddExpance></Referral>";
    //
    //
    //    }
    //
    //    xml += "</CinvLines>";
	//	xml +="</CareInvoice>";
    //
    //
    //    return xml;
    //}

    #region Chen

    public DataSet getPricesFromDB(ClAsciFile item)
    {

        DataSet retVal = null;
        string supplierId = item.InnerUse_AgencyHotelId;
        string fromDate = item.FromDate.ToString("dd-MMM-yy");
        string toDate = item.Todate.ToString("dd-MMM-yy");
        int nights = (item.Todate - item.FromDate).Days;
        string priceId = string.Empty;

        //Query to get the price that availabe for the specific dates on the requested hotel.
        //AND isActive = 1   removed because some price did not found.
		string getHotelPricesSql = string.Format(@"
                            SELECT id, price_name, from_date, to_date into #temp
                            FROM PR_HOTEL_PRICES 
                            WHERE {0} = hotel_supplier_id
                                AND cast('{1}' as smalldatetime) >= from_date 
                                AND cast('{2}' as smalldatetime) <= to_date 
                                

                            SELECT id FROM #temp 
                            WHERE id in (SELECT hotel_price_id FROM PR_HOTEL_PRICES_DATES WHERE hotel_price_id = #temp.id 
                                			AND from_date = cast('{1}' as smalldatetime) AND to_date = cast('{2}' as smalldatetime) AND status = 1
                                            OR
                                            {3} >= min_nights AND {3} <= max_nights AND {3} >= min_nights_mid AND cast('{1}' as smalldatetime) >= from_date AND cast('{2}' as smalldatetime) <= to_date AND status = 1 
                                            OR 
                                            {3} >= min_nights AND {3} <= max_nights AND {3} >= min_nights_end AND cast('{1}' as smalldatetime) >= from_date AND cast('{2}' as smalldatetime) <= to_date AND status = 1
                                            OR    
                                            {3} >= min_nights AND 0 <= max_nights AND cast('{1}' as smalldatetime) >= from_date AND cast('{2}' as smalldatetime) <= to_date AND status = 1) drop table #temp ",
      supplierId, fromDate, toDate, nights);

        //Gets all the prices that relevant to the specific order.
		DataSet pricesIdsForSelectedBundle = null;
		try
		{
			pricesIdsForSelectedBundle = DAL_SQL.RunSqlDataSet(getHotelPricesSql);
		}
		catch (Exception ex)
		{
			//Logger.EmptyLog(getHotelPricesSql, eLogger.INVOICE_XML_ERROR);
		}
        //Check that the dataSet is not empty.
		
        if (isDataSetRowsNotEmpty(pricesIdsForSelectedBundle))
        {
            //if (pricesIdsForSelectedBundle.Tables[0].Rows.Count == 1)
            //{
                foreach (DataRow priceRow in pricesIdsForSelectedBundle.Tables[0].Rows)
                {
                    //If got only 1 price match so save the priceId and days.
                    priceId = priceRow["id"].ToString();
                }
            //}
        }
		else
		{
			supplierId = item.HotelId;
			getHotelPricesSql = string.Format(@"
                            SELECT id, price_name, from_date, to_date into #temp
                            FROM PR_HOTEL_PRICES 
                            WHERE {0} = hotel_supplier_id
                                AND cast('{1}' as smalldatetime) >= from_date 
                                AND cast('{2}' as smalldatetime) <= to_date 
                                

                            SELECT id FROM #temp 
                            WHERE id in (SELECT hotel_price_id FROM PR_HOTEL_PRICES_DATES WHERE hotel_price_id = #temp.id 
                                			AND from_date = cast('{1}' as smalldatetime) AND to_date = cast('{2}' as smalldatetime) AND status = 1
                                            OR
                                            {3} >= min_nights AND {3} <= max_nights AND {3} >= min_nights_mid AND cast('{1}' as smalldatetime) >= from_date AND cast('{2}' as smalldatetime) <= to_date AND status = 1 
                                            OR 
                                            {3} >= min_nights AND {3} <= max_nights AND {3} >= min_nights_end AND cast('{1}' as smalldatetime) >= from_date AND cast('{2}' as smalldatetime) <= to_date AND status = 1
                                            OR    
                                            {3} >= min_nights AND 0 <= max_nights AND cast('{1}' as smalldatetime) >= from_date AND cast('{2}' as smalldatetime) <= to_date AND status = 1) drop table #temp ",
      supplierId, fromDate, toDate, nights);
	  
	  //Logger.EmptyLog(getHotelPricesSql, eLogger.INVOICE_XML_ERROR);
	  
			DataSet pricesIdsForSelectedBundle2 = DAL_SQL.RunSqlDataSet(getHotelPricesSql);
			//Check that the dataSet is not empty.
			
			if (isDataSetRowsNotEmpty(pricesIdsForSelectedBundle2))
			{
				//if (pricesIdsForSelectedBundle2.Tables[0].Rows.Count == 1)
				//{
					foreach (DataRow priceRow in pricesIdsForSelectedBundle2.Tables[0].Rows)
					{
						//If got only 1 price match so save the priceId and days.
						priceId = priceRow["id"].ToString();
					}
				//}
			}
		}

        if (!string.IsNullOrEmpty(priceId))
        {
            string getPriceDetailsSql = @"SELECT * 
								FROM PR_HOTEL_PRICES_DETAILS
								WHERE price_id = " + priceId;

            DataSet requstedPriceDetails = DAL_SQL.RunSqlDataSet(getPriceDetailsSql);

            retVal = requstedPriceDetails;
        }
		else
		{
			if (Convert.ToInt16(item.NightNum) != 1)
			{
				sb.Append("<tr><td>" + item.CinvoiceNum.ToString() + "</td><td>" + DAL_SQL.GetRecord("Agency_Admin.dbo.SUPPLIERS", "name_1255", "id", item.InnerUse_AgencyHotelId) + "</td><td>" + item.FromDate.ToString("dd/MM/yy") + "</td><td>" + item.Todate.ToString("dd/MM/yy") + "</td></tr>");
			}
			//Logger.EmptyLog(getHotelPricesSql, eLogger.INVOICE_XML_ERROR);
		}

        return retVal;
    }

    private bool isDataSetRowsNotEmpty(DataSet iDataSet)
    {//Check if the DataSet has rows in first table.
        bool isNotEmpty = false;

        if (iDataSet != null && iDataSet.Tables != null && iDataSet.Tables.Count > 0
                && iDataSet.Tables[0].Rows != null && iDataSet.Tables[0].Rows.Count > 0)
        {
            isNotEmpty = true;
        }

        return isNotEmpty;
    }

    private int getDayFromDate(DateTime iDate)
    {

        //Enum of days in week (C# code)
        DayOfWeek dayInWeek = iDate.DayOfWeek;
        int day = 0;

        switch (dayInWeek)
        {
            case DayOfWeek.Sunday:
                day = 1;
                break;
            case DayOfWeek.Monday:
                day = 2;
                break;
            case DayOfWeek.Tuesday:
                day = 3;
                break;
            case DayOfWeek.Wednesday:
                day = 4;
                break;
            case DayOfWeek.Thursday:
                day = 5;
                break;
            case DayOfWeek.Friday:
                day = 6;
                break;
            case DayOfWeek.Saturday:
                day = 7;
                break;

            default:
                day = -1;
                break;
        }

        return day;
    }

    private List<int> getHotelRoomsTypes(string iBaseTypeGOV)
    {
        //How many travellers GOV pays for.
        const int single = 1;
        const int couple = 2;
		const int ZakaiAndMelaveBeTashlum = 2330;
        List<int> hotelRoomsTypeId = new List<int>();

        switch (iBaseTypeGOV.Trim())
        {
            case "Zakai":
                hotelRoomsTypeId.Add(single);
                break;
            case "ZakaiAndMelave":
                hotelRoomsTypeId.Add(couple);
                break;
            case "ZakaiAnd2Melavim":
                hotelRoomsTypeId.Add(single);
                hotelRoomsTypeId.Add(couple);
                break;
            case "ZakaiAndMelaveBeTashlum":
                hotelRoomsTypeId.Add(ZakaiAndMelaveBeTashlum);
                break;
            case "ZakaiAndMelaveBeTashlumHelekTkufa": //Chen. Verify
                hotelRoomsTypeId.Add(single);
                break;
            case "ZakaiBeTashlumAndMelaveLeLoTashlum": //makat 40 
                hotelRoomsTypeId.Add(ZakaiAndMelaveBeTashlum); //was single now ZakaiAndMelaveBeTashlum
                break;
            case "ZakaiAndMelaveTmuratZakaut":
                hotelRoomsTypeId.Add(couple);
                break;
            default:
                // handleError("No 'erkev_type' found", eLogger.DEBUG);
                break;
        }

        return hotelRoomsTypeId;
    }

    private string getAddId(string bundleId)
    {
        string addId = string.Empty;

        string travIdFromBTT = string.Empty;
        string makat = string.Empty;
        string travId = string.Empty;

        travIdFromBTT = DAL_SQL.GetRecord("BUNDLES_to_TRAVELLERS", "traveller_id", bundleId, "bundle_id");
        makat = DAL_SQL.GetRecord("TRAVELLERS", "gov_makat_number", travIdFromBTT, "id");

        if (makat == "027235")
        {
            addId = "1";
        }
        else
        {
            if (makat == "027236")
            {
                addId = "2";
            }
            else
            {
                if (makat == "027242")
                {
                    travId = DAL_SQL.GetRecord("TRAVELLERS", "id_no", travIdFromBTT, "traveller_id");
                    DataSet ds = DAL_SQL.RunSqlDataSet("SELECT ItemSKU FROM Gov_TRAVLLERS WHERE TravellerID = " + travId);
                    if (isDataSetRowsNotEmpty(ds))
                    {
                        foreach (DataRow row in ds.Tables[0].Rows)
                        {
                            if (row["ItemSKU"].ToString() == "027236")
                            {
                                addId = "2";
                                break;
                            }
                            else
                            {
                                if (row["ItemSKU"].ToString() == "027235")
                                {
                                    addId = "1";
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }

        return addId;
    }
	
	private void saveFileInGovDirZip(object iDoc, string iFileNameXml, string textToSave, string path)
	{
		path = path.Replace("CareInvoice\\", "CareInvoiceForZip\\");
		saveFileInGovDir(iDoc, iFileNameXml, textToSave, path);
	}
	
	private void saveFileInGovDir(object iDoc, string iFileNameXml, string textToSave, string path)
    {
		if (!path.Contains(iFileNameXml))
			path = path + iFileNameXml;
		
        try
        {
            if (iDoc is XmlDocument)
			{
                (iDoc as XmlDocument).Save(path);

				//Save the xml and then cleanup
				XmlWriterSettings settings = new XmlWriterSettings { Indent = true };
				XmlWriter writer = XmlWriter.Create(path, settings);
				(iDoc as XmlDocument).Save(writer);
			}
            else if (iDoc is StringWriter)
            {
                //Save the .txt file
                System.IO.File.WriteAllText(path, textToSave);
            }
        }
        catch (Exception ex)
        {
            Logger.Log("Failed to save the mimushim.xml file." + ex.Message);

            string message = "אירעה תקלה בעת שמירת הקובץ.";
            ClientScript.RegisterStartupScript(this.GetType(), "Popup", "ShowPopup('" + message + "');", true);
        }
    }
	
	private string getFileName(string invoiceNumber, string invoiceDate, int index)
	{		
        string MODNUMBER = "0083695749";
		string path = HttpContext.Current.Request.MapPath("Logs\\CareInvoice\\");
        string[] actualFilesFromDir = Directory.GetFiles(path );

        string MsgDocNum = (actualFilesFromDir.Length + index).ToString();
		//path = HttpContext.Current.Request.MapPath("Logs\\MimushimFiles\\CareInvoice\\MsgDocNum\\");
        string delimiterFileName = "_";
		
		string MsgReceiver = "M18_SHIKUM";
		string MsgTimeStamp = DateTime.Now.ToString("yyyyddMMhhmmss");
        //string fileExtensionXml = ".xml";
		string fileExtensionXml = ".xml";

        //Set the file name.
		string fileNameXml = "MOD_CareInvoice";
		fileNameXml += delimiterFileName + MsgReceiver;
        fileNameXml += delimiterFileName + MODNUMBER;
		fileNameXml += delimiterFileName + invoiceNumber;
		fileNameXml += delimiterFileName + invoiceDate;
        fileNameXml += delimiterFileName + MsgDocNum;
        fileNameXml += delimiterFileName + MsgTimeStamp;
        fileNameXml += fileExtensionXml;
		
		return fileNameXml;
	}
	
	//Save a zip file for the directories in requested path.
	private void saveZip(string DirString, string savedZipPath)
	{
		string[] MainDirs = Directory.GetDirectories(DirString);

		
		for (int i = 0; i < MainDirs.Length; i++)
		{
			if (MainDirs[i] == "D:\\inetpub\\GovSystem\\Logs\\CareInvoiceForZip")
			{
				using (ZipFile zip = new ZipFile())
				{
					zip.UseUnicodeAsNecessary = true;
					zip.AddDirectory(MainDirs[i]);
					zip.CompressionLevel = Ionic.Zlib.CompressionLevel.BestCompression;
					zip.Comment = "This zip was created at " + System.DateTime.Now.ToString("G");
					//zip.Save(string.Format("C:\\Users\\User\\Desktop\\zipped.zip"));   
					zip.Save(string.Format(savedZipPath));   
				}
			}
		}
	}
	
    #endregion
}
