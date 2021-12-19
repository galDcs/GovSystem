<%@ Page Title="" Language="C#" MasterPageFile="~/MainMasterPage.master" AutoEventWireup="true"
    CodeFile="ManageStatus.aspx.cs" Inherits="ManageStatus" EnableEventValidation="false" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="Server">
    <script>


        $(document).ready(function () {
            //$('.MainTable').css('direction','rtl');
            $('[id*="tBoxID"]').change(this, function () {
                $('#<%=LError.ClientID%>').text("");
                if ($.isNumeric($('[id*="tBoxID"]').val())) return true;
                else {
                    $('#<%=LError.ClientID%>').text("מספרים בלבד ! ");
                    return false;
                }
            });
        });

        

       
    </script>
	<Style>
		input{
			height: auto;
		}
	</style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="Server">
    
    <h2 dir="rtl">ניהול סטטוס</h2>
    <asp:GridView ID="GridView1" runat="server" CssClass="MainTable" AutoGenerateColumns="false"
        ShowFooter="true">
        <Columns>
            <asp:TemplateField HeaderText="קוד">
                <ItemTemplate>
                    <center>
                        <%# Eval("id") %></center>
                </ItemTemplate>
                <FooterTemplate>
                    <center>
                        <asp:TextBox runat="server" ID="tBoxID" /></center>
                </FooterTemplate>
            </asp:TemplateField>
            <asp:TemplateField HeaderText="שם">
                <ItemTemplate>
                        <%# Eval("Name")%>
                </ItemTemplate>
                <FooterTemplate>
                    <center>
                        <asp:TextBox runat="server" ID="tBoxName" /></center>
                </FooterTemplate>
            </asp:TemplateField>
            <asp:TemplateField HeaderText="סטטוס">
                <ItemTemplate>
                    <center>
                        <asp:CheckBox runat="server" Text="פעיל" ID="chIsActive1" data='<%# Eval("id") %>'
                            Checked='<%# Eval("Status")%>' AutoPostBack="true" OnCheckedChanged="OnCheckedChanged_Status" /></center>
                </ItemTemplate>
                <FooterTemplate>
                    <center>
                        <asp:CheckBox runat="server" ID="chIsActive" Text="פעיל" Checked="true" /></center>
                </FooterTemplate>
            </asp:TemplateField>
            <asp:TemplateField>
                <ItemTemplate>
                </ItemTemplate>
                <FooterTemplate>
                    <center>
                        <asp:Button ID="LBAddRow" runat="server" CommandName="Footer" OnClick="Addrow" Text="הוסף">
                        </asp:Button></center>
                </FooterTemplate>
            </asp:TemplateField>
        </Columns>
    </asp:GridView>
    <asp:Panel ID="Panelempty" Visible="false" runat="server">
        <table dir="rtl" class="MainTable">
            <tr>
                <th>
                    id :
                </th>
                <th>
                    שם :
                </th>
                <th>
                    סטטוס
                </th>
                <th>
                </th>
            </tr>
            <tr>
                <td align="center">
                    <asp:TextBox runat="server" ID="tBoxID"></asp:TextBox>
                </td>
                <td align="right">
                    <asp:TextBox runat="server" ID="tBoxName"></asp:TextBox>
                </td>
                <td align="center">
                    <asp:CheckBox runat="server" ID="chIsActive" Checked="false" />
                </td>
                <td align="center">
                    <asp:Button ID="LBAddRow" runat="server" CommandName="EmptyDataTemplate" OnClick="Addfirstrow"
                        Text="Add"></asp:Button>
                </td>
            </tr>
        </table>
    </asp:Panel>
    <asp:Label ID="LError" runat="server"></asp:Label>
</asp:Content>
