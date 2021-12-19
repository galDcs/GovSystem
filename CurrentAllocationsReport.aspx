<%@ Page Language="C#" MasterPageFile="~/MainMasterPage.master" AutoEventWireup="true" CodeFile="CurrentAllocationsReport.aspx.cs" Inherits="CurrentAllocationsReport" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
    
    <script type="text/jscript">
         
        $(document).ready(function () {
            var txtFromDateID = '<%=txtFromDate.ClientID%>';
			var txtToDateID = '<%=txtToDate.ClientID%>';
			
            $('#<%=txtFromDate.ClientID%>').datepicker({
                firstDay: 0,
				onSelect: function () {
				
					var nights = 7;
					var fromDate = ConvertToDate($("#" + txtFromDateID).val());
					var newToDate = new Date(fromDate.setDate(fromDate.getDate() + nights));
					if($("#" + txtToDateID).val().length === 0){
						$("#" + txtToDateID).val($.datepicker.formatDate("dd/mm/yy", newToDate));
					}
				},
				
				onClose: function (selectedDate) {
					$('#<%=txtToDate.ClientID%>').datepicker("option", "minDate", selectedDate);
					}
            });
			
			function ConvertToDate(dateStr)//converts from dd/mm/yy format
			{
				var arr = dateStr.split("/");
				return new Date(arr[2], arr[1] - 1, arr[0]);
			}
            
            $('#<%=txtToDate.ClientID%>').datepicker({
                firstDay: 0
            });
            
            checkMailRes();
            checkFaxRes();
            sendMail(); 
			
			//show first logo
			$('.hide_element').first().show();
            
        });
		
		//show 'please wait'
		function showProgress() {
			var updateProgress = $get("<%= updateProgress.ClientID %>");
			updateProgress.style.display = "block";
		}
        
        
        function sendMail()
        {
            $('#btnEmail').click( function(e) { 
            e.preventDefault();
            
            var address = prompt("Please, enter email adress","");
            if(address != null )
            {
                if(! validateAddress(address))
                {
                    alert("Error: e-mail is invalid.");
                }
                else
                {
                    $('[id*="hdnFMail"]').val(address);                    
                    $('[id*="btnMail"]').trigger('click');                   
                }
                
             }  
           });
        }
        
         function checkMailRes()
        {
            var res = $('[id*="hdnFRes"]').val();
            
            if( res == "True") { alert("Mail sent");}
            if( res == "False") {alert("Error: sending mail failed");}
            
            $('[id*="hdnFRes"]').val('');
        }
        
        function checkFaxRes()
        {
            var res = $('[id*="hdnFFax"]').val();
            
            if( res == "True") { alert("Fax sent");}
            if( res == "False") {alert("Error: sending fax failed");}
            
            $('[id*="hdnFFax"]').val('');
        }
        function validateAddress(address)
        {
                   
            var filter = /^([\w-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([\w-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$/;
            
            if( filter.test(address)) {return true;} else {return false;}
        }

        
        function PrintElem(divID)
        {
			$('.hide_element').show();
            var elem = document.getElementById(divID);
            var tData = elem.innerHTML;
            var mywindow = window.open("Print", "", "fullscreen");
            mywindow.document.write("<html><head><title>printwindow</title>");
           // mywindow.document.write('<link href="css/style-page.css" rel="stylesheet" type="text/css" />');
            mywindow.document.write("</head><body>");
            mywindow.document.write(tData);
            mywindow.document.write("</body></html>");
            mywindow.document.close();
            mywindow.print();
            mywindow.close();
			$('.hide_element').hide();
            return true;
        }
    </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">
 <div class="box-col" style="width: 98%; float:inherit;" dir="rtl">
    <h2 dir="rtl">דו"ח מימוש לינות</h2>
    <center>
       
        <table dir="rtl" class="trans" > 
            <tr>
                <th align="right">מתאריך</th>
                <td align="right"><asp:TextBox runat="server" name="txtFromDate" id="txtFromDate" /> </td>
				
                <th align="right">עד תאריך</th>
                <td align="right"><asp:TextBox runat="server" name="txtToDate" id="txtToDate" /> </td> 
            </tr>

            <tr> 
                <th align="right">מלון</th>
                <td align="right" colspan="3">
                    <asp:DropDownList runat="server" ID="Hotels" DataValueField="id" DataTextField="NameAndArea">
                    </asp:DropDownList>
                </td> 
            </tr>
  
        </table>
		     <br/>
		<asp:Label runat="server" id="lbMessage" style="font-size:25px; color:red;"/>
        <br/>
		   <br/>
		<div id="buttons-options">
			<asp:Button runat="server" ID="ShowReport" OnClick="ShowReport_OnClick" Text="להפיק דוח"  cssClass="button-submit button-inline" OnClientClick="showProgress()"/>
			<input type="button" ID="PrintReport" onclick="PrintElem('tbDiv')" value="להדפיס דוח" class="button-submit button-inline"/>
			<asp:Button ID="btnToXls" runat="server" Text="לייצא לאקסל" OnClick="btnToXls_Click" cssClass="button-submit button-inline"/>
			<asp:Button ID="btnMail" runat="server" Text="לשלוח מייל" 
				onclick="btnMail_Click"  Visible="True" style="display:none;" cssClass="button-submit button-inline"/>
			 <input type="button" id = "btnEmail" value="לשלוח מייל" class="button-submit button-inline"/>
			<asp:Button ID="btnFax" runat="server" Text="לשלוח פקס" onclick="btnFax_Click"  Visible="True" cssClass="button-submit button-inline"/>                         
		</div>
		<br/>
        <div id="tbDiv">
			<%=resultTable.ToString() %>
        </div>
        <div>
            <asp:HiddenField ID="hdnFMail" runat="server" />        
        </div>
        <div>
           <asp:HiddenField ID="hdnFRes" runat="server" />    
        </div>
        <div>
           <asp:HiddenField ID="hdnFFax" runat="server" />    
        </div>
    </center>
	 </div>
	 
	   <asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>
	  
	<asp:UpdateProgress id="updateProgress" runat="server">
    <ProgressTemplate>
        <div style="position: fixed; text-align: center; height: 100%; width: 100%; top: 0; right: 0; left: 0; z-index: 9999999; background-color: #000000; opacity: 0.7;">
            <span style="border-width: 0px; position: fixed; padding: 50px; background-color: #FFFFFF; font-size: 36px; left: 37%; top: 40%;">
			<img src="css/images/gears.gif" />
			<br/>
			Please Wait</span>
        </div>
    </ProgressTemplate>
</asp:UpdateProgress>
</asp:Content>