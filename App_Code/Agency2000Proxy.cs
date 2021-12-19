using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Collections.Generic;
using System.Xml;
using System.Text;
using System.IO;
using System.Globalization;
using System.Threading;

/// <summary>
/// Summary description for Agency2000Proxy
/// </summary>
public static class Agency2000Proxy
{
    private static string AgencyXmlServicesBaseUrl = ConfigurationManager.AppSettings.Get("AgencyXmlServices");
    private static string AgencyXmlServicesPricesBaseUrl = ConfigurationManager.AppSettings.Get("AgencyXmlServicesPrices");
    private static string AgencyXmlServicesCreateDocketUrl = ConfigurationManager.AppSettings.Get("AgencyXmlServicesCreateDocket");

    private static string AgencyBclientId = ConfigurationManager.AppSettings.Get("AgencyDocketBclientId");

    //private static string AgencyUserId = AgencyUser.AgencyUserId;
    //private static string AgencyUserName = AgencyUser.AgencyUserName;
    //private static string AgencyPassword = AgencyUser.AgencyUserPassword;

    // agency xml services
    private static string baseTypesAction = "agency_on_bases_xml.asp";
    private static string hotelPricesAction = "hotel_price_selector_xml_server_proxy.aspx"; //"hotel_price_selector_xml_server.asp";
    private static string createDocketAction = "xmlService.asp";
    private static string updateAllocationAction = "hotel_room_check_xml_server.asp";


    #region PUBLIC methods

    public static List<BaseTypes> getAgencyBases(int hotelId, DateTime iFromDate, DateTime iToDate)
    {
        //XmlDocument doc = new XmlDocument();
        List<BaseTypes> bases = new List<BaseTypes>();
        DataSet allBases = DAL_SQL_Helper.GetAllDatesHaveBase(hotelId.ToString(), iFromDate, iToDate);
        foreach(DataRow row in allBases.Tables[0].Rows)
        {
            BaseTypes currBaseType = new BaseTypes();
            currBaseType.Id = int.Parse(row["id"].ToString());
            currBaseType.Name = row["name"].ToString();
            currBaseType.Description = row["description"].ToString();
            bases.Add(currBaseType);
        }
        /*bool isUsingNewPrices = (isNewPrice == "1");
		
		if (!isUsingNewPrices)
		{
			doc = getAgencyRequestXmlDoc(getAgencyBasesUrl(hotelId, allocationId));
			
			XmlNodeList list = doc.SelectNodes("//BASE");

			foreach (XmlNode node in list)
			{
				bases.Add(new BaseTypes()
				{
					Id = int.Parse(node.SelectSingleNode("ID").InnerText),
					Name = HttpUtility.UrlDecode(node.SelectSingleNode("NAME").InnerText),
					Description = HttpUtility.UrlDecode(node.SelectSingleNode("DESCRIPTION").InnerText)
				});
			}
		}
		else
		{
			DataSet ds = DAL_SQL.RunSqlDataSet("SELECT id FROM P_HOTEL_PRICES WHERE supplier_id = " + hotelId);
			string supplierPriceId = string.Empty;

			if (Utils.isDataSetRowsNotEmpty(ds))
			{
				supplierPriceId = ds.Tables[0].Rows[0]["id"].ToString();
				//DataSet baseAmounts = DAL_SQL.RunSqlDataSet("SELECT * FROM P_HOTEL_PRICE_TO_BASES WHERE hotel_price_id = " + supplierPriceId);
				DataSet baseAmounts = DAL_SQL.RunSqlDataSet("SELECT * FROM P_HOTEL_PRICES WHERE supplier_id = " + hotelId);
				
				if (Utils.isDataSetRowsNotEmpty(baseAmounts))
				{
					foreach (DataRow row in baseAmounts.Tables[0].Rows)
					{
						if (bool.Parse(row["status"].ToString()))
						{
							bases.Add(new BaseTypes()
							{
								Id = int.Parse(row["base_price_base_id"].ToString()),
								Name = DAL_SQL.GetRecord("Agency_Admin.dbo.HOTEL_ON_BASE", "name", row["base_price_base_id"].ToString(), "id"),
								Description = ""
							});
						}
					}
				}
			}
		}*/

        return bases;
    }

