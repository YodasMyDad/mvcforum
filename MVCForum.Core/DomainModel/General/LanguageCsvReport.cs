using System.Collections.Generic;
using System.Linq;

namespace MVCForum.Domain.DomainModel
{
    public static class CsvExtensionMethods
    {
        public static List<string> ExtractMessages(this  List<CsvErrorWarning> errorWarnings)
        {
            return errorWarnings.Select(errorWarning => errorWarning.Message).ToList();
        }
    }

    public enum CsvErrorWarningType
    {
        GeneralError,
        BadDataFormat,
        MissingLanguageName,
        DoesNotExist,
        AlreadyExists,
        ItemBad,
        MissingKeyOrValue,
        NewKeyCreated,
    }

    public class CsvErrorWarning
    {
        public string Message { get; set; }
        public CsvErrorWarningType ErrorWarningType { get; set; }
    }

    /// <summary>
    /// A report generated after a CSV import
    /// </summary>
    public class CsvReport
    {
        public List<CsvErrorWarning> Errors { get; set; }
        public List<CsvErrorWarning> Warnings { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public CsvReport()
        {
            Errors = new List<CsvErrorWarning>();
            Warnings = new List<CsvErrorWarning>();
        }
    }
}
