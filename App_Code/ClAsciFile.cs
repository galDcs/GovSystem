using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for ClAsciFile
/// </summary>
public class ClAsciFile
{
    public string HotelId { get; set; }
    public string BaseID { get; set; }
    public string NightNum { get; set; }
    public string TotalPeople { get; set; }
    public string AccompaniedPay { get; set; }
    public string MsgType { get; set; }
    public string MsgMapCode { get; set; }
    public string MsgDocNum { get; set; }
    public string MsgApkey { get; set; }
    public string MsgSender { get; set; }
    public string MsgReceiver { get; set; }
    public string MsgCreDate { get; set; }
    public string MsgCreTime { get; set; }
    public string DocumentType { get; set; }
    public string InvoiceType { get; set; }
    public string ActionType { get; set; }
    public string CinvoiceRefBy { get; set; }
    public string CinvoiceNum { get; set; }
    public string BunchNum { get; set; }
    public string CinvPeriod { get; set; }
    public string CinvDate { get; set; }
    public string MsgCreationDate { get; set; }
    public string InvoiceDate { get; set; }
    public string LiasionUnit { get; set; }
    public string MODNumber { get; set; }
    public string CinvTotal { get; set; }
    public string VATPercentage { get; set; }
    public string VATTotal { get; set; }
    public string Currency { get; set; }
    public string LineNO { get; set; }
    public string ReferralNum { get; set; }
    public string ReferralVoucher { get; set; }
    public string LineType { get; set; }
    public string IdCard { get; set; }
    public string PatientFileNum { get; set; }
    public string ModCatNum { get; set; }
    public string TreatStartDate { get; set; }
    public string TreatEndDate { get; set; }
    public string Quantity { get; set; }
    public string UOM { get; set; }
    public string UOMPrice { get; set; }
    public string HotelMelave1 { get; set; }
    public string HotelMelave2 { get; set; }
    public string ReferrTotal { get; set; }
    public string CoRemTextLine1 { get; set; }
    public string CoDiscountamnt { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime Todate { get; set; }
    public int LineNum { get; set; }

    public string InnerUse_AgencyHotelId { get; set; }
    public string InnerUse_AgencyBaseId { get; set; }
    public string InnerUse_VoucherId { get; set; }
    public string InnerUse_BundleId { get; set; }
    public string InnerUse_Erkev_Type_Name { get; set; }
    public List<int> InnerUse_Erkev_Type { get; set; }
    public List<CLReferral> Referral
    {
        get;
        set;
    }
    public ClAsciFile()
    {
    }
}