using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using TreatmentEntitledService;

public partial class AcceptOrder : System.Web.UI.Page
{
	string failureMessageToClient = string.Empty;
    //OrderDetails.
    private string mDocket = string.Empty;
    private string mVoucher = string.Empty;
    private string mFromDate = string.Empty;
    private string mToDate = string.Empty;
    private string mNumberOfNights = string.Empty;
    private string mMelavim = string.Empty;
    private string mHotelName = string.Empty;
    private string mMarpe = string.Empty;
    private string mBase = string.Empty;
    private string mBaseId = string.Empty;
    private string mBody = string.Empty;
    private string mSupplierId = string.Empty;
    private string mAllocationId = string.Empty;
    private string mMarpeName = string.Empty;
    private string mMarpeId = string.Empty;




    //Insert here the order.
    OrderDetailsHandler mOrderToMake = null;

    //Extra Details
    private bool mIsOnly4Nights = false;
    private bool mIsMakatTipulim = false;


    GovTraveller traveller;

    //private readonly DateTime entitledYearLimitDate = new DateTime(DateTime.Today.Year, 3, 1); /* 01/03/<currentYear> */
    private const string k_FailedCode = "0";

    private string flyingPhoneNo = string.Empty;
    private string mMailTo = string.Empty;

    //CHEN!@!@!@
    private string fromDateParsed = string.Empty;
    private string toDateParsed = string.Empty;

    private string mMarpeSupplierId = string.Empty;
    private string mNightsAttractions = string.Empty;

    private OrderDetailsHandler firstOrder;
    private OrderDetailsHandler secondOrder;

