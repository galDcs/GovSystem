<%@ Page Language="C#" MasterPageFile="~/MainMasterPage.master" AutoEventWireup="true" EnableEventValidation="false" CodeFile="Default.aspx.cs" Inherits="_Default" Title="Search page" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
    <!-- header content here (like js includes and css) -->
    <script type="text/javascript" language="javascript">

        var hidSelectedErkevClientID = '<%= hidSelectedErkev.ClientID%>';
        var hidSelectedFourOneSevenClientID = '<%= hidSelectedFourOneSeven.ClientID%>';
        
        //var dependedItemsSKU = '027241,027242';
        
        function ToggleCheckboxes(chkSelected)
        {
           
            //uncheck repeaters chkboxes
            $('[id*="rptMakatChkBx"]').each( function() {
                this.checked = false;
             });
            chkSelected.checked = true;
            
            $($get("<%= btnRefreshErkevPanel.ClientID %>")).click();
        }
        
        function ValidateSelection()
        {
            var checkedCount = 0;
            $('[id*="rptMakatChkBx"]').each( function() {
                if(this.checked == true) { checkedCount++; }
            });
            if(checkedCount < 1) {
                alert("נא לבחור מק''ט");
                return false;
            }
           
            var four_one_seven = $("#erkevTable").find('input:radio:[name="four_one_seven"]');
            if (four_one_seven.length > 0) {
                var four_one_seven_Selected = $("#erkevTable").find('input:radio:[name="four_one_seven"]:checked');
                if ($("#erkevTable").find('input:radio[name="four_one_seven"]:checked').length <= 0) {
                    //if ($("#erkevTable").find('input:radio[data]').length <= 0)
                    //    alert("נא לבחור  7+7");
                    //else
					if (!($("#erkevTable").find('input:radio[data]').length <= 0))
                        alert("נא לבחור אחד מתוך 4+1");
                    return false;
                } else {
                    $("#" + hidSelectedFourOneSevenClientID).val(four_one_seven_Selected[0].value);
                    //return true;
                }
            }
            var erkev = $("#erkevTable").find('input:radio[name="erkev"]:checked');
            if(erkev.length<=0) {
                alert("נא לבחור הרכב");
                return false;
            } else {
                $("#"+hidSelectedErkevClientID).val(erkev[0].value);
                return true;
            }
        }

        $(document).ready(function(){
            $('#'+'<%=txtTravellerId.ClientID%>').focus();
        });

    </script>
