﻿<%@ Page Title="" Language="C#" MasterPageFile="~/MainMasterPage.master" AutoEventWireup="true" CodeFile="Info.aspx.cs" Inherits="Info" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="Server">
  <% Response.WriteFile("menu.html"); %>
  <div class="box-col width-content">
    <h3>מידע כללי אודות המכרז</h3>
    לכל זכאי אגף השיקום של משרד הביטחון שלום רב!
      <br />
    אנו מבקשים להביא בפניכם מספר הגדרות ותנאים כללים לביצוע ההזמנות שיקלו עליכם את ביצוע התהליך מול מרכז ההזמנות:
      <br />
    <ul>
      <li>
        מחזור הבראה, השהייה בבתי המלון הינה מספר ימים רצופים, החל מיום אחד ועד 28 יום (בכפוף להסכם), איש איש בהתאם לזכאותו. שהיה רצופה של למעלה מחמישה ימים רצוי שתחל בימי א' או ד' בשבוע. פיצול ימי ההבראה אפשרי אך ורק בהתאם לתנאי הזכאות האישיים של כל אחד מהזכאים.
      </li>
      <li>
        במידה ,ובהתאם לתנאי זכאותו של הזכאי, הוא נדרש לשלם תוספת בגין שהייתו בבית המלון, ישלם הזכאי לקל נופש את התוספת מיד עם אישור הזמנתו. המחירים של אמצע שבוע שונים ממחירי סוף השבוע וכמו כן משתנים מחודש לחודש ולכן יש שוני בסכומים מהזמנה להזמנה ובין מלון למלון. את התוספת ניתן לשלם באחת מהאפשרויות הבאות:
