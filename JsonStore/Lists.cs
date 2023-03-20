using System.Collections.Generic;
using Newtonsoft.Json;

namespace JsonStore
{
    /// <summary>
    /// List of SIMPL digital, analog and string values
    /// </summary>
    public class Lists
    {
        /// <summary>
        /// Current file ID
        /// </summary>
        public string FileID { get; set; }

        /// <summary>
        /// List of stored strings
        /// </summary>
        public List<string> strings { get; set; }

        /// <summary>
        /// List of stored integers
        /// </summary>
        public List<int> integers { get; set; }

        /// <summary>
        /// List of stored bools
        /// </summary>
        public List<bool> bools { get; set; }

        /// <summary>
        /// Returns object value as a string
        /// </summary>
        /// <returns>Object value as a string</returns>
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}