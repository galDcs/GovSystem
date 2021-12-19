<%@ Page Language="C#" MasterPageFile="~/MainMasterPage.master" AutoEventWireup="true" CodeFile="AddNewTraveller.aspx.cs" Inherits="AddNewTraveller" Title="Add new traveller" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">

 	<script type="text/javascript">
 	    $(document).ready(function(){
		    $('.grid_date_style').datepicker(  );
			 $('#details input[type=text]').attr("disabled",true);

	    });
	</script>
	<style>
	input[type="text"]:disabled {
		background: #f5f5f5;
		text-align:center;
	}
	 .trans tr:nth-child(odd):hover {
		   background-color: #eaeaea;
	 }
	 	 .trans tr:nth-child(even):hover {
		   background-color: #DCDCDC;
	 }
	</style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">

  
  <h2 dir="rtl">פרטי זכאי</h2>
  
<center> 
  
    <div style="border-width:thin; border-style:solid">
        <h2 dir="rtl">פרטי חיפוש:</h2>
        <table class="trans" width="80%" dir="rtl" >
            <tr>
                <th>מס' ת.ז.:</th>
                <td><asp:TextBox runat="server" ID="txtTravellerId" Text=""/> </td><!-- 023718307 -->
                <th>מס' נכה:</th>
                <td><asp:TextBox runat="server" ID="txtDocketId"  /> </td>
				<tr >
					<td colspan="5"><asp:Button ID="Button12" runat="server" Text="חפש" OnClick="SeachOnClick"  style="background-color: #89bdd3;color:white;cursor: pointer;" /></td>
				</tr>
            </tr>
            <tr><td colspan="6"><asp:Button ID="Button1" runat="server" Text="הוספת חדש" OnClick="SeachOnClick" CommandName="addnew" Visible ="false"/></td></tr>
            <tr runat="server" id="messageContainer">
                <td colspan="6"><asp:Literal runat="server" ID="Message" /></td>
            </tr>
        </table>
        <br/>
    </div>
    
    <div runat="server" id="divResult" dir="rtl">
    <asp:Repeater runat="server" ID="rpt">
    <HeaderTemplate></HeaderTemplate>
    <ItemTemplate>
        <table dir="rtl" class="trans" id="details" style="border-style:outset;border-width: 1px">
            <tr><th colspan="8" align="center"><b>פרטי נוסע</b></th></tr>
            <tr>
                <th>מספר תיק:</th>
                <td><asp:TextBox runat="server" MaxLength="9"  ID="tBoxDocketID" onkeypress="return validateInputNumberRunTime(event, this);" Text='<%# Eval("DocketId") %>'></asp:TextBox></td>
                <th>ת.ז.:</th>
                <td><asp:TextBox runat="server" MaxLength="9" ID="tBoxTravellerID" onkeypress="return validateInputNumberRunTime(event, this);" Text='<%# Eval("TravellerID") %>'></asp:TextBox></td>
            </tr>
            <tr>
                <th>שם פרטי:</th>
                <td><asp:TextBox runat="server"  ID="tBoxFirstName" Text='<%# Eval("FirstName") %>'></asp:TextBox></td>
                <th>שם משפחה:</th>
                <td><asp:TextBox runat="server"  ID="tBoxSecondName" Text='<%# Eval("SecondName") %>'></asp:TextBox></td>
                
            </tr>
            <tr>
                <th>כתובת:</th>
                <td><asp:TextBox runat="server"  ID="tBoxAddress" Text='<%# Eval("Address") %>'></asp:TextBox></td>
                <th>קוד ישוב:</th>
                <td><asp:TextBox runat="server" MaxLength="4" ID="tBoxCityCode" onkeypress="return validateInputNumberRunTime(event, this);" Text='<%# Eval("CityCode") %>'></asp:TextBox></td>
            </tr>
			<tr>
                <th>מיקוד:</th>
                <td><asp:TextBox runat="server" MaxLength="7" ID="tBoxZipCode" onkeypress="return validateInputNumberRunTime(event, this);" Text='<%# Eval("ZipCode") %>'></asp:TextBox></td>
				<th>ישוב:</th>
                <td><asp:TextBox runat="server"  ID="tBoxCity" Text='<%# Eval("City") %>'></asp:TextBox></td>
            </tr>
            <tr>
                <th>מק"ט:</th>
                <td><asp:TextBox runat="server" MaxLength="6"  ID="tBoxItemSKU" Text='<%# Eval("ItemSKU") %>'></asp:TextBox></td>
                <th>תאור מק"ט:</th>
                <td><asp:TextBox runat="server"  ID="tBoxItemDesc" Text='<%# Eval("ItemDescription") %>'></asp:TextBox></td>
            </tr>

            <tr>
                <th>לשכה:</th>
                <td><asp:TextBox runat="server" MaxLength="2" ID="tBoxDepartment" onkeypress="return validateInputNumberRunTime(event, this);" Text='<%# Eval("Department") %>'></asp:TextBox></td>
                <th>רמה:</th>
                <td><asp:TextBox runat="server" MaxLength="2" ID="tBoxLevel" onkeypress="return validateInputNumberRunTime(event, this);" Text='<%# Eval("Level") %>'></asp:TextBox></td>
                
            </tr>
            <tr>
                <th>ניצול:</th>
                <td><asp:TextBox runat="server" MaxLength="3" ID="tBoxUsageBalance" onkeypress="return validateInputNumberRunTime(event, this);" Text='<%# Eval("UsageBalance") %>'></asp:TextBox></td>
                <th>ניצול ירושלים:</th>
                <td><asp:TextBox runat="server" MaxLength="3" ID="tBoxJerusalemUsageBalance" onkeypress="return validateInputNumberRunTime(event, this);" Text='<%# Eval("JerusalemUsageBalance") %>'></asp:TextBox></td>
                
            </tr>
            <tr>
                <th>מספר טלפון 1:</th>
                <td><asp:TextBox runat="server" MaxLength="10" ID="txbPhoneNumber1" onkeypress="" Text='<%# Eval("Tel_Pr_1")+"-"+Eval("Tel_Num_1")  %>'></asp:TextBox></td>
				<th>מספר טלפון 2:</th>
                <td><asp:TextBox runat="server" MaxLength="10" ID="txbPhoneNumber2" onkeypress="" Text='<%# Eval("Tel_Pr_2") +"-"+ Eval("Tel_Num_2") %>'></asp:TextBox></td>
            </tr>
            <tr>
                <th>מספר טלפון 3:</th>
                <td><asp:TextBox runat="server" MaxLength="10" ID="txbPhoneNumber3" onkeypress="" Text='<%# Eval("Tel_Pr_3") +"-"+ Eval("Tel_Num_3") %>'></asp:TextBox></td>
                <th>הערות משרד:</th>
                <td>
                    <asp:TextBox runat="server" MaxLength="50" ID="txbOfficeComments" onkeypress=""  Text = '<%# Eval("OfficeComment") %>'></asp:TextBox>
                </td>
            </tr>
            <tr>
			<th>פעיל</th>
                <td><asp:TextBox runat="server" ID="chIsActive" Text='<%# getStatus(Eval("IsActive"))%>' ></asp:TextBox></td>
				<th>מק"ט 40</th>
                <td><asp:TextBox runat="server" MaxLength="1" ID="tMakat40" onkeypress="return validateInputNumberRunTime(event, this);" Text='<%# Eval("Makat40") %>'></asp:TextBox></td>
            </tr>      
			<tr>
                <th>תאריך התחלה:</th>
                <td><asp:TextBox runat="server"  ID="tBoxStartDate" Text='<%# FormatDate(Eval("StartDate")) %>' CssClass="grid_date_style"></asp:TextBox></td>
                <th>תאריך סיום:</th>
                <td><asp:TextBox runat="server"  ID="tBoxEndDate" Text='<%# FormatDate(Eval("EndDate")) %>' CssClass="grid_date_style"></asp:TextBox></td>
                
            </tr>
            <tr>
                <th>מספר ימי זכאות:</th>
                <td><asp:TextBox runat="server" MaxLength="2" ID="tBoxDaysNum" onkeypress="return validateInputNumberRunTime(event, this);" Text='<%# Eval("DaysNum") %>'></asp:TextBox></td>
                <th>מספר מלווים:</th>
                <td><asp:TextBox runat="server" MaxLength="1" ID="tBoxEscortNum" onkeypress="return validateInputNumberRunTime(event, this);" Text='<%# Eval("EscortNum") %>'></asp:TextBox></td>
                
            </tr>
			<tr>
			<th>סטטוס:</th>
                <td><asp:TextBox runat="server" MaxLength="1" ID="tBoxStatus" onkeypress="return validateInputNumberRunTime(event, this);" Text='<%# Eval("Status") %>'></asp:TextBox></td>
				<th>תאריך מהדורה</th>
                <td><asp:TextBox runat="server"  ID="tReleaseDate" Text='<%# FormatDate(Eval("ReleaseDate")) %>' CssClass="grid_date_style"></asp:TextBox></td>
			</tr>
        </table>

        </ItemTemplate>
        <SeparatorTemplate><br /></SeparatorTemplate>
        <FooterTemplate></FooterTemplate>
        </asp:Repeater>
        <asp:Button runat="server" ID="btnSave" OnClick="SaveBtnClick" Text="שמור" Visible="false"/>
    </div>
    
    <p>
        <asp:Label ID="lblError" runat="server" Text=""></asp:Label>
    </p>
</center>
</asp:Content>

