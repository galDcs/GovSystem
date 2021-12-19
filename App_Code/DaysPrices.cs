using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for DaysPrices
/// </summary>
public class DaysPrices
{
    public PriceByDay Neto { get; set; }
    public PriceByDay Bruto { get; set; }
    
	public DaysPrices()
	{
        
	}

    public class PriceByDay
    {
        public double Sunday { get; set; }
        public double Monday { get; set; }
        public double Tuesday { get; set; }
        public double Wednesday { get; set; }
        public double Thursday { get; set; }
        public double Friday { get; set; }
        public double Saturday { get; set; }

        public PriceByDay() { }

        public double GetPriceByDayOfWeek(DayOfWeek iDayOfWeek)
        {
            double priceForDay = 0.0;
            switch (iDayOfWeek)
            {
                case DayOfWeek.Sunday:
                    priceForDay = this.Sunday;
                    break;
                case DayOfWeek.Monday:
                    priceForDay = this.Monday;
                    break;
                case DayOfWeek.Tuesday:
                    priceForDay = this.Tuesday;
                    break;
                case DayOfWeek.Wednesday:
                    priceForDay = this.Wednesday;
                    break;
                case DayOfWeek.Thursday:
                    priceForDay = this.Thursday;
                    break;
                case DayOfWeek.Friday:
                    priceForDay = this.Friday;
                    break;
                case DayOfWeek.Saturday:
                    priceForDay = this.Saturday;
                    break;

                default:
                    priceForDay = -1;
                    break;
            }

            return priceForDay;
        }
		
		public double getBrutoWithoutVat(DayOfWeek iDayOfWeek, string iVatPercent) // example: iVatPercent = 17 
		{
			return GetPriceByDayOfWeek(iDayOfWeek) / (1 + (double.Parse(iVatPercent) / 100));
		}
    }
}