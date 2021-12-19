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
using System.Web.UI.MobileControls;
using System.Collections.Generic;
using System.Threading;
using System.Globalization;
using TreatmentEntitledService;
using System.Text;

public partial class _Default : System.Web.UI.Page
{
    Dictionary<string, Dictionary<string, int>> mDictMakatAndRequestIdCount = null;
    private string mAccessToken { get; set; }
    private GovTraveller travellers;
    private int mMaxDaysToShow = 0;
    private string mSupplierID = ConfigurationManager.AppSettings["supplierID"].ToString();
    private int mSupplierNumber = Convert.ToInt32(ConfigurationManager.AppSettings["supplierNumber"].ToString());
    private string mSupplierSecret = ConfigurationManager.AppSettings["supplierSecret"].ToString();
    private bool isUsageBalanceUpdatedAstmaTipulim = false;
	AgencyGovConnector.AgencyGovConnector connector = new AgencyGovConnector.AgencyGovConnector();
	
    protected void Page_Load(object sender, EventArgs e)
    {		
        if (Session["ClientMessage"] != null)
        {
            string messageDeleted = Session["ClientMessage"].ToString();
            ClientScript.RegisterStartupScript(this.GetType(), "Popup", "ShowPopup('" + messageDeleted + "');", true);
            Session["ClientMessage"] = null;
        }
		if (ddlEntitledYear.Items.Count == 0)
		{
			//ddlEntitledYear.Items.Add(new ListItem("2020", "2020"));//what is this?
			ddlEntitledYear.Items.Add(new ListItem((2021).ToString(), (2021).ToString()));
		}
		
			//003072004
			//if (Request.QueryString["AgnClerkId"] == "1")
			//{
			//	ddlEntitledYear.Items.Add(new ListItem((2021).ToString(), (2021).ToString()));
			//}
		if (ddlEntitledYear.Items.Count == 0)
		{
			int currentEntitledYear = DateTime.Now.Year;
			if (DateTime.Now.Month < 3)
			{
				currentEntitledYear = DateTime.Now.Year - 1;
			}				
			
			DataSet ds = DAL_SQL.RunSqlDataSet(@"SELECT top 1 entitled_year, opening_first_half_date 
												FROM GOV_OPENING_DATES 
												WHERE GETDATE() >= opening_first_half_date 
												and entitled_year = (" + (currentEntitledYear + 1) + @")
												order by entitled_year asc");

			ddlEntitledYear.Items.Add(new ListItem(currentEntitledYear.ToString(), currentEntitledYear.ToString()));
			if (Utils.IsDataSetRowsNotEmpty(ds))
			{
				ddlEntitledYear.Items.Add(new ListItem((currentEntitledYear + 1).ToString(), (currentEntitledYear + 1).ToString()));
			}			
		}

        if (!Page.IsPostBack)
        {
            setAgencyData();
            // check if need redirect
            if (Request["action"] != null && Request["action"].Length > 0) 
			{
                Response.Redirect(Request["action"]);
			}
        }

		//if (!Utils.CheckSecurity(225) || Request["AgnClerkId"] != "1") Response.Redirect("AccessDenied.aspx");
        //if (!Utils.CheckSecurity(225)) Response.Redirect("AccessDenied.aspx");

        messageContainer.Visible = false;
        divResult.Visible = false;
    }

    private void setAgencyData()
    {
        AgencyUser user = new AgencyUser();
        
        user.AgencyId = Request["AgnAgencyId"];//"85";
        user.AgencySystemType = Request["AgnSystemType"];//"3";  
        user.AgencyUserId = Request["AgnClerkId"];//"1"; 
        user.AgencyUserName = Request["AgnClerkName"]; //"aviran";
        user.AgencyUserPassword = Request["AgnClerkPassword"];//"aviran";
        
        /*user.AgencyId = "85";
        user.AgencySystemType = "3";
        user.AgencyUserId = "1";
        user.AgencyUserName = "Agency2000";
        user.AgencyUserPassword = "11071964";
       */
	   
        
        // init conn str
        DAL_SQL.ConnStr = ConfigurationManager.AppSettings["CurrentAgencyDBConnStr"];
        DAL_SQL.ConnStr = DAL_SQL.ConnStr.Replace("^agencyId^", ((user.AgencyId.Length == 1) ? "000" + user.AgencyId : "00" + user.AgencyId));
        DAL_SQL.ConnStr = DAL_SQL.ConnStr.Replace("^systemType^", ((user.AgencySystemType == "3") ? "INN" : "OUT"));
    }

    protected void RefreshErkevPanel_Click(object sender, EventArgs e)
    {
        try
        {
            GovTraveller travellers = new GovTraveller();
            GovTraveller curTraveller = GovTraveller.LoadFromSession();
            GovTravellerMakat selectedMakat = new GovTravellerMakat();

            divErkev.Visible = true;
             
            // commented by igor on 2012.02.23 - preffer to save selected makat and work with it (whole object)
            int counter = 0;
            foreach (RepeaterItem rptItem in rpt.Items)
            {
                CheckBox chkBx = rptItem.FindControl("rptMakatChkBx") as CheckBox;
                if (chkBx != null && chkBx.Checked == true)
                {
                    selectedMakat = curTraveller.Makats[counter];
                    chkBx.Checked = false;
                    break;
                }
                counter++;
            }
            // commented by igor on 2012.02.23 - preffer to save selected makat and work with it (whole object)
            //GenerateErkevTable(escortNumChecked, makatChecked, makatBalance);
            GenerateErkevTable(selectedMakat);
        }
        catch (Exception ex)
        {
            Logger.Log(ex.Message);
			Session["ClientMessage"] = "הזמן תם. אנא חפש מחדש";
			Response.Redirect("./Default.aspx");
        }

    }

    protected void btnContinue_Click(object sender, EventArgs e)
    {
		try{
			GovTraveller curTraveller = GovTraveller.LoadFromSession();
			curTraveller.SelectedMakat.Clear();
			// chen. clear all the melavim that was before.
			curTraveller.Melave.Clear();
			curTraveller.EntitledYear = ddlEntitledYear.SelectedValue;//@EY
			
			int counter = 0;
			foreach (RepeaterItem rptItem in rpt.Items)
			{
				CheckBox chkbx = rptItem.FindControl("rptMakatChkBx") as CheckBox;
	
				if (chkbx != null && chkbx.Checked == true)
				{
					curTraveller.SelectedMakat.Add(curTraveller.Makats[counter]);
				}
				++counter;
			}
			DateTime openPsoriasis = DateTime.Parse("07-Apr-21 15:30:00");//@corona
			DateTime openAll = DateTime.Parse("11-Apr-21 08:30:00");//@corona
			
			if (curTraveller.SelectedMakat[0].ItemSKU == "027241" && DateTime.Now <= openPsoriasis)//@corona
			{
				ClientScript.RegisterStartupScript(this.GetType(), "Popup", "ShowPopup('המכירה תפתח ב-" + openPsoriasis + " למקט 41');", true); 
				return;
			}
			else if (DateTime.Now <= openAll && curTraveller.SelectedMakat[0].ItemSKU != "027241")//@corona
			{
				ClientScript.RegisterStartupScript(this.GetType(), "Popup", "ShowPopup('המכירה תפתח ב-" + openAll + "');", true); 
				return;
			}
			//@corona endFor
			
			string erkevType = hidSelectedErkev.Value;
			ErkevTypes erkev_enum = (ErkevTypes)Enum.Parse(typeof(ErkevTypes), erkevType, true);
			curTraveller.ErkevType = erkevType;
			curTraveller.BalanceUssage = false;
			
			switch (erkev_enum)
			{
				case ErkevTypes.Zakai:
					curTraveller.RoomsAmount = 1;
					break;
				case ErkevTypes.ZakaiAndMelave:
					curTraveller.RoomsAmount = 1;
					break;
				case ErkevTypes.ZakaiAnd2Melavim:
					curTraveller.RoomsAmount = 2;
					break;
				case ErkevTypes.ZakaiAndMelaveBeTashlum:
					curTraveller.MelavePays = true;
					curTraveller.RoomsAmount = 1;
					break;
				case ErkevTypes.ZakaiAndMelaveBeTashlumHelekTkufa:
					curTraveller.MelavePays = true;
					curTraveller.RoomsAmount = 1;
					break;
				case ErkevTypes.ZakaiBeTashlumAndMelaveLeLoTashlum: // makat 40
					curTraveller.ZakayPays = true;
					curTraveller.RoomsAmount = 1;
					//curTraveller.BalanceUssage = (Request.Form["chBalanceUssage"] != null && Request.Form["chBalanceUssage"].ToLower() == "on");
					break;
				case ErkevTypes.ZakaiAndMelaveTmuratZakaut: // makat 40
					curTraveller.ZakayPays = true;
					curTraveller.RoomsAmount = 1;
					curTraveller.BalanceUssage = true;
					break;
	
			}
			// save selected 4+1 7+7
			string FourOneSeven = hidSelectedFourOneSeven.Value;
			curTraveller.FourOneSeven = FourOneSeven;
			//if (!string.IsNullOrEmpty(txtMisparTik.Text.Trim())) curTraveller.AgencyDocketId = txtMisparTik.Text;
			
			//is100 will get true if got from shikum true, or makat 36 or makat 41 
			bool is100 = ((!string.IsNullOrEmpty(curTraveller.Simon_100) && (curTraveller.Simon_100 == "1"))
						|| curTraveller.SelectedMakat[0].ItemSKU == "027236" 
						|| curTraveller.SelectedMakat[0].ItemSKU == "027241"
						);
						//|| curTraveller.SelectedMakat[0].ItemSKU == "027241" -- Added again according to request from nava
			
			curTraveller.LastDateToOrder = getLastDate(int.Parse(ddlEntitledYear.SelectedValue), is100);
			
			curTraveller.SaveToSession();
					
           	Response.Redirect("./UpdateEscorts.aspx", false);
			
		}
		catch(Exception exc)
		{
			Logger.Log(exc.Message);
			Logger.Log(exc.StackTrace);
			Response.Write(exc.Message);
			Response.Write(exc.StackTrace);
		}

    }

	
	//get the last year of the chosen entitled year
	public DateTime getLastDate(int iEntitledYear, bool iIs100)
    {
		DateTime today = DateTime.Now;
        int chosenEntitledYear = iEntitledYear;
        string lastDate = "01-Mar-" + (chosenEntitledYear+1);
		
		if (today > DateTime.Parse("02-Aug-20 08:30:00") || (Request.QueryString["x"] != null))
		{
			lastDate = "01-Apr-" + (chosenEntitledYear+1);
		}
		
        DateTime firstOpening = DateTime.Parse(DAL_SQL.GetRecord("GOV_OPENING_DATES", "opening_first_half_date", chosenEntitledYear.ToString(), "entitled_year"));
        DateTime secondOpening = DateTime.Parse(DAL_SQL.GetRecord("GOV_OPENING_DATES", "opening_second_half_date", chosenEntitledYear.ToString(), "entitled_year"));
        
        if (!iIs100 && today >= firstOpening && today <= secondOpening)
        {
            lastDate = "01-Sep-" + chosenEntitledYear;
        }
		int coronaYear = 2020;//@corona
		
		if (iEntitledYear == coronaYear)//@corona
		{
			lastDate = "01-Sep-" + (coronaYear + 1);
		}
		if (iEntitledYear == 2021)//@corona
		{
			lastDate = "01-Mar-22";
		}
		//Logger.Log("LastDate:iEntitledYear:"+iEntitledYear+":" + lastDate);
        return DateTime.Parse(lastDate);
    }
	
    protected void SeachOnClick(object sender, EventArgs e)
    {
        string travellerId = txtTravellerId.Text;
        string docketId = txtDocketId.Text;
        // there can be more than one traveller
        travellers = new GovTraveller();
		bool isGotTravellerSuccess = true;
			
        if (travellerId.Length > 0) // search by traveller id
        {
			
            // save the traveller. but still need to set it again!.
			try
			{
				travellers = travellers.LoadDataByTravellerId(travellerId.Trim());
			}
			catch(Exception ex1)
			{
				isGotTravellerSuccess = false;
				Logger.Log("Failed to get traveller from DB. Exception = " + ex1.Message);
				Session["ClientMessage"] = "יש להיכנס למודול שיקום מחדש";
			}
			
			if (isGotTravellerSuccess)
			{
				if (!string.IsNullOrEmpty(travellers.DocketId))
				{
					isGotTravellerSuccess = setAndGetTravellerDetails(travellers.DocketId);
					btnContinue.Visible = true;
				}
				else
				{
					travellers = null;
					ClientScript.RegisterStartupScript(this.GetType(), "Popup", "ShowPopup('" + "לא קיים מספר ת.ז כזה במשרד הביטחון" + "');", true); 
				}
			}
        }
        else
        {

            if (docketId.Length > 0)
            {
				try{
                //Chen. Getting the accessToken from misrad habitahon.
				btnContinue.Visible = true;
                
				isGotTravellerSuccess = setAndGetTravellerDetails(docketId);
				
				}catch(Exception ex){
					ClientScript.RegisterStartupScript(this.GetType(), "Popup", "ShowPopup('" + "לא קיים מספר תיק כזה במשרד הביטחון" + "');", true); 

				}
            }
			else if (txtDocketIdPartial.Text.Length > 0)
			{
				btnContinue.Visible = true;
				string originalDocket = DAL_SQL.RunSql("SELECT DocketId FROM Gov_TRAVELLERS WHERE DocketId like N'"+ txtDocketIdPartial.Text +"%' ");
				
				isGotTravellerSuccess = setAndGetTravellerDetails(originalDocket);
			}
          
        }


        if (travellers != null && isGotTravellerSuccess)
        {
            travellers = GovTraveller.LoadFromSession();
            divResult.Visible = true;

            rpt.DataSource = travellers.Makats;
            rpt.DataBind();

            tBoxDocketID.Text = travellers.DocketId;
            tBoxTravellerID.Text = travellers.TravellerId;
            tBoxFirstName.Text = travellers.FirstName;
            tBoxSecondName.Text = travellers.SecondName;
            tBoxAddress.Text = travellers.Address;
            tBoxCity.Text = travellers.City;
            tBoxZipCode.Text = travellers.ZipCode;
			bool isOneHundred = false;
			if (!string.IsNullOrEmpty(travellers.Simon_100))
			{
				if (travellers.Simon_100=="1")
				{
					isOneHundred = true;
				}
			}
			if (isOneHundred)
			{
				tBoxSimon.Text = "קיימת";
				tBoxSimon.ForeColor = System.Drawing.ColorTranslator.FromHtml("#008040");
	
			}
			else
			{
				tBoxSimon.Text = "לא קיימת";
				tBoxSimon.ForeColor = System.Drawing.ColorTranslator.FromHtml("#800040");
			}
        }
        else
        {
            //ShowMessage("לא נימצא תוצאות", MessageType.Info);
			ClientScript.RegisterStartupScript(this.GetType(), "Popup", "ShowPopup('" + "יש להיכנס למודול שיקום מחדש" + "');", true); 
            divResult.Visible = false;
        }

        divErkev.Visible = false;
    }

    private bool setAndGetTravellerDetails(string iDocketId)
    {
		bool isGotTravellerSuccess = true;
        travellers = new GovTraveller();

        mAccessToken = connector.GetAccessToken(iDocketId);
		try
		{
			travellers = travellers.LoadDataByDocketId(iDocketId);	
			travellers.SaveToSession();

			calculateUsage();
			updateUsageBalanceForAstmaAndTipulim();
			travellers.setTravellerBalance(mDictMakatAndRequestIdCount);
			//travellers.setMakatAstmaAndTipulimMaxDaysNumber(mMaxDaysToShow);
			
			travellers.SaveToSession();

			travellers = GovTraveller.LoadFromSession();

		}
		catch(Exception ex2)
		{
			isGotTravellerSuccess = false;
			Logger.Log("Failed to get traveller from DB. Exception = " + ex2.Message);
			Session["ClientMessage"] = "!יש להיכנס למודול שיקום מחדש";			
		}
		
        return isGotTravellerSuccess;
    }

    private void updateUsageBalanceForAstmaAndTipulim()
    {
        bool hasAstma = false;
        bool hasTipulim = false;
        bool isChoosed5plus5 = false;
        bool isChoosedMakat40 = false;

        foreach (GovTravellerMakat makat in travellers.Makats)
        {
            if (makat.ItemSKU == "027242")
                hasAstma = true;
            if (makat.ItemSKU == "027236" || makat.ItemSKU == "027235" || makat.ItemSKU == "027241")
                hasTipulim = true;
        }


        if (hasAstma && hasTipulim)
        {
            mMaxDaysToShow = 0;

            foreach (GovTravellerMakat makat in travellers.Makats)
            {
                makat.UsageBalance = 0;
                mMaxDaysToShow = makat.DaysNum > mMaxDaysToShow ? makat.DaysNum : mMaxDaysToShow;
            }
        }

        int daysUsedForAstmaAndTipulim = 0;
        StringBuilder clientFailedMsg = new StringBuilder();
		System.Net.ServicePointManager.SecurityProtocol = (System.Net.SecurityProtocolType)3072;
        TreatmentEntitledServiceClient treatmentClient = new TreatmentEntitledServiceClient();

        AgencyGovConnector.ServiceEntitledResponse userDetails = connector.GetTreatmentDetails(mAccessToken);

        if (userDetails.FailureCode != null)
        {
            int failure = userDetails.FailureCode.Id;
            foreach (string msg in userDetails.FailureCode.ClientMessages)
            {
                clientFailedMsg.Append(msg);
                clientFailedMsg.Append(Environment.NewLine);
            }
        }

        mDictMakatAndRequestIdCount = new Dictionary<string, Dictionary<string, int>>();

        foreach (AgencyGovConnector.EntitledOrder order in userDetails.EntitledOrders)
        {
			// && order.Id.ToString() == "114623" && ddlEntitledYear.SelectedValue == "2020" || 
			//order.Id.ToString() == "114623" && ddlEntitledYear.SelectedValue == "2019" 
			DateTime? startDate = order.StartDate;
			if (order.Id.ToString() == "114623")
			{
				startDate = DateTime.Parse("01-Mar-19"); //Force to be 2019! //Special request from 'Shikum' this zakai has order in 2020 and days taken from 2019
			}
			
			if (isOrderInEntitledYear(order))//@EY 
			{
				// if order not canceled.
				if (order.Status)
				{
					Logger.Log(ddlEntitledYear.SelectedValue + " - " + order.Id);
					if (hasAstma && hasTipulim)
					{
						if (order.KatalogNumber == "027242" || order.KatalogNumber == "027236" || order.KatalogNumber == "027235" || order.KatalogNumber == "027241")
						{
							daysUsedForAstmaAndTipulim += order.DaysNumber;
							//if (travellers.TravellerId == "058250168") daysUsedForAstmaAndTipulim = 0;
						}
					}
				}
			}
        }

        foreach (AgencyGovConnector.EntitledOrder order in userDetails.EntitledOrders)
        {
			// && order.Id.ToString() == "114623" && ddlEntitledYear.SelectedValue == "2020" || 
			//order.Id.ToString() == "114623" && ddlEntitledYear.SelectedValue == "2019" 
			DateTime? startDate = order.StartDate;
			if (order.Id.ToString() == "114623")
			{
				startDate = DateTime.Parse("01-Mar-19"); //Force to be 2019!//Special request from 'Shikum' this zakai has order in 2020 and days taken from 2019
			}
			if (isOrderInEntitledYear(order))//@EY 
            {
				// if order not canceled.
				if (order.Status)
				{
					Logger.Log(ddlEntitledYear.SelectedValue + " - " + order.Id);
					Logger.Log("order.KatalogNumber = " + order.KatalogNumber + " , order.RequestId = " + order.RequestId);
					TimeSpan? ts = new TimeSpan();
					ts = (order.EndDate - order.StartDate);
	
					if (order.KatalogNumber == "027240")
					{
						isChoosedMakat40 = true;
					}
	
					if (!mDictMakatAndRequestIdCount.ContainsKey(order.KatalogNumber)) // if 
					{
						mDictMakatAndRequestIdCount.Add(order.KatalogNumber, new Dictionary<string, int>());
						if (order.DaysNumber == ts.Value.Days * 2)
						{
							mDictMakatAndRequestIdCount[order.KatalogNumber].Add(order.RequestId, ts.Value.Days * 2 + 1); //chen special makat 5+5
							isChoosed5plus5 = true;
						}
						else
							mDictMakatAndRequestIdCount[order.KatalogNumber].Add(order.RequestId, order.DaysNumber);
					}
					else
					{
						if (mDictMakatAndRequestIdCount[order.KatalogNumber].ContainsKey(order.RequestId))
						{
							if (order.DaysNumber == ts.Value.Days * 2)
							{
								//Chen. remove cause no need. handled in other method.
								//mDictMakatAndRequestIdCount[order.KatalogNumber][order.RequestId] += 1;//chen special makat 5+5
								isChoosed5plus5 = true;
							}
	
							mDictMakatAndRequestIdCount[order.KatalogNumber][order.RequestId] += order.DaysNumber;
						}
						else
						{
							mDictMakatAndRequestIdCount[order.KatalogNumber].Add(order.RequestId, order.DaysNumber);
						}
					}
	
					if (hasAstma && hasTipulim && !isUsageBalanceUpdatedAstmaTipulim)
					{
						updateUsageBalanceForAstmaAndTipulim(daysUsedForAstmaAndTipulim, order.KatalogNumber);
						isUsageBalanceUpdatedAstmaTipulim = true;
					}
				}
				else
				{
					if (order.FineDays != 0)
					{
						if (!mDictMakatAndRequestIdCount.ContainsKey(order.KatalogNumber)) // if 
						{
							mDictMakatAndRequestIdCount.Add(order.KatalogNumber, new Dictionary<string, int>());
							mDictMakatAndRequestIdCount[order.KatalogNumber].Add(order.RequestId, order.FineDays);
						}
						else
						{
							if (mDictMakatAndRequestIdCount[order.KatalogNumber].ContainsKey(order.RequestId))
							{
								mDictMakatAndRequestIdCount[order.KatalogNumber][order.RequestId] += order.FineDays;
							}
							else
							{
								mDictMakatAndRequestIdCount[order.KatalogNumber].Add(order.RequestId, order.FineDays);
							}
						}
						if (hasAstma && hasTipulim)
						{
							//Check if the special makats exist to this zakai.
							updateUsageBalanceForAstmaAndTipulim(order.FineDays, order.KatalogNumber);
						}
					}
				}
			}//@EY
        }


        // If makat40 was chosed in the past. change the options.
        if (isChoosedMakat40)
        {
            changeMakat40Option(isChoosed5plus5);
        }
		
		if (hasAstma && hasTipulim)
		{
			foreach (GovTravellerMakat makat in travellers.Makats)
			{
				if (makat.ItemSKU == "027242" || makat.ItemSKU == "027236" || makat.ItemSKU == "027235" || makat.ItemSKU == "027241")
				{
					//Makat 42 + (35 or 36)
					if (mDictMakatAndRequestIdCount.ContainsKey(makat.ItemSKU) && mDictMakatAndRequestIdCount[makat.ItemSKU].ContainsKey(makat.Request_SH))
					{	
						mDictMakatAndRequestIdCount[makat.ItemSKU][makat.Request_SH] = makat.UsageBalance;
					}
					else
					{
						if (mDictMakatAndRequestIdCount.ContainsKey(makat.ItemSKU))
						{
							mDictMakatAndRequestIdCount[makat.ItemSKU].Add(makat.Request_SH, makat.UsageBalance);						
						}
						else
						{
							mDictMakatAndRequestIdCount.Add(makat.ItemSKU, new Dictionary<string, int>());
							mDictMakatAndRequestIdCount[makat.ItemSKU].Add(makat.Request_SH, makat.UsageBalance);
						}
					}
					
					//if (travellers.TravellerId == "054095914")
					//{
					//	mDictMakatAndRequestIdCount[makat.ItemSKU][makat.Request_SH] = 0;
					//}
				}
			}
		}
    }

    private void updateUsageBalanceForAstmaAndTipulim(int iDaysToAdd, string iItemSKU)
    {
        foreach (GovTravellerMakat makat in travellers.Makats)
        {
            if (makat.ItemSKU == "027242" || makat.ItemSKU == "027236" || makat.ItemSKU == "027235" || makat.ItemSKU == "027241")
            {
                //Check if the special makats exist to this zakai.
                makat.UsageBalance += iDaysToAdd;
				if (makat.UsageBalance > makat.DaysNum)
                {
                    makat.UsageBalance = makat.DaysNum;
                }
            }
        }
    }
	
	
    private void changeMakat40Option(bool isChoosed5plus5)
    {
        //If indexOfOption = 1 - disable option 1 
        //If indexOfOption = 2 - disable 5+5 
        int indexOfOptionToDisable = 0;
        
        if (isChoosed5plus5) 
        {
            indexOfOptionToDisable = 1;
        }
        else
        {
            indexOfOptionToDisable = 2;
        }

        foreach (GovTravellerMakat makat in travellers.Makats)
        {
            if (makat.ItemSKU == "027240")
            {
                makat.Makat40 = indexOfOptionToDisable;
            }
        }
    }

    private string getAccessToken(string docketID)
    {
        try
        {
			System.Net.ServicePointManager.SecurityProtocol = (System.Net.SecurityProtocolType)3072;
            TreatmentEntitledServiceClient treatmentClient = new TreatmentEntitledServiceClient();
            ServiceTokenResponse tokenResponse = treatmentClient.GetTokenMoked(docketID, mSupplierNumber, mSupplierSecret, mSupplierID);
			
			string[] messages;
			string message;
			
			if (tokenResponse != null)
			{
				messages = tokenResponse.Status.ClientMessages;
				message = messages[0];
			}
			
            StringBuilder clientFailedMsg = new StringBuilder();

            if (tokenResponse == null || tokenResponse.FailureCode != null)
            {
				
                int failure = tokenResponse.FailureCode.Id;
                foreach (string msg in tokenResponse.FailureCode.ClientMessages)
                {
                    clientFailedMsg.Append(msg);
					Logger.Log(failure.ToString() + ". " + msg);
                    clientFailedMsg.Append(Environment.NewLine);
                }
            }
            else
            {
				//try{
				//Logger.Log("heyy4 " + tokenResponse.AccessToken );
				//}catch(Exception takeABreak){ Logger.Log(takeABreak.Message);}
                mAccessToken = tokenResponse.AccessToken;
				//Logger.Log("ALEXANDER " +mAccessToken);
                ServiceEntitledResponse userDetails = treatmentClient.GetEntitledDetails(mAccessToken);
				

                //calculateUsage(userDetails.EntitledOrders);

                docketID = userDetails.EntitledMainDetails.TikNumber;
                Session["DocketID"] = docketID;
                Session["AccessToken"] = mAccessToken;
            }
			//Logger.Log("heyy5 ");
			 treatmentClient.Close();
        }
        catch (Exception ex)
        {
			Logger.Log("heyy6 ");
            Logger.Log("Default.aspx.cs: getAccessToken: " + ex.Message + ", trace = " + ex.StackTrace);
            Session["ClientMessage"] = "לא ניתן להתחבר, נא ליצור קשר עם משרד הביטחון.";
			Response.Redirect("./error.aspx");
        }

        return mAccessToken;
    }

    private void calculateUsage()
    {
        bool hasAstma = false;
        bool hasTipulim = false;

        foreach (GovTravellerMakat makat in travellers.Makats)
        {
            if (makat.ItemSKU == "027242")
                hasAstma = true;
            if (makat.ItemSKU == "027236" || makat.ItemSKU == "027235" || makat.ItemSKU == "027241")
                hasTipulim = true;

        }

        if (hasAstma && hasTipulim)
        {
            mMaxDaysToShow = 0;

            foreach (GovTravellerMakat makat in travellers.Makats)
            {
                mMaxDaysToShow = makat.DaysNum > mMaxDaysToShow ? makat.DaysNum : mMaxDaysToShow;
            }
        }

        StringBuilder clientFailedMsg = new StringBuilder();
		System.Net.ServicePointManager.SecurityProtocol = (System.Net.SecurityProtocolType)3072;
        TreatmentEntitledServiceClient treatmentClient = new TreatmentEntitledServiceClient();

		AgencyGovConnector.ServiceEntitledResponse userDetails = connector.GetTreatmentDetails(mAccessToken);

        if (userDetails.FailureCode != null)
        {
            int failure = userDetails.FailureCode.Id;
            foreach (string msg in userDetails.FailureCode.ClientMessages)
            {
                clientFailedMsg.Append(msg);
                clientFailedMsg.Append(Environment.NewLine);
            }
			Session["IsLoggedIn"] =clientFailedMsg.ToString();
			Response.Redirect("./error.aspx");
        }

        mDictMakatAndRequestIdCount = new Dictionary<string, Dictionary<string, int>>();
		int DaysCount5plus5 = 0;
        foreach (AgencyGovConnector.EntitledOrder order in userDetails.EntitledOrders)
        {
			//Logger.Log(order.OrderNumberSupplier);
			//if (order.Status)
			//	Response.Write("</br>order.KatalogNumber = " + order.KatalogNumber + ", " + order.StartDate + " - " + order.EndDate);
			// && order.Id.ToString() == "114623" && ddlEntitledYear.SelectedValue == "2020" || 
			//order.Id.ToString() == "114623" && ddlEntitledYear.SelectedValue == "2019" 
			DateTime? startDate = order.StartDate;
			if (order.Id.ToString() == "114623")
			{
				startDate = DateTime.Parse("01-Mar-19"); //Force to be 2019! //Special request from 'Shikum' this zakai has order in 2020 and days taken from 2019
			}
			
			if (isOrderInEntitledYear(order))//@EY 
            {
				// if order not canceled.
				if (order.Status)
				{					
					TimeSpan? ts = new TimeSpan();
					ts = (order.EndDate - order.StartDate);
	
					if (!mDictMakatAndRequestIdCount.ContainsKey(order.KatalogNumber)) // if the requestID does not entered yet.
					{
						mDictMakatAndRequestIdCount.Add(order.KatalogNumber, new Dictionary<string, int>());
						if (order.DaysNumber == ts.Value.Days * 2)
						{
							DaysCount5plus5 += order.DaysNumber;
							if (DaysCount5plus5 == 10) // 5 for melave, 5 for zakai * 2 + 1
							{ 
								mDictMakatAndRequestIdCount[order.KatalogNumber].Add(order.RequestId, ts.Value.Days * 2 + 1); //chen special makat 5+5
							}
							else
							{
								mDictMakatAndRequestIdCount[order.KatalogNumber].Add(order.RequestId, ts.Value.Days * 2); //chen special makat 5+5
							}
						}
						else
						{
							mDictMakatAndRequestIdCount[order.KatalogNumber].Add(order.RequestId, order.DaysNumber);
						}
					}
					else 
					{
						if (mDictMakatAndRequestIdCount[order.KatalogNumber].ContainsKey(order.RequestId))
						{
							if (order.DaysNumber == ts.Value.Days * 2)
							{
								DaysCount5plus5 += order.DaysNumber;
								if (DaysCount5plus5 == 10) // 5 for melave, 5 for zakai * 2 + 1
								{
									mDictMakatAndRequestIdCount[order.KatalogNumber][order.RequestId] += 1;//chen special makat 5+5
								}
							}
	
							mDictMakatAndRequestIdCount[order.KatalogNumber][order.RequestId] += order.DaysNumber;
						}
						else
						{
							mDictMakatAndRequestIdCount[order.KatalogNumber].Add(order.RequestId, order.DaysNumber);
						}
					}
				}
            }//@EY
        }
    }
	
	
	
	private bool isDateInEntitledYear(DateTime? iOrderStartDate)// @EY
    {
        bool isInEntitledYear = false;
        DateTime orderStartDate;
        int selectedYear = int.Parse(ddlEntitledYear.SelectedValue);

        orderStartDate = DateTime.Parse(iOrderStartDate != null ? iOrderStartDate.Value.ToString("dd-MMM-yy") : DateTime.Now.AddYears(-100).ToString());
		
		if (2020 == selectedYear)
		{
			if ((orderStartDate.Date.Month < 9 && orderStartDate.Date.Year == selectedYear + 1) || (orderStartDate.Date.Month >= 3 && orderStartDate.Date.Year == selectedYear))
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
    
	
	private bool isOrderInEntitledYear(AgencyGovConnector.EntitledOrder iOrder)// @EY - by order entitled_year 20/05/21
    {
        bool isInEntitledYear = false;
        int orderEntitledYear = 0;
        int selectedEntitledYear = int.Parse(ddlEntitledYear.SelectedValue);

		orderEntitledYear = iOrder.EntitledYear;//year from gov service
		if (orderEntitledYear <= 0)
		{
			//if gov service didnt provide a year check db
			string bundleEntitledYear = DAL_SQL.RunSql("SELECT ISNULL(gov_entitled_year, 0) " + 
													" FROM         BUNDLES " + 
													" WHERE gov_order_id = '" + iOrder.Id  + "'" + 
													" AND service_type = 2 ");
			
			int.TryParse(bundleEntitledYear, out orderEntitledYear);
			
			if (orderEntitledYear > 0)
			{
				isInEntitledYear = true;
			}
		}
		else
		{
			isInEntitledYear = true;
		}
		
		if (isInEntitledYear)
		{
			//check if Order is In Entitled Year
			isInEntitledYear = orderEntitledYear == selectedEntitledYear;
		}
		else
		{
			//if didnt find in gov service and db check the old way
			//old way of checking
			isInEntitledYear = isDateInEntitledYear(iOrder.StartDate);
		}
		
        return isInEntitledYear;
    }

    protected void GvResult_OnDataBound(object sender, EventArgs e)
    {
    }

    // commented by igor on 2012.02.23 - preffer to save selected makat and work with it (whole object)
    //private void GenerateErkevTable(int escortNumber, string makat, int makatBalance)
    private void GenerateErkevTable(GovTravellerMakat selectedMakat)
    {
        // commented by igor on 2012.02.23 - preffer to save selected makat and work with it (whole object)
        // divErkev.InnerHtml = Erkev.GenerateTableHtml(escortNumber, makat, makatBalance);
        divErkev.InnerHtml = Erkev.GenerateTableHtml(selectedMakat);
    }

    private void ShowMessage(string strMessage, MessageType type)
    {
        switch (type)
        {
            case MessageType.Error:
                Message.Text = strMessage;
                messageContainer.Visible = true;
                messageContainer.Style.Add("color", "red");
                break;
            case MessageType.Info:
                Message.Text = strMessage;
                messageContainer.Visible = true;
                messageContainer.Style.Add("color", "blue");
                break;
            case MessageType.Warning:
                Message.Text = strMessage;
                messageContainer.Visible = true;
                messageContainer.Style.Add("color", "yellow");
                break;
        }
    }
    protected int CalculateBalance(string iMakat, string iRequest_SH)
    {
        GovTraveller traveller = GovTraveller.LoadFromSession();

		
        GovTravellerMakat gtMakatRetVal = null;
        //Request_SH is not good... need 1 per makat...
        foreach (GovTravellerMakat gtMakat in traveller.Makats)
        {
            if (gtMakat.ItemSKU == iMakat && gtMakat.Request_SH == iRequest_SH)
            {
                gtMakatRetVal = gtMakat;
                break;
            }
        }

        return gtMakatRetVal.DaysNum - gtMakatRetVal.UsageBalance;
    }

    protected string formatDate(DateTime date)
    {
        return date.ToString("dd-MMM-yy");
    }

    protected void btnGoToTravellerHistory_Click(object sender, EventArgs e)
    {
        Response.Redirect("./TravellerHistory.aspx");
    }
}
