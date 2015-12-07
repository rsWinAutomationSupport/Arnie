using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

namespace Arnie
{
    [ServiceContract]
    public interface IRestService
    {

        [OperationContract]
        [WebInvoke(Method = "POST",
                   RequestFormat = WebMessageFormat.Json,
                   ResponseFormat = WebMessageFormat.Json,
                   BodyStyle = WebMessageBodyStyle.Bare,
                   UriTemplate = "DoItNow")]
        string DoItNow(GitHubPushWebhook pushInfo);

    }

    [ServiceContract]
    public interface ISecureRestService
    {
        [OperationContract]
        [WebInvoke(Method = "POST",
                            RequestFormat = WebMessageFormat.Json,
                            ResponseFormat = WebMessageFormat.Json,
                            BodyStyle = WebMessageBodyStyle.Bare,
                            UriTemplate = "ItsShowtime")]
        string ItsShowtime(ClientRegistration test);

    }
}