    protected void Page_Load(object sender, EventArgs e)
    {
        int orderIdFromGov = -1;
        int orderIdFromGov_1 = -1;
        int orderIdFromGov_2 = -1;

        //mMailTo = ConfigurationManager.AppSettings["sendMailTo"].ToString();
        mDocket = Request.QueryString["DocketId"];
        mVoucher = Request.QueryString["VoucherId"];
        mMarpeId = Request.QueryString["addID"];
			
        if (!IsPostBack)
        {
			//If docket is missing means that docket didnt open in agency. or its 7+7.
			//if (!string.IsNullOrEmpty(mDocket) && !string.IsNullOrEmpty(mVoucher))
			//Chen. if null its regular order.
			if (Request.QueryString["sevenPlusSeven"] == null)
			{
				Logger.Log("regular order order");
				//put the order details from query string in the class members.
				setOrderDetailsFromQueryString();
				mOrderToMake = firstOrder;
				//make order in gov
				orderIdFromGov = makeOrderInGov();
				
				if (orderIdFromGov != -1)
				{
					lblDocketId.Text = Request.QueryString["docketid"];
					lblVoucherId.Text = Request["VoucherId"];
					Session["orderid"] = orderIdFromGov;
					Response.Redirect("./OrderViewer.aspx?docketid_1=" + lblDocketId.Text + "&voucherid_1=" + lblVoucherId.Text);
				}
			}
			else
			{
				Logger.Log("7+7 order");
				if (make2OrdersInAgency(out orderIdFromGov_1, out orderIdFromGov_2))
				{
					//set2Receipts(orderIdFromGov_1, orderIdFromGov_2);
					lblDocketId.Text = firstOrder.getDocketId();
					lblDocketId2.Text = secondOrder.getDocketId();
					
					lblVoucherId.Text = firstOrder.getVoucherId();
					lblVoucherId2.Text = secondOrder.getVoucherId();
					
					Session["orderid"] = orderIdFromGov_1.ToString() + ";" + orderIdFromGov_2.ToString();
					Response.Redirect("./OrderViewer.aspx?docketid_1=" + lblDocketId.Text + "&voucherid_1=" + lblVoucherId.Text + "&docketid_2=" + lblDocketId2.Text + "&voucherid_2=" + lblVoucherId2.Text);
				}
				else
				{
					// Chen. 25.9 docket didnt open.
					Session["ClientMessage"] = "קרתה תקלה בביצוע ההזמנה יש ליצור קשר עם השטיח המעופף - " + flyingPhoneNo;
					//Response.Redirect("./Default.aspx");
				}
			}
        }
    }
	
	
	 //put the order details from query string in the class members.
    private void setOrderDetailsFromQueryString()
    {
        // If the docketId does NOT exist means that its 7+7. (14 nights splitted to 7+7).
        if (string.IsNullOrEmpty(Request.QueryString["DocketId"]))
        {
            try
            {
                string marpeName_1;
                string marpeName_2;
                mMarpeId = Request.QueryString["addID_1"];

                //mHotelName = hotel_rooms_check_ws.GetRecord("Agency_Admin.dbo.suppliers", "name_1255", "id=" + mSupplierId).ToString();
                if (mMarpeId != "no" && !string.IsNullOrEmpty(Request.QueryString["marpeName_1"]))
                {

                    //Was before.
                    mMarpeName = hotel_rooms_check_ws.GetRecord("SUPPLIERS_OTHER_ADDS", "name", "id=" + mMarpeId).ToString();
                    //Added to this specific func.
                    marpeName_1 = hotel_rooms_check_ws.GetRecord("SUPPLIERS_OTHER_ADDS", "name", "id=" + mMarpeId).ToString();
                }
                else
                {
                    mMarpeName = Request.QueryString["marpeName_1"];

                    marpeName_1 = Request.QueryString["marpeName_1"];
                }

                mMarpeId = Request.QueryString["addID_2"];

                if (mMarpeId != "no" && !string.IsNullOrEmpty(Request.QueryString["marpeName_2"]))
                {
                    //Was before.
                    mMarpeName = hotel_rooms_check_ws.GetRecord("SUPPLIERS_OTHER_ADDS", "name", "id=" + mMarpeId).ToString();
                    //Added to this specific func.
                    marpeName_2 = hotel_rooms_check_ws.GetRecord("SUPPLIERS_OTHER_ADDS", "name", "id=" + mMarpeId).ToString();
                }
                else
                {
                    mMarpeName = Request.QueryString["marpeName_2"];

                    marpeName_2 = Request.QueryString["marpeName_2"];
                }

                //save the first order
                firstOrder = new OrderDetailsHandler(DateTime.Parse(Request.QueryString["fromDate_1"]), DateTime.Parse(Request.QueryString["toDate_1"]),
                    int.Parse(Request.QueryString["numberOfNights_1"]), int.Parse(Request.QueryString["melavim_1"]), Request.QueryString["hotelName_1"], Request.QueryString["marpe_1"],
                    Request.QueryString["base_1"], int.Parse(Request.QueryString["baseid_1"]), Request.QueryString["supplierid_1"], Request.QueryString["allocationId_1"],
                    marpeName_1, Request.QueryString["addid_1"]);


                //save the second order
                secondOrder = new OrderDetailsHandler(DateTime.Parse(Request.QueryString["fromDate_2"]), DateTime.Parse(Request.QueryString["toDate_2"]),
                    int.Parse(Request.QueryString["numberOfNights_2"]), int.Parse(Request.QueryString["melavim_2"]), Request.QueryString["hotelName_2"], Request.QueryString["marpe_2"],
                    Request.QueryString["base_2"], int.Parse(Request.QueryString["baseid_2"]), Request.QueryString["supplierid_2"], Request.QueryString["allocationId_2"],
                    marpeName_2, Request.QueryString["addid_2"]);
            }
            catch (Exception ex)
            {
                Logger.Log(ex.Message);
            }

        }
        else //Regular order.
        {
            mFromDate = Request.QueryString["fromDate"];
            mToDate = Request.QueryString["toDate"];
            mNumberOfNights = Request.QueryString["numberOfNights"];
            mMelavim = Request.QueryString["melavim"];

            mMarpe = Request.QueryString["marpe"];
            mBase = Request.QueryString["base"];
            mBaseId = Request.QueryString["baseid"];
            mSupplierId = Request.QueryString["supplierid"];
            mAllocationId = Request.QueryString["allocationid"];

            mHotelName = hotel_rooms_check_ws.GetRecord("Agency_Admin.dbo.suppliers", "name_1255", "id=" + mSupplierId).ToString();
            if (mMarpeId != "no")
                mMarpeName = hotel_rooms_check_ws.GetRecord("SUPPLIERS_OTHER_ADDS", "name", "id=" + mMarpeId).ToString();

            firstOrder = new OrderDetailsHandler(DateTime.Parse(mFromDate), DateTime.Parse(mToDate), int.Parse(mNumberOfNights), int.Parse(mMelavim),
                                                   mHotelName, mMarpe, mBase, int.Parse(mBaseId), mSupplierId, mAllocationId, mMarpeName, mMarpeId);
			
			firstOrder.setDocketId(mDocket);
			firstOrder.setVoucherId(mVoucher);
        }
    }
	
	
	//making orders in agency,  if anything fails returb false.
    public bool make2OrdersInAgency(out int orderIdFromGov_1, out int orderIdFromGov_2)
    {
        bool isSuccess = true;
        bool isDocketsCreated = false;
        int areaID;
        orderIdFromGov_1 = -1;
        orderIdFromGov_2 = -1;

		string newPriceStr = "0";
        HotelPriceOrderResult makeOrderFirstResult = null;
        HotelPriceOrderResult makeOrderSecondResult = null;

        setOrderDetailsFromQueryString();

        traveller = GovTraveller.LoadFromSession();

        //Area needed to  get prices. got from the first order in the query string.
        areaID = int.Parse(Request.QueryString["AreaId"]);

        AjaxService aj = new AjaxService();

        HotelPrice hp1 = aj.GetHotelPrice(int.Parse(firstOrder.mSupplierid), int.Parse(firstOrder.mAllocationid), firstOrder.mBaseid, firstOrder.mFromDate.ToString("dd/MM/yyyy"), firstOrder.mToDate.ToString("dd/MM/yyyy"), newPriceStr);

        makeOrderFirstResult = aj.MakeOrder(int.Parse(firstOrder.mSupplierid), hp1.PriceId, int.Parse(firstOrder.mAllocationid),
                                                 firstOrder.mFromDate.ToString("dd/MM/yyyy"), firstOrder.mToDate.ToString("dd/MM/yyyy"),
                                                 hp1.PriceAmountNetto.ToString(), hp1.PriceAmountBruto.ToString(), hp1.ZakayPays.ToString(),
                                                 hp1.ZakaySibsud.ToString(), hp1.MelavePays.ToString(), hp1.MelaveSibsud.ToString(), areaID, "");

        //Make sure the docket and voucher was created.
        if (!string.IsNullOrEmpty(makeOrderFirstResult.AgencyDocketId) && !string.IsNullOrEmpty(makeOrderFirstResult.AgencyVoucherId) && makeOrderFirstResult.AgencyVoucherId != "0")
        {
            Logger.Log("First Docket created = " + makeOrderFirstResult.AgencyDocketId);
            HotelPrice hp2 = aj.GetHotelPrice(int.Parse(secondOrder.mSupplierid), int.Parse(secondOrder.mAllocationid), secondOrder.mBaseid, secondOrder.mFromDate.ToString("dd/MM/yyyy"), secondOrder.mToDate.ToString("dd/MM/yyyy"), newPriceStr);

            makeOrderSecondResult = aj.MakeOrder(int.Parse(secondOrder.mSupplierid), hp2.PriceId, int.Parse(secondOrder.mAllocationid),
                                                     secondOrder.mFromDate.ToString("dd/MM/yyyy"), secondOrder.mToDate.ToString("dd/MM/yyyy"),
                                                     hp2.PriceAmountNetto.ToString(), hp2.PriceAmountBruto.ToString(), hp2.ZakayPays.ToString(),
                                                     hp2.ZakaySibsud.ToString(), hp2.MelavePays.ToString(), hp2.MelaveSibsud.ToString(), areaID, "");

            if (!string.IsNullOrEmpty(makeOrderSecondResult.AgencyDocketId) && !string.IsNullOrEmpty(makeOrderSecondResult.AgencyVoucherId) && makeOrderSecondResult.AgencyVoucherId != "0")
            {
                Logger.Log("Second Docket created = " + makeOrderSecondResult.AgencyDocketId);
                isDocketsCreated = true;
            }
            else
            {
                Logger.Log("Second docket failed to create.");
                //Canceling the first and second docket voucher.
                hotel_rooms_check_ws.cancelVoucher(makeOrderFirstResult.AgencyDocketId);
				Logger.Log("Second Docket cancelled");
                // Second docket didn't open.
                Session["ClientMessage"] = "קרתה תקלה בביצוע ההזמנה יש ליצור קשר עם השטיח המעופף - " + flyingPhoneNo;
                //Response.Redirect("./Default.aspx");
            }
        }
        else
        {
            Logger.Log("First docket failed to create.");
            // First docket didn't open.
            Session["ClientMessage"] = "קרתה תקלה בביצוע ההזמנה יש ליצור קשר עם השטיח המעופף - " + flyingPhoneNo;
            //Response.Redirect("./Default.aspx");
        }

        //Only if both docket were reated fine, then send request to GOV.
        if (isDocketsCreated)
        {
            try
            {
                //making the first order in GOV
                firstOrder.setDocketId(makeOrderFirstResult.AgencyDocketId);
                firstOrder.setVoucherId(makeOrderFirstResult.AgencyVoucherId);

                mOrderToMake = firstOrder;
                orderIdFromGov_1 = makeOrderInGov();

                //Make sure the docket and voucher was created.
                if (orderIdFromGov_1 != -1)
                {
                    try
                    {
                        //making the second order in GOV
                        secondOrder.setDocketId(makeOrderSecondResult.AgencyDocketId);
                        secondOrder.setVoucherId(makeOrderSecondResult.AgencyVoucherId);

                        mOrderToMake = secondOrder;
                        orderIdFromGov_2 = makeOrderInGov();
                        setGovConnetedVoucherInBundle(makeOrderFirstResult.AgencyVoucherId, makeOrderSecondResult.AgencyVoucherId);

                        if (orderIdFromGov_2 != -1)
                        {
                            try
                            {//Insert into GOV_Approval_Order table, the number of the other order. (so each order will 'know' the other)
                                connectOrdersInOrderApprovalTable(orderIdFromGov_1, orderIdFromGov_2);
								Logger.Log("First Gov Order Id: "+orderIdFromGov_1 + " Connected to Seond Gov Order Id: "+ orderIdFromGov_2);
                                isSuccess = true;
                            }
                            catch
                            {
                                CancelOrderWebService orderAction = new CancelOrderWebService();
                                string cancelReason = "0";
                                int cancelStatus_1, cancelStatus_2;
                                //Canceling the both orders in GOV.
                                Logger.Log("Canceling the both orders in GOV. orders: " + orderIdFromGov_1 + ", " + orderIdFromGov_2);
                                cancelStatus_1 = orderAction.CancelOrder(firstOrder.getDocketId(), orderIdFromGov_1.ToString(), cancelReason,"");
                                cancelStatus_2 = orderAction.CancelOrder(secondOrder.getDocketId(), orderIdFromGov_2.ToString(), cancelReason,"");

                                if (cancelStatus_1 != 0)
                                {
                                    Logger.Log("Order " + orderIdFromGov_1 + " was canceled successfully.");
                                }
                                else
                                {
                                    Logger.Log("Failed to cancel order " + orderIdFromGov_1);
                                }
                                if (cancelStatus_2 != 0)
                                {
                                    Logger.Log("Order " + orderIdFromGov_2 + " was canceled successfully.");
                                }
                                else
                                {
                                    Logger.Log("Failed to cancel order " + orderIdFromGov_2);
                                }
                            }
                        }
                        else
                        {
                            CancelOrderWebService orderAction = new CancelOrderWebService();
                            string cancelReason = "0";
                            orderAction.CancelOrder(firstOrder.getDocketId(), orderIdFromGov_1.ToString(), cancelReason,"");

                        }
                    }
                    catch (Exception exc)
                    {//Second GovOrder Error.
                        Logger.Log("Error making GovOrders.  Exception : " + exc.Message);

                        //Canceling the first docket voucher.
                        try
                        {

                            hotel_rooms_check_ws.cancelVoucher(makeOrderFirstResult.AgencyDocketId);
                        }
                        catch (Exception exc1)
                        {
                            Logger.Log("Failed to cancel voucher1 (maybe cause the voucher already canceled -- docketId=" + makeOrderFirstResult.AgencyDocketId + ", exception=" + exc1.Message);
                        }
                        //Canceling the Second docket voucher.
                        try
                        {
                            hotel_rooms_check_ws.cancelVoucher(makeOrderSecondResult.AgencyDocketId);
                        }
                        catch (Exception exc2)
                        {
                            Logger.Log("Failed to cancel voucher2 (maybe cause the voucher already canceled -- docketId=" + makeOrderSecondResult.AgencyDocketId + ", exception=" + exc2.Message);
                        }

                        // First docket didn't open.
                        Session["ClientMessage"] = "קרתה תקלה בביצוע ההזמנה יש ליצור קשר עם השטיח המעופף - " + flyingPhoneNo;
                        //Response.Redirect("./Default.aspx");
                    }
                }
                else
                {//First GovOrder Error.
                    Logger.Log("First GovOrder failed to create.");

                    //Canceling the first docket voucher.
                    try
                    {
                        hotel_rooms_check_ws.cancelVoucher(makeOrderFirstResult.AgencyDocketId);
                    }
                    catch (Exception exc1)
                    {
                        Logger.Log("Failed to cancel voucher1 (maybe cause the voucher already canceled -- docketId=" + makeOrderFirstResult.AgencyDocketId + ", exception=" + exc1.Message);
                    }
                    //Canceling the Second docket voucher.
                    try
                    {
                        hotel_rooms_check_ws.cancelVoucher(makeOrderSecondResult.AgencyDocketId);
                    }
                    catch (Exception exc2)
                    {
                        Logger.Log("Failed to cancel voucher2 (maybe cause the voucher already canceled -- docketId=" + makeOrderSecondResult.AgencyDocketId + ", exception=" + exc2.Message);
                    }

                    // First docket didn't open.
                    Session["ClientMessage"] = "קרתה תקלה בביצוע ההזמנה יש ליצור קשר עם השטיח המעופף - " + flyingPhoneNo;
                    //Response.Redirect("./Default.aspx");
                }
            }
            catch (Exception ex)
            {//If any error caught in making GovOrders.
                Logger.Log("Error making GovOrders.  Exception : " + ex.Message);

                //Canceling the first docket voucher. ?? already canceled in makeOrderInGov.
                //hotel_rooms_check_ws.cancelVoucher(makeOrderFirstResult.AgencyDocketId);

                // First docket didn't open.
                Session["ClientMessage"] = "קרתה תקלה בביצוע ההזמנה יש ליצור קשר עם השטיח המעופף - " + flyingPhoneNo;
                //Response.Redirect("./Default.aspx");
            }
        }
        else
        {//Failed to create one of the dockets.
            Logger.Log("Failed to create one of the dockets.");

            // First docket didn't open.
            Session["ClientMessage"] = "קרתה תקלה בביצוע ההזמנה יש ליצור קשר עם השטיח המעופף - " + flyingPhoneNo;
            //Response.Redirect("./Default.aspx");
        }

        //Took from orderAction (maybe need to delete).
        //DAL_SQL_Helper.GOV_UpdateGovOrderID(orderID, int.Parse(res.AgencyDocketId), int.Parse(traveller.TravellerId));

        return isSuccess;
    }

