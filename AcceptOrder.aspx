<%@ Page Title="" Language="C#" MasterPageFile="~/MainMasterPage.master" AutoEventWireup="true" CodeFile="AcceptOrder.aspx.cs" Inherits="AcceptOrder" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="Server">
  <style>
    table {
      width: 100%;
      border-collapse: collapse;
      border-spacing: 0;
      margin-bottom: 20px;
    }

      table tr:nth-child(2n) td {
        background: #F5F5F5;
      }

      table th,
      table td {
        text-align: center;
      }

      table th {
        padding: 5px 20px;
        color: #5D6975;
        border-bottom: 1px solid #C1CED9;
        white-space: nowrap;
        font-weight: normal;
      }

      table .service,
      table .desc {
        text-align: left;
      }

      table td {
        padding: 20px;
      }

        table td.service,
        table td.desc {
          vertical-align: top;
        }

        table td.unit,
        table td.qty,
        table td.total {
          font-size: 1.2em;
        }

        table td.grand {
          border-top: 1px solid #5D6975;
        }
  </style>

  <script type="text/javascript">
    function printDiv(divName) {
      var printContents = document.getElementById(divName).innerHTML;
      var originalContents = document.body.innerHTML;

      document.body.innerHTML = printContents;

      window.print();

      document.body.innerHTML = originalContents;
    }
    </script>
 <style type="text/css">
        .backToHome
        {
        	color: Blue;
        	text-decoration: underline;
        	cursor:pointer;
        }
    </style>
    
    <script type="text/javascript">
        $(document).ready(function(){
            
            $(".backToHome").bind('click', function(){
                location.href = "Default.aspx";
                return;
            });

            $(".docket_id").bind('click', function(){
                if(self.parent) {
                    self.parent.location.href="<%=ConfigurationManager.AppSettings.Get("AgencyDocketLink")%><%=Request["DocketId"]%>&PageFunc=common";
                } else {
                    document.location.href="<%=ConfigurationManager.AppSettings.Get("AgencyDocketLink")%><%=Request["DocketId"]%>&PageFunc=common";
                }
            });

            
        });
    </script>
    
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">
 <div class="box-col" style="width:95%;">
 <asp:PlaceHolder ID="PlaceHolder1" runat="server"></asp:PlaceHolder>
<table width="50%" dir="rtl" class="trans">
  <tr>
    <td colspan="2">
      <h1 style="color:green">הזמנה בוצעה בהצלחה</h1>
    </td>
  </tr>
  <tr>
    <th>1מס' תיק</th>
    <td>
	  <asp:Label runat="server" ID="lblDocketId" class="docket_id_lbl" Font-Bold="true" />
	</td>
  </tr>
  <tr>
    <th>1מס' שובר</th>
    <td>
	  <asp:Label runat="server" ID="lblVoucherId" Font-Bold="true" />
    </td>
  </tr>
  <tr>
    <th>2מס' תיק</th>
    <td>
	  <asp:Label runat="server" ID="lblDocketId2" class="docket_id_lbl" Font-Bold="true" />
	</td>
  </tr>
  <tr>
    <th>2מס' שובר</th>
    <td>
	  <asp:Label runat="server" ID="lblVoucherId2" Font-Bold="true" />
    </td>
  </tr>
</table>
<br/>
<br/>
<span class="backToHome">חזרה</span>
    

 

 
</div>

</asp:Content>

