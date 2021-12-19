<%@ Page Language="C#" AutoEventWireup="true" CodeFile="MakatDetails.aspx.cs" Inherits="MakatDetails" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Makat Details</title>
    <link href="App_Themes/Theme1/StyleSheet.css" rel="stylesheet" type="text/css" />
	   <link href="css/style-page.css?ver=1.5" rel="stylesheet" />
	   <style>
	   textarea{
	   }
	   input, select {
			width: 50px;
		}
		input {
    border-radius: 0;
    padding: 2px;
    text-align: center;
    width: 100%;
    height: auto;
    box-shadow: none;
}
	   </style>
    <script src="Scripts/jquery-1.7.1.min.js" type="text/javascript"></script>
    <script type="text/javascript">
    var makatsArray = new Array();
    <%=makats.ToString()%>
    
    function saveData()
    {
        var isDataSavedSuccessfully = true;
        for(i=0;i<makatsArray.length;i++)
        {
            var dataString = "";
            var currentMakat = makatsArray[i];
            dataString = "&MakatNumber="+currentMakat;
            dataString+="&MakatDescription="+document.getElementById(currentMakat+"_MakatDescription").value;
            var GeneralAreas = "";
            var GATags = document.getElementsByTagName("input"); 
            
            for(j = 0; j < GATags.length; j++) 
            {   
                if(GATags[j].id.indexOf(currentMakat+"_GA")!=-1)
                {
                    if(GATags[j].checked==true)
                    {
                        if(GeneralAreas!="")
                        {
                            GeneralAreas+=",";
                        }
                    
                        GeneralAreas+=GATags[j].id.substring(GATags[j].id.indexOf("GA_")+3);
                    }
                }
            }
            dataString+="&GeneralAreas="+GeneralAreas;
            dataString+="&MinNights="+document.getElementById(currentMakat+"_MinNights").value;
            dataString+="&MaxNights="+document.getElementById(currentMakat+"_MaxNights").value;
            var OneTimeUssage = (document.getElementById(currentMakat+"_OneTimeUssage").checked==true)?"1":"0";
            dataString+="&OneTimeUssage="+OneTimeUssage;
            var MakatTipulim = (document.getElementById(currentMakat+"_MakatTipulim").checked==true)?"1":"0";
            dataString+="&MakatTipulim="+MakatTipulim;
            var Allow5And5Nights = (document.getElementById(currentMakat+"_Allow5And5Nights").checked==true)?"1":"0";
            dataString+="&Allow5And5Nights="+Allow5And5Nights;
            dataString+="&OfficeRemarkForOrder="+document.getElementById(currentMakat+"_OfficeRemarkForOrder").value;
            dataString+="&StartOrderDateFromTodayMin="+document.getElementById(currentMakat+"_StartOrderDateFromTodayMin").value;
            dataString+="&StartOrderDateFromTodayMax="+document.getElementById(currentMakat+"_StartOrderDateFromTodayMax").value;
            var AllowedToAdd5NightForPay = (document.getElementById(currentMakat+"_AllowedToAdd5NightForPay").checked==true)?"1":"0";
            dataString+="&AllowedToAdd5NightForPay="+AllowedToAdd5NightForPay;
            dataString+="&VoucherRemark="+document.getElementById(currentMakat+"_VoucherRemark").value;
            
            $.ajax({
              type: "POST",
              url: "MakatDetails.aspx",
              data: "action=1"+dataString
            }).done(function( msg ) {
              if(msg=='0')
              {
                isDataSavedSuccessfully = false;
              }
            });
        }
        if(isDataSavedSuccessfully==true)
        {
            alert(".הנתונים נשמרו בהצלחה")
        }
        else
        {
            alert(".אירעה שגיאה. הנתונים לא נשמרו")
        }
        
    }
    </script>
</head>
<body dir="rtl" >
<div class="box-col" style="width:95%; overflow-x:scroll;">
 <h2 dir="rtl">הגדרת מק"טים</h2>
    <%=mainTable.ToString() %>
    <%--<form id="form1" runat="server">
    
    
    </div>
    </form>--%>
</body>
</html>
