using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

public class PriceDetails
{
    public string PriceId { get; set; }
    public string SupplierId { get; set; }
    public string FromDate { get; set; }
    public string ToDate { get; set; }

    public int FromDay { get; set; }
    public int ToDay { get; set; }
    public int Nights { get; set; }

    public double Vat{ get; set; }

    public decimal AmountNetto { get; set; }
    public decimal AmountBrutto { get; set; }
    public decimal AmountBruttoCouple { get; set; }
	
    public PriceDetails()
	{
		
	}

    public PriceDetails(string PriceId, string supplierId, string fromDate, string toDate, int fromDay, int toDay, int nights, double vat)
    {
        this.PriceId = PriceId;
        this.SupplierId = supplierId;
        this.FromDate = fromDate;
        this.ToDate = toDate;
        this.FromDay = fromDay;
        this.ToDay = toDay;
        this.Nights = nights;
        this.Vat = vat;
    }
}