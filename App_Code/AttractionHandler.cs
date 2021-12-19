using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Script.Services;
using System.Web.Services;

/// <summary>
/// Summary description for AjaxService
/// </summary>
[WebService(Namespace = "http://tempuri.org/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
// To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
[System.Web.Script.Services.ScriptService]
public class AttractionHandler : System.Web.Services.WebService {

    public AttractionHandler () {

        //Uncomment the following line if using designed components 
        //InitializeComponent(); 
    }


    [WebMethod(EnableSession = true)]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public List<Attraction> GetAttractionList(int iSupplierID)
    {
		iSupplierID = getMarpeSupplierId(iSupplierID.ToString());
        string query ="SELECT  S_T_A.supplier_id, S_T_A.add_id,  S_A.name " 
				  +" FROM    SUPPLIERS_TO_OTHER_ADDS S_T_A INNER JOIN " 
                  +"			SUPPLIERS_OTHER_ADDS S_A ON S_T_A.add_id = S_A.id	" 
                  +" WHERE	S_T_A.status = 1 AND S_T_A.supplier_id = '"+ iSupplierID  +"'"
                  +" ORDER by add_id";

        DataSet ds = DAL_SQL.RunSqlDataSet(query);

        List<Attraction> attractions = new List<Attraction>();
        Attraction attraction;

        //Only if got row from DB.
        if (ds != null && ds.Tables != null && ds.Tables[0].Rows != null)
        {
            foreach (DataRow row in ds.Tables[0].Rows)
            {
                attraction = new Attraction();

                attraction.SupplierID = (int)row["supplier_id"];
                attraction.AddID = (int)row["add_id"];
                attraction.Name = row["name"].ToString();

                attractions.Add(attraction);
         
            }
        }

        return attractions;
    }

    

    [WebMethod]
    public string HelloWorld() {
        return "Hello World";
    }
	
	
	private int getMarpeSupplierId(string iSupplierId)
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

            //Tiberias
            case eHotels.GlatLavy:
            case eHotels.TiberiasLeonardo:
            case eHotels.TiberiasLeonardoClub:
                hotel = eHotels.TiberiasHameiCaesar;
                break;

            case eHotels.TiberiasRimonimMineral:
            case eHotels.TiberiasSpaVillage:
			case eHotels.TiberiasLakeHouse:
                hotel = eHotels.TiberiasHameiTiberias;
                break;

            //Netanye
            case eHotels.NenatyaRamada:
            case eHotels.NenatyaShfaim:
                hotel = eHotels.NatanyaHameiGaash;
                break;

            default:
                //hotel = eHotels.None;
				hotel = eHotels.TiberiasHameiTiberias;
                break;
        }
        int id = (int)hotel;
        return id;
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
        //Tiberias
        TiberiasLeonardo = 2323,
        TiberiasLeonardoClub = 756,
        TiberiasSpaVillage = 2487,
        TiberiasRimonimMineral = 131,
		TiberiasLakeHouse = 18977,
        TiberiasHameiCaesar = 146,
		TiberiasHameiTiberias = 18978,
		 // Changed to new supplierId 18978
        //TiberiasHameiTiberias = 367,
        //Glat
        GlatLavy = 306,
        //Ashdod
        AshdodWest = 12206,
        AshdodHameiYoav = 1712,
        //Netanya
        NenatyaRamada = 10164,
        NenatyaShfaim = 244,
        NatanyaHameiGaash = 526
    }
}
