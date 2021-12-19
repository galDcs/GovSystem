<%@ Page Language="C#" MasterPageFile="~/MainMasterPage.master" AutoEventWireup="true" CodeFile="InvoicesFileNew.aspx.cs" Inherits="InvoicesFileNew" Title="קובץ חשבוניות חדש" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
    
    <script type="text/jscript">
        
    
    $(document).ready(function () {
               $('.grid_date_style').datepicker({});
        });
      
	  /*
	  function submitForm(func) {
			showLoading();
			$('#pageFunc').val(func);
			$('#form1').submit();	
	  }
    
     function showLoading() {
			$('.pleaseWait').show();
	 }*/
	 
	 
	</script>
    
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">
<div class="box-col" style="width:95%">
     <h2 dir="rtl">קובץ חשבוניות חדש</h2>
    <center>
       
        <table dir="rtl" class="MainTable">
            <tr>
                <th colspan="4" align="center">תאריכי יציאה של הזמנות</th>
            </tr> 
            <tr id="mustUpperRow">
                <td>מתאריך יציאה</td>
                <td><input type="text" class="grid_date_style" name="txtFromExitDate" id="txtFromExitDate" autocomplete="off"/></td> 
                <td>עד תאריך יציאה</td>
                <td><input type="text" class="grid_date_style" name="txtToExitDate" id="txtToExitDate" autocomplete="off"/></td> 
            </tr>
            
            <tr id="optionalUpperRow" class="optRow" style="display:none;">
                <td>מתאריך סיום</td>
                <td><input type="text" class="grid_date_style" name="txtFromTermDate" id="txtFromTermDate" autocomplete="off"/></td>
                <td>עד תאריך סיום</td>
                <td><input type="text" class="grid_date_style" name="txtToTermDate" id="txtToTermDate" autocomplete="off"/></td>
            </tr>
          
            <tr style="display:none;">
                <th colspan="4" align="center">תאריכי יציאה ועדכון של הזמנות</th>
            </tr>
            <tr style="display:none;">
                <td>מתאריך יציאה</td>
                <td><input type="text" class="grid_date_style" name="txtUpdateFromExitDate" id="txtUpdateFromExitDate" /></td> 
                <td>עד תאריך יציאה</td>
                <td><input type="text" class="grid_date_style" name="txtUpdateToExitDate" id="txtUpdateToExitDate" /> </td> 
            </tr style="display:none;">
            <tr id ="optionalLowerRow" class="optRow">
                <td>מתאריך סיום</td>
                <td><input type="text" class="grid_date_style" name="txtUpdateFromTermDate" id="txtUpdateFromTermDate" /></td>
                <td>עד תאריך סיום</td>
                <td><input type="text" class="grid_date_style" name="txtUpdateToTermDate" id="txtUpdateToTermDate" /></td>
            </tr>
            <tr style="display:none;">
                <td>מתאריך שינוי שובר</td>
                <td><input type="text" class="grid_date_style" name="txtUpdateFromDate" id="txtUpdateFromDate" /> </td> 
                <td>עד תאריך שינוי שובר</td>
                <td><input type="text" class="grid_date_style" name="txtUpdateToDate" id="txtUpdateToDate" /> </td> 
            </tr>
            <tr style="display:none;">
                <td>סוג דוח</td>
                <td><select id="ddlReportType" name="ddlReportType" class="grid_date_style" style="width:75%;text-align:right">
                        <option value="0" selected="selected">רגיל</option>
                        <option value="1" >מורחב</option>
                    </select> 
                </td> 
                <td></td>
                <td></td> 
            </tr>
        </table>
        
		<input type="hidden" name="pageFunc" /> 
        <asp:Button id="btSearch" Text="הורדת קובץ" class="button-submit" OnClick="btSearch_Click" runat="server" OnClientClick="showLoading();" /> 
        
        <div width="500px"> 
            <asp:Table ID="table1" runat="server" ></asp:Table> 
        </div>
         
        <br/> 
        <div width="500px"> 
            <asp:Table ID="table2" runat="server" ></asp:Table> 
        </div> 
        <br/>
        
        <div width="500px"> 
            <asp:Table ID="table3" runat="server" ></asp:Table> 
        </div> 
        <br/>
		<div width="500px"> 
            <h1><a href="./InvoiceForGov.aspx">ליצירת קובץ  בפורמט של משרד הביטחון</a></h1>
        </div> 
    </center>
	<div id="loading" runat="server" style="position: fixed; text-align: center; height: 100%; width: 100%; top: 0; right: 0; left: 0; z-index: 9999999; background-color: #000000; opacity: 0.7; display:none;" class="pleaseWait">
            <span style="border-width: 0px; position: fixed; padding: 50px; background-color: #FFFFFF; font-size: 36px; left: 37%; top: 40%;">
				<img src="css/images/gears.gif" />
				<br/>
				Please Wait
			</span>
        </div>
	</div>
</asp:Content>
