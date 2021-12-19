<%@ Page Title="" Language="C#" MasterPageFile="~/MainMasterPage.master" AutoEventWireup="true" CodeFile="UsageFileNew.aspx.cs" Inherits="UsageFileNew" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
    
    <script type="text/jscript">


        $(document).ready(function () {
            $('.grid_date_style').datepicker({});

        });
      
    
     
    </script>
    
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">
<div class="box-col" style="width: 98%; float:inherit;" dir="rtl">
    <h2 dir="rtl"> קובץ מימושים חדש</h2>
    <center>
         
        <table dir="rtl" class="trans">
            <tr>
                <th colspan="4" align="center" style="background-color:#3c948b;"><h2>תאריכי יציאה של הזמנות</h2></th>
            </tr> 
            <tr id="mustUpperRow">
                <th>מתאריך יציאה</th>
                <td><input type="text" class="grid_date_style" name="txtFromExitDate" id="txtFromExitDate" /></td> 
                <th>עד תאריך יציאה</th>
                <td><input type="text" class="grid_date_style" name="txtToExitDate" id="txtToExitDate" /></td> 
            </tr>
            
            <tr id="optionalUpperRow" class="optRow">
                <th>מתאריך סיום</th>
                <td><input type="text" class="grid_date_style" name="txtFromTermDate" id="txtFromTermDate" /></td>
                <th>עד תאריך סיום</th>
                <td><input type="text" class="grid_date_style" name="txtToTermDate" id="txtToTermDate" /></td>
            </tr>
          
            <tr>
                <th colspan="4" align="center" style="background-color:#3c948b;"><h2>תאריכי יציאה ועדכון של הזמנות</h2></th>
            </tr>
            <tr>
                <th>מתאריך יציאה</th>
                <td><input type="text" class="grid_date_style" name="txtUpdateFromExitDate" id="txtUpdateFromExitDate" /></td> 
                <th>עד תאריך יציאה</th>
                <td><input type="text" class="grid_date_style" name="txtUpdateToExitDate" id="txtUpdateToExitDate" /> </td> 
            </tr>
            <tr id ="optionalLowerRow" class="optRow">
                <th>מתאריך סיום</th>
                <td><input type="text" class="grid_date_style" name="txtUpdateFromTermDate" id="txtUpdateFromTermDate" /></td>
                <th>עד תאריך סיום</th>
                <td><input type="text" class="grid_date_style" name="txtUpdateToTermDate" id="txtUpdateToTermDate" /></td>
            </tr>
            <tr>
                <th>מתאריך שינוי שובר</th>
                <td><input type="text" class="grid_date_style" name="txtUpdateFromDate" id="txtUpdateFromDate" /> </td> 
                <th>עד תאריך שינוי שובר</th>
                <td><input type="text" class="grid_date_style" name="txtUpdateToDate" id="txtUpdateToDate" /> </td> 
            </tr>
            <tr>
                <th>סוג דוח</th>
                <td colspan="3"><select id="ddlReportType" name="ddlReportType" class="grid_date_style" style="width:75%;text-align:right">
                        <option value="0" selected="selected">רגיל</option>
                        <option value="1" >מורחב</option>
                    </select> 
                </td> 
            </tr>
        </table>
        
        <input type="submit" name="submit" value="חפש" class="button-submit"/> 
        
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
    </center>
</div> 
</asp:Content>

