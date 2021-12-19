<%@ Page Language="C#" MasterPageFile="~/MainMasterPage.master" AutoEventWireup="true" CodeFile="IPSOrdersByDateReport.aspx.cs" Inherits="IPSOrdersByDateReport" %>

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
            
          
        });
		
		
		
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
            var i;

            for (i = 1;i <= <%= index %> ; i++)
            {
                document.getElementById("headerForPrint_" + i).style.display = "";
            }
		  var mywindow = window.open('', 'PRINT', 'fullscreen');
		
			mywindow.document.write('<html><head><title>דו"ח הזמנות נופש</title>');
			mywindow.document.write('</head><body >');
			mywindow.document.write('<h1>דו"ח הזמנות נופש</h1>');
			mywindow.document.write(document.getElementById(divID).innerHTML);
				
			mywindow.document.write('</body></html>');
		
			mywindow.document.close(); // necessary for IE >= 10
			mywindow.focus(); // necessary for IE >= 10*/
		
			mywindow.print();
			mywindow.close();

			for (i = 1;i <= <%= index %> ; i++)
			{
			    document.getElementById("headerForPrint_" + i).style.display = "none";
			}
        return true;
		/*
            var elem = document.getElementById(divID);
            var tData = elem.innerHTML;
            var mywindow = window.open("Print", "", "'height=600,width=800'");
            mywindow.document.write("<html><head><title>printwindow</title>");
            mywindow.document.write('<link href="App_Themes/Theme1/StyleSheet.css" rel="stylesheet" type="text/css" />');
            mywindow.document.write("</head><body>");
            mywindow.document.write(tData);
            mywindow.document.write("</body></html>");
            mywindow.document.close();
            mywindow.print();
            mywindow.close();
            return true;*/
        }
		//<![CDATA[
		function showLoading() {
			$('.pleaseWait').show();
		}
		//]]>

		

    </script> 
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">

 <div class="box-col" style="width: 98%; float:inherit;" dir="rtl" runat="server" >
    <div class="form-view1">
	<h2 dir="rtl">דו"ח הזמנות נופש</h2>
    <center>
			
        <table dir="rtl" class=""> 
            <tr>
                <th align="right">מתאריך</th>
                <td align="right"><asp:TextBox runat="server" name="txtFromDate" id="txtFromDate" autocomplete="off" /> </td>
				
                <th align="right">עד תאריך</th>
                <td align="right"><asp:TextBox runat="server" name="txtToDate" id="txtToDate" autocomplete="off" /> </td> 
            </tr>
            <tr> 
                <th align="right">מלון</th>
                <td align="right">
					<asp:DropDownList runat="server" ID="Hotels" DataValueField="id" DataTextField="NameAndArea">
                    </asp:DropDownList>
                </td> 
				<th align="right">סוג נופש</th>
                <td align="right">
					<asp:DropDownList runat="server" ID="ddlVacationType">
						<asp:ListItem Text="הכל" Value="none"></asp:ListItem>
						<asp:ListItem Text="תקציבי" Value="ת"></asp:ListItem>
						<asp:ListItem Text="מבצעי" Value="מ"></asp:ListItem>
						<asp:ListItem Text="ללא זכאות נופש	" Value="empty"></asp:ListItem>
                    </asp:DropDownList>
                </td> 
            </tr>
			<tr>
				<th align="right" colspan="">פרטי\שב''ס</th>
                <td align="right" colspan="">
                    <asp:DropDownList runat="server" ID="ddlDocketType" >
						<asp:ListItem Text="שב''ס" Value="false"></asp:ListItem>
						<asp:ListItem Text="פרטי" Value="true"></asp:ListItem>
                    </asp:DropDownList>
                </td>
			</tr>
        </table>
        <br/>
		<div id="buttons-options">
			<button runat="server" ID="ShowReport" onclick="goToServer('showReport'); showLoading();" class="ips-btn">
				<i class="far fa-file-alt"></i>
				להפיק דוח
			</button>
			<button ID="PrintReport" onclick="PrintElem('tbDiv');" class="ips-btn">
				<i class="fas fa-print"></i>
				להדפיס דוח
			</button>
			<button ID="btnToXls" runat="server" onclick="goToServer('excel');" class="ips-btn">
				<i class="far fa-file-excel"></i>
				לייצא לאקסל
			</button>
			<!--<button ID="btnMail" runat="server" onserverclick="btnMail_Click"  Visible="True" class="ips-btn">
				<i class="fa fa-envelope"></i>
				לשלוח מייל
			</button>
			<button ID="btnFax" runat="server" onserverclick="btnFax_Click"  Visible="True" class="ips-btn"> 
				<i class="fa fa-fax""></i>
				לשלוח פקס
			</button>-->
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
		</div>
    </center>
	
</div> 
        <div style="position: fixed; text-align: center; height: 100%; width: 100%; top: 0; right: 0; left: 0; z-index: 9999999; background-color: #000000; opacity: 0.7; display:none;" class="pleaseWait">
            <span style="border-width: 0px; position: fixed; padding: 50px; background-color: #FFFFFF; font-size: 36px; left: 37%; top: 40%;">
				<img src="css/images/gears.gif" />
				<br/>
				Please Wait
			</span>
        </div>
		<input type="hidden" id="pageFunc" name="pageFunc" />
		
		<script>
			 function goToServer(func) {
				debugger;
				$('#pageFunc').val(func);
				$("#form1").submit();
			}
		</script>
</asp:Content>