    //Set bundles row, 
    private void setGovConnetedVoucherInBundle(string iVoucherId_1, string iVoucherId_2)
    {
        DAL_SQL_Helper.setGovConnetedVoucherInBundle(iVoucherId_1, iVoucherId_2);
		DAL_SQL_Helper.setGovConnetedVoucherInBundle(iVoucherId_2, iVoucherId_1);
    }

    //Make order in Gov (misrad habitahon)
    //Need to set the member mOrdedToMake to make an order.
    private int makeOrderInGov()
    {
        mDocket = mOrderToMake.getDocketId();
        int orderId = -1;
        StringBuilder govFailedMsg = null;
        traveller = GovTraveller.LoadFromSession();
		ServiceNewOrderResponse orderResponse = null;
		if (traveller.SelectedMakat[0].ItemSKU == "029940" )
		{
			// 940 itemsku should behave as 35
			traveller.SelectedMakat[0].ItemSKU = "027235";
		}
        if ((traveller.makatSelected("027236") || traveller.makatSelected("027235")))
        {
            mIsMakatTipulim = true;

            if (traveller.IsAdded5thNight == true)
            {
                mIsOnly4Nights = true;
            }
        }
        try
        {
            //Get order to make in GOV object.
            EntitledOrder newOrder = getNewOrderForGov();
			System.Net.ServicePointManager.SecurityProtocol = (System.Net.SecurityProtocolType)3072;
            TreatmentEntitledServiceClient treatmentClient = new TreatmentEntitledServiceClient();
            string accessToken = Session["AccessToken"].ToString();

            orderResponse = treatmentClient.NewOrder(accessToken, newOrder);
            string orderStatus = orderResponse.Status.ClientMessages[0];
            string logMessage = string.Empty;


            if (orderResponse.FailureCode != null)
            {//An error occured, canceling voucher, getting log messages from GOV.

                failureMessageToClient = GovErrorHandler.getErrorInNewOrder(orderResponse, out govFailedMsg, out logMessage);
                Logger.Log(logMessage + "Misrad habitahon message : " + govFailedMsg.ToString());
				Logger.Log("Canceling order. (docket = " + mDocket + ")");
				if (string.IsNullOrEmpty(Request.QueryString["TreatmentOnly"])) //@940
				{ 
					hotel_rooms_check_ws.cancelVoucher(newOrder.OrderNumberSupplier);
					
					string bundleId = string.Empty;
					try
					{
						bundleId = DAL_SQL.GetRecord("BUNDLES", "id", mDocket, "docket_id");
	
						DAL_SQL.UpdateRecord("BUNDLES", "gov_voucher_cancel_date", DateTime.Now.Date.ToString("dd-MMM-yy"), bundleId, "id");
						DAL_SQL.UpdateRecord("BUNDLES", "last_update_date", DateTime.Now.Date.ToString("dd-MMM-yy"), bundleId, "id");                                                           
					}
					catch (Exception ex2)
					{
						Logger.Log("Failed to update last_update_date, gov_voucher_cancel_date = " + DateTime.Now.Date.ToString("dd-MMM-yy")  + ", bundleId = " + bundleId + ", Exception = " + ex2.Message);
					} 
					Session["ClientMessage"] = failureMessageToClient;
				}
				//Response.Redirect("./Default.aspx");
            }
            else
            {//If succeeded making newOrder in GOV. 
                Session["orderid"] = orderResponse.OrderId.ToString();
                Logger.Log("MakeOrder: GovOrderId=" + orderResponse.OrderId + ", docket=" + mDocket + ".");

				try
				{
					//Chen. in hotel Lot from 7.1.18 need to add that they are making some changes in the hotel.
					string text = string.Empty;
					//lotText = "%D7%94%D7%97%D7%9C%20%D7%9E%D7%94%D7%AA%D7%90%D7%A8%D7%99%D7%9A%2007.01.18%20%D7%94%D7%9E%D7%9C%D7%95%D7%9F%20%D7%91%D7%A9%D7%99%D7%A4%D7%95%D7%A6%D7%99%D7%9D.";
					DateTime changesDate = DateTime.Parse("07-Jan-18");
					string bundleIdToUpdate = DAL_SQL.GetRecord("VOUCHERS", "bundle_id", mVoucher, "id");

					if (DateTime.Parse(mToDate) >= changesDate && mSupplierId == "102")
					{
						//text = "החל מהתאריך 07.01.18 המלון בשיפוצים.";
						//DAL_SQL.RunSql("UPDATE BUNDLES SET traveller_remark = N'" + text + "' WHERE id = " + bundleIdToUpdate);						
					}
					else if (mSupplierId == "13748")
					{
						text = "הערה חשובה: לידיעתכם ארוחת הצהריים תוגש ביום ההגעה ולא ביום העזיבה.";
						DAL_SQL.RunSql("UPDATE BUNDLES SET traveller_remark = N'" + text + "' WHERE id = " + bundleIdToUpdate);
					}
					else if (mSupplierId == "335")
					{
						text = "לתשומת לבכם:<br/>"  +
								" העישון אסור בכל שטחי המלון – כולל חדרי האירוח ומרפסות החדרים<br/>" + 
								" (מלבד באזורים שהוגדרו לכך: המרפסת בלובי. מרפסת רוזמרין ומתחם הכניסה למלון)<br/>";
						DAL_SQL.RunSql("UPDATE BUNDLES SET traveller_remark = N'" + text + "' WHERE id = " + bundleIdToUpdate);
					}
				}
				catch (Exception exception1)
				{
					Logger.Log("error update LOT text. exception = " + exception1.Message);
				}
				
				if (Request.QueryString["TreatmentOnly"] == null)
				{
					makeOrderAcceptedUpdates(orderResponse.OrderId);
				}
				
                if (isAttractionNeeded())
                {
                    createAttraction();
					if (Request.QueryString["TreatmentOnly"] != null)
					{
						makeOrderAcceptedUpdates(orderResponse.OrderId);
					}
                }

                orderId = orderResponse.OrderId;
            }
        }
        catch (Exception ex)
        {//If failed to make order in GOV. cancel the voucher in created docket.
            orderId = -1;
            try
            {
				Logger.Log("Error orrured. canceling voucher. mDocket = " + mDocket + ", exception=" + ex.Message);
				if (string.IsNullOrEmpty(Request.QueryString["TreatmentOnly"])) //@940
                { 
					hotel_rooms_check_ws.cancelVoucher(mDocket);
				
					string bundleId1 = string.Empty;
					
					try
					{
						bundleId1 = DAL_SQL.GetRecord("BUNDLES", "id", mDocket, "docket_id");
	
						DAL_SQL.UpdateRecord("BUNDLES", "gov_voucher_cancel_date", DateTime.Now.Date.ToString("dd-MMM-yy"), bundleId1, "id");
						DAL_SQL.UpdateRecord("BUNDLES", "last_update_date", DateTime.Now.Date.ToString("dd-MMM-yy"), bundleId1, "id");                                                           
					}
					catch (Exception ex1)
					{
						Logger.Log("Failed to update last_update_date, gov_voucher_cancel_date = " + DateTime.Now.Date.ToString("dd-MMM-yy")  + ", bundleId = " + bundleId1 + ", Exception = " + ex1.Message);
					} 
				}
            }
            catch (Exception exc)
            {
                Logger.Log("Failed to cancel voucher (maybe cause the voucher already canceled -- docketId=" + mDocket + ", exception=" + exc.Message);
            }

            //chen. handle error.
            // One of the query string order is missing.
            if (govFailedMsg != null)
            {
                Logger.Log(ex.Message + ", Misrad habitahon message : " + govFailedMsg.ToString());
            }
            else
            {
                Logger.Log(ex.Message + "Failed to get GOV (Misrad habitahon) message");
            }
			System.Net.ServicePointManager.SecurityProtocol = (System.Net.SecurityProtocolType)3072;
			TreatmentEntitledServiceClient treatmentClient1 = new TreatmentEntitledServiceClient();
			string accessToken1 = Session["AccessToken"].ToString();
		
			//Itamar. canceling in GOV
			ServiceResponse cancelResponse1 = treatmentClient1.CancelOrder(accessToken1, orderResponse.OrderId, 0, 0);
			string orderStatus = cancelResponse1.Status.ClientMessages[0];
			Logger.Log("got exception in makeOrder. Canceling in GOV order: " + orderResponse.OrderId.ToString() + " || Traveller ID: " + traveller.TravellerId);
			string cancelError = string.Empty;
			StringBuilder clientFailedMsg1 = new StringBuilder();
			
			if (cancelResponse1.FailureCode != null)
			{
				int failure1 = cancelResponse1.FailureCode.Id;
				foreach (string msg1 in cancelResponse1.FailureCode.ClientMessages)
				{
					clientFailedMsg1.Append(msg1);
					clientFailedMsg1.Append(Environment.NewLine);
				}

				cancelError = getClientErrorMessage(failure1);
				//Response.Redirect("./Default.aspx");//chen - todo pass error file
			}
			else
			{
				Logger.Log("ההזמנה בוטלה במשרד הביטחון");
			}
		
			
			Session["ClientMessage"] = cancelError + ", " + failureMessageToClient;
            //Response.Redirect("./Default.aspx");

        }

        return orderId;
    }
	
	
	private EntitledOrder getNewOrderForGov()
    {
        EntitledOrder newOrder = new EntitledOrder();
		if (Request.QueryString["TreatmentOnly"] != null)
		{
			newOrder.KatalogNumber = "029940";
		}
		else
		{
			newOrder.KatalogNumber =  traveller.SelectedMakat[0].ItemSKU;
		}
        newOrder.RequestId = traveller.SelectedMakat[0].Request_SH;

        //Calculating the entitled year. (every 1.3.yyyy   it chagnes)
        //newOrder.EntitledYear = (DateTime.Today > entitledYearLimitDate) ? DateTime.Today.Year : DateTime.Today.Year - 1;
        //newOrder.EntitledYear = (mOrderToMake.mFromDate.Month < 3) ? mOrderToMake.mFromDate.Year - 1 : mOrderToMake.mFromDate.Year;
		newOrder.EntitledYear = int.Parse(traveller.EntitledYear);//@EY
		//if (traveller.TravellerId == "057332173")
		//{
		//	newOrder.EntitledYear = 2019;
		//}
        //נשלח ריק לפי אפיון.
        //newOrder.Id 

		try
		{
			//string bundleId = DAL_SQL.RunSql("SELECT bundle_id FROM VOUCHERS WHERE id = " + mVoucher);
			//DAL_SQL.RunSql("UPDATE BUNDLES SET gov_entitled_year = " + newOrder.EntitledYear + " WHERE id = " + bundleId);
			DAL_SQL.RunSql("UPDATE BUNDLES SET gov_entitled_year = " + newOrder.EntitledYear + " WHERE docket_id = " + mDocket);
		}
		catch(Exception exx)
		{
			Logger.Log("Exception update entitledYear. Message = " + exx.Message);
		}
		
        //Chen. 25.9 changed to docket
        newOrder.OrderNumberSupplier = mOrderToMake.getDocketId();
        newOrder.StartDate = mOrderToMake.mFromDate;  // DateTime.Parse(mFromDate);
        newOrder.EndDate = mOrderToMake.mToDate;    //DateTime.Parse(mToDate);
		
		//if (traveller.TravellerId == "022966659")
		//{
		//	newOrder.StartDate = DateTime.Parse("23-Feb-2020");
		//	newOrder.EndDate = DateTime.Parse("08-Mar-2020");
		//}
		
        // makat 40 - chose 5+5: Zakai And Melave Tmurat Zakaut
        if ((ErkevTypes)Enum.Parse(typeof(ErkevTypes), traveller.ErkevType, true) == ErkevTypes.ZakaiAndMelaveTmuratZakaut)
        {
            newOrder.DaysNumber = mOrderToMake.mNumberOfNights * 2;
        }
        else//all others
        {
            //Checking whether its 4+1. if it is then 4 nights. else mNumberOfNights.
            newOrder.DaysNumber = (mIsOnly4Nights == true) ? 4 : mOrderToMake.mNumberOfNights;
        }

        newOrder.AccompaniedNumber = mOrderToMake.mNumOfMelavim;  //int.Parse(mMelavim);
        newOrder.HotelName = mOrderToMake.mHotelName;  //mHotelName;

        Logger.Log(string.Format(@"Gov newOrder-  ItemSKU : {0} , RequestID : {1} , EntitledYear : {2}
                        OrderNumberSupplier : {3} , StartDate : {4} , EndDate : {5}
                        DaysNumber : {6} , AccompaniedNumber : {7} , HotelName : {8} "
                , newOrder.KatalogNumber, newOrder.RequestId, newOrder.EntitledYear, newOrder.OrderNumberSupplier,
                newOrder.StartDate.ToString(), newOrder.EndDate.ToString(), newOrder.DaysNumber,
                newOrder.AccompaniedNumber, newOrder.HotelName));


        return newOrder;
    }
	