א- העברה בנקאית שתבוצע תוך שני ימי עבודה מאישור ההזמנה ובתנאי שנשארו עוד 14 יום עד היציאה להבראה.
ב- תשלום בכרטיס אשראי

      </li>
      <li>
        במידה וזכאי מבקש לממש את זכאותו להבראה בתקופת חגי ישראל, יידרש הזכאי לשלם על חשבונו תוספת בעבור החג אותה ישלם הזכאי לקל נופש מיד עם אישור הזמנתו. לפי סעיף ב, בכל חודש המחיר שונה ולכן גם מחיר החגים אינו זהה והסכום הסופי יימסר לזכאי בעת ביצוע הזמנתו.
      </li>
      <li>
        תקופת זכאות. השנה תקופת מימוש הזכאות השנתית תחל ב 01/03/2015 ותסתיים ב 29/02/2016. כל שנת זכאות תחל ב 01/03 ותסתיים ב 28/02 בשנה שלאחר מכן.
      </li>
      <li>
        חמי מרפא- לזכאים אשר מגיע להם ימי טיפול, במידה ובמלון המוזמן ישנם חמי מרפא, יוכל הזכאי לממש את הטיפולים בחמי המרפא של המלון. במידה ובמלון המוזמן אין חמי מרפא, יוצעו לזכאי חמי מרפא בסביבה הקרובה למלון לצורך מימוש הטיפולים (למעט באזורים ת"א, אילת, גליל עליון וגליל מערבי).
      </li>
      <li>
        הטיפולים ב"חמי מרפא" כוללים : בריכת מי גופרית/אמבטיות גופרית, חבישות/אמבטיות בוץ וטיפולי עיסוי, שלושה טיפולים, 30דקות כל טיפול (עיסוי/בוץ), אחד מכל סוג (למעט נכים קשים הרשאים לקבל טיפול עיסוי 60 דקות וזאת במקום טיפול בוץ 30 דקות), כל יום בהתאם לזכאותו. על הזכאי למלא ולחתום על הצהרת בריאות שתישלח אליו יחד עם השובר אותו הוא חייב להציג בחמי המרפא ולחתום על כל יום טיפול. קל נופש מנפיקה את השובר ללקוח בעת ביצוע ההזמנה וחשוב לשים לב שאחריות ביצוע הטיפולים הינה על הזכאי בלבד וחובה עליו לתאם את הטיפולים כמה ימים לפחות לפני ההגעה למלון.
      </li>
      <li>
        בסיס האירוח: בתי המלון רשאים לספק ארוחות לפי ראות עינהם ואין הכרח לספק את כל הארוחות בחדר האוכל. 
      </li>
    </ul>
    <h3>הקצאת חדרים</h3>
    <ul>
      <li>החברה מתחייבת להקצות לכל חודש מחודשי השנה חדרים בבתי המלון למימוש מחזורי ההבראה וזאת בהתאם להיקף המינימאלי של בנק החדרים בכל בית מלון כמפורט בהסכם מול אגף השיקום. היה ובחודש כלשהו מכסת החדרים התמלאה, רשאית החברה לנתב הזמנות לחודש שלאחר מכן.</li>
      <li>קל נופש לא מחוייבת לספק הקצאה בכל יום במהלך החודש ולעיתים ההקצאות מבתי המלון נפרסות על גבי כמה ימים ספורים מתוך כל ימי החודש.</li>
      <li>מכסת החדרים בכל חודש תחולק בין בתי המלון ע"י החברה, וזאת על פי ההסכם של אגף השיקום.</li>
      <li>על פי תנאי ההסכם חברת קל נופש אינה רשאית לחרוג ממכסות החדרים שנקבעו לה ללא אישור בכתב מנציג משרד הביטחון/אגף השיקום.</li>
      <li>החברה מחוייבת לשחרר את הקצאת החדרים למלונות 14 יום טרם תחילת מועד אירוח. הזמנות בפחות מ 14 יום לפני מועד תחילת הנופש,יבוצעו אך ורק באישור החברה ועל בסיס מקום פנוי במלון.</li>
      <li>
        במידה ובית מלון הכלול ברשימה הזוכה עובר שיפוצים וקיימת הקצאת חדרים לגביו, תיידע החברה את המשרד והזכאים על כך, 
    הן בחוברות המידע כמפורט לעיל והן בעת הזמנת ההבראה, ותקבל את הסכמתו המפורשת של הזכאי שעודכן על כך לביצוע ההזמנה. החברה תציע לזכאי מלון חלופי באותה קטגוריה.
      </li>
      <li>
        החברה תאפשר לזכאים המעוניינים בכך, להאריך את מחזור השהייה מעבר לזכאותם ועל חשבונם, בתנאים 
    ובמחירים מיוחדים וזה יבוצע מול מוקד השיקום. במידה והזכאי רוצה לצרף ילדים ומשפחה במקביל להזמנתו אך הדבר לא ניתן בחדר הסטנדרטי שהזכאי מקבל, 
    ניתן לבצע זאת מול מוקד ההזמנות הפרטי  בו הזכאי ייהנה ממחירים מיוחדים לנכי צה"ל.
     בכל מקרה, כל הזמנה שתבוצע על ידי הזכאי באופן פרטי תתנהל לפי נוהל דמי ביטול של חוק הגנת הצרכן ולא לפי הנהוג במכרז של אגף השיקום.
      </li>
    </ul>
    על מנת לייעל את השירות, למנוע לחץ ולאפשר לזכאים להגדיל את הסיכויים לקבל את המלון המבוקש על ידם, מקבל המוקד ההזמנות, בכל זמן נתון, הזמנות ליציאות עד 180 יום קדימה מיום פנייתכם למוקד! זכאי בעלי נכות 100+ רשאים להזמין שנה מראש.
    <h3>דרכים ליצירת קשר עם מרכז הזמנות</h3>
    <ul>
      <li>
       מרכז ההזמנות יפעל כל השנה (למעט בחגים) בימים א' עד ה'- בין השעות 9:00 – 17:00 ובימי שישי וערבי חג- בין השעות 8:00 ועד 13:00, בלבד. 
טלפון  להזמנות: 03-5685550, או 1-800-850-800. 
        <br />
* הזכאים מתבקשים שלא להתקשר לפני או אחרי השעות הנ"ל.
מספרי פקס לשליחת פניות: 035426200 
        <br />
ניתן גם לשלוח בקשות במייל לכתובת : SHIKUM@KAL.CO.IL בשליחת פנייה כתובה בפקס או במייל, אנא לציין בבירור : שם מלא, מספר טלפון ליצירת קשר, תאריך בקשה למימוש ההבראה, מלון מבוקש ועוד 2-3 מלונות חלופיים, מס' תעודת זהות של הזכאי.
לזכאי השיקום הקמנו אתר אינטרנט מיוחד בו תוכלו לראות את מצב זכאותכם, לצפות ברשימת המלונות, ולבצע הזמנה און ליין. כתובת האתר :
www.kal-shikum.co.il.
        </li>
      <li>
        אישור או דחיית הזמנה ימסרו תוך 24 שעות מקבלת הבקשה,  בטלפון ובפקס למזמין ולנציג המשרד.
      </li>
      <li>
 מרכז ההזמנות מורשה לטפל אך ורק בזכאים אשר רשומים במאגר הזכאים המועבר ליידי החברה ע"י אגף השיקום.
      </li>
      <li>
 זכאי משהב"ט יפנה את הזמנתו ישירות לחברה . החברה רשאית לאשר או לא לאשר את ההזמנה עד 24 שעות מרגע קבלתה. היה ואושרה ההזמנה, החברה תנפיק לזכאי שובר בתוך 48 שעות מרגע אישור ההזמנה. 
      </li>
      <li>
 בתחילתו של מועד הנופש, ימסור הזכאי את שובר ההזמנה לבית המלון ו/או לחמי המרפא, 
    לאחר שנחתם על ידו (ועל ידו בלבד). באחריות הזכאי לוודא כי בסיום מועד הנופש ייחתם שובר המלון ושובר הטיפולים ע"י נציג מוסמך מטעם המלון/ חמי המרפא. 
     </li>
      <li>
 מימוש ההבראה של הזכאי יעשה על ידו בלבד וכנגד הצגת תעודת נכה במלון ע"י בעל התעודה בלבד. 
    החברה תממש לזכאי את הזמנתו אך ורק על פי תנאי זכאותו. במידה ויזמין הזכאי הבראה שחורגת את תנאי זכאותו, אחריותו לשלם על החריגה בעצמו.  
    </li>
    </ul>
    <h3>ביטול ההזמנה</h3>
    <p>
      במקרה של ביטול אירוח עד 8 ימים לפני תחילת האירוח הזכאי לא יחויב בדמי ביטול ויקבל החזר כספי (במידה ושילם)
במקרה של ביטול אירוח פחות מ 8 ימים לפני תחילת האירוח או באי הגעה למלון, וכל זאת ללא אישור רופא המאשר את הביטול עקב מצב רפואי, יחויב הזכאי ביום זכאות מימי הזכאות השנתית לו זכאי (ההפחתה תבוצע מידית על ידי קל נופש ותחזור חזרה לזכאי רק לאחר אישור לקל נופש מאגף השיקום), וכמו כן במידה ובוצע תשלום בגין ההזמנה הזכאי יחויב בחיוב כספי על הלילה הראשון ששולם.
אנו פונים לזכאים בבקשה לנסות ולהמעיט ככל הניתן בשינויים ו/או ביטולים של הזמנות שלא מכורח סיבה רפואית. שינויים אלו גורמים ללחץ על המוקד, ומקשים את הטיפול בכל מי שמבקש לבצע הזמנות חדשות !!!                            
לא ניתן לבצע הזמנות לבתי מלון שאינם מופיעים ברשימה המלונות אשר משתתפים במכרז.  
      <br />
      כמות ההקצאות החודשית בכל מלון מוגבלת וזאת על פי הנחיות אגף השיקום !
      <br />
      היה ובמלון כלשהו או בקטגוריה כלשהי נסתיימה ההקצאה של אותו החודש, תינתן לזכאי אפשרות לממש את זכאותו במלון אחר או בקטגוריה אחרת או באותו מלון אך בחודש אחר. 
חשוב לציין כי הקצאת החדרים לזכאים מבוצעת על ידי חברת קל נופש בהתאם לתנאי ההסכם ואין קשר למצב תפוסת המלון הרלוונטי בתאריך כלשהו.
    </p>
  </div>
</asp:Content>

