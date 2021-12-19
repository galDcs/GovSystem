<%@ Page Title="" Language="C#" MasterPageFile="~/MainMasterPage.master" AutoEventWireup="true" CodeFile="GovFalseStatus.aspx.cs" Inherits="GovFalseStatus" %>

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
</style>
 
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="Server">
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
					text: '<i class="far fa-file-excel"></i> Excel'
			},
			{
                extend: 'print',
				text: '<i class="fas fa-print"></i> Print',
                messageTop: function () {
                    return "<div style='text-align:center;'><h4 style='text-align:rtl'>" + $(".reportTitle").html()+ "</h4></div>";
                },
                messageBottom: null
            },
			{
				extend: 'copy',
				text: '<i class="fas fa-copy"></i> Copy'
			}
			
		],
			"oLanguage": {
				"sSearch": '<i class="fas fa-search""></i>',
				"sInfo": '<i class="fas fa-hashtag"></i> _END_ ',
				"sInfoFiltered": " / _MAX_ "
			}} );
		
		
		//$(excelButton).addClass('agnbtn ripple');
	});
</script>

</asp:Content>

