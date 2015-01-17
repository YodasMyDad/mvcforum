using System.IO;
using System.Web;

namespace MVCForum.Website.Application
{
    internal class MemoryFile : HttpPostedFileBase
    {
        readonly Stream _stream;
        readonly string _contentType;
        readonly string _fileName;

        public MemoryFile(Stream stream, string contentType, string fileName)
        {
            _stream = stream;
            _contentType = contentType;
            _fileName = fileName;
        }

        public override int ContentLength
        {
            get { return (int)_stream.Length; }
        }

        public override string ContentType
        {
            get { return _contentType; }
        }

        public override string FileName
        {
            get { return _fileName; }
        }

        public override Stream InputStream
        {
            get { return _stream; }
        }

        public override void SaveAs(string filename)
        {
            using (var file = File.Open(filename, FileMode.CreateNew))
            {
                _stream.CopyTo(file);
            }
        }

    }
}
