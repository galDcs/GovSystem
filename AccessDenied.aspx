<%@ Page Language="C#" AutoEventWireup="true" CodeFile="AccessDenied.aspx.cs" Inherits="AccessDenied" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Access Denied</title>
</head>
<body>
    <form id="form1" runat="server">
    <center>
        <br /><br /><br /><br />
        <div runat="server" id="messageContainer" style="text-align:center; font-weight:bold" >
            <asp:Literal runat="server" ID="Message" />
        </div>
    </center>
    </form>
</body>
</html>