<style>
.transHead .trans th{
	width:  285px;
}
.transHead input, .transHead select {
    width: 285px;
}
input, select,button {
    width: 340px;
}
input, select, .box-col .button-submit {
    border-radius: 5px;
}
.form{
	font-size:16px;
}
</style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">
<script>
function trim(elem){
	elem.value = elem.value.trim();
}
</script>
	<div>
        <div class="box-col" style="width:95%">
		<u><h1 dir="rtl">ביצוע הזמנה</h1></u>
            <h2 dir="rtl">פרטי חיפוש:</h2>
            <table class="trans transHead" width="80%" dir="rtl">
				<tr>
					<th>מספר תעודת זהות</th>
					<th>מספר תיק מלא (9 ספרות)</th>
					<th>מספר תיק</th>
				</tr>
				<tr>
                    <td><asp:TextBox runat="server" ID="txtTravellerId"  placeholder="תעודת זהות" onkeydown="trim(this);"/> 
					<br/>
					<asp:CompareValidator runat="server" Display = "Dynamic" Operator="DataTypeCheck" Type="Integer"
							ControlToValidate="txtTravellerId" ErrorMessage="ניתן להכניס רק מספרים" />
					<asp:RegularExpressionValidator Display = "Dynamic" ControlToValidate = "txtTravellerId" runat="server" 
							ID="RegularExpressionValidator1" ValidationExpression = "^[\s\S]{9,9}$" 
							ErrorMessage="נא להכניס 9 ספרות של מספר תעודת זהות"></asp:RegularExpressionValidator>
	    
					</td><!-- 023718307 -->
                    <td><asp:TextBox runat="server" ID="txtDocketId" MaxLength="9" placeholder="תיק מלא" />
						<asp:CompareValidator runat="server" Display = "Dynamic" Operator="DataTypeCheck" Type="Integer" 
						ControlToValidate="txtDocketId" ErrorMessage="ניתן להכניס רק מספרים" />
						<br/>		
						<asp:RegularExpressionValidator Display = "Dynamic" ControlToValidate = "txtDocketId" runat="server" 
						ValidationExpression = "^[\s\S]{9,9}$" ErrorMessage="נא להכניס 9 ספרות של מספר תיק"></asp:RegularExpressionValidator>
					</td>
					<td>
						<asp:TextBox runat="server" ID="txtDocketIdPartial" MaxLength="6" placeholder="תיק"/>
						<asp:CompareValidator runat="server" Display = "Dynamic" Operator="DataTypeCheck" Type="Integer" ControlToValidate="txtDocketIdPartial" ErrorMessage="ניתן להכניס רק מספרים" />
						<asp:RegularExpressionValidator Display = "Dynamic" ControlToValidate = "txtDocketIdPartial" runat="server" ValidationExpression = "^[\s\S]{6,6}$" ErrorMessage="נא להכניס 6 ספרות של מספר תיק"></asp:RegularExpressionValidator>
					</td>
					</tr>
					<tr>
                    <td colspan="4"><asp:Button ID="Button12" class="button-submit" runat="server" Text="חפש" OnClick="SeachOnClick" /></td>
                </tr>
                <tr runat="server" id="messageContainer">
                    <td colspan="6"><asp:Literal runat="server" ID="Message" /></td>
                </tr>
            </table>
		<br/>
		<table dir="rtl" class="trans"><!--@EY-->
			<tr>
			<th>שנת זכאות</th>
			</tr>
			<tr>
				
				<td><asp:DropDownList ID="ddlEntitledYear" runat="server" AutoPostBack="true" OnSelectedIndexChanged="SeachOnClick"></asp:DropDownList></td>
			</tr>
		</table>
        <br/>
        <div runat="server" id="divResult">
            <h2 dir="rtl">תוצאות חיפוש:</h2>
            <table dir="rtl" class="trans">
                <tr>
                    <th>מס' נכה:</th>
                    <td><asp:Label runat="server" MaxLength="9"  ID="tBoxDocketID"  ></asp:Label></td>
                    <th>שם פרטי:</th>
                    <td><asp:Label runat="server"  ID="tBoxFirstName"></asp:Label></td>
                    <th>שם משפחה:</th>
                    <td><asp:Label runat="server"  ID="tBoxSecondName"></asp:Label></td>
                    <th>ת.ז.:</th>
                    <td><asp:Label runat="server" MaxLength="9" ID="tBoxTravellerID"  ></asp:Label></td>
                </tr>
                <tr>
                    <th>כתובת:</th>
                    <td><asp:Label runat="server"  ID="tBoxAddress"></asp:Label></td>
                    <th>ישוב:</th>
                    <td><asp:Label runat="server"  ID="tBoxCity"></asp:Label></td>
                    <th>מיקוד:</th>
                    <td><asp:Label runat="server" MaxLength="5" ID="tBoxZipCode"  ></asp:Label></td>
					<th>נכות 100%:</th>
                    <td><asp:Label runat="server" MaxLength="5" ID="tBoxSimon"  ></asp:Label></td>
                    <th style='display:none;'>מס' תיק</th>
                    <td style='display:none;'><asp:TextBox runat="server" Enabled="false"  ID="txtMisparTik"  onkeypress="return validateInputNumberRunTime(event, this);"></asp:TextBox></td>
                </tr>
            </table>
            <br />
            
            <asp:Repeater runat="server" ID="rpt" >
                <HeaderTemplate></HeaderTemplate>
                <ItemTemplate>
                    <table dir="rtl" style="border:1px; border-color:Black;" class="trans" width="100%">  
                        <tr>
                            <th></th>
                            <th>מק''ט</th>
                            <th>תיאור מק''ט</th>
                            <th>ת. תחילת זכאות</th>
                            <th>ת. סיום זכאות</th>
                            <th>ת. מהדורה</th>
                            <th>מלווה</th>
                            <th>מס' ימי זכאות</th>
                            <th>ימי מימוש</th>
                            <th>יתרה</th>
                        </tr>
                        <tr class="<%# ((Boolean)Eval("IsActive") == true) ? "":"redBackground" %>">
                            <td>
                                <asp:CheckBox ID="rptMakatChkBx" runat="server" onclick="ToggleCheckboxes(this)" CssClass="rptChk" Visible='<%# Eval("IsActive") %>' />
                            </td>
                            <td>
                                <asp:Label ToolTip='<%#Eval("IsActive")%>' ID="LabelMakat" runat="server" Text='<%# Eval("ItemSKU") %>'></asp:Label>
                            </td>
                             <td>
                               <asp:Label ID="LabelMakatDesc" runat="server" Text='<%# Eval("ItemDescription") %>'></asp:Label>
                            </td>

                             <td  dir="ltr">
                                  <asp:Label ID="LabelStartDate" runat="server" Text='<%# ((DateTime)Eval("StartDate")).ToString("dd-MMM-yyyy") %>' ></asp:Label>
                             </td>
                             <td  dir="ltr">
                                 <asp:Label ID="LabelEndDate" runat="server" Text='<%# ((DateTime) Eval("EndDate")).ToString("dd-MMM-yyyy")  %>'></asp:Label>
                             </td>
                             <td  dir="ltr">
                                 <asp:Label ID="LabelReleaseDate" runat="server" Text='<%# ((DateTime) Eval("ReleaseDate")).ToString("dd-MMM-yyyy")  %>'></asp:Label>
                             </td>
                            <td>
                                 <asp:Label ID="LabelEscortNum" runat="server" Text='<%# Eval("EscortNum") %>'></asp:Label>
                             </td>
                             <td>
                                 <asp:Label ID="LabelDaysNum" runat="server" Text='<%# Eval("DaysNum") %>'></asp:Label>
                            </td>
                             <td>
                                  <asp:Label ID="LabelUsageBalance" runat="server" Text='<%# Eval("UsageBalance") %>'></asp:Label>
                             </td>
                            <td>
                              <asp:Label ID="LabelDaysRemained" runat="server"
                                Text='<%#CalculateBalance(Eval("ItemSKU").ToString(), Eval("Request_SH").ToString())%>'>
                              </asp:Label>
                             </td>
                        </tr>
                        <tr align="right">
                            <th colspan="2">הערת משרד:</th>
                            <td colspan="8">
                                <asp:Label ID="LabelOfficeComment" runat="server" Text='<%# Eval("OfficeComment") %>'></asp:Label>
                            </td>
                        </tr>
                    </table>
                </ItemTemplate>
            </asp:Repeater>
        </div>
		
        <asp:ScriptManager ID="ScriptManager1" runat="server">
        </asp:ScriptManager>

        <asp:UpdatePanel ID="panelErkev" runat="server">
            <ContentTemplate>
                <div runat="server" id="divErkev" style="width:100%;" dir="rtl"></div>
            </ContentTemplate>
            <Triggers>
                <asp:AsyncPostBackTrigger  ControlID="btnRefreshErkevPanel" EventName="Click"/>
            </Triggers>
        </asp:UpdatePanel>
        <br/>
        <asp:HiddenField ID="hidSelectedFourOneSeven" runat="server" Value=""/>
        <asp:HiddenField ID="hidSelectedErkev" runat="server" Value=""/>
        <asp:Button runat="server" ID="btnContinue" OnClientClick="if(!ValidateSelection()) return false;" OnClick="btnContinue_Click" Text="המשך" Visible="false" cssClass="button-submit"/>
        <asp:Button runat="server" ID="btnRefreshErkevPanel" OnClick="RefreshErkevPanel_Click" Text="" style="display:none" />
	</div>
  </div>
  <div style="font-size:20px; color:red;  text-align:center; display:none;">
    <b>Agency לא ניתן לבצע הזמנות עד שהתקלה במשרד הביטחון תסתדר. בברכה </b>
  </div>
</asp:Content>
