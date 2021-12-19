<%@ Page Language="C#" MasterPageFile="~/MainMasterPage.master" AutoEventWireup="true" CodeFile="AllocationSearch.aspx.cs" Inherits="AllocationSearch" Title="Allocation search" %>
<%@ Register TagName="UC_GovTraveller" TagPrefix="UC" Src="~/UC_GovTraveller.ascx"%>


<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="Server">

  <style type="text/css">
    .baseSelectorButton {
      font-size: 15.3px !important;
    }

    .baseSelectorSelect {
      width: 38%;
    }

    /*chen -  moving the 'Deals' left.*/
    .center-div {
      left: 50px;
    }

    .order_button {
      color: Blue;
      text-decoration: underline;
      cursor: pointer;
    }

    .hotel-div {
      color: #31708f;
      background-color: #d9edf7;
      border-color: #bce8f1;
    }
	.trans th{width: 8%;}
	.sug_erkev {
    border: 3px double;
    display: block;
    margin: 5px 0;
    text-align: center;
    width: 50%;
    margin: 5px auto;
    padding: 5px;
    border-radius: 5px;
}
  </style>

  <script type="text/javascript">
    var txtFromDateID = '<%= txtFromDate.ClientID%>';
    var txtToDateID = '<%= txtToDate.ClientID%>';
    var txtNightsID = '<%= txtNights.ClientID%>';
    var hotelName = '';
    var escorts = '';
    var marpe = '';
    var marpeName = 'no';
    var marpeAddId = 'no';
    var baseDictionary = {};
    var chosenAddId = '<%= mAddId%>';
    var flagMarpe = true;
	var maxDateForDatePicker;
	var minDateForDatePicker;// =(new Date("03/01/2017").setHours(0, 0, 0, 0) > new Date().setHours(0, 0, 0, 0)) ? new Date("03/01/2017") : new Date().setHours(0, 0, 0, 0);


    hideModal = function () {
      document.getElementById('myModal').style.display = "none";
      marpeName = $('#marpe-selector option:selected').text();
      marpeAddId = $('#marpe-selector').val();
      $('#lbChosenMarpe').innerHtml = marpeName;
      console.log('Op ' + marpeName + ' add' + marpeAddId);
    }

    createMarpe = function (hotelId) {
      //document.getElementById('myModal').style.display = "block";
      $.ajax({
        type: "POST",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        data: "{'iSupplierID':'" + parseInt(hotelId) + "'}",
        url: "./AttractionHandler.asmx/GetAttractionList",
        error: function (xhr, status, error) {
          alert("Server error accured, please try again later.");
          return false;
        },
        success: function (data, status, xhr) {
          if (data != null && data.d != null) {
            if (data.d.length === 0) {
              ShowPopup("מחירון חמי מרפא למלון זה אינו מוגדר");
              flagMarpe = false;
              marpeName = "no";
              marpeAddId = "no";
            }
            else {
              flagMarpe = true;
              //var options = "<option value='0'>בחר סוג אירוח</option>";
              var options;
              for (var i = 0; i < data.d.length; i++) {
                if (data.d[i].AddID.toString() === chosenAddId)
                {
                  //options += "<option value='" + data.d[i].AddID + "'>" + data.d[i].Name + "</option>";
                  marpeName = data.d[i].Name;
                  marpeAddId = data.d[i].AddID;
                }
              }
            }
          }
          //$('#marpe-selector').html(options);//$("." + selectorControlName + " option:first").attr('selected', 'selected');//$('#marpe-selector').show(100);
        }
      })
    };


    $(document).ready(function () {
	var ey = '<%=EntitledYear%>';// @EY
	
	var ldate = "03/01/"+ey;// @EY
	var mdate = "04/01/"+ey;// @EY
	<% if (EntitledYear == "2020" && traveller.makatSelected("027241")) //@corona
	  {%>
		//mdate = "31/08/"+ey;
		//alert(mdate);
	<%}%>
	mdate = "<%=traveller.LastDateToOrder.Date.AddYears(-1).ToString("MM/dd/yyyy")%>";
	console.log(mdate);
	minDateForDatePicker = new Date(ldate);// @EY
	maxDateForDatePicker = new Date(mdate);// @EY
	maxDateForDatePicker.setFullYear(maxDateForDatePicker.getFullYear() + 1);// @EY
	$('#entitled-year').text("שנת זכאות: " + ey);//@EY
      $("#" + txtFromDateID).datepicker({
        minDate: minDateForDatePicker,
		maxDate: maxDateForDatePicker,
        onSelect: function (date) {
          $("#" + txtToDateID).datepicker('option', 'minDate', date);
          CalculateToDate();

        }
      });

      if ($("#" + txtNightsID).val().length <= 0) {
        $("#" + txtNightsID).val("5"); // default 3 nights
      }

      $("#" + txtNightsID).bind('blur', function () {
        CalculateToDate();
      });

      $("#" + txtToDateID).datepicker({
        firstDay: 0,
		minDate: minDateForDatePicker,
		maxDate: maxDateForDatePicker,
        onSelect: function () {
          CalculateNights();
        }

      });

      function CalculateNights() {
        var toDateStr = $("#" + txtToDateID).val();
        var fromDateStr = $("#" + txtFromDateID).val();
        var one_day = 1000 * 60 * 60 * 24; // milisec
        var daysDiff = Math.round((ConvertToDate(toDateStr) - ConvertToDate(fromDateStr)) / one_day);
        $("#" + txtNightsID).val(daysDiff);
      }


      function ConvertToDate(dateStr)//converts from dd/mm/yy format
      {
        var arr = dateStr.split("/");
        return new Date(arr[2], arr[1] - 1, arr[0]);
      }

      function CalculateToDate() {
        var nights = parseInt($("#" + txtNightsID).val());
        var fromDate = ConvertToDate($("#" + txtFromDateID).val());
        var newToDate = new Date(fromDate.setDate(fromDate.getDate() + nights));
        $("#" + txtToDateID).val($.datepicker.formatDate("dd/mm/yy", newToDate));
      }

      // retrieve and build base combo
      $(".baseSelectorButton").button().bind('click', function (el) {
		
        var rowIndex = $(this).attr('rowIndex');
        var hotelId = $(this).attr('hotelid');
        var allocationId = $(this).attr('allocationId');
        var txtFromDateID = '<%= txtFromDate.ClientID%>';
        var txtToDateID = '<%= txtToDate.ClientID%>';
        //chen - new attr for new order
        hotelName = $(this).attr('hotelname').toString();
        escorts = $(this).attr('escorts').toString();
          hameiMarpey = $(this).attr('hotelName').toString();
        // /chen
        
        var selectorControlName = $(this).attr('selectorControlName');
          GetPriceBaseTypes(hotelId, selectorControlName, txtFromDateID, txtToDateID, allocationId);
        $(this).unbind(); // prevent additional clicks
        $(this).hide(100);
      });
    });

	
    var currHotelId, currAllocationId;
      function GetPriceBaseTypes(hotelId, selectorControlName, FromDate, ToDate, allocationId) {
      currHotelId = hotelId;
        currAllocationId = allocationId;
      $.ajax({
        type: "POST",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
          data: "{'hotelId':'" + hotelId + "', 'FromDate':'" + FromDate + "', 'ToDate':'" + ToDate + "' }",
        url: "./AjaxService.asmx/GetPriceBaseTypes",
        error: function (xhr, status, error) {
          ShowPopup("Server error accured, please try again later.");
          return false;
        },
        success: function (data, status, xhr) {
          if (chosenAddId != "") {
            createMarpe(hotelId);
            //$('<%=dGeneralAreaId.ClientID%>').prop('disabled', 'disabled');


          }
          if (data != null && data.d != null) {
            if (data.d.length === 0) {
              ShowPopup('המחירון אינו מקושר להקצאות');
            }
            else {

              var options = "<option value='0'>בחר סוג אירוח</option>";
			  if (data.d.length > 1) {
			  var textError = 'יותר מבסיס אירוח 1.';
				alert(textError);
				options += "<option value='0'>" + textError + "</option>";
				$(this).after('<div><label>' + textError + '</label></div>');
			  }
			  else {
				  for (var i = 0; i < data.d.length; i++) {
					options += "<option value='" + data.d[i].Id + "'>" + data.d[i].Name + "</option>";
					baseDictionary[data.d[i].Id] = data.d[i].Name;
				  }
			  }
            }
          }

          
          
          $("." + selectorControlName).html(options);
          $("." + selectorControlName).attr("hotelId", currHotelId);
		  $("." + selectorControlName).attr("isNewPrice", isNewPrice);
          $("." + selectorControlName).attr("allocationId", currAllocationId);
          $("." + selectorControlName + " option:first").attr('selected', 'selected');
          $("." + selectorControlName).show(100);

          // binding change for base selector

          $("." + selectorControlName).bind('change', function () {
            var baseId = $(this).val();
            var hotelId = $(this).attr('hotelId');
			var isNewPrice = $(this).attr('isNewPrice');
			
            var allocationId = $(this).attr('allocationId');
            //var fromDate = $("#" + txtFromDateID).val();
            //var toDate = $("#" + txtToDateID).val();
			var fromDate = $("#" + '<%= lblFd.ClientID %>').text();
			var toDate = $("#" + '<%= lblTd.ClientID %>').text();
			var lnights = $("#" + '<%= lblN.ClientID %>').text();
			debugger;
            var isSameMonthOfSelectedDates = (fromDate.substring(3, 5) == toDate.substring(3, 5));
            var priceField = $(this).attr('hotelPriceAmountFieldName');
            var orderBtnName = $("." + priceField).attr('orderbuttonctrname');
            var FourOneSeven = $(this).prev().attr('FourOneSeven');

            var priceId = 0;
            var amountNeto = 0;
            var amountBruto = 0;
            var TotalAmountToShow = 0;
            var ZakayPays = 0;
            var ZakaySibsud = 0;
            var MelavePays = 0;
            var MelaveSibsud = 0;

            if (baseId == null || baseId == "" || baseId == 0 || !flagMarpe) {
              var errorMsg = "";
              if (!flagMarpe) {
                errorMsg = "מחירון חמי מרפא למלון זה אינו מוגדר";
              }
              else {
                errorMsg = "מחירון זה אינו מוגדר כראוי";
              }
              $(this).after('<div><label>' + errorMsg + '</label></div>');
              $('.' + orderBtnName).hide();
              return;
            }

            // commnted at 2013.08.11 - to allow cross month search
            //if (isSameMonthOfSelectedDates && erkevType != "fourNightHotel5Tipulim" && erkevType != "fiveNightHotelTipulim") {
            // commented at 2013.08.15 - only if selected 5th night pay - need to put 10 NIS

            // commented at 2013.10.28 - No 10NIS orders
            //if (FourOneSeven != "fiveNightHotelTipulim") {

            // getting the price for selected allocation
			var tblAllocationsIdCtrName = "<%=tblAllocations.ClientID%>";
             var datesSelectedStr = "";
             $("#" + tblAllocationsIdCtrName + " input:checkbox").each(function () {
               datesSelectedStr += (this.checked) ? "1" : "0";
             });
			 
            $.ajax({
              type: "POST",
              contentType: "application/json; charset=utf-8",
              dataType: "json",
              data: "{'hotelId':'" + hotelId + "', 'isNewPrice':'" + isNewPrice + "', 'allocationId':'" + allocationId + "', 'baseId':'" + baseId + "', 'fromDateStr':'" + (fromDate + "|" + datesSelectedStr) + "', 'toDateStr':'" + toDate + "' }",
              url: "AjaxService.asmx/GetHotelPrice",
              error: function (xhr, status, error) {
                var err = $.parseJSON(xhr.responseText);
                ShowPopup(err.Message);
                //alert("Server error accured, please try again later.\n(" + xhr.responseText + ")");
                $('.' + orderBtnName).hide();
                $('.' + orderBtnName).after('<div><label>' + err.Message + '</label></div>');
                return false;
              },
              success: function (data, status, xhr) {

                if (data != null && data.d != null) {
                  $('.' + orderBtnName).show();

                  priceId = data.d["PriceId"];

                  amountNeto = data.d["PriceAmountNetto"];
                  amountBruto = data.d["PriceAmountBruto"];
                  TotalAmountToShow = data.d["TotalAmountToShow"];
                  ZakayPays = data.d["ZakayPays"];
                  ZakaySibsud = data.d["ZakaySibsud"];
                  MelavePays = data.d["MelavePays"];
                  MelaveSibsud = data.d["MelaveSibsud"];
				
				  if (data.d["TravellerPriceToPay"] != undefined) {
					debugger;
					var TravellerPriceToPay = data.d["TravellerPriceToPay"];
					if (MelavePays == "") {
						MelavePays = TravellerPriceToPay;
					}
					// var msg = "עלייך להוסיף " + TravellerPriceToPay + "₪ להזמנה";
					// ShowPopup(msg);
					// $('#< % = lblMessagePay.ClientID %>').html(msg);
					// $('#< % = lblMessagePay.ClientID %>').css('display', 'block');
				  }

                } else {
                  $('.' + orderBtnName).hide();
                }
              }
            });
            //}

            // not need to show full price, but only part that payable
            //$("." + priceField).text(amountNeto);
            $("." + priceField).text(TotalAmountToShow);

            if (parseFloat(TotalAmountToShow) > 0) {
              $("." + priceField).show(100);
              $("." + orderBtnName).show(100);
              $("." + orderBtnName).attr('alt', amountBruto);
            }

            $("." + orderBtnName).bind('click', function () {
              $("." + orderBtnName).hide();
              var areaIdCtrName = "<%=dGeneralAreaId.ClientID%>";
             var tblAllocationsIdCtrName = "<%=tblAllocations.ClientID%>";
             var datesSelectedStr = "";
             $("#" + tblAllocationsIdCtrName + " input:checkbox").each(function () {
               datesSelectedStr += (this.checked) ? "1" : "0";
             });

             //leave empty string validation for old browsers
             if (datesSelectedStr != '' && parseInt(datesSelectedStr) == 0) {
               ShowPopup('יש לבחור לפחות יום אחד עבור המלווה');
             }
             else {

               var baseNameByID = baseDictionary[baseId];
               //var dataToSend = 'fromDate=' + fromDate + '&toDate=' + toDate + '&numberOfNights=' + $("#" + txtNightsID).val() + '&melavim=' + escorts + '&hotelName=' + hotelName + '&marpe=' + hameiMarpey + '&base=' + baseNameByID;
               //location.href = '/AcceptOrder.aspx?' + dataToSend;
               //////ajax

               var dataToSend = "{'hotelId':'" + hotelId + "', 'priceId':'" + priceId + "', 'allocationId':'" + allocationId + "', 'baseId':'" + baseId + "', 'fromDateStr':'" + fromDate + "', 'toDateStr':'" + toDate + "', 'amountNetoStr':'" + amountNeto + "', 'amountBrutoStr':'" + amountBruto + "', 'zakayPaysStr':'" + ZakayPays + "', 'zakaySibsudStr':'" + ZakaySibsud + "', 'melavePaysStr':'" + MelavePays + "', 'melaveSibsudStr':'" + MelaveSibsud + "', 'areaId':'" + $("#" + areaIdCtrName).val() + "', 'datesSelectedStr':'" + datesSelectedStr.toString() + "' }";

               $.ajax({
                 type: "POST",
                 contentType: "application/json; charset=utf-8",
                 dataType: "json",
                 //data: "{'hotelId':'"+ hotelId +"', 'priceId':'"+ priceId +"', 'allocationId':'"+ allocationId +"', 'baseId':'"+ baseId +"', 'fromDateStr':'"+ fromDate +"', 'toDateStr':'"+ toDate +"', 'amountNetoStr':'"+ amountNeto +"', 'amountBrutoStr':'"+ amountBruto +"' }",
                 data: dataToSend,
                 url: "./AjaxService.asmx/MakeOrder",
                 error: function (xhr, status, error) {
                   ShowPopup("Server error accured, please try again later.");
                   $("." + orderBtnName).show();
                   return false;
                 },
                 success: function (data, status, xhr) {
                   var AgencyDocketId = data.d["AgencyDocketId"];
                   var AgencyVoucherId = data.d["AgencyVoucherId"];
                   var OrderCompleted = data.d["OrderCompleted"];
                   var OrderMessage = data.d["OrderMessage"];

                   if (OrderCompleted == false) {
                     ShowPopup("Error: " + OrderMessage);
                     $("." + orderBtnName).show();
                     return false;
                   }
                   else {
                     var dataToSend = 'DocketId=' + AgencyDocketId + '&VoucherId=' + AgencyVoucherId + '&fromDate=' + fromDate + '&toDate=' + toDate + '&numberOfNights=' + lnights + '&melavim=' + escorts + '&hotelName=' + hotelName + '&marpe=' + hameiMarpey + '&base=' + baseNameByID + '&baseid=' + baseId + '&supplierid=' + hotelId + "&allocationid=" + allocationId + "&marpename=" + marpeName + "&addid=" + marpeAddId;
                     uri = './AcceptOrder.aspx?' + dataToSend;
                     encodeURI(uri);
                     location.href = uri;
                     //location.href = "OrderDetails.aspx?DocketId=" + AgencyDocketId + "&VoucherId=" + AgencyVoucherId;
                     return;
                   }
                 }
               });


             } //eof if-else validation block	

           });
         });
        }
      });
   }
   
   function orderTreatment(iSupplierId){
		var dataToSend = "{'iSupplierId':'" + iSupplierId + "'}";
		$(".box-col").hide();
		$.ajax({
			type: "POST",
			contentType: "application/json; charset=utf-8",
			dataType: "json",
			data: dataToSend,
			url: "./AjaxService.asmx/MakeOrderTreatment",
			error: function (xhr, status, error) {
				ShowPopup("Server error accured, please try again later.");
				return false;
			},
			success: function (data, status, xhr) {
			debugger;
				var AgencyDocketId = data.d["AgencyDocketId"];
				var AgencyVoucherId = data.d["AgencyVoucherId"];
				var OrderCompleted = data.d["OrderCompleted"];
				var OrderMessage = data.d["OrderMessage"];
				var fromDateTO = $("#" + '<%= lblFd.ClientID %>').text();
				var toDateTO = $("#" + '<%= lblTd.ClientID %>').text();
				var lnightsTO = $("#" + '<%= lblN.ClientID %>').text();
	
				if (OrderCompleted == false) {
					ShowPopup("Error: " + OrderMessage);
					return false;
				}
				else {
					var dataToSend = 'TreatmentOnly=True&DocketId=' + AgencyDocketId + '&VoucherId=-1&fromDate=' + fromDateTO
									+ '&toDate=' + toDateTO + '&numberOfNights=' + lnightsTO
									+ '&melavim=0&hotelName=-1&marpe=1&base=0&baseid=0&supplierid=' + iSupplierId
									+ "&allocationid=-1&marpename=-1&addid=1";
					uri = './AcceptOrder.aspx?' + dataToSend;
					encodeURI(uri);
					location.href = uri;
					return;
				}
			}
		});
   };

  </script>
  <style>
  
  </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="Server">

  <div class="box-col" style="width: 98%;" dir="rtl">

    <UC:UC_GovTraveller runat="server" ID="gtUC" />
	<asp:label runat="server" id="lblSugErkev" CssClass='sug_erkev'></asp:label>
    <div dir="rtl" style="margin: 0 auto;">
      
    </div>
    <div style="border-width: thin; border-style: solid">
      <div dir="rtl" style="height: 22px; font-size: 18px; font-weight: bold;">פרטי חיפוש:</div>
	  <label id="entitled-year"></label><!--@EY-->
	  <asp:label runat="server" id="lbLastDateToOrder" style="direction: ltr;text-align: center;width: 100%;display: block;"></asp:label>
      <table width="80%" dir="rtl" class="trans">
        <tr>
          <th>מתאריך</th>
          <td>
            <asp:TextBox runat="server" ID="txtFromDate" autocomplete='off'></asp:TextBox></td>

          <th>עד תאריך</th>
          <td>
            <asp:TextBox runat="server" ID="txtToDate" autocomplete='off'></asp:TextBox></td>
        </tr>
        <tr>
          <th>מס' לילות</th>
          <td>
            <asp:TextBox runat="server" ID="txtNights" MaxLength="2"></asp:TextBox></td>
          <th>אזור</th>
          <td>
            <asp:DropDownList runat="server" ID="dGeneralAreaId" DataValueField="GeneralAreaId" DataTextField="GeneralAreaName">
            </asp:DropDownList>
          </td>

        </tr>
        <tr>
          <td colspan="6">
            <asp:Button runat="server" ID="btnSend" OnClick="btnSendOnClick" Text="חפש" CssClass="button-submit" /></td>
        </tr>
        <tr>
          <td>
		   <asp:LinkButton OnClick="btGoToAllocationSearchSplit_Click" runat="server" ID="btGoToAllocationSearchSplit" Text="לפיצול הזמנה ל-2 מלונות" style="font-size:13px; color:blue;" Visible="false" />
		  </td>
          <td style="color: Red">
		  <asp:label runat="server" id="lbChosenMarpe" class="attraction-name" Visible="false"></asp:label>
            <asp:Label runat="server" ID="ZakaiAndMelavePartLabel1" Text=""></asp:Label>
          </td>
		  
          <td colspan="3">
		  </td>
          <td style="color: Red">
            <asp:Label runat="server" ID="ZakaiAndMelavePartLabel2" Text=""></asp:Label>
          </td>
          <td colspan="3"></td>
        </tr>
		<tr>
		<td colspan="4" style="color: #DC143C;font-weight: bold;font-size:16px;font-family:tahoma,arial;">
			<asp:Label runat="server" ID="lblMoneySKU" Visible="false"  Text="(₪) שימו לב! נבחר מסלול הכולל תשלום, נא לא לשכוח לבקש מהזכאי פרטי אשראי" style="text-shadow: 1px 1px #888888;"/>
			<br/>
			<asp:Label runat="server" ID="lblMoneyHoliday" Visible="false" Text="(₪) שימו לב! אחד או יותר מהימים שנבחרו כולל חג, אם ישנם תוספות תשלום נא לא לשכוח לבקש מהזכאי פרטי אשראי" style="text-shadow: 1px 1px #888888;"/>
		</td>
		</tr>
      </table>
    </div>
	<h2 style="color:red;"><asp:Label runat="server" ID="lbSpecialRoomMessage" Text=""></asp:Label></h2>


    <center>
		<h2>
			<asp:Label runat="server" ID="lblMessage" Visible="false" CssClass="attraction-name"  />
		</h2>
	</center>
    <br />
    <asp:Table runat="server" ID="tblAllocations" style="display:none;" CssClass="MainTable">
    </asp:Table>
    <br />
   
    <div class="content">
      <div class="deals" style="width: auto;">
        <asp:PlaceHolder ID="PlaceHolder1" runat="server"></asp:PlaceHolder>
      </div>
    </div>
  </div>


  <div id="myModal" class="modal">
    <!-- Modal content -->
    <div class="modal-content">
      <div class="modal-header">
        <p>אנא בחר חמי מרפא</p>
      </div>
      <div id="selector">
        <select id="marpe-selector"></select>
      </div>
      <span class="close button-modal" id="submit-modal" onclick="hideModal()">אישור</span>
    </div>
  </div>
  <div id="xcds">
	<asp:Label runat="server" ID="lblFd" style="display:none;" Text=""></asp:Label>
	<asp:Label runat="server" ID="lblTd" style="display:none;" Text=""></asp:Label>
	<asp:Label runat="server" ID="lblN" style="display:none;" Text=""></asp:Label>
  </div>
</asp:Content>
