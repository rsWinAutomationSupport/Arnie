using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.ServiceModel;
using System.Reflection;

namespace Arnie
{
    [DataContract]
    public class ClientRegistration
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

        [DataMember(Name = "ConfigID", IsRequired = true)]
        public String configID { get; set; }

        [DataMember(Name = "ClientDSCCert", IsRequired = true)]
        public String clientDSCCert { get; set; }

        [DataMember(Name = "ClientConfig", IsRequired = true)]
        public String clientConfig { get; set; }

        [DataMember(Name = "MetaData", IsRequired = false)]
        public String metaData { get; set; }

    }

    //[DataContract(Name = "embedded")]
    //public class class1
    //{
    //    [DataMember(Name = "value1")]
    //    public string value1 { get; set; }
    //}
}