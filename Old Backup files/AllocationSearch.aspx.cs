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
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Threading;
using System.Globalization;
using TreatmentEntitledService;
using System.Text;
using System.Globalization;

public partial class AllocationSearch : System.Web.UI.Page
{
    public GovTraveller traveller;
    private DataSet dsHolidays;
    private string mClientMessage = string.Empty;
    public string mAddId = "";
	public DateTime mFromDate;
	public DateTime mToDate;
	public bool mIsDeadSeaMonthBefore = false;
	public int daysBeforeMerhatzaot = 45;
	public bool isErrorDetected = false;
	
    protected void Page_Load(object sender, EventArgs e)
    {
        try
        {
            //if (!Utils.CheckSecurity(225)) Response.Redirect("AccessDenied.aspx");
            if (Session["ClientMessage"] != null)
            {
                string messageDeleted = Session["ClientMessage"].ToString();
                ClientScript.RegisterStartupScript(this.GetType(), "Popup", "ShowPopup('" + messageDeleted + "');", true);
                Session["ClientMessage"] = null;
            }

            lblMessage.Visible = false;
            traveller = GovTraveller.LoadFromSession();
            gtUC.GovTraveller = traveller; 
 
			string erkevType = traveller.ErkevType;
			ErkevTypes erkev_enum = (ErkevTypes)Enum.Parse(typeof(ErkevTypes), erkevType, true);
			
			switch (erkev_enum)
			{
				case ErkevTypes.ZakaiAndMelaveBeTashlum:        
				case ErkevTypes.ZakaiAndMelaveBeTashlumHelekTkufa:
				case ErkevTypes.ZakaiBeTashlumAndMelaveLeLoTashlum: // makat 40
					lblMoneySKU.Visible = true;
					
					break;
				default:
				lblMoneySKU.Visible = false;
				break;
	
			}
			
			if (traveller.FourOneSeven.Equals("fourNightHotel5Tipulim"))
			{
				lblMoneySKU.Visible = true;
			}
		
			//7+7 - show the link to the allocation search which allows splitting an orders
            if (traveller.makatSelected("027241") || traveller.makatSelected("027242"))
            {
                foreach (GovTravellerMakat makat in traveller.Makats)
                {
                    if (makat.ItemSKU == "027241" && makat.DaysNum >= 14 || makat.ItemSKU == "027242" && makat.DaysNum >= 14)
                    {
                        btGoToAllocationSearchSplit.Visible = true;
                        break;
                    }
                }
            }
			
			if (traveller.makatSelected("027242"))
			{
				foreach (GovTravellerMakat makat in traveller.Makats)
				{
					if (makat.ItemSKU == "027235")
					{
						mAddId = "1";
					}

					else if (makat.ItemSKU == "027236")
					{
						mAddId = "2";
						lbSpecialRoomMessage.Text = "שים לב! זהו אישור לחדר מלון רגיל";
						break;
					}
				}
			}
			else if (traveller.SelectedMakat[0].ItemSKU == "027235")
			{
				mAddId = "1";
			}
			else if (traveller.SelectedMakat[0].ItemSKU == "027236")
			{
				mAddId = "2";
				lbSpecialRoomMessage.Text = "שים לב! זהו אישור לחדר מלון רגיל";
			}
		
			if (!string.IsNullOrEmpty(mAddId))
            {
				string marpeName = string.Empty;
				
				try
				{
					marpeName = DAL_SQL.GetRecord("SUPPLIERS_OTHER_ADDS", "name", mAddId, "id");
				}
				catch(Exception ex1)
				{
					isErrorDetected = true;
					Session["ClientMessage"] = "אירעה שגיאה בחיפוש המלונות";
					Logger.Log("Failed to get add_id ,Exception = " + ex1.Message);
				}
                lbChosenMarpe.Text = marpeName;
                lbChosenMarpe.Visible = true;
				
            }
            

            if (Session["Holidays"] != null)
            {
                dsHolidays = (DataSet)Session["Holidays"];
            }
            else
            {
				try
				{
					dsHolidays = DAL_SQL_Helper.GetHolidays();
				}
				catch(Exception ex2)
				{
					isErrorDetected = true;
					Session["ClientMessage"] = "אירעה שגיאה בחיפוש המלונות";
					Logger.Log("Failed to get holiday ,Exception = " + ex2.Message);
				}
                
                Session["Holidays"] = dsHolidays;
            }

            //for debug
            //traveller.ErkevType = ErkevTypes.ZakaiAndMelaveBeTashlum.ToString();
            
			if (!Page.IsPostBack)
            {
                lblMessage.Text = "";
                tblAllocations.Attributes.Add("dir", "rtl");
				if (!isErrorDetected)
				{
					loadPageInitialData();
				}
            }
			
			ZakaiMelavePartLabels(traveller.ErkevType.Equals("ZakaiAndMelaveBeTashlumHelekTkufa"));
        }
        catch (Exception ex)
        {
            Logger.Log("1" + ex.Message);
            Session["IsLoggedIn"] = "זמן ההתחברות תם, אנא כנס/י מחדש";
            Response.Redirect("./default.aspx");
        }

    }

    //Displays text on labels under datepickers if traveller ErkevType is ZakaiAndMelaveBeTashlumHelekTkufa"
    private void ZakaiMelavePartLabels(bool textOn)
    {
        string txt = "";
        if (textOn)
        {
            txt = "דגל\\י את ימי מלווה בתשלום";
        }
        ZakaiAndMelavePartLabel1.Text = ZakaiAndMelavePartLabel2.Text = txt;
    }

    private void loadPageInitialData()
    {
        List<GeneralArea> areas = new List<GeneralArea>();
        areas = GeneralArea.LoadGeneralAreas(traveller);
        dGeneralAreaId.DataSource = areas;
        dGeneralAreaId.DataBind();
    }

    protected void btnSendOnClick(object sender, EventArgs e)
    {
		try{
			if (!validateSearch() || !isVacationOnSameDates())
			{
				ClientScript.RegisterStartupScript(this.GetType(), "Popup", "ShowPopup('" + mClientMessage + "');", true);
				mClientMessage = string.Empty;
	
				// return;
			}
			else
			{
	
				TimeSpan ts = new TimeSpan();
				int rowIndex = 0;
				
				string dateFrom = txtFromDate.Text;
				if ( !string.IsNullOrEmpty(dateFrom) )
				{
					DateTime originFromDate = DateTime.Parse(txtFromDate.Text);
					DateTime originToDate = DateTime.Parse(txtToDate.Text);
					//aviran - changed the 2 days before and after range
					DateTime fromDate = DateTime.Parse(txtFromDate.Text);
					DateTime toDate = DateTime.Parse(txtToDate.Text).AddDays(-1);
					
					mFromDate = fromDate;
					mToDate = toDate;
		
					ts = (originFromDate - originToDate);
					int countdaysSeacrch = Convert.ToInt32(txtNights.Text);//ts.Days;
					string resXML = string.Empty;
					
					try
					{
						resXML = DAL_SQL_Helper.SearchAvailableAllocations(int.Parse(dGeneralAreaId.SelectedValue),
																		fromDate, toDate,
																		traveller.SelectedMakat[0].MakatTipulim,
																		(1 + traveller.Melave.Count));
						//Logger.Log(resXML);
					}
					catch(Exception ex3)
					{
						isErrorDetected = true;
						Session["ClientMessage"] = "אירעה שגיאה בחיפוש המלונות";
						Logger.Log("Failed to search ,Exception = " + ex3.Message);
					}
					
					/*chen - new function that build the availble hotel views */
					buildHotelViews(resXML);
				}
			}
		}
		catch(Exception ex){
			Logger.Log(ex.Message);
			Session["ClientMessage"] = "תוקף טוקן משרד הביטחון תם, אנא חפש מחדש";
			Response.Redirect("./Default.aspx");
		}
    }

    private bool isVacationOnSameDates()
    {
        bool noVacationOnSameDates = true;

        DateTime fromDate = DateTime.Parse(txtFromDate.Text);
        DateTime toDate = DateTime.Parse(txtToDate.Text);
		System.Net.ServicePointManager.SecurityProtocol = (System.Net.SecurityProtocolType)3072;
        TreatmentEntitledServiceClient treatmentClient = new TreatmentEntitledServiceClient();
        string accessToken = Session["accessToken"].ToString();
        ServiceEntitledResponse responseEntitled = treatmentClient.GetEntitledDetails(accessToken);
        string[] messages = responseEntitled.Status.ClientMessages;
        string message = messages[0];
        StringBuilder clientFailedMsg = new StringBuilder();

        if (responseEntitled.FailureCode != null)
        {
            int failure = responseEntitled.FailureCode.Id;
            foreach (string msg in responseEntitled.FailureCode.ClientMessages)
            {
                clientFailedMsg.Append(msg);
                clientFailedMsg.Append(Environment.NewLine);
            }

            Logger.Log("Failed to get entitledDetails.  Misrad Habitahon message: " + clientFailedMsg.ToString());
        }

        foreach (EntitledOrder order in responseEntitled.EntitledOrders)
        {
            if ((order.EndDate > fromDate && order.EndDate <= toDate ||
                order.StartDate >= fromDate && order.StartDate < toDate)
                && order.Status == true)
            {
                noVacationOnSameDates = false;
                mClientMessage = "קיימת הזמנה בתאריכים אלו אנא בחר תאריך אחר";
                break;
            }
        }

        return noVacationOnSameDates;
    }

    //chen 31/08 - this function build the hotel view
    private void buildHotelViews(string resXML)
    {
        //XmlNodeList allocationsNodes = iXmlDoc.GetElementsByTagName("root/allocation");
        string hotelAttributes = string.Empty;
        string areaName = string.Empty;
        int rowIndex = 0;
        int i = 0;
        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.LoadXml(resXML);
        XmlNodeList allocationsNodes = xmlDoc.FirstChild.SelectNodes("allocation");
        string hotelId = string.Empty;
        string allocationId = string.Empty;
		bool isFoundValidResult = false;
        if (allocationsNodes.Count > 0)
        {
            foreach (XmlNode aNode in allocationsNodes)
            {
                if (isNodeValid(aNode))
                {
					isFoundValidResult = true;
                    areaName = string.Empty;
                    hotelAttributes = string.Empty;
                    if (aNode.Attributes["has_treatment_in_hotel"].Value == "1") hotelAttributes += "קיים טיפול במלון<br/>";
                    if (aNode.Attributes["has_treatment_near_hotel"].Value == "1") hotelAttributes += "קיים טיפול ליד המלון<br/>";
                    if (aNode.Attributes["suitable_for_religious"].Value == "1") hotelAttributes += "מתאים לדתיים<br/>";
                    if (aNode.Attributes["suitable_for_wheel_chair"].Value == "1") hotelAttributes += "מתאים לכיסא גלגלים<br/>";
                    areaName = aNode.Attributes["area_name"].Value;

                    HtmlGenericControl newControl = new HtmlGenericControl("div");
                    newControl.ID = i.ToString();
                    rowIndex++;
                    newControl.Attributes.Add("class", "deal center-div");
                    string hotelName = aNode.Attributes["supplier_name"].Value;
                    newControl.InnerHtml = "<h2><label id='name" + rowIndex + "'>" + hotelName + "</h2>"
                                           + "<div class='img'>"
                                           + "<span class='tree'></span>"
                                           + "<img src='http://web14.agency2000.co.il/hotel_images/"
                                           + aNode.Attributes["image_name_1"].Value + "' alt='" + hotelName + " תמונה ' image='img' />"
                                           + "</div>"
                                           + "<div class='text'>"
                                           + "<h3 class='font-color'>" + areaName + "</h3>"
                                           + "<div class='description'>" + hotelAttributes + "</div>"
                                           + "</div>";
                    i++;

                    hotelId = aNode.Attributes["supplier_id"].Value;
					//If its Oazis hotel then show it.
                    if (!mIsDeadSeaMonthBefore || hotelId == "2050")
                    {
						int halfYear = 5;

						//if (mToDate.Month - DateTime.Now.Month <= halfYear)
						//{
							allocationId = aNode.Attributes["allocation_id"].Value;
							var orderButton = GetButtonTableCellButton(++rowIndex, hotelId, allocationId, traveller.FourOneSeven, hotelName, traveller.Melave.Count.ToString());
							newControl.Controls.Add(orderButton);
							PlaceHolder1.Controls.Add(newControl);
							
						//}
                    }
					lblMessage.Visible = false;
					
                }
				else
				{
					//string messageDeleted = "אין מלונות שתואמים את בקשתך <br/> אנא בחר תאריכים אחרים";
					//lblMessage.Text = messageDeleted;
					//lblMessage.Visible = true;
				}
            }
        }
        else
        {
            string messageDeleted = string.Format(@"אין מלונות שתואמים את בקשתך, אנא בחר תאריכים אחרים.</br>
            בגרסת הביטא ניתן להזמין רק בין התאריכים 1/3/17 עד 28/3/17.");
            lblMessage.Text = messageDeleted;
            lblMessage.Visible = true;
        }
		
		if (isFoundValidResult == false)
		{
			string messageDeleted = "אין מלונות שתואמים את בקשתך <br/> אנא בחר תאריכים אחרים";
			lblMessage.Text = messageDeleted;
			lblMessage.Visible = true;
			
		}
    }

    private bool isNodeValid(XmlNode iNode)
    {
        bool isNodeValid = false;
        try
        {
			
            string numberOfSelectedDates = txtNights.Text;
            int allocationId = int.Parse(iNode.Attributes["allocation_id"].Value.ToString());
            int supplierId = int.Parse(iNode.Attributes["supplier_id"].Value.ToString());
            List<BaseTypes> baseList = null;
            string errorMessage = string.Empty;

            if (iNode.HasChildNodes)
            {
                // number of dates is the number of selcted dates
                if ((iNode.ChildNodes.Count).ToString() == numberOfSelectedDates)
                {
					
                   // baseList = Agency2000Proxy.getAgencyBases(supplierId, allocationId);
                    foreach (BaseTypes hotelBase in baseList)
                    {
                        try
						{
							//Logger.Log(supplierId.ToString()+" "+ allocationId.ToString()+" "+ hotelBase.Id.ToString() +" "+ mFromDate.ToString()+" "+ mToDate.AddDays(1).ToString());
							//List<HotelPrice> hotelPrice = Agency2000Proxy.getHotelPrice(supplierId, allocationId, hotelBase.Id, mFromDate, mToDate.AddDays(1), traveller, out errorMessage);                        
						
							if (string.IsNullOrEmpty(errorMessage))
							{
								Logger.Log("hotelPrice - OK");
								isNodeValid = true;
							}
							else
							{
								Logger.Log("hotelPrice - not found" + errorMessage);
								errorMessage = string.Empty;
							}
						}
						catch(Exception exc)
						{
							Logger.Log("hotelPrice - exception = " + exc.Message);
							isNodeValid = false;
						}
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Logger.Log("AllocationSearch" + ex.Message);
        }
        return isNodeValid;
    }

    private bool validateSearch()
    {
        lblMessage.Visible = false;
		
        if (txtFromDate.Text.Length <= 0 || txtToDate.Text.Length <= 0 || txtNights.Text.Length <= 0)
        {
            lblMessage.Visible = true;
            //lblMessage.Text = "נא להזין תאריכים לחיפוש.";
            mClientMessage = "נא להזין תאריכים לחיפוש.";
            return false;
        }

		

		//traveller.SelectedMakat[0].ItemSKU == "030773" || 
		
        if (traveller.SelectedMakat[0].ItemSKU == "027240" || traveller.SelectedMakat[0].ItemSKU == "027238" ||
            traveller.SelectedMakat[0].ItemSKU == "030772")
        {

            TimeSpan ts1 = new TimeSpan();
            DateTime fromDate = DateTime.Parse(txtFromDate.Text);
            DateTime todayDate = DateTime.Today.Date;
            ts1 = (fromDate - todayDate);

            if (ts1.Days > daysBeforeMerhatzaot)
            {
				//if == 12 (12 - ים המלח) need to ignore month before.
                if (dGeneralAreaId.SelectedValue == "12")
                {
					mClientMessage = "מרחצאות חמי מרפא ניתן להזמין רק חודש וחצי לפני מועד ההזמנה";
					mIsDeadSeaMonthBefore = true;
					return false;
				}
            }
        }
        
        traveller.Balance = traveller.SelectedMakat[0].DaysNum - traveller.SelectedMakat[0].UsageBalance;

        TimeSpan ts = new TimeSpan();
		try{
        
        DateTime fromDate = DateTime.Parse(txtFromDate.Text);
        DateTime toDate = DateTime.Parse(txtToDate.Text);
        ts = (toDate - fromDate);

		
		//////////////////////Blocked dates - entitled year ///////////
		int halfYear = 6;
		bool checkHalfYear = true;                                                                    
		int nextEntitledYear = (DateTime.Now.Month < 3) ? DateTime.Now.Year : DateTime.Now.Year + 1;
		bool isNeedToBlock = false;
		int endOfNextEntitledYear = nextEntitledYear + 1;
		string dateToBlock = "10-Sep-17";
		//dateToBlock = "07-Sep-17";/////////////////////////////////////// TEST REMOVE ON PRODUCTION!!!!!!!!!!!!!!!
		if ((DateTime.Now.Date > DateTime.Parse(dateToBlock).Date) || (DateTime.Now.Date == DateTime.Parse(dateToBlock).Date && DateTime.Now.Hour > 7))
		{
			//Opens the new ENTITLED YEAR for 6 month (until september - const) when september starts will go to else
			if (!traveller.makatSelected("027236") && DateTime.Now.Date < DateTime.Parse("01-Mar-" + nextEntitledYear.ToString()).Date)
			{
				endOfNextEntitledYear = nextEntitledYear;
				
				DateTime xStartDateMax = DateTime.Parse("01-Sep-" + endOfNextEntitledYear.ToString());
				if (toDate.Date > xStartDateMax.Date)
				{
					lblMessage.Visible = true;
					mClientMessage = "תאריך תחילת הזמנה לא יכול להיות מאוחר מ(" + xStartDateMax.ToString("dd/MM/yyyy") + ")";
					return false;
				}
			}
			else
			{
				isNeedToBlock = true;
				endOfNextEntitledYear = nextEntitledYear + 1;
				
			}
		}
		else
		{
			isNeedToBlock = true;
			endOfNextEntitledYear = nextEntitledYear;
		}
		
		if (isNeedToBlock)
		{
				// Untill the end of the entitled year
				DateTime xStartDateMax = DateTime.Parse("01-Mar-" + endOfNextEntitledYear.ToString());
				if (toDate.Date > xStartDateMax.Date)
				{
					lblMessage.Visible = true;
					mClientMessage = "תאריך תחילת הזמנה לא יכול להיות מאוחר מ(" + xStartDateMax.ToString("dd/MM/yyyy") + ") ";
					return false;
				}
		}
		//////////////////////Blocked dates - entitled year - end///////////
		
        // validate Jerusalem balance
        if (int.Parse(dGeneralAreaId.SelectedValue) == 16) // Jerusalem balance
        {
            int balanceTipulim = 0;
            int balanceAstma = 0;

            if (traveller.makatSelected("027242") && (traveller.makatSelected("027236") || traveller.makatSelected("027235")))
            {
                if (traveller.getSelectedMakatByNumber("027236") != null)
                    balanceTipulim = traveller.getSelectedMakatByNumber("027236").JerusalemUsageBalance;
                else
                    balanceTipulim = traveller.getSelectedMakatByNumber("027235").JerusalemUsageBalance;
                balanceAstma = traveller.getSelectedMakatByNumber("027242").JerusalemUsageBalance;

                if ((balanceTipulim - balanceAstma) < ts.Days)
                {
                    lblMessage.Visible = true;
                    //lblMessage.Text = "לא ניתן להזמין יותר באזור ירושלים.";
                    mClientMessage = "לא ניתן להזמין יותר באזור ירושלים.";
                    return false;
                }
            }
        }
        // check 4+1 
        if ((traveller.makatSelected("027236") || traveller.makatSelected("027235")))
        {
            if (traveller.FourOneSeven.Equals("fourNightHotel5Tipulim"))
            {
                if (ts.Days == 4)
                {
                    traveller.IsAdded5thNight = false; //not one day on zakai
                    return true;
                }
                else
                {
                    lblMessage.Visible = true;
                    //lblMessage.Text = "יכול לבחור 4 לילות בלבד  ";
                    mClientMessage = "יכול לבחור 4 לילות בלבד  ";
                    return false;
                }
            }
            if (traveller.FourOneSeven.Equals("fiveNightHotelTipulim"))
            {
                if (ts.Days == 5)
                {
                    traveller.IsAdded5thNight = true; //one day on zakai
                    return true;
                }
                else
                {
                    lblMessage.Visible = true;
                    //lblMessage.Text = "יכול לבחור 5 לילות בלבד  ";
                    mClientMessage = "יכול לבחור 5 לילות בלבד  ";
                    return false;
                }
            }
        }
        // check 7+7 
        if (traveller.makatSelected("027241"))
        {
            if (traveller.FourOneSeven.Equals("sevenOnZakai"))
            {
                if (ts.Days != 14)
                {
                    lblMessage.Visible = true;
                    //lblMessage.Text = "חייב לבחור 14 לילות בלבד  ";
                    mClientMessage = "חייב לבחור 14 לילות בלבד  ";
                    return false;
                }
            }
        }
        // check 5th night for traveller payment
        if (traveller.Balance == 4 && traveller.getOrderMinNights() == 5 && ts.Days == 5) // check if allowed to add 1 night for traveller payment
        {
            if (!traveller.IsAllovedToAdd5thNight())
            {
                lblMessage.Visible = true;
                //lblMessage.Text = "יש יתרה של 4 לילות, לא ניתו להשלים לילה 5 ע''ח הלקוח.";
                mClientMessage = "יש יתרה של 4 לילות, לא ניתו להשלים לילה 5 ע''ח הלקוח.";
                return false;
            }
            else
            {
                traveller.IsAdded5thNight = true;
            }
        }
        else if (traveller.Balance < ts.Days)
        {
            if (!(traveller.makatSelected("027241") && traveller.FourOneSeven.Equals("sevenOnZakai")))
            {

                lblMessage.Visible = true;
                //lblMessage.Text = "לא ניתן להזמין ימים (" + ts.Days + ") כאשר יתרה (" + traveller.Balance.ToString() + ").";
                mClientMessage = "לא ניתן להזמין ימים (" + ts.Days + ") כאשר יתרה (" + traveller.Balance.ToString() + ").";
                return false;
            }
        }

            if (traveller.BalanceUssage && traveller.Balance < (ts.Days * 2)) // && ts.Days != 5) // must to search only 5 nights not less and not more (makat 40)
            {
                lblMessage.Visible = true;
                //lblMessage.Text = "נבחרה האופציה שימוש ביתרה של זכאי, אין מספיק יתרה להזמנה זו.";
                mClientMessage = "נבחרה האופציה שימוש ביתרה של זכאי, אין מספיק יתרה להזמנה זו.";
                return false;
            }

        if (!traveller.FourOneSeven.Equals("fiveNightHotelTipulim") && !traveller.FourOneSeven.Equals("fourNightHotel5Tipulim") && !traveller.FourOneSeven.Equals("sevenOnZakai") && (ts.Days < traveller.getOrderMinNights() && traveller.getOrderMinNights() != 0) || (ts.Days > traveller.getOrderMaxNights() && traveller.getOrderMaxNights() != 0))
        {
            lblMessage.Visible = true;
            //lblMessage.Text = "לא ניתן לבצע הזמנה למס לילות " + ts.Days + " מינימום לילות(" + traveller.getOrderMinNights().ToString() + ") ומקסימום (" + traveller.getOrderMaxNights().ToString() + ") ";
            mClientMessage = "לא ניתן לבצע הזמנה למס לילות " + ts.Days + " מינימום לילות(" + traveller.getOrderMinNights().ToString() + ") ומקסימום (" + traveller.getOrderMaxNights().ToString() + ") ";

            return false;
        }

        int startDateAddToTodayMin = traveller.getMinStartOrderDate();
        int startDateAddToTodayMax = traveller.getMaxStartOrderDate();
        // check start order date
        if (startDateAddToTodayMin != 0) // min limit
        {
            DateTime xStartDateMin = DateTime.Now.AddDays(startDateAddToTodayMin);
            if (fromDate < xStartDateMin)
            {
                lblMessage.Visible = true;
                //lblMessage.Text = "לא ניתן להתחיל הזמנה בתאריך נבחר. תאריך תחילת הזמנה לא פחות מ(" + xStartDateMin.ToString("dd/MMM/yyyy") + ")";
                mClientMessage = "לא ניתן להתחיל הזמנה בתאריך נבחר. תאריך תחילת הזמנה לא פחות מ(" + xStartDateMin.ToString("dd/MMM/yyyy") + ")";
                return false;
            }
        }
       /* Old not relevant
	    if (DateTime.Now < DateTime.Parse("16-May-17"))
		{
			// field Phone3Prefix use as  canceletion flag of  start date restriction
			if (startDateAddToTodayMax != 0 && traveller.Phone3Prefix.Trim() != "999") // max limit
			{
				DateTime xStartDateMax = DateTime.Now.AddDays(startDateAddToTodayMax);
				if (fromDate > xStartDateMax)
				{
					lblMessage.Visible = true;
					//lblMessage.Text = "תאריך תחילת הזמנה לא יכול להיות מאוחר מ(" + xStartDateMax.ToString("dd/MMM/yyyy") + ")";
					mClientMessage = "תאריך תחילת הזמנה לא יכול להיות מאוחר מ(" + xStartDateMax.ToString("dd/MMM/yyyy") + ")";
					return false;
				}
			}
		}
		else
		{
			DateTime xStartDateMax = DateTime.Parse("01-Mar-18");
			if (toDate > xStartDateMax)
			{
				lblMessage.Visible = true;
				//lblMessage.Text = "תאריך תחילת הזמנה לא יכול להיות מאוחר מ(" + xStartDateMax.ToString("dd/MMM/yyyy") + ")";
				mClientMessage = "תאריך תחילת הזמנה לא יכול להיות מאוחר מ(" + xStartDateMax.ToString("dd/MMM/yyyy") + ")";
				return false;
			}
		}
		*/
        if (traveller.SelectedMakat[0].EndDate < toDate)
        {
            lblMessage.Visible = true;
            //lblMessage.Text = "תאריך סיום הזמנה לא יכול להיות גדול מתאריך סיום זכאות (" + traveller.SelectedMakat[0].EndDate.ToShortDateString() + ").";
            mClientMessage = "תאריך סיום הזמנה לא יכול להיות גדול מתאריך סיום זכאות (" + traveller.SelectedMakat[0].EndDate.ToShortDateString() + ").";
            return false;
        }
		
				bool isHoliday = false;
		for (int i = 0; i < ts.Days; i++)
		{
			if (CheckHolidays(fromDate.AddDays(i)))
			{
				isHoliday = true;
			}
		}
		if (isHoliday == true)
		{
			lblMoneyHoliday.Visible = true;
		}
		else
		{
			lblMoneyHoliday.Visible = false;
		}
		
		}
        catch (Exception ex)
        {
            Logger.Log("2" + ex.Message);
            txtFromDate.Text = string.Empty;
            txtToDate.Text = string.Empty;
            txtNights.Text = string.Empty;
			if (string.IsNullOrEmpty(dGeneralAreaId.SelectedValue))
			{
				mClientMessage = "יש להיכנס למודול שיקום מחדש";
			}
			else
			{
				if (ex is FormatException)
				{
					mClientMessage = "אנא השתמש בבוחר התאריכים על מנת להזין תאריך";
				}
				else
				{
					mClientMessage = "אירעה בעיה, אנא נסו שנית";
				}
			}

            return false;
        }
        return true;
    }

    private static TableHeaderCell GetTextTableHeaderCell(string text, bool selecredCell)
    {
        TableHeaderCell thc = new TableHeaderCell();
        thc.Text = text;
        if (selecredCell)
        {
            //thc.BackColor = System.Drawing.Color.GreenYellow; //thc.BorderColor = System.Drawing.Color.Red; thc.BorderWidth = 3;
            thc.BackColor = System.Drawing.Color.Orange;
        }
        return thc;
    }
    //private static TableCell GetTextTableCell(string text, bool selecredCell,DateTime d)
    //{
    //    TableCell thc = new TableCell();
    //    thc.Text = text;
    //    if (selecredCell)
    //    {
    //        if (d >= new DateTime(2013, 3, 25) && d <= new DateTime(2013, 4, 1) || d >= new DateTime(2013, 5, 14) && d <= new DateTime(2013, 5, 16))
    //            thc.BackColor = System.Drawing.Color.Orange;
    //        else
    //        thc.BackColor = System.Drawing.Color.GreenYellow;
    //    }
    //    return thc;
    //}
    private static TableCell GetTextTableCell(string text, bool selecredCell, string color, string CssClass)
    {
        TableCell thc = new TableCell();
        thc.Text = text;
        if (selecredCell)
        {
            if (color != "")
                thc.BackColor = System.Drawing.ColorTranslator.FromHtml(color);
            else
                thc.BackColor = System.Drawing.Color.White;
        }
        else thc.BackColor = GetLightColor(color);
        if (CssClass != "")
            thc.CssClass = CssClass;
        return thc;
    }
    private static TableCell GetButtonTableCellButton(int rowIndex, string hotelId, string allocationId,
                                                       string FourOneSeven, string hotelName, string numOfEscorts)
    {
        TableCell tc = new TableCell();
        Label getPriceBaseTypesBtn = new Label();
        Literal baseTypesSelectorCrt = new Literal();
        Literal hotelPriceAmountField = new Literal();
        Literal orderButton = new Literal();

        string baseTypesSelectorCtrName = "baseSelectorSelect" + rowIndex.ToString();
        string hotelPriceAmountFieldCtrName = "priceAmount" + rowIndex.ToString();
        string orderButtonCtrName = "orderButtonCtrName" + rowIndex.ToString();

        tc.Wrap = false;

        // button to retrieve base types
        getPriceBaseTypesBtn.ID = "baseSelectorButton" + rowIndex;
        getPriceBaseTypesBtn.CssClass = "baseSelectorButton";
        getPriceBaseTypesBtn.Attributes.Add("rowIndex", rowIndex.ToString());
        getPriceBaseTypesBtn.Attributes.Add("hotelId", hotelId);
        getPriceBaseTypesBtn.Attributes.Add("allocationId", allocationId);
        getPriceBaseTypesBtn.Attributes.Add("hotelname", hotelName);
        getPriceBaseTypesBtn.Attributes.Add("escorts", numOfEscorts);
        getPriceBaseTypesBtn.Attributes.Add("FourOneSeven", FourOneSeven);

        getPriceBaseTypesBtn.Text = "להמשך הזמנה";

        // attach selector to the button
        getPriceBaseTypesBtn.Attributes.Add("selectorControlName", baseTypesSelectorCtrName);

        // base selector control
        baseTypesSelectorCrt.Text = "<div class='left'><div class='price'>&nbsp;<select class='" + baseTypesSelectorCtrName + "' style='display:none' hotelPriceAmountFieldName='" + hotelPriceAmountFieldCtrName + "'></select></div>";
        // field to show price amount
        hotelPriceAmountField.Text = "&nbsp;<span class='" + hotelPriceAmountFieldCtrName + "' style='display:none;' orderButtonCtrName='" + orderButtonCtrName + "'></span>";
        // order button
        orderButton.Text = "&nbsp;<span class='" + orderButtonCtrName + " order_button res' style='display:none;' >הזמן</span></div>";

        tc.Controls.Add(getPriceBaseTypesBtn);
        tc.Controls.Add(baseTypesSelectorCrt);
        tc.Controls.Add(hotelPriceAmountField);
        tc.Controls.Add(orderButton);

        return tc;
    }

    private static TableCell GetButtonTableCell(int rowIndex, string hotelId, string allocationId, string FourOneSeven)
    {
        TableCell tc = new TableCell();
        Label getPriceBaseTypesBtn = new Label();
        Literal baseTypesSelectorCrt = new Literal();
        Literal hotelPriceAmountField = new Literal();
        Literal orderButton = new Literal();

        string baseTypesSelectorCtrName = "baseSelectorSelect" + rowIndex.ToString();
        string hotelPriceAmountFieldCtrName = "priceAmount" + rowIndex.ToString();
        string orderButtonCtrName = "orderButtonCtrName" + rowIndex.ToString();

        tc.Wrap = false;

        // button to retrieve base types
        getPriceBaseTypesBtn.ID = "baseSelectorButton" + rowIndex;
        getPriceBaseTypesBtn.CssClass = "baseSelectorButton";
        getPriceBaseTypesBtn.Attributes.Add("rowIndex", rowIndex.ToString());
        getPriceBaseTypesBtn.Attributes.Add("hotelId", hotelId);
        getPriceBaseTypesBtn.Attributes.Add("allocationId", allocationId);
        getPriceBaseTypesBtn.Attributes.Add("FourOneSeven", FourOneSeven);


        getPriceBaseTypesBtn.Text = "בחר אירוח";

        // attach selector to the button
        getPriceBaseTypesBtn.Attributes.Add("selectorControlName", baseTypesSelectorCtrName);
        // base selector control
        baseTypesSelectorCrt.Text = "&nbsp;<select class='" + baseTypesSelectorCtrName + "  baseSelectorSelect' style='display:none; width:38% !important;' hotelPriceAmountFieldName='" + hotelPriceAmountFieldCtrName + "'></select>";
        // field to show price amount
        hotelPriceAmountField.Text = "&nbsp;<span class='" + hotelPriceAmountFieldCtrName + "' style='display:none;' orderButtonCtrName='" + orderButtonCtrName + "'></span>";
        // order button
        orderButton.Text = "&nbsp;<span class='" + orderButtonCtrName + " order_button' style='display:none;'>הזמן</span>";

        tc.Controls.Add(getPriceBaseTypesBtn);
        tc.Controls.Add(baseTypesSelectorCrt);
        tc.Controls.Add(hotelPriceAmountField);
        tc.Controls.Add(orderButton);

        return tc;
    }

    private bool CheckHolidays(DateTime dateCheck)
    {
        foreach (DataRow row in dsHolidays.Tables[0].Rows)
        {
            //Logger.Log("CheckHolidays date : line 696 , allocationSearch.aspx.cs :: " + row["HolidayDate"].ToString());
            DateTime Holiday = new DateTime();
            DateTime.TryParse(row["HolidayDate"].ToString(), out Holiday);

            if (DateTime.Equals(dateCheck, Holiday))
            {
                return true;
            }
        }
        return false;
    }
    public static System.Drawing.Color GetLightColor(string color)
    {
        System.Drawing.Color lightcolor = System.Drawing.Color.White;
        switch (color)
        {
            case "#FFFFFF":
                lightcolor = System.Drawing.Color.White;
                break;
            case "#FF0000": ///red
                lightcolor = System.Drawing.ColorTranslator.FromHtml("#FF6666");//System.Drawing.Color.Pink;
                break;
            case "#FFFF00": //yellow
                lightcolor = System.Drawing.ColorTranslator.FromHtml("#FFFF99");
                break;
            case "#008000": //green
                lightcolor = System.Drawing.Color.LightGreen;
                break;
            default:
                lightcolor = System.Drawing.Color.White;

                break;

        }
        return lightcolor;
    }

	protected void btGoToAllocationSearchSplit_Click(object sender, EventArgs e)
    {
        Response.Redirect("./allocationsearchsplit.aspx");
    }

	protected void btGoToAllHotels_Click(object sender, EventArgs e)
    {
        Response.Redirect("./allocationsearchglobal.aspx");
    }
}
