namespace ProjectCollaborationPlatforn.Security.Services.Autentication
{
    public class VerifiedMessage
    {
        public static string SuccessMessage = @"<!DOCTYPE html>
                <html lang=""uk"">
                <head>
                  <meta charset=""UTF-8"">
                  <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                  <title>Confirmation email</title>
                </head>
                <body>
                  <h1>Your email address was succesfully confrirmed!</h1>
                  <p>Congratulations! You succesfully confirmed your email.</p>
                  <p>Now You can use our service</p>
                  <p>Thanks a lot!</p>
                </body>
                </html>";

        public static string FailureMessage = @"<!DOCTYPE html>
                <html lang=""uk"">
                <head>
                  <meta charset=""UTF-8"">
                  <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                  <title>Confirmation email</title>
                </head>
                <body>
                  <h1>Unfortunately, email confirmation did not happen!</h1>
                  <p>We could not verify your email address..</p>
                  <p>Try again later.</p>
                </body>
                </html>";
    }
}
