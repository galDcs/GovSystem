<%@ Page Language="C#" MasterPageFile="~/MainMasterPage.master" AutoEventWireup="true" CodeFile="InvoicesFile.aspx.cs" Inherits="InvoicesFile" Title="Invoices File" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
    
    <script type="text/jscript">
         
        $(document).ready(function () {
            
            $('#txtFromDate').datepicker({
                firstDay: 0
            });
            
            $('#txtToDate').datepicker({
                firstDay: 0
            });
            
        });

    </script> 
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">
   <h2 dir="rtl">קובץ חשבוניות</h2>
    <center>
        
        <table dir="rtl" class="MainTable"> 
            <tr>
                <th align="right">מתאריך הפקה</th>
                <td align="right"><input type="text" name="txtFromDate" id="txtFromDate" /> </td>
                <th align="right">עד תאריך הפקה</th>
                <td align="right"><input type="text" name="txtToDate" id="txtToDate" /> </td> 
            </tr>
            <tr>
                <th align="right">סוג דוח</th>
                <td>
                    <select id="ddlReportType" name="ddlReportType" class="grid_date_style" style="width:75%;text-align:right">
                        <option value="0" selected="selected">רגיל</option>
                        <option value="1" >מורחב</option>
                    </select> 
                </td>           
                <th align="right">טיפוס מידע</th>
                <td>
                    <select id="DataType" name="DataType" class="grid_date_style" style="width:75%;text-align:right">
                        <option value="0" selected="selected">טקסט</option>
                        <option value="1" >xml</option>
                    </select> 
                </td>
           </tr> 
            <tr>
                <td colspan=4 align="center">
                    <input type="submit" value="יצוא"/>
                </td> 
            </tr>  
        </table>
        <br/>
    </center>
</asp:Content>
