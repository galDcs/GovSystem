using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Script.Services;
using System.Web.Services;
using TreatmentEntitledService;
using System.Xml;

/// <summary>
/// Summary description for AjaxService
/// </summary>
[WebService(Namespace = "http://tempuri.org/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
// To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
[System.Web.Script.Services.ScriptService]
public class CancelOrderWebService : System.Web.Services.WebService
{
    private string mAccessToken { get; set; }
    private string mSupplierID = ConfigurationManager.AppSettings["supplierID"].ToString();
    private int mSupplierNumber = Convert.ToInt32(ConfigurationManager.AppSettings["supplierNumber"].ToString());
    private string mSupplierSecret = ConfigurationManager.AppSettings["supplierSecret"].ToString();
	
	private static string AgencyXmlServicesCreateDocketUrl = ConfigurationManager.AppSettings.Get("AgencyXmlServicesCreateDocket");
    private static string createDocketAction = "xmlService.asp";
	
    public CancelOrderWebService()
    {

	}
	
    [WebMethod(EnableSession = true)]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public int CancelOrder(string travellerDocketID, string govOrderID, string indicationCancelReason, string daysOfStayToShort)
    {
		//start as true, in case canceling with other reason than 1,6,7,9,10.
		bool cancelSucceeded = true;
		AgencyUser user = new AgencyUser();
        
        user.AgencyId = "85";
        user.AgencySystemType = "3";
        user.AgencyUserId = "1";
        user.AgencyUserName = "Agency2000";
        user.AgencyUserPassword = "11071964";
      
	    DAL_SQL.ConnStr = ConfigurationManager.AppSettings["CurrentAgencyDBConnStr"];
        DAL_SQL.ConnStr = DAL_SQL.ConnStr.Replace("^agencyId^", ((user.AgencyId.Length == 1) ? "000" + user.AgencyId : "00" + user.AgencyId));
        DAL_SQL.ConnStr = DAL_SQL.ConnStr.Replace("^systemType^", ((user.AgencySystemType == "3") ? "INN" : "OUT"));
		
        int failureCancle = 0;
        int fineDays = 0;
        int cancelReason = 0;
		
		//if (govOrderID == "92459")
		//{
		//	System.Threading.Thread.Sleep(200000);
		//}
		
Logger.Log("CancelOrder -recieved:  reason=" + indicationCancelReason + ",travellerDocketID= "+ travellerDocketID + ", govOrderID= " + govOrderID);
        switch (indicationCancelReason)
        {
            case "1":
                cancelReason = 1;
                fineDays = 1;
                break;
            case "7":
            case "10":
                cancelReason = 2;
                fineDays = 1;
                break;
            case "13":
                fineDays = 0;
                cancelReason = 0;
                break;
            default:
                break;
        }

		try 
		{
			failureCancle = cancelOrder(travellerDocketID, govOrderID, cancelReason, fineDays);
		}
		catch (Exception ex12)
		{
			failureCancle = -1;
			Logger.Log("Failed to cancelOrder. exception = " + ex12.Message);
		}
		
		//No error.
		if (failureCancle == 0 || govOrderID.Trim() == "9999999" || govOrderID.Trim() == "114598")
		{
			if (indicationCancelReason == "1" || indicationCancelReason == "6" || indicationCancelReason == "7" || indicationCancelReason == "9" || indicationCancelReason == "10")
			{
				cancelSucceeded = cancelVoucherWithCharges(govOrderID, int.Parse(indicationCancelReason));
				if (!cancelSucceeded)
				{
					failureCancle = 99;
					//TODO: add to maatafa.  ('אירעה שגיאה בעת הוספת השורה לחיוב על יום 1. יש להוסיף ידנית את השורה.');
				}
			}
			
			string bundleId = string.Empty;
			
			try
			{
				bundleId = DAL_SQL.GetRecord("BUNDLES", "id", govOrderID, "gov_order_id");				

				Logger.Log("Updating gov_indicator_id = " + indicationCancelReason + ", bundleId = " + bundleId);
				DAL_SQL.UpdateRecord("BUNDLES", "gov_indicator_id", indicationCancelReason, bundleId, "id");
				Logger.Log("Succeded to update gov_indicator_id = " + indicationCancelReason + ", bundleId = " + bundleId);
			}
			catch(Exception ex)
			{
				Logger.Log("Failed to update gov_indicator_id = " + indicationCancelReason + ", bundleId = " + bundleId + ", Exception = " + ex.Message);
			}
		}
		
        return failureCancle;
    }

    private int cancelOrder(string iDocketId, string iOrderID, int iCancleReason, int iFineDays)
    {
		System.Net.ServicePointManager.SecurityProtocol = (System.Net.SecurityProtocolType)3072;
        TreatmentEntitledServiceClient treatmentClient = new TreatmentEntitledServiceClient();
		
		Logger.Log("|Cancel Order| before GetTokenMoked");
        ServiceTokenResponse tokenResponse = treatmentClient.GetTokenMoked(iDocketId, mSupplierNumber, mSupplierSecret, mSupplierID);
		Logger.Log("|Cancel Order| after GetTokenMoked");
		
		string[] messages;
		string message;
		
		try
		{
			messages = tokenResponse.Status.ClientMessages;
			message = messages[0];
		}
		catch(Exception ex123)
		{
			message = string.Empty;
			Logger.Log("Exception when get message. exception = " + ex123.Message);
		}
		
        StringBuilder clientFailedMsgAccessT = new StringBuilder();
        int failureCancle = 0;

		try 
		{
			if (tokenResponse.FailureCode != null)
			{
				Logger.Log("CancelOrder - 4");
				failureCancle = tokenResponse.FailureCode.Id;
				foreach (string msg in tokenResponse.FailureCode.ClientMessages)
				{
					clientFailedMsgAccessT.Append(msg);
					clientFailedMsgAccessT.Append(Environment.NewLine);
				}
				
				Logger.Log("|Cancel Order| Failed to get Token in cancel order WS: " + clientFailedMsgAccessT  + ", " + failureCancle + ", Traveller Docket Id: "+ iDocketId + ", Order Id: " + iOrderID+ ", CancleReason: "+ iCancleReason+ ", FineDays: "+iFineDays.ToString());    
				Logger.Log("|Cancel Order| Docket is canceled but gov order is still alive");
			}
			else
			{
				mAccessToken = tokenResponse.AccessToken;
				int OrderID = int.Parse(iOrderID);
				StringBuilder clientFailedMsg = new StringBuilder();
				
				Logger.Log("|Cancel Order| before cancel in gov");
				ServiceResponse cancelResponse = treatmentClient.CancelOrder(mAccessToken, OrderID, iFineDays, iCancleReason);
				Logger.Log("|Cancel Order| after cancel in gov");
				
				string orderStatus = cancelResponse.Status.ClientMessages[0];
				Logger.Log("Canceled order: " + iOrderID + " || Docket ID: " + iDocketId);

				if (cancelResponse.FailureCode != null)
				{
					failureCancle = cancelResponse.FailureCode.Id;
					foreach (string msg in cancelResponse.FailureCode.ClientMessages)
					{
						clientFailedMsg.Append(msg);
						clientFailedMsg.Append(Environment.NewLine);

					}
									
					Session["ClientMessage"] = getClientErrorMessage(failureCancle);
					Logger.Log("Failed to cancel order : " + clientFailedMsg.ToString() + "       id = " +  getClientErrorMessage(failureCancle));
					//Response.Redirect("./Default.aspx");//chen - todo pass error file
				}
				else
				{
					//succeded to cancel order
					Logger.Log("Order ID: " + iOrderID + " Was Cancelled in GOV");
					
					 string bundleId = string.Empty;

					try
					{
						bundleId = DAL_SQL.GetRecord("BUNDLES", "id", iOrderID, "gov_order_id");

						DAL_SQL.UpdateRecord("BUNDLES", "gov_voucher_cancel_date", DateTime.Now.Date.ToString("dd-MMM-yy"), bundleId, "id");
						DAL_SQL.UpdateRecord("BUNDLES", "last_update_date", DateTime.Now.Date.ToString("dd-MMM-yy"), bundleId, "id");                                                           
					}
					catch (Exception ex)
					{
						Logger.Log("Failed to update last_update_date, gov_voucher_cancel_date = " + DateTime.Now.Date.ToString("dd-MMM-yy")  + ", bundleId = " + bundleId + ", Exception = " + ex.Message);
					} 
				}
				
				treatmentClient.Close();
			}
		}
		catch (Exception ex1234)
		{
			Logger.Log("Error occured while canceling. exception = " + ex1234.Message);
		}
		

        return failureCancle;
    }

    private string getClientErrorMessage(int iFailureID)
    {
        const string kPleaseReLoginMsg = "אירעה שגיאה, אנא התחברו מחדש";
        string failureMessageToClient = string.Empty;
        string logMessage = string.Empty;
        switch (iFailureID)
        {
            case 1:
                failureMessageToClient = kPleaseReLoginMsg;
                logMessage = "שגיאת ולידציה-נתונים חסרים או לא תקינים";
                break;
            case 4:
                failureMessageToClient = kPleaseReLoginMsg;
                logMessage = "Access Token אינו תקין או פג תוקף";
                break;
            case 3:
                failureMessageToClient = kPleaseReLoginMsg;
                logMessage = "אירעה שגיאה";
                break;
            case 6:
                failureMessageToClient = "שגיאה, הזמנה לא קיימת";
                logMessage = "הזמנה לא קיימת (במקרה של ביטול)";
                break;
            case 9:
                failureMessageToClient = "הזמנה זו רשומה על שם ספק אחר, אנא צור קשר עם אגף שיקום";
                logMessage = "מקט לא קיים לזכאי";
                break;
            case 7:
                failureMessageToClient = "הזמנה מבוטלת";
                logMessage = "הזמנה בסטאטוס מבוטל (במקרה של ביטול - לא ניתן לבטל, כבר בוטלה)";
                break;
            default:
                failureMessageToClient = kPleaseReLoginMsg;
                break;
        }

        Logger.Log(logMessage);

        return failureMessageToClient;
    }
	
    public static bool cancelVoucherWithCharges(string iGovOrderId, int iIndication)
    {
        bool isSucceded = true;
        int percent = 50;
        string bundleId = DAL_SQL.GetRecord("BUNDLES", "id", "gov_order_id", iGovOrderId);
		//Logger.Log("DAL_SQL.ConnStr = " + DAL_SQL.ConnStr);
        if (isSucceded)
        {
            switch (iIndication)
            {
                case 1:
                    percent = 50;
                    break;
                case 6:
                    percent = 50;
                    break;
                case 7:
                    percent = 50;
                    break;
                case 9:
                    percent = 100;
                    break;
                case 10:
                    percent = 50;
                    break;

                default:
                    percent = 0;
                    break;
            }
			
			try
			{
				isSucceded = createNewBundleRowInDocket(bundleId, percent, iIndication);
			}
			catch(Exception excs)
			{
				Logger.Log(excs.Message + " stack = " + excs.StackTrace);
				throw excs;
			}
        }
        else
        {
            Logger.Log("Failed to update indication.");
        }
                 

        return isSucceded;
    }

    private static bool createNewBundleRowInDocket(string bundleId, int iPercent, int iIndication)
    {
        bool isSucceded = true;
        DateTime fromDate, toDate;
        XmlDocument doc = new XmlDocument();
        string newVoucherId = string.Empty;
		string newBundleId = string.Empty;

		Logger.Log("11 - bundleId = " + bundleId);
        string makatFromBundle = DAL_SQL.RunSql("SELECT gov_makat_number FROM TRAVELLERS WHERE id_no <> '' and id in (SELECT traveller_id FROM BUNDLES_to_TRAVELLERS as BTT WHERE bundle_id = " + bundleId + " and BTT.subsid <> 0)");
		Logger.Log("here 1");
		string travDocketId = DAL_SQL.RunSql("SELECT gov_docket_id FROM TRAVELLERS WHERE id_no <> '' and id in (SELECT traveller_id FROM BUNDLES_to_TRAVELLERS as BTT WHERE bundle_id = " + bundleId + " and BTT.subsid <> 0)");
		Logger.Log("here 2");
        string priceId = DAL_SQL.GetRecord("HOTELS", "hotel_price_id", "bundle_id", bundleId);
		Logger.Log("here 3");
        string supplierPriceId = DAL_SQL.GetRecord("HOTELS", "supplier_price_id", "bundle_id", bundleId);
		Logger.Log("here 4");
        string hotelId = DAL_SQL.GetRecord("BUNDLES", "carrier", "id", bundleId);

		Logger.Log("12");
        fromDate = DateTime.Parse(DAL_SQL.GetRecord("BUNDLES", "from_date", "id", bundleId));
        toDate = fromDate.AddDays(1);
		
		//Get a traveller to open new row in the docket.
		GovTraveller traveller = new GovTraveller();
		traveller = traveller.LoadDataByDocketId(travDocketId);
		traveller.RoomsAmount = int.Parse(DAL_SQL.GetRecord("HOTELS", "rooms", "bundle_id", bundleId));
		Logger.Log("13");
        traveller.SaveToSession();
		traveller = GovTraveller.LoadFromSession();
        traveller.AgencyDocketId = DAL_SQL.GetRecord("BUNDLES", "docket_id", "id", bundleId);

		int i = 0;
		foreach(GovTravellerMakat makat in traveller.Makats)
		{
			if (makat.ItemSKU == makatFromBundle)
			{
				traveller.SelectedMakat.Add(traveller.Makats[i]);
				break;
			}
			
			i++;
		}
		Logger.Log("14");
        //When adding the docket id into "traveller then its does not create new docket but add new bundle.
        string createDocketXml = Agency2000Proxy.getAgencyCreateDocketXML2(int.Parse(hotelId), int.Parse(priceId), int.Parse(supplierPriceId), fromDate, toDate, 0, 0, traveller, makatFromBundle);
		Logger.Log("15 - createDocketXml = " + createDocketXml);
        string createDocketXmlUrl = getAgencyCreateDocketUrl() + "&Query=" + createDocketXml;
		
        try
        {
            doc = Agency2000Proxy.getAgencyRequestXmlDoc(createDocketXmlUrl);
			Logger.Log("16 - doc = " + doc.OuterXml);
            if (doc == null)
            {
                Logger.Log("doc == null | OrderMessage = לא ניתן לפתוח תיק.");
                return false;
            }

			Logger.Log("17");
			
            // check if all nodes are ok
            XmlNodeList statuses = doc.SelectNodes("//STATUS");
			Logger.Log("18");
            foreach (XmlNode status in statuses)
            {
                if (status.InnerText.ToLower() != "ok" && status.InnerText.ToLower() != "existing docket")
                {
                    Logger.Log("status.InnerText.ToLower() != ok | לא ניתן ליצור תיק, אנא פנה למנהל מערכת.");
                    return false;
                }
            }

			Logger.Log("19");
			
			bool gotVoucherformDB = false;
			string tempNewVoucherId = string.Empty;
			
			try
			{
				//In case got catch will get the last voucher in VOUCHERS table.
				newVoucherId = doc.SelectSingleNode("/ROOT/SERVICES/HOTEL/VOUCHER_ID").InnerText;
			}
			catch (Exception ex)
			{
				Logger.Log("19 - 1");
				tempNewVoucherId = (DAL_SQL_Helper.getMaxTableId("VOUCHERS") - 1).ToString();
				Logger.Log("19 - 2");
				isSucceded = !string.IsNullOrEmpty(tempNewVoucherId);
				gotVoucherformDB = true;
			}
			
            if (!gotVoucherformDB)
				isSucceded = newVoucherId != "0";
			else
			{
				newVoucherId = tempNewVoucherId;
			}
			
			
			newBundleId = DAL_SQL.GetRecord("VOUCHERS","bundle_id", "id", newVoucherId);
			Logger.Log("19 - 3 - newBundleId = " + newBundleId  + ", bundleId = " + bundleId + ", newVoucherId = " + newVoucherId);
			if (DAL_SQL.GetRecord("BUNDLES","docket_id", "id", newBundleId) == traveller.AgencyDocketId)
			{
				//Attach the travellers to the new bundle.
				try
				{
					Logger.Log("19 - 3 - newBundleId = " + newBundleId  + ", bundleId = " + bundleId);
					DataSet dsBundlesToTravellers = DAL_SQL.RunSqlDataSet("SELECT * FROM BUNDLES_to_TRAVELLERS WHERE bundle_id = " + bundleId);
					
					if (isDataSetRowsNotEmpty(dsBundlesToTravellers))
					{
						string query = string.Empty;
						i = 0;
						
						foreach(DataRow rowBTT in dsBundlesToTravellers.Tables[0].Rows)
						{
							query = "INSERT INTO BUNDLES_to_TRAVELLERS ";
							
							//TODO get the old row and update.
							query += " VALUES (";
							query += DAL_SQL_Helper.getMaxTableId("BUNDLES_to_TRAVELLERS").ToString() + ", ";
							i = 0;
							
							foreach(DataColumn col in rowBTT.Table.Columns)
							{
								//Logger.Log("dtaInBTT = " + col.ColumnName);
								if (i != 0)
								{
									if (col.ColumnName == "bundle_id")
									{
										query += (newBundleId.ToString() + ", ");
									}
									else
									{
										string value = "";
										
										if (string.IsNullOrEmpty(rowBTT[col.ColumnName].ToString()) || 
											col.ColumnName == "f_ticket" || 
											col.ColumnName == "i_passport" || 
											col.ColumnName == "v_passport" || 
											col.ColumnName == "BSP_File" || 
											col.ColumnName == "remark")
										{
											value = "''";
										}
										else
										{
											if (col.ColumnName == "traveller_type")
											{
												value = "N'" + rowBTT[col.ColumnName].ToString() + "'";
											}
											else if(col.ColumnName == "from_date")
											{
												value = "N'" + fromDate.ToString("dd-MMM-yy") + "'";
											}										
											else if(col.ColumnName == "to_date")
											{
												value = "N'" + toDate.ToString("dd-MMM-yy") + "'";
											}											
											else
											{
												value = rowBTT[col.ColumnName].ToString();
											}
										}
										
										if (i != rowBTT.Table.Columns.Count - 1)
										{
											
											query += (value + ", ");
										}
										else
										{
											query += value;
										}
									}
								}
								i++;
							}
							
							query += ")";	
							Logger.Log("19 - 4 -- " + query);
							DAL_SQL.RunSql(query);
							Logger.Log("after update BTT");
						}
					}
					
				}
				catch (Exception ex)
				{
					Logger.Log("Failed to attach travellers. exception  = " + ex.Message);
					Logger.Log("stackTrace = " + ex.StackTrace);
					isSucceded = false;
				}
			}
			else
			{
					Logger.Log("Failed to create new bundle.");
					isSucceded = false;
			}
			
			if (isSucceded)
			{
				//Update from_date in new bundle (for any reason it gets 1/1/2000... so fixing it.)
				try
				{
					DAL_SQL.RunSql("UPDATE BUNDLES set from_date = '" + fromDate.ToString("dd-MMM-yy") + "' WHERE id = " + newBundleId);
					try
					{
						string erkevTypeTemp = DAL_SQL.GetRecord("BUNDLES", "erkev_type", "id", bundleId);
						DAL_SQL.RunSql("UPDATE BUNDLES set erkev_type = " + erkevTypeTemp + " WHERE id = " + newBundleId);
					}
					catch(Exception exexexex)
					{
						Logger.Log("Failed to get erkev_type. exceprtion = " + exexexex.Message);
					}
				}
				catch(Exception exxxx)
				{
					Logger.Log("exception while update fromDate. exception = " + exxxx.Message);
				}
				//DAL_SQL.RunSql("UPDATE BUNDLES set gov_order_id = " + DAL_SQL.GetRecord("BUNDLES", "gov_order_id", "id", bundleId) + " WHERE id = " + newBundleId);
			}
			
			Logger.Log("20");
        }
        catch(Exception ex)
        {
            Logger.Log("Failed to create new bundel in agency. Exception = " + ex.Message);
            isSucceded = false;
        }

		
		if (isSucceded)
		{
			Logger.Log("24");
			try
			{
				Logger.Log("25");
				bool updateNewBundle = true;
				isSucceded = updateIndicationInBundleRow(newBundleId, iIndication);
				if (isSucceded)
				{
					Logger.Log("before updateBundlesHotelPrices");
					isSucceded = updateBundlesHotelPrices(bundleId, 1, updateNewBundle, newVoucherId, iPercent);
					Logger.Log("after updateBundlesHotelPrices");
					
					if (isSucceded)
					{
						Logger.Log("before updateBundleOtherPrices");
						string docketIdTemp = DAL_SQL.GetRecord("VOUCHERS", "docket_id", "bundle_id", bundleId);
						Logger.Log("docketIdTemp = " + docketIdTemp + ", bundleId  = " + bundleId);
						string bundleIdOther = DAL_SQL.RunSql("SELECT bundle_id FROM VOUCHERS WHERE service_type_id = 8 and status = 1 and docket_id = " + docketIdTemp);
						
						Logger.Log("bundleIdOther = " + bundleIdOther);
						isSucceded = updateBundleOtherPrices(bundleIdOther, 1);
						Logger.Log("after updateBundleOtherPrices");
						
						try
						{
							if (isSucceded)
							{
								Logger.Log("UPDATE BUNDLES SET gov_indicator_id = " + iIndication + " WHERE id = " + bundleIdOther);
								DAL_SQL.RunSql("UPDATE BUNDLES SET gov_indicator_id = " + iIndication + " WHERE id = " + bundleIdOther);
							}
						}
						catch(Exception exchenchen1)
						{
							Logger.Log("Failed to update indication in other. exception = " + exchenchen1.Message);
						}
							
						//Update BUNDLES_TO_ROOMS
						Logger.Log("before updating BUNDLES_TO_ROOMS");
						string queryUpdayeBundlesToRooms = "UPDATE BUNDLES_TO_ROOMS SET bundle_id = " + newBundleId + " , room = " + traveller.RoomsAmount + " WHERE bundle_id = " + bundleId;
						Logger.Log("query = " + queryUpdayeBundlesToRooms);
						DAL_SQL.RunSql(queryUpdayeBundlesToRooms);
						Logger.Log("after updating BUNDLES_TO_ROOMS");
						
						//Update HOTELS_TO_ROOMS_TYPE
						Logger.Log("before updating HOTELS_TO_ROOMS_TYPE");
						string hotelsDbId = DAL_SQL.RunSql("SELECT id FROM HOTELS WHERE bundle_id = " + newBundleId);
						string queryUpdayeHotelsToRoomsType = "UPDATE HOTELS_TO_ROOMS_TYPE SET bundle_id = " + newBundleId + ", hotels_id = " + hotelsDbId + " WHERE bundle_id = " + bundleId;
						Logger.Log("query = " + queryUpdayeHotelsToRoomsType);
						DAL_SQL.RunSql(queryUpdayeHotelsToRoomsType);
						Logger.Log("after updating HOTELS_TO_ROOMS_TYPE");
						
						//Update HOTELS_TO_BASES_TYPE
						Logger.Log("before updating HOTELS_TO_BASES_TYPE");
						string queryUpdayeHotelsToRoomsType2 = "UPDATE HOTELS_TO_BASES_TYPE SET bundle_id = " + newBundleId + " WHERE bundle_id = " + bundleId;
						Logger.Log("query = " + queryUpdayeHotelsToRoomsType2);
						DAL_SQL.RunSql(queryUpdayeHotelsToRoomsType2);
						Logger.Log("after updating HOTELS_TO_BASES_TYPE");
					}
				}
			}
			catch (Exception ex)
			{
				Logger.Log("failed to update price in new bundle id = " + newBundleId + ". exception = " + ex.Message + ", Trace = " + ex.StackTrace);
				isSucceded = false;
			}
		}
		
        return isSucceded;
    }

    private static bool updateIndicationInBundleRow(string iBundleId, int iIndication)
    {
        bool isSucceded = true;

		Logger.Log("6");
        if (!string.IsNullOrEmpty(iBundleId))
        {
			Logger.Log("7");
            DAL_SQL.RunSql("UPDATE BUNDLES SET gov_indicator_id = " + iIndication + " WHERE id = " + iBundleId);
			Logger.Log("8");
        }
        else
        {
			Logger.Log("9");
            isSucceded = false; 
        }

        return isSucceded;
    }
	
	private static bool updateToDateInHotelsAndBundlesAndBundlesToTravellers(DateTime toDate, string bundleId, int serviceType)
    {
        string toDateStr = string.Empty;
        bool isSucceded = true;

        toDateStr = toDate.ToString("dd-MMM-yy");

		Logger.Log("xx - toDateStr = " + toDateStr);
		
        string queryUpdateBundles = "UPDATE BUNDLES SET to_date = '" + toDateStr + "' WHERE id = " + bundleId;
        string queryUpdateBundlesToTravellers = "UPDATE BUNDLES_to_TRAVELLERS SET to_date = '" + toDateStr + "' WHERE bundle_id = " + bundleId;
        string queryUpdateHotels = "UPDATE HOTELS SET exit_date = '" + toDateStr + "' WHERE bundle_id = " + bundleId;
		string queryUpdateOther = "UPDATE OTHER SET exit_date = '" + toDateStr + "' WHERE bundle_id = " + bundleId;
		
        try
        {
            DAL_SQL.RunSql(queryUpdateBundles);

            try
            {
                DAL_SQL.RunSql(queryUpdateBundlesToTravellers);

                try
                {
					//Hotel
					if (serviceType == 2)
					{
						DAL_SQL.RunSql(queryUpdateHotels);
					}
					else
					{
						if (serviceType == 8)
						{
							DAL_SQL.RunSql(queryUpdateOther);
						}
					}
                }
                catch (Exception e)
                {
                    Logger.Log("Failed to update toDate in HOTELS / OTHER (serviceType = " + serviceType + ". exception = " + e.Message);
                    isSucceded = false;
                }
            }
            catch (Exception e1)
            {
                Logger.Log("Failed to update toDate in BUNDLES_to_TRAVELLERS. exception = " + e1.Message);
                isSucceded = false;
            }
        }
        catch (Exception e2)
        {
            Logger.Log("Failed to update toDate in BUNDLES. exception = " + e2.Message);
            isSucceded = false;
        }

        return isSucceded;
    }
	
	private static bool updateBundlesHotelPrices(string iBundleId, int iOrderDaysAfterShort, bool updateNewBundle, string voucherId, int iPercent)
    {
        string bundleId = iBundleId;
        string queryUpdateBundles = string.Empty;
        string queryUpdateBundlesToTravellers = string.Empty;
        string queryUpdateHotels = string.Empty;
        string priceId = string.Empty;
        string erkevType = string.Empty;
		int serviceType = 2;
		
        double amountBrutto = 0;
        double amountNetto = 0;
        double amountBruttoTemp = 0;
        double amountNettoTemp = 0;

		string newBundleId = string.Empty;
			
		newBundleId = DAL_SQL.GetRecord("VOUCHERS", "bundle_id", "id", voucherId);
		DateTime fromDate, toDate;
	
		fromDate = DateTime.Parse(DAL_SQL.GetRecord("BUNDLES", "from_date", "id", bundleId));
		Logger.Log("21");
		//Update to date in tables: BUNDLES, BUNDLES_to_TRAVELLERS
		toDate = fromDate.AddDays(1);

		if (updateToDateInHotelsAndBundlesAndBundlesToTravellers(toDate, newBundleId, serviceType))
		{
			
			//Get prices to change
			erkevType = DAL_SQL.GetRecord("BUNDLES", "erkev_type", "id", bundleId);
			priceId = DAL_SQL.GetRecord("HOTELS", "hotel_price_id", "bundle_id", bundleId);
			List<int> hotelRoomType = getHotelRoomsTypes(erkevType);
	Logger.Log("22");
			DataSet dsPriceDetails = DAL_SQL.RunSqlDataSet("SELECT * FROM PR_HOTEL_PRICES_DETAILS WHERE price_id = " + priceId);
			AgencyPricesSearch searchWs = new AgencyPricesSearch();
			foreach (int roomType in hotelRoomType)
			{
				if (priceId != "-1")
				{
					Logger.Log("Release fromDate = " + fromDate.ToString("dd-MMM-yy") + ", toDate = " + toDate.ToString("dd-MMM-yy"));
					setRoomTypeNettoBrutto(dsPriceDetails, roomType, fromDate, toDate, (toDate - fromDate).Days, out amountBruttoTemp, out amountNettoTemp);
				}
				else
				{
					
					string bruttoNetto = searchWs.GetPriceByPriceIdAndRoomTypesAndDates("85", "3", priceId, 
												  roomType.ToString(), fromDate.ToString("dd-MMM-yy"), toDate.ToString("dd-MMM-yy"),
												  string.Empty, DAL_SQL.GetRecord("BUNDLES", "carrier", "id", bundleId));
					double.TryParse(bruttoNetto.Split('|')[0].Split(':')[1], out amountBruttoTemp);
					double.TryParse(bruttoNetto.Split('|')[1].Split(':')[1], out amountNettoTemp);
				}
				
				amountBrutto += amountBruttoTemp;
				amountNetto += amountNettoTemp;
			}
	Logger.Log("23");
			string incomeType = DAL_SQL.GetRecord("BUNDLES", "income_type", "id", bundleId);
			string vatPercent = DAL_SQL.GetRecord("BUNDLES", "vat_percent", "id", bundleId);
			BundleRow brHotel = null;

			try
			{
				Logger.Log("got here 1");
				brHotel = new BundleRow(amountBrutto * iPercent / 100, amountNetto, incomeType, vatPercent);
				Logger.Log("got here 2");
			}
			catch (Exception ex)
			{
				Logger.Log("Failed to crate BundleRow object. exception = " + ex.Message);
				return false;
			}

			//Update prices in bundle and bundle_to_travellers
			Logger.Log("got here 3");
			
			bool isUpdateHotelSucceded = true;
			
			if (!updateNewBundle)
			{
				isUpdateHotelSucceded = updatePrices(brHotel, bundleId);
			}	
			else
			{
				isUpdateHotelSucceded = updatePrices(brHotel, newBundleId);
			}
			
			Logger.Log("got here 4");
			
			return isUpdateHotelSucceded;
		}
		else
		{
			return false;
		}
    }

    private static bool updateBundleOtherPrices(string iBundleId, int iOrderDaysAfterShort)
    {
        bool hasAttraction = false;
        double amountBrutto = 0;
        double amountNetto = 0;
		Logger.Log("updateBundleOtherPrices 1");
		
		if (!string.IsNullOrEmpty(iBundleId))
		{
			DataSet dsAttraction = DAL_SQL.RunSqlDataSet("SELECT * FROM VOUCHERS WHERE status = 1 and service_type_id = 8 and bundle_id = " + iBundleId);
			hasAttraction = isDataSetRowsNotEmpty(dsAttraction);
			Logger.Log("updateBundleOtherPrices 2");
			DateTime fromDate, toDate;

			fromDate = DateTime.Parse(DAL_SQL.GetRecord("BUNDLES", "from_date", "id", iBundleId));
	Logger.Log("updateBundleOtherPrices 3");
			//Update to date in tables: BUNDLES, BUNDLES_to_TRAVELLERS
			toDate = fromDate.AddDays(iOrderDaysAfterShort);
			
			//if has attraction
			if (hasAttraction)
			{
				DAL_SQL.RunSql("UPDATE OTHER SET nights = " + iOrderDaysAfterShort + ", qty = " + iOrderDaysAfterShort + " WHERE bundle_id = " + iBundleId);
				
				
				int serviceType = 8;
				bool isSucceded = updateToDateInHotelsAndBundlesAndBundlesToTravellers(toDate, iBundleId, serviceType);
				
				if (isSucceded)
				{
					Logger.Log("updateBundleOtherPrices 4");
					BundleRow brOther = null;
					string voucherIdOther = dsAttraction.Tables[0].Rows[0]["id"].ToString();
					string incomeTypeOther = DAL_SQL.GetRecord("BUNDLES", "income_type", "id", iBundleId);
					string vatPercentOther = DAL_SQL.GetRecord("BUNDLES", "vat_percent", "id", iBundleId);
		Logger.Log("updateBundleOtherPrices 5");
					Logger.Log("About to change *other* BUNDLES, BUNDLES_to_TRAVELLERS price. brutto = " + amountBrutto + ", netto = " + amountNetto);
					string addId = getAddId(iBundleId);
					string supplierId = DAL_SQL.GetRecord("BUNDLES", "carrier", "id", iBundleId);
		Logger.Log("updateBundleOtherPrices 6");
					amountBrutto = DAL_SQL_Helper.GetBrutoPriceForAttraction(int.Parse(iBundleId), supplierId, addId, iOrderDaysAfterShort.ToString(), fromDate, toDate);
					amountNetto = DAL_SQL_Helper.GetNettoPriceForAttraction(int.Parse(iBundleId), supplierId, addId, iOrderDaysAfterShort.ToString(), fromDate, toDate);
					DAL_SQL.RunSql("UPDATE BUNDLES_TO_SUPPLIERS_ADDS SET quantity = " + iOrderDaysAfterShort + ", amount = " + amountBrutto + " WHERE bundle_id = " + iBundleId);
					DAL_SQL.RunSql("UPDATE BUNDLES_to_TRAVELLERS SET quantity = " + iOrderDaysAfterShort + " WHERE bundle_id = " + iBundleId + " and subsid <> 0");
					
		Logger.Log("updateBundleOtherPrices 7");
					try
					{
						Logger.Log("updateBundleOtherPrices 8");
						brOther = new BundleRow(amountBrutto, amountNetto, incomeTypeOther, vatPercentOther);
						Logger.Log("updateBundleOtherPrices 9");
						if (!updatePrices(brOther, iBundleId))
						{
							Logger.Log("updateBundleOtherPrices 10");
							Logger.Log("Failed to update Other");
							return false;
						}
					}
					catch (Exception ex)
					{
						Logger.Log("Failed to crate BundleRow object. exception = " + ex.Message);
						return false;
					}
				}
				else
				{
					Logger.Log("Failed to updaate toDate, bundleId = " + iBundleId + ".");
					return false;
				}
			}
			else
			{
				Logger.Log("there is no attraction.");
				Logger.Log("updateBundleOtherPrices 11");
			}

			Logger.Log("updateBundleOtherPrices 12");
			return true;
		}
		else
		{
			//Means there is no attraction.
			return true;
		}
    }

    private static bool updatePrices(BundleRow iBundleRow, string bundleId)
    {
        bool isSucceded = true;

        string updateBundlesQuery = "UPDATE BUNDLES SET " + iBundleRow.getBUNDLESUpdateString() + " WHERE id = " + bundleId;
        string updateBundlesToTravellersQuery = "UPDATE BUNDLES_to_TRAVELLERS SET " + iBundleRow.getBUNDLES_to_TRAVELLERSUpdateString() + " WHERE bundle_id = " + bundleId + " and subsid <> 0"; //only main traveller!

        Logger.Log("About to change *hotel* BUNDLES, BUNDLES_to_TRAVELLERS price. brutto = " + iBundleRow.mAmount + ", netto = " + iBundleRow.mToSupplier);
        try
        {
            Logger.Log("update bundles netto etc. query = " + updateBundlesQuery);
            DAL_SQL.RunSql(updateBundlesQuery);
            Logger.Log("Done. update bundles netto etc. query = " + updateBundlesQuery);
            try
            {
                Logger.Log("update bundles_to_travllers netto etc. query = " + updateBundlesToTravellersQuery);
                DAL_SQL.RunSql(updateBundlesToTravellersQuery);
                Logger.Log("Done. update bundles_to_travllers netto etc. query = " + updateBundlesToTravellersQuery);
            }
            catch (Exception ex1)
            {
                Logger.Log("Failed to update bundles_to_travllers prices! exception = " + ex1.Message);
                isSucceded = false;
            }
        }
        catch (Exception ex)
        {
            Logger.Log("Failed to update bundles prices! exception = " + ex.Message);
            isSucceded = false;
        }

        return isSucceded;
    }

    private static void setRoomTypeNettoBrutto(DataSet iPriceDetails, int iRoomType, DateTime iFromDate, DateTime iToDate, int iNights, out double ioAmountBrutto, out double ioAmountNetto)
    {
        int fromDay = getDayOfWeek(iFromDate);
        int toDay = getDayOfWeek(iToDate);
        double brutto = 0;
        double netto = 0;

        int countNights = 0;
        //Must assign into out variables before use.
        ioAmountBrutto = brutto;
        ioAmountNetto = netto;

        foreach (DataRow priceRow in iPriceDetails.Tables[0].Rows)
        {
			Logger.Log("in loop 1");
            if (int.Parse(priceRow["hotel_room_type_id"].ToString()) == iRoomType)
            {
				Logger.Log("fromDay = " + fromDay + ", toDay = " + toDay + ", iNights = " + iNights + ", countNights = " + countNights);
                while (fromDay != toDay || iNights != countNights)
                {
					Logger.Log("in loop 2");
                    brutto = double.Parse(priceRow["night_bruto_" + fromDay].ToString());
                    netto = double.Parse(priceRow["night_netto_" + fromDay].ToString());
                    ioAmountBrutto += brutto;
                    ioAmountNetto += netto;

                    fromDay++;
                    if (fromDay == 8)// (8) means its sunday.
                        fromDay = 1;
                    countNights++;
                }

                break;
            }
        }
    }

    private static List<int> getHotelRoomsTypes(string iErkevType)
    {
        //How many travellers GOV pays for.
        const int single = 1;
        const int couple = 2;
        const int couple1WithSubsid = 2330;
        List<int> hotelRoomsTypeId = new List<int>();

        switch (iErkevType.Trim())
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
                hotelRoomsTypeId.Add(couple1WithSubsid);
                break;
            case "ZakaiAndMelaveBeTashlumHelekTkufa": //Chen. Verify
                hotelRoomsTypeId.Add(single);
                break;
            case "ZakaiBeTashlumAndMelaveLeLoTashlum": //makat 40 
                hotelRoomsTypeId.Add(single);
                break;
            case "ZakaiAndMelaveTmuratZakaut":
                hotelRoomsTypeId.Add(couple);
                break;

            default:
                Logger.Log("No 'erkev_type' found");
                break;
        }

        return hotelRoomsTypeId;
    }

    private static bool isDataSetRowsNotEmpty(DataSet iDataSet)
    {//Check if the DataSet has rows in first table.
        bool isNotEmpty = false;

        if (iDataSet != null && iDataSet.Tables != null && iDataSet.Tables.Count > 0
                && iDataSet.Tables[0].Rows != null && iDataSet.Tables[0].Rows.Count > 0)
        {
            isNotEmpty = true;
        }

        return isNotEmpty;
    }

    private static int getDayOfWeek(DateTime iDate)
    {
        return ((int)iDate.DayOfWeek + 1);
    }

    private static string getAddId(string iBundleId)
    {
        string addId = string.Empty;
        string travIdFromBTT = string.Empty;
        string makat = string.Empty;
        string travId = string.Empty;

        travIdFromBTT = DAL_SQL.GetRecord("BUNDLES_to_TRAVELLERS", "traveller_id", iBundleId, "bundle_id");
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

    private static string getInsertToBundleHistoryQuery(string bundleId)
    {
        string str = string.Format(@"
IF (EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES 
                 WHERE  TABLE_NAME = 'BUNDLES_HISTORY'))
BEGIN
INSERT INTO 
BUNDLES_HISTORY(
bundle_id, docket_id, service_type,
 amount, tax, vat_percent, 
 vat_value, adds_amount, commision, commision_value, commision_vat, to_supplier, 
 to_supplier_vat,to_clerk, to_clerk_vat, to_clerk_percent, total_supplier, xchange_rate_usd, xchange_rate_nis, currency_id, 
 price, mark_up, mark_up_vat, cdate, author_id, last_update_date, 
 last_update_clerk_id, carrier, pay_to_supplier_id, payment_type_id, 
 income_type, remark, subsid, trav_pay, from_date, to_date, 
 staff_quantity, staff_remark, staff_amount, advance_amount1, advance_date1, advance1_prq_id, advance_amount2, advance_date2, 
 advance2_prq_id, advance_amount3, advance_date3, advance3_prq_id, traveller_remark, 
 external_commision, seller_id, supplier_invoice, source_type_id, 
 source_id, erkev_type, melave_selected_nights, four_one_seven, 
 gov_connected_voucher_number, gov_indicator_id, gov_voucher_cancel_date, gov_order_id
)
SELECT id, docket_id, service_type,amount, tax, vat_percent, 
 vat_value, adds_amount, commision, commision_value, 
 commision_vat, to_supplier, to_supplier_vat, to_clerk, 
 to_clerk_vat, to_clerk_percent, total_supplier, xchange_rate_usd, 
 xchange_rate_nis, currency_id, price, mark_up, mark_up_vat, 
 cdate, author_id, last_update_date, last_update_clerk_id, 
 carrier, pay_to_supplier_id, payment_type_id, income_type, 
 remark, subsid, trav_pay, from_date, to_date, staff_quantity, 
 staff_remark, staff_amount, advance_amount1, advance_date1, 
 advance1_prq_id, advance_amount2, advance_date2, advance2_prq_id, 
 advance_amount3, advance_date3, advance3_prq_id, traveller_remark, 
 external_commision, 
 seller_id, supplier_invoice, source_type_id, source_id, erkev_type, 
 melave_selected_nights, four_one_seven, gov_connected_voucher_number, 
 gov_indicator_id, gov_voucher_cancel_date, gov_order_id 
FROM         BUNDLES
WHERE     (id = {0})
END", bundleId);

        return str;
    }

    private static string getAgencyCreateDocketUrl()
    {
        string retUrl = string.Empty;
        retUrl = AgencyXmlServicesCreateDocketUrl + createDocketAction + getAgencyLoginUrlPart();
    
        return retUrl;
    }

    private static string getAgencyLoginUrlPart()
    {
        string str_url = string.Empty;
        AgencyUser user = new AgencyUser();
        str_url = "?AgencyID=" + user.AgencyId + "&SystemType=" + user.AgencySystemType + "&UserName=" + user.AgencyUserName + "&Password=" + user.AgencyUserPassword + "&ClerkID=" + user.AgencyUserId + "&language=1"; // &WSClientIp

        return str_url;
    }
}
