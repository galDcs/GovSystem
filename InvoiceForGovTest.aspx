<%@ Page Title="" Language="C#" MasterPageFile="~/MainMasterPage.master" AutoEventWireup="true" CodeFile="InvoiceForGovTest.aspx.cs" Inherits="InvoiceForGovTest" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="Server">
  <script>
    $(document).ready(function () {
	debugger;
	  if ('<%=toShow%>' == 'True'){
		$('#lblDownloadLink').show();
	  }
	  else
	  {
		$('#lblDownloadLink').hide();
	  }
	  
    });

    function PrintPage() {
    }

    //<![CDATA[
    function showLoading() {
    //  $('.pleaseWait').show();
    }
    //]]>
  </script>
  <style>
    .trans th {
      font-size: 21px;
      background: url(search-dark.png) no-repeat 10px 6px #6d6b6b;
      color: white;
      width: 150px;
      padding: 6px 15px 6px 35px;
      border-radius: 20px;
      box-shadow: 0 1px 0 #ccc inset;
      width: 30%;
    }

    .trans td {
      border: 1px solid #ccc;
      padding: 5px;
      box-sizing: border-box;
      box-shadow: 0 6px 10px #DFDCDC inset;
      width: 50%;
    }

    .trans table {
      margin: auto;
      width: 960px;
      padding: 30px 0 50px;
      clear: both;
      overflow: hidden;
    }

    input, select {
      width: 340px;
    }

    .button {
      background: linear-gradient(to bottom, rgba(0,152,209,1) 0%,rgba(0,105,144,1) 100%);
      filter: progid:DXImageTransform.Microsoft.gradient( startColorstr='#0098d1', endColorstr='#006990',GradientType=0 );
      border: medium none;
      border-radius: 8px;
      box-shadow: none;
      color: #fff;
      float: left;
      font-family: "dcs",arial;
      font-size: 17px;
      line-height: 40px;
      padding: 0;
      width: 140px;
      text-align: center;
      cursor: pointer;
      text-decoration: none;
    }

      .button:hover {
        background: linear-gradient(to bottom, rgb(23, 39, 45) 0%,rgba(0,105,144,1) 100%);
      }

    .form-view {
      max-width: 91%;
      background: #FAFAFA;
      padding: 30px;
      margin: 50px auto;
      box-shadow: 1px 1px 25px rgba(0, 0, 0, 0.35);
      border-radius: 10px;
      border: 6px solid #305A72;
      height: 100%;
      font-size: 11px;
      font-family: Arimo, Arial, Helvetica, sans-serif;
    }

    .calender-pic {
      background: #fff url(images/cal.png) no-repeat 5px center;
      background-size: inherit !important;
    }

    .table-result {
      width: 100%;
      background-image: url(images/white_border.jpg);
      background-position: left top;
      background-repeat: repeat-y;
      /* min-width: 8%; */
      padding: 10px;
      vertical-align: top;
      border-bottom: 1px solid #fff;
    }

      .table-result td {
        /*padding:1%;*/
      }

    .table-result-tr {
      padding: 2%;
      background: #d4d4d4;
    }

    .bg-color {
      background: #4e4d4d;
      background: -moz-linear-gradient(top, #3f3f3f 0%, #727272 100%);
      background: -webkit-gradient(linear, left top, left bottom, color-stop(0%,#3f3f3f), color-stop(100%,#727272));
      background: -webkit-linear-gradient(top, #3f3f3f 0%,#727272 100%);
      background: -o-linear-gradient(top, #3f3f3f 0%,#727272 100%);
      background: -ms-linear-gradient(top, #3f3f3f 0%,#727272 100%);
      background: linear-gradient(to bottom, #3f3f3f 0%,#727272 100%);
      filter: progid:DXImageTransform.Microsoft.gradient( startColorstr='#3f3f3f', endColorstr='#727272',GradientType=0 );
      color: #fff;
    }
  </style>

  <div id="formView" class="form-view" style="width: 700px; float: inherit;" dir="rtl">
    <div id="printDiv">
      <h1 dir="rtl">יצירת קובץ חשבוניות למשרד הבטחון</h1>
      <center>

        <div runat="server" id="searchDiv">
          <table dir="rtl" style="font-size: 17px;">
            <tr>
              <td>העלה קובץ חשבוניות
         </td>
            </tr>
            <tr>
              <td>
                <asp:FileUpload ID="FileUpLoadInvoice" runat="server" />
              </td>
            </tr>
          </table>
		  		  
		<a href="http://web08.agency2000.co.il/GovSystem/Logs/CareInvoiceForZip/careInvoices.zip" id="lblDownloadLink" style="color:blue; font-size:16px;">Download</a>
					
          <div id="button">
            <asp:Button ID="Upload" runat="server" class="button" Text="צור קובץ XML" OnClick="Upload_Click" OnClientClick="showLoading()" />
          </div>
        </div>
      </center>

    </div>
    <div style="position: fixed; text-align: center; height: 100%; width: 100%; top: 0; right: 0; left: 0; z-index: 9999999; background-color: #000000; opacity: 0.7; display: none;" class="pleaseWait">
      <span style="border-width: 0px; position: fixed; padding: 50px; background-color: #FFFFFF; font-size: 36px; left: 37%; top: 40%;">
        <img src="css/images/gears.gif" />
        <br />
        Please Wait
      </span>
    </div>	
</asp:Content>

