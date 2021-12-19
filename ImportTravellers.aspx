<%@ Page Language="C#" MasterPageFile="~/MainMasterPage.master" AutoEventWireup="true" CodeFile="ImportTravellers.aspx.cs" Inherits="ImportTravellers" Title="Import travellers" %>

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
	function showProgress() {
          var updateProgress = $get("<%= updateProgress.ClientID %>");
        updateProgress.style.display = "block";
      }
  </script>
  <style>
	.upload{
		width:100%;
		height:300px;
		background:#e7e7e7;
		background-image:url('images/upload.png');
		background-repeat: no-repeat;
		background-position: center; 
		opacity: 0.7;
		margin:0;
		cursor:pointer;
		font-size: 20px;
		font-family: arimo,arial;
	}
    .upload:hover {
        opacity: 0.9;
    }
	th{
	    background: #4D4D4D;
    color: whitesmoke;
    font-weight: 100;
    font-size: 17px;
	}
	.button-sp{
		-moz-box-shadow: inset 0 1px 0 0 #45d6d6;
		-webkit-box-shadow: inset 0 1px 0 0 #45d6d6;
		box-shadow: inset 0 1px 0 0 #45d6d6;
		background-color: #2cbbbb;
		border: 1px solid #27a0a0;
		display: inline-block;
		cursor: pointer;
		color: #fff;
		font-family: sans-serif;
		font-size: 14px;
		padding: 8px 18px;
		text-decoration: none;
		width: 100%;
		height: 3em;
	}
	.button-sp:hover{
		opacity:0.8;
	}
  </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="Server">
<div class="box-col" style="width:95%" dir="rtl">
      <h2 >קליטה של קובץ זכאים</h2>
	  <table dir="rtl" class=""> 
		<tr>
			<th>העלאת קובץ
			<br/>
			(לחץ או גרור לשטח המסומן)
			</th>
		</tr>
		<tr>
		
		<td>
			<asp:FileUpload ID="FileUpLoad1" runat="server" CssClass="upload" />
			<asp:Button ID="UploadBtn" Text="העלאת קובץ" cssclass="button-sp" OnCommand="UploadBtn_Click" CommandName="" runat="server"  OnClientClick="showProgress()"/>
			
        <asp:Button ID="ProcessWithDisable" runat="server" cssclass="button-submit" Text="2. קליטה עם איפוס" Visible="false" OnClick="ProcessWithDisable_Click" OnClientClick="showProgress()"/>
        <asp:Button ID="ProcessWithOutDisable" runat="server" cssclass="button-submit" Text="2. קליטה ללא איפוס" Visible="false" OnClick="ProcessWithOutDisable_Click" OnClientClick="showProgress()"/>
        <asp:Button ID="ProcessWithOutZakautClean" runat="server" cssclass="button-submit" Text="2. קליטה ללא איפוס זכאויות" Visible="false" OnClick="ProcessWithOutZakautClean_Click" OnClientClick="showProgress()"/>
			</td>
		</tr>
	  </table>
        <center style="display:none;">
			<asp:Button ID="StopProcess" runat="server" Text="עצור" Visible="true" cssClass="button-submit button-inline" OnClick="StopProcess_Click" />
			<input type="button" id="ClientRest" value="שחזר" class="button-submit button-inline"/>
		</center>
        <br />
        <br />
        <br />
        <center>
          <label id="lpassword" style="display: none;">סיסמה :</label><asp:TextBox ID="TBpaswword" runat="server" Style="display: none;"></asp:TextBox>
          <asp:Button ID="Restore" runat="server" Text="שחזר" Style="display: none;" OnClick="Restore_Click" cssclass="button-submit"/>
        </center>
        <!--input type="button" value="Stop1" id="testStop" /-->

      </p>
      <%
  //<p><b>OR</b></p>
  //<p>
  //    <asp:Button id="ViewBtnFromSrv" Text="View server files" OnCommand="ViewServerFiles_Click" CommandName="ShowListFiles" runat="server" Width="165px" />
  //</p>
      %>
      <p>
        <asp:Label ID="lblError" runat="server" Text=""></asp:Label>
      </p>
      <p>
        <asp:Panel ID="pnlFileList" runat="server"></asp:Panel>
      </p>
      <p>
        <%
  /*<asp:GridView runat="server" ID="grid" 
            AllowPaging="true" 
            AutoGenerateColumns="true" 
            HeaderStyle-BackColor="Silver"
            HeaderStyle-Font-Bold="true"
            HeaderStyle-HorizontalAlign="Center"
            OnPageIndexChanging="grid_PageIndexChanging"
            PageSize="100"
            PagerStyle-HorizontalAlign="Center"
            PagerStyle-Width="150"
            RowStyle-HorizontalAlign="Right"
            EnableViewState="true"
            >
            <PagerSettings Mode="NumericFirstLast"  Position="TopAndBottom" PageButtonCount="25"/>
        </asp:GridView>*/
        %>
      </p>
      <asp:HiddenField runat="server" ID="FileToUpload" />
      <!--aviran 25/08-->
      <asp:HiddenField runat="server" ID="HebrewFileToUpload" />
      <!--/aviran 25/08 -->
	  </div>
  <asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>
	  
	    <asp:UpdateProgress id="updateProgress" runat="server">
    <ProgressTemplate>
        <div style="position: fixed; text-align: center; height: 100%; width: 100%; top: 0; right: 0; left: 0; z-index: 9999999; background-color: #000000; opacity: 0.7;">
            <span style="border-width: 0px; position: fixed; padding: 50px; background-color: #FFFFFF; font-size: 36px; left: 18%; top: 40%;">התהליך עשוי לקחת מספר דקות אנא המתן</span>
        </div>
    </ProgressTemplate>
</asp:UpdateProgress>
</asp:Content>
