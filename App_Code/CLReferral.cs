using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for CLReferral
/// </summary>
public class CLReferral
{
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
    public string ReferrTotal { get; set; }
    public string NumEscort { get; set; }
    public string DocetNum { get; set; }
    public string HotelCode { get; set; }
    public string HotelInvNo { get; set; }
    public string HotelInvAmnt { get; set; }
    public string HotelAreaCode { get; set; }
    public string HotelRamaCode { get; set; }
    public string HotelMelave1 { get; set; }
    public string HotelMelave2 { get; set; }
    public string HotelPrcntHoliday { get; set; }
    public string NumEscortPatient { get; set; }
    public string AmountPayPatient { get; set; }
    public string SuppDiscount { get; set; }
    public string SuppDiscountAmt { get; set; }
    public string HotelMelave1Tmp { get; set; }
    public string HotelMelave2Tmp { get; set; }
    public CLReferral()
    {
    }
}