<%@ Page Title="" Language="C#" MasterPageFile="~/MainMasterPage.master" AutoEventWireup="true" CodeFile="ManageHolidays.aspx.cs" Inherits="ManageHolidays" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
<style>
    .datepicker
    {
        color:Black;
    }
    .inputWide
    {
        width:90%;
    }
</style>
    <script>
        var index = <%=rowIndex.ToString() %>

        $(document).ready(function () {
            $(".datepicker").datepicker({dateFormat:'dd-mm-y'}).val();
            $("#btnAdd").button().click(function () {
                addRow();
            });
            $("#btnSubmit").button().click(function () {
                $("#hdAction").val("actionSave");
                $("#hdCurrentRowIndex").val(index);
            });
        });

        function addRow() {
            index++;
            var target = $('#mainTable tr:last');
            //$("#divMain").append("<span style='display:block;'><input id='holidayPK" + index + "' name='holidayPK" + index + "' type='hidden' value='0'><input type='text' id='datepicker" + index + "' name='datepicker" + index + "' class='datepicker' value=''/><input type='text' id='description" + index + "' name='description" + index + "' value=''><input type='checkbox' id='chbDelete" + index + "' name='chbDelete" + index + "'/>Delete </span>");
            target.after("<tr><input id='holidayPK" + index + "' name='holidayPK" + index + "' type='hidden' value='0' /><td><input type='text' id='datepicker" + index + "' name='datepicker" + index + "' class='datepicker inputWide' value='' /></td><td><input type='text' id='description" + index + "' name='description" + index + "' class='inputWide' value='' /> </td><td><input type='checkbox' id='chbDelete" + index + "' name='chbDelete" + index + "' />סמן למחיקה</td></tr>");
            $(".datepicker").datepicker({dateFormat:'dd-mm-y'}).val();
        }
    </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">
    <h2 dir="rtl">ניהול חגים</h2>
    <center>
    
       
        <div id="divMain">
            <%=resultHolidays.ToString()%>
        </div>
        <asp:Label runat="server" ID="lblMessage" Visible=false ></asp:Label>
        <br/>
        <input id="btnAdd" type="button" value="הוסף חדש" style="width:100px;" />
        <input id="btnSubmit" type="submit" value="שמור" style="width:100px;" />
        <input id="hdAction" name="hdAction" type="hidden" value="actionSave" />
        <input id="hdCurrentRowIndex" name="hdCurrentRowIndex" type="hidden" value="0" />
    </center>
</asp:Content>

