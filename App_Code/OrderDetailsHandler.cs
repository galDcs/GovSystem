using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for OrderDetails
/// </summary>
public class OrderDetailsHandler
{
    //Order docket and voucher (after making the order in agency will update).
    private string mDocketId = string.Empty;
    private string mVoucherId = string.Empty;

    //Order Details.
    public DateTime mFromDate { get; set; }
    public DateTime mToDate { get; set; }
    public int mNumberOfNights { get; set; }
    public int mNumOfMelavim { get; set; }
    public string mHotelName { get; set; }
    public string mMarpe { get; set; }
    public string mBase { get; set; }
    public int mBaseid { get; set; }
    public string mSupplierid { get; set; }
    public string mAllocationid { get; set; }
    public string mMarpename { get; set; }
    public string mAddid { get; set; }

    public OrderDetailsHandler(DateTime mFromDate, DateTime mToDate, int mNumberOfNights,
                        int mNumOfMelavim, string mHotelName, string marpe, string mBase, int mBaseid,
                        string mSupplierid, string mAllocationid, string mMarpename, string mAddid)
    {
        // TODO: Complete member initialization
        //this.mDocketId = mDocketId;
        //this.mVoucherId = mVoucherId;
        this.mFromDate = mFromDate;
        this.mToDate = mToDate;
        this.mNumberOfNights = mNumberOfNights;
        this.mNumOfMelavim = mNumOfMelavim;
        this.mHotelName = mHotelName;
        this.mMarpe = marpe;
        this.mBase = mBase;
        this.mBaseid = mBaseid;
        this.mSupplierid = mSupplierid;
        this.mAllocationid = mAllocationid;
        this.mMarpename = mMarpename;
        this.mAddid = mAddid;
    }


    public string getDocketId()
    {
        return mDocketId;
    }

    public string getVoucherId()
    {
        return mVoucherId;
    }

    public void setDocketId(string iDocketId)
    {
        mDocketId = iDocketId;
    }

    public void setVoucherId(string iVoucherId)
    {
        mVoucherId = iVoucherId;
    }
}