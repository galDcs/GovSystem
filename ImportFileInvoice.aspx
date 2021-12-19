<%@ Page Language="C#" MasterPageFile="~/MainMasterPage.master" AutoEventWireup="true"
  CodeFile="ImportFileInvoice.aspx.cs" Inherits="ImportFileInvoice" Title="Import File Invoice" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="Server">
  <script type="text/jscript">
    $(document).ready(function () {

      $('#ClientRest').click(function () {
        $('[id*="lblError"]').text('');
        $('[id*="TBpaswword"]').val('');
        $('#lpassword').show();
        $(this).hide();
        $('[id*="TBpaswword"]').show();
        $('[id*="Restore"]').show();
      });
      $('#testStop').click(function () {
        $.ajax({
          type: "POST",
          contentType: "application/json; charset=utf-8",
          dataType: "json",
          data: '',
          url: "AjaxService.asmx/StopProcessing",
          error: function (xhr, status, error) {
            alert("Server error accured, please try again later.");
            return false;
          },
          success: function (data, status, xhr) {
            alert(data);
          }
        });
      });
    });
  </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="Server">
<div class="box-col" style="width:95%">
  <h2 dir="rtl">קליטה של קובץ</h2>
  <p>
    <table class="trans" style="direction: rtl; float: right; width: 100%;">
      <tr>
        <td>בחר קובץ
        </td>
        <td>העלאת קובץ לשרת
        </td>
        <td></td>
      </tr>
      <tr>
	  <td>
		עדכון מספר חשבונית GOV לחשבוניות ספק לפי קובץ
	  </td>
	  </tr>
      <tr>
        <td style="vertical-align: top">
          <asp:FileUpload ID="FileUpLoad1" runat="server" />
        </td>
        <td>
          <asp:Button ID="UploadBtn" Text=" שלח" OnCommand="UploadBtn_Click" CommandName=""
            runat="server" Width="105px" Enabled="False"/>
        </td>
        <td>
          <asp:Button ID="ProcessWithDisable" runat="server" Text="עדכן את טבלת חשבוניות" Visible="false"
            OnClick="ProcessUpdateInv_Click" />
        </td>
      </tr>
      <tr>
        <td></td>
        <td></td>
        <td>
          <asp:Panel ID="pnlFileList" runat="server">
          </asp:Panel>
          <asp:Label ID="lblError" runat="server" Text=""></asp:Label>
        </td>
      </tr>
    </table>
    <!--input type="button" value="Stop1" id="testStop" /-->
  </p>
  </div>
  <asp:HiddenField runat="server" ID="FileToUpload" />
   
</asp:Content>
