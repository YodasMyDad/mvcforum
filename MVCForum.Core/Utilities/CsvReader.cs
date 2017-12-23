using System;
using System.Collections.Generic;
using System.IO;

namespace MVCForum.Utilities
{
    public class CsvLine
    {
        /// <summary>
        /// The values in this CSV line
        /// </summary>
        public List<string> Values { get; set; }    

        /// <summary>
        /// HIde empty constructor
        /// </summary>
        private CsvLine()
        {
            
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="values">Line values (words)</param>
        public CsvLine(List<string> values)
        {
            Values = values;
        }

        /// <summary>
        /// Retrieve a value by columnIndex (get this from the CsvReader class [] indexer)
        /// </summary>
        /// <param name="columnIndex"></param>
        /// <returns></returns>
        public string this[int columnIndex]
        {
            get
            {
                if (Values == null)
                {
                    return null;
                }

                return Values[columnIndex];
            }
        }
    }

    /// <summary>
    /// Reads a CSV file and parses the data according to column header names
    /// </summary>
    public class CsvReader 
    {
        private List<CsvLine> _rows = new List<CsvLine>();
        private Dictionary<string, int> _columnNames = new Dictionary<string, int>();
 
        /// <summary>
        /// Constructor
        /// </summary>
        public CsvReader()
        {
        }

        /// <summary>
        /// Process a set of CSV lines
        /// </summary>
        /// <param name="lines"></param>
        private void ReadCsv(List<string> lines)
        {
            var separator = new char[] { ',' };
            
            // First row is column headers
            string columnHeaderLine = lines[0];
            string[] fileColHeaders = columnHeaderLine.Trim().Split(separator);
            _columnNames.Clear();

            int columnPosition = 0;

            // Store the ordinal position of each column
            foreach (var columnHeader in fileColHeaders)
            {
                _columnNames.Add(columnHeader, columnPosition++);
            }

            // Extract all the data
            for (int lineIndex = 1 /* Skip first */; lineIndex < lines.Count; lineIndex++)
            {
                if (string.IsNullOrEmpty(lines[lineIndex]))
                {
                    continue;
                }

                var values = new List<string>(lines[lineIndex].Trim().Split(separator));
                CsvLine nextLine = new CsvLine(values);
                _rows.Add(nextLine);
            }
            
        }

        /// <summary>
        /// Load a stream
        /// </summary>
        /// <param name="stream"></param>
        public void Load(Stream stream)
        {
            var allLines = new List<string>();

            using (var sr = new StreamReader(stream, System.Text.Encoding.UTF8, true))
            {
                while (sr.Peek() >= 0)
                {
                    allLines.Add(sr.ReadLine());
                }
            }

            ReadCsv(allLines);
        }

        /// <summary>
        /// Load a file
        /// </summary>
        /// <param name="filePath"></param>
        public void Load(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new ApplicationException(string.Format("CSV file does not exist: {0}", filePath));
            }

            string[] lines = File.ReadAllLines(filePath);

            if (lines.Length == 0)
            {
                throw new ApplicationException("CSV file is empty.");    
            }

            var linesLst = new List<string>(lines);

            ReadCsv(linesLst);
        }

        /// <summary>
        /// Access a column header to find its ordinal value
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public int this[string key]
        {
          get
          {
              if (_columnNames.ContainsKey(key))
              {
                  return _columnNames[key];
              }

              throw new KeyNotFoundException();
          }
        }

        /// <summary>
        /// All lines in the CSV
        /// </summary>
        public List<CsvLine> Lines
        {
            get { return _rows; }
        }
    }

}
