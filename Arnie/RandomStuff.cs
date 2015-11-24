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
    public class RandomStuff
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

        [DataMember(Name = "test")]
        public String test { get; set; }

    
    }
}