	//Making some update in DB after the order accepted in GOV.
    private void makeOrderAcceptedUpdates(int iOrderId)
    {
        //Updating GovOrderId in bundles table
        DAL_SQL_Helper.GOV_UpdateGovOrderID(iOrderId, int.Parse(mOrderToMake.getDocketId()), int.Parse(traveller.TravellerId));

        string melave1FirstName = (traveller.Melave.Count > 0) ? traveller.Melave[0].FirstName : string.Empty;
        string melave1LastName = (traveller.Melave.Count > 0) ? traveller.Melave[0].LastName : string.Empty;
        string melave2FirstName = (traveller.Melave.Count > 1) ? traveller.Melave[1].FirstName : string.Empty;
        string melave2LastName = (traveller.Melave.Count > 1) ? traveller.Melave[1].LastName : string.Empty;

        DAL_SQL_Helper.InsertOrderApproval(iOrderId, traveller.SelectedMakat[0].ItemSKU, mOrderToMake.getDocketId(),
                                            mOrderToMake.mFromDate, mOrderToMake.mToDate,
                                            int.Parse(mOrderToMake.mSupplierid), mOrderToMake.mHotelName, mOrderToMake.mBaseid,
                                            traveller.FirstName, traveller.SecondName, mOrderToMake.mNumOfMelavim,
                                            melave1FirstName, melave1LastName, melave2FirstName, melave2LastName, int.Parse(mOrderToMake.mAllocationid));
    }

	 private bool isAttractionNeeded()
    {//If choosed makat tipulim only.
        bool wasMakatTipulimChosen = false;

        if (traveller.makatSelected("027242"))
        {
            foreach (GovTravellerMakat makat in traveller.Makats)
            {
                if (makat.ItemSKU == "027235" || makat.ItemSKU == "027236")
                {
                    wasMakatTipulimChosen = true;
                    break;
                }
            }
        }
        else if (traveller.SelectedMakat[0].ItemSKU == "027235" || traveller.SelectedMakat[0].ItemSKU == "027236")
        {
            wasMakatTipulimChosen = true;
        }
		/*
		bool safeCh = wasMakatTipulimChosen;
		try
		{
			if (traveller.TravellerId != null)
			{
				if (traveller.TravellerId != "054095914")
				{
					wasMakatTipulimChosen = safeCh;
				}
				else
				{
					wasMakatTipulimChosen = false;
				}
			}
		}
		catch
		{
			Logger.Log("Failed to make order for 054095914");
			wasMakatTipulimChosen = safeCh;
		}
		*/
        return wasMakatTipulimChosen;
    }

	
    private void connectOrdersInOrderApprovalTable(int iOrderIdFromGov_1, int iOrderIdFromGov_2)
    {
        //Inserting into GOV_Approval_Order the number the 7 plus 7 other order.
        DAL_SQL_Helper.InsertConnectSevenPlusSevenOrder(iOrderIdFromGov_1, iOrderIdFromGov_2);
        DAL_SQL_Helper.InsertConnectSevenPlusSevenOrder(iOrderIdFromGov_2, iOrderIdFromGov_1);

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

        //Logger.Log(logMessage);

        return failureMessageToClient;
    }    
	
	
	
	
	  //////////////////////////////////////////////////// ATTRACTION /////////////////////////////////////

    private void createAttraction()
    {
        mDocket = mOrderToMake.getDocketId();
        mFromDate = mOrderToMake.mFromDate.ToString("dd-MMM-yy");
        mToDate = mOrderToMake.mToDate.ToString("dd-MMM-yy");
        mNumberOfNights = mOrderToMake.mNumberOfNights.ToString();
        mSupplierId = mOrderToMake.mSupplierid;

        traveller = GovTraveller.LoadFromSession();
        const int serviceType = 8;
        int bundleId;
        int indicator = 0;//none
        string todayDate = DateTime.Today.Date.ToString("dd-MMM-yy");
        string clerkId;// = ClerkNameSingelton.getId();

        if (Session["User_ID"] != null)
        {
            clerkId = Session["User_ID"].ToString();
        }
        else
        {
            clerkId = "2";  // 'Internet' clerk id = 2.
        }

        //Calculate the amount of days for tipulim, according to 'each' makat selected (currently only makat 027242 has special behavior.
        mNightsAttractions = getNightsForAttraction(traveller.SelectedMakat[0].ItemSKU);
		if (mNightsAttractions != "0")
		{
			fromDateParsed = DateTime.Parse(mFromDate).ToString("dd-MMM-yy");
			toDateParsed = DateTime.Parse(mToDate).ToString("dd-MMM-yy");

			// Get Marpe Supplier Id
			mMarpeSupplierId = getMarpeSupplierId(mSupplierId);

			// 0. -- create "empty" bundle with basic details (without prices)
			bundleId = DAL_SQL_Helper.createBundle(mDocket, serviceType, todayDate, clerkId);

			// 1. -- insert row in OTHER DataBase 
			saveRowInOther(bundleId);

			// 2. -- Update Bundle 
			if (traveller.FourOneSeven.Equals("fourNightHotel5Tipulim"))
			{
				indicator = 5; //4+1 indication
			}
			string query = "UPDATE BUNDLES SET gov_indicator_id = '" + indicator + "', from_date = '" + fromDateParsed + "', to_date ='" + toDateParsed + "' WHERE id = '" + bundleId + "'";
			DAL_SQL.RunSqlbool(query);

			// 3. -- insert row in Supplier_ADDS
			saveSupplierAdds(bundleId);

			// 4. -- Attach Travellers To Bundle
			attachTravellersToBundle(bundleId);

			// 5. -- create voucher
			CreateVoucherNew(bundleId);
		}
    }

    private string getNightsForAttraction(string selectedMakat)
    {
        int amountOfDays = 0;
        int nightsOrdered = int.Parse(mNumberOfNights);

        if (traveller.SelectedMakat[0].ItemSKU == "027242")
        {
            int tipulDaysLeft35 = 0;
            int tipulDaysLeft36 = 0;

            foreach (GovTravellerMakat makat in traveller.Makats)
            {
                if (makat.ItemSKU == "027235")
                {
                    tipulDaysLeft35 = makat.DaysNum - makat.UsageBalance;
                }
                else if (makat.ItemSKU == "027236")
                {
                    tipulDaysLeft36 = makat.DaysNum - makat.UsageBalance;
                    break;
                }
            }

            // if amount of day left in makat tipulim is bigger than nightsOrdered, then client get tipulim as amount of nights he ordered.
            // else client get tipulim as amount of days he got left in makata tipulim.
            if (tipulDaysLeft36 != 0)
            {
                amountOfDays = (nightsOrdered > tipulDaysLeft36) ? tipulDaysLeft36 : nightsOrdered;
            }
            else if (tipulDaysLeft35 != 0)
            {
                amountOfDays = (nightsOrdered > tipulDaysLeft35) ? tipulDaysLeft35 : nightsOrdered;
            }
        }
        else
        {
            amountOfDays = nightsOrdered;
        }

        return amountOfDays.ToString();
    }

    private string getMarpeSupplierId(string iSupplierId)
    {
        eHotels supplierId = (eHotels)Enum.Parse(typeof(eHotels), iSupplierId);
        eHotels hotel;

        switch (supplierId)
        {
            //Ashdod
            case eHotels.AshdodWest:
                hotel = eHotels.AshdodHameiYoav;
                break;
            //DeadSea
            case eHotels.DeadSeaDaniel:
            case eHotels.DeadSeaDavid:
            case eHotels.DeadSeaLeonardoClub:
            case eHotels.DeadSeaLeonardoPlazaPrivillage:
            case eHotels.DeadSeaLot:
                hotel = supplierId;
                break;

            case eHotels.DeadSeaOasis:
                hotel = eHotels.DeadSeaSpaClub;
                break;
			
			case eHotels.DeadSeaHerods: //@ey20
				hotel = eHotels.DeadSeaHerods;
				break;
			
			case eHotels.DeadSeaCrownPlaza: //@ey20
				hotel = eHotels.DeadSeaCrownPlaza;
				break;

				
            //Tiberias
			//case eHotels.TiberiasLeonardoClub: //@ey20
			//	hotel = eHotels.TiberiasLeonardoClub;
			//	break;

			case eHotels.TiberiasLeonardoPlaza: //@ey20
			case eHotels.TiberiasLeonardoClub:
				hotel = eHotels.TiberiasLeonardoPlaza;
				break;

            case eHotels.GlatLavy:
            case eHotels.TiberiasLeonardo: //@ey20
                hotel = eHotels.TiberiasHameiCaesar;
                break;
			
            case eHotels.TiberiasRimonimMineral:
            case eHotels.TiberiasSpaVillage:
			case eHotels.TiberiasLakeHouse:
                hotel = eHotels.TiberiasHameiTiberias;
                break;
				
			case eHotels.TiberiasHofHagay: //@ey20
				hotel = eHotels.TiberiasHofHagay;
				break;
				
            //Netanye
            case eHotels.NenatyaRamada:
            case eHotels.NenatyaShfaim:
			case eHotels.NatanyaWestLagune:	//@ey20
                hotel = eHotels.NatanyaHameiGaash;
                break;
			
			
            default:
                hotel = eHotels.None;
                break;
        }
        int id = (int)hotel;
        string idStr = id.ToString();

        if (id == 0)
        {
            idStr = supplierId.ToString();
        }

        return idStr;
    }

    private void saveRowInOther(int bundleId)
    {
        int rowId;
        string attractionName = "חמי מרפא";
        string fromTime = "00:00";
        string toTime = "00:00";
        string numberOfNights = mNightsAttractions;
        string supplierID = mMarpeSupplierId;
        string marpeAddId = mOrderToMake.mAddid;//Request.QueryString["addid"];
        string quantity = mNightsAttractions;
        string status = "2"; //2 = OK.
        string todayDate = DateTime.Today.Date.ToString("dd-MMM-yy");
        bool isFiveTipulim = false;
        string clerkId = string.Empty;
        //Seems to be not in use.
        string origin = "";
        string destin = "";
        string orderNum = "";
        string confirmedBy = "";

        if (Session["User_ID"] != null)
        {
            clerkId = Session["User_ID"].ToString();
        }
        else
        {
			AgencyUser user = new AgencyUser();
			clerkId = user.AgencyUserId;
        }

        //If traveller add 5th tipul for extra money (4+1).
        if (traveller.FourOneSeven.Equals("fourNightHotel5Tipulim") || traveller.FourOneSeven.Equals("fiveNightHotelTipulim"))
        {
            isFiveTipulim = true; //4+1 indication
            quantity = "5";
            numberOfNights = "5";
        }

        rowId = DAL_SQL_Helper.createOtherRow(bundleId, attractionName, supplierID, origin, destin, fromDateParsed, toDateParsed, fromTime, toTime, numberOfNights,
                                                 orderNum, quantity, confirmedBy, status, clerkId, todayDate, isFiveTipulim);
    }

    private void saveSupplierAdds(int iBundleId)
    {
        string supplierID = mMarpeSupplierId;
        string marpeAddId = mOrderToMake.mAddid; // Request.QueryString["addid"];
        string quantity = mNightsAttractions;
        bool is4Plus1 = false;

        //If traveller add 5th tipul for extra money (4+1).
        if (traveller.FourOneSeven.Equals("fourNightHotel5Tipulim") || traveller.FourOneSeven.Equals("fiveNightHotelTipulim"))
        {
            quantity = "5";
            is4Plus1 = true;
        }

        DAL_SQL_Helper.AddNewBundleToSupplierAddsRow(iBundleId, supplierID, marpeAddId, quantity, DateTime.Parse(fromDateParsed), DateTime.Parse(toDateParsed), is4Plus1);
    }

    private void attachTravellersToBundle(int iBundleID)
    {
        bool is4Plus1 = false;


        //params to get prices for adds
        string SupplierAddID = mOrderToMake.mAddid; // Request.QueryString["addid"];
        string supplierID = mMarpeSupplierId;
        string from_date = fromDateParsed;
        string to_date = toDateParsed;
        string quantity = mNightsAttractions;// amount of nights is amount of quantity for attraction
        //Params attach bundle to traveller
        string ticket = "0";
        //CurrTravellerID = gerReecord(dockettotraveller, by docket id)
        string remark = "Flying-Shikum";
        string travellerType = "traveller";
        string roomType = "0";
        string Tax = "0.00";
        //Not in used for this func. but needed for other cases.
        string FlightAddAmount = "0.00";
        string TravellerMarkUp = "0.00";
        string NetoAmount = "0.00";

        string TravPay = "0.00";
        string TourAmount = "0.00";
        string FlightTaxAmount = "0.00";
        string TourAddAmount = "0.00";
        string VisaAmount = "0.00";
        string InsuranceAmount = "0.00";
        string travellerIdNumber = traveller.TravellerId;
		DateTime toDateForAttractionPriceNetto;
		
        //If traveller add 5th tipul for extra money (4+1).
        if (traveller.FourOneSeven.Equals("fourNightHotel5Tipulim") || traveller.FourOneSeven.Equals("fiveNightHotelTipulim"))
        {
            quantity = "5";
            is4Plus1 = true;
			toDateForAttractionPriceNetto = DateTime.Parse(to_date).AddDays(1);
        }
		else
		{
			toDateForAttractionPriceNetto = DateTime.Parse(to_date);
		}
        //////////////////////////////// Update bundle ///////////////////////////
        string Amount;
        string MarkUp;
        string TotalSupplierAmount;
        string AddsAmount;
        string TaxAmount;
        string PriceAmount;
        string ToSupplier;
        double CommisionGain;
        double ToClerk;
        string MarkUpVat;
        double CommVat;
        double CommValue;
        string commission;
        string defaultZero = "0.00";

        commission = DAL_SQL.GetRecord("SUPPLIER_DETAILS", "commission_percentage", supplierID, "supplier_id");
        double vat = DAL_SQL_Helper.getVat(DateTime.Now.Date.ToShortDateString());
        Amount = DAL_SQL_Helper.GetBrutoPriceForAttraction(iBundleID, supplierID, SupplierAddID, quantity, DateTime.Parse(from_date), DateTime.Parse(to_date)).ToString();			//Total amount                       | a:price bruto

        double AmountDbl = double.Parse(Amount);
        string subsid = Amount;

        if (traveller.FourOneSeven.Equals("fourNightHotel5Tipulim")) // 4 nights
        {
            Amount = (AmountDbl * 5 / 4).ToString();
        }
        else if (traveller.FourOneSeven.Equals("fiveNightHotelTipulim")) // 5 nights
        {
            subsid = (double.Parse(subsid) * 4 / 5).ToString();
        }

        AmountDbl = double.Parse(Amount);
        TravPay = (AmountDbl - double.Parse(subsid)).ToString();


        MarkUp = defaultZero; //MarkUp - vat                     | a:תוספת נטו usually: 0.00
        TotalSupplierAmount = Amount;    //TotalSupplierAmount        | a:price bruto
        AddsAmount = defaultZero;		//Adds                             | a: AddsAmount 0.00
        TaxAmount = defaultZero;                            			//Tax                                  | a: 0.00
        PriceAmount = Amount;   			//Price  | a: price bruto in case of sibsud = to amount
        double commisionDbl = double.Parse(commission);
        ToSupplier = DAL_SQL_Helper.GetNettoPriceForAttraction(iBundleID, supplierID, SupplierAddID, quantity, DateTime.Parse(from_date), toDateForAttractionPriceNetto).ToString();			//Total amount                       | a:price bruto(AmountDbl * ((100 - commisionDbl) / 100)).ToString();		//To supplier                      | a: amount * ((100 - commision)/100)
        CommValue = (AmountDbl * commisionDbl / 100) / ((vat / 100) + 1);//Supplier commision value         |  ( (parseFloat(TotalAmount) * parseFloat(SupplierComm.value)) / 100 ) / ( (parseFloat(vat)/100) + 1 ) ; // Commision to clerk (pure)
        CommisionGain = (CommValue / AmountDbl * 100);		//To clerk gain (include mark    up) | a: SupplierCommValue.value / amount * 100  // רווח באחוזים לספק
        ToClerk = CommValue;	//Gain ( to clerk )                    | a:  מעמ של עמלת ספק
        MarkUpVat = defaultZero;	//MarkUpVat (vat)                  | 0:00
        CommVat = CommValue * (vat / 100);	//Vat on supplier commision          | parseFloat(SupplierCommValue.value) * (parseFloat(vat) / 100)

        string CurrencyID = "1";// 1 = שקל
        string Cdate = DateTime.Now.ToString("dd-MMM-yy");
        AgencyUser user = new AgencyUser();
        string clerkId = user.AgencyUserId; //2 = internet clerk
        string PayType = "4"; //4 = הזמנה
        string Carrier = mMarpeSupplierId;
        string IncomeType = "4"; // 1= COM INC VAT
        string VatValue = "0.00000"; //default value
		//Chen.
        string to_supplier_vat = (double.Parse(ToSupplier) / (vat / 100 + 1) * (vat / 100)).ToString();
        double to_clerk_vat = CommVat + double.Parse(MarkUpVat);
        string xchange_rate_nis = "1.00000";
        string xchange_rate_usd = "0.00000";// Chen 10/11.  HANDLE!!!!
        string remarkToSupplier = "Flying-Shikum";
        string remarkToTraveller = "Flying-Shikum";
        string StaffQuantity = "0";
        string StaffRemark = "";
        string StaffAmount = "0.00";
        string AdvanceAmount1 = "0.00";
        string AdvanceDate1 = "01-Jan-01";
        string AdvanceAmount2 = "0.00";
        string AdvanceDate2 = "01-Jan-01";
        string SupplierInvoice = "";

        DAL_SQL_Helper.AttachBundleToTraveller(supplierID, travellerIdNumber, iBundleID, SupplierAddID, Tax, FlightAddAmount,
            TravellerMarkUp, TravPay, from_date, to_date, quantity, TourAmount, FlightTaxAmount, TourAddAmount, VisaAmount, InsuranceAmount,
            remark, ticket, travellerType, NetoAmount, is4Plus1);

        string sql = " UPDATE BUNDLES SET " +
                "	amount 				= " + Amount + ", " +
                "	tax					= " + TaxAmount + ", " +
                "	adds_amount			= " + AddsAmount + ", " +
                "	price				= " + PriceAmount + ", " +
                "	total_supplier		= " + TotalSupplierAmount + ", " +
                "	mark_up				= " + MarkUp + ", " +
                "	mark_up_vat			= " + MarkUpVat + ", " +
                "	currency_id			= " + CurrencyID + ", " +
                "	last_update_date	= '" + Cdate + "'," +
                "	last_update_clerk_id= '" + clerkId + "', " +
                "	pay_to_supplier_id	= " + supplierID + ", " +
                "	payment_type_id		= " + PayType + ", " +
                "	carrier				= " + Carrier + ", " +
                "	income_type			= " + IncomeType + ", " +
                "	vat_percent			= " + vat + ", " +
                "	vat_value			= " + VatValue + ", " +
                "	commision			= " + commission + ", " +
                "	commision_vat		= " + CommVat + ", " +
                "	commision_value		= " + CommValue + ", " +
                "	to_supplier			= " + ToSupplier + ", " +
                "	to_supplier_vat		= " + to_supplier_vat + ", " +
                "	to_clerk			= " + ToClerk + ", " +
                "	to_clerk_vat		= " + to_clerk_vat + ", " +
                "	to_clerk_percent	= " + CommisionGain + ", " +
                "	xchange_rate_usd	= " + xchange_rate_usd + ", " +
                "	xchange_rate_nis	= " + xchange_rate_nis + ", " +
                "	remark				=N'" + remarkToSupplier + "', " +
                "	traveller_remark	=N'" + remarkToTraveller + "', " +
                "	subsid				= " + subsid + ", " +
                "	trav_pay			= " + TravPay + ", " +
                "	staff_quantity		= " + StaffQuantity + ", " +
                "	staff_remark		=N'" + StaffRemark + "'," +
                "	staff_amount		= " + StaffAmount + ", " +
                "	advance_amount1		= " + AdvanceAmount1 + ", " +
                "	advance_date1		= '" + AdvanceDate1 + "'," +
                "	advance_amount2		= " + AdvanceAmount2 + ", " +
                "	advance_date2		= '" + AdvanceDate2 + "'," +
                "	supplier_invoice	= '" + SupplierInvoice + "' " +
                "   WHERE id = " + iBundleID;

				Logger.Log("update sql to bundle other" + sql);
        DAL_SQL.RunSql(sql);

    }

    public void CreateVoucherNew(int iBundleId)
    {
        string sql = string.Empty;
        int maxVoucherId = DAL_SQL_Helper.getMaxTableId("VOUCHERS");
        int ServiceType = 8;
        string clerkId = string.Empty;

        if (Session["User_ID"] != null)
        {
            clerkId = Session["User_ID"].ToString();
        }
        else
        {
            clerkId = "2";  // 'Internet' clerk id = 2.
        }

        string CurrencyId = "1";// 1 = שקל
        string VoucherCDate = DateTime.Today.ToString("dd-MMM-yy");
        string ValueDate = VoucherCDate;

        DAL_SQL_Helper.CreateVoucherForAttraction(maxVoucherId, mDocket, ServiceType, iBundleId, mMarpeSupplierId, clerkId, VoucherCDate, ValueDate, CurrencyId);
        //agency never used - sql = " UPDATE Order_Site SET isAccepted = 1, Status = 1 WHERE DocketId = '" + mDocket + "' AND bundle_id = '" + iBundleId + "' ";
    }

    public enum eHotels
    {
        None = 0,

        //DeadSea
        DeadSeaOasis = 2050,
        DeadSeaDavid = 13748,
        DeadSeaDaniel = 2741,
        DeadSeaLeonardoPlazaPrivillage = 4859,
        DeadSeaLeonardoClub = 96,
        DeadSeaLot = 102,
        DeadSeaSpaClub = 107,
		DeadSeaHerods = 4860,//@ey20 <<tipulim>>:inhouse
		DeadSeaCrownPlaza = 335,//@ey20 <<tipulim>>:inhouse
		
        //Tiberias
        TiberiasLeonardo = 2323,
        TiberiasLeonardoClub = 756,
        TiberiasSpaVillage = 2487,
        TiberiasRimonimMineral = 131,
		TiberiasLakeHouse = 18977,
        TiberiasHameiCaesar = 146,
		TiberiasHameiTiberias = 18978,
        //TiberiasHameiTiberias = 367, // Changed to new supplierId 18978
		TiberiasLeonardoPlaza = 151,//@ey20 <<tipulim>>:hamei keisar aka keisar premier(146)
		TiberiasHofHagay = 134, //@ey20 <<tipulim>>:inhouse
		
        //Glat
        GlatLavy = 306,
        //Ashdod
        AshdodWest = 12206,
        AshdodHameiYoav = 1712,
        //Netanya
        NenatyaRamada = 10164,
        NenatyaShfaim = 244,
        NatanyaHameiGaash = 526,
		NatanyaWestLagune = 23753 //@ey20 <<tipulim>>:HameiGaash(526)
    }
}
