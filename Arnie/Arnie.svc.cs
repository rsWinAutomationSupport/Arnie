using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Messaging;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Security;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Hosting;

namespace Arnie
{
    public class Arnie : IRestService,ISecureRestService
    {
        string ISecureRestService.ItsShowtime(ClientRegistration registrationData)
        {
            WebOperationContext ctx = WebOperationContext.Current;
            string queuePath = Config.ArnieConfig.Settings.QueuePath;
            if (!MessageQueue.Exists(queuePath))
            {
                ctx.OutgoingResponse.StatusCode = System.Net.HttpStatusCode.InternalServerError;
                return "Could not locate Queue configured in Web.config. Please check service Configuration";
            }
            
            Guid guid;
            if (!Guid.TryParse(registrationData.uuid, out guid))
            {
                ctx.OutgoingResponse.StatusCode = System.Net.HttpStatusCode.UnsupportedMediaType;
                return "Uuid specified is in the wrong format.";
            }
            
            MessageQueue msgQ = new MessageQueue(queuePath);
            Message msg = new Message();
            msg.Formatter = new XmlMessageFormatter(new String[] { "System.String" });
            try
            {
                msg.Body = String.Format("{{'uuid':'{0}','publicCert':'{1}','dsc_config':'{2}'}}", registrationData.uuid, registrationData.publicCert, registrationData.dsc_config);
                msg.Label = "Client_Registration";
                msgQ.Send(msg);
                msgQ.Close();
            }
            catch (Exception exp)
            {
                Utility.LogException(exp);
            }
            return String.Format("{{'uuid':'{0}','publicCert':'{1}'}}", registrationData.uuid, registrationData.publicCert);  //String.Format("This worked, you entered: {0}", test["embedded"]);
        }


        string IRestService.DoItNow(GitHubPushWebhook pushInfo)
        {
            WebOperationContext ctx = WebOperationContext.Current;
            if(!(Utility.IsAllowed(HttpContext.Current)))
            {
                ctx.OutgoingResponse.StatusCode = System.Net.HttpStatusCode.Forbidden;
                return "Source IP not in ACL";
            }
            
            
            string confJSON = HostingEnvironment.MapPath("~/App_Data/Configuration.json");
            Repos config;
            try
            {
                string json = File.ReadAllText(confJSON);
                DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(Repos));

                MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
                config = (Repos)ser.ReadObject(stream);
                stream.Close();
            }
            catch (FileNotFoundException exc)
            {
                Utility.LogException(exc);
                ctx.OutgoingResponse.StatusCode = System.Net.HttpStatusCode.InternalServerError;
                return "Configuration.json not found, no commands are run.";
            }
            catch (SerializationException exc)
            {
                Utility.LogException(exc);
                ctx.OutgoingResponse.StatusCode = System.Net.HttpStatusCode.InternalServerError;
                return String.Format("The following error occurred trying to deserialize JSON in Configuration.json:\n{0}",exc.Message);
            }


            List<string> commands = new List<string>();
            IEnumerable<RepositoryConfig> repositories = new List<RepositoryConfig>();

            try
            {
                repositories = config.repos.Where(repo => (string)pushInfo.repository[repo.repo_key] == repo.repo_value).ToList();
            }
            catch (Exception exc)
            {
                Utility.LogException(exc);
                ctx.OutgoingResponse.StatusCode = System.Net.HttpStatusCode.BadRequest;
                return String.Format("The following error occurred trying to match repository information with commands defined via Configuration.json:\n{0}", exc.Message);
            }

            if (repositories.Count() == 0)
            {
                return String.Format("No actions defined for repo with full name: {0}", pushInfo.repository.full_name);
            }
            foreach (RepositoryConfig repository in repositories)
            {
                List<Commands> list = repository.commands.ToList<Commands>();
                list.Sort();
                commands.AddRange(list.Select(command => command.command));
            }
            PowerShell shell = PowerShell.Create();
            

            foreach (string command in commands)
            {
                shell.AddScript(command);
            }
            shell.BeginInvoke();
            return String.Format("Following actions were executed for repo with full name: {0}:\n\t{1}", 
                                 pushInfo.repository.full_name, 
                                 String.Join("\n\t",commands));
        }
    }

    public class Utility
    {
        public static void LogException(Exception exc)
        {
            string sSource;
            string sLog;
            int eventId;

            sSource = "Arnie WCF Web Service";
            sLog = "DevOps";
            eventId = 1004;
            try
            {
                if (!EventLog.SourceExists(sSource))
                {
                    EventLog.CreateEventSource(sSource, sLog);
                }
                EventLog.WriteEntry(sSource, String.Format("Following Exception occurred: \n{0}",
                                    exc), EventLogEntryType.Error, eventId);

            }
            catch (SecurityException)
            {
                try
                {
                    string attemptedSource = sSource;
                    sSource = ".NET Runtime";

                    string queryString = String.Format("*[System[(Level = {0}) and Provider[@Name = '{1}'] " +
                                                       "and (EventID = {2}) and TimeCreated[timediff(@SystemTime) >= '{3}']]]",
                                                       3,
                                                       sSource,
                                                       eventId,
                                                       3600000);

                    EventLogQuery eQuery = new EventLogQuery("Application", PathType.LogName, queryString);
                    EventLogReader logReader = new EventLogReader(eQuery);
                    EventRecord record = logReader.ReadEvent();
                    if (record == null)
                    {
                        EventLog.WriteEntry(sSource, String.Format("SiteName: {0}\nApplicationVirtualPath: {1}\nFramework version: {2}\nDescription: Could not write to Eventlog " +
                                                                   "using {3} as source because the source didn't exist and/or could not be created.",
                                                                   HostingEnvironment.SiteName,
                                                                   HostingEnvironment.ApplicationVirtualPath,
                                                                   Environment.Version,
                                                                   attemptedSource),
                                            EventLogEntryType.Warning, eventId);
                    }

                }
                catch (SecurityException)
                {
                    //Do nothing for now...
                }
            }
        }

        public static bool IsAllowed(HttpContext context)
        {
            var allowedAddresses = Config.ArnieConfig.Settings.AllowedAddresses ?? "";
            if (string.IsNullOrWhiteSpace(allowedAddresses) || allowedAddresses.ToLowerInvariant() == "none")
            {
                return false;
            }
            var method = context.Request.HttpMethod.ToUpperInvariant();
            if (method != "POST" && method != "PUT")
            {
                return false;
            }
            var allowed = false;
            var requestAddress = context.Request.UserHostAddress ?? "null";
            foreach (var address in allowedAddresses.Split(new[] { ',', ';', ' ' }))
            {
                if (!string.IsNullOrWhiteSpace(address))
                {
                    if (address == "*")
                    {
                        return true;
                    }
                    var regex = new Regex("^" + address.Replace(".", "\\.").Replace("*", "\\d+") + "$");
                    if (regex.IsMatch(requestAddress))
                    {
                        allowed = true;
                        break;
                    }
                }
            }
            return allowed;
        }

    }

}
