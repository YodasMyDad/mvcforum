namespace MvcForum.Core
{
    using System.IO;
    using System.Web;

    public class MemoryFile : HttpPostedFileBase
    {
        private readonly Stream _stream;

        public MemoryFile(Stream stream, string contentType, string fileName)
        {
            _stream = stream;
            ContentType = contentType;
            FileName = fileName;
        }

        public override int ContentLength => (int) _stream.Length;

        public override string ContentType { get; }

        public override string FileName { get; }

        public override Stream InputStream => _stream;

        public override void SaveAs(string filename)
        {
            using (var file = File.Open(filename, FileMode.CreateNew))
            {
                _stream.CopyTo(file);
            }
        }
    }
}