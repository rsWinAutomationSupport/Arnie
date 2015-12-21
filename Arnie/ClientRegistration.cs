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

        [DataMember(Name = "uuid", IsRequired = true)]
        public String uuid { get; set; }

        [DataMember(Name = "publicCert", IsRequired = true)]
        public String publicCert { get; set; }

        [DataMember(Name = "dsc_config", IsRequired = true)]
        public String dsc_config { get; set; }

    }

    //[DataContract(Name = "embedded")]
    //public class class1
    //{
    //    [DataMember(Name = "value1")]
    //    public string value1 { get; set; }
    //}
}