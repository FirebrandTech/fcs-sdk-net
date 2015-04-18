<%@ Page Title="Log in" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="WebFormsTest.Account.Login" Async="true" %>

<asp:Content runat="server" ID="BodyContent" ContentPlaceHolderID="MainContent">
    <div class="row" style="margin-top: 100px; margin-bottom: 100px">
        <div class="col-md-4 col-md-offset-4">
            <div data-fc-login="mode:credentials"></div>                
        </div>
    </div>
</asp:Content>
