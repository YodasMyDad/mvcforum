using System.Text;
using System.Web;
using System.Web.Mvc;

namespace MVCForum.Website.Application
{
    public class CsvFileResult : FileResult
    {
        public CsvFileResult()
            : base("text/csv")
        {
        }

        protected override void WriteFile(HttpResponseBase response)
        {
            if (!string.IsNullOrEmpty(Body))
            {
                // Write result to output-stream
                var byteArray = Encoding.Default.GetBytes(Body);
                response.OutputStream.Write(byteArray, 0, byteArray.GetLength(0));
            }
        }

        public string Body { get; set; }
    }
}