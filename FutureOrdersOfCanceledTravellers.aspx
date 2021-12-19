<%@ Page Title="" Language="C#" MasterPageFile="~/MainMasterPage.master" AutoEventWireup="true" CodeFile="FutureOrdersOfCanceledTravellers.aspx.cs" Inherits="FutureOrdersOfCanceledTravellers" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="Server">
<style>
.status{
text-align:center;
}
.status.active{
	color:green;
}
.status.disabled{
	color:red;
}
.table-result{
	direction:rtl;
}
html{
	direction:rtl;
	background: #eee;
}
input[type="search"], input {
    padding: 1px;
    font-size: 11px;
    width: auto;
    height: auto;
    border-radius: 0;
    box-shadow: none;
}
div#tblResult_wrapper {
    border: 1px solid #fff;
    padding: 4px;
    margin: 0 auto;
    float: right;
    width: 100%;
    background: #fff;
    box-shadow: 1px 1px 1px #ccc;
}
body {
    width: 90%;
    margin: 0 auto;
}
button {
    color: #fff;
    background-color: #337ab7;
    border-color: #2e6da4;
	display: inline-block;
    margin-bottom: 0;
    font-weight: 400;
    text-align: center;
    white-space: nowrap;
    vertical-align: middle;
    -ms-touch-action: manipulation;
    touch-action: manipulation;
    cursor: pointer;
    background-image: none;
    border: 1px solid transparent;
    padding: 6px 12px;
    font-size: 14px;
    line-height: 1.42857143;
    border-radius: 4px;
    -webkit-user-select: none;
    -moz-user-select: none;
    -ms-user-select: none;
    user-select: none;
}
h1{
color: #2e6da4;
}
th {
    color: #2e6da4;
    border-bottom: 1px solid #2e6da4!important;
}
</style>
 
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="Server">
<h1>הזמנות עתידיות לזכאים מבוטלים</h1>
<h3><%=DateTime.Now.ToString()%></h3>
<asp:Label id="lbResult" runat="server" />

<% Response.WriteFile("css/DATATABLE/datatable.txt"); %>
<script>
$(document).ready(function () {
		$('#tblResult').DataTable( {
		dom: 'Blfrtip',
		"paging": false,
		"searching": true,	
		"ordering": true,		
		"lengthMenu": [[10, 25, 50, -1], [10, 25, 50, "All"]],	
		buttons: [
			{
					extend: 'excel',
					text: ' Excel'
			},
			{
                extend: 'print',
				text: ' הדפסה',
                messageTop: function () {
                    return "<div style='text-align:center;'><h4 style='text-align:rtl'>הזמנות עתידיות לזכאים מבוטלים</h4></div>";
                },
                messageBottom: null
            },
			{
				extend: 'copy',
				text: ' העתקה'
			}
			
		],
			"oLanguage": {
				"sSearch": 'חיפוש בתוצאות: ',
				"sInfo": '# _END_ ',
				"sInfoFiltered": " / _MAX_ "
			}} );
		
		
		//$(excelButton).addClass('agnbtn ripple');
	});
</script>

</asp:Content>

