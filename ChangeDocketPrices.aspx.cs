using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class ChangeDocketPrices : System.Web.UI.Page
{
    public bool isShowClientMessage { get; set; }


    private const string nettoHeadline = "נטו";
    private const string bruttoHeadline = "ברוטו";
    private string errorMsg = string.Empty;
    private string dateFormat = "dd-MMM-yy";
    private string getBundleDetailsSql = string.Empty;
    private double mAmountNetto = 0;
    private double mAmountBrutto = 0;
    private double vat;
    private DataSet mBundles = null;
    private string filteHotelId = string.Empty;
    private string filterFromDate = string.Empty;
    private string filterToDate = string.Empty;
    private List<BundleRow> mBundlesList = new List<BundleRow>();
    private string getPriceDetailsSql = string.Empty;
    private DataSet requstedPriceDetails = null;
    BundleRow mBundleRow = null;

    string mPriceId = string.Empty;

    protected void Page_Load(object sender, EventArgs e)
    {
		//Added to not let anyone do anything here.
		//Response.Redirect("./AccessDenied.aspx", false);
		
        isShowClientMessage = true;

        //Chen. delete when production!!!!@@!
        setLocalHost();

        if (!IsPostBack)
        {
            loadHotels();
        }
    }

	private DataSet getDataSet()
	{
		return new DataSet();
	}

    private void loadHotels()
    {
        //Hotels ddl
        DataSet ds1 = DAL_SQL_Helper.GetHotels();
        ds1.Tables[0].Columns.Add("NameAndArea", typeof(string), "name +' - '+description");
        DataRow AllRow1 = ds1.Tables[0].NewRow();
        AllRow1[0] = 0;
        AllRow1[1] = "הכל";
        AllRow1[2] = "הכל";
        AllRow1[3] = "";
        ds1.Tables[0].Rows.InsertAt(AllRow1, 0);
        ddlHotels.DataSource = ds1.Tables[0];
        ddlHotels.DataBind();
    }

    private void setBundleNewPrice(BundleRow iBundleRow, string iServiceType)
    {
        switch(iServiceType)
        {
            case "2":
                setHotelBundleNewPrice(iBundleRow);
                break;

            case "8":
                setOtherBundleNewPrice(iBundleRow);
                break;
        }
    }

    private void setHotelBundleNewPrice(BundleRow iBundleRow)
    {
        string supplierId = string.Empty;
        string fromDate = string.Empty;
        string toDate = string.Empty;
        int fromDay = 0, toDay = 0;
        int nights = 0;

        PriceDetails priceDetails = new PriceDetails();

        string getHotelPricesSql = string.Empty;

        mPriceId = string.Empty;
        supplierId = string.Empty;
        fromDate = string.Empty;
        toDate = string.Empty;
        fromDay = 0;
        toDay = 0;
        nights = 0;

        //Save the current bundle id.
        fromDate = iBundleRow.mFromDate.ToString(dateFormat);
        toDate = iBundleRow.mToDate.ToString(dateFormat);
        supplierId = iBundleRow.mPayToSupplier.ToString();
        nights = (DateTime.Parse(toDate) - DateTime.Parse(fromDate)).Days;

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
											
											OR    
        
                                            {3} >= min_nights_mid
                                            AND {3} >= min_nights_end 
                                            AND cast('{1}' as smalldatetime) >= from_date 
											AND cast('{2}' as smalldatetime) <= to_date 
											AND status = 1
                                            ) 
                                
                            drop table #temp",
      supplierId, fromDate, toDate, nights);

        //Set the priceId,  fromDay, toDay, nights fro the currnet bundle.
        getPriceIdFordBundle(getHotelPricesSql, iBundleRow.mId.ToString(), supplierId, fromDate, toDate, ref mPriceId, ref fromDay, ref toDay);

        Session["PriceId"] = mPriceId;

        if (!string.IsNullOrEmpty(mPriceId))
        {
            getPriceDetailsSql = @"SELECT * 
									FROM PR_HOTEL_PRICES_DETAILS
									WHERE price_id = " + mPriceId;

            //Create a priceDetails.
            priceDetails = new PriceDetails(mPriceId, supplierId, fromDate, toDate, fromDay, toDay, nights, vat);

            setAmountsByPriceDetails(priceDetails, iBundleRow);

            // ********************** //
            // End of bundle handling //
            // ********************** //


            //Clear params before changing the next price.
            clearParams();
        }
        else
        {
            mPriceId = "";
            Session["PriceId"] = string.Empty;
        }
    }

    private void setOtherBundleNewPrice(BundleRow iBundleRow)
    {
        string supplierId = string.Empty;
        DateTime fromDate;
        DateTime toDate;
        int nights = 0;
        string addId = string.Empty;
        double amountBrutto;
        double amountNetto;

        addId = getAddId(iBundleRow);

        if (!string.IsNullOrEmpty(addId))
        {
            fromDate = iBundleRow.mFromDate;
            toDate = iBundleRow.mToDate;
            supplierId = iBundleRow.mPayToSupplier.ToString();
            nights = (toDate - fromDate).Days;

            amountBrutto = DAL_SQL_Helper.GetBrutoPriceForAttraction(iBundleRow.mId, supplierId, addId, nights.ToString(), fromDate, toDate);
            amountNetto = DAL_SQL_Helper.GetNettoPriceForAttraction(iBundleRow.mId, supplierId, addId, nights.ToString(), fromDate, toDate);

            //Setting the new details.
            iBundleRow.setPrices(amountBrutto, amountNetto);

            //If 4+1. and trav add one treatmen which he pays for it. (not adding 1 night)
            if (iBundleRow.mFourOneSeven.Contains("night") && nights == 4 || iBundleRow.mGovIndicator == 5 && nights == 4)
            {
                amountBrutto = DAL_SQL_Helper.GetBrutoPriceForAttraction(iBundleRow.mId, supplierId, addId, nights.ToString(), fromDate, toDate.AddDays(1));
                amountNetto = DAL_SQL_Helper.GetNettoPriceForAttraction(iBundleRow.mId, supplierId, addId, nights.ToString(), fromDate, toDate.AddDays(1));

                //Setting the new details.
                iBundleRow.setPrices(amountBrutto, amountNetto);

                //sibsud only for 4 days.
                double newSubsid = DAL_SQL_Helper.GetBrutoPriceForAttraction(iBundleRow.mId, supplierId, addId, nights.ToString(), fromDate, toDate);
                
                iBundleRow.setSubsid(newSubsid);
            }
        }
        else
        {
            Session["failedToChangeDetails"] = "Failed to get addId.";
        }
    }

    private void setAmountsByPriceDetails(PriceDetails iPriceDetails, BundleRow iBundleRow)
    {// iPriceDetails needs to be fill with (priceId, supplierId, fromDate, toDate, fromDay, toDay, nights, vat).
        List<int> hotelRoomType = null;

        if (!string.IsNullOrEmpty(iPriceDetails.PriceId))
        {
            requstedPriceDetails = DAL_SQL.RunSqlDataSet(getPriceDetailsSql);

            //Get the ordered baseType (hotel_room_type_id) 
            hotelRoomType = getHotelRoomsTypes(iBundleRow.mErkevType);

            //If fails to get prices will return False.
            if (!setAmountNettoAndBrutto(requstedPriceDetails, hotelRoomType, iPriceDetails))
            {
                handleError("No matches for requested price. priceId = " + iPriceDetails.PriceId, eLogger.PRICE_NOT_FOUND);
                Session["failedToChangeDetails"] = "No matches for requested price. priceId = " + iPriceDetails.PriceId;
                Logger.EmptyLog(mBundleRow.mDocketId, eLogger.PRICE_NOT_FOUND);
            }
            else
            {
                //Set the new prices.
                iBundleRow.setPrices(mAmountBrutto, mAmountNetto);
            }
        }
    }

    private void setBundlesList()
    {
        getBundleDetailsSql = getBundlesSql();

        if (ddlServiceType.SelectedValue == "2")
        {
            Logger.Log("Setting only hotels.", eLogger.DEBUG);
        }
        else if (ddlServiceType.SelectedValue == "8")
        {
            Logger.Log("Setting only others.", eLogger.DEBUG);
        }

        setBundlesDataSet(getBundleDetailsSql);

        Logger.Log("Set bundleList: Found " + mBundles.Tables[0].Rows.Count + " bundles rows.", eLogger.DEBUG);

        foreach (DataRow bundleRow in mBundles.Tables[0].Rows)
        {
            try
            {
                mBundlesList.Add(new BundleRow(bundleRow));
            }
            catch (Exception ex)
            {
                Logger.Log(ex.Message, eLogger.EXCEPTION);
            }
        }



    }

    private string getBundlesSql()
    {
        int count = 0;

        string sql = string.Format(@"
               SELECT *  
               FROM BUNDLES AS B
               WHERE from_date >= cast('01-Apr-17' as smalldatetime)
                AND B.id IN (SELECT V.bundle_id FROM VOUCHERS AS V
			   WHERE V.status = 1 AND V.service_type_id = {0})", ddlServiceType.SelectedValue);

        if (ddlServiceType.SelectedValue == "2")
        {
            sql += " AND gov_order_id <> 0 ";
        }

        if (!string.IsNullOrEmpty(ddlHotels.SelectedValue))
        {
            if (ddlHotels.SelectedValue != "0")
            {
                count++;
                filteHotelId = ddlHotels.SelectedValue;
                sql += (" AND carrier = " + filteHotelId);
            }
        }

        if (!string.IsNullOrEmpty(txtFromDocket.Text))
        {
            count++;
            sql += (" AND " + txtFromDocket.Text + " <= docket_id ");
        }

        if (!string.IsNullOrEmpty(txtToDocket.Text))
        {
            count++;
            sql += (" AND " + txtToDocket.Text + " >= docket_id ");
        }

        if (!string.IsNullOrEmpty(txtFromDate.Text) && !string.IsNullOrEmpty(txtToDate.Text))
        {
            count++;
            filterFromDate = getDateByFormat(txtFromDate.Text);
            filterToDate = getDateByFormat(txtToDate.Text);
            sql += (" AND from_date >= cast('" + filterFromDate + "' as smalldatetime) AND from_date <= cast('" + filterToDate + "' as smalldatetime) ");
        }

        if (count == 0)
        {
            sql += "AND 1 = 2";
            isShowClientMessage = true;
            sendMessageToClient("אנא בחר מהאופציות, אתה עומד לשנות את כל התיקים שעומדים בתנאים הנ''ל",eLogger.DEBUG);
            isShowClientMessage = false;
        }

        return sql;
    }

    private double getVatByBundle(string iBundleId)
    {
        double vat = 0;
        try
        {
            DataSet ds = DAL_SQL_Helper.getVatFromBundle(iBundleId);

            if (isDataSetRowsNotEmpty(ds))
            {
                vat = double.Parse(ds.Tables[0].Rows[0]["vat_percent"].ToString());
            }
            else
            {
                handleError("Failed to get vat from bundle row (bundleId = " + iBundleId, eLogger.EXCEPTION);
            }
        }
        catch (Exception ex)
        {
            handleError("Failed to get vat from bundle. Exception = " + ex.Message, eLogger.EXCEPTION);
        }

        return vat;
    }

    private void clearParams()
    {
        //equate to zero the amounts.
        mAmountNetto = 0;
        mAmountBrutto = 0;

        vat = 0;
    }

    private void setBundlesDataSet(string iGetBundleDetailsSql)
    {
        try
        {
            //all bundle rows
            mBundles = DAL_SQL.RunSqlDataSet(getBundleDetailsSql);
        }
        catch (Exception ex)
        {
            handleError("Failed to get bundles details. sql = " + getBundleDetailsSql + ", Exception = " + ex.Message, eLogger.EXCEPTION);
        }
    }

    private void getPriceIdFordBundle(string iGetHotelPricesSql, string iBundleId, string iSupplierId, string iFromDate, string iToDate,
                                      ref string iPriceId, ref int iFromDay, ref int iToDay)
    {
        DataSet pricesIdsForSelectedBundle = null;

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
                        iPriceId = priceRow["id"].ToString();
                        iFromDay = getDayFromDate(DateTime.Parse(iFromDate));
                        iToDay = getDayFromDate(DateTime.Parse(iToDate)); //.AddDays(-1) . 
                    }
                }
                else
                {
                    Session["MultiPriceFound"] = "true";
                    handleError("More than 1 price match to this dates. supplierId = " + iSupplierId + ", fromDate = " + iFromDate + ", toDate = " + iToDate + ",bundleId = " + iBundleId + ",docket_id = " + mBundleRow.mDocketId + ", query = " + iGetHotelPricesSql, eLogger.MULTI_PRICES);
                    Session["failedToChangeDetails"] = "More than 1 price match to this dates.";
                    Logger.EmptyLog(mBundleRow.mDocketId, eLogger.MULTI_PRICES);
                }
            }
            else
            {
                handleError("No matches for requested price. supplierId = " + iSupplierId + ", fromDate = " + iFromDate + ", toDate = " + iToDate + ",bundleId = " + iBundleId + ",docket_id = " + mBundleRow.mDocketId + ", query = " + iGetHotelPricesSql, eLogger.PRICE_NOT_FOUND);
                Session["failedToChangeDetails"] = "No matches for requested price.";
                Logger.EmptyLog(mBundleRow.mDocketId, eLogger.PRICE_NOT_FOUND);
            }
        }
        catch (Exception ex)
        {
            handleError("Failed to get prices details. bundleId = " + iBundleId + ",docket_id = " + mBundleRow.mDocketId + ", exception = " + ex.Message, eLogger.EXCEPTION);
            Session["failedToChangeDetails"] = "Failed to get prices details.";
            Logger.EmptyLog(mBundleRow.mDocketId, eLogger.EXCEPTION);
        }
    }

    private bool setAmountNettoAndBrutto(DataSet iRequstedPriceDetails, List<int> iHotelRoomType, PriceDetails iPriceDetail)
    {
        //Logger.Log("In setAmountNettoAndBrutto", eLogger.DEBUG);

        bool gotNettoAndBrutto = false;

        //Check that the dataSet is not empty.
        if (isDataSetRowsNotEmpty(iRequstedPriceDetails))
        {
            foreach (int roomType in iHotelRoomType)
            {
                //Handle errors in func also.
                setRoomTypeNettoBrutto(iRequstedPriceDetails, roomType, iPriceDetail);

                //Means that we didnt get any pricefor neto or brutto.
                if (mAmountBrutto == 0 || mAmountNetto == 0)
                {
                    handleError(string.Format(@"Failed to get prices for priceId = {0} roomTypeId = {1}", iPriceDetail.PriceId, roomType), eLogger.DEBUG);
                }

                gotNettoAndBrutto = true;
            }
        }

        return gotNettoAndBrutto;
    }

    private void setRoomTypeNettoBrutto(DataSet iRequstedPriceDetails, int iRoomType, PriceDetails iPriceDetail)
    {
        int fromDay = iPriceDetail.FromDay;
        int toDay = iPriceDetail.ToDay;
        int nights = iPriceDetail.Nights;
        double brutto = 0;
        double netto = 0;

        //Logger.Log("In setRoomTypeNettoBrutto", eLogger.DEBUG);
        int countNights = 0;

        foreach (DataRow priceRow in iRequstedPriceDetails.Tables[0].Rows)
        {
            if (int.Parse(priceRow["hotel_room_type_id"].ToString()) == iRoomType)
            {
                while (fromDay != toDay || nights != countNights)
                {
                    brutto = double.Parse(priceRow["night_bruto_" + fromDay].ToString());
                    netto = double.Parse(priceRow["night_netto_" + fromDay].ToString());
                    mAmountBrutto += brutto;
                    mAmountNetto += netto;

                    fromDay++;
                    if (fromDay == 8)// (8) means its sunday.
                        fromDay = 1;
                    countNights++;
                }

                break;
            }
        }
    }   

    private List<int> getHotelRoomsTypes(string iBaseTypeGOV)
    {
        //How many travellers GOV pays for.
        const int single = 1;
        const int couple = 2;
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
                handleError("No 'erkev_type' found", eLogger.DEBUG);
                break;
        }

        return hotelRoomsTypeId;
    }    

    //User interface
    protected void btGetNextBundle_Click(object sender, EventArgs e)
    {
        string updateQuery = string.Empty;

        Session["MultiPriceFound"] = null;
        if (Session["bundleToChangeCount"] != null)
        {
            Session["bundleToChangeCount"] = (int.Parse(Session["bundleToChangeCount"].ToString()) + 1).ToString();
        }

        List<BundleRow> bundleList = (List<BundleRow>)Session["BundleList"];

        BundleRow beforeChangeBundleRow = null;

        if (bundleList != null && bundleList.Count > 0)
        {
            if (int.Parse(Session["bundleToChangeCount"].ToString()) < bundleList.Count)
            {
                mBundleRow = bundleList[int.Parse(Session["bundleToChangeCount"].ToString())];

                lbDocketId.Text = mBundleRow.mDocketId;
                beforeChangeBundleRow = new BundleRow(mBundleRow);

                if (!isBundleWithExtraPay(beforeChangeBundleRow.mId))
                {
                    setBundleNewPrice(mBundleRow, ddlServiceType.SelectedValue);

                    setScreenDetails(beforeChangeBundleRow, mBundleRow);

                    btChangeBundleDetails.Visible = true;

                    getPriceDetails();
                }
                else
                {
                    Session["failedToChangeDetails"] = "תיק זה עם תוספת תשלום אנא בחלף ידנית בתיק.";
                    Session["isBundleWithExtraPay"] = "1";
                    sendMessageToClient("תיק זה עם תוספת תשלום אנא בחלף ידנית בתיק.", eLogger.CHANGED_DETAILS);
                    btChangeBundleDetails.Visible = false;
                    Logger.EmptyLog(mBundleRow.mDocketId, eLogger.EXTRA_PAY);
                }
            }
            else
            {
                sendMessageToClient("אין עוד תיקים לשנות.", eLogger.DEBUG);
            }
        }
        else
        {
            sendMessageToClient("אין עוד תיקים לשנות.", eLogger.DEBUG);
        }
    }

    private bool isBundleWithExtraPay(int iBundleId)
    {
        bool isBundleWithExtraPay = false;
Logger.Log("chen2", eLogger.EXCEPTION);
        string sql = @"SELECT id FROM BUNDLES WHERE id in
                       (
                       SELECT BUNDLES_to_TRAVELLERS.bundle_id
                       FROM BUNDLES_to_TRAVELLERS 
                       WHERE amount <> 0
                       group by bundle_id
                       having COUNT(*) > 1
                       )";

        DataSet ds = DAL_SQL.RunSqlDataSet(sql);
        string bundleId = iBundleId.ToString();
Logger.Log("chen3", eLogger.EXCEPTION);
        if (isDataSetRowsNotEmpty(ds))
        {
            foreach (DataRow row in ds.Tables[0].Rows)
            {
                if (bundleId == row["id"].ToString())
                    isBundleWithExtraPay = true;
            }
        }
Logger.Log("chen4 " + isBundleWithExtraPay + ", sql = " + sql, eLogger.EXCEPTION);
        return isBundleWithExtraPay;
    }

    private void getPriceDetails()
    {
        getPriceDetailsSql = @"SELECT *,  
                            (SELECT HRT.name from Agency_Admin.dbo.HOTEL_ROOM_TYPE AS HRT WHERE HRT.id = PHPD.hotel_room_type_id) AS room_type_name,
                            (SELECT PHP.price_name from PR_HOTEL_PRICES AS PHP WHERE PHP.id = PHPD.price_id) AS price_name 
                            FROM PR_HOTEL_PRICES_DETAILS AS PHPD 
                            WHERE price_id = " + Session["PriceId"].ToString();

        if (!string.IsNullOrEmpty(Session["PriceId"].ToString()))
        {
            requstedPriceDetails = DAL_SQL.RunSqlDataSet(getPriceDetailsSql);
        }

        if (isDataSetRowsNotEmpty(requstedPriceDetails))
        {
            try
            {
                lbPriceName.Text = requstedPriceDetails.Tables[0].Rows[0]["price_name"].ToString().Trim();
                foreach (DataRow priceRow in requstedPriceDetails.Tables[0].Rows)
                {
                    TableRow trNetto = new TableRow();
                    TableRow trBrutto = new TableRow();
                    TableCell cell = new TableCell();

                    cell.Text = priceRow["room_type_name"].ToString().Trim() + " " + nettoHeadline;
                    trNetto.Cells.Add(cell);
                    cell = new TableCell();
                    cell.Text = priceRow["room_type_name"].ToString().Trim() + " " + bruttoHeadline;
                    trBrutto.Cells.Add(cell);
                    cell = new TableCell();
                    for (int day = 1; day <= 7; day++)
                    {
                        cell.Text = priceRow["night_netto_" + day].ToString();
                        trNetto.Cells.Add(cell);
                        cell = new TableCell();
                        cell.Text = priceRow["night_bruto_" + day].ToString();
                        trBrutto.Cells.Add(cell);
                        cell = new TableCell();
                    }

                    tablePriceDetails.Rows.Add(trNetto);
                    tablePriceDetails.Rows.Add(trBrutto);
                }
            }
            catch (Exception ex)
            {
                string msg = "אירעה שגיאה במציאת המחירון";

                clearUIParams();
                btChangeBundleDetails.Visible = false;
                //handleError(msg + "docket_id = " + bundleRow.mDocketId + ", bundle_id = " + bundleRow.mId, eLogger.PRICE_NOT_FOUND);
                sendMessageToClient(msg, eLogger.EXCEPTION);

                Logger.Log("priceId = " + Session["PriceId"].ToString() + ", Exception = " + ex.Message, eLogger.EXCEPTION);
            }
        }
        else
        {
            if (Session["MultiPriceFound"] != null)
            {
                string msg = "מחריון כפול " + mBundleRow.mDocketId;

                lbPriceName.Text = msg;
                clearUIParams();
                btChangeBundleDetails.Visible = false;
                //handleError(msg + "docket_id = " + bundleRow.mDocketId + ", bundle_id = " + bundleRow.mId, eLogger.PRICE_NOT_FOUND);
                sendMessageToClient(msg, eLogger.MULTI_PRICES);
                Session["failedToChangeDetails"] = "מחריון כפול";
                Logger.EmptyLog(mBundleRow.mDocketId, eLogger.MULTI_PRICES);
            }
            else
            {
                string msg = "לא נמצא מחירון מתאים תיק " + mBundleRow.mDocketId;

                lbPriceName.Text = msg;
                clearUIParams();
                btChangeBundleDetails.Visible = false;
                //handleError(msg + "docket_id = " + bundleRow.mDocketId + ", bundle_id = " + bundleRow.mId, eLogger.PRICE_NOT_FOUND);
                sendMessageToClient(msg, eLogger.PRICE_NOT_FOUND);
                Session["failedToChangeDetails"] = "לא נמצא מחירון מתאים תיק";
                Logger.EmptyLog(mBundleRow.mDocketId, eLogger.PRICE_NOT_FOUND);
            }
        }
    }

    private void setScreenDetails(BundleRow before, BundleRow after)
    {
        //Old amounts.
        lbOldBrutto.Text = before.mAmount.ToString();
        lbOldNetto.Text = before.mToSupplier.ToString();
        lbOldSubsid.Text = before.mSubsid.ToString();
        lbOldVatBrutto.Text = before.mVatValue.ToString();
        lbOldCommisionValue.Text = before.mCommisionValue.ToString();
        lbOldCommisionVat.Text = before.mCommisionVat.ToString();
        lbOldToClerk.Text = before.mToClerk.ToString();

        //New amounts.
        lbNewBrutto.Text = after.mAmount.ToString();
        lbNewNetto.Text = after.mToSupplier.ToString();
        lbNewSubsid.Text = after.mSubsid.ToString();
        lbNewVatBrutto.Text = after.mVatValue.ToString();
        lbNewCommisionValue.Text = after.mCommisionValue.ToString();
        lbNewCommisionVat.Text = after.mCommisionVat.ToString();
        if (after.mToClerk < 0)
        {
            lbNewToClerk.Text = after.mToClerk.ToString() + "<b>(שים לב שהרווח במינוס)</b>";
        }
        else
        {
            lbNewToClerk.Text = after.mToClerk.ToString();
        }

		string fullNameHotel = string.Empty;
		string areaId = string.Empty;
		
        fullNameHotel = DAL_SQL.GetRecord("agency_admin.dbo.SUPPLIERS", "name", before.mPayToSupplier, "id");
		areaId = DAL_SQL.GetRecord("agency_admin.dbo.SUPPLIERS", "area_id", before.mPayToSupplier, "id");
		lbHotelName.Text = fullNameHotel + " " + DAL_SQL.GetRecord("agency_admin.dbo.AREAS", "name", areaId, "id");
        lbFromDate.Text = before.mFromDate.ToString("dd/MM/yyyy");
        lbToDate.Text = before.mToDate.ToString("dd/MM/yyyy");
        lbSubsidBase.Text = getErkevText(before.mErkevType.ToString());
    }

    private string getErkevText(string iErkevType)
    {
        string erkevText = string.Empty;

        switch (iErkevType.Trim())
        {
            case "Zakai":
                erkevText = "יחיד";
                break;
            case "ZakaiAndMelave":
                erkevText = "זוג";
                break;
            case "ZakaiAnd2Melavim":
                erkevText = "יחיד + זוג";
                break;
            case "ZakaiAndMelaveBeTashlum":
                erkevText = "יחיד";
                break;
            case "ZakaiAndMelaveBeTashlumHelekTkufa":
                erkevText = "יחיד + חלקי";
                break;
            case "ZakaiBeTashlumAndMelaveLeLoTashlum":
                erkevText = "יחיד";
                break;
            case "ZakaiAndMelaveTmuratZakaut":
                erkevText = "זוג";
                break;

            default:
                erkevText = "אירעה שגיאה בקבלת סוג ההרכב";
                break;
        }

        return erkevText;
    }

    private void setLocalHost()
    {
        AgencyUser user = new AgencyUser();
        
        //Connect local db
        user.AgencyId = "85";
        user.AgencySystemType = "3";
        user.AgencyUserId = "1";
        user.AgencyUserName = "Agency2000";
        user.AgencyUserPassword = "11071964";
      

        DAL_SQL.ConnStr = ConfigurationManager.AppSettings["CurrentAgencyDBConnStr"];
        DAL_SQL.ConnStr = DAL_SQL.ConnStr.Replace("^agencyId^", ((user.AgencyId.Length == 1) ? "000" + user.AgencyId : "00" + user.AgencyId));
        DAL_SQL.ConnStr = DAL_SQL.ConnStr.Replace("^systemType^", ((user.AgencySystemType == "3") ? "INN" : "OUT"));

    }

    public bool changeBundleDetails(BundleRow bundleRow)
    {
        bool isChangeSuccess = false;

        string updateBundlesQuery = string.Empty;
        string updateBundlesToTravellersQuery = string.Empty;
        string updateHOTELSPriceId = string.Empty;

        try
        {
            updateBundlesQuery = @"UPDATE BUNDLES SET " + bundleRow.getBUNDLESUpdateString() + " WHERE id = " + bundleRow.mId;

            //UPDATE BUNDLES table.
            DAL_SQL.RunSql(updateBundlesQuery);
            handleError("bundleId = " + bundleRow.mId + ",BUNDLES was updated succesfully", eLogger.DEBUG);

            try
            {
                updateBundlesToTravellersQuery = @"UPDATE BUNDLES_to_TRAVELLERS SET " + bundleRow.getBUNDLES_to_TRAVELLERSUpdateString() + " WHERE bundle_id = " + bundleRow.mId + " AND subsid > 0";

                //UPDATE BUNDLES_to_TRAVELLERS table.
                DAL_SQL.RunSql(updateBundlesToTravellersQuery);
                handleError("bundleId = " + bundleRow.mId + ",BUNDLES_to_TRAVELLERS was updated succesfully", eLogger.DEBUG);

                if (ddlServiceType.SelectedValue == "2")
                {
                    try
                    {
                        updateHOTELSPriceId = "UPDATE HOTElS SET hotel_price_id = " + Session["PriceId"].ToString() + "WHERE bundle_id = " + bundleRow.mId;

                        DAL_SQL.RunSql(updateHOTELSPriceId);
                        handleError("updated HOTELS bundle_id = " + bundleRow.mId + ", priceId(" + Session["PriceId"].ToString() + ") was set)", eLogger.DEBUG);
                        isChangeSuccess = true;
                        sendMessageToClient("הפרטים שונו בהצלחה", eLogger.DEBUG);
                        //Log for user to see the docket that changed
                        Logger.EmptyLog(bundleRow.mDocketId, eLogger.CHANGED_DETAILS);
                    }
                    catch (Exception exce)
                    {
                        handleError("Failed to update HOTELS bundleId = " + bundleRow.mId + ", query = " + updateHOTELSPriceId + ", exception = " + exce.Message, eLogger.EXCEPTION);
                        sendMessageToClient("(HOTELS) אירעה שגיאה בעת שינוי הפרטים, אנא עדכן את מנהל התוכנה", eLogger.EXCEPTION);
                        Session["failedToChangeDetails"] = "(HOTELS) אירעה שגיאה בעת שינוי הפרטים, אנא עדכן את מנהל התוכנה";
                        Logger.EmptyLog(mBundleRow.mDocketId, eLogger.EXCEPTION);
                    }
                }
                else
                {
                    if (ddlServiceType.SelectedValue == "8")
                    {
                        string updateSupllierAddQuery = string.Format("UPDATE BUNDLES_TO_SUPPLIERS_ADDS SET amount = {0} WHERE bundle_id = {1}"
                                                                       , bundleRow.mAmount, bundleRow.mId);
                        try
                        {
                            DAL_SQL.RunSql(updateSupllierAddQuery);
                            handleError("updated BUNDLES_TO_SUPPLIERS_ADDS bundle_id = " + bundleRow.mId + " was set)", eLogger.DEBUG);
                            isChangeSuccess = true;
                            sendMessageToClient("הפרטים שונו בהצלחה", eLogger.DEBUG);
                            //Log for user to see the docket that changed
                            Logger.EmptyLog(bundleRow.mDocketId, eLogger.CHANGED_DETAILS);
                        }
                        catch (Exception excep)
                        {
                            handleError("Failed to update BUNDLES_TO_SUPPLIERS_ADDS bundleId = " + bundleRow.mId + ", query = " + updateSupllierAddQuery + ", exception = " + excep.Message, eLogger.EXCEPTION);
                            sendMessageToClient("(BUNDLES_TO_SUPPLIERS_ADDS) אירעה שגיאה בעת שינוי הפרטים, אנא עדכן את מנהל התוכנה", eLogger.EXCEPTION);
                            Session["failedToChangeDetails"] = "(BUNDLES_TO_SUPPLIERS_ADDS) אירעה שגיאה בעת שינוי הפרטים, אנא עדכן את מנהל התוכנה";
                            Logger.EmptyLog(mBundleRow.mDocketId, eLogger.EXCEPTION);
                        }
                    }
                }
            }
            catch (Exception exc)
            {
                handleError("Failed to update BUNDLES_to_TRAVELLERS bundleId = " + bundleRow.mId + ", query = " + updateBundlesToTravellersQuery + ", exception = " + exc.Message, eLogger.EXCEPTION);
                sendMessageToClient("(BUNDLES_to_TRAVELLERS) אירעה שגיאה בעת שינוי הפרטים, אנא עדכן את מנהל התוכנה", eLogger.EXCEPTION);
                Session["failedToChangeDetails"] = "(BUNDLES_to_TRAVELLERS) אירעה שגיאה בעת שינוי הפרטים, אנא עדכן את מנהל התוכנה";
                Logger.EmptyLog(mBundleRow.mDocketId, eLogger.EXCEPTION);
            }
        }
        catch (Exception ex)
        {
            handleError("Failed to update BUNDLES bundleId = " + bundleRow.mId + ", query = " + updateBundlesQuery + ", exception = " + ex.Message, eLogger.EXCEPTION);
            sendMessageToClient("(BUNDLES) אירעה שגיאה בעת שינוי הפרטים, אנא עדכן את מנהל התוכנה",eLogger.EXCEPTION);
            Session["failedToChangeDetails"] = "(BUNDLES) אירעה שגיאה בעת שינוי הפרטים, אנא עדכן את מנהל התוכנה";
            Logger.EmptyLog(mBundleRow.mDocketId, eLogger.EXCEPTION);
        }

        return isChangeSuccess;
    }

    protected void btChangeBundleDetails_Click(object sender, EventArgs e)
    {
       
        BundleRow bundleRow = null;
        List<BundleRow> bundleList = null;

        bundleList = (List<BundleRow>)Session["BundleList"];

        getPriceDetails();

        if (Session["bundleToChangeCount"] != null)
        {
            //bundleRow = bundleList[int.Parse(Session["bundleToChangeCount"].ToString())];
            sendMessageToClient("עכת ניתן לשנות רק את כל התיקים לפי האופציות שנבחרו .", eLogger.DEBUG);

            //changeBundleDetails(bundleRow);
        }
        else
        {
            sendMessageToClient("אנא בחר תיק.", eLogger.DEBUG);
        }
    }

    protected void btSearch_Click(object sender, EventArgs e)
    {
        isShowClientMessage = true;

        Session["bundleToChangeCount"] = "-1";
        setBundlesList();
        Session["BundleList"] = mBundlesList;

        btGetNextBundle_Click(sender, e);
    }

    protected void btChangeAll_Click(object sender, EventArgs e)
    {
        isShowClientMessage = false;
        BundleRow oldBundle = null;
Logger.Log("chen1", eLogger.EXCEPTION);


        if (Session["Done"] == null)
        {
            setBundlesList();
Logger.Log("chen8", eLogger.EXCEPTION);
            foreach (BundleRow bundleRow in mBundlesList)
            {
                Session["isBundleWithExtraPay"] = null;
                Session["failedToChangeDetails"] = null;

                mBundleRow = bundleRow;
                if (!isBundleWithExtraPay(bundleRow.mId))
                {
                    Logger.Log("BundleId = " + bundleRow.mId + " About to change ", eLogger.CHANGED_DETAILS);
                    oldBundle = new BundleRow(bundleRow);
                    Logger.Log("Changing from :  " + oldBundle.getBUNDLESUpdateString(), eLogger.CHANGED_DETAILS);

                    //Getting the prices to change.
                    setBundleNewPrice(bundleRow, ddlServiceType.SelectedValue);

                    Logger.Log("Changing to:  " + bundleRow.getBUNDLESUpdateString(), eLogger.CHANGED_DETAILS);

                    if (Session["failedToChangeDetails"] == null)
                    {
                        changeBundleDetails(bundleRow);
                        Logger.Log("BundleId = " + bundleRow.mId + "DONE.", eLogger.CHANGED_DETAILS);
                    }
                    else
                    {
                        if (Session["isBundleWithExtraPay"] != null && Session["isBundleWithExtraPay"].ToString() == "1")
                        {
                            Logger.Log("BundleId = " + bundleRow.mId + " got extraPay, skip to next bundle.", eLogger.CHANGED_DETAILS);
                        }
                        else
                        {
                            Logger.Log("Failed to update. BundleId = " + bundleRow.mId + "(" + Session["failedToChangeDetails"].ToString() + ").", eLogger.CHANGED_DETAILS);
                        }
                    }
                }
                else
                {
                	Session["failedToChangeDetails"] = "תיק זה עם תוספת תשלום אנא בחלף ידנית בתיק.";
                	Session["isBundleWithExtraPay"] = "1";
                	sendMessageToClient("תיק זה עם תוספת תשלום אנא בחלף ידנית בתיק.", eLogger.CHANGED_DETAILS);
                	btChangeBundleDetails.Visible = false;
                	Logger.EmptyLog(bundleRow.mDocketId, eLogger.EXTRA_PAY);
                }
            }

            string fileNameMultiPrice = string.Empty;
            string fileNameNoPriceFound = string.Empty;
            string fileNameExtraPay = string.Empty;
            string fileNameDocketsChanged = string.Empty;

            fullPage.Visible = false;
            afterChangeAllFiles.Visible = true;

            fileNameMultiPrice = HttpContext.Current.Request.MapPath("Logs\\UserPriceNotFound_" + (DateTime.Now.ToString("yyyy_MM_dd") + ".txt"));
            fileNameNoPriceFound = HttpContext.Current.Request.MapPath("Logs\\UserMultiPrices_" + (DateTime.Now.ToString("yyyy_MM_dd") + ".txt"));
            fileNameDocketsChanged = HttpContext.Current.Request.MapPath("Logs\\UserChangeDetails_" + (DateTime.Now.ToString("yyyy_MM_dd") + ".txt"));
            fileNameExtraPay = HttpContext.Current.Request.MapPath("Logs\\UserExtraPay_" + (DateTime.Now.ToString("yyyy_MM_dd") + ".txt"));



            btMultiPricesTxt.PostBackUrl = btMultiPricesTxt.ResolveClientUrl(fileNameMultiPrice);
            btNoPriceFoundTxt.PostBackUrl = btNoPriceFoundTxt.ResolveClientUrl(fileNameNoPriceFound);
            btExtraPaytxt.PostBackUrl = btExtraPaytxt.ResolveClientUrl(fileNameExtraPay);
            btDocketsChangedTxt.PostBackUrl = btDocketsChangedTxt.ResolveClientUrl(fileNameDocketsChanged);

            //Clear all.
            mBundlesList.Clear();
            mBundleRow = null;
            Session["Done"] = "true";

            isShowClientMessage = true;
            sendMessageToClient("התהליך הושלם.", eLogger.DEBUG);

        }
        else
        {
            isShowClientMessage = true;
            sendMessageToClient("אנא בחר שוב.", eLogger.DEBUG);
            Session["Done"] = null;
            isShowClientMessage = false;
        }
    }


    private string getAddId(BundleRow iBundleRow)
    {
        string addId = string.Empty;

        int bundleId = iBundleRow.mId;
        string travIdFromBTT = string.Empty;
        string makat = string.Empty;
        string travId = string.Empty;

        travIdFromBTT = DAL_SQL.GetRecord("BUNDLES_to_TRAVELLERS","traveller_id",bundleId.ToString() ,"bundle_id");
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

    private void clearUIParams()
    {
        //New amounts.
        lbNewBrutto.Text = string.Empty; ;
        lbNewNetto.Text = string.Empty; ;
        lbNewSubsid.Text = string.Empty; ;
        lbNewVatBrutto.Text = string.Empty; ;
        lbNewCommisionValue.Text = string.Empty; ;
        lbNewCommisionVat.Text = string.Empty; ;
        lbNewToClerk.Text = string.Empty; ;


        //lbFromDate.Text = string.Empty; ;
        //lbToDate.Text = string.Empty; ;
        //lbSubsidBase.Text = string.Empty; ;
    }

    private string getDateByFormat(string date)
    {
        string[] dateSplitted = date.Split('/');
        string newDate = string.Empty;
        string monthStr = string.Empty;

        //Check the month.
        switch (dateSplitted[1])
        {
            case "01":
                monthStr = "JAN";
                break;
            case "02":
                monthStr = "FEB";
                break;
            case "03":
                monthStr = "MAR";
                break;
            case "04":
                monthStr = "APR";
                break;
            case "05":
                monthStr = "MAY";
                break;
            case "06":
                monthStr = "JUN";
                break;
            case "07":
                monthStr = "JUL";
                break;
            case "08":
                monthStr = "AUG";
                break;
            case "09":
                monthStr = "SEP";
                break;
            case "10":
                monthStr = "OCT";
                break;
            case "11":
                monthStr = "NOV";
                break;
            case "12":
                monthStr = "DEC";
                break;
        }

        newDate = dateSplitted[0] + "-" + monthStr + "-" + dateSplitted[2];

        return newDate;
    }

    private void sendMessageToClient(string message,eLogger iDebugLevel)
    {
        Logger.Log(message, iDebugLevel);

        if (isShowClientMessage)
        {
            ClientScript.RegisterStartupScript(this.GetType(), "Popup", "ShowPopup('" + message + "');", true);
        }
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

    private void handleError(string errorMsg, eLogger debugLevel)
    {
        Logger.Log(errorMsg, debugLevel);
        Session["ClientMessage"] = errorMsg;
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
}