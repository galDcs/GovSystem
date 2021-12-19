using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;
using System.Web;
using System.Xml;

/// <summary>
/// Summary description for hotel_rooms_check
/// </summary>
public class hotel_rooms_check_ws
{
	private static string AgencyXmlServicesCreateDocketUrl = ConfigurationManager.AppSettings.Get("AgencyXmlServicesCreateDocket");
    private static string createDocketAction = "xmlService.asp";
	
	
    public hotel_rooms_check_ws()
    {
        //
        // TODO: Add constructor logic here
        //
    }

    public static bool cancelVoucher(string iDocketID)
    {
        string docket_id = iDocketID;
        int voucher_id = (int)hotel_rooms_check_ws.GetRecord("VOUCHERS", "id", " service_type_id=2 and docket_id=" + docket_id);//
        int bundle_id = (int)hotel_rooms_check_ws.GetRecord("VOUCHERS", "bundle_id", "id=" + voucher_id);//
        int rooms_amount = 0;
        DateTime from_date, to_date;
        int nights = 0;
        const int order_mode = 2;// release rooms
        int supplier_id = 0;

        int PriceID = (int)hotel_rooms_check_ws.GetRecord("HOTELS", "price_agency_id", "bundle_id=" + bundle_id.ToString());
        int docket_remark = 0;// getPriceID - SELECT remark FROM dockets d inner join vouchers v on d.id=v.docket_id WHERE v.id = voucher_id 
        int SupplierPriceID = (int)hotel_rooms_check_ws.GetRecord("HOTELS", "supplier_price_id", "bundle_id=" + bundle_id.ToString());


        if (PriceID > 0 || SupplierPriceID > 0)
        {
            rooms_amount = (int)hotel_rooms_check_ws.GetRecord("HOTELS", "rooms", "bundle_id=" + bundle_id.ToString());
            from_date = DateTime.Parse((hotel_rooms_check_ws.GetRecord("HOTELS", "entrance_date", "bundle_id=" + bundle_id.ToString())).ToString());
            to_date = DateTime.Parse((hotel_rooms_check_ws.GetRecord("HOTELS", "exit_date", "bundle_id=" + bundle_id.ToString())).ToString());
            nights = (int)hotel_rooms_check_ws.GetRecord("HOTELS", "nights", "bundle_id=" + bundle_id.ToString());

            //docket_id = getRecord("VOUCHERS", "docket_id", voucher_id, "id");
            supplier_id = (int)hotel_rooms_check_ws.GetRecord("VOUCHERS", "supplier_id", "id=" + voucher_id);


          if (!checkRoomsAmount(PriceID, SupplierPriceID, rooms_amount, from_date, to_date, order_mode))
            {
                return false;
            }
            else
            {
                cancelVoucherInvoice(voucher_id.ToString(), bundle_id.ToString());
				var attractionVoucherId = hotel_rooms_check_ws.GetRecord("VOUCHERS", "id", " service_type_id=8 And docket_id=" + iDocketID);
				if (attractionVoucherId != null)
				{
					if (!string.IsNullOrEmpty(attractionVoucherId.ToString()))
					{
						cancelVoucherAttraction(attractionVoucherId.ToString());
					}
				}
            }

            //if (!createVoucherCheckBalance(docket_id, supplier_id, nights)) return false;
            //if (!createVoucherCheckBalance(docket_id, supplier_id, from_date, to_date)) return false;

        }
        return true;
    }

	public static bool cancelVoucherAttraction(string iVoucherId)
    {
        bool isSucceed = false;
        try
        {

            int bundle_id = (int)hotel_rooms_check_ws.GetRecord("VOUCHERS", "bundle_id", "id=" + iVoucherId);//

            string tableName = "OTHER";

            DAL_SQL.RunSqlbool("UPDATE VOUCHERS SET status=2 WHERE bundle_id = " + bundle_id);

			DAL_SQL.RunSqlbool("UPDATE " + tableName + " SET service_status_id = 5 WHERE bundle_id =" + bundle_id);

        }
        catch (Exception e)
        {
            Logger.Log("Cancel Voucher Attraction: " + Environment.NewLine + e.Message);
        }
        return isSucceed;
    }
	
