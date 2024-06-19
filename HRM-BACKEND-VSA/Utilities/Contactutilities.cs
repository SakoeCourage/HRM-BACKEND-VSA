using OfficeOpenXml;

namespace HRM_BACKEND_VSA.Utilities
{
    static class Contactutilities
    {

        public class NewSMSContactDTO
        {
            public string firstName { get; set; }
            public string lastName { get; set; }
            public string contact { get; set; }
            public string email { get; set; }
        }
        internal static List<NewSMSContactDTO> ExtractDataFromExcel(ExcelPackage package)
        {
            List<NewSMSContactDTO> contacts = new List<NewSMSContactDTO>();

            var worksheet = package.Workbook.Worksheets[0];
            var rowCount = worksheet.Dimension.Rows - 1;


            for (int row = 3; row <= rowCount; row++)
            {
                var contact = new NewSMSContactDTO
                {
                    firstName = worksheet.Cells[row, 1].Value?.ToString(),
                    lastName = worksheet.Cells[row, 2].Value?.ToString(),
                    contact = worksheet.Cells[row, 3].Value?.ToString(),
                    email = worksheet.Cells[row, 4].Value?.ToString()
                };
                contacts.Add(contact);
            }

            return contacts;
        }


        internal static List<NewSMSContactDTO> GetContactsFromFile(IFormFile rfile)
        {
            try
            {
                var file = rfile;
                if (file != null && file.Length > 0)
                {
                    if (Path.GetExtension(file.FileName).ToLower() != ".xlsx")
                    {
                        throw new Exception("Invalid file. Please use the provided template");
                    }
                    var package = new ExcelPackage(file.OpenReadStream());
                    var worksheet = package.Workbook.Worksheets[0];
                    var firstName = worksheet.Cells[2, 1].Value?.ToString();
                    var lastName = worksheet.Cells[2, 2].Value?.ToString();
                    var contact = worksheet.Cells[2, 3].Value?.ToString();
                    var email = worksheet.Cells[2, 4].Value?.ToString();
                    if (firstName != "First Name" && lastName != "Last Name" && contact != "Contact" && email != "Email")
                    {
                        throw new Exception("Invalid file. Please use the provided template");
                    }
                    var contacts = ExtractDataFromExcel(package);
                    return contacts;
                }

                throw new Exception("File not provided or is empty.");
            }
            catch (Exception ex)
            {
                throw new Exception($"Error: {ex.Message}");
            }
        }
    }
}
