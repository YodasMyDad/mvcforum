namespace MVCForum.Domain.DomainModel
{
    public class UploadFileResult
    {
        public bool UploadSuccessful { get; set; }
        public string ErrorMessage { get; set; }
        public string UploadedFileName { get; set; }
        public string UploadedFileUrl { get; set; }
    }
}
