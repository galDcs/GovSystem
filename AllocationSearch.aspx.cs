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
	int rowIndex = 0;
    public GovTraveller traveller;
    private DataSet dsHolidays;
    private string mClientMessage = string.Empty;
    public string mAddId = "";
	public DateTime mFromDate;
	public DateTime mToDate;
	public bool mIsDeadSeaMonthBefore = false;
	public int daysBeforeMerhatzaot = 45;
	public bool isErrorDetected = false;
	public string EntitledYear = "2017";//@EY
	AgencyGovConnector.AgencyGovConnector connector = new AgencyGovConnector.AgencyGovConnector();

	protected void Page_Load(object sender, EventArgs e)
    {
        try
        {
           // if (!Utils.CheckSecurity(225)) Response.Redirect("AccessDenied.aspx");
            if (Session["ClientMessage"] != null)
            {
                string messageDeleted = Session["ClientMessage"].ToString();
                ClientScript.RegisterStartupScript(this.GetType(), "Popup", "ShowPopup('" + messageDeleted + "');", true);
                Session["ClientMessage"] = null;
            }

            lblMessage.Visible = false;
            traveller = GovTraveller.LoadFromSession();
			EntitledYear = traveller.EntitledYear;// @EY
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
			switch (erkev_enum)
			{
				case ErkevTypes.Zakai:
					lblSugErkev.Text = "זכאי";
					break;
				case ErkevTypes.ZakaiAndMelave:
					lblSugErkev.Text = "זכאי + מלווה";
					break;
				case ErkevTypes.ZakaiAnd2Melavim:
					lblSugErkev.Text = "זכאי + 2 מלווים";
					break;
				case ErkevTypes.ZakaiAndMelaveBeTashlum:
					lblSugErkev.Text = "זכאי + מלווה בתשלום לכל התקופה";
					break;
				case ErkevTypes.ZakaiAndMelaveBeTashlumHelekTkufa:
					lblSugErkev.Text = "זכאי + מלווה בתשלום לתקופה חלקית";
					break;
				case ErkevTypes.ZakaiBeTashlumAndMelaveLeLoTashlum:
					lblSugErkev.Text = "זכאי בתשלום + מלווה ללא תשלום";
					break;
				case ErkevTypes.ZakaiAndMelaveTmuratZakaut:
					lblSugErkev.Text = "זכאי ומלווה שניהם ללא תשלום תמורת ניצול כפול של ימי זכאות(5+5)";
					break;
			}
			lblSugErkev.Text = "<b><u>הרכב בפועל</u>:</b> " + lblSugErkev.Text;
			
			if (traveller.FourOneSeven.Equals("fourNightHotel5Tipulim"))
			{
				lblMoneySKU.Visible = true;
			}
			lbLastDateToOrder.Text = "<br/>" + traveller.LastDateToOrder.ToString("dd-MMM-yy");
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
					//Logger.Log("Failed to get add_id ,Exception = " + ex1.Message);
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
					//Logger.Log("Failed to get holiday ,Exception = " + ex2.Message);
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
            Logger.Log("aa - Exception mesaage = " + ex.Message);
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
		DateTime originFromDate1 = DateTime.Parse(txtFromDate.Text);
		//if (originFromDate1 < (new DateTime (2020, 6, 1)))
		//{
		//	mClientMessage = "תאריך תחילה חייב להיות אחרי 01.06.2020 עקב סגירת בתי המלון והנחיית אגף השיקום";
		//	ClientScript.RegisterStartupScript(this.GetType(), "Popup", "ShowPopup('" + mClientMessage + "');", true);
		//	return;
		//}
		try
		{
			{
				//if ((!validateSearch() || !isVacationOnSameDates()) && (traveller.TravellerId != "013340336") && (traveller.TravellerId != "013340336"))
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
					if ( !string.IsNullOrEmpty(dateFrom))
					{
						DateTime originFromDate = DateTime.Parse(txtFromDate.Text);
						DateTime originToDate = DateTime.Parse(txtToDate.Text);
						//aviran - changed the 2 days before and after range
						DateTime fromDate = DateTime.Parse(txtFromDate.Text);
						DateTime toDate = DateTime.Parse(txtToDate.Text).AddDays(-1);
						
						mFromDate = fromDate;
						mToDate = toDate;
			
						TableRow daysRow = new TableRow();
						TableRow checkBoxexDays = new TableRow();
						TableCell cell = null;
						
						DateTime currDate = originFromDate;
						while (currDate.AddDays(1).Date != originToDate.AddDays(1).Date)
						{
							cell = new TableCell();
							cell.Text = currDate.ToString("dd/MM");
							daysRow.Cells.Add(cell);
							
							CheckBox chkbx = new CheckBox() { Checked = true };
							cell = new TableCell();
							cell.Controls.Add(chkbx);
							checkBoxexDays.Cells.Add(cell);
							currDate = currDate.AddDays(1);
						}
						
						tblAllocations.Rows.Add(daysRow);
						tblAllocations.Rows.Add(checkBoxexDays);
						//Response.Write(traveller.ErkevType);
						if (traveller.ErkevType.Equals("ZakaiBeTashlumAndMelaveLeLoTashlum"))//ZakaiAndMelaveBeTashlumHelekTkufa
						{
							//tblAllocations.Style["display"] = "block";
						}
						
						ts = (originFromDate - originToDate);
						lblFd.Text = txtFromDate.Text;
						lblTd.Text = txtToDate.Text;
						lblN.Text = Math.Abs(ts.Days).ToString();
						int countdaysSeacrch = Convert.ToInt32(txtNights.Text);//ts.Days;
						string resXML = string.Empty;
						
						bool isOnlyTreatments = traveller.SelectedMakat[0].ItemSKU =="029940";
						try
						{
							if (!isOnlyTreatments)
							{
								//Logger.Log(dGeneralAreaId.SelectedValue + fromDate + toDate + traveller.SelectedMakat[0].MakatTipulim + (1 + traveller.Melave.Count));
								resXML = DAL_SQL_Helper.SearchAvailableAllocations(int.Parse(dGeneralAreaId.SelectedValue),
																			fromDate, toDate,
																			traveller.SelectedMakat[0].MakatTipulim,
																			(1 + traveller.Melave.Count));	
								//Logger.Log("resXML = " + resXML);
							}
							else
							{
								resXML = DAL_SQL_Helper.SearchAvailableAttractions(int.Parse(dGeneralAreaId.SelectedValue),
																			fromDate, toDate,
																			traveller.SelectedMakat[0].MakatTipulim,
																			(1 + traveller.Melave.Count));	
							}
							
							Logger.Log(resXML);
						}
						catch(Exception ex3)
						{
							isErrorDetected = true;
							Session["ClientMessage"] = "אירעה שגיאה בחיפוש המלונות";
							Logger.Log("Failed to search ,Exception = " + ex3.Message);
						}
						
						/*chen - new function that build the availble hotel views */
						bool isNewPrices = false;
						//Only deadsea and tiberias are in new prices. //08-Apr-18.
						if ((dGeneralAreaId.SelectedValue == "14" ||
							dGeneralAreaId.SelectedValue == "15" ||
							dGeneralAreaId.SelectedValue == "16")
							&&
							toDate.AddDays(1) < DateTime.Parse("01-Mar-2019 09:00:00"))
						{
							buildHotelViews(resXML, isNewPrices);
						}
						else
						{
							//New prices.
							AjaxService ajaxWs = new AjaxService();
							isNewPrices = true;
							buildHotelViews(resXML, isNewPrices);
						}
					}
				}
			}
		}
		catch(Exception ex){
			Logger.Log("bb - Exception message = " + ex.Message);
			Logger.Log("bb - Exception StackTrace = " + ex.StackTrace);
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
		// TreatmentEntitledServiceClient treatmentClient = new TreatmentEntitledServiceClient();
		// string accessToken = Session["accessToken"].ToString();
		string accessToken = connector.GetAccessToken(traveller.DocketId);
        AgencyGovConnector.ServiceEntitledResponse responseEntitled = connector.GetTreatmentDetails(accessToken);
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

		//if (traveller.TravellerId != "058782111")
        foreach (AgencyGovConnector.EntitledOrder order in responseEntitled.EntitledOrders)
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
    private void buildHotelViews(string resXML, bool iIsNewPrices)
    {
        //XmlNodeList allocationsNodes = iXmlDoc.GetElementsByTagName("root/allocation");
        string hotelAttributes = string.Empty;
        string areaName = string.Empty;
		string supplierId = string.Empty;

        int i = 0;
        XmlDocument xmlDoc = new XmlDocument();
        xmlDoc.LoadXml(resXML);
        XmlNodeList allocationsNodes = xmlDoc.FirstChild.SelectNodes("allocation");
        string hotelId = string.Empty;
        string allocationId = string.Empty;
		bool isFoundValidResult = false;
		
		bool isOnlyTreatments = traveller.SelectedMakat[0].ItemSKU == "029940";
        if (allocationsNodes.Count > 0)
        {
            foreach (XmlNode aNode in allocationsNodes)
            {
                if (isNodeValid(aNode, iIsNewPrices))
                {
					supplierId = aNode.Attributes["supplier_id"].Value; 
					if (!iIsNewPrices)
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
					string hotelNameExtraText = string.Empty;
					if (isOnlyTreatments){
						try{
						supplierId = getMarpeSupplierId(supplierId);
						hotelNameExtraText = " <span style='font-size:8px'>טיפול זה מוצע גם בהזמנות ל <span style='color:#0000ff;'>" + hotelName + "</span></span>";
						hotelName = DAL_SQL.GetRecord("Agency_Admin.dbo.SUPPLIERS", "name_1255", supplierId, "id");
						}
						catch(Exception exFactor)
						{
							Response.Write(exFactor.Message + " " + supplierId);
						}
					}
                    
					newControl.InnerHtml = "<h2><label id='name" + rowIndex + "'>" + hotelName + " " + hotelNameExtraText + "</h2>"
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
								var orderButton = GetButtonTableCellButton(++rowIndex, hotelId, allocationId, traveller.FourOneSeven, hotelName, traveller.Melave.Count.ToString(), iIsNewPrices);
								newControl.Controls.Add(orderButton);
								PlaceHolder1.Controls.Add(newControl);
								
							//}
						}
						lblMessage.Visible = false;
					}
					else
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
						
						string hotelNameExtraText = string.Empty;
						if (isOnlyTreatments){
							try{
							supplierId = getMarpeSupplierId(supplierId);
							hotelNameExtraText = " <span style='font-size:8px'>טיפול זה מוצע גם בהזמנות ל <span style='color:#0000ff;'>" + hotelName + "</span></span>";
							hotelName = DAL_SQL.GetRecord("Agency_Admin.dbo.SUPPLIERS", "name_1255", supplierId, "id");
							}
							catch(Exception exFactor)
							{
								Response.Write(exFactor.Message + " " + supplierId);
							}
						}
						newControl.InnerHtml = "<h2><label id='name" + rowIndex + "'>" + hotelName + " " + hotelNameExtraText + "</h2>"
											   + "<div class='img'>"
											   + "<span class='tree'></span>"
											   + "<img src='http://web14.agency2000.co.il/hotel_images/"
											   + aNode.Attributes["image_name_1"].Value + "' alt='images/' image='img' />"//" + hotelName + " תמונה 
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
								var orderButton = GetButtonTableCellButton(++rowIndex, hotelId, allocationId, traveller.FourOneSeven, hotelName, traveller.Melave.Count.ToString(), iIsNewPrices);
								DateTime fromDate = DateTime.Parse(txtFromDate.Text);
								DateTime today = DateTime.Now;
								//Response.Write("(toDate - fromDate).Days"+ (fromDate - today).Days);
								//Response.Write("hotelId" + hotelId);
								//if ((hotelId == "18977" && (fromDate - today).Days < 15) || 
								//	(fromDate - today).Days >= 15)
								//{
									newControl.Controls.Add(orderButton);
								//}
								PlaceHolder1.Controls.Add(newControl);
								
							//}
						}
						lblMessage.Visible = false;
					}
					
					
					
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
		else
		{
			lblMessage.Text = string.Empty;
			lblMessage.Visible = false;
		}
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
			//case eHotels.TiberiasLeonardoClub:
			//	hotel = eHotels.TiberiasLeonardoClub;
			//	break;

			case eHotels.TiberiasLeonardoPlaza:
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

    private bool isNodeValid(XmlNode iNode, bool isNewPrices)
   {
        bool isNodeValid = false;
		
		//Spa village hamat gader is disabled for makat35
		if (iNode.Attributes["supplier_id"].Value == "2487" && traveller.SelectedMakat[0].ItemSKU == "027235")
		{
			return false;
		}
		
        try
        {
            string numberOfSelectedDates = txtNights.Text;
            int allocationId = int.Parse(iNode.Attributes["allocation_id"].Value.ToString());
            int supplierId = int.Parse(iNode.Attributes["supplier_id"].Value.ToString());
            List<BaseTypes> baseList = null;
            string errorMessage = string.Empty;
	    	bool isOnlyTreatments = traveller.SelectedMakat[0].ItemSKU == "029940";
			string newPriceStr = (isNewPrices) ? "1" : "0";

			//if (supplierId == 306)
			//{
			//	newPriceStr = "0";
			//	isNewPrices = false;
			//}
            if (iNode.HasChildNodes)
            {
                // number of dates is the number of selcted dates
				if ((iNode.ChildNodes.Count).ToString() == numberOfSelectedDates)
                {
					if (!isOnlyTreatments)
					{
						baseList = Agency2000Proxy.getAgencyBases(supplierId, mFromDate, mToDate.AddDays(1));
						if (baseList.Count == 1)
						{
							try
							{
								List<HotelPrice> hotelPrice = null;
								newPriceStr = "1";
								hotelPrice = Agency2000Proxy.getHotelPrice(supplierId, allocationId, baseList[0].Id, mFromDate, mToDate.AddDays(1), traveller, out errorMessage, newPriceStr,null);
								if (string.IsNullOrEmpty(errorMessage))
								{
									isNodeValid = true;
								}
								else
								{
									isNodeValid = false;
									errorMessage = string.Empty;
								}
							}
							catch (Exception exc)
							{
								Logger.Log("hotelPrice - exception = " + exc.Message);
								Logger.Log("hotelPrice - Trace = " + exc.StackTrace);
								isNodeValid = false;
							}
						}
						else if (baseList.Count > 1)
                        {
							try
							{
								List<HotelPrice> hotelPrice = null;
								newPriceStr = "1";
								hotelPrice = Agency2000Proxy.getHotelPrice(supplierId, allocationId, -1, mFromDate, mToDate.AddDays(1), traveller, out errorMessage, newPriceStr, getBaseIdsFromBaseTypes(baseList));
								if (string.IsNullOrEmpty(errorMessage))
								{
									isNodeValid = true;
								}
								else
								{
									isNodeValid = false;
									errorMessage = string.Empty;
								}
							}
							catch (Exception exc)
							{
								Logger.Log("hotelPrice - exception = " + exc.Message);
								Logger.Log("hotelPrice - Trace = " + exc.StackTrace);
								isNodeValid = false;
							}
						}
					}
					else
					{
						try
						{
							supplierId = int.Parse(getMarpeSupplierId(supplierId.ToString()));
							DataSet ds = DAL_SQL_Helper.GOV_GetAttractionResultsForOnlyTreatment(supplierId, mFromDate, mToDate);
							if (Utils.isDataSetRowsNotEmpty(ds))
							{
								if (ds.Tables[0].Rows.Count == (mToDate - mFromDate).Days)
								{
									isNodeValid = true;
								}
							}
							else
							{
								Response.Write("5");
							}
						}
						catch(Exception ex)
						{
							Response.Write(ex.Message);
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
	

	private List<int> getBaseIdsFromBaseTypes(List<BaseTypes> iBaseList)
    {
		List<int> retVal = new List<int>();
		foreach(BaseTypes baseType in iBaseList)
        {
			retVal.Add(baseType.Id);
		}
		return retVal;

	}
	
	private bool isDateInEntitledYear(DateTime? iOrderStartDate)// @EY
    {
        bool isInEntitledYear = false;
        DateTime orderStartDate;
        int selectedYear = int.Parse(EntitledYear);
		int coronaYear = 2020;//@corona
		
        orderStartDate = DateTime.Parse(iOrderStartDate != null ? iOrderStartDate.Value.ToString("dd-MMM-yy") : DateTime.Now.AddYears(-100).ToString());
		
		if (selectedYear == coronaYear)//@corona
		{
			if ((orderStartDate.Date.Month < 9 && orderStartDate.Date.Year == selectedYear + 1) || (orderStartDate.Date.Month >= 3 && orderStartDate.Date.Year == selectedYear + 1))
			{
				isInEntitledYear = true;
			}
		}
		else
		{
			if ((orderStartDate.Date.Month < 3 && orderStartDate.Date.Year == selectedYear + 1) || (orderStartDate.Date.Month >= 3 && orderStartDate.Date.Year == selectedYear))
			{
				isInEntitledYear = true;
			}
		}
		
		if (orderStartDate.Date == DateTime.Now.AddYears(-100).Date)
		{
			isInEntitledYear = false;
		}

        return isInEntitledYear;
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
		
		if (!isDateInEntitledYear(fromDate))//@EY
		{
			mClientMessage = "תאריך תחילה לא בשנת הזכאות";
			return false;
		}

		if (!isDateInEntitledYear(toDate.AddDays(-1)))
			
		{
			mClientMessage = "תאריך סיום לא בשנת הזכאות";
			return false;
		}
		
		if (traveller.SelectedMakat[0].EndDate.AddDays(1) < toDate)
        {
            lblMessage.Visible = true;
            //lblMessage.Text = "תאריך סיום הזמנה לא יכול להיות גדול מתאריך סיום זכאות (" + traveller.SelectedMakat[0].EndDate.ToShortDateString() + ").";
            mClientMessage = "תאריך סיום הזמנה לא יכול להיות גדול מתאריך סיום זכאות (" + traveller.SelectedMakat[0].EndDate.ToShortDateString() + ").";
            return false;
        }
		
		//////////////////////Blocked dates - entitled year ///////////
		//int halfYear = 6;
		//bool checkHalfYear = true;                                                                    
		//int nextEntitledYear = (DateTime.Now.Month < 4 || (DateTime.Now.Month == 4 && DateTime.Now.Day < 10) || (DateTime.Now.Month == 4 && DateTime.Now.Day == 10 && DateTime.Now.Hour < 9) ) ? DateTime.Now.Year : DateTime.Now.Year + 1;
		//bool isNeedToBlock = false;
		//int endOfNextEntitledYear = nextEntitledYear + 1;
		////to allow 100 travellers to order for the next 6 months
		//string dateToBlock = "10-Sep-17";
		//bool isOneHundred = false;
		//if (!string.IsNullOrEmpty(traveller.Simon_100))
		//{
		//	if (traveller.Simon_100=="1")
		//	{
		//		isOneHundred = true;
		//	}
		//}
		//
		//string not100DateToOpenAllYear = "10-Apr-18";
		//	//Opens the new ENTITLED YEAR for 6 month (until september - const) when september starts will go to else
		//if ((DateTime.Now.Date > DateTime.Parse(dateToBlock).Date) || (DateTime.Now.Date == DateTime.Parse(dateToBlock).Date && DateTime.Now.Hour > 7))
		//{
		//	// until september
		//	if (!isOneHundred && DateTime.Now.Date < DateTime.Parse(not100DateToOpenAllYear).Date)
		//	{
		//		isNeedToBlock = false;
		//		endOfNextEntitledYear = nextEntitledYear;
		//		
		//		DateTime xStartDateMax = DateTime.Parse("01-Sep-18");
		//		if (toDate.Date > xStartDateMax.Date)
		//		{
		//			lblMessage.Visible = true;
		//			mClientMessage = "תאריך תחילת הזמנה לא יכול להיות מאוחר מ(" + xStartDateMax.ToString("dd/MM/yyyy") + ")";
		//			return false;
		//		}
		//	}
		//	else
		//	{
		//		isNeedToBlock = true;
		//		endOfNextEntitledYear = nextEntitledYear + 1;
		//		
		//	}
		//}
		//else
		//{
		//	isNeedToBlock = true;
		//	endOfNextEntitledYear = nextEntitledYear;
		//}
		//
		//if (isNeedToBlock)
		//{
		//		// Untill the end of the entitled year
		//		DateTime xStartDateMax = DateTime.Parse("01-Mar-" + endOfNextEntitledYear.ToString());
		//		if (toDate.Date > xStartDateMax.Date)
		//		{
		//			lblMessage.Visible = true;
		//			mClientMessage = "תאריך תחילת הזמנה לא יכול להיות מאוחר מ(" + xStartDateMax.ToString("dd/MM/yyyy") + ") ";
		//			return false;
		//		}
		//}
		//////////////////////Blocked dates - entitled year - end///////////
        //DateTime openDateNewYear = DateTime.Parse("12-Dec-18 9:00:00");//user choice
        //DateTime openDateMidYear = DateTime.Parse("07-Apr-19 9:00:00");//user choice
		
		//const string newLastDatePrefix = "01-Mar-";
        //const string midddleLastDatePrefix = "01-Sep-";
        //string lastDateStr = string.Empty;
        DateTime lastDate;
		//DateTime nowDate = DateTime.Now;
		//bool is100 = false;
		////is100 will get true if got from shikum true, or makat 36 or makat 41 
		//if (!string.IsNullOrEmpty(traveller.Simon_100))
		//{
		//	if (traveller.Simon_100=="1")
		//	{
		//		is100 = true;
		//	}
		//}
		//else
		//{
		//	if (traveller.SelectedMakat[0].ItemSKU == "027236" || traveller.SelectedMakat[0].ItemSKU == "027241")
		//	{
		//		is100 = true;
		//	}
		//}
		//
		//if (nowDate >= openDateNewYear)
        //{
        //    if (is100 || nowDate >= openDateMidYear)
        //    {
		//		//open full entitled year
        //        lastDateStr = newLastDatePrefix + (int.Parse(EntitledYear) + 1);
		//		//Response.Write("1<br />");
        //    }
        //    else//not 100
        //    {
		//		//open half entitled year
        //        lastDateStr = midddleLastDatePrefix + (int.Parse(EntitledYear)); // + 1
		//		//Response.Write("2<br />");
        //    }
        //}
        //else
        //{
        //    //open full entitled year
        //    lastDateStr = newLastDatePrefix + (int.Parse(EntitledYear) + 1);
		//	//Response.Write("3<br />");
        //}

		//Response.Write(lastDateStr + "<br />");
        //lastDate = DateTime.Parse(lastDateStr);
		lastDate = traveller.LastDateToOrder;
        Response.Write("EntitledYear = " + EntitledYear);
        Response.Write("toDate = " + toDate);
        if (toDate.Date > lastDate && EntitledYear == "2021" ||
		    EntitledYear == "2020" && toDate.Date >= DateTime.Parse("11-Sep-21"))// && toDate.Date <= DateTime.Parse("05-Sep-21"))//@EY)
        {
			lblMessage.Visible = true;
			mClientMessage = "תאריך תחילת הזמנה לא יכול להיות מאוחר מ(" + lastDate.ToString("dd/MM/yyyy") + ") ";
		
			return false;
        }
		
		
		//if ((!is100 && traveller.SelectedMakat[0].ItemSKU !="027236" && traveller.SelectedMakat[0].ItemSKU !="027241") && toDate.Date >= openDateMidYear)
		//{
		//	lblMessage.Visible = true;
		//	
		//	mClientMessage = "לא ניתן כעת להזמין אחרי ספטמבר";
		//	return false;
		//}
		
        int startDateAddToTodayMin = traveller.getMinStartOrderDate();
        int startDateAddToTodayMax = traveller.getMaxStartOrderDate();
		
        // check start order date
        //if (startDateAddToTodayMin != 0) // min limit
        {
            DateTime xStartDateMin = DateTime.Now.AddDays(startDateAddToTodayMin);
			//Response.Write(fromDate);
			//Response.Write(xStartDateMin);
            if (fromDate.Date < xStartDateMin.Date) // && dGeneralAreaId.SelectedValue != "13")
            {
                lblMessage.Visible = true;
                //lblMessage.Text = "לא ניתן להתחיל הזמנה בתאריך נבחר. תאריך תחילת הזמנה לא פחות מ(" + xStartDateMin.ToString("dd/MMM/yyyy") + ")";
                mClientMessage = "לא ניתן להתחיל הזמנה בתאריך נבחר. תאריך תחילת הזמנה לא פחות מ(" + xStartDateMin.ToString("dd/MMM/yyyy") + ")";
                return false;
            }
        }
			
		//if ((!is100 && traveller.SelectedMakat[0].ItemSKU !="027236" && traveller.SelectedMakat[0].ItemSKU !="027241") && toDate.Date >= DateTime.Parse("01-SEP-19"))
		//{
		//	lblMessage.Visible = true;
		//	
		//	mClientMessage = "לא ניתן כעת להזמין אחרי ספטמבר";
		//	return false;
		//}
		
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
    private TableCell GetButtonTableCellButton(int rowIndex, string hotelId, string allocationId,
                                                       string FourOneSeven, string hotelName, string numOfEscorts, bool isNewPrices)
    {
        TableCell tc = new TableCell();
        Label getPriceBaseTypesBtn = new Label();
        Literal baseTypesSelectorCrt = new Literal();
        Literal hotelPriceAmountField = new Literal();
        Literal orderButton = new Literal();

        string baseTypesSelectorCtrName = "baseSelectorSelect" + rowIndex.ToString();
        string hotelPriceAmountFieldCtrName = "priceAmount" + rowIndex.ToString();
        string orderButtonCtrName = "orderButtonCtrName" + rowIndex.ToString();
		string newPriceStr = (isNewPrices) ? "1" : "0";
		bool isTreatmeantOnly = traveller.SelectedMakat[0].ItemSKU == "029940";
		tc.Wrap = false;
		if (isTreatmeantOnly)
		{
			// button to retrieve base types
			getPriceBaseTypesBtn.ID = "baseSelectorButton_treatmeant_" + rowIndex;
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
			baseTypesSelectorCrt.Text = "<div class='left'><div class='price'>&nbsp;</div>";
			// field to show price amount
			hotelPriceAmountField.Text = "&nbsp;<span class='" + hotelPriceAmountFieldCtrName + "' style='display:none;' "
											 + " orderButtonCtrName='" + orderButtonCtrName + "' ></span>";
			// order button
			orderButton.Text = "&nbsp;<span onclick='orderTreatment("+ hotelId +");' class='" + orderButtonCtrName + " order_button_treatmeant res res_attraction'>הזמן טיפולים</span></div>";
	
			//tc.Controls.Add(getPriceBaseTypesBtn);
			tc.Controls.Add(baseTypesSelectorCrt);
			tc.Controls.Add(hotelPriceAmountField);
			tc.Controls.Add(orderButton);
		}
		else
		{
			
			// button to retrieve base types
		        getPriceBaseTypesBtn.ID = "baseSelectorButton" + rowIndex;
		        getPriceBaseTypesBtn.CssClass = "baseSelectorButton";
		        getPriceBaseTypesBtn.Attributes.Add("rowIndex", rowIndex.ToString());
		        getPriceBaseTypesBtn.Attributes.Add("hotelId", hotelId);
				getPriceBaseTypesBtn.Attributes.Add("isNewPrice", newPriceStr);
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
			if (!isNewPrices)
			{
				orderButton.Text = "&nbsp;<span class='" + orderButtonCtrName + " order_button res' style='display:none;' >הזמן</span></div>";
			}
			else
			{
				orderButton.Text = "&nbsp;<span class='" + orderButtonCtrName + " order_button res' style='display:none; background:#f51fa9;' >הזמן</span></div>";
				//orderButton.Text = "</div>";
			}

		        tc.Controls.Add(getPriceBaseTypesBtn);
		        tc.Controls.Add(baseTypesSelectorCrt);
		        tc.Controls.Add(hotelPriceAmountField);
		        tc.Controls.Add(orderButton);
		}
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
