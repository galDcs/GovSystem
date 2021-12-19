<%@ Page Title="" Language="C#" MasterPageFile="~/MainMasterPage.master" AutoEventWireup="true" CodeFile="ChangeDocketPrices.aspx.cs" Inherits="ChangeDocketPrices" %>





<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="Server">

  <style type="text/css">
    #folders {
      background: #fafafa;
      float: right;
      width: 100%;
      position: relative;
    }

      #folders:before, #folders:after {
        z-index: -1;
        position: absolute;
        content: "";
        bottom: 15px;
        left: 10px;
        width: 50%;
        top: 80%;
        max-width: 300px;
        background: #777;
        -webkit-box-shadow: 0 15px 10px #777;
        -moz-box-shadow: 0 15px 10px #777;
        box-shadow: 0 15px 10px #777;
        -webkit-transform: rotate(-3deg);
        -moz-transform: rotate(-3deg);
        -o-transform: rotate(-3deg);
        -ms-transform: rotate(-3deg);
        transform: rotate(-3deg);
      }

      #folders:after {
        -webkit-transform: rotate(3deg);
        -moz-transform: rotate(3deg);
        -o-transform: rotate(3deg);
        -ms-transform: rotate(3deg);
        transform: rotate(3deg);
        right: 10px;
        left: auto;
      }

    .head-special {
      /*color: #CEF0D4; font-family: 'Rouge Script', cursive; font-size: 55px; font-weight: normal; line-height: 48px; margin: 0  50px; text-align: center; text-shadow: 1px 1px 2px #082b34; */
    }


    label {
      font-weight: bold;
    }
    label:after { content: ": " }
  </style>
</asp:Content>


<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="Server">