    public static List<HotelPrice> getHotelPrice(int hotelId, int allocationId, int baseId, DateTime fromDate, DateTime toDate, GovTraveller traveller, out string errorMessage, string isNewPrice, List<int> baseIds)
    {
		string selectedMelaveDates = isNewPrice;
		//Logger.Log("isNewPrice = " + isNewPrice);
		try
		{
			selectedMelaveDates = isNewPrice.Split('|')[1];
		}
		catch
		{
			selectedMelaveDates = string.Empty;
		}
		
		isNewPrice = isNewPrice.Split('|')[0];
		//Logger.Log("selectedMelaveDates = " + selectedMelaveDates);
		bool isUsingNewPrices = (isNewPrice == "1");
		bool hasAllocationError = false;
		XmlNodeList list = null;
        List<HotelPrice> prices = new List<HotelPrice>();
        XmlDocument doc = new XmlDocument();
        //Logger.Log("LogAA price response" + getAgencyHotelPricesUrl(hotelId, allocationId, baseId, fromDate, toDate, traveller));
        // Logger.Log(getAgencyHotelPricesUrl(hotelId, allocationId, baseId, fromDate, toDate, traveller), "LogAA price responce", @"Logs\log_" + "uurrll1" + ".txt");

		
		errorMessage = string.Empty;
		//if (!isUsingNewPrices)
		//{
		//	doc = getAgencyRequestXmlDoc(getAgencyHotelPricesUrl(hotelId, allocationId, baseId, fromDate, toDate, traveller));

		//	//Logger.Log("Log price response qqq" + doc.InnerXml);
		//	//Logger.Log("hotelId = " + hotelId + ", allocationId = " + allocationId + ", baseId = " + baseId +", fromDate = " + fromDate + ", toDate = " + toDate);
		//	//Logger.Log(doc.InnerXml, "Log price responce qqq", @"Logs\log_" + DateTime.Now.ToShortDateString() + ".txt");

		//	XmlNode error = doc.SelectSingleNode("//ERROR");
		//	XmlNode fatalError = doc.SelectSingleNode("//FATAL_ERROR");
		//	if (error != null || fatalError != null)
		//	{
		//		errorMessage = (error != null ? error.InnerText : fatalError.InnerText);
		//		// try arrange error description
		//		switch (errorMessage)
		//		{
		//			case "No seems data found.":
		//				errorMessage = "לא נימצאו מחירונים מתאימים.";
		//				break;
		//			case "No Data Found.":
		//				errorMessage = "לא נימצאו מחירונים.";
		//				break;
		//			case "Maximum number of monthly alocations":
		//				errorMessage = "אין יותר חדרים בהקצאה (מספר על)";
		//				hasAllocationError = true;
		//				break;
		//			default:
		//				break;
		//		}
		//		return null;
		//	}
		
		//	list = doc.SelectNodes("//ROW_DATA");
		//}
		//else
		//{
			//Logger.Log("before getFinelPrice");
			getAgencyHotelPricesUrl(hotelId, allocationId, baseId, fromDate, toDate, traveller);
			string areaIdAgency = DAL_SQL.GetRecord("Agency_Admin.dbo.SUPPLIERS", "area_id", hotelId.ToString(), "id");
			string areaIdgov = DAL_SQL.GetRecord("Agency_Admin.dbo.GENERAL_AREAS_TO_AREAS", "general_area_id", areaIdAgency, "area_id");
            string finalPriceXml;
            if (baseId == -1)
            {
                finalPriceXml = getFinalPrice(areaIdgov, hotelId.ToString(), fromDate, toDate, traveller, baseIds, baseId.ToString());
            }
            else
            {
                finalPriceXml = getFinalPrice(areaIdgov, hotelId.ToString(), fromDate, toDate, traveller, null, baseId.ToString());
            }
					
			XmlDocument docNewPrices = new XmlDocument();
			docNewPrices.LoadXml(finalPriceXml);
			list = docNewPrices.SelectNodes("//ROW_DATA");
		//}
		
		//Logger.Log("1");
		if (list != null)
		{
			//Logger.Log("2");
			foreach (XmlNode node in list)
			{
				//Logger.Log("3");
				HotelPrice currPrice = new HotelPrice();
				currPrice.PriceId = int.Parse(node.SelectSingleNode("PRICE_ID").InnerText);
				AgencyPricesSearch localPriceWs = new AgencyPricesSearch();
				string brutto, netto, coupleBrutto = string.Empty;
				
				//if selectedMelaveDates contains 0 then the its makat 027240. and the escort selected days to come (not all days)
				//if (selectedMelaveDates.Contains("0"))
				Logger.Log(traveller.TravellerId + " - traveller.ErkevType = " + traveller.ErkevType);
				if (traveller.ErkevType.Contains("ZakaiBeTashlumAndMelaveLeLoTashlum"))
				{
					//roomType is the composition here. "2330" is zakaiMesubsadAndMelaveBeTashlum
					string roomType = "2330";
					string bruttoNetto = localPriceWs.GetPriceByPriceIdAndRoomTypesAndDates("85", "3", node.SelectSingleNode("PRICE_ID").InnerText, 
																							roomType, fromDate.ToString("dd-MMM-yy"), toDate.ToString("dd-MMM-yy"), 
																							selectedMelaveDates, hotelId.ToString());
					//"Netto:" + priceDetails.AmountBrutto.ToString() + "|Brutto:" + priceDetails.AmountNetto.ToString()
					
					Logger.Log(traveller.TravellerId + " - bruttoNetto = " + bruttoNetto);
					brutto = bruttoNetto.Split('|')[1].Split(':')[1];
					netto = bruttoNetto.Split('|')[0].Split(':')[1];
					
					coupleBrutto = bruttoNetto.Split('|')[2].Split(':')[1];
				}
				else
				{
					brutto = node.SelectSingleNode("AMOUNT_NETTO").InnerText.Replace(",", "").Replace("%2C", "");
					netto = node.SelectSingleNode("AMOUNT_BRUTO").InnerText.Replace(",", "").Replace("%2C", "");
				}
				
				//Logger.Log("4");
				currPrice.PriceAmountNetto = Math.Round(double.Parse(brutto), 2);
				currPrice.PriceAmountBruto = Math.Round(double.Parse(netto), 2);
				currPrice.RoomState = node.SelectSingleNode("ROOMS_STATE").InnerText;
				currPrice.calculateAmountToShow(traveller);

				//Logger.Log("5");
				//Logger.Log("isUsingNewPrices = " + isUsingNewPrices);
				//Logger.Log("currPrice.PriceId = " + currPrice.PriceId);
				//Logger.Log("currPrice.PriceAmountNetto = " + currPrice.PriceAmountNetto);
				//Logger.Log("currPrice.PriceAmountBruto = " + currPrice.PriceAmountBruto);
				//Logger.Log("currPrice.RoomState = " + currPrice.RoomState);
				
				if (!string.IsNullOrEmpty(coupleBrutto))
				{
					currPrice.TravellerPriceToPay = Math.Round(double.Parse(coupleBrutto) - currPrice.PriceAmountBruto, 2);
					currPrice.TravellerPriceToPay = currPrice.TravellerPriceToPay * 1.08;
				}
				
				prices.Add(currPrice);
				//Logger.Log("6");
			}
		}
		
		// check reservation for allocation @PE
		if (!CheckRoomsAllocation(prices[0].PriceId, allocationId, fromDate, toDate, traveller))
		{
			Logger.Log("@PE CHECKROOM תאריך סגור");
			errorMessage = "תאריך סגור";
			return null;
		}
		
        return prices;
    }
	
	private static string getFinalPrice(string iAreaId, string iSupplierId, DateTime iFromDate, DateTime iToDate, GovTraveller iTraveller, List<int> iBaseIds, string iBaseId)
    {
        string result = string.Empty;
        List<string> compositions = new List<string>();
        AgencyPricesSearchWs.AgencyPricesSearch priceWs = new AgencyPricesSearchWs.AgencyPricesSearch();        
        string langStr = "1255";
        string priceType = "1";
        string clerkId = "1";
        string agencyId = "85";
        string systemType = "3";
        string baseId = iBaseId;

        if (iTraveller.Melave.Count == 2)
        {
            compositions.Add("2"); //couple
            compositions.Add("1"); //single
        }
        else if (iTraveller.Melave.Count == 1)
        {
            compositions.Add("2"); //couple
        }
        else
        {
            compositions.Add("1"); //single
        }

		Logger.Log("agencyId = " + agencyId + ", systemType = " + systemType + ", clerkId = " + clerkId + ", iAreaId = " + iAreaId + ", iSupplierId = " + iSupplierId + ", priceType = " + priceType + ", langStr = " + langStr + ", iFromDate = " + iFromDate.ToString("dd-MMM-yy") + ", iToDate = " + iToDate.ToString("dd-MMM-yy") + ", compositions = " + compositions[0] + ", baseId = " + baseId);
        if (iTraveller.Melave.Count <= 1)
        {
            if(iBaseId != "-1")
            {
                result = priceWs.getHotelPricePerArea(agencyId, systemType, clerkId, 
				    											     iAreaId, iSupplierId, priceType, langStr, 
																     iFromDate.ToString("dd-MMM-yy"), iToDate.ToString("dd-MMM-yy"), 
				    											     compositions[0], baseId);
            }
            else
            {
                result = priceWs.getHotelPricePerAreaByMultipleBaseIds(agencyId, systemType, clerkId,
                                                     iAreaId, iSupplierId, priceType, langStr,
                                                     iFromDate.ToString("dd-MMM-yy"), iToDate.ToString("dd-MMM-yy"),
                                                     compositions[0], iBaseIds.ToArray());
            }
            result = "<Room>" + result + "</Room>";
        }
        else
        {
            int oneRoom = 1;
            string tempResult = string.Empty;

            //iTraveller.Melave.Count = 2 now...
            for (int i = 0; i < iTraveller.Melave.Count; i++)
            {
                if (iBaseId != "-1")
                {
                    tempResult = priceWs.getHotelPricePerArea(agencyId, systemType, clerkId,
                                                                         iAreaId, iSupplierId, priceType, langStr,
                                                                         iFromDate.ToString("dd-MMM-yy"), iToDate.ToString("dd-MMM-yy"),
                                                                         compositions[i], baseId);
                }
                else
                {
                    tempResult = priceWs.getHotelPricePerAreaByMultipleBaseIds(agencyId, systemType, clerkId,
                                                         iAreaId, iSupplierId, priceType, langStr,
                                                         iFromDate.ToString("dd-MMM-yy"), iToDate.ToString("dd-MMM-yy"),
                                                         compositions[i], iBaseIds.ToArray());
                }
                //Logger.Log(tempResult);
                if (!string.IsNullOrEmpty(tempResult))
                {
                    if (tempResult != "CompositionNotFound")
                    {
                        result += "<Room>" + tempResult + "</Room>";
                    }
                    else
                    {
                        Logger.Log("CompositionNotFound for supplierId = " + iSupplierId + ", compositionId = " + compositions[i]);
                        //If one of the compositions not exists will return null.
                        return string.Empty;
                    }
                }
                else
                {
                    //Didn't get result
                    return string.Empty;
                }
            }
			
			Logger.Log("before combine - " + result);
			combinePriceForMoreThanOneRoom(ref result);
			Logger.Log("after combine - " + result);
        }
		
		//Logger.Log(result);
		result = Utils.convertToAgencyXml(result, agencyId, systemType, priceType, iAreaId, iFromDate, iToDate, "-1", langStr);
		
        return result;
    }

