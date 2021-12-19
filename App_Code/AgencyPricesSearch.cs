using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Xml;

/// <summary>
/// Summary description for AgencyPricesSearch
/// </summary>
[WebService(Namespace = "http://tempuri.org/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
// To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
 [System.Web.Script.Services.ScriptService]
public class AgencyPricesSearch : System.Web.Services.WebService {

    public AgencyPricesSearch () {

        //Uncomment the following line if using designed components 
        //InitializeComponent(); 
    }    

    [WebMethod(EnableSession = true)]
    public string GetPriceByPriceIdAndRoomTypesAndDates(string iAgencyId, string iSystemType, string iPriceId, string iRoomType, string iFromDate, string iToDate, string iMelaveDates, string iSupplierId)
    {
        DateTime fromDate, toDate;
        int nights = -1, fromDay = -1, toDay = -1;
        string getHotelPricesSql = string.Empty;
        string getPriceDetailsSql = string.Empty;
        bool isSuccess = true;
        string supplierId = iSupplierId;
        double vat = 0;
        string retVal = string.Empty;// amountBrutto|amountNetto
		
        try
        {
            fromDate = DateTime.Parse(iFromDate);
            toDate = DateTime.Parse(iToDate);
            nights = (toDate - fromDate).Days;
            fromDay = getDayFromDate(fromDate);
            toDay = getDayFromDate(toDate);
            isSuccess = !string.IsNullOrEmpty(iPriceId);
			
			if (string.IsNullOrEmpty(iMelaveDates))
			{
				iMelaveDates = "";
				for (int i = 0; i < nights; i++)
					iMelaveDates += "1";
			}
        }
        catch (Exception ex)
        {
            Logger.Log("Failed to parse data. Exception = " + ex.Message);
            retVal = "error";
            isSuccess = false;
        }

        if (isSuccess)
        {
            AgencyUser user = new AgencyUser();

            user.AgencyId = iAgencyId;
            user.AgencySystemType = iSystemType;

            DAL_SQL.ConnStr = ConfigurationManager.AppSettings["CurrentAgencyDBConnStr"];
            DAL_SQL.ConnStr = DAL_SQL.ConnStr.Replace("^agencyId^", ((user.AgencyId.Length == 1) ? "000" + user.AgencyId : "00" + user.AgencyId));
            DAL_SQL.ConnStr = DAL_SQL.ConnStr.Replace("^systemType^", ((user.AgencySystemType == "3") ? "INN" : "OUT"));

			if (iPriceId != "-1")
			{
				getPriceDetailsSql = @"SELECT * 
                                   FROM PR_HOTEL_PRICES_DETAILS
                                   WHERE price_id = " + iPriceId;
			}
			else
			{
				
			}

            PriceDetails priceDetails = new PriceDetails(iPriceId, supplierId, iFromDate, iToDate, fromDay, toDay, nights, vat);
            setPriceBySqlAndRoomType(getPriceDetailsSql, iRoomType, priceDetails, iMelaveDates);
            
			retVal = ("Brutto:" + priceDetails.AmountBrutto.ToString() + "|Netto:" + priceDetails.AmountNetto.ToString() + "|BruttoCouple:" + priceDetails.AmountBruttoCouple.ToString());
        }

        //retVal = amountBrutto|amountNetto
        return retVal;
    }

    private PriceDetails setPriceBySqlAndRoomType(string getPriceDetailsSql, string iRoomType, PriceDetails iPriceDetails, string iMelaveDates)
    {
        string price = string.Empty;
        DataSet requstedPriceDetails = null;

        requstedPriceDetails = DAL_SQL.RunSqlDataSet(getPriceDetailsSql);
        if (isDataSetRowsNotEmpty(requstedPriceDetails) || iPriceDetails.PriceId == "-1")
        {
            setPriceFromDataSet(requstedPriceDetails, iRoomType, iPriceDetails, iMelaveDates);
        }
        else
        {
            throw new Exception("Not found details for price. priceId = " + iPriceDetails.PriceId);
        }

        return iPriceDetails;
    }

    private void setPriceFromDataSet(DataSet iRequstedPriceDetails, string iRoomType, PriceDetails iPriceDetails, string iMelaveDates)
    {
        int fromDay = iPriceDetails.FromDay;
        int toDay = iPriceDetails.ToDay;
        int nights = iPriceDetails.Nights;
        decimal bruttoCouple = 0;
        decimal brutto = 0;
        decimal netto = 0;

        int countNights = 0;

		if (iPriceDetails.PriceId != "-1")
		{
			foreach (DataRow priceRow in iRequstedPriceDetails.Tables[0].Rows)
			{
				if (priceRow["hotel_room_type_id"].ToString() == iRoomType)
				{
					while (fromDay != toDay || nights != countNights)
					{
						if (iMelaveDates[countNights] == '1')
						{
							brutto = decimal.Parse(priceRow["night_bruto_" + fromDay].ToString());
							netto = decimal.Parse(priceRow["night_netto_" + fromDay].ToString());
							iPriceDetails.AmountBrutto += brutto;
							iPriceDetails.AmountNetto += netto;
						}
						else
						{
							brutto = decimal.Parse(priceRow["night_bruto_" + fromDay].ToString());
							netto = decimal.Parse(priceRow["night_netto_" + fromDay].ToString());
							iPriceDetails.AmountBrutto += (brutto / 1.6M * 1.3M);
							iPriceDetails.AmountNetto += (netto / 1.6M * 1.3M);
						}
						
						fromDay++;
						if (fromDay == 8)// (8) means its sunday.
							fromDay = 1;
						countNights++;
					}

					break;
				}
			}
		}
		else
		{
			string langStr = "1255";
			string priceType = "1";
			string clerkId = "1";
			string agencyId = "85";
			string systemType = "3";
			string baseId = DAL_SQL.RunSql("SELECT base_price_base_id FROM P_HOTEL_PRICES WHERE supplier_id = " + iPriceDetails.SupplierId);
			string areaId = "0";
			string supplierId = iPriceDetails.SupplierId;
			string compositionId = iRoomType;
			AgencyPricesSearchWs.AgencyPricesSearch priceWs = new AgencyPricesSearchWs.AgencyPricesSearch();
			DateTime fromDate = DateTime.Parse(iPriceDetails.FromDate);
			DateTime toDate = DateTime.Parse(iPriceDetails.ToDate);
			DateTime currentDate = fromDate;
			string resultSingle,resultCouple, resultMelaveSubsid;
			
			compositionId = iRoomType;
			Logger.Log("1agencyId = " + agencyId + ", systemType = " + systemType + ", clerkId = " + clerkId + ", iAreaId = " + areaId + ", iSupplierId = " + supplierId + ", priceType = " + priceType + ", langStr = " + langStr + ", iFromDate = " + currentDate.ToString("dd-MMM-yy") + ", iToDate = " + currentDate.AddDays(1).ToString("dd-MMM-yy") + ", compositions = " + compositionId + ", baseId = " + baseId);
			resultMelaveSubsid = priceWs.getHotelPricePerAreaWithoutSaleCycle(agencyId, systemType, clerkId, 
												  areaId, supplierId, priceType, langStr, 
												  fromDate.ToString("dd-MMM-yy"), toDate.AddDays(0).ToString("dd-MMM-yy"), 
												  compositionId, baseId);
			compositionId = "1"; //if melave not selected then get price for single			
			resultSingle = priceWs.getHotelPricePerAreaWithoutSaleCycle(agencyId, systemType, clerkId, 
												  areaId, supplierId, priceType, langStr, 
												  fromDate.ToString("dd-MMM-yy"), toDate.AddDays(0).ToString("dd-MMM-yy"), 
												  compositionId, baseId);									  
			
			compositionId = "2"; //netto when have escort is caluculates by couple(100%)
			resultCouple = priceWs.getHotelPricePerAreaWithoutSaleCycle(agencyId, systemType, clerkId, 
												  areaId, supplierId, priceType, langStr, 
												  fromDate.ToString("dd-MMM-yy"), toDate.AddDays(0).ToString("dd-MMM-yy"), 
												  compositionId, baseId);									  
			
			
			XmlDocument docToGetPriceFrom, docToGetPriceNettoFrom, docToGetPriceFromCouple;
			XmlDocument finalPricexXmlSingle = new XmlDocument();
			XmlDocument finalPricexXmlCouple = new XmlDocument();
			XmlDocument finalPricexXmlMelaveSubsid = new XmlDocument();
			Logger.Log("1 resultMelaveSubsid- " + resultMelaveSubsid);
			Logger.Log("2 resultSingle - " + resultSingle);
			Logger.Log("3 resultCouple - " + resultCouple);
			finalPricexXmlMelaveSubsid.LoadXml(resultMelaveSubsid);
			finalPricexXmlSingle.LoadXml(resultSingle);
			finalPricexXmlCouple.LoadXml(resultCouple);
			
			if (finalPricexXmlMelaveSubsid.SelectNodes("Root//FinalPrices//FinalPrice").Count > 0) //has result
			{
				for (int i = 0; i < nights; i++)
				{
					//Composition / erkev
					if (iRoomType == "2330")
					{
						if (iMelaveDates[i] == '1')
						{
							docToGetPriceFrom = finalPricexXmlMelaveSubsid;
							docToGetPriceNettoFrom = finalPricexXmlCouple;
							docToGetPriceFromCouple = finalPricexXmlCouple;
						}
						else
						{
							docToGetPriceFrom = finalPricexXmlSingle;
							docToGetPriceNettoFrom = finalPricexXmlSingle;
							docToGetPriceFromCouple = finalPricexXmlSingle;
						}
					}
					else
					{
						docToGetPriceFrom = finalPricexXmlMelaveSubsid;
						docToGetPriceNettoFrom = finalPricexXmlMelaveSubsid;
						docToGetPriceFromCouple = finalPricexXmlMelaveSubsid;
					}
					
					XmlDocument tempBrutto = new XmlDocument();
					XmlDocument tempNetto = new XmlDocument();
					XmlDocument tempBruttoCouple = new XmlDocument();
					
					tempBrutto.LoadXml(docToGetPriceFrom.SelectNodes("//Root//FinalPrices//FinalPrice//FinalPricesPerDays").Item(0).SelectNodes("//FinalPricePerDay")[i].OuterXml);
					tempNetto.LoadXml(docToGetPriceNettoFrom.SelectNodes("//Root//FinalPrices//FinalPrice//FinalPricesPerDays").Item(0).SelectNodes("//FinalPricePerDay")[i].OuterXml);
					//Get couple price any way.
					tempBruttoCouple.LoadXml(docToGetPriceFromCouple.SelectNodes("//Root//FinalPrices//FinalPrice//FinalPricesPerDays").Item(0).SelectNodes("//FinalPricePerDay")[i].OuterXml);
					
					bruttoCouple = decimal.Parse(tempBruttoCouple.SelectSingleNode("//Price").InnerText);
					brutto = decimal.Parse(tempBrutto.SelectSingleNode("//Price").InnerText);
					netto = decimal.Parse(tempNetto.SelectSingleNode("//PriceNetto").InnerText);
					Logger.Log("bruttoCouple = " + bruttoCouple);
					Logger.Log("brutto = " + brutto);
					Logger.Log("netto = " + netto);
					iPriceDetails.AmountBruttoCouple += bruttoCouple;
					iPriceDetails.AmountBrutto += brutto;
					iPriceDetails.AmountNetto += netto;
				}
			}
			
			currentDate = currentDate.AddDays(1);
		}
    }   
	
    private string getPriceIdFordBundle(string iGetHotelPricesSql, string iSupplierId, string iFromDate, string iToDate)
    {
        DataSet pricesIdsForSelectedBundle = null;
        string priceId = string.Empty;

        try
        {
            //Gets all the prices that relevant to the specific order.
            pricesIdsForSelectedBundle = DAL_SQL.RunSqlDataSet(iGetHotelPricesSql);

            //Check that the dataSet is not empty.
            if (isDataSetRowsNotEmpty(pricesIdsForSelectedBundle))
            {
                if (pricesIdsForSelectedBundle.Tables[0].Rows.Count == 1)
                {
                    foreach (DataRow priceRow in pricesIdsForSelectedBundle.Tables[0].Rows)
                    {
                        //If got only 1 price match so save the priceId and days.
                        priceId = priceRow["id"].ToString();
                    }
                }
                else
                {
                    Logger.Log("More than 1 price");
                    string pricesIds = string.Empty;

                    foreach (DataRow priceRow in pricesIdsForSelectedBundle.Tables[0].Rows)
                    {
                        //If got only 1 price match so save the priceId and days.
                        pricesIds += (priceRow["id"].ToString() + ", ");
                    }

                    throw new Exception("More than 1 price, found " + pricesIdsForSelectedBundle.Tables[0].Rows.Count + " prices. priceId = " + pricesIds);
                }
            }
            else
            {
                string msg = "priceId not found.";

                Logger.Log(msg);
                throw new Exception(msg);
            }
        }
        catch (Exception ex)
        {
            string msg = "Failed to get priceId. Exception = " + ex.Message;

            Logger.Log(msg);
            throw new Exception(msg);
        }

        return priceId;
    }

	[WebMethod(EnableSession = true)]
	public void setErkevTypeZakaiAndMelaveBeTashlum(string iAgencyId, string iSystemType, string iBundleId)
	{
		string otherSys = string.Empty;
		string erkevType = "ZakaiAndMelaveBeTashlum";
		
		try
		{
			otherSys = DAL_SQL.GetRecord("AGENCIES", "other_sys", iAgencyId , "id");
			
			if (otherSys == "1")
			{
				DAL_SQL.RunSql("UPDATE BUNDLES SET erkev_type = '" + erkevType + "' WHERE id = " + iBundleId);
			}
		}
		catch (Exception ex)
		{
			Logger.Log("failed update erkev. Exception = " + ex.Message);
		}
	}

    //  'Tools'
    private string getHotelPricesSqlString(string iSupplierId, string iFromDate, string iToDate, int iNights)
    {
        string getHotelPricesSql = string.Empty;

        //Query to get the price that availabe for the specific dates on the requested hotel.
        getHotelPricesSql = string.Format(@"
                            SELECT id, price_name, from_date, to_date into #temp
                            FROM PR_HOTEL_PRICES 
                            WHERE {0} = hotel_supplier_id
                                AND cast('{1}' as smalldatetime) >= from_date 
                                AND cast('{2}' as smalldatetime) <= to_date 
                                AND isActive = 1  

                            SELECT id FROM #temp 
                            WHERE id in (SELECT hotel_price_id 
                                			FROM PR_HOTEL_PRICES_DATES
                                			WHERE hotel_price_id = #temp.id 
                                			AND from_date = cast('{1}' as smalldatetime)
                                			AND to_date = cast('{2}' as smalldatetime)
                                            AND status = 1

                                            OR

                                            {3} >= min_nights
                                            AND {3} <= max_nights 
                                            AND {3} >= min_nights_mid
                                            AND cast('{1}' as smalldatetime) >= from_date 
											AND cast('{2}' as smalldatetime) <= to_date 
                                            AND status = 1

                                            OR 

                                            {3} >= min_nights
                                            AND {3} <= max_nights 
                                            AND {3} >= min_nights_end
                                            AND cast('{1}' as smalldatetime) >= from_date 
											AND cast('{2}' as smalldatetime) <= to_date 
                                            AND status = 1
                                                
                                            OR    
        
                                            {3} >= min_nights
                                            AND 0 <= max_nights 
                                            AND cast('{1}' as smalldatetime) >= from_date 
											AND cast('{2}' as smalldatetime) <= to_date 
											AND status = 1
                                            ) 
                                
                            drop table #temp",
      iSupplierId, iFromDate, iToDate, iNights);

        return getHotelPricesSql;
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
	
	//protected void duplicateAttractionsPricesClick(object sender, EventArgs e)
    //{
    //    //duplicateAttractionsPrices();
	//	
    //}
	//
	//[WebMethod(EnableSession = true)]
	//public void duplicateAttractionsPrices()
    //{
    //    string queryGetAllHotelsFromPrices = "SELECT DISTINCT supplier_id FROM SUPPLIERS_TO_OTHER_ADDS_DATES";
    //    DataSet dsSuppIds = null, dsBruttoAndNetto = null;
    //    string suppId = string.Empty;
    //    decimal brutto = 0, netto = 0;
    //    string addId = string.Empty;
    //    DateTime startDate= DateTime.Parse("01-Mar-19");
    //    DateTime endDate = DateTime.Parse("01-Mar-20");
    //    DateTime currentDate;
    //    int daysToAdd = 0;
    //    string queryInsert = string.Empty;
    //    string queryGetBrtuuAndNettoBySuppId = string.Empty;
    //
    //    //dsSuppIds = DAL_SQL.RunSqlDataSet(queryGetAllHotelsFromPrices);
    //    //int[] suppIdsArr = new int[] {96, 97, 99, 102, 107, 108, 131, 146, 315, 335, 367, 526, 1712, 2050, 2099, 2487, 2741, 4859, 4860, 10164, 13748, 18977};
	//	//int[] suppIdsArr = new int[] {107, 146, 526, 1712};
	//	int[] suppIdsArr = new int[] { 146};
    //
    //    //Logger.EmptyLog("count1 = " + dsSuppIds.Tables[0].Rows.Count, eLogger.EXTRA_PAY);
    //    //foreach (DataRow row in dsSuppIds.Tables[0].Rows)
    //    for (int i = 0;i < suppIdsArr.Length;i++)
    //    {
    //        suppId = suppIdsArr[i].ToString();
    //        Logger.EmptyLog("suppId = " + suppId, eLogger.EXTRA_PAY);
    //        //suppId = Utils.getColumnValueByName(row, "supplier_id");
    //        queryGetBrtuuAndNettoBySuppId = "SELECT DISTINCT add_id, amount_bruto, amount_netto FROM SUPPLIERS_TO_OTHER_ADDS_DATES WHERE supplier_id = *supplierId* AND cast(price_date as smalldatetime) >= CAST('01-Mar-18' as smalldatetime)";
    //
    //        queryGetBrtuuAndNettoBySuppId = queryGetBrtuuAndNettoBySuppId.Replace("*supplierId*", suppId);
    //        dsBruttoAndNetto = DAL_SQL.RunSqlDataSet(queryGetBrtuuAndNettoBySuppId);
    //
    //        if (isDataSetRowsNotEmpty(dsBruttoAndNetto))
    //        {
    //            if (dsBruttoAndNetto.Tables[0].Rows.Count == 2)
    //            {
    //                foreach (DataRow rowBruttoNetto in dsBruttoAndNetto.Tables[0].Rows)
    //                {
	//					currentDate = startDate;
    //                    addId = Utils.getColumnValueByName(rowBruttoNetto, "add_id");
    //                    brutto = decimal.Parse(Utils.getColumnValueByName(rowBruttoNetto, "amount_bruto"));
    //                    netto = decimal.Parse(Utils.getColumnValueByName(rowBruttoNetto, "amount_netto"));
    //
	//					switch(suppId)
	//					{
	//						case "107":
	//							if (addId == "1")
	//							{
	//								netto = 228.15M;
	//							}
	//							else
	//							{
	//								netto = 296.5M;
	//							}
	//							break;
	//						case "146":
	//							if (addId == "1")
	//							{
	//								netto = 182.52M;
	//							}
	//							else
	//							{
	//								netto = 237.27M;
	//							}
	//							break;
	//						case "526":
	//						case "1712":
	//							if (addId == "1")
	//							{
	//								brutto = 216.45M;
	//								netto = 210.6M;
	//							}
	//							else
	//							{
	//								brutto = 281.38M;
	//								netto = 273.78M;
	//							}
	//							break;
	//					}
    //                    while (currentDate != endDate)
    //                    {
    //                        queryInsert = "INSERT INTO SUPPLIERS_TO_OTHER_ADDS_DATES VALUES(" + suppId + ", " + addId + ", N'" + currentDate.ToString("dd-MMM-yy") + "', " + netto.ToString() + ", " + brutto.ToString() + ")";
    //                        Logger.EmptyLog(queryInsert, eLogger.PRICE_NOT_FOUND);
	//						try
	//						{
	//							DAL_SQL.RunSql(queryInsert);
	//						}
    //                        catch(Exception exUpdate)
	//						{
	//							Logger.EmptyLog("catch = " + exUpdate.Message, eLogger.PRICE_NOT_FOUND);
	//						}
	//						currentDate = currentDate.AddDays(1);
    //                    }
    //                }
    //            }
    //            else
    //            {
    //                Logger.EmptyLog("There are " + dsBruttoAndNetto.Tables[0].Rows.Count + " brutto and netto. query = " + queryGetBrtuuAndNettoBySuppId, eLogger.PRICE_NOT_FOUND);
    //            }
    //        }
    //    }
    //}
}