<div runat="server" id="fullPage">

  <div class="box-col" style="width:95%">

  <br /><br />
  <div id="folders">

  <table style="width:100% !important; direction:rtl; float:right; " class="trans">
    <tr>
      <th style="width:10%;">שם מלון</th>
      <td style="width:40%;">
        <asp:DropDownList runat="server" ID="ddlHotels" DataValueField="id" DataTextField="NameAndArea">
        </asp:DropDownList>
      </td>
      <th style="width:10%;"></th>
      <td style="width:40%;">
          <asp:DropDownList runat="server" ID="ddlServiceType" DataValueField="id" DataTextField="NameAndArea">
              <asp:ListItem Value="2" Text="מלון"></asp:ListItem>
              <asp:ListItem Value="8" Text="טיפולים"></asp:ListItem>
        </asp:DropDownList>
      </td>
    </tr>
    
    <tr>
      <th style="width:10%;">מתיק</th>
      <td style="width:40%;">
        <asp:TextBox runat="server" ID="txtFromDocket"></asp:TextBox></td>
      <th style="width:10%;">עד תיק</th>
        <td style="width:40%;">
        <asp:TextBox runat="server" ID="txtToDocket"></asp:TextBox></td>
    </tr>

    <tr>
      <th style="width:10%;">מתאריך</th>
      <td style="width:40%;">
        <asp:TextBox runat="server" ID="txtFromDate"></asp:TextBox></td>
      <th style="width:10%;">עד תאריך</th>
      <td style="width:40%;">
        <asp:TextBox runat="server" ID="txtToDate"></asp:TextBox></td>
    </tr>
  </table>
    <asp:Button ID="btSearch" runat="server" OnClick="btSearch_Click" Text="חפש" CssClass="button-submit" 
      style="float:right; margin-top:6px; margin-right:10px; width:99%;"/>

    <div id="docket-id" style="width:100%; float:right; direction:rtl; background-color:#4cdaa4; text-align:center; font-size:26px;">
      <br />
      <label>מספר תיק</label>
      <asp:Label ID="lbDocketId" runat="server"></asp:Label>
      <br />

      <!-- BundleDetails -->
      <div id="hotel-name">
        <label>שם מלון</label>
        <asp:Label ID="lbHotelName" runat="server"></asp:Label>
      </div>
      <div id="from-date">
        <label>תאריך יציאה</label>
        <asp:Label ID="lbFromDate" runat="server"></asp:Label>
      </div>
      <div id="to-date">
        <label>תאריך חזרה</label>
        <asp:Label ID="lbToDate" runat="server"></asp:Label>
      </div>
      <div id="subsid-base">
        <label>סיבסוד לפי</label>
        <asp:Label ID="lbSubsidBase" runat="server"></asp:Label>
      </div>
    </div>

    <div class="content" style="direction:rtl; width:40%;">
      <div id="priceHeader" style="font-size:30px; color:darkblue; text-decoration: underline;">
        <label for="lbPriceName" class="head-special">שם מחירון</label>
        <asp:Label ID="lbPriceName" runat="server" CssClass="head-special"></asp:Label>
      </div>
    <br /><br />
      <asp:Table ID="tablePriceDetails" runat="server" Width="100%" Style="direction:rtl" class="trans">
        <asp:TableHeaderRow>
          <asp:TableHeaderCell>הרכב</asp:TableHeaderCell>
          <asp:TableHeaderCell>א</asp:TableHeaderCell>
          <asp:TableHeaderCell>ב</asp:TableHeaderCell>
          <asp:TableHeaderCell>ג</asp:TableHeaderCell>
          <asp:TableHeaderCell>ד</asp:TableHeaderCell>
          <asp:TableHeaderCell>ה</asp:TableHeaderCell>
          <asp:TableHeaderCell>ו</asp:TableHeaderCell>
          <asp:TableHeaderCell>ש</asp:TableHeaderCell>
        </asp:TableHeaderRow>
      </asp:Table>
    </div>

    <!-- Old details-->
    <div id="old" class="formAdjusted" style="width:17%; margin: 53px 0% 0% 0; float:right; direction:rtl; font-size: initial; padding: 4%;">
      <h1 style="text-decoration: underline;"> פרטים ישנים</h1>
      <div id="old-brutto">
        <label>ברוטו</label>
        <asp:Label ID="lbOldBrutto" runat="server"></asp:Label>
      </div>
      <div id="old-netto">
        <label>נטו</label>
        <asp:Label ID="lbOldNetto" runat="server"></asp:Label>
      </div>
      <div id="old-subsid">
        <label>סובסיד</label>
        <asp:Label ID="lbOldSubsid" runat="server"></asp:Label>
      </div>
      <div id="old-brutto-vat">
        <label>מע"מ על הברוטו</label>
        <asp:Label ID="lbOldVatBrutto" runat="server"></asp:Label>
      </div>
      <div id="old-commision-value">
        <label>נטו עמלת ספק</label>
        <asp:Label ID="lbOldCommisionValue" runat="server"></asp:Label>
      </div>
      <div id="old-commision-vat">
        <label>מע''מ על עמלה</label>
        <asp:Label ID="lbOldCommisionVat" runat="server"></asp:Label>
      </div>
      <div id="old-toclerck">
        <label>רווח נטו</label>
        <asp:Label ID="lbOldToClerk" runat="server"></asp:Label>
      </div>
    </div>
    <br />
    <!-- New details -->
    <div id="New" class="formAdjusted" style="width:17%; margin: 53px 2% 0% 0; float:right; direction:rtl; font-size: initial; padding:4%;">
      <h1 style="text-decoration: underline;"> פרטים חדשים</h1>
      <div id="new-brutto">
        <label>ברוטו</label>
        <asp:Label ID="lbNewBrutto" runat="server"></asp:Label>
      </div>
      <div id="new-netto">
        <label>נטו</label>
        <asp:Label ID="lbNewNetto" runat="server"></asp:Label>
      </div>
      <div id="new-subsid">
        <label>סובסיד</label>
        <asp:Label ID="lbNewSubsid" runat="server"></asp:Label>
      </div>
      <div id="new-brutto-vat">
        <label>מע"מ על הברוטו</label>
        <asp:Label ID="lbNewVatBrutto" runat="server"></asp:Label>
      </div>
      <div id="new-commision-value">
        <label>נטו עמלת ספק</label>
        <asp:Label ID="lbNewCommisionValue" runat="server"></asp:Label>
      </div>
      <div id="new-commision-vat">
        <label>מע''מ על עמלה</label>
        <asp:Label ID="lbNewCommisionVat" runat="server"></asp:Label>
      </div>
      <div id="new-toclerck">
        <label>רווח נטו</label>
        <asp:Label ID="lbNewToClerk" runat="server"></asp:Label>
      </div>
    </div>
    <!-- .folders -->


    <asp:Button ID="btGetNextBundle" runat="server" OnClick="btGetNextBundle_Click" Text="עבור לתיק הבא" CssClass="button-submit" 
      style="float:right; margin-top:6px; margin-right:10px; width:99%;"/>
    <asp:Button ID="btChangeBundleDetails" runat="server" OnClick="btChangeBundleDetails_Click" Text="שנה נתונים" CssClass="button-submit" 
      style="float:right; margin-top:6px; margin-right:10px; width:99%;"/>

    <asp:Button ID="btChangeAll" runat="server" OnClick="btChangeAll_Click" Text="שנה הכל" CssClass="button-submit" 
      style="float:right; margin-top:6px; margin-right:10px; width:99%;"/>

    </div>
  </div>
</div>

<div runat="server" visible="false" id="afterChangeAllFiles" style="text-align:center; font-size:20px;">
    
    <label>קבצים עם מספרי תיקים</label>
    <br />
    <br />
    <asp:LinkButton text="מחירונים כפולים" ID="btMultiPricesTxt" runat="server" Style="color: white; padding: 10px 15px; background-color: #0072ff; border: solid 3px; border-color: black; cursor:pointer"></asp:LinkButton>
    <asp:LinkButton text="אין מחירון" ID="btNoPriceFoundTxt" runat="server" Style="color: white; padding: 10px 15px; background-color: #0072ff; border: solid 3px; border-color: black; cursor:pointer"></asp:LinkButton>
    <asp:LinkButton text="תשלום נוסף" ID="btExtraPaytxt" runat="server" Style="color: white; padding: 10px 15px; background-color: #0072ff; border: solid 3px; border-color: black; cursor:pointer"></asp:LinkButton>
    <asp:LinkButton text="הוחלפו" ID="btDocketsChangedTxt" runat="server" Style="color: white; padding: 10px 15px; background-color: #0072ff; border: solid 3px; border-color: black; cursor:pointer"></asp:LinkButton>

</div>

</asp:Content>

