<%@ Page Title="" Language="C#" MasterPageFile="~/MainMasterPage.master" AutoEventWireup="true" CodeFile="Login.aspx.cs" Inherits="Login" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="Server">
  <div class="maincontainer">
    <div class="content">
      <div class="box-col width-sidebar" style="margin: 0;">
        <h1>כניסה למערכת שיקום
        </h1>
        <label for="tbTravellerID">
          תעודת זהות
        </label>
        <br />
        <asp:TextBox ID="tbTravellerID" runat="server" MaxLength="9" placeholder="הכנס/י תעודת זהות" TabIndex="1"></asp:TextBox>
        <label for="tbDocketID">
          מספר תיק
        </label>
        <br />
        <asp:TextBox ID="tbDocketID" runat="server" MaxLength="9" placeholder="הכנס/י מספר תיק" TabIndex="2"></asp:TextBox>
        <asp:Label ID="lbMessage" runat="server"></asp:Label>
        <asp:Button ID="btSubmit" runat="server" Text="כניסה" CssClass="button-submit" OnClick="btSubmit_Click" />
      </div>
      <div class="box-col width-content">
        <p>
          שלום רב,
        </p>

        <p>
          ברוכים הבאים לאתר להזמנת הבראה וטיפולים לזכאי אגף השיקום של משרד הבטחון בניהול חברת קל נופש.
          <br />  <br />  <br />
לכניסה למערכת נא הקש פרטים אישיים בחלון הימני     (9 ספרות תעודת זהות + 9 ספרות תיק נכה)
אם לא ידוע לך מספר תיק הנכה המלא ניתן לשלוח מייל לכתובת shikum@kal.co.il ותקבל תשובה בהתאם
        </p>
        <hr />
        <p>
          הודעה חשובה!
אנו ממליצים לבצע הזמנות חצי שנה מראש,
וזאת כדי להגדיל את הסיכויים להשיג מקום במלון ובתאריך המבוקש על ידכם
        </p>
      </div>


    </div>
  </div>
</asp:Content>

