using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;

/// <summary>
/// Summary description for BasTypes
/// </summary>
public class BaseTypes
{
    public int Id {get; set; }
    public string Name { get; set; }
    public string Description { get; set; }

	public BaseTypes()
	{
	}
}
