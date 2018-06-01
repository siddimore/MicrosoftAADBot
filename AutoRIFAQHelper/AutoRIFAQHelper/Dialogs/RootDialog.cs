using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.CognitiveServices.QnAMaker;
using System.Linq;
using System.Threading;
using AutoRIFAQHelper.Utils;

namespace AutoRIFAQHelper.Dialogs
{
    [Serializable]
    public class RootDialog : IDialog<object>
    {
        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);

            //return Task.CompletedTask;
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as Activity;

            var message = await result;

            // calculate something for us to return
            int length = (activity.Text ?? string.Empty).Length;

            var qnaSubscriptionKey = "001b2f650a8c4676a89e7133818d00c3";//Utils.GetAppSetting("QnASubscriptionKey");
            var qnaKBId = "52ba1b5d-afcb-42a1-ae94-38675dba3c49";//Utils.GetAppSetting("QnAKnowledgebaseId");
            if (!string.IsNullOrEmpty(qnaSubscriptionKey) && !string.IsNullOrEmpty(qnaKBId))
            {
                await context.Forward(new BasicQnAMakerDialog(), AfterAnswerAsync, message, CancellationToken.None);
            }
            else
            {
                await context.PostAsync("Please set QnAKnowledgebaseId and QnASubscriptionKey in App Settings. Get them at https://qnamaker.ai.");
            }
        }

        private async Task AfterAnswerAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            // wait for the next user message
            context.Wait(MessageReceivedAsync);
        }
    }

    // For more information about this template visit http://aka.ms/azurebots-csharp-qnamaker
    [Serializable]
    public class BasicQnAMakerDialog : QnAMakerDialog
    {
        // Go to https://qnamaker.ai and feed data, train & publish your QnA Knowledgebase.        
        // Parameters to QnAMakerService are:
        // Required: subscriptionKey, knowledgebaseId, 
        // Optional: defaultMessage, scoreThreshold[Range 0.0 – 1.0]
        //public BasicQnAMakerDialog() : base(new QnAMakerService(new QnAMakerAttribute(Utils.GetAppSetting("QnASubscriptionKey"), Utils.GetAppSetting("QnAKnowledgebaseId"), "No good match in FAQ.", 0.5)))
        //{}
        static string qnaSubscriptionKey = "001b2f650a8c4676a89e7133818d00c3";//Utils.GetAppSetting("QnASubscriptionKey");
        static string qnaKBId = "52ba1b5d-afcb-42a1-ae94-38675dba3c49";//Utils.GetAppSetting("QnAKnowledgebaseId");
        public BasicQnAMakerDialog() : base(new QnAMakerService(new QnAMakerAttribute(qnaSubscriptionKey, qnaKBId, "Hi At the moment i dont understand your question but i am a learning bot!!", 0.5)))
        { }

        protected override async Task RespondFromQnAMakerResultAsync(IDialogContext context, IMessageActivity message, QnAMakerResults result)
        {

            var answer = result.Answers.First().Answer;
            Activity reply = ((Activity)context.Activity).CreateReply();

            reply.Text = "Enjoy Your Day!!!";

            if (answer.Equals("No good match in FAQ"))
            {
                await context.PostAsync(reply);
            }
            else
            {
                if (message.Text.Equals("tags"))
                {

                    //try
                    //{
                    //    using (HttpClient client = new HttpClient())
                    //    {
                    //        // Empty String
                    //        string Uri = "https://cosaportal.visualstudio.com";
                    //        string Project = "CosaPortalSampleBug";

                    //        string _personalAccessToken = "sw5zz7v5ue4sk6lulroupgocjxkmtkcglz3jskqf6k7xtd7pjyxa";
                    //        string Credentials = Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(string.Format("{0}:{1}", "", _personalAccessToken)));

                    //        //set our headers
                    //        client.DefaultRequestHeaders.Accept.Clear();
                    //        client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                    //        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Credentials);

                    //        //create wiql object
                    //        var wiql = new
                    //        {
                    //            query = "Select [State], [Title] " +
                    //                        "From WorkItems " +
                    //                        "Where [Work Item Type] = 'Bug' " +
                    //                        "And [System.TeamProject] = '" + Project + "' " +
                    //                        "Order By [State] Asc, [Changed Date] Desc, [Created By] Desc, [Created Date] Desc"
                    //        };


                    //        //serialize the wiql object into a json string   
                    //        var postValue = new StringContent(JsonConvert.SerializeObject(wiql), Encoding.UTF8, "application/json"); //mediaType needs to be application/json for a post call
                    //        Console.WriteLine(postValue.ToString());

                    //        //send qeury to REST endpoint to return list of id's from query
                    //        var method = new HttpMethod("POST");
                    //        var httpRequestMessage = new HttpRequestMessage(method, Uri + "/_apis/wit/wiql?api-version=2.2") { Content = postValue };
                    //        var httpResponseMessage = client.SendAsync(httpRequestMessage).Result;

                    //        if (httpResponseMessage.IsSuccessStatusCode)
                    //        {
                    //            var workItemsBase = JsonConvert.DeserializeObject<Dictionary<string, object>>(httpResponseMessage.Content.ReadAsStringAsync().Result);

                    //            foreach (KeyValuePair<string, object> entry in workItemsBase)
                    //            {
                    //                if (entry.Key == "workItems")
                    //                {
                    //                    JArray a = JArray.Parse(entry.Value.ToString());
                    //                    foreach (var item in a)
                    //                    {
                    //                        answer += item["id"].ToString() + item["url"].ToString();


                    //                    }
                    //                }
                    //            }
                    //        }
                    //    }
                    //}
                    //catch (Exception ex)
                    //{

                    //}


                }

                await context.PostAsync(answer);
            }

        }
    }

}