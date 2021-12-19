<%@ Page Title="" Language="C#" MasterPageFile="~/MainMasterPage.master" AutoEventWireup="true" CodeFile="Error.aspx.cs" Inherits="Error" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">
  <Style>
  body{
      float: right;
    width: 100%;
	}
  </style>

  </div>
  <div class="box-col width-content" style="width:95%;max-width: 95%;">
    <h1><asp:Label ID="lbMsgError" runat="server"></asp:Label></h1>
    <h2>
      </h2>
  </div>
</asp:Content>

