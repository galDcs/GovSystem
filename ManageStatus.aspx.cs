using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;

public partial class ManageStatus : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        Panelempty.Visible = false;
        if (!IsPostBack)
        {
            //ds = DAL_SQL.ExecuteDataset(DAL_SQL.ConnStr, CommandType.Text, "select id,Name,Status from Gov_indicators ");
            loadData();
        }
        LError.Text = "";
       //GridView1.DataBind();
    }
    
    private void loadData()
    {
        DataSet ds = new DataSet();
        DataTable dt = new DataTable();
        //פעיל
        ds = DAL_SQL_Helper.GetStatusIndicators();
        if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
        {
            Session["rptData"] = ds;
            GridView1.DataSource = ds.Tables[0];
            GridView1.DataBind();
            GridView1.Style[HtmlTextWriterStyle.Direction] = "rtl";
        }
        else
        {
            Panelempty.Visible = true;
        }
    }

    protected void OnCheckedChanged_Status(object sender, EventArgs e)
    {
        CheckBox cb = (CheckBox)sender;
        string id = cb.Attributes["data"];
        //update status
        DAL_SQL_Helper.GOV_UpsertStatus(true, Convert.ToInt32(id), "", cb.Checked);
    }

    protected void Addrow(object sender, EventArgs e)
    {
        Control control = null;
        if (GridView1.FooterRow != null)
        {
            control = GridView1.FooterRow;
        }
        else
        {
            control = GridView1.Controls[0].Controls[0];
        }
        string id = (control.FindControl("tBoxID") as TextBox).Text;
        string name = (control.FindControl("tBoxName") as TextBox).Text;
        bool status = (control.FindControl("chIsActive") as ICheckBoxControl).Checked;
        int intId;
        Int32.TryParse(id,out intId);
      
        if (intId == 0 && id != "0") //id wrong
        {
            LError.Text = "Error in insert data ";
        }
        else
        {
            object[] items = new object[3];

            items[0] = intId.ToString();
            items[1] = name;
            items[2] = status.ToString();
            //insert status
            DAL_SQL_Helper.GOV_UpsertStatus(false, intId, name, status);
            DataSet ds = (DataSet)Session["rptData"];
            ds.Tables[0].Rows.Add(items);

            GridView1.DataSource = ds.Tables[0];
            GridView1.DataBind();
            Session["rptData"] = ds;
        }
    }

    protected void Addfirstrow(object sender, EventArgs e)
    {
        string id = tBoxID.Text;
        string name = tBoxName.Text;
        bool status = chIsActive.Checked;
        int intId;
        Int32.TryParse(id, out intId);

        if (intId == 0 && id != "0") //id wrong
        {
            LError.Text = "Error in insert data ";
        }
        else
        {
            //insert status
            DAL_SQL_Helper.GOV_UpsertStatus(false, intId, name, status);
            DataSet ds = new DataSet();
            //ds = DAL_SQL.ExecuteDataset(DAL_SQL.ConnStr, CommandType.Text, "select id,Name,Status from Gov_indicators ");
            ds = DAL_SQL_Helper.GetStatusIndicators();
            
            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                Session["rptData"] = ds;
                GridView1.DataSource = ds.Tables[0];
                GridView1.DataBind();
                //GridView1.Style[HtmlTextWriterStyle.Direction] = "rtl";
            }
            else
            {
                Panelempty.Visible = true;
            }
        }
    }
}