using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace EmbyRefresh
{
    class EmbyRefresh
    {
        private const string Format = "http://{0}:{1}/emby/ScheduledTasks?IsHidden=false&api_key={2}";
        private string agent = "EmbyRefresh";

        static async Task Main(string[] args)
        {
            EmbyRefresh p = new EmbyRefresh();
            await p.RealMainAsync(args);
        }

        public async Task RealMainAsync(string[] args)
        {
            System.Reflection.Assembly asm = System.Reflection.Assembly.GetExecutingAssembly();          
            Console.WriteLine($"{asm.GetName().Name} {asm.GetName().Version}");

            Uri uriResult;
            string host = "localhost";
            string port = "8096";
            string id = string.Empty;
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
                HttpClient httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Add("user-agent", agent);
                string json = httpClient.GetStringAsync(uriResult).Result;
                List<TaskObject> taskObjects = null;
                taskObjects = JsonConvert.DeserializeObject<List<TaskObject>>(json);

                foreach (TaskObject task in taskObjects)
                {
                    if (task.Key == "RefreshGuide")
                    {
                        id = task.Id;
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
                                HttpClient httpClient2 = new HttpClient();
                                httpClient2.DefaultRequestHeaders.Add("user-agent", agent);
                                await httpClient2.PostAsync(uriName, null);
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

            Console.WriteLine("Waiting for guide refresh to complete ...");
            WaitTilFinished(id, host, port, args[0]);
            Console.WriteLine("Guide refresh complete.");
        }

        public void WaitTilFinished(string id, string host, string port, string key)
        {
            bool done = false;

            Uri uriResult;

            string uriName = $"http://{host}:{port}/emby/ScheduledTasks/{id}?api_key={key}";

            bool result = Uri.TryCreate(uriName, UriKind.Absolute, out uriResult) && uriResult.Scheme == Uri.UriSchemeHttp;
            if (!result)
            {
                Console.WriteLine("Invalid URI parameters");
                Console.WriteLine("EmbyRefresh API_KEY {server} {port}");
                Console.WriteLine("To get Emby api key go to dashboard>advanced>security and generate one");
                return;
            }

            while (!done)
            {
                System.Threading.Thread.Sleep(5000);

                try
                {
                    HttpClient httpClient = new HttpClient();
                    httpClient.DefaultRequestHeaders.Add("user-agent", agent);
                    string json = httpClient.GetStringAsync(uriResult).Result;
                    Root status = JsonConvert.DeserializeObject<Root>(json);

                    if (status.State == "Idle")
                        done = true;
                }
                catch (WebException wex)
                {
                    Console.WriteLine(wex.Message);
                    done = true;
                }
                catch (Exception err)
                {
                    Console.WriteLine(err.Message);
                    done = true;
                }

                if (!done)
                    Console.WriteLine("Still waiting for guide refresh to complete ...");
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

    public class Root
    {
        public string Name { get; set; }
        public string State { get; set; }
        public double CurrentProgressPercentage { get; set; }
        public string Id { get; set; }
        public LastExecutionResult LastExecutionResult { get; set; }
        public List<object> Triggers { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public bool IsHidden { get; set; }
        public string Key { get; set; }
    }


    #endregion json classes
}
