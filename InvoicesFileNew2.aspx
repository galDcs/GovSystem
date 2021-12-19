<%@ Page Language="C#" MasterPageFile="~/MainMasterPage.master" AutoEventWireup="true" CodeFile="InvoicesFileNew2.aspx.cs" Inherits="InvoicesFileNew" Title="Mimushim Duplicate File" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
    
    <script type="text/jscript">
        
    
    $(document).ready(function () {
               $('.grid_date_style').datepicker({});
             
        });
      
    
     
    </script>
    
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">
     <h2 dir="rtl">קובץ חשבוניות חדש</h2>
    <center>
       
        <table dir="rtl" class="MainTable">
            <tr>
                <th colspan="4" align="center">תאריכי יציאה של הזמנות</th>
            </tr> 
            <tr id="mustUpperRow">
                <td>מתאריך יציאה</td>
                <td><input type="text" class="grid_date_style" name="txtFromExitDate" id="txtFromExitDate" /></td> 
                <td>עד תאריך יציאה</td>
                <td><input type="text" class="grid_date_style" name="txtToExitDate" id="txtToExitDate" /></td> 
            </tr>
            
            <tr id="optionalUpperRow" class="optRow">
                <td>מתאריך סיום</td>
                <td><input type="text" class="grid_date_style" name="txtFromTermDate" id="txtFromTermDate" /></td>
                <td>עד תאריך סיום</td>
                <td><input type="text" class="grid_date_style" name="txtToTermDate" id="txtToTermDate" /></td>
            </tr>
          
            <tr>
                <th colspan="4" align="center">תאריכי יציאה ועדכון של הזמנות</th>
            </tr>
            <tr>
                <td>מתאריך יציאה</td>
                <td><input type="text" class="grid_date_style" name="txtUpdateFromExitDate" id="txtUpdateFromExitDate" /></td> 
                <td>עד תאריך יציאה</td>
                <td><input type="text" class="grid_date_style" name="txtUpdateToExitDate" id="txtUpdateToExitDate" /> </td> 
            </tr>
            <tr id ="optionalLowerRow" class="optRow">
                <td>מתאריך סיום</td>
                <td><input type="text" class="grid_date_style" name="txtUpdateFromTermDate" id="txtUpdateFromTermDate" /></td>
                <td>עד תאריך סיום</td>
                <td><input type="text" class="grid_date_style" name="txtUpdateToTermDate" id="txtUpdateToTermDate" /></td>
            </tr>
            <tr>
                <td>מתאריך שינוי שובר</td>
                <td><input type="text" class="grid_date_style" name="txtUpdateFromDate" id="txtUpdateFromDate" /> </td> 
                <td>עד תאריך שינוי שובר</td>
                <td><input type="text" class="grid_date_style" name="txtUpdateToDate" id="txtUpdateToDate" /> </td> 
            </tr>
            <tr>
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
        
        <input type="submit" name="חפש"/> 
        
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
</asp:Content>
