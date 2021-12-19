<%@ Page Title="" Language="C#" MasterPageFile="~/MainMasterPage.master" AutoEventWireup="true" CodeFile="ConvertFile.aspx.cs" Inherits="ConvertFile" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
<script type="text/jscript">


    $(document).ready(function () {

        $('[id*="UploadBtn"]').click(this, function () {
            $('[id*="lblError"]').text("");
            var drp = $('[id*="DropDownList1"]').val();
            if (drp == 0) {
                $('[id*="lblError"]').text(" חייב לבחור סוג קובץ ");
                return false;
            }
            return true;
        });

        $('[id*="DropDownList1"]').change(function () {
            if ($(this).val() == 1)
                $('#trApkey').show();
            else
                $('#trApkey').hide();
        });

        $('[id*="FileUpLoad1"]').click(this, function () {
            $('[id*="lblError"]').text("");
            return true;
        });
    });
      
    
     
    </script>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">
<h2 dir="rtl">המרת קובץ </h2>
<center>
        
        <table dir="rtl" class="MainTable">
            
            
            <tr>
                <th>בחר סוג קובץ :</th> 
                <td>
                    <asp:DropDownList ID="DropDownList1" runat="server">
                        <asp:ListItem Value="0">בחר...</asp:ListItem>
                        <asp:ListItem Value="1">קובץ מימושים</asp:ListItem>
                        <asp:ListItem Value="2">קובץ חשבוניות</asp:ListItem>
                        <asp:ListItem Value="3">קובץ חיווי למימושים</asp:ListItem>
                    </asp:DropDownList>
                </td>
            </tr>
            <tr id="trApkey" style="display:none;">
                <th><asp:Label ID="LApkey" for="Apkey" runat="server" Text="הכנס מפתח אפליקטיבי :"></asp:Label></th>
                <td><asp:TextBox  ID="Apkey" runat="server" ></asp:TextBox></td> 
            </tr>
            <tr>
                <td><asp:FileUpload ID="FileUpLoad1" runat="server" /></td>
                <td><asp:Button id="UploadBtn" Text=" המרת הקובץ" OnCommand="UploadBtn_Click"  runat="server" Width="105px" /></td>
            </tr>
            <tr>
                <td colspan="2" > <asp:Label ID="lblError" runat="server" Text=""></asp:Label></td> 
            </tr>
        </table>
        <p>
        </p> 
    </center>
</asp:Content>

