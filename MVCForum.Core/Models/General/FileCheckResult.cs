namespace MvcForum.Core.Models.General
{
    public class FileCheckResult
    {
        public bool IsOk { get; set; }
        public string FileName { get; set; }
        public string FileExtension { get; set; }
        public string Message { get; set; }
        public bool IsImage { get; set; }
    }
}