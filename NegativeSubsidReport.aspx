<%@ Page Language="C#" MasterPageFile="~/MainMasterPage.master" AutoEventWireup="true" CodeFile="NegativeSubsidReport.aspx.cs" Inherits="NegativeSubsidReport" Title="Untitled Page" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">

<script type="text/jscript">
  $(document).ready(function () {
            
            $('#<%=txtFromDate.ClientID%>').datepicker({
                firstDay: 0
            });
            
            $('#<%=txtToDate.ClientID%>').datepicker({
                firstDay: 0
            });
        });

		function printReport(){
			$('.MainTable').hide();
			window.print();
			$('.MainTable').show();
		};
		
			//<![CDATA[
		function showLoading() {
			$('.pleaseWait').show();
		}
		//]]>
    </script> 
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">
<div class="box-col" style="width:95%;">
     <h2 dir="rtl">דו"ח הזמנות שגויות</h2>
    <center>
       
        <table dir="rtl" class="MainTable"> 
            <tr>
                <th align="right">מתאריך</th>
                <td align="right"><asp:TextBox runat="server" name="txtFromDate" id="txtFromDate" autocomplete="off" /> </td>
                <th align="right">עד תאריך</th>
                <td align="right"><asp:TextBox runat="server" name="txtToDate" id="txtToDate" autocomplete="off"/> </td> 
            </tr>
            <tr>
                <td colspan="2" align="center">
                    <asp:Button runat="server" ID="ShowReport"  Text="להפיק דוח" 
                        onclick="ShowReport_Click" class="button" style="width:100%;" OnClientClick="showLoading()"/>                                    
                </td> 
                <td colspan="2" align="center">
                    <label id="print-page" onclick="printReport();" class="button" style="width:100%;">הדפס</button>                                    
                </td> 
				
            </tr>
            
        </table>
        <br/>
        <div id="tbDiv">
        <%=resultTable.ToString() %>
        </div>
       
    </center>

	</div>
	</div> 
        <div style="position: fixed; text-align: center; height: 100%; width: 100%; top: 0; right: 0; left: 0; z-index: 9999999; background-color: #000000; opacity: 0.7; display:none;" class="pleaseWait">
            <span style="border-width: 0px; position: fixed; padding: 50px; background-color: #FFFFFF; font-size: 36px; left: 37%; top: 40%;">
				<img src="css/images/gears.gif" />
				<br/>
				Please Wait
			</span>
        </div>
<script src="Scripts/sorttable.js"></script>
	</asp:Content>

