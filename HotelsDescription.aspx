<%@ Page Title="" Language="C#" MasterPageFile="~/MainMasterPage.master" AutoEventWireup="true" CodeFile="HotelsDescription.aspx.cs" Inherits="Default2" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">
      <% Response.WriteFile("menu.html"); %>
  <div class="box-col width-content">
  <h1>
    <asp:Label ID="hotelName" runat="server"></asp:Label>
  </h1>
  <asp:Label ID="hotelDescription" runat="server"></asp:Label>
    </div>
</asp:Content>