    private static void cancelVoucherInvoice(string voucherID, string bundleID)
    {
        string tableName = "HOTELS"; 
        //string bundleID;

        if (!string.IsNullOrEmpty(voucherID))
        {
            DAL_SQL.RunSqlbool("UPDATE VOUCHERS SET status=2 WHERE bundle_id = " + bundleID);
           // srv_tbl_name = GetRecord("VOUCHERS V INNER JOIN AGENCY_ADMIN.dbo.SERVICE_TYPES ST ON V.service_type_id = ST.id", "ST.db_table", " V.id=" + voucherID).ToString();
            DAL_SQL.RunSqlbool("UPDATE " + tableName + " SET service_status_id = 5 WHERE bundle_id =" + bundleID);
        }
    
    }

    public static bool checkRoomsAmount(int PriceID, int SupplierPriceID, int rooms_amount, DateTime from_date, DateTime to_date, int order_mode)
    {
        bool isSucceded = true;

        int Agency_ID = 85;
        int SystemType = 3;
        int SuppPriceType = 1; // 1 - old (by days), 2 - new (by days and room groups)
        string RoomTypeStr = "";

        // this flag = 0 need to check price rooms
        //		flag = 1 need to check the supplier rooms
        if (rooms_amount < 0)
        {
            //Chen 27/10
            Logger.Log("rooms_amount < 0");
            return false;
        }

        if (SupplierPriceID == 0) // use the regular (alocations) rooms from price
        {
            try
            {
                string hotelRoomsCheck = hotel_rooms_check_ws.hotel_rooms_check(Agency_ID, SystemType, SupplierPriceID.ToString(), PriceID.ToString(), SuppPriceType.ToString(), rooms_amount.ToString(), from_date.ToShortDateString(), to_date.ToShortDateString(), order_mode.ToString(), RoomTypeStr);

                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(hotelRoomsCheck);

                XmlNodeList root = xmlDoc.GetElementsByTagName("ROOT");
                if (root.Count == 0)
                {
                    foreach (XmlNode node in root)
                    {
                        switch (node.Name)
                        {
                            case "ROW_DATA":
                                return true;
                                break;
                            case "NoRooms":
                                Logger.Log("There is no rooms...");
                                return false;
                                break;
                            case "ERROR":
                                return true;
                                break;

                            default:
                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex.Message);
                isSucceded = false;
            }
        }
        else // need to use the suppliers rooms based on dates
        {
            if (SupplierPriceID > 0)
            {
                try
                {
                    string hotelRoomsCheck = hotel_rooms_check_ws.hotel_rooms_check(Agency_ID, SystemType, SupplierPriceID.ToString(), PriceID.ToString(), SuppPriceType.ToString(), rooms_amount.ToString(), from_date.ToShortDateString(), to_date.ToShortDateString(), order_mode.ToString(), RoomTypeStr);

                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.LoadXml(hotelRoomsCheck);

                    if (xmlDoc.FirstChild != null && xmlDoc.FirstChild.FirstChild != null)
                    {
                        XmlNode node = xmlDoc.FirstChild.FirstChild;

                        if (node.Name != "OK")
                        {
                            Logger.Log("Error. got tag name : " + node.Name);

                            if (node.Name == "FATAL_ERROR")
                                isSucceded = false;
                        }
                    }
                    else // no data in root
                    {
                        Logger.Log("No data about rooms.");
                        isSucceded = false;
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log(ex.Message);
                    Logger.Log("error check the price xml");
                    isSucceded = false;
                }
            }
        }

        return isSucceded;
    }

    public static string hotel_rooms_check(int AgencyId, int SystemType, string SupplierPriceID, string PriceID, string SuppPriceType,
        string OrderRooms, string FromDate, string ToDate, string MakeOrder, string RoomTypeStr)
    {
		string logMessage = string.Empty;
			
        try
        {

            bool err_flag = false;
            StringBuilder xml = new StringBuilder();
            
            DateTime fromdate, todate;
            DateTime.TryParse(FromDate, out fromdate);
            DateTime.TryParse(ToDate, out todate);
            SuppPriceType = GetStringRecord("SUPPLIERS_TO_PRICES", "type_id", "id=" + SupplierPriceID);

            string str_price_ids = "";
            string str_order_rms = "";
            string str_price_room_group_ids = "";
            string str_order_room_group_rms = "";
			
            if (SuppPriceType == "1")// old allocation
            {
                int TotalDays = (todate - fromdate).Days;
                DateTime date_tmp = fromdate;
                string err = Check_monthly_assignment_num(SupplierPriceID, fromdate, todate, int.Parse(OrderRooms));

                xml.Append("<ROOT>");
                if (MakeOrder == "2")// need to release the rooms
                {
                    int rooms_released;
                    string curr_day_id;
                    rooms_released = 0;
                    for (int i = 0; i < TotalDays; i++)
                    {// releasing/canceling the order for day rooms amount (total)
                        //getting the current day id
                        curr_day_id = GetStringRecord("SUPPLIERS_TO_PRICES_DATES_ROOMS", "id", "(CAST(room_date AS smalldatetime) = CAST('" + date_tmp.ToString("dd-MMM-yyyy") + "' AS smalldatetime)) AND price_date_id=" + SupplierPriceID);

                        if (curr_day_id != null && curr_day_id != "" && curr_day_id != "0")
                        {
                            rooms_released = rooms_released + 1;
                            int check_room_tmp;

                            check_room_tmp = (int)GetRecord("SUPPLIERS_TO_PRICES_DATES_ROOMS", "rooms_ordered", "id=" + curr_day_id);

							logMessage += ("before update allocations, " + date_tmp.ToString("dd-MMM-yyyy") + ":" + check_room_tmp);
                            if (check_room_tmp <= 0)
                                check_room_tmp = 0;
                            else
                                check_room_tmp = check_room_tmp - int.Parse(OrderRooms);
							
                            //  string sql = " UPDATE    SUPPLIERS_TO_PRICES_DATES_ROOMS SET						" +
                            //          "	room_status	  = 1,												" +
                            //          "	rooms_ordered = (SELECT		(rooms_ordered - " + OrderRooms + ")	" +
                            //          "					FROM		SUPPLIERS_TO_PRICES_DATES_ROOMS		" +
                            //          "					WHERE		id = '" + curr_day_id + "')			" +
                            //         " WHERE id = '" + curr_day_id + "'									";
                            string sql = " UPDATE    SUPPLIERS_TO_PRICES_DATES_ROOMS SET	" +
                             "	rooms_ordered = '" + check_room_tmp + "'		" +
                             " WHERE id = '" + curr_day_id + "' ";
                            DAL_SQL.RunSqlbool(sql);
							
							
							logMessage += ("after update allocations, " + date_tmp.ToString("dd-MMM-yyyy") + ":" + GetRecord("SUPPLIERS_TO_PRICES_DATES_ROOMS", "rooms_ordered", "id=" + curr_day_id));
							
							Logger.Log(logMessage);
                        }

                        date_tmp = date_tmp.AddDays(1);
                    }//for
                    xml.Append("<OK>" + rooms_released + " - Rooms released.</OK>");
                }
                
                xml.Append("</ROOT>");
            }
            else // new allocation (must by room types and amount of them)
            {
                string SuppRoomTypeGroupID = (string)GetRecord("PR_HOTEL_PRICES", "supplier_sub_price_id", "id=" + PriceID);
                int TotalDays = (todate - fromdate).Days;
                DateTime date_tmp = fromdate;

                string err = Check_monthly_assignment_num(SupplierPriceID, fromdate, todate, Convert.ToInt32(OrderRooms));

                xml.Append("<ROOT>");


                if (MakeOrder == "2")// need to release the rooms
                {
                    int rooms_released;
                    string curr_day_id;
                    rooms_released = 0;
                    for (int i = 0; i < TotalDays; i++)
                    {// releasing/canceling the order for day rooms amount (total)
                        //getting the current day id
                        curr_day_id = GetStringRecord("SUPPLIERS_TO_PRICES_DATES_ROOMS", "id", "(CAST(room_date AS smalldatetime) = CAST('" + date_tmp.ToString("dd-MMM-yyyy") + "' AS smalldatetime)) AND price_date_id=" + SupplierPriceID);

                        if (curr_day_id != null && curr_day_id != "" && curr_day_id != "0")
                        {
                            rooms_released = rooms_released + 1;
                            string sql = " UPDATE    SUPPLIERS_TO_PRICES_DATES_ROOMS SET						" +
                                    "	rooms_ordered = (SELECT		(rooms_ordered - " + OrderRooms + ")	" +
                                    "					FROM		SUPPLIERS_TO_PRICES_DATES_ROOMS		" +
                                    "					WHERE		id = '" + curr_day_id + "')			" +
                                    " WHERE id = '" + curr_day_id + "'									";
                            DAL_SQL.RunSqlbool(sql);
                        }

                        // releasing/canceling the ordered rooms by room group type
                        string curr_room_group_day_id = GetStringRecord("SUPPLIERS_TO_PRICES_DATES_ROOMS_DETAILS", "id", "day_id='" + curr_day_id + "' AND room_group_id" + SuppRoomTypeGroupID);

                        // check if record exist
                        if (curr_room_group_day_id != null && curr_room_group_day_id != "" && curr_room_group_day_id != "0")
                        {
                            string sql = " UPDATE    SUPPLIERS_TO_PRICES_DATES_ROOMS_DETAILS SET	" +
                                    "	rooms_ordered = (SELECT		(rooms_ordered - " + OrderRooms + ")	" +
                                    "					FROM		SUPPLIERS_TO_PRICES_DATES_ROOMS_DETAILS	" +
                                    "					WHERE		id = '" + curr_room_group_day_id + "')	" +
                                    " WHERE id = '" + curr_room_group_day_id + "' ";
                            DAL_SQL.RunSqlbool(sql);
                        }

                        date_tmp = date_tmp.AddDays(1);
                    }//for
                    xml.Append("<OK>" + rooms_released + " - Rooms released.</OK>");
                }
                
                xml.Append("</ROOT>");
            }

			
            return xml.ToString();
        }
        catch (Exception e)
        {
			Logger.Log("Exception in update allocations. logMessage = " + logMessage + ", Message = " + e.Message + ", " + e.StackTrace);
            return e.Message;// +"*" + e.StackTrace;
        }
    }

    private static string Check_monthly_assignment_num(string SupplierPriceID, DateTime FromDate, DateTime ToDate, int OrderRooms)
    {
        string monthlyAssignmentNum;
        StringBuilder xml = new StringBuilder();
        int TotalDays = (ToDate - FromDate).Days;
        DateTime date_tmp = FromDate;
        bool crossmonth = false;
        int year = FromDate.Year;
        if (!DateTime.Equals(FromDate.Month, ToDate.Month)) //cross month ,check for second month
        {
            crossmonth = true;

            int ToDateDays = ToDate.Day;//days at second month , use only at cross month
            TotalDays = TotalDays - ToDateDays;// only days at first month
        }

        int currMonth = FromDate.Month;

        monthlyAssignmentNum = GetStringRecord("SUPPLIERS_TO_PRICES_DATES_ROOMS", "top 1 monthly_assignment_num", "monthly_assignment_num is not null and monthly_assignment_num <>0 AND price_date_id=" + SupplierPriceID + " and Datepart(month,cast(room_date as smalldatetime))=" + currMonth);

        if (monthlyAssignmentNum == "0" || monthlyAssignmentNum == "")
        {
            if (!crossmonth)// check other month
                return "";//exit
        }
        else
        {
            int sumMonthOrdered = Convert.ToInt32(GetStringRecord("SUPPLIERS_TO_PRICES_DATES_ROOMS", "SUM(rooms_ordered)", "price_date_id= " + SupplierPriceID + " and Datepart(month,cast(room_date as smalldatetime))=" + currMonth + " and Datepart(year,cast(room_date as smalldatetime))=" + year));


            int currRoomandDays = TotalDays * OrderRooms;
            sumMonthOrdered += currRoomandDays;

            if (sumMonthOrdered > Convert.ToInt32(monthlyAssignmentNum))
            {
                xml.Append("<ROOT><FATAL_ERROR>המכסה החודשית למלון זה הסתיימה - נא לפנות למנהל.</FATAL_ERROR></ROOT>");//Maximum number of monthly alocations
                return xml.ToString();
            }
        }

        if (crossmonth)//cross month ,check for second month
        {
            currMonth = ToDate.Month;
            year = ToDate.Year;
            monthlyAssignmentNum = GetStringRecord("SUPPLIERS_TO_PRICES_DATES_ROOMS", "top 1 monthly_assignment_num", "monthly_assignment_num is not null and monthly_assignment_num <>0 AND price_date_id=" + SupplierPriceID + " and Datepart(month,cast(room_date as smalldatetime))=" + currMonth);

            if (monthlyAssignmentNum == "0" || monthlyAssignmentNum == "")
                return "";//exit

            int sumMonthOrdered = Convert.ToInt32(GetStringRecord("SUPPLIERS_TO_PRICES_DATES_ROOMS", "SUM(rooms_ordered)", "price_date_id= " + SupplierPriceID + " and Datepart(month,cast(room_date as smalldatetime))=" + currMonth + " and Datepart(year,cast(room_date as smalldatetime))=" + year));
            int currRoomandDays = TotalDays * OrderRooms;//days at second month
            sumMonthOrdered += currRoomandDays;

            if (sumMonthOrdered > Convert.ToInt32(monthlyAssignmentNum))
            {
                xml.Append("<ROOT><FATAL_ERROR>המכסה החודשית למלון זה הסתיימה - נא לפנות למנהל.</FATAL_ERROR></ROOT>");
                return xml.ToString();
            }
        }
        return "";

    }

    public static string GetStringRecord(string table, string field, string where)
    {
        DataSet ds = new DataSet();

        if (string.IsNullOrEmpty(where)) where = "1=1";

        string SQL = " SELECT " + field + " FROM " + table + "  where  " + where;
        ds = DAL_SQL.ExecuteDataset(DAL_SQL.ConnStr, CommandType.Text, SQL, null);


        try // to return value
        {
            return ds.Tables[0].Rows[0].ItemArray[0].ToString();
        }
        catch (Exception) // no value
        {
            return "";
        }
    }


    public static object GetRecord(string table, string field, string where)
    {
        DataSet ds = new DataSet();

        if (string.IsNullOrEmpty(where)) where = "1=1";

        string SQL = " SELECT " + field + " FROM " + table + "  where  " + where;
        ds = DAL_SQL.ExecuteDataset(DAL_SQL.ConnStr, CommandType.Text, SQL, null);

        if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
        {
            return ds.Tables[0].Rows[0][field];
        }
        else return null;
    }
	
	public static bool shortOrder(string iDocketId, int iShortedDays, int iIndication, int iGovOrderId)
    {
        string bundleId = DAL_SQL.GetRecord("BUNDLES", "id", "gov_order_id", iGovOrderId.ToString());
		Logger.Log("xx - 1");
        string query = getInsertToBundleHistoryQuery(bundleId);
        bool isSucceded = true;

        try
        {
			Logger.Log("xx - 2");
            isSucceded = changeBundleDatesAndPrices(bundleId, iShortedDays);
			Logger.Log("xx - 3");
        }
        catch (Exception ex)
        {
            isSucceded = false;
            Logger.Log(ex.Message + ". stackTrace: " + ex.StackTrace);
        }

        if (isSucceded)
        {
            try
            {
                DAL_SQL.RunSql("UPDATE BUNDLES SET gov_indicator_id = " + iIndication + " WHERE id = " + bundleId);
            }
            catch (Exception ex)
            {
                Logger.Log("Failed to update indication in bundle = " + bundleId + ", indication to update = " + iIndication + ". exception = " + ex.Message);
                isSucceded = false;
            }
        }

        return isSucceded;
    }

    private static bool changeBundleDatesAndPrices(string iBundleId, int iShortedDays) //iOrderDaysAfterShort = days that short
    {
        //string voucherId = DAL_SQL.GetRecord("VOUCHERS", "id", " service_type_id=2 and docket_id", iDocketId);
        //string bundleId = getBundleIdOfHotelByDocketId(iDocketId);
        bool isSucceeded = true;
Logger.Log("xx - 3");
        DateTime fromDate, toDateNew, toDateOrigin;

		string fromDateStr = DAL_SQL.GetRecord("BUNDLES", "from_date", "id", iBundleId);
		string toDateStr =  DAL_SQL.GetRecord("BUNDLES", "to_date", "id", iBundleId);
		
		Logger.Log("bundleId = " + iBundleId);
		Logger.Log("fromDateStr = " + fromDateStr + ".");
		Logger.Log("toDateStr = " + toDateStr + ".");
		
		fromDate = DateTime.Parse(fromDateStr);
		toDateOrigin = DateTime.Parse(toDateStr);
        
Logger.Log("xx - 5");
        
		//Update to date in tables: BUNDLES, BUNDLES_to_TRAVELLERS
        toDateNew = toDateOrigin.AddDays(-1 * iShortedDays);

		int serviceType = 2;
        isSucceeded = updateToDateInHotelsAndBundlesAndBundlesToTravellers(toDateNew, iBundleId, serviceType);
        if (isSucceeded)
        {
            //Update hotel prices
            Logger.Log("Before update hotel prices");			
            isSucceeded = updateBundlesHotelPrices(iBundleId, iShortedDays);
            if (isSucceeded)
            {
                Logger.Log("Done update hotel prices");
                
                //Update other prices
                Logger.Log("Before update other prices");
				
				string docketId = DAL_SQL.GetRecord("BUNDLES", "docket_id", "id", iBundleId);
				string otherBundleId = DAL_SQL.RunSql("SELECT bundle_id FROM VOUCHERS WHERE status = 1 and service_type_id = 8 and docket_id = " + docketId);
				
				if (!string.IsNullOrEmpty(otherBundleId))
				{
					Logger.Log("otherBundleId = " + otherBundleId);
					int orderDaysAfterShort = (toDateNew - fromDate).Days;
					string querySetHotelNight = "UPDATE HOTELS SET nights = " + orderDaysAfterShort + " WHERE bundle_id = " + iBundleId;
					DAL_SQL.RunSql(querySetHotelNight);
					isSucceeded = updateBundleOtherPrices(otherBundleId, orderDaysAfterShort);
				}
				
                if (isSucceeded)
                {
                    Logger.Log("Done update other prices");
					Logger.Log("Before update allocations");
                    isSucceeded = updateAllocationsAccordingToOldAndNewToDates(iBundleId, fromDate, toDateNew, toDateOrigin);
					
                    if (!isSucceeded)
                    {
                        Logger.Log("failed in updateAllocationsAccordingToOldAndNewToDates.  failed to update allocations. bundleId = " + iBundleId);
                        throw new Exception("failed to update allocations. bundleId = " + iBundleId);
                    }
					Logger.Log("Done update allocations");
					//TODO
                    updateRemarksInBundle(iBundleId);
                }
				else				
				{
					Logger.Log("Failed in updateBundleOtherPrices");
				}
            }
			else
			{
				Logger.Log("Failed in updateBundlesHotelPrices");
			}
        }

        return isSucceeded;
    }

    private static void updateRemarksInBundle(string iBundleId)
    {
        string remarks = DAL_SQL.GetRecord("BUNDLES", "remark", "id", iBundleId);

        remarks += ". (הזמנה זו עברה קיצור)";

        DAL_SQL.RunSql("UPDATE BUNDLES SET remark = N'" + remarks + "' WHERE id = " + iBundleId);
    }

    private static bool updateAllocationsAccordingToOldAndNewToDates(string iBundleId, DateTime iFromDate, DateTime iToDateNew, DateTime iToDateOrigin)
    {
        string priceId = DAL_SQL.GetRecord("HOTELS", "hotel_price_id", "bundle_id", iBundleId);
        string supplierPriceId = DAL_SQL.GetRecord("HOTELS", "supplier_price_id", "bundle_id", iBundleId);
        string roomsAmount = DAL_SQL.GetRecord("HOTELS", "rooms", "bundle_id", iBundleId);
        int releaseRooms = 2;
        int orderRooms = 1;
        bool isSucceeded = true;
		
		Logger.Log("Release fromDate = " + iToDateNew.ToString("dd-MMM-yy") + ", toDate = " + iToDateOrigin.ToString("dd-MMM-yy"));
		Logger.Log("priceId = " + priceId + ", supplierPriceId = " + supplierPriceId + ", roomsAmount = " + roomsAmount);
		
        try
        {
            //Release the allocation from origin order.
            isSucceeded = hotel_rooms_check_ws.checkRoomsAmount(int.Parse(priceId), int.Parse(supplierPriceId), int.Parse(roomsAmount), iToDateNew, iToDateOrigin, releaseRooms);

            if (!isSucceeded)
            {
                //If failed to return the allocations than alert the client to call us.
                return false;
            }
        }
        catch (Exception ex)
        {
            Logger.Log("Failed when try to return allocations, bundleId = " + iBundleId + ". exception = " + ex.Message);
        }

        return isSucceeded;
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
	
    private static bool updateToDateInHotelsAndBundlesAndBundlesToTravellersOld(DateTime toDate, string bundleId)
    {
        string toDateStr = string.Empty;
        bool isSucceded = true;

        toDateStr = toDate.ToString("dd-MMM-yy");

		Logger.Log("xx - toDateStr = " + toDateStr);
		
        string queryUpdateBundles = "UPDATE BUNDLES SET to_date = '" + toDateStr + "' WHERE id = " + bundleId;
        string queryUpdateBundlesToTravellers = "UPDATE BUNDLES_to_TRAVELLERS SET to_date = '" + toDateStr + "' WHERE bundle_id = " + bundleId;
        string queryUpdateHotels = "UPDATE HOTELS SET exit_date = '" + toDateStr + "' WHERE bundle_id = " + bundleId;

        try
        {
            DAL_SQL.RunSql(queryUpdateBundles);

            try
            {
                DAL_SQL.RunSql(queryUpdateBundlesToTravellers);

                try
                {
                    DAL_SQL.RunSql(queryUpdateHotels);
                }
                catch (Exception e)
                {
                    Logger.Log("Failed to update toDate in HOTELS. exception = " + e.Message);
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

    private static bool updateBundlesHotelPrices(string iBundleId, int iOrderDaysAfterShort)
    {
        string bundleId = iBundleId;
        string queryUpdateBundles = string.Empty;
        string queryUpdateBundlesToTravellers = string.Empty;
        string queryUpdateHotels = string.Empty;
        string priceId = string.Empty;
        string erkevType = string.Empty;

        double amountBrutto = 0;
        double amountNetto = 0;
        double amountBruttoTemp = 0;
        double amountNettoTemp = 0;

        DateTime fromDate, toDate;

		fromDate = DateTime.Parse(DAL_SQL.GetRecord("BUNDLES", "from_date", "id", bundleId));
		
        //Update to date in tables: BUNDLES, BUNDLES_to_TRAVELLERS
        toDate = DateTime.Parse(DAL_SQL.GetRecord("BUNDLES", "to_date", "id", bundleId));//to_date has already changed to the new date.

        //Get prices to change
        erkevType = DAL_SQL.GetRecord("BUNDLES", "erkev_type", "id", bundleId);
        priceId = DAL_SQL.GetRecord("HOTELS", "hotel_price_id", "bundle_id", bundleId);
		List<int> hotelRoomType = getHotelRoomsTypes(erkevType);
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
		
        string incomeType = DAL_SQL.GetRecord("BUNDLES", "income_type", "id", bundleId);
        string vatPercent = DAL_SQL.GetRecord("BUNDLES", "vat_percent", "id", bundleId);
        BundleRow brHotel = null;

        try
        {
			Logger.Log("got here 1");
            brHotel = new BundleRow(amountBrutto, amountNetto, incomeType, vatPercent);
			Logger.Log("got here 2");
        }
        catch (Exception ex)
        {
            Logger.Log("Failed to crate BundleRow object. exception = " + ex.Message);
            return false;
        }

        //Update prices in bundle and bundle_to_travellers
		Logger.Log("got here 3");
        bool isUpdateHotelSucceded = updatePrices(brHotel, bundleId);
		Logger.Log("got here 4");
        
        return true;
    }

	private static bool updateBundleOtherPrices(string iBundleId, int iOrderDaysAfterShort)
    {
        bool hasAttraction = false;
        double amountBrutto = 0;
        double amountNetto = 0;
		
		if (!string.IsNullOrEmpty(iBundleId))
		{
			Logger.Log("updateBundleOtherPrices 1");
			DataSet dsAttraction = DAL_SQL.RunSqlDataSet("SELECT * FROM VOUCHERS WHERE status = 1 and bundle_id = " + iBundleId);
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
				return false;
			}

			Logger.Log("updateBundleOtherPrices 12");
			return true;
		}
		else
		{
			return false;
		}
    }
	
    private static bool updatePrices(BundleRow iBundleRow, string bundleId)
    {
        bool isSucceded = true;

        string updateBundlesQuery = "UPDATE BUNDLES SET " + iBundleRow.getBUNDLESUpdateString() + " WHERE id = " + bundleId;
        string updateBundlesToTravellersQuery = "UPDATE BUNDLES_to_TRAVELLERS SET " + iBundleRow.getBUNDLES_to_TRAVELLERSUpdateString() + " WHERE subsid <> 0 and bundle_id = " + bundleId;

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
                hotelRoomsTypeId.Add(single);
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

    private static bool updateIndicationInBundleRow(string iBundleId, int iIndication)
    {
        bool isSucceded = true;

        if (!string.IsNullOrEmpty(iBundleId))
        {
            DAL_SQL.RunSql("UPDATE BUNDLES SET gov_indicator_id = " + iIndication + " WHERE id = " + iBundleId);
        }
        else
        {
            isSucceded = false; 
        }

        return isSucceded;
    }
}