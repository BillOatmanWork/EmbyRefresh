using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace EmbyRefresh
{
    class EmbyRefresh
    {
        private const string Format = "http://{0}:{1}/emby/ScheduledTasks?IsHidden=false&api_key={2}";

        static void Main(string[] args)
        {
            EmbyRefresh p = new EmbyRefresh();
            p.RealMain(args);
        }

        public void RealMain(string[] args)
        {
            System.Reflection.Assembly asm = System.Reflection.Assembly.GetExecutingAssembly();
            string agent = "EmbyRefresh:" + asm.GetName().Version;
            Console.WriteLine(agent);

            Uri uriResult;
            string host = "localhost";
            string port = "8096";
            if (args.Length == 2)
            {
                host = args[1];
            }
            else if (args.Length == 3)
            {
                host = args[1];
                port = args[2];
            }
            else if (args.Length != 1)
            {
                Console.WriteLine("EmbyRefresh API_KEY {server} {port}");
                Console.WriteLine("To get Emby api key go to dashboard>advanced>security and generate one");
                return;
            }

            string uriName = string.Format(Format, host, port, args[0]);
            bool result = Uri.TryCreate(uriName, UriKind.Absolute, out uriResult) && uriResult.Scheme == Uri.UriSchemeHttp;
            if (!result)
            {
                Console.WriteLine("Invalid URI parameters");
                Console.WriteLine("EmbyRefresh API_KEY {server} {port}");
                Console.WriteLine("To get Emby api key go to dashboard>advanced>security and generate one");
                return;
            }

            try
            {
                WebClient webClient = new WebClient();
                webClient.Headers.Add("user-agent", agent);
                Stream stream = webClient.OpenRead(uriResult);
                StreamReader sr = new StreamReader(stream);
                String request = sr.ReadToEnd();
                List<TaskObject> taskObjects = null;
                taskObjects = JsonConvert.DeserializeObject<List<TaskObject>>(request);

                foreach (TaskObject task in taskObjects)
                {
                    if (task.Key == "RefreshGuide")
                    {
                        if (task.State == "Idle")
                        {
                            bool waitForDaily = false;
                            foreach (Trigger trigger in task.Triggers)
                            {
                                Console.WriteLine(trigger.Type);
                                if (trigger.Type == "DailyTrigger")
                                {
                                    DateTime triggerTime = new DateTime(trigger.TimeOfDayTicks);
                                    TimeSpan ts = triggerTime.TimeOfDay - DateTime.Now.TimeOfDay;
                                    if (ts.TotalSeconds > 0 && ts.TotalMinutes < 15)
                                    {
                                        Console.WriteLine("Guide refreshing soon, no forced update.");
                                        waitForDaily = true;
                                        break;
                                    }
                                }
                            }
                            if (waitForDaily == false)
                            {
                                uriName = string.Format("http://{0}:{1}/emby/ScheduledTasks/Running/{2}?api_key={3}", host, port, task.Id, args[0]);
                                result = Uri.TryCreate(uriName, UriKind.Absolute, out uriResult) && uriResult.Scheme == Uri.UriSchemeHttp;
                                webClient = new WebClient();
                                webClient.Headers.Add("user-agent", agent);
                                webClient.UploadString(uriResult, "");
                                webClient.Dispose();
                                Console.WriteLine("Refresh guide activated");
                            }
                            break;
                        }
                        else
                        {
                            Console.WriteLine("Refresh guide skipped " + task.State);
                        }
                        break;
                    }
                }
            }
            catch (WebException wex)
            {
                Console.WriteLine(wex.Message);
            }
            catch (Exception err)
            {
                Console.WriteLine(err.Message);
            }
        }
    }

    #region json classes
    public class Trigger
    {
        public string Type { get; set; }
        public long TimeOfDayTicks { get; set; }
        public long MaxRuntimeTicks { get; set; }
        public long? IntervalTicks { get; set; }
    }

    public class LastExecutionResult
    {
        public DateTime StartTimeUtc { get; set; }
        public DateTime EndTimeUtc { get; set; }
        public string Status { get; set; }
        public string Name { get; set; }
        public string Key { get; set; }
        public string Id { get; set; }
    }

    public class TaskObject
    {
        public string Name { get; set; }
        public string State { get; set; }
        public string Id { get; set; }
        public List<Trigger> Triggers { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public bool IsHidden { get; set; }
        public string Key { get; set; }
        public LastExecutionResult LastExecutionResult { get; set; }
    }
    #endregion json classes
}
