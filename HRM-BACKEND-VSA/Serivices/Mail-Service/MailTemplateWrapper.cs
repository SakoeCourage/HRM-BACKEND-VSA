namespace HRM_BACKEND_VSA.Serivices.Mail_Service
{
    public static class MailTemplateWrapper
    {
        public static string wrappMailBody(string body)
        {
            return @$"<!doctype html>
    <html lang=""en"">
  <head>
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <meta http-equiv=""Content-Type"" content=""text/html; charset=UTF-8"">
    <title>Simple Transactional Email</title>
    <style media=""all"" type=""text/css"">
           {TemplateStyle.styles}
      </style>
  </head>
  <body>
    <table role=""presentation"" border=""0"" cellpadding=""0"" cellspacing=""0"" class=""body"">
      <tr>
        <td>&nbsp;</td>
        <td class=""container"">
          <div class=""content"">

            <!-- START CENTERED WHITE CONTAINER -->
      
            <table role=""presentation"" border=""0"" cellpadding=""0"" cellspacing=""0"" class=""main"">

              <!-- START MAIN CONTENT AREA -->
              <tr>
                <td class=""wrapper"">
                    {body}
                </td>
              </tr>

              <!-- END MAIN CONTENT AREA -->
              </table>

            <!-- START FOOTER -->
            <div class=""footer"">
              <table role=""presentation"" border=""0"" cellpadding=""0"" cellspacing=""0"">
                <tr>
                  <td class=""content-block powered-by"">
                    @{DateTime.Now.Year} HRM-SYSTEM.  All Rights reserved
                  </td>
                </tr>
              </table>
            </div>

            <!-- END FOOTER -->
            
<!-- END CENTERED WHITE CONTAINER --></div>
        </td>
        <td>&nbsp;</td>
      </tr>
    </table>
  </body>
</html>";
        }



    }
}
