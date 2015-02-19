using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace Arnie
{
    [DataContract]
    public class Repos
    {
        [DataMember(Name = "repos")]
        public IEnumerable<RepositoryConfig> repos { get; set; }
    }

    [DataContract]
    public class RepositoryConfig
    {
        [DataMember(Name = "commands")]
        public IEnumerable<Commands> commands { get; set; }

        [DataMember(Name = "repo_key")]
        public string repo_key { get; set; }

        [DataMember(Name = "repo_value")]
        public string repo_value { get; set; }

        [DataMember(Name = "ref")]
        public string ref_ { get; set; }
    }

    [DataContract]
    public class Commands : IComparable<Commands>
    {

        [DataMember(Name = "order")]
        public int order { get; set; }

        [DataMember(Name = "command")]
        public string command { get; set; }

        public int CompareTo(Commands that)
        {
            if (this.order > that.order) return 1;
            if (this.order == that.order) return 0;
            return -1;
        }
    }
}