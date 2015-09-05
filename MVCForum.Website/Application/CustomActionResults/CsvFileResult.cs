using System.Linq;
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
                var data = Encoding.UTF8.GetBytes(Body);
                var result = Encoding.UTF8.GetPreamble().Concat(data).ToArray();
                response.OutputStream.Write(result, 0, result.GetLength(0));
            }
        }

        public string Body { get; set; }
    }
}