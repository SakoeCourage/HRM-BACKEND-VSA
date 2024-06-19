namespace HRM_BACKEND_VSA.Contracts
{
    public class EmailDTO
    {
        public string? ToName { get; set; }
        public string ToEmail { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
    }
    public static class EmailContracts
    {

        public record StaffPostingRecord(
            string firstName,
            string lastName,
            string staffType,
            string staffId,
            string unitName,
            string departmentName,
            string directorateName,
            DateOnly notionalDate
            );

        public static string generateStaffPostingEmailBodyTemplate(StaffPostingRecord postingdetail)
        {

            return @$"
                <p>Dear {postingdetail.firstName} {postingdetail.lastName} </p>
					<br/>
                  <p>
                  Congratulations  on your appointment as {postingdetail.staffType} staff at Korle-Bu teaching hospital. We are thrilled to have you join our team.
                  </p>
                    <p>
                         Below are the details of your posting:
                    </p>
                  <nav>
                    
                    <b>Directorate  &nbsp;: </b> {postingdetail.directorateName} <br>
                    <b>Department : </b> {postingdetail.departmentName} <br>
                    <b> Unit &nbsp;:</b>   {postingdetail.unitName}<br>
                    <b> Effective Date:</b>   {postingdetail.notionalDate}<br>
                   </nav>
                      <br/>
                      <br/>
                     <p>
                       To help you get started, We have assigned to you a staff Id as <b>{postingdetail.staffId}</b>,and also here are your credentials for accessing the 							staff portal, where you can find important resources to 						complete the onboarding process
                     </p>
                    <nav>
                     <b>Staff Portal Url  &nbsp;: </b> <a href='https://hrm-staff.vercel.app/login'>https://hrm-staff.vercel.app/login</a>							<br>
                    <b>Username &nbsp;&nbsp;: </b> {postingdetail.staffId} <br>
                    <b>Temporary password :</b> {postingdetail.firstName}
                    </nav>
                ";
        }
    }

}
