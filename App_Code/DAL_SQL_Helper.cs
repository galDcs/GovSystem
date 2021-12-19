using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml;
using System.Data.SqlClient;

public class DAL_SQL_Helper
{

    /* chen - CheckCredentials checks user existence */
    public static bool CheckCredentials(string iTravellerID, string iDocketId)
    {
        bool isExist = false;
        const string trueValue = "true";
        DataSet ds = new DataSet();
        ds = DAL_SQL.ExecuteDataset(DAL_SQL.ConnStr, CommandType.StoredProcedure, "GOV_CheckTravellerCredentials",
            SqlDalParam.formatParam("@TravellerID", SqlDbType.NVarChar, iTravellerID),
            SqlDalParam.formatParam("@DocketID", SqlDbType.NVarChar, iDocketId)
            );

        DataRow row = ds.Tables[0].Rows[0];
        string t = row["UserExists"].ToString();
        //return string "true" if user exist, "false" otherwise
        if (trueValue.Equals(t))
        {
            isExist = true;
        }

        return isExist;
    }

    public static void UpsertTravellerUpdate(string PortfolioNo, string TravellerID, string FirstName, string SecondName,
                                        string Address, string CityCode, string City, string ZipCode, string ItemSKU, string ItemDescription,
                                        DateTime StartDate, DateTime EndDate, int DaysNum, int EscortNum,
                                        int Department, int Level, int Status, int UsageBalance, int JerusalemUsageBalance
                                        , string Tel_Pr_1, string Tel_Num_1, string Tel_Pr_2, string Tel_Num_2, string Tel_Pr_3, string Tel_Num_3,
                                        string OfficeComment, DateTime ReleaseDate, Boolean IsActive, int Makat40)
    {
        DataSet ds = new DataSet();

        ds = DAL_SQL.ExecuteDataset(DAL_SQL.ConnStr, CommandType.StoredProcedure, "GOV_Traveller_Upsert",
            SqlDalParam.formatParam("@DocketId", SqlDbType.NVarChar, PortfolioNo),
            SqlDalParam.formatParam("@TravellerID", SqlDbType.NVarChar, TravellerID),
            SqlDalParam.formatParam("@FirstName", SqlDbType.NVarChar, FirstName),
            SqlDalParam.formatParam("@SecondName", SqlDbType.NVarChar, SecondName),
            SqlDalParam.formatParam("@Address", SqlDbType.NVarChar, Address),
            SqlDalParam.formatParam("@CityCode", SqlDbType.NVarChar, CityCode),
            SqlDalParam.formatParam("@City", SqlDbType.NVarChar, City),
            SqlDalParam.formatParam("@ZipCode", SqlDbType.NVarChar, ZipCode),
            SqlDalParam.formatParam("@ItemSKU", SqlDbType.NVarChar, ItemSKU),
            SqlDalParam.formatParam("@ItemDescription", SqlDbType.NVarChar, ItemDescription),
            SqlDalParam.formatParam("@StartDate", SqlDbType.DateTime, StartDate),
            SqlDalParam.formatParam("@EndDate", SqlDbType.DateTime, EndDate),
            SqlDalParam.formatParam("@DaysNum", SqlDbType.Int, DaysNum),
            SqlDalParam.formatParam("@EscortNum", SqlDbType.Int, EscortNum),
            SqlDalParam.formatParam("@Department", SqlDbType.Int, Department),
            SqlDalParam.formatParam("@Level", SqlDbType.Int, Level),
            SqlDalParam.formatParam("@Status", SqlDbType.Int, Status),
            SqlDalParam.formatParam("@UsageBalance", SqlDbType.Int, UsageBalance),
            SqlDalParam.formatParam("@JerusalemUsageBalance", SqlDbType.Int, JerusalemUsageBalance),
            SqlDalParam.formatParam("@Tel_Pr_1", SqlDbType.NVarChar, Tel_Pr_1),
            SqlDalParam.formatParam("@Tel_Num_1", SqlDbType.NVarChar, Tel_Num_1),
            SqlDalParam.formatParam("@Tel_Pr_2", SqlDbType.NVarChar, Tel_Pr_2),
            SqlDalParam.formatParam("@Tel_Num_2", SqlDbType.NVarChar, Tel_Num_2),
            SqlDalParam.formatParam("@Tel_Pr_3", SqlDbType.NVarChar, Tel_Pr_3),
            SqlDalParam.formatParam("@Tel_Num_3", SqlDbType.NVarChar, Tel_Num_3),
            SqlDalParam.formatParam("@OfficeComment", SqlDbType.NVarChar, OfficeComment),
            SqlDalParam.formatParam("@ReleaseDate", SqlDbType.DateTime, ReleaseDate),
            SqlDalParam.formatParam("@IsActive", SqlDbType.Bit, IsActive),
            SqlDalParam.formatParam("@Makat40", SqlDbType.Int, Makat40)
        );
    }
    public static bool BackupTravellerTable()
    {
        return DAL_SQL.RunSqlbool("drop table GOV_TRAVELLERS_Backup;  select * into GOV_TRAVELLERS_Backup from GOV_TRAVELLERS");
    }
    public static bool DisableAllTravellers()
    {
		bool isSucceed = false;
		isSucceed = DAL_SQL.RunSqlbool("update GOV_TRAVELLERS set IsActive=0");
        isSucceed &= DAL_SQL.RunSqlbool("update GOV_TRAVELLERS set Status=0");
        return isSucceed;
    }
    public static bool RestoreTravellerTable()
    {
        return DAL_SQL.RunSqlbool("delete from GOV_TRAVELLERS; insert into  GOV_TRAVELLERS select * from GOV_TRAVELLERS_Backup ");
    }
    public static void GOV_UpsertStatus(bool update, int id, string name, bool status)
    {
        DAL_SQL.ExecuteDataset(DAL_SQL.ConnStr, CommandType.StoredProcedure, "GOV_UpsertStatus",
            SqlDalParam.formatParam("@update", SqlDbType.Bit, update),
            SqlDalParam.formatParam("@id", SqlDbType.Int, id),
           SqlDalParam.formatParam("@name", SqlDbType.NVarChar, name),
           SqlDalParam.formatParam("@status", SqlDbType.Bit, status)
          );
    }

    /* Update gov order id in BUNDLES table.*/
    public static void GOV_UpdateGovOrderID(int iGovOrderID, int iDocketID, int iTravellerID)
    {
        DAL_SQL.ExecuteDataset(DAL_SQL.ConnStr, CommandType.StoredProcedure, "GOV_Update_GovOrderID",
            SqlDalParam.formatParam("@govOrderID", SqlDbType.Int, iGovOrderID),
            SqlDalParam.formatParam("@DocketID", SqlDbType.Int, iDocketID),
            SqlDalParam.formatParam("@TravellerID", SqlDbType.Int, iTravellerID)
          );
    }

    /* Check if the govOrderID exists in BUNDLES table. */
    public static int GOV_GetDocketIdByGovOrderId(int iGovOrderID)
    {
        DataSet ds = new DataSet();
        int docketId = -1;

        ds = DAL_SQL.ExecuteDataset(DAL_SQL.ConnStr, CommandType.StoredProcedure, "GOV_GetDocketIdByGovOrderId",
            SqlDalParam.formatParam("@GovOrderID", SqlDbType.Int, iGovOrderID)
          );

        if (ds.Tables.Count != 0)
        {
            DataRow row = ds.Tables[0].Rows[0];
            string t = string.Empty;
            t = row["retval"].ToString();


            if (!int.TryParse(t, out docketId))
            {
                //Chen. Error...
            }
        }

        return docketId;
    }

    /**
     * InsertOrderApproval - insert to Gov_ORDER_APPROVAL new row
     * chen 25/10
     */
    public static void InsertOrderApproval(int iOrderId, string iItemSku, string iDocketId, DateTime iFromDate, DateTime iToDate, 
                                  int iSupplierID, string iHotelName, int iBase, string iFirstName, string iLastName, int iNumOfEscorts,
                                  string iMelave1FirstName, string iMelave1LastName, string iMelave2FirstName, string iMelave2LastName,
								  int iAllocationID)
    {
        DataSet ds = new DataSet();

        ds = DAL_SQL.ExecuteDataset(DAL_SQL.ConnStr, CommandType.StoredProcedure, "GOV_InsertOrderApproval",
           SqlDalParam.formatParam("@order_id", SqlDbType.Int, iOrderId),
           SqlDalParam.formatParam("@itemSKU", SqlDbType.NVarChar, iItemSku),
           SqlDalParam.formatParam("@docket_id", SqlDbType.NVarChar, iDocketId),
           SqlDalParam.formatParam("@from_date", SqlDbType.Date, iFromDate),
           SqlDalParam.formatParam("@to_date", SqlDbType.Date, iToDate),
           SqlDalParam.formatParam("@supplier_id", SqlDbType.Int, iSupplierID),
           SqlDalParam.formatParam("@hotel_name", SqlDbType.NVarChar, iHotelName),
           SqlDalParam.formatParam("@base", SqlDbType.Int, iBase),
           SqlDalParam.formatParam("@first_name", SqlDbType.NVarChar, iFirstName),
           SqlDalParam.formatParam("@last_name", SqlDbType.NVarChar, iLastName),
           SqlDalParam.formatParam("@num_of_escorts", SqlDbType.Int, iNumOfEscorts),
           SqlDalParam.formatParam("@melave1_first_name", SqlDbType.NVarChar, iMelave1FirstName),
           SqlDalParam.formatParam("@melave1_last_name", SqlDbType.NVarChar, iMelave1LastName),
           SqlDalParam.formatParam("@melave2_first_name", SqlDbType.NVarChar, iMelave2FirstName),
           SqlDalParam.formatParam("@melave2_last_name", SqlDbType.NVarChar, iMelave2LastName),
           SqlDalParam.formatParam("@allocation_id", SqlDbType.Int, iAllocationID));

    }

    /**
     * chen 25/10
     * */
    public static DataSet GetOrderApprovalDetails(int iOrderID)
    {
        DataSet ds = DAL_SQL.ExecuteDataset(DAL_SQL.ConnStr, CommandType.StoredProcedure, "Gov_GetApprovalOrder",
            SqlDalParam.formatParam("@order_id", SqlDbType.Int,  iOrderID)
            );
        return ds;
    }


