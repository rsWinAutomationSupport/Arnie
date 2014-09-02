using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.ServiceModel;
using System.Text;
namespace Arnie
{
    [DataContract]
    public class GitHubPushWebhook
    {
        public object this[string i]
        {
            get
            {
                PropertyInfo property = this.GetType().GetProperties().FirstOrDefault(prop => prop.Name == i);
                if (property == null)
                {
                    return null;
                }
                return property.GetValue(this);
            }
        }

        [DataMember(Name = "ref")]
        public String ref_ { get; set; }

        [DataMember(Name = "after")]
        public String after { get; set; }

        [DataMember(Name = "before")]
        public String LastName { get; set; }

        [DataMember(Name = "created")]
        public Boolean created { get; set; }

        [DataMember(Name = "deleted")]
        public Boolean deleted { get; set; }

        [DataMember(Name = "forced")]
        public Boolean forced { get; set; }

        [DataMember(Name = "commits")]
        public IEnumerable<Commit> commits { get; set; }

        [DataMember(Name = "head_commit")]
        public Commit head_commit { get; set; }

        [DataMember(Name = "repository")]
        public Repository repository { get; set; }

        [DataMember(Name = "pusher")]
        public GithubUser pusher { get; set; }

        public override string ToString()
        {
            MemoryStream stream = new MemoryStream();
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(GitHubPushWebhook));
            serializer.WriteObject(stream, this);
            string jsonString = Encoding.UTF8.GetString(stream.ToArray());
            stream.Close();
            return jsonString;
        }
    }

    [DataContract(Name = "Commit")]
    public class Commit
    {

        [DataMember(Name = "id")]
        public String id { get; set; }

        [DataMember(Name = "distinct")]
        public Boolean distinct { get; set; }

        [DataMember(Name = "message")]
        public String message { get; set; }

        [IgnoreDataMember]
        public DateTime timestamp { get; set; }

        [DataMember(Name = "timestamp")]
        private String timestampString
        {
            get { return this.timestamp.ToString("yyyy-MM-ddTHH:mm:sszzz"); }
            set 
            {
                try
                {
                    this.timestamp = DateTime.ParseExact(value, "yyyy-MM-ddTHH:mm:sszzz", null);
                }
                catch (Exception exc)
                {
                    if (exc is ArgumentNullException || exc is FormatException)
                    {
                        Utility.LogException(exc);
                        this.timestamp = new DateTime(0);
                        return;
                    }
                    throw;
                }
            }
        }

        [DataMember(Name = "url")]
        public String url { get; set; }

        [DataMember(Name = "author")]
        public GithubUser author { get; set; }

        [DataMember(Name = "committer")]
        public GithubUser committer { get; set; }

        [DataMember(Name = "added")]
        public IEnumerable<String> added { get; set; }

        [DataMember(Name = "removed")]
        public IEnumerable<String> removed { get; set; }

        [DataMember(Name = "modified")]
        public IEnumerable<String> modified { get; set; }
    }

    [DataContract(Name = "repository")]
    public class Repository
    {
        public object this[string i]
        {
            get
            {
                PropertyInfo property = this.GetType().GetProperties().FirstOrDefault(prop => prop.Name == i);
                if (property == null)
                {
                    return null;
                }
                return property.GetValue(this);
            }
        }

        [DataMember(Name = "id")]
        public long id { get; set; }

        [DataMember(Name = "name")]
        public string name { get; set; }

        [DataMember(Name = "full_name")]
        public string full_name { get; set; }

        [DataMember(Name = "owner")]
        public GithubUser owner { get; set; }

        [DataMember(Name = "private")]
        public bool private_ { get; set; }

        [DataMember(Name = "html_url")]
        public string html_url { get; set; }

        [DataMember(Name = "description")]
        public string description { get; set; }

        [DataMember(Name = "fork")]
        public bool fork { get; set; }

        [DataMember(Name = "url")]
        public string url { get; set; }

        [DataMember(Name = "forks_url")]
        public string forks_url { get; set; }
    }

    [DataContract(Name = "GithubUser")]
    public class GithubUser
    {

        [DataMember(Name = "name")]
        public String name { get; set; }

        [DataMember(Name = "email")]
        public String email { get; set; }

        [DataMember(Name = "username", EmitDefaultValue = false)]
        public String username { get; set; }
    }
}