    private static void combinePriceForMoreThanOneRoom(ref string result)
    {
		result = "<Rooms>" + result.Replace("<Root>", "").Replace("</Root>", "") + "</Rooms>";
        XmlDocument xmlRoomsPrices = new XmlDocument();
        XmlDocument xmlFinalReturn = new XmlDocument();
        int i = 0, j = 0;
        decimal[] finalPriceBrutto = null;
        decimal[] finalPriceNetto = null;
        Dictionary<int, decimal[]> dailyPrices = null;
        Dictionary<int, decimal[]> dailyPricesNetto = null;
        int finalPricesAmount;
        int nights;

        xmlRoomsPrices.LoadXml(result);
        xmlFinalReturn.AppendChild(xmlFinalReturn.ImportNode(xmlRoomsPrices.SelectSingleNode("Rooms//Room"), true));
        finalPricesAmount = xmlRoomsPrices.SelectNodes("Rooms//Room")[0].SelectNodes("FinalPrices").Count;
        nights = xmlRoomsPrices.SelectNodes("Rooms//Room//FinalPrices")[0].SelectNodes("FinalPrice//FinalPricesPerDays//FinalPricePerDay").Count;
        //Initiate temp lengthes.
        finalPriceBrutto = new decimal[finalPricesAmount];
        finalPriceNetto = new decimal[finalPricesAmount];
        dailyPrices = new Dictionary<int, decimal[]>();
        dailyPricesNetto = new Dictionary<int, decimal[]>();
        for (int k = 0; k < finalPricesAmount; k++)
        {
            dailyPrices.Add(k, new decimal[nights]);
			dailyPricesNetto.Add(k, new decimal[nights]);
        }

        foreach (XmlNode xmlRoom in xmlRoomsPrices.SelectNodes("Rooms//Room"))
        {
            foreach (XmlNode xmlFinalPrice in xmlRoom.SelectNodes("FinalPrices"))
            {
                finalPriceBrutto[i] += decimal.Parse(xmlFinalPrice.SelectSingleNode("FinalPrice//FinalPriceBrutto").FirstChild.Value);
                finalPriceNetto[i] += decimal.Parse(xmlFinalPrice.SelectSingleNode("FinalPrice//FinalPriceNetto").FirstChild.Value);

                j = 0;
                foreach (XmlNode finalPricePerDayNode in xmlFinalPrice.SelectNodes("FinalPrice//FinalPricesPerDays//FinalPricePerDay"))
                {
                    dailyPrices[i][j] += decimal.Parse(finalPricePerDayNode.SelectSingleNode("Price").FirstChild.Value);
                    dailyPricesNetto[i][j] += decimal.Parse(finalPricePerDayNode.SelectSingleNode("Price").FirstChild.Value);
                    j++;
                }
                i++;
            }
            i = 0;
        }

        i = 0;
        j = 0;
        //Set the final xml
        foreach (XmlNode xmlNodeFinalReturn in xmlFinalReturn.SelectSingleNode("Room"))
        {
            xmlNodeFinalReturn.SelectSingleNode("FinalPrice//FinalPriceBrutto").FirstChild.Value = finalPriceBrutto[i].ToString();
            xmlNodeFinalReturn.SelectSingleNode("FinalPrice//FinalPriceNetto").FirstChild.Value = finalPriceNetto[i].ToString();
            foreach (XmlNode finalPricePerDayNode in xmlNodeFinalReturn.SelectNodes("FinalPrice//FinalPricesPerDays//FinalPricePerDay"))
            {
                finalPricePerDayNode.SelectSingleNode("Price").FirstChild.Value = dailyPrices[i][j].ToString();
                finalPricePerDayNode.SelectSingleNode("PriceNetto").FirstChild.Value = dailyPricesNetto[i][j].ToString();
                j++;
            }
            j = 0;
            i++;
        }

        result = xmlFinalReturn.OuterXml;
    }

    public static HotelPriceOrderResult createDocket(int hotelId, int priceId, int allocationId, DateTime fromDate, DateTime toDate, double amountNeto, double amountBruto, GovTraveller traveller, int areaId)
    {
		Logger.Log("In CreateDocket");
        HotelPriceOrderResult result = new HotelPriceOrderResult();
        string createDocketXml = string.Empty;
        XmlDocument doc = new XmlDocument();
        string usedMakat = ((traveller.SelectedMakat != null && traveller.SelectedMakat.Count > 0) ? traveller.SelectedMakat[0].ItemSKU : "0");
        TimeSpan ts = toDate - fromDate;
        int OrderNights = ts.Days;
        AgencyUser user = new AgencyUser();

        if (user.AgencyUserName.Length <= 0 || user.AgencyUserPassword.Length <= 0)
        {
            return new HotelPriceOrderResult()
            {
                OrderMessage = "עבר הרבה זמן מהתחברות אחרונה, נא לבצע כניסה מחדש"
            };
        }

        // Chen 25.9 always return empty string. changed the return value to get from web.config
        if (user.AgencySystemType.Length <= 0)
        {
            return new HotelPriceOrderResult()
            {
                OrderMessage = "עבר הרבה זמן מהתחברות אחרונה, נא לבצע כניסה מחדש"
            };
        }

        // check reservation for allocation
        if (!CheckRoomsAllocation(priceId, allocationId, fromDate, toDate, traveller))
        {
            return new HotelPriceOrderResult()
            {
                OrderMessage = "לא ניתן להזמין את כל החדרים מאלוקציאה."
            };
        }
		
		try
		{
			Dictionary<DateTime, string> AllocByDate = new Dictionary<DateTime, string>();
			TimeSpan allocTs = toDate - fromDate;
			int days = allocTs.Days;
			DateTime currDate = fromDate;
			string allocNum = "0";
			string curr_id = "0";
			
			Logger.EmptyLog("START $$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$", eLogger.DEBUG);
			Logger.EmptyLog(" ", eLogger.DEBUG);
			string logMessage = string.Empty;
			
			for (int j = 0; j < days; j++ )
			{
				currDate = fromDate.AddDays(j);
				curr_id = DAL_SQL.GetRecord("SUPPLIERS_TO_PRICES_DATES_ROOMS", "id", allocationId.ToString(), "(CAST(room_date AS smalldatetime) = CAST('" + currDate.ToString("dd-MMM-yy") + "' AS smalldatetime)) AND price_date_id");
				allocNum = DAL_SQL.GetRecord("SUPPLIERS_TO_PRICES_DATES_ROOMS", "rooms_ordered", curr_id, "id");
				logMessage += (currDate.ToString("dd-MMM-yy") + ":" + allocNum + " | ");
			}
			Logger.Log("before UpdateRoomsAllocation. travId = " + traveller.TravellerId + 
					   ", fromDate = " + fromDate.ToString("dd-MMM-yy") + 
					   ", toDate = " + toDate.ToString("dd-MMM-yy") +
					   ", allocationId = " + allocationId.ToString() + ", " +
					   logMessage);
			
			// commented by Igor on 2012.03.12 - not to update allocations, allocations will be updated at create voucher (in agency)
			// make reservation for allocation
			if (!UpdateRoomsAllocation(priceId, allocationId, fromDate, toDate, traveller))
			{
				//Logger.EmptyLog("#####################################################################", eLogger.DEBUG);
				Logger.EmptyLog("#####################################################################", eLogger.DEBUG);
				return new HotelPriceOrderResult()
				{
					OrderMessage = "אירעה שגיאה בעת לקיחת ההקצאות, אנא פנה למנהל"
					
				};
			}
			
			logMessage = "";
			
			for (int j = 0; j < days; j++)
			{
				currDate = fromDate.AddDays(j);
				curr_id = DAL_SQL.GetRecord("SUPPLIERS_TO_PRICES_DATES_ROOMS", "id", allocationId.ToString(), "(CAST(room_date AS smalldatetime) = CAST('" + currDate.ToString("dd-MMM-yy") + "' AS smalldatetime)) AND price_date_id");
				allocNum = DAL_SQL.GetRecord("SUPPLIERS_TO_PRICES_DATES_ROOMS", "rooms_ordered", curr_id, "id");
				logMessage += (currDate.ToString("dd-MMM-yy") + ":" + allocNum + " | ");
			}
			
			Logger.Log("after UpdateRoomsAllocation. travId = " + traveller.TravellerId + 
					   ", fromDate = " + fromDate.ToString("dd-MMM-yy") + 
					   ", toDate = " + toDate.ToString("dd-MMM-yy") +
					   ", allocationId = " + allocationId.ToString() + ", " +
					   logMessage);
			Logger.EmptyLog("END $$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$", eLogger.DEBUG);
		}
		catch(Exception exc)
		{
			Logger.EmptyLog("Exception on logging allocation before and after. " + exc.Message, eLogger.DEBUG);
		}

		
        //Chen 25.9
        //Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture("he-il");
        createDocketXml = getAgencyCreateDocketXML(hotelId, priceId, allocationId, fromDate, toDate, amountNeto, amountBruto, traveller, usedMakat);

        string createDocketXmlUrl = getAgencyCreateDocketUrl() + "&Query=" + createDocketXml;
        Logger.Log("ALog create docket createDocketXml" + createDocketXml);
        //Logger.Log("BLog create docket createDocketXmlUrl" + createDocketXmlUrl);
        Logger.Log("Log create docket request" + createDocketXmlUrl);
        //Logger.Log(createDocketXml, "ALog create docket createDocketXml", @"Logs\log_create_docket_" + DateTime.Now.ToShortDateString() + ".txt");
        //Logger.Log(createDocketXmlUrl, "BLog create docket createDocketXmlUrl", @"Logs\log_create_docket_" + DateTime.Now.ToShortDateString() + ".txt");
        //Logger.Log(createDocketXmlUrl, "Log create docket request", @"Logs\log_create_docket_" + DateTime.Now.ToShortDateString() + ".txt");
        doc = getAgencyRequestXmlDoc(createDocketXmlUrl);
        //Logger.Log(doc.InnerXml, "Log create docket response", @"Logs\log_create_docket_" + DateTime.Now.ToShortDateString() + ".txt");
        Logger.Log("Log create docket response " + doc.InnerXml);

        if (doc == null)
        {
			Logger.Log("doc == null | OrderMessage = לא ניתן לפתוח תיק.");
            return new HotelPriceOrderResult()
            {
                OrderMessage = "לא ניתן לפתוח תיק."
            };
        }

        // check if all nodes are ok
        XmlNodeList statuses = doc.SelectNodes("//STATUS");
		//Logger.Log(doc.InnerXml);
        foreach (XmlNode status in statuses)
        {
            if (status.InnerText.ToLower() != "ok")
            {
				Logger.Log("status.InnerText.ToLower() != ok | לא ניתן ליצור תיק, אנא פנה למנהל מערכת.");
                return new HotelPriceOrderResult()
                {
                    OrderMessage = "לא ניתן ליצור תיק, אנא פנה למנהל מערכת."
                };
            }
        }

        // all statuses are ok
        result.AgencyDocketId = doc.SelectSingleNode("/ROOT/DOCKET/DOCKET_ID").InnerText;
        result.AgencyVoucherId = doc.SelectSingleNode("/ROOT/SERVICES/HOTEL/VOUCHER_ID").InnerText;
        result.OrderCompleted = true;
        result.OrderMessage = "";

		if (!string.IsNullOrEmpty(result.AgencyDocketId))
		{
			try
			{
				Logger.Log("before income_type and bundle update");
				string income_type = DAL_SQL.GetRecord("BUNDLES", "income_type", result.AgencyDocketId, "docket_id");
				Logger.Log("get income_type");
				string vat_percent = DAL_SQL.GetRecord("BUNDLES", "vat_percent", result.AgencyDocketId, "docket_id");
				Logger.Log("get vat_percent");
				Logger.Log("amountBruto -" + amountBruto + ", amountNeto -" + amountNeto.ToString() + ", income_type -" + income_type.ToString()+ ", vat_percent -" + vat_percent.ToString());
				BundleRow br = new BundleRow(amountBruto, amountNeto, income_type, vat_percent);
				Logger.Log("create BundleRow");
				
				string updateBundlesQuery = "UPDATE BUNDLES SET " + br.getBUNDLESUpdateString() + " WHERE docket_id = " + result.AgencyDocketId;
				Logger.Log("update bundles netto etc. query = " + updateBundlesQuery);
				DAL_SQL.RunSql(updateBundlesQuery);
				Logger.Log("Succeded the BundleRow Update");
				
			}
			catch (Exception ex)
			{
				try
				{
					Logger.Log("Exception when updating BUNDLES amounts, subsid...");
					hotel_rooms_check_ws.cancelVoucher(result.AgencyDocketId);
				}
				catch (Exception exc)
				{
					Logger.Log("Failed to cancel docket_id = " + result.AgencyDocketId + ", Exception = " + exc.Message);
					return new HotelPriceOrderResult()
					{
						OrderMessage = "אירעה שגיאה בעת ביצוע ההזמנה, אנא פנה למנהל."
					};
				}

				Logger.Log("Failed to update BUNDLES after createDocket, docket_id = " + result.AgencyDocketId + ", Exception = " + ex.Message);
				return new HotelPriceOrderResult()
				{
					OrderMessage = "אירעה שגיאה בעת ביצוע ההזמנה."
				};
			}
		}
		else
		{
			return new HotelPriceOrderResult()
			{
				OrderMessage = "אירעה שגיאה בעת ביצוע ההזמנה, אנא פנה למנהל."
			};
		}
		
		
        // commented by Igor on 2012.03.12 - not to update balance, balance will be updated at create voucher (in agency)
        // write to action log and update traveller balance
        //string record_type = "first_room";
        //foreach(GovTravellerMakat makat in traveller.SelectedMakat)
        //{
        //    //OrderNights = (trav)
        //    // in case makat 40 and 5+5 option need to reduce from balance 11 nights
        //    OrderNights = (traveller.Ussage5and5) ? 11 : OrderNights;
        //    DAL_SQL_Helper.UpdateTravellerBalance(traveller.DocketId, traveller.TravellerId, makat.StartDate, OrderNights, areaId, makat.ItemSKU);
        //    // commented on 2012.02.23 - no need to write to acc_log due to actions export will be done at docket
        //    //DAL_SQL_Helper.WRiteActionLog(traveller.DocketId, traveller.TravellerId, result.AgencyVoucherId, makat.ItemSKU, fromDate, toDate, 0, amountBruto, traveller.Melave.Count, 0, hotelId, ((traveller.MelavePays)?1:0), int.Parse(makat.Level), 0, areaId, result.AgencyDocketId, 0, record_type);
        //    //if (traveller.Melave.Count > 1)
        //    //    record_type = "second_room";
        //}
        //return new HotelPriceOrderResult()
        //{
        //    OrderMessage = "from " + fromDate + " to " + toDate + " anon " + amountNeto + " anonb " + amountBruto + ""

        //return new HotelPriceOrderResult() { OrderMessage = "This  user." + hotelId + "*" + priceId + "*" + allocationId + "*" + fromDate + "*" + toDate + "*" + amountNeto + "*" + amountBruto + "*" + traveller.FirstName + "*" + areaId };

        //};
        return result;
    }

    public static string createDocketOnly(GovTraveller traveller)
    {
        string createDocketXml = string.Empty;
        XmlDocument doc = new XmlDocument();
        string usedMakat = ((traveller.SelectedMakat != null && traveller.SelectedMakat.Count > 0) ? traveller.SelectedMakat[0].ItemSKU : "0");

        createDocketXml = getAgencyCreateDocketOnlyXML(traveller, usedMakat);
        string createDocketXmlUrl = getAgencyCreateDocketUrl() + "&Query=" + createDocketXml;

        Logger.Log("Log create docket request" + createDocketXmlUrl);
        //Logger.Log(createDocketXmlUrl, "Log create docket request", @"Logs\log_" + DateTime.Now.ToShortDateString() + ".txt");
        doc = getAgencyRequestXmlDoc(createDocketXmlUrl);
        Logger.Log("Log create docket response" + doc.InnerXml);

        //Logger.Log(doc.InnerXml, "Log create docket response", @"Logs\log_" + DateTime.Now.ToShortDateString() + ".txt");

        // all statuses are ok
        return doc.SelectSingleNode("/ROOT/DOCKET/DOCKET_ID").InnerText;
    }
	
	public static HotelPriceOrderResult createDocketOnlyWithHotelPriceOrderResult(GovTraveller traveller)
    {
        string createDocketXml = string.Empty;
        XmlDocument doc = new XmlDocument();
        string usedMakat = ((traveller.SelectedMakat != null && traveller.SelectedMakat.Count > 0) ? traveller.SelectedMakat[0].ItemSKU : "0");
		HotelPriceOrderResult result = new HotelPriceOrderResult();
        createDocketXml = getAgencyCreateDocketOnlyXML(traveller, usedMakat);
        string createDocketXmlUrl = getAgencyCreateDocketUrl() + "&Query=" + createDocketXml;

        Logger.Log("Log create docket request" + createDocketXmlUrl);
        //Logger.Log(createDocketXmlUrl, "Log create docket request", @"Logs\log_" + DateTime.Now.ToShortDateString() + ".txt");
        doc = getAgencyRequestXmlDoc(createDocketXmlUrl);
        Logger.Log("Log create docket response" + doc.InnerXml);

        //Logger.Log(doc.InnerXml, "Log create docket response", @"Logs\log_" + DateTime.Now.ToShortDateString() + ".txt");
		if (doc == null)
        {
			Logger.Log("doc == null | OrderMessage = לא ניתן לפתוח תיק. - create docket only | traveller=" + traveller.TravellerId + " | ");
            return new HotelPriceOrderResult()
            {
                OrderMessage = "לא ניתן לפתוח תיק."
            };
        }
		try{
			// check if all nodes are ok
			XmlNodeList statuses = doc.SelectNodes("//STATUS");
			foreach (XmlNode status in statuses)
			{
				if (status.InnerText.ToLower() != "ok")
				{
					Logger.Log("status.InnerText.ToLower() != ok | לא ניתן ליצור תיק, אנא פנה למנהל מערכת. - create docket only | traveller=" + traveller.TravellerId + " | ");
					return new HotelPriceOrderResult()
					{
						OrderMessage = "לא ניתן ליצור תיק, אנא פנה למנהל מערכת."
					};
				}
			}
		}
		catch(Exception exFactor)
		{
			Logger.Log("statuses - " + exFactor.Message);			
		}
		
        // all statuses are ok
        result.AgencyDocketId = doc.SelectSingleNode("/ROOT/DOCKET/DOCKET_ID").InnerText;
        result.AgencyVoucherId = string.Empty;
        result.OrderCompleted = true;
        result.OrderMessage = "";
		
        return result;
    }

    #endregion



    #region PRIVATE methods

    private static bool CheckRoomsAllocation(int priceId, int allocationId, DateTime fromDate, DateTime toDate, GovTraveller traveller)
    {
        StringBuilder str_data = new StringBuilder();
        int rooms = traveller.RoomsAmount;
        string url = AgencyXmlServicesPricesBaseUrl + updateAllocationAction + getAgencyLoginUrlPart();
        url += "&Price=true&PriceID=" + priceId.ToString() + "&RoomsAmount=" + rooms.ToString();
        url += "&SupplierPriceID=" + allocationId.ToString() + "&FromDate=" + fromDate.ToString("dd-MMM-yyyy");
        url += "&ToDate=" + toDate.ToString("dd-MMM-yyyy");
        url += "&MakeOrder=0";
        //Logger.Log("AAAAAAAA   -   " + url);

        XmlDocument doc = getAgencyRequestXmlDoc(url);
		//Logger.Log("AAAAAAAA   -   " + doc.InnerXml);
        //error at allocations
        if (doc == null)
		{	
			Logger.Log("here1");
			return false;
		}

        XmlNodeList list = doc.SelectNodes("/ROOT//*");
        foreach (XmlNode node in list)
        {
			//Logger.Log("@PE node.Name.ToLower()" + node.Name.ToLower() );
            // no all rooms are available
            if (node.Name.ToLower() != "ok") 
			{
				Logger.Log("here2 - " + url);
				Logger.Log("here2 - " + doc.OuterXml);
				Logger.Log("here2 + node.Name.ToLower() = " + node.Name.ToLower());
				return false; 
			}
        }
        return true;
    }

    private static bool UpdateRoomsAllocation(int priceId, int allocationId, DateTime fromDate, DateTime toDate, GovTraveller traveller)
    {
        StringBuilder str_data = new StringBuilder();
        int rooms = traveller.RoomsAmount;
        string url = AgencyXmlServicesPricesBaseUrl + updateAllocationAction + getAgencyLoginUrlPart();
		
		Logger.Log("UpdateRoomsAllocation - allocationId = " + allocationId.ToString() + ", rooms = " + rooms.ToString());
        url += "&Price=true&PriceID=" + priceId.ToString() + "&RoomsAmount=" + rooms.ToString();
        url += "&SupplierPriceID=" + allocationId.ToString() + "&FromDate=" + fromDate.ToString("dd-MMM-yyyy");
        url += "&ToDate=" + toDate.ToString("dd-MMM-yyyy");
        url += "&MakeOrder=1";
		//Logger.Log("update Allocation: "+url);
        XmlDocument doc = getAgencyRequestXmlDoc(url);

        // error at allocations
        if (doc == null) 
		{
		//	Logger.Log("// error at allocations if (doc == null) ");
			return false;
		}

        XmlNodeList list = doc.SelectNodes("/ROOT//*");
        foreach (XmlNode node in list)
        {
            // no all rooms are available
            if (node.Name.ToLower() != "ok")
			{
				//TEST - *)
				Logger.Log("node.Name.ToLower() = " + node.ToString());
				return false;
			}
        }
        return true;
    }

    private static string getAgencyCreateDocketUrl()
    {
        string retUrl = string.Empty;
        retUrl = AgencyXmlServicesCreateDocketUrl + createDocketAction + getAgencyLoginUrlPart();
        return retUrl;
    }


    #region CREATE DOCKET XML

    private static string getAgencyCreateDocketOnlyXML(GovTraveller traveller, string usedMakat)
    {
        StringBuilder request = new StringBuilder();
        AgencyUser user = new AgencyUser();
        // docket
        request.Append("<ROOT>");
        request.Append("<DOCKET>");
        request.Append("<DOCKET_TYPE>2</DOCKET_TYPE>"); // docket will be created bussiness (always)
        request.Append("<DOCKET_SYSTEM_TYPE>" + user.AgencySystemType + "</DOCKET_SYSTEM_TYPE>");
        request.Append("<BCLIENT_ID>" + AgencyBclientId + "</BCLIENT_ID>");
        if (traveller.InternrtDocket)//internet docket
        {
            request.Append("<DOCKET_REMARK>created by Gov internet</DOCKET_REMARK>");
            request.Append("<DOCKET_SOURCE_TYPE>3</DOCKET_SOURCE_TYPE>");
        }
        else
        {
            request.Append("<DOCKET_REMARK>created by Gov manual</DOCKET_REMARK>");
            request.Append("<DOCKET_SOURCE_TYPE>2</DOCKET_SOURCE_TYPE>");
        }
        request.Append("</DOCKET>");
        // travellers
        request.Append(getTravellersXml(traveller, usedMakat));
        request.Append("</ROOT>");

        return request.ToString();
    }


    private static string getAgencyCreateDocketXML(int hotelId, int priceId, int allocationId, DateTime fromDate, DateTime toDate, double amountNeto, double amountBruto, GovTraveller traveller, string usedMakat)
    {
        StringBuilder request = new StringBuilder();
        AgencyUser user = new AgencyUser();
        // docket
        request.Append("<ROOT>");
        request.Append("<DOCKET>");
        // check if existing docket
        if (traveller.AgencyDocketId != null && traveller.AgencyDocketId.Length > 0)
        {
            request.Append("<DOCKET_ID>" + traveller.AgencyDocketId + "</DOCKET_ID>");
        }
        else // new docket
        {
            request.Append("<DOCKET_TYPE>2</DOCKET_TYPE>"); // docket will be created bussiness (always)
            request.Append("<DOCKET_SYSTEM_TYPE>" + user.AgencySystemType + "</DOCKET_SYSTEM_TYPE>"); // chen:doesnt work!
            request.Append("<BCLIENT_ID>" + AgencyBclientId + "</BCLIENT_ID>");
        }
        request.Append("<DOCKET_REMARK>created by Gov</DOCKET_REMARK>");
        request.Append("<DOCKET_SOURCE_TYPE>2</DOCKET_SOURCE_TYPE>");
        request.Append("</DOCKET>");
        // travellers
		request.Append(getTravellersXml(traveller, usedMakat));
        // service
        request.Append("<SERVICES><HOTELS><HOTEL>");
        request.Append("<SUPPLIER_ID>" + hotelId.ToString() + "</SUPPLIER_ID>");
        //Chen 26.9
	
        request.Append("<BUNDLE_NUMBER></BUNDLE_NUMBER>");
        request.Append("<STATUS>OK</STATUS>");
        request.Append("<FROM_DATE>" + fromDate.ToString("dd-MMM-yy") + "</FROM_DATE>");
        request.Append("<TO_DATE>" + toDate.ToString("dd-MMM-yy") + "</TO_DATE>");
        request.Append("<ROOMS>" + traveller.RoomsAmount + "</ROOMS>");
        request.Append("<PAID_TO_SUPPLIER_ID>" + hotelId.ToString() + "</PAID_TO_SUPPLIER_ID>");
        request.Append("<PRICE_AGENCY_ID>" + user.AgencyId + "</PRICE_AGENCY_ID>");
        request.Append("<HOTEL_PRICE_ID>" + priceId.ToString() + "</HOTEL_PRICE_ID>");
        request.Append("<SUPPLIER_PRICE_ID>" + allocationId.ToString() + "</SUPPLIER_PRICE_ID>");
        request.Append("<GOV_ERKEV_TYPE>" + traveller.ErkevType + "</GOV_ERKEV_TYPE>");
        request.Append("<GOV_FOUR_ONE_SEVEN>" + traveller.FourOneSeven + "</GOV_FOUR_ONE_SEVEN>");
        request.Append("<MELAVE_SELECTED_NIGHTS>" + traveller.StrMelaveSelectedNights + "</MELAVE_SELECTED_NIGHTS>");
        request.Append("<CREATE_VOUCHER>true</CREATE_VOUCHER>");
        // can damage xml
        //request.Append("<SUPPLIER_REMARK>" + traveller.SelectedMakat[0].OfficeRemarkForOrder + "</SUPPLIER_REMARK>");
			
        //request.Append("<TRAVELLER_REMARK>" +  + "</TRAVELLER_REMARK>");
        request.Append("</HOTEL>");
        // service travellers
		
        request.Append("<CUSTOMERS>");
        request.Append(getServiceCustomers(traveller));
        request.Append("</CUSTOMERS>");
        request.Append("</HOTELS></SERVICES>");
        request.Append("</ROOT>");

        return request.ToString();
    }
	
	public static string getAgencyCreateDocketXML2(int hotelId, int priceId, int allocationId, DateTime fromDate, DateTime toDate, double amountNeto, double amountBruto, GovTraveller traveller, string usedMakat)
    {
		string priceIdStr = (priceId.ToString() == "-1") ? "0" : priceId.ToString();
        StringBuilder request = new StringBuilder();
        AgencyUser user = new AgencyUser();
        // docket
        request.Append("<ROOT>");
        request.Append("<DOCKET>");
        // check if existing docket
        if (traveller.AgencyDocketId != null && traveller.AgencyDocketId.Length > 0)
        {
            request.Append("<DOCKET_ID>" + traveller.AgencyDocketId + "</DOCKET_ID>");
        }
        else // new docket
        {
            request.Append("<DOCKET_TYPE>2</DOCKET_TYPE>"); // docket will be created bussiness (always)
            request.Append("<DOCKET_SYSTEM_TYPE>" + user.AgencySystemType + "</DOCKET_SYSTEM_TYPE>"); // chen:doesnt work!
            request.Append("<BCLIENT_ID>" + AgencyBclientId + "</BCLIENT_ID>");
        }
        request.Append("<DOCKET_REMARK>created by Gov</DOCKET_REMARK>");
        request.Append("<DOCKET_SOURCE_TYPE>2</DOCKET_SOURCE_TYPE>");
        request.Append("</DOCKET>");
        // travellers
		//Chen removed to not attach a traveller to an existing docket.
        //request.Append(getTravellersXml(traveller, usedMakat));
		
        // service
        request.Append("<SERVICES><HOTELS><HOTEL>");
        request.Append("<SUPPLIER_ID>" + hotelId.ToString() + "</SUPPLIER_ID>");
        //Chen 26.9
        request.Append("<BUNDLE_NUMBER></BUNDLE_NUMBER>");
        request.Append("<STATUS>OK</STATUS>");
        request.Append("<FROM_DATE>" + fromDate.ToString("dd-MMM-yy") + "</FROM_DATE>");
        request.Append("<TO_DATE>" + toDate.ToString("dd-MMM-yy") + "</TO_DATE>");
        request.Append("<ROOMS>" + traveller.RoomsAmount + "</ROOMS>");
        request.Append("<PAID_TO_SUPPLIER_ID>" + hotelId.ToString() + "</PAID_TO_SUPPLIER_ID>");
        request.Append("<PRICE_AGENCY_ID>" + user.AgencyId + "</PRICE_AGENCY_ID>");
        request.Append("<HOTEL_PRICE_ID>" + priceIdStr + "</HOTEL_PRICE_ID>");
        request.Append("<SUPPLIER_PRICE_ID>" + allocationId.ToString() + "</SUPPLIER_PRICE_ID>");
        request.Append("<GOV_ERKEV_TYPE>" + traveller.ErkevType + "</GOV_ERKEV_TYPE>");
        request.Append("<GOV_FOUR_ONE_SEVEN>" + traveller.FourOneSeven + "</GOV_FOUR_ONE_SEVEN>");
        request.Append("<MELAVE_SELECTED_NIGHTS>" + traveller.StrMelaveSelectedNights + "</MELAVE_SELECTED_NIGHTS>");
        request.Append("<CREATE_VOUCHER>true</CREATE_VOUCHER>");
        // can damage xml
        //request.Append("<SUPPLIER_REMARK>" + traveller.SelectedMakat[0].OfficeRemarkForOrder + "</SUPPLIER_REMARK>");
        //request.Append("<TRAVELLER_REMARK>" +  + "</TRAVELLER_REMARK>");
        request.Append("</HOTEL>");
        // service travellers		
        request.Append("<CUSTOMERS>");
        request.Append(getServiceCustomers(traveller));
        request.Append("</CUSTOMERS>");
        request.Append("</HOTELS></SERVICES>");
        request.Append("</ROOT>");

        return request.ToString();
    }

    private static string getTravellersXml(GovTraveller traveller, string usedMakat)
    {
        StringBuilder str = new StringBuilder();

        str.Append("<TRAVELLERS>");
        str.Append("<TRAVELLER>");
        str.Append("<TITLE>Mr</TITLE>");
        str.Append("<LAST_NAME>" + Utils.ConvertStringToAgencyUtf(traveller.SecondName) + "</LAST_NAME>");
        str.Append("<FIRST_NAME>" + Utils.ConvertStringToAgencyUtf(traveller.FirstName) + "</FIRST_NAME>");
        str.Append("<ID_NUM>" + traveller.TravellerId + "</ID_NUM>");
        str.Append("<PASSPORT></PASSPORT>");
        str.Append("<PASSPORT_VALID></PASSPORT_VALID>");
        str.Append("<CCARD_NUMBER></CCARD_NUMBER>");
        str.Append("<CCARD_VALID></CCARD_VALID>");
        str.Append("<CCARD_CVV></CCARD_CVV>");
        str.Append("<DOB></DOB>");
        str.Append("<PHONES>" + traveller.GetContacts() + "</PHONES>");
        str.Append("<EMAIL></EMAIL>");
        str.Append("<ADR_STREET>" + Utils.ConvertStringToAgencyUtf(traveller.Address) + "</ADR_STREET>");
        str.Append("<ADR_HOUSE></ADR_HOUSE>");
        str.Append("<ADR_FLAT></ADR_FLAT>");
        str.Append("<ADR_CITY>" + Utils.ConvertStringToAgencyUtf(traveller.City) + "</ADR_CITY>");
        str.Append("<ADR_ZIP>" + traveller.ZipCode + "</ADR_ZIP>");
        str.Append("<ADR_POB></ADR_POB>");
        str.Append("<GOV_MAKAT>" + usedMakat + "</GOV_MAKAT>");
        str.Append("<GOV_DOCKET_ID>" + traveller.DocketId + "</GOV_DOCKET_ID>");
        str.Append("<GOV_LEVEL>" + traveller.SelectedMakat[0].Level + "</GOV_LEVEL>");
        str.Append("<REMARK>Gov. makat:" + usedMakat + " docketId:" + traveller.DocketId + "</REMARK>");
        str.Append("<GOV_MAKAT_START_DATE>" + traveller.SelectedMakat[0].StartDate.ToString("dd-MMM-yy") + "</GOV_MAKAT_START_DATE>");
        str.Append("<GOV_BALANCE_USSAGE>" + ((traveller.BalanceUssage) ? "true" : "false") + "</GOV_BALANCE_USSAGE>");
        str.Append("<GOV_FIVE_NIGHT_PAY>" + ((traveller.IsAdded5thNight) ? "true" : "false") + "</GOV_FIVE_NIGHT_PAY>");
        str.Append("<GOV_ESCORT_NUMBERS>" + traveller.SelectedMakat[0].EscortNum + "</GOV_ESCORT_NUMBERS>");
        str.Append("</TRAVELLER>");

        foreach (GovTravellerMelave melave in traveller.Melave)
        {
            str.Append("<TRAVELLER>");
            str.Append("<TITLE>Mrs</TITLE>");
            str.Append("<LAST_NAME>" + Utils.ConvertStringToAgencyUtf(melave.LastName) + "</LAST_NAME>");
            str.Append("<FIRST_NAME>" + Utils.ConvertStringToAgencyUtf(melave.FirstName) + "</FIRST_NAME>");
            str.Append("<GOV_LEVEL>0</GOV_LEVEL>");
            str.Append("<ID_NUM></ID_NUM>");
            str.Append("<PASSPORT></PASSPORT>");
            str.Append("<PASSPORT_VALID></PASSPORT_VALID>");
            str.Append("<CCARD_NUMBER></CCARD_NUMBER>");
            str.Append("<CCARD_VALID></CCARD_VALID>");
            str.Append("<CCARD_CVV></CCARD_CVV>");
            str.Append("<DOB></DOB>");
            str.Append("<PHONES></PHONES>");
            str.Append("<EMAIL></EMAIL>");
            str.Append("<ADR_STREET></ADR_STREET>");
            str.Append("<ADR_HOUSE></ADR_HOUSE>");
            str.Append("<ADR_FLAT></ADR_FLAT>");
            str.Append("<ADR_CITY></ADR_CITY>");
            str.Append("<ADR_ZIP></ADR_ZIP>");
            str.Append("<ADR_POB></ADR_POB>");
            //str.Append("<REMARK></REMARK>");
            str.Append("</TRAVELLER>");
        }

        str.Append("</TRAVELLERS>");
        return str.ToString();
    }
	
