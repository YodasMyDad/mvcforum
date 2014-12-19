using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;

namespace MVCForum.OpenAuth.GooglePlus
{
    [DataContract]
    public class GooglePlusModel
    {
        private static readonly DataContractJsonSerializer jsonSerializer = new DataContractJsonSerializer(typeof(GooglePlusModel));

        [DataMember(Name = "id")]
        public string Id { get; set; }

        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "first_name")]
        public string FirstName { get; set; }

        [DataMember(Name = "last_name")]
        public string LastName { get; set; }

        [DataMember(Name = "link")]
        public Uri Link { get; set; }

        [DataMember(Name = "birthday")]
        public string Birthday { get; set; }

        [DataMember(Name = "email")]
        public string Email { get; set; }

        public static GooglePlusModel Deserialize(string json)
        {
            if (string.IsNullOrEmpty(json))
            {
                throw new ArgumentNullException("json");
            }
            return Deserialize(new MemoryStream(Encoding.UTF8.GetBytes(json)));
        }

        public static GooglePlusModel Deserialize(Stream jsonStream)
        {
            if (jsonStream == null)
            {
                throw new ArgumentNullException("jsonStream");
            }
            return (GooglePlusModel)jsonSerializer.ReadObject(jsonStream);
        }
    }
}