    /**
     * chen 25/10
     * */
    public static DataSet GetAreaIDFromBundle(int iOrderID)
    {
        DataSet ds = DAL_SQL.ExecuteDataset(DAL_SQL.ConnStr, CommandType.StoredProcedure, "[GOV_GetAreaIDFromBundleTable]",
            SqlDalParam.formatParam("@order_id", SqlDbType.Int, iOrderID)
            );
        return ds;
    }

    /**
    * chen 25/10
    * */
    public static void CancelApprovalOrder(int iOrderID)
    {
        DataSet ds = DAL_SQL.ExecuteDataset(DAL_SQL.ConnStr, CommandType.StoredProcedure, "GOV_CancelApprovalOrder",
            SqlDalParam.formatParam("@order_id", SqlDbType.Int,  iOrderID)
            );
    }

    
    /**
    * chen 25/10
    * */
    public static void UpdateApprovalOrderDate(int iOrderID, DateTime iToDate)
    {
        DataSet ds = DAL_SQL.ExecuteDataset(DAL_SQL.ConnStr, CommandType.StoredProcedure, "UpdateApprovalOrderDates",
            SqlDalParam.formatParam("@order_id", SqlDbType.Int,  iOrderID),
            SqlDalParam.formatParam("@to_date", SqlDbType.Date,  iToDate)
            );
    }



    /* aviran 25/08 - added new fields - Stored Procedure also changed! */
    // ONLY FOR IMPORT TRAVELLERS (UPLOAD) form!!!
    public static string UpsertTravellerImport(string PortfolioNo, string TravellerID, string FirstName, string SecondName,
                                   string Address, string CityCode, string City, string ZipCode, string ItemSKU, string ItemDescription,
                                   DateTime StartDate, DateTime EndDate, DateTime ReleaseDate, int DaysNum, int EscortNum,
                                   int Department, int Level, int Status, string Tel_Pr_1, string Tel_Num_1, string Tel_Pr_2, string Tel_Num_2, string Tel_Pr_3, string Tel_Num_3, bool CleanZakaut,
                                   string Simon_100, string Nagish, string Request_SH, string Mahadora)
    {
        DataSet ds = new DataSet();

        ds = DAL_SQL.ExecuteDataset(DAL_SQL.ConnStr, CommandType.StoredProcedure, "GOV_Traveller_Upsert_IMPORT",
            SqlDalParam.formatParam("@DocketId", SqlDbType.NVarChar, PortfolioNo),
            SqlDalParam.formatParam("@TravellerID", SqlDbType.NVarChar, TravellerID),
            SqlDalParam.formatParam("@FirstName", SqlDbType.NVarChar, FirstName),
            SqlDalParam.formatParam("@SecondName", SqlDbType.NVarChar, SecondName),
            SqlDalParam.formatParam("@Address", SqlDbType.NVarChar, Address),
            SqlDalParam.formatParam("@CityCode", SqlDbType.NVarChar, CityCode),
            SqlDalParam.formatParam("@City", SqlDbType.NVarChar, City),
            SqlDalParam.formatParam("@ZipCode", SqlDbType.NVarChar, ZipCode),
            SqlDalParam.formatParam("@ItemSKU", SqlDbType.NVarChar, ItemSKU),
            SqlDalParam.formatParam("@ItemDescription", SqlDbType.NVarChar, ItemDescription),
            SqlDalParam.formatParam("@StartDate", SqlDbType.DateTime, StartDate),
            SqlDalParam.formatParam("@EndDate", SqlDbType.DateTime, EndDate),
            SqlDalParam.formatParam("@ReleaseDate", SqlDbType.DateTime, ReleaseDate),
            SqlDalParam.formatParam("@DaysNum", SqlDbType.Int, DaysNum),
            SqlDalParam.formatParam("@EscortNum", SqlDbType.Int, EscortNum),
            SqlDalParam.formatParam("@Department", SqlDbType.Int, Department),
            SqlDalParam.formatParam("@Level", SqlDbType.Int, Level),
            SqlDalParam.formatParam("@Status", SqlDbType.Int, Status),
            SqlDalParam.formatParam("@Tel_Pr_1", SqlDbType.NVarChar, Tel_Pr_1),
            SqlDalParam.formatParam("@Tel_Num_1", SqlDbType.NVarChar, Tel_Num_1),
            SqlDalParam.formatParam("@Tel_Pr_2", SqlDbType.NVarChar, Tel_Pr_2),
            SqlDalParam.formatParam("@Tel_Num_2", SqlDbType.NVarChar, Tel_Num_2),
            SqlDalParam.formatParam("@Tel_Pr_3", SqlDbType.NVarChar, Tel_Pr_3),
            SqlDalParam.formatParam("@Tel_Num_3", SqlDbType.NVarChar, Tel_Num_3),
            SqlDalParam.formatParam("@CleanZakaut", SqlDbType.Bit, CleanZakaut),
            SqlDalParam.formatParam("@Simon_100", SqlDbType.NVarChar, Simon_100),
            SqlDalParam.formatParam("@Nagish", SqlDbType.NVarChar, Nagish),
            SqlDalParam.formatParam("@Request_SH", SqlDbType.NVarChar, Request_SH),
            SqlDalParam.formatParam("@Mahadora", SqlDbType.NVarChar, Mahadora)
        );

        if (ds.Tables[0].Rows[0].ItemArray[0].ToString() == "1")
        {
            return "update";
        }
        else
        {
            return "insert";
        }
    }

	
    public static string UpsertTravellerImportExe(string PortfolioNo, string TravellerID, string FirstName, string SecondName,
                                   string Address, string CityCode, string City, string ZipCode, string ItemSKU, string ItemDescription,
                                   DateTime StartDate, DateTime EndDate, DateTime ReleaseDate, int DaysNum, int EscortNum,
                                   int Department, int Level, int Status, string Tel_Pr_1, string Tel_Num_1, string Tel_Pr_2, string Tel_Num_2, string Tel_Pr_3, string Tel_Num_3, bool CleanZakaut,
                                   string Simon_100, string Nagish, string Request_SH, string Mahadora)
    {
        DataSet ds = new DataSet();

        ds = DAL_SQL.ExecuteDataset(DAL_SQL.ConnStr, CommandType.StoredProcedure, "GOV_Traveller_Upsert_IMPORT_FROM_EXE",
            SqlDalParam.formatParam("@DocketId", SqlDbType.NVarChar, PortfolioNo),
            SqlDalParam.formatParam("@TravellerID", SqlDbType.NVarChar, TravellerID),
            SqlDalParam.formatParam("@FirstName", SqlDbType.NVarChar, FirstName),
            SqlDalParam.formatParam("@SecondName", SqlDbType.NVarChar, SecondName),
            SqlDalParam.formatParam("@Address", SqlDbType.NVarChar, Address),
            SqlDalParam.formatParam("@CityCode", SqlDbType.NVarChar, CityCode),
            SqlDalParam.formatParam("@City", SqlDbType.NVarChar, City),
            SqlDalParam.formatParam("@ZipCode", SqlDbType.NVarChar, ZipCode),
            SqlDalParam.formatParam("@ItemSKU", SqlDbType.NVarChar, ItemSKU),
            SqlDalParam.formatParam("@ItemDescription", SqlDbType.NVarChar, ItemDescription),
            SqlDalParam.formatParam("@StartDate", SqlDbType.DateTime, StartDate),
            SqlDalParam.formatParam("@EndDate", SqlDbType.DateTime, EndDate),
            SqlDalParam.formatParam("@ReleaseDate", SqlDbType.DateTime, ReleaseDate),
            SqlDalParam.formatParam("@DaysNum", SqlDbType.Int, DaysNum),
            SqlDalParam.formatParam("@EscortNum", SqlDbType.Int, EscortNum),
            SqlDalParam.formatParam("@Department", SqlDbType.Int, Department),
            SqlDalParam.formatParam("@Level", SqlDbType.Int, Level),
            SqlDalParam.formatParam("@Status", SqlDbType.Int, Status),
            SqlDalParam.formatParam("@Tel_Pr_1", SqlDbType.NVarChar, Tel_Pr_1),
            SqlDalParam.formatParam("@Tel_Num_1", SqlDbType.NVarChar, Tel_Num_1),
            SqlDalParam.formatParam("@Tel_Pr_2", SqlDbType.NVarChar, Tel_Pr_2),
            SqlDalParam.formatParam("@Tel_Num_2", SqlDbType.NVarChar, Tel_Num_2),
            SqlDalParam.formatParam("@Tel_Pr_3", SqlDbType.NVarChar, Tel_Pr_3),
            SqlDalParam.formatParam("@Tel_Num_3", SqlDbType.NVarChar, Tel_Num_3),
            SqlDalParam.formatParam("@CleanZakaut", SqlDbType.Bit, CleanZakaut),
            SqlDalParam.formatParam("@Simon_100", SqlDbType.NVarChar, Simon_100),
            SqlDalParam.formatParam("@Nagish", SqlDbType.NVarChar, Nagish),
            SqlDalParam.formatParam("@Request_SH", SqlDbType.NVarChar, Request_SH),
            SqlDalParam.formatParam("@Mahadora", SqlDbType.NVarChar, Mahadora)
        );

        if (ds.Tables[0].Rows[0].ItemArray[0].ToString() == "1")
        {
            return "update";
        }
        else
        {
            return "insert";
        }
    }
	
    public static bool WRiteActionLog(string GovDocketId, string GovTravellerId, string AgnVoucherId, string Makat, DateTime OrderStartDate, DateTime OrderEndDate,
                                      int TipulimCount, double Amount, int MelaveCount, int Cancelation, int HotelId, int ZakayPaysForMelave,
                                      int Level, int GroupCode, int AreaCode, string AgnDocketId, int FixIndication, string RecordType)
    {
        DataSet ds = new DataSet();

        try
        {
            ds = DAL_SQL.ExecuteDataset(DAL_SQL.ConnStr, CommandType.StoredProcedure, "GOV_ActionLogWrite",
                SqlDalParam.formatParam("@GovDocketId", SqlDbType.NVarChar, GovDocketId),
                SqlDalParam.formatParam("@GovTravellerId", SqlDbType.NVarChar, GovTravellerId),
                SqlDalParam.formatParam("@AgnVoucherId", SqlDbType.NVarChar, AgnVoucherId),
                SqlDalParam.formatParam("@Makat", SqlDbType.NVarChar, Makat),
                SqlDalParam.formatParam("@OrderStartDate", SqlDbType.DateTime, OrderStartDate),
                SqlDalParam.formatParam("@OrderEndDate", SqlDbType.DateTime, OrderEndDate),
                SqlDalParam.formatParam("@TipulimCount", SqlDbType.Int, TipulimCount),
                SqlDalParam.formatParam("@Amount", SqlDbType.Decimal, Amount),
                SqlDalParam.formatParam("@MelaveCount", SqlDbType.Int, MelaveCount),
                SqlDalParam.formatParam("@Cancelation", SqlDbType.Int, Cancelation),
                SqlDalParam.formatParam("@HotelId", SqlDbType.Int, HotelId),
                SqlDalParam.formatParam("@ZakayPaysForMelave", SqlDbType.Int, ZakayPaysForMelave),
                SqlDalParam.formatParam("@Level", SqlDbType.Int, Level),
                SqlDalParam.formatParam("@GroupCode", SqlDbType.Int, GroupCode),
                SqlDalParam.formatParam("@AreaCode", SqlDbType.Int, AreaCode),
                SqlDalParam.formatParam("@AgnDocketId", SqlDbType.Int, AgnDocketId),
                SqlDalParam.formatParam("@FixIndication", SqlDbType.Int, FixIndication),
                SqlDalParam.formatParam("@RecordType", SqlDbType.NVarChar, RecordType)
            );

            return true;
        }
        catch (Exception ex)
        {
            Logger.Log(ex.Message);
            return false;
        }
    }

    public static string GetTravellerDetails(string travellerId, string docketId)
    {
        string strXml = string.Empty;
        /*strXml = DAL_SQL.ExecuteXmlString(DAL_SQL.ConnStr, CommandType.StoredProcedure, "Gov_GetTravellerDetails",*/
        strXml = DAL_SQL.ExecuteXmlString(DAL_SQL.ConnStr, CommandType.StoredProcedure, "GOV_GetTravellerDetailsNew",
            SqlDalParam.formatParam("@TravellerID", SqlDbType.NVarChar, 9, travellerId),
            SqlDalParam.formatParam("@DocketID", SqlDbType.NVarChar, 9, docketId)
            );
        return strXml;
    }

    public static DataSet GetTravellerDetailsDS(string travellerId, string docketId)
    {
        DataSet ds = DAL_SQL.ExecuteDataset(DAL_SQL.ConnStr, CommandType.StoredProcedure, "Gov_GetTravellerDetailsDT",
            SqlDalParam.formatParam("@TravellerID", SqlDbType.NVarChar, 9, travellerId),
            SqlDalParam.formatParam("@DocketID", SqlDbType.NVarChar, 9, docketId)
            );
        return ds;
    }

    public static string GetTravellerOrdersHistoryDetails(string travellerId, string docketId)
    {
        string strXml = string.Empty;
        strXml = DAL_SQL.ExecuteXmlString(DAL_SQL.ConnStr, CommandType.StoredProcedure, "GOV_GetTravellerOrdersHistory",
            SqlDalParam.formatParam("@TravellerID", SqlDbType.NVarChar, travellerId),
            SqlDalParam.formatParam("@DocketID", SqlDbType.NVarChar, docketId)
            );
        return strXml;
    }

    public static DataSet GetGeneralAreas(string makat1, string makat2)
    {
        DataSet ds = new DataSet();
        ds = DAL_SQL.ExecuteDataset(DAL_SQL.ConnStr, CommandType.StoredProcedure, "GOV_GetGeneralAreas",
            SqlDalParam.formatParam("@Makat1", SqlDbType.NVarChar, 12, makat1),
            SqlDalParam.formatParam("@Makat2", SqlDbType.NVarChar, 12, makat2)
        );
        return ds;
    }

    public static string SearchAvailableAllocations(int generalAreaId, DateTime fromDate, DateTime toDate, Boolean IsMakatTipulim, int Persons)
    {
        //aviran - original - GOV_SearchAvailableAllocations
        //aviran -GOV_SearchAvailableAllocationsWithDetails-> images and remark
Logger.Log("generalAreaId = " + generalAreaId);
Logger.Log("fromDate = " + fromDate.ToString("dd-MMM-yy"));
Logger.Log("toDate = " + toDate.ToString("dd-MMM-yy"));
Logger.Log("geneIsMakatTipulimralAreaId = " + IsMakatTipulim);
Logger.Log("Persons = " + Persons);

        string result = DAL_SQL.ExecuteXmlString(DAL_SQL.ConnStr, CommandType.StoredProcedure, "GOV_SearchAvailableAllocationsWithDetails",
            SqlDalParam.formatParam("@GeneralAreaId", SqlDbType.Int, 9, generalAreaId),
            SqlDalParam.formatParam("@FromDate", SqlDbType.DateTime, 9, fromDate),
            SqlDalParam.formatParam("@ToDate", SqlDbType.DateTime, 9, toDate),
            SqlDalParam.formatParam("@IsMakatTipulim", SqlDbType.Bit, 9, IsMakatTipulim),
            SqlDalParam.formatParam("@Persons", SqlDbType.Int, 9, Persons)
            );

        return result;
    }
	
	public static string SearchAvailableAttractions(int generalAreaId, DateTime fromDate, DateTime toDate, Boolean IsMakatTipulim, int Persons)
    {
        //aviran - original - GOV_SearchAvailableAllocations
        //aviran -GOV_SearchAvailableAllocationsWithDetails-> images and remark
		//string result = DAL_SQL.ExecuteXmlString(DAL_SQL.ConnStr, CommandType.StoredProcedure, "GOV_SearchAvailableAllocationsWithDetails",
        string result = DAL_SQL.ExecuteXmlString(DAL_SQL.ConnStr, CommandType.StoredProcedure, "GOV_SearchAvailableAllocationsWithDetails_NEW",
            SqlDalParam.formatParam("@GeneralAreaId", SqlDbType.Int, 9, generalAreaId),
            SqlDalParam.formatParam("@FromDate", SqlDbType.DateTime, 9, fromDate),
            SqlDalParam.formatParam("@ToDate", SqlDbType.DateTime, 9, toDate),
            SqlDalParam.formatParam("@IsMakatTipulim", SqlDbType.Bit, 9, IsMakatTipulim),
            SqlDalParam.formatParam("@Persons", SqlDbType.Int, 9, Persons)
            );

        return result;
    }

	
	public static DataSet GOV_GetAttractionResultsForOnlyTreatment(int iSupplierID, DateTime fromDate, DateTime toDate)
    {
        //aviran - original - GOV_SearchAvailableAllocations
        //aviran -GOV_SearchAvailableAllocationsWithDetails-> images and remark
		
        DataSet result = DAL_SQL.ExecuteDataset(DAL_SQL.ConnStr, CommandType.StoredProcedure, "GOV_GetAttractionResultsForOnlyTreatment",
            SqlDalParam.formatParam("@SupplierId", SqlDbType.Int, iSupplierID),
            SqlDalParam.formatParam("@FromDate", SqlDbType.DateTime, 9, fromDate),
            SqlDalParam.formatParam("@ToDate", SqlDbType.DateTime, 9, toDate)
            );

        return result;
    }
	
	
    public static bool UpdateTravellerBalance(string GovDocketId, string GovTravellerId, int OrderNights, int AreaId, string MakatNumber)
    {
        try
        {
            DataSet ds = new DataSet();
            ds = DAL_SQL.ExecuteDataset(DAL_SQL.ConnStr, CommandType.StoredProcedure, "GOV_UpdateTravellerMakatBalance",
                SqlDalParam.formatParam("@GovDocketId", SqlDbType.NVarChar, GovDocketId),
                SqlDalParam.formatParam("@GovTravellerId", SqlDbType.NVarChar, GovTravellerId),
                SqlDalParam.formatParam("@OrderNights", SqlDbType.Int, OrderNights),
                SqlDalParam.formatParam("@ActionType", SqlDbType.NVarChar, "order"),
                SqlDalParam.formatParam("@GeneralAreaId", SqlDbType.Int, AreaId),
                SqlDalParam.formatParam("@MakatNumber", SqlDbType.NVarChar, MakatNumber)
            );
            return true;
        }
        catch (Exception ex)
        {
            Logger.Log(ex.Message);

            return false;
        }

    }


    public static DataSet GetUsageFile(DateTime fromExitDate, DateTime toExitDate, DateTime fromTermDate, DateTime toTermDate, DateTime fromUpdateExitDate, DateTime toUpdateExitDate, DateTime fromUpdateDate, DateTime toUpdateDate, DateTime fromUpdateTermDate, DateTime toUpdateTermDate)
    {
        DataSet ds = new DataSet();
        ds = DAL_SQL.ExecuteDataset(DAL_SQL.ConnStr, CommandType.StoredProcedure, "GOV_GetUsageFile",
            SqlDalParam.formatParam("@fromExitDate", SqlDbType.DateTime, 20, fromExitDate),
            SqlDalParam.formatParam("@toExitDate", SqlDbType.DateTime, 20, toExitDate),
            SqlDalParam.formatParam("@fromTermDate", SqlDbType.DateTime, 20, fromTermDate),
            SqlDalParam.formatParam("@toTermDate", SqlDbType.DateTime, 20, toTermDate),
            SqlDalParam.formatParam("@fromUpdateExitDate", SqlDbType.DateTime, 20, fromUpdateExitDate),
            SqlDalParam.formatParam("@toUpdateExitDate", SqlDbType.DateTime, 20, toUpdateExitDate),
            SqlDalParam.formatParam("@fromUpdateDate", SqlDbType.DateTime, 20, fromUpdateDate),
            SqlDalParam.formatParam("@toUpdateDate", SqlDbType.DateTime, 20, toUpdateDate),
            SqlDalParam.formatParam("@fromUpdateTermDate", SqlDbType.DateTime, 20, fromUpdateTermDate),
            SqlDalParam.formatParam("@toUpdateTermDate", SqlDbType.DateTime, 20, toUpdateTermDate)
        );

        return ds;
    }
    public static DataSet GetUsageFileNew(DateTime fromExitDate, DateTime toExitDate, DateTime fromTermDate, DateTime toTermDate, DateTime fromUpdateExitDate, DateTime toUpdateExitDate, DateTime fromUpdateDate, DateTime toUpdateDate, DateTime fromUpdateTermDate, DateTime toUpdateTermDate)
    {
        DataSet ds = new DataSet();
        ds = DAL_SQL.ExecuteDataset(DAL_SQL.ConnStr, CommandType.StoredProcedure, "GOV_GetUsageFileNew",
            SqlDalParam.formatParam("@fromExitDate", SqlDbType.DateTime, 20, fromExitDate),
            SqlDalParam.formatParam("@toExitDate", SqlDbType.DateTime, 20, toExitDate),
            SqlDalParam.formatParam("@fromTermDate", SqlDbType.DateTime, 20, fromTermDate),
            SqlDalParam.formatParam("@toTermDate", SqlDbType.DateTime, 20, toTermDate),
            SqlDalParam.formatParam("@fromUpdateExitDate", SqlDbType.DateTime, 20, fromUpdateExitDate),
            SqlDalParam.formatParam("@toUpdateExitDate", SqlDbType.DateTime, 20, toUpdateExitDate),
            SqlDalParam.formatParam("@fromUpdateDate", SqlDbType.DateTime, 20, fromUpdateDate),
            SqlDalParam.formatParam("@toUpdateDate", SqlDbType.DateTime, 20, toUpdateDate),
            SqlDalParam.formatParam("@fromUpdateTermDate", SqlDbType.DateTime, 20, fromUpdateTermDate),
            SqlDalParam.formatParam("@toUpdateTermDate", SqlDbType.DateTime, 20, toUpdateTermDate)
        );

        return ds;
    }
    public static DataSet GetHotels()
    {
        DataSet ds = new DataSet();
         
		ds = DAL_SQL.ExecuteDataset(DAL_SQL.ConnStr, CommandType.StoredProcedure, "GOV_GetHotels"
         );

        return ds;
    }
	
	public static DataSet GetHotelsByArea(string generalAreaId, string agencyId, string systemType, string lang )
	{
		DataSet ds = new DataSet();
		string serviceType = "2";//Hotels
		
        try
        {
			/* ds = DAL_SQL.RunSqlDataSet(string.Format(@"SELECT id , name_1255, name_1252, name_1251 FROM [Agency_Admin].[dbo].[SUPPLIERS] 
													   WHERE area_id in (SELECT A.id FROM agency_admin.dbo.AREAS as A WHERE general_area_id = {0})
													   AND id in (SELECT supplier_id FROM AGN_{1}_00{2}.dbo.SUPPLIERS_to_PRODUCT_TYPES WHERE product_type = {3})
													   AND id in (SELECT supplier_id FROM AGN_{1}_00{2}.dbo.SUPPLIER_DETAILS WHERE supplier_id = id)
													   order by name_1255", generalAreaId, systemType, agencyId, serviceType));
			*/

			ds = DAL_SQL.RunSqlDataSet(string.Format(@" SELECT	S.id, S.name_1255, S.name_1252, S.name_1251
														FROM      dbo.SUPPLIER_DETAILS SD INNER JOIN
																dbo.SUPPLIERS_to_PRODUCT_TYPES S_T_P ON SD.supplier_id = S_T_P.supplier_id INNER JOIN
																AGENCY_ADMIN.dbo.SUPPLIERS S INNER JOIN
																AGENCY_ADMIN.dbo.AREAS A ON S.area_id = A.id ON SD.supplier_id = S.id
																  left outer join Agency_Admin.dbo.HOTELS_NETWORKS HN on HN.id=SD.network_id
														WHERE
														(S_T_P.product_type = {0}) AND (SD.isActive = 1) 
														AND A.general_area_id = {1}
														order by S.name_{2}", serviceType, generalAreaId, lang));														
        }
        catch(Exception ex)
        {
            Logger.Log(string.Format("Failed to get hotels. ({0})", ex.Message));
            throw ex;
        }

        return ds;
	}
	
	public static DataSet GetAllHotels(string agencyId, string systemType, string lang )
    {
        DataSet ds = new DataSet();
		string serviceType = "2";//Hotels
		
        try
        {
			/* ds = DAL_SQL.RunSqlDataSet(string.Format(@"SELECT id , name_1255, name_1252, name_1251 FROM [Agency_Admin].[dbo].[SUPPLIERS] 
													   WHERE area_id in (SELECT A.id FROM agency_admin.dbo.AREAS as A WHERE general_area_id = {0})
													   AND id in (SELECT supplier_id FROM AGN_{1}_00{2}.dbo.SUPPLIERS_to_PRODUCT_TYPES WHERE product_type = {3})
													   AND id in (SELECT supplier_id FROM AGN_{1}_00{2}.dbo.SUPPLIER_DETAILS WHERE supplier_id = id)
													   order by name_1255", generalAreaId, systemType, agencyId, serviceType));
			*/

			ds = DAL_SQL.RunSqlDataSet(string.Format(@" SELECT	S.id, S.name_1255, S.name_1252, S.name_1251
														FROM      dbo.SUPPLIER_DETAILS SD INNER JOIN
																dbo.SUPPLIERS_to_PRODUCT_TYPES S_T_P ON SD.supplier_id = S_T_P.supplier_id INNER JOIN
																AGENCY_ADMIN.dbo.SUPPLIERS S INNER JOIN
																AGENCY_ADMIN.dbo.AREAS A ON S.area_id = A.id ON SD.supplier_id = S.id
																  left outer join Agency_Admin.dbo.HOTELS_NETWORKS HN on HN.id=SD.network_id
														WHERE
														(S_T_P.product_type = {0}) AND (SD.isActive = 1) 
														
														order by S.name_{1}", serviceType, lang));
        }
        catch(Exception ex)
        {
            Logger.Log(string.Format("Failed to get hotels. ({0})", ex.Message));
            throw ex;
        }

        return ds;
    }

	public static DataSet GetAreas()
    {
        DataSet ds = new DataSet();

        try
        {
            ds = DAL_SQL.RunSqlDataSet("SELECT id , name_1255, name_1252, name_1251 FROM [Agency_Admin].[dbo].[GENARAL_AREAS] WHERE gov_general_area = 0 order by name_1255");
        }
        catch (Exception ex)
        {
            Logger.Log(string.Format("Failed to get hotels. ({0})", ex.Message));
            throw ex;
        }

        return ds;
    }
	
    public static DataSet GetBCInvoicesForInvoicesFile(DateTime fromDate, DateTime toDate)
    {
        DataSet ds = new DataSet();
        ds = DAL_SQL.ExecuteDataset(DAL_SQL.ConnStr, CommandType.StoredProcedure, "GOV_GetBCInvoicesForInvoicesFile",
            SqlDalParam.formatParam("@fromDate", SqlDbType.DateTime, 20, fromDate),
            SqlDalParam.formatParam("@toDate", SqlDbType.DateTime, 20, toDate)
        );

        return ds;
    }

    public static DataSet GetCurrentAllocationsForReport(DateTime currentDate, int supplier_id)
    {
        DataSet ds = new DataSet();
        ds = DAL_SQL.ExecuteDataset(DAL_SQL.ConnStr, CommandType.StoredProcedure, "GOV_GetCurrentAllocationsForReport",
            SqlDalParam.formatParam("@currentDate", SqlDbType.DateTime, 20, currentDate),
            SqlDalParam.formatParam("@supplier_id", SqlDbType.Int, 20, supplier_id)
        );

        return ds;
    }

    public static DataSet GetCurrentAllocationsForReportOrdersByDate(DateTime currentDate, int supplier_id)
    {
        DataSet ds = new DataSet();
        ds = DAL_SQL.ExecuteDataset(DAL_SQL.ConnStr, CommandType.StoredProcedure, "GOV_GetCurrentAllocationsForReportOrdersByDateNew",
            SqlDalParam.formatParam("@currentDate", SqlDbType.DateTime, 20, currentDate),
            SqlDalParam.formatParam("@supplier_id", SqlDbType.Int, 20, supplier_id)
        );

        return ds;
    }
	
	public static DataSet GetCurrentAllocationsForReportOrdersByDateRange(DateTime fromDate,DateTime toDate, int supplier_id)
    {
        DataSet ds = new DataSet();
        ds = DAL_SQL.ExecuteDataset(DAL_SQL.ConnStr, CommandType.StoredProcedure, "GOV_GetCurrentAllocationsForReportOrdersByDateRange",
            SqlDalParam.formatParam("@fromDate", SqlDbType.DateTime, 20, fromDate),
			SqlDalParam.formatParam("@toDate", SqlDbType.DateTime, 20, toDate),
            SqlDalParam.formatParam("@supplier_id", SqlDbType.Int, 20, supplier_id)
        );

        return ds;
    }



    public static DataSet GetSuppliersForOrdersReport(DateTime fromDate, DateTime toDate)
    {
        DataSet ds = new DataSet();
        ds = DAL_SQL.ExecuteDataset(DAL_SQL.ConnStr, CommandType.StoredProcedure, "GOV_GetSuppliersForOrdersReport",
            SqlDalParam.formatParam("@fromDate", SqlDbType.DateTime, 20, fromDate),
            SqlDalParam.formatParam("@toDate", SqlDbType.DateTime, 20, toDate)
        );

        return ds;
    }
	
	public static DataSet GetSuppliersForOrdersReportByArea(string generalAreaId, DateTime fromDate, DateTime toDate)
    {
        DataSet ds = new DataSet();
        ds = DAL_SQL.ExecuteDataset(DAL_SQL.ConnStr, CommandType.StoredProcedure, "GOV_GetSuppliersForOrdersReportByArea",
            SqlDalParam.formatParam("@fromDate", SqlDbType.DateTime, 20, fromDate),
            SqlDalParam.formatParam("@toDate", SqlDbType.DateTime, 20, toDate),
			SqlDalParam.formatParam("@generalAreaId", SqlDbType.NVarChar, 20, generalAreaId)
        );

        return ds;
    }

    public static DataSet GetRoomsForOrdersReport(DateTime currentDate, DateTime fromDate, DateTime toDate, int supplierId)
    {
        DataSet ds = new DataSet();
        ds = DAL_SQL.ExecuteDataset(DAL_SQL.ConnStr, CommandType.StoredProcedure, "GOV_GetRoomsForOrdersReport",
            SqlDalParam.formatParam("@currentDate", SqlDbType.DateTime, 20, currentDate),
            SqlDalParam.formatParam("@fromDate", SqlDbType.DateTime, 20, fromDate),
            SqlDalParam.formatParam("@toDate", SqlDbType.DateTime, 20, toDate),
            SqlDalParam.formatParam("@supplierId", SqlDbType.Int, 20, supplierId)
        );

        return ds;
    }

    public static DataSet GetBCInvoicesPerformaDetailsForInvoicesFile(int invoiceId)
    {
        DataSet ds = new DataSet();
        ds = DAL_SQL.ExecuteDataset(DAL_SQL.ConnStr, CommandType.StoredProcedure, "GOV_GetBCInvoicesPerformaDetailsForInvoicesFile",
            SqlDalParam.formatParam("@invoiceId", SqlDbType.Int, 20, invoiceId)
        );

        return ds;
    }



    public static bool UpdateMakatDetails(string MakatNumber,
                                            string MakatDescription,
                                            string GeneralAreas,
                                            string MinNights,
                                            string MaxNights,
                                            string OneTimeUssage,
                                            string MakatTipulim,
                                            string Allow5And5Nights,
                                            string OfficeRemarkForOrder,
                                            string StartOrderDateFromTodayMin,
                                            string StartOrderDateFromTodayMax,
                                            string AllowedToAdd5NightForPay,
                                            string VoucherRemark
        )
    {
        try
        {
            DataSet ds = new DataSet();
            ds = DAL_SQL.ExecuteDataset(DAL_SQL.ConnStr, CommandType.StoredProcedure, "[GOV_UpdateMakatDetails]",
                SqlDalParam.formatParam("@MakatNumber", SqlDbType.NVarChar, MakatNumber),
                SqlDalParam.formatParam("@MakatDescription", SqlDbType.NVarChar, MakatDescription),
                SqlDalParam.formatParam("@GeneralAreas", SqlDbType.NVarChar, GeneralAreas),
                SqlDalParam.formatParam("@MinNights", SqlDbType.NVarChar, MinNights),
                SqlDalParam.formatParam("@MaxNights", SqlDbType.NVarChar, MaxNights),
                SqlDalParam.formatParam("@OneTimeUssage", SqlDbType.NVarChar, OneTimeUssage),
                SqlDalParam.formatParam("@MakatTipulim", SqlDbType.NVarChar, MakatTipulim),
                SqlDalParam.formatParam("@Allow5And5Nights", SqlDbType.NVarChar, Allow5And5Nights),
                SqlDalParam.formatParam("@OfficeRemarkForOrder", SqlDbType.NVarChar, OfficeRemarkForOrder),
                SqlDalParam.formatParam("@StartOrderDateFromTodayMin", SqlDbType.NText, StartOrderDateFromTodayMin),
                SqlDalParam.formatParam("@StartOrderDateFromTodayMax", SqlDbType.NVarChar, StartOrderDateFromTodayMax),
                SqlDalParam.formatParam("@AllowedToAdd5NightForPay", SqlDbType.NVarChar, AllowedToAdd5NightForPay),
                SqlDalParam.formatParam("@VoucherRemark", SqlDbType.NText, VoucherRemark)

            );
            return true;
        }
        catch (Exception ex)
        {
            Logger.Log(ex.Message);

            return false;
        }

    }


    public static DataSet GetAllGeneralAreas()
    {
        DataSet ds = new DataSet();
        ds = DAL_SQL.ExecuteDataset(DAL_SQL.ConnStr, CommandType.StoredProcedure, "GOV_GetAllGeneralAreas"
        );
        return ds;
    }

    public static DataSet GetMakatDetails()
    {
        DataSet ds = new DataSet();
        ds = DAL_SQL.ExecuteDataset(DAL_SQL.ConnStr, CommandType.StoredProcedure, "GOV_GetMakatDetails"
        );
        return ds;
    }


    //gets num of melavim acc. to voucher. returns 0, if called with null or empty
    public static int GetVoucherMelavim(string bundle_id)
    {
        if (bundle_id == null || bundle_id == "")
        {
            return 0;
        }
        int voucher_melavim_number;
        string strMelave = "Select count(BTT.traveller_id) as counter,BTT.bundle_id from BUNDLES_to_TRAVELLERS AS BTT where BTT.bundle_id = " + bundle_id + " and (BTT.trav_pay=0 and BTT.subsid=0) group by BTT.bundle_id";
        try
        {
            voucher_melavim_number = int.Parse(DAL_SQL.RunSql(strMelave));
        }
        catch (Exception ex)
        {
            Logger.Log(ex.Message);

            voucher_melavim_number = 0;
        }
        return voucher_melavim_number;
    }

    public static string GetRoomsNumByBundle(int bundleId)
    {

        DataSet ds = new DataSet();
        ds = DAL_SQL.ExecuteDataset(DAL_SQL.ConnStr, CommandType.StoredProcedure, "GOV_GetHotelRoomsByBundleAndSubsid",
            SqlDalParam.formatParam("@bundleId", SqlDbType.Int, 20, bundleId)
            );
        if (ds.Tables.Count > 0)
        {
            string ss = ds.Tables[0].Rows[0][0].ToString();
            return ss;
        }
        return "0";
    }

    public static DataSet GetInvoicesForInvoicesFileNew(DateTime fromExitDate, DateTime toExitDate, DateTime fromTermDate, DateTime toTermDate, DateTime fromUpdateExitDate, DateTime toUpdateExitDate, DateTime fromUpdateDate, DateTime toUpdateDate, DateTime fromUpdateTermDate, DateTime toUpdateTermDate)
    {
        DataSet ds = new DataSet();
        ds = DAL_SQL.ExecuteDataset(DAL_SQL.ConnStr, CommandType.StoredProcedure, "GOV_GetInvoicesForInvoicesFileNew",
            SqlDalParam.formatParam("@fromExitDate", SqlDbType.DateTime, 20, fromExitDate),
            SqlDalParam.formatParam("@toExitDate", SqlDbType.DateTime, 20, toExitDate),
            SqlDalParam.formatParam("@fromTermDate", SqlDbType.DateTime, 20, fromTermDate),
            SqlDalParam.formatParam("@toTermDate", SqlDbType.DateTime, 20, toTermDate),
            SqlDalParam.formatParam("@fromUpdateExitDate", SqlDbType.DateTime, 20, fromUpdateExitDate),
            SqlDalParam.formatParam("@toUpdateExitDate", SqlDbType.DateTime, 20, toUpdateExitDate),
            SqlDalParam.formatParam("@fromUpdateDate", SqlDbType.DateTime, 20, fromUpdateDate),
            SqlDalParam.formatParam("@toUpdateDate", SqlDbType.DateTime, 20, toUpdateDate),
            SqlDalParam.formatParam("@fromUpdateTermDate", SqlDbType.DateTime, 20, fromUpdateTermDate),
            SqlDalParam.formatParam("@toUpdateTermDate", SqlDbType.DateTime, 20, toUpdateTermDate)
        );

        return ds;
    }
    public static DataSet GOV_UpdateInvoices(int inid, string gov_invId)
    {
        DataSet ds = new DataSet();
        ds = DAL_SQL.ExecuteDataset(DAL_SQL.ConnStr, CommandType.StoredProcedure, "GOV_UpdateInvoices",
             SqlDalParam.formatParam("@invoice_id", SqlDbType.Int, inid),
              SqlDalParam.formatParam("@GOV_INV", SqlDbType.VarChar, gov_invId)
          );
        return ds;
    }


    public static DataSet GetNegativeSubsidAndTravPayReport(DateTime fromDate, DateTime toDate)
    {
        DataSet ds = new DataSet();
        ds = DAL_SQL.ExecuteDataset(DAL_SQL.ConnStr, CommandType.StoredProcedure, "GOV_GetBundlesWithNegativeSubsid",
            SqlDalParam.formatParam("@fromDate", SqlDbType.DateTime, 20, fromDate),
            SqlDalParam.formatParam("@toDate", SqlDbType.DateTime, 20, toDate)
            );
        return ds;
    }

    public static DataSet GetNotPaidVouchers(DateTime fromDate, DateTime toDate)
    {
        DataSet ds = new DataSet();
        ds = DAL_SQL.ExecuteDataset(DAL_SQL.ConnStr, CommandType.StoredProcedure, "GOV_GetUnpaidVouchers_old",
            SqlDalParam.formatParam("@fromDate", SqlDbType.DateTime, 20, fromDate),
            SqlDalParam.formatParam("@toDate", SqlDbType.DateTime, 20, toDate)
            );
        return ds;
    }
	
	public static DataSet GetNotPaidVouchersByServiceType(DateTime fromDate, DateTime toDate, int ServiceType)
    {
        DataSet ds = new DataSet();
        ds = DAL_SQL.ExecuteDataset(DAL_SQL.ConnStr, CommandType.StoredProcedure, "GOV_GetUnpaidVouchersByServiceType",
            SqlDalParam.formatParam("@fromDate", SqlDbType.DateTime, 20, fromDate),
            SqlDalParam.formatParam("@toDate", SqlDbType.DateTime, 20, toDate),
			SqlDalParam.formatParam("@ServiceType", SqlDbType.Int, 20, ServiceType)
            );
        return ds;
    }
	
	public static DataSet GetNotPaidVouchers(DateTime fromDate, DateTime toDate, int ServiceType, int VoucherStatus)
    {
        DataSet ds = new DataSet();
        ds = DAL_SQL.ExecuteDataset(DAL_SQL.ConnStr, CommandType.StoredProcedure, "GOV_GetUnpaidVouchers",
            SqlDalParam.formatParam("@fromDate", SqlDbType.DateTime, 20, fromDate),
            SqlDalParam.formatParam("@toDate", SqlDbType.DateTime, 20, toDate),
            SqlDalParam.formatParam("@ServiceType", SqlDbType.Int, 20, ServiceType),
            SqlDalParam.formatParam("@VoucherStatus", SqlDbType.Int, 20, VoucherStatus)
            );
        return ds;
    }
	
    public static DataSet GetStatusIndicators()
    {
        DataSet ds = new DataSet();
        ds = DAL_SQL.ExecuteDataset(DAL_SQL.ConnStr, CommandType.StoredProcedure, "GOV_GetStatusIndicators"
        );

        return ds;
    }

    public static DataSet GetHolidays()
    {
        DataSet ds = new DataSet();
        ds = DAL_SQL.ExecuteDataset(DAL_SQL.ConnStr, CommandType.StoredProcedure, "GOV_GetHolidays"
        );

        return ds;
    }

    public static void UpsertHolidays(int id, DateTime HolidayDate, string HolidayDescription)
    {
        DataSet ds = new DataSet();
        ds = DAL_SQL.ExecuteDataset(DAL_SQL.ConnStr, CommandType.StoredProcedure, "[GOV_HOLIDAYS_UPSERT]",
            SqlDalParam.formatParam("@id", SqlDbType.Int, id),
            SqlDalParam.formatParam("@HolidayDate", SqlDbType.DateTime, HolidayDate),
            SqlDalParam.formatParam("@HolidayDescription", SqlDbType.NVarChar, HolidayDescription)
        );
    }

    public static void DeleteHolidays(int id)
    {
        DataSet ds = new DataSet();
        ds = DAL_SQL.ExecuteDataset(DAL_SQL.ConnStr, CommandType.StoredProcedure, "[GOV_HOLIDAYS_DELETE]",
            SqlDalParam.formatParam("@id", SqlDbType.Int, id)
        );
    }

    public static DataSet GetTravellersBalanceReport()
    {
        DataSet ds = new DataSet();
        ds = DAL_SQL.ExecuteDataset(DAL_SQL.ConnStr, CommandType.StoredProcedure, "[GOV_GetTravellersBalanceReport]"
        );

        return ds;
    }
    public static DataSet GetRoomsBalance(DateTime fromDate, DateTime toDate)
    {
        DataSet ds = new DataSet();
        ds = DAL_SQL.ExecuteDataset(DAL_SQL.ConnStr, CommandType.StoredProcedure, "GOV_GetRoomsBalance",
            SqlDalParam.formatParam("@fromDate", SqlDbType.DateTime, 20, fromDate),
            SqlDalParam.formatParam("@toDate", SqlDbType.DateTime, 20, toDate)
            );
        return ds;
    }
	
    public static int update_referral_number(string referral_number, int voucherId)
    {
        try
        {
            DataSet ds = new DataSet();
            ds = DAL_SQL.ExecuteDataset(DAL_SQL.ConnStr, CommandType.StoredProcedure, "GOV_update_referral_number",
                SqlDalParam.formatParam("@VoucherId", SqlDbType.Int, voucherId),
                SqlDalParam.formatParam("@referral_number", SqlDbType.NVarChar, referral_number)

            );
            return voucherId;
        }
        catch (Exception ex)
        {
            Logger.Log(ex.Message);

            return -1;
        }

    }
    public static DataTable GetAgencyDetails(int agencyid)
    {
        DataSet ds = new DataSet();
        string query = "SELECT * FROM AGENCIES WHERE id =@agencyid";

        ds = DAL_SQL.ExecuteDataset(DAL_SQL.ConnStr, CommandType.Text, query,
          SqlDalParam.formatParam("@agencyid", SqlDbType.Int, agencyid));
        DataTable dt = ds.Tables[0];


        return dt;



    }
	
	  public static DataSet GetClerkDetails(int agencyid)
    {
        DataSet ds = new DataSet();
        string query = "SELECT login_name, password FROM CLERKS WHERE id =@agencyid";

        ds = DAL_SQL.ExecuteDataset(DAL_SQL.ConnStr, CommandType.Text, query,
          SqlDalParam.formatParam("@agencyid", SqlDbType.Int, agencyid));


        return ds;
    }
	
	public static DataSet getAmountOfkidsAndAdults(int iBundleId)
    {
        string query = @"SELECT HRT.adt_amount, HRT.chd_amount 
                         FROM Agency_Admin.dbo.HOTEL_ROOM_TYPE AS HRT inner join
                              HOTELS_TO_ROOMS_TYPE AS HTRT ON HRT.id = HTRT.room_type_id
	                     WHERE HTRT.bundle_id = " + iBundleId;

        DataSet ds = DAL_SQL.RunSqlDataSet(query);

		return ds;
    }

    public static string getRequstSHBytravellerId(string iTravelerId, string makat)
    {
        return DAL_SQL.GetRecord("GOV_TRAVELLERS", "Request_SH", iTravelerId, " status = 1 and TravellerID ");
    }

    public static string getMahadoraBytravellerId(string iTravelerId, string makat)
    {
        return DAL_SQL.GetRecord("GOV_TRAVELLERS", "Mahadora", iTravelerId, " status = 1 and TravellerID ");
    }
	
	
     ///////////////////////////////////////////////////// ATTRACTIONS METHODS /////////////////////////////////////////////////////////

    //Chen 9/11.  Attraction. new code.
    public static int createBundle(string iDocket, int iServiceType, string iTodayDate, string iClerkId)
    {
        string sqlString = string.Empty;
        int newBundleID = getMaxTableId("BUNDLES");
        Logger.Log("creaating bundle row - id = " + newBundleID);
        sqlString = " INSERT INTO BUNDLES (id, docket_id, service_type, cdate, author_id, last_update_date, last_update_clerk_id) "
					+ " OUTPUT INSERTED.ID "
                    + " VALUES ((SELECT max(B1.id) + 1 FROM BUNDLES B1), " + iDocket + ", " + iServiceType + ", '" + iTodayDate + "', " + iClerkId + ",'"
                    + iTodayDate + "', " + iClerkId + " )";
        try
        {
            string bundleId = DAL_SQL.RunSql(sqlString);
			int.TryParse(bundleId, out newBundleID);
        }
        catch (Exception ex)
        {
            Logger.Log("Failed to insert new BUNDLE row. - " + ex.Message);
        }

        return newBundleID;
    }

    //Create OTHER row, for attraction.
    public static int createOtherRow(int iBundleId, string iAttractionName, string iSupplierId, string iOrigin, string iDestin, string iFromDate, string iToDate,
                                     string iFromTime, string iToTime, string iNumOfNights, string iOrderNum, string iQuantity, string iConfirmedBy, string iStatus,
                                     string iClerkId, string iTodayDate, bool isFiveTipulim)
    {
        string sqlString = string.Empty;
        int otherMaxId = -1;

        otherMaxId = getMaxTableId("OTHER");
        Logger.Log("creaating OTHER row - id = " + otherMaxId);

        if (otherMaxId != -1)
        {
				
            sqlString = " INSERT INTO OTHER ( " + " id             , bundle_id ,name          , location          ,departure_location  ,destination_location "
                                                + ",entrance_date  , exit_date ,entrance_time , exit_time         ,nights              ,order_number "
                                                + ",supplier_id    , qty       ,confirmed_by  , service_status_id , data_thru_clerk_id ,data_recieved_date "
                                                + ",gov_4and5tipulim_selected	) "
					+ " OUTPUT INSERTED.ID "
                    + " VALUES((SELECT max(B1.id) + 1 FROM OTHER B1),'" + iBundleId + "', " + " N'" + iAttractionName + "', " + iSupplierId + " , "
                    + " N'" + iOrigin + "', " + " N'" + iDestin + "', " + " '" + iFromDate + "', '" + iToDate + "', '"
                    + iFromTime + "' , " + " '" + iToTime + "', " + " '" + iNumOfNights + "', '" + iOrderNum + "', " + " '" + iSupplierId + "','"
                    + iQuantity + "', " + " N'" + iConfirmedBy + "', '" + iStatus + "', "
                    + " '" + iClerkId + "', '" + iTodayDate + "', '" + isFiveTipulim + "')";
					
			Logger.Log("sql ---- " + sqlString);
        }

        try
        {
            string retVal = DAL_SQL.RunSql(sqlString);
			int.TryParse(retVal, out otherMaxId);
        }
        catch (Exception ex)
        {
            Logger.Log("Failed to insert new OTHER row. - " + ex.Message);
        }

        return otherMaxId;
    }

    /// <summary>
    /// <para />
    /// Insert new row in BUNDLES_to_SUPPLIERS_ADDS, table that contains all details on supplier attraction 
    /// in a specific docket
    /// 09/11/16
    /// </summary>
    /// <param name="iBundleId">Bundle Id</param>
    /// <param name="iSupplierId">Supplier id of the attraction supplier</param>
    /// <param name="iAddId">Add id of the type of attraction</param>
    /// <param name="iQuantity">The amount of days the traveller order</param>
    /// <param name="iFromDate">Start date of attraction</param>
    /// <param name="iToDate">End date of attraction</param>
    /// <returns>new BundleToSupplier ID</returns>
    public static int AddNewBundleToSupplierAddsRow(int iBundleId, string iSupplierId, string iAddId, string iQuantity, DateTime iFromDate, DateTime iToDate, bool is4Plus1)
    {
        string query = string.Empty;

        int newBundleToSupplierID = getMaxTableId("BUNDLES_to_SUPPLIERS_ADDS");
        Logger.Log("creaating BUNDLES_to_SUPPLIERS_ADDS row - id = (SELECT max(B1.id) + 1 FROM BUNDLES_to_SUPPLIERS_ADDS B1)");


        double addAmount;

        try
        {
            //bruto price as amount
            addAmount = GetBrutoPriceForAttraction(iBundleId, iSupplierId, iAddId, iQuantity, iFromDate, iToDate);

            if (is4Plus1)
            {
                int dayDiff = (iToDate-iFromDate).Days;
                
                if (dayDiff == 4)
                {
					//Chen. no need, because when pressing 'accounting' button  the SUBSID change according to this value. so this value should be for 4 days only.
                    addAmount = addAmount * 5 / 4;
                }
            }

            query = "INSERT INTO BUNDLES_to_SUPPLIERS_ADDS (id, bundle_id, supplier_id, supplier_add_id, quantity, amount) "
                                  + " OUTPUT INSERTED.ID "
								  + " VALUES( (SELECT max(B1.id) + 1 FROM BUNDLES_to_SUPPLIERS_ADDS B1), '" + iBundleId + "', "
                                  + " '" + iSupplierId + "', '" + iAddId + "', '" + iQuantity + "', '" + addAmount + "' ) ";
            string retVal = DAL_SQL.RunSql(query);
			int.TryParse(retVal, out newBundleToSupplierID);
        }
        catch (Exception ex)
        {
            Logger.Log("Failed to insert new BUNDLES_to_SUPPLIERS_ADDS row. - " + ex.Message);
        }

        return newBundleToSupplierID;
    }

    public static int AttachBundleToTraveller(string iSupplierId, string iTravellerId, int iBundleId, string iAddId,
                                                string Tax,
                                                string FlightAddAmount, string TravellerMarkUp,
                                                string TravPay, string from_date,
                                                string to_date, string quantity, string TourAmount,
                                                string FlightTaxAmount, string TourAddAmount, string VisaAmount,
                                                string InsuranceAmount, string tremark, string Ticket,
                                                string TravellerType, string NetoAmount, bool is4Plus1)
    {
        int newBundleToTravellerId = getMaxTableId("BUNDLES_to_TRAVELLERS");

        string query = string.Empty;
        double addAmount;

        Logger.Log("creaating BUNDLES_to_SUPPLIERS_ADDS row - id = (SELECT max(B1.id) + 1 FROM BUNDLES_to_TRAVELLERS B1)");

        try
        {
            string travellerIdFromTravellers = getTravellerId(iTravellerId);
            addAmount = GetBrutoPriceForAttraction(iBundleId, iSupplierId, iAddId, quantity, DateTime.Parse(from_date), DateTime.Parse(to_date));

            double subsid = addAmount;
            if (is4Plus1)
            {
                int dayDiff = (DateTime.Parse(to_date) - DateTime.Parse(from_date)).Days;
				Logger.Log("got 4+1,  days = " + dayDiff);
				
                if (dayDiff == 4)
                {
                    addAmount = addAmount * 5 / 4;
                }
                else if (dayDiff == 5) // 5 nights
                {
                    subsid = subsid * 4 / 5;
                }

                TravPay = (addAmount - subsid).ToString();
				Logger.Log("addAmount = " + addAmount +" subsid = " + subsid + " TravPay = " + TravPay);
            }

			double travellerPrice = addAmount;
            double travellerTotal = addAmount;

			
            query = " INSERT INTO BUNDLES_to_TRAVELLERS ( " +
                  " id,						bundle_id,				traveller_id,		  " +
                  "	amount,					price,					tax,				  " +
                  "	flight_add_amount,		total_to_sup,			mark_up,			  " +
                  "	subsid,					trav_pay,				from_date,			  " +
                  "	to_date,				quantity,				tour_amount,		  " +
                  " flight_tax_amount,		tour_add_amount,		visa_amount,		  " +
                  "	insurance_amount,		remark,					f_ticket,			  " +
                  "	traveller_type,			neto_amount,            supplier_add_id      )" +
                  " VALUES " +
                  "	((SELECT max(B1.id) + 1 FROM BUNDLES_to_TRAVELLERS B1),  " + iBundleId + "," + travellerIdFromTravellers + ",    " +
                  " '" + addAmount + "',			" + travellerPrice + ",	" + Tax + ",			  " +
                  "	" + FlightAddAmount + ",	" + travellerTotal + ",	" + TravellerMarkUp + "," +
                  "	" + subsid + ",			" + TravPay + ",			'" + from_date + "',	  " +
                  " '" + to_date + "',		" + quantity + ",			" + TourAmount + ",	  " +
                  "	" + FlightTaxAmount + ",	" + TourAddAmount + ",		" + VisaAmount + ",	  " +
                  "	" + InsuranceAmount + ",	N'" + tremark + "',		'" + Ticket + "',		  " +
                  " N'" + TravellerType + "', " + NetoAmount + "," + iAddId + ")";
			Logger.Log(query);
            DAL_SQL.RunSql(query);
        }
        catch (Exception ex)
        {
            Logger.Log("Failed to insert new BUNDLES_to_SUPPLIERS_ADDS row. - " + ex.Message);
        }
        return newBundleToTravellerId;
    }

    /// <summary>
    /// Get the maximum id number of a table by a given table name
    /// </summary>
    /// <param name="iTableName">table name</param>
    /// <returns>int max id of table</returns>
    public static int getMaxTableId(string iTableName)
    {
        DataSet ds = new DataSet();
        int maxRowID = -1;
        string query = "SELECT max(id) FROM " + iTableName;

        try
        {
            ds = DAL_SQL.RunSqlDataSet(query);

            //Retrun only one row with one column.
            if (ds != null && ds.Tables[0] != null && ds.Tables[0].Rows[0] != null && ds.Tables[0].Rows[0].ItemArray[0] != null)
            {
                maxRowID = int.Parse(ds.Tables[0].Rows[0].ItemArray[0].ToString());
            }
        }
        catch (Exception ex)
        {
            Logger.Log("Failed to get maxTableRowId : " + ex.Message);
        }

        return maxRowID + 1;
    }

    /// <summary>
    /// Get the total brutto price of attraction.
    /// </summary>
    /// <param name="iBundleId"></param>
    /// <param name="iSupplierId"></param>
    /// <param name="iAddId"></param>
    /// <param name="iQuantity"></param>
    /// <param name="iFromDate"></param>
    /// <param name="iToDate"></param>
    /// <returns>int price brutto</returns>
    public static double GetBrutoPriceForAttraction(int iBundleId, string iSupplierId, string iAddId, string iQuantity, DateTime iFromDate, DateTime iToDate)
    {
        double priceBrutto = 0;

		Logger.Log(string.Format("xxxxxx iBundleId {0}, iSupplierId {1},  iAddId {2},  iQuantity {3},  iFromDate {4},  iToDate {5}",
		iBundleId, iSupplierId,  iAddId,  iQuantity,  iFromDate.ToString(),  iToDate.ToString()));
		
		int daysDiff;
        TimeSpan ts = new TimeSpan();

        ts = iToDate - iFromDate;
        daysDiff = ts.Days;
        //means that choosed makat astma (minimum order days is 14)
        if (daysDiff != int.Parse(iQuantity) && daysDiff >= 14)
        {
            //Need to calculate the amount for <quantity> days and not for days of the order.
            iToDate = iFromDate.AddDays(int.Parse(iQuantity));
        }
		
        try
        {
            //get the total amount of the price by dates and supplier id - in order to fill the addAmount variable (necessary for BUNDLES_to_SUPPLIERS_ADDS creation)
            DataSet ds = new DataSet();
            ds = DAL_SQL.ExecuteDataset(DAL_SQL.ConnStr, CommandType.StoredProcedure, "GOV_GetPricesForSupplierAdd",
                SqlDalParam.formatParam("@supplierId", SqlDbType.Int, iSupplierId),
                SqlDalParam.formatParam("@addId", SqlDbType.Int, iAddId),
                SqlDalParam.formatParam("@fromDate", SqlDbType.Date, iFromDate),
                SqlDalParam.formatParam("@toDate", SqlDbType.Date, iToDate)
            );

            if (ds != null && ds.Tables[0] != null && ds.Tables[0].Rows[0] != null && ds.Tables[0].Rows[0] != null)
            {
                Logger.Log("ds.Tables[0].Rows[0]['PriceBrutto'].ToString() ====    " + ds.Tables[0].Rows[0]["PriceBrutto"].ToString());

                priceBrutto = double.Parse(ds.Tables[0].Rows[0]["PriceBrutto"].ToString());
            }
        }
        catch (Exception ex)
        {
            if (ex is FormatException || ex is OverflowException || ex is ArgumentNullException)
            {
                Logger.Log("Failed to parse price brutto to int. exception Thrown From ** DAL_SQL_Helper - getTotalPriceAmountBrutoOfAttraction: " + Environment.NewLine + ex.Message);
            }
            else
            {
                Logger.Log("Failed. exception Thrown From ** DAL_SQL_Helper - getTotalPriceAmountBrutoOfAttraction" + Environment.NewLine + ex.Message);
            }
        }

        return priceBrutto;
    }
	
	
	public static double GetNettoPriceForAttraction(int iBundleId, string iSupplierId, string iAddId, string iQuantity, DateTime iFromDate, DateTime iToDate)
    {
        double priceNetto = 0;

        Logger.Log(string.Format("xxxxxx iBundleId {0}, iSupplierId {1},  iAddId {2},  iQuantity {3},  iFromDate {4},  iToDate {5}",
        iBundleId, iSupplierId, iAddId, iQuantity, iFromDate.ToString(), iToDate.ToString()));

        int daysDiff;
        TimeSpan ts = new TimeSpan();

        ts = iToDate - iFromDate;
        daysDiff = ts.Days;
        //means that choosed makat astma (minimum order days is 14)
        if (daysDiff != int.Parse(iQuantity) && daysDiff >= 14)
        {
            //Need to calculate the amount for <quantity> days and not for days of the order.
            iToDate = iFromDate.AddDays(int.Parse(iQuantity));
        }

        try
        {
            //get the total amount of the price by dates and supplier id - in order to fill the addAmount variable (necessary for BUNDLES_to_SUPPLIERS_ADDS creation)
            DataSet ds = new DataSet();
            ds = DAL_SQL.ExecuteDataset(DAL_SQL.ConnStr, CommandType.StoredProcedure, "GOV_GetPricesForSupplierAdd",
                SqlDalParam.formatParam("@supplierId", SqlDbType.Int, iSupplierId),
                SqlDalParam.formatParam("@addId", SqlDbType.Int, iAddId),
                SqlDalParam.formatParam("@fromDate", SqlDbType.Date, iFromDate),
                SqlDalParam.formatParam("@toDate", SqlDbType.Date, iToDate)
            );

            if (ds != null && ds.Tables[0] != null && ds.Tables[0].Rows[0] != null && ds.Tables[0].Rows[0] != null)
            {
                Logger.Log("ds.Tables[0].Rows[0]['PriceNetto'].ToString() ====    " + ds.Tables[0].Rows[0]["PriceNetto"].ToString());

                priceNetto = double.Parse(ds.Tables[0].Rows[0]["PriceNetto"].ToString());
            }
        }
        catch (Exception ex)
        {
            if (ex is FormatException || ex is OverflowException || ex is ArgumentNullException)
            {
                Logger.Log("Failed to parse price brutto to int. exception Thrown From ** DAL_SQL_Helper - getTotalPriceAmountBrutoOfAttraction: " + Environment.NewLine + ex.Message);
            }
            else
            {
                Logger.Log("Failed. exception Thrown From ** DAL_SQL_Helper - getTotalPriceAmountBrutoOfAttraction" + Environment.NewLine + ex.Message);
            }
        }

        return priceNetto;
    }

	
    /// <summary>
    /// Get traveller id from dbo.Travellers
    /// </summary>
    /// <param name="iIdNumber"></param>
    /// <returns>int id</returns>
    private static string getTravellerId(string iIdNumber)
    {
        string travellerIdStr = DAL_SQL.GetRecord("TRAVELLERS", "top 1 id", iIdNumber + " order by id desc", "id_no");

        return travellerIdStr;
    }

    public static double getVat(string vat_date)
    {
        string queryGetMaxId;
        string rs;
        string sql;
        string MaxUpdID;
        string VAT_IN_ID = "1";
        double retVal = 0.00;
		
        queryGetMaxId = " SELECT MAX(V_Upd.id) AS max_id " +
              " FROM   VAT V INNER JOIN " +
              "        VAT_VALUES_UPDATES V_Upd ON V.id = V_Upd.vat_id " +
              " WHERE  vat_id = '" + VAT_IN_ID + "' AND (CAST(V_Upd.update_date AS smalldatetime) <= CAST(GETDATE() AS smalldatetime))";
			  
		Logger.Log("queryGetMaxId ============= " + queryGetMaxId);
        MaxUpdID = DAL_SQL.RunSql(queryGetMaxId);


        sql = " SELECT  V_Upd.value " +
              " FROM    VAT V INNER JOIN " +
              "			VAT_VALUES_UPDATES V_Upd ON V.id = V_Upd.vat_id	" +
              " WHERE	V_Upd.id = '" + MaxUpdID + "' ";
		Logger.Log("sql ============= " + sql);
        retVal = double.Parse(DAL_SQL.RunSql(sql));

        return retVal;
    }
	
	
    public static void CreateVoucherForAttraction(int maxVoucherId, string mDocket, int ServiceType, int iBundleId,
                                                  string mSupplierId, string clerkId, string VoucherCDate,
                                                  string ValueDate, string CurrencyId)
    {
        string sql = string.Empty;

        sql = @" INSERT INTO VOUCHERS ( " +
                    "	id,					docket_id,				service_type_id,		" +
                    "	bundle_id,			supplier_id,			issue_clerk_id,			" +
                    "	issue_date,			value_date,				currency_id)			" +
                    " VALUES " +
                    "	(" + maxVoucherId + ",	" + mDocket + ",			" + ServiceType + " ,		" +
                    "	" + iBundleId + ",		" + mSupplierId + ",	" + clerkId + "," +
                    "	'" + VoucherCDate + "','" + ValueDate + "',		" + CurrencyId + " );";
					
							Logger.Log(sql);

        DAL_SQL.RunSql(sql);
    }
	
	/**
     * chen 22/12
     * Inserting into GOV_Approval_Order the number the 7 plus 7 other order.
     * */
    public static void InsertConnectSevenPlusSevenOrder(int iOrderId1, int iOrderId2)
    {
        DataSet ds = DAL_SQL.ExecuteDataset(DAL_SQL.ConnStr, CommandType.StoredProcedure, "[GOV_InsertConnectSevenPlusSevenOrder]",
            SqlDalParam.formatParam("@order_id_to_find", SqlDbType.Int, iOrderId1),
            SqlDalParam.formatParam("@order_id_to_insert", SqlDbType.Int, iOrderId2)
            );
    }
	
	//7+7 code
    public static string getConnectedSevenPlusSevenOrder(int iOrderID)
    {
        string sevenPlusSevenOrderId = DAL_SQL.GetRecord("GOV_ORDER_APPROVAL", "connect_to_order_7_plus_7", "order_id", iOrderID.ToString());

        return sevenPlusSevenOrderId;
    }

    public static void setGovConnetedVoucherInBundle(string iVoucherId_1, string iVoucherId_2)
    {
        string bundleId_1 = DAL_SQL.GetRecord("VOUCHERS", "bundle_id", iVoucherId_1, "id");
        string bundleId_2 = DAL_SQL.GetRecord("VOUCHERS", "bundle_id", iVoucherId_2, "id");

        DAL_SQL.RunSql("UPDATE BUNDLES SET gov_connected_voucher_number = " + iVoucherId_2 + "WHERE id = " + bundleId_1);
        DAL_SQL.RunSql("UPDATE BUNDLES SET gov_connected_voucher_number = " + iVoucherId_1 + "WHERE id = " + bundleId_2);
    }	
	
	public static DataSet getVatFromBundle(string bundleId)
    {
        DataSet ds = null;

        ds = DAL_SQL.ExecuteDataset(DAL_SQL.ConnStr, CommandType.StoredProcedure, "GetVatByBundleId",
          SqlDalParam.formatParam("@bundleId", SqlDbType.Int, bundleId));

        return ds;
    }
	
	public static DataSet GetBaselessOrders(DateTime fromDate, DateTime toDate)
    {
        DataSet ds = new DataSet();
        ds = DAL_SQL.ExecuteDataset(DAL_SQL.ConnStr, CommandType.StoredProcedure, "GOV_GetBaselessOrders",
            SqlDalParam.formatParam("@fromDate", SqlDbType.DateTime, 20, fromDate),
            SqlDalParam.formatParam("@toDate", SqlDbType.DateTime, 20, toDate)
            );
        return ds;
    }
	
	public static DataSet GetRoomlessOrders(DateTime fromDate, DateTime toDate)
    {
        DataSet ds = new DataSet();
        ds = DAL_SQL.ExecuteDataset(DAL_SQL.ConnStr, CommandType.StoredProcedure, "GOV_GetRoomlessOrders",
            SqlDalParam.formatParam("@fromDate", SqlDbType.DateTime, 20, fromDate),
            SqlDalParam.formatParam("@toDate", SqlDbType.DateTime, 20, toDate)
            );
        return ds;
    }
	
	public static DataSet GetVoucherlessOrders(DateTime fromDate, DateTime toDate)
    {
        DataSet ds = new DataSet();
        ds = DAL_SQL.ExecuteDataset(DAL_SQL.ConnStr, CommandType.StoredProcedure, "GOV_GetVoucherlessOrders",
            SqlDalParam.formatParam("@fromDate", SqlDbType.DateTime, 20, fromDate),
            SqlDalParam.formatParam("@toDate", SqlDbType.DateTime, 20, toDate)
            );
        return ds;
    }
	
	public static DataSet GetStatusDifferencesOrders(DateTime fromDate, DateTime toDate)
    {
        DataSet ds = new DataSet();
        ds = DAL_SQL.ExecuteDataset(DAL_SQL.ConnStr, CommandType.StoredProcedure, "GOV_GetDifferentStatusesOrders",
            SqlDalParam.formatParam("@fromDate", SqlDbType.DateTime, 20, fromDate),
            SqlDalParam.formatParam("@toDate", SqlDbType.DateTime, 20, toDate)
            );
        return ds;
    }
	
	public static DataSet GetIndicationlessVouchers(DateTime fromDate, DateTime toDate)
    {
        DataSet ds = new DataSet();
        ds = DAL_SQL.ExecuteDataset(DAL_SQL.ConnStr, CommandType.StoredProcedure, "GOV_GetIndicationless",
            SqlDalParam.formatParam("@fromDate", SqlDbType.DateTime, 20, fromDate),
            SqlDalParam.formatParam("@toDate", SqlDbType.DateTime, 20, toDate)
            );
        return ds;
    }	
	
	public static DataSet GetAttractionLowerThenFiveDays(DateTime fromDate, DateTime toDate)
    {
        DataSet ds = new DataSet();
        ds = DAL_SQL.ExecuteDataset(DAL_SQL.ConnStr, CommandType.StoredProcedure, "GOV_GetOthersLessThenFiveDays",
            SqlDalParam.formatParam("@fromDate", SqlDbType.DateTime, 20, fromDate),
            SqlDalParam.formatParam("@toDate", SqlDbType.DateTime, 20, toDate)
            );
        return ds;
    }	
	
	public static DataSet GetOthersFourPlusOneLessThanFourDays(DateTime fromDate, DateTime toDate)
    {
        DataSet ds = new DataSet();
        ds = DAL_SQL.ExecuteDataset(DAL_SQL.ConnStr, CommandType.StoredProcedure, "GOV_GetOthersFourPlusOneLessThanFourDays",
            SqlDalParam.formatParam("@fromDate", SqlDbType.DateTime, 20, fromDate),
            SqlDalParam.formatParam("@toDate", SqlDbType.DateTime, 20, toDate)
            );
        return ds;
    }	
	
	public static DataSet GetVoucherWithMarkUpOrTaxOrTravPay(DateTime fromDate, DateTime toDate)
    {
        DataSet ds = new DataSet();
        ds = DAL_SQL.ExecuteDataset(DAL_SQL.ConnStr, CommandType.StoredProcedure, "GOV_GetVoucherWithMarkUpOrTaxOrTravPay",
            SqlDalParam.formatParam("@fromDate", SqlDbType.DateTime, 20, fromDate),
            SqlDalParam.formatParam("@toDate", SqlDbType.DateTime, 20, toDate)
            );
        return ds;
    }
	
	public static DataSet GetTravPayBundlesHotels(DateTime fromDate, DateTime toDate)
    {
        DataSet ds = new DataSet();
        ds = DAL_SQL.ExecuteDataset(DAL_SQL.ConnStr, CommandType.StoredProcedure, "GOV_GetTravPayBundlesHotels",
            SqlDalParam.formatParam("@fromDate", SqlDbType.DateTime, 20, fromDate),
            SqlDalParam.formatParam("@toDate", SqlDbType.DateTime, 20, toDate)
            );
        return ds;
    }
	
	public static DataSet GetTravPayBundlesAttraction(DateTime fromDate, DateTime toDate)
    {
        DataSet ds = new DataSet();
        ds = DAL_SQL.ExecuteDataset(DAL_SQL.ConnStr, CommandType.StoredProcedure, "GOV_GetTravPayBundlesAttraction",
            SqlDalParam.formatParam("@fromDate", SqlDbType.DateTime, 20, fromDate),
            SqlDalParam.formatParam("@toDate", SqlDbType.DateTime, 20, toDate)
            );
        return ds;
    }

    public static DataSet GetAllDatesHaveBase(string iSupplierId, DateTime iFromDate, DateTime iToDate)
    {
        DataSet ds = null;
        string query = string.Empty;
        string hotelPriceID = Utils.getHotelPriceId(iSupplierId);

        try
        {
            query = @"
                    SELECT HOB.id, HOB.name, HOB.description
                    FROM P_HOTEL_BASES_TO_DATES PHBTD
                    JOIN  Agency_Admin.dbo.HOTEL_ON_BASE HOB ON PHBTD.base_id = HOB.id
                    WHERE PHBTD.hotel_price_id = @hotel_price_id  AND (PHBTD.date BETWEEN @from_date AND @to_date) 
                    ";
            ds = DAL_SQL.ExecuteDataset(DAL_SQL.ConnStr, CommandType.Text, query,
                SqlDalParam.formatParam("@hotel_price_id", SqlDbType.Int, hotelPriceID),
                SqlDalParam.formatParam("@from_date", SqlDbType.DateTime, iFromDate),
                SqlDalParam.formatParam("@to_date", SqlDbType.DateTime, iToDate),
                SqlDalParam.formatParam("@status", SqlDbType.Bit, true)
                );

        }
        catch (Exception ex)
        {
            throw new Exception("Failed save monthly allocations. Ex = " + ex.Message);
        }

        return ds;
    }
}