    private static string getServiceCustomers(GovTraveller traveller)
    {
        int rooms = traveller.RoomsAmount;
        StringBuilder str = new StringBuilder();

		// if StrMelaveSelectedNights contains 0 then the its makat 027240. and the escort selected days to come (not all days)
		// ZakaiBeTashlumAndMelaveLeLoTashlum                
		// !string.IsNullOrEmpty(traveller.StrMelaveSelectedNights) && traveller.StrMelaveSelectedNights.Contains("0"))
		if (traveller.ErkevType.Contains("ZakaiBeTashlumAndMelaveLeLoTashlum"))
		{
			traveller.RoomId = 2330;
		}
		
        str.Append("<CUSTOMER>");
        str.Append("<AMOUNT>" + traveller.TotalAmountBruto.ToString() + "</AMOUNT>");
        str.Append("<SUBSID>" + traveller.SibsudAmount.ToString() + "</SUBSID>");
        str.Append("<TRAV_PAY>" + traveller.TravellerPayAmount + "</TRAV_PAY>");
        str.Append("<CURRENCY>1</CURRENCY>");
        str.Append("<ROOM_TYPE_ID>" + traveller.RoomId.ToString() + "</ROOM_TYPE_ID>");
        str.Append("<ROOM_TYPE_ITM>1</ROOM_TYPE_ITM>");
        str.Append("<ON_BASE_ID>" + traveller.BaseId.ToString() + "</ON_BASE_ID>");
        str.Append("<ON_BASE_ITM>1</ON_BASE_ITM>");
        str.Append("<BABIES>0</BABIES>");
        str.Append("</CUSTOMER>");

        foreach (GovTravellerMelave melave in traveller.Melave)
        {
            str.Append("<CUSTOMER>");
            str.Append("<AMOUNT>" + melave.TotalAmount.ToString() + "</AMOUNT>");
            str.Append("<SUBSID>" + melave.SibsudAmount.ToString() + "</SUBSID>");
            str.Append("<TRAV_PAY>" + melave.TravPayAmount.ToString() + "</TRAV_PAY>");
            str.Append("<CURRENCY>1</CURRENCY>");

            if (melave.RoomId > 0)
            {
                str.Append("<ROOM_TYPE_ID>" + melave.RoomId.ToString() + "</ROOM_TYPE_ID>");
                str.Append("<ROOM_TYPE_ITM>1</ROOM_TYPE_ITM>");
            }
            else
            {
                str.Append("<ROOM_TYPE_ID></ROOM_TYPE_ID>");
                str.Append("<ROOM_TYPE_ITM>0</ROOM_TYPE_ITM>");
            }
            str.Append("<ON_BASE_ID>" + traveller.BaseId.ToString() + "</ON_BASE_ID>");
            str.Append("<ON_BASE_ITM>1</ON_BASE_ITM>");
            str.Append("<BABIES>0</BABIES>");
            str.Append("</CUSTOMER>");
        }
        return str.ToString();
    }
    #endregion CREATE DOCKET XML


    private static string getAgencyBasesUrl(int hotelId, int allocationId)
    {
        string retUrl = string.Empty;
		
        retUrl = AgencyXmlServicesBaseUrl + baseTypesAction + getAgencyLoginUrlPart() + ((allocationId > 0) ? "&AllocationId=" + allocationId.ToString() : "");
		
        return retUrl;
    }

    //?PriceID=29880&PriceName=&HotelName=&AreaName=&OnBaseStr=0|2|0|0,&RoomTypeStr=2|1,&FromDate=01-Mar-11&ToDate=02-Mar-11&Type=0
    private static string getAgencyHotelPricesUrl(int hotelId, int allocationId, int baseId, DateTime fromDate, DateTime toDate, GovTraveller traveller)
    {
        string retUrl = string.Empty;

        traveller.BaseId = baseId;
        // due to no room id - will use hard coded
        traveller.SaveToSession();

        retUrl = AgencyXmlServicesPricesBaseUrl + hotelPricesAction + getAgencyLoginUrlPart();
        retUrl += "&HotelName=" + hotelId.ToString() + "&FromDate=" + fromDate.ToString("dd-MMM-yy") + "&ToDate=" + toDate.ToString("dd-MMM-yy");
        retUrl += "&OnBaseStr=" + traveller.getBaseStr() + "&RoomTypeStr=" + traveller.getRoomStr() + "&Type=1";
        return retUrl;
    }

    // ?AgencyID=7&SystemType=3&UserName=Agency2000&Password=123456&language=1
    private static string getAgencyLoginUrlPart()
    {
        string str_url = string.Empty;
        AgencyUser user = new AgencyUser();
        string agencyId = "85";
        string systemType = "3";
        string username = "agency2000";
        string password = "11071964";
        string clerkId = "1";

        //str_url = "?AgencyID=" + user.AgencyId + "&SystemType=" + user.AgencySystemType + "&UserName=" + user.AgencyUserName + "&Password=" + user.AgencyUserPassword + "&ClerkID=" + user.AgencyUserId + "&language=1"; // &WSClientIp
        str_url = "?AgencyID=" + agencyId + "&SystemType=" + systemType + "&UserName=" + username + "&Password=" + password + "&ClerkID=" + clerkId + "&language=1"; // &WSClientIp

        //const string AgencyId = "39";
        //const string AgencySystemType = "3";
        //const string AgencyUserName = "avyossi";
        //const string AgencyUserPassword = "av123456";
        //const string AgencyUserId = "166";

        //const string AgencyId = "85";
        //const string AgencySystemType = "3";
		
		//string AgencyUserId = ClerkNameSingelton.getId();

       //DataSet ds = DAL_SQL_Helper.GetClerkDetails(int.Parse(AgencyUserId));

       //string AgencyUserName = ds.Tables[0].Rows[0].ItemArray[0].ToString();
       //string AgencyUserPassword = ds.Tables[0].Rows[0].ItemArray[1].ToString();

	   //Logger.Log("x : " + AgencyUserId + ",y : " + AgencyUserName + ",z : " + AgencyUserPassword);
	   
        //const string AgencyUserName = user.;
        //const string AgencyUserPassword = user;
       // const string AgencyUserId = "1";

        //str_url = "?AgencyID=" + AgencyId + "&SystemType=" + AgencySystemType + "&UserName=" + AgencyUserName + "&Password=" + AgencyUserPassword + "&ClerkID=" + AgencyUserId + "&language=1"; // &WSClientIp
        return str_url;
    }

    public static XmlDocument getAgencyRequestXmlDoc(string url)
    {
        XmlDocument doc = new XmlDocument();
        StringBuilder oBuilder = new StringBuilder();
        StringWriter oStringWriter = new StringWriter(oBuilder);

        XmlTextReader oXmlReader = new XmlTextReader(url);
        XmlTextWriter oXmlWriter = new XmlTextWriter(oStringWriter);

        try
        {
            //Logger.Log(url);
            //Logger.Log(url, "Log", @"Logs\log_" + DateTime.Now.ToShortDateString() + ".txt");
            while (oXmlReader.Read())
            {
                oXmlWriter.WriteNode(oXmlReader, true);
            }
            oXmlReader.Close();
            oXmlWriter.Close();
            doc.LoadXml(oBuilder.ToString());

            //Logger.Log(oBuilder.ToString());
            //Logger.Log(oBuilder.ToString(), "Log", @"Logs\log_" + DateTime.Now.ToShortDateString() + ".txt");
        }
        catch (Exception ex)
        {
            Logger.Log(ex.Message + ", url = " + url);
            //Logger.Log(ex.Message, "Log create docket response error", @"Logs\log_" + DateTime.Now.ToShortDateString() + ".txt");
            return null;
        }
        return doc;
    }

    #endregion PRIVATE methods
}
