using Library.Core;

namespace Library.Model.External
{
    public class CompanyGroupEmp : BaseModel
    {
        #region Scalar Properties

        public string Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
        public string MailingUserName { get; set; }
        public string MailingPassword { get; set; }
        public bool IsSSL { get; set; }
        public string LogoFileName { get; set; }
        public string DocumentFolderName { get; set; }
        public string UserId { get; set; }
        public string Password { get; set; }

        #endregion Scalar Properties
    }
}