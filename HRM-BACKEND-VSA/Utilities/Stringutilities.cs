using System.Security.Cryptography;

namespace HRM_BACKEND_VSA.Utilities
{
    public class Stringutilities
    {
        static readonly int MAX_STAFF_ID_LENGTH = 7;
        static readonly string newRegisterationLink = "https://hrm-staff.vercel.app/register";

        public static string GenerateRandomOtp()
        {
            Random random = new Random();
            // Generate a random 6-digit number
            int otpNumber = random.Next(1000, 9999);
            // Convert the number to a string with leading zeros if necessary
            string otp = otpNumber.ToString("D4");

            return otp;
        }

        public static string GeneraterandomStaffNumbers()
        {
            byte[] randomNumberBytes = GenerateRandomBytes(MAX_STAFF_ID_LENGTH);
            string randomNumberString = ConvertBytesToHexString(randomNumberBytes).Substring(0, MAX_STAFF_ID_LENGTH);
            long randomNumber = long.Parse(randomNumberString, System.Globalization.NumberStyles.HexNumber);
            return $"MS{randomNumber.ToString()}";
        }

        public static string GenerateStaffNumberPerRegistratonCriteria(string initial, int lastCount)
        {
            var currentDate = DateTime.UtcNow;
            string currentDateInitial = currentDate.ToString("yy");
            string paddedCount = lastCount.ToString("D6");

            var staffInitial = $"{initial}{paddedCount}{currentDateInitial}";

            return staffInitial;
        }

        public static byte[] GenerateRandomBytes(int length)
        {
            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
            {
                byte[] rndNumbers = new byte[length];
                rng.GetBytes(rndNumbers);
                return rndNumbers;
            }
        }

        public static string ConvertBytesToHexString(byte[] bytes)
        {
            return BitConverter.ToString(bytes).Replace("-", "");
        }


        public static string generateNewRecruitMessageTemplate(string templateMessage, string newRecruitName)
        {
            return templateMessage.Replace("[Name]", newRecruitName)
                .Replace("[Registration Link]", newRegisterationLink);
        }
    }
}
