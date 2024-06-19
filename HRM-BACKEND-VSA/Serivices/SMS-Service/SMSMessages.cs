namespace HRM_BACKEND_VSA.Services.SMS_Service
{
    public class SMSMessages
    {

        public static string OTPMessage(string opt, int timeoutMins)
        {
            return $"Your otp is {opt}. Valid for only {timeoutMins}mins";
        }


        public static string generateUserOnboardingMessage(string user)
        {
            return @$"Hello {user},

                Your account has been created successfully.

                Before you get started, we need to verify your email address to ensure the security of your account. Please click on the link below to verify your email:
                
                Verify Email

                If the link doesn't work, please copy and paste the URL into your web browser.

                Thank you for joining us. If you have any questions or need assistance, feel free to reach out to our support team.
    ";
        }
    }
}
