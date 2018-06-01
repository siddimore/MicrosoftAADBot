using AuthBot;
using AuthBot.Dialogs;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System;
using System.Configuration;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Bot.Builder.CognitiveServices.QnAMaker;
using System.Linq;
using AutoRIFAQHelper.Models;
using AutoRIFAQHelper.Utils;

// UseFul Resources For Bot AAD Login
// https://www.c-sharpcorner.com/article/building-bot-application-with-azure-ad-login-authentication-using-authbot/

// https://docs.microsoft.com/en-us/azure/bot-service/bot-builder-tutorial-authentication?view=azure-bot-service-3.0

// https://github.com/MicrosoftDX/AuthBot/tree/master/AuthBot

namespace AutoRIFAQHelper.Dialogs.Authentication
{
    [Serializable]
    public class AzureADDialog : IDialog<string>
    {

        public Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Login and Logout
        /// </summary>
        /// <param name="context"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        public virtual async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> item)
        {
            var message = await item;
            var activity = await item as Activity;
            int length = (activity.Text ?? string.Empty).Length;

            //endpoint v1
            if (string.IsNullOrEmpty(await context.GetAccessToken(ConfigurationManager.AppSettings["ActiveDirectory.ResourceId"])))
            {
                //Navigate to website for Login
                await context.Forward(new AzureAuthDialog(ConfigurationManager.AppSettings["ActiveDirectory.ResourceId"]), this.ResumeAfterAuth, message, CancellationToken.None);
            }
            else
            {
                await context.Forward(new BasicQnAMakerDialog(), this.AfterAnswerAsync, message, CancellationToken.None);

            }
        }

        /// <summary>
        /// ResumeAfterAuth
        /// </summary>
        /// <param name="context"></param>
        /// <param name="result"></param>
        /// <returns>String Message</returns>
        private async Task LogOutMessageReceived(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as Activity;

            // calculate something for us to return
            int length = (activity.Text ?? string.Empty).Length;
            var msg = activity.Text;

            // Check If User Wants To Logout
            if (msg.ToLower().Equals("logout"))
            {
                await context.Logout();
                //context.Done("U have been logged out");
            }
        }

        private async Task AfterAnswerAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            // wait for the next user message

            //await context.PostAsync(msg);
            context.Wait(MessageReceivedAsync);
        }

        /// <summary>
        /// ResumeAfterAuth
        /// </summary>
        /// <param name="context"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        private async Task ResumeAfterAuth(IDialogContext context, IAwaitable<string> result)
        {
            //AD resposnse message 
            var msg = await result;
            AuthResult authResult;

            string validated = "";

            if (context.UserData.TryGetValue(ContextConstants.AuthResultKey, out authResult))
            {
                try
                {
                    //IMPORTANT: DO NOT REMOVE THE MAGIC NUMBER CHECK THAT WE DO HERE. THIS IS AN ABSOLUTE SECURITY REQUIREMENT

                    //REMOVING THIS WILL REMOVE YOUR BOT AND YOUR USERS TO SECURITY VULNERABILITIES. 

                    //MAKE SURE YOU UNDERSTAND THE ATTACK VECTORS AND WHY THIS IS IN PLACE.
                    context.UserData.TryGetValue<string>(ContextConstants.MagicNumberValidated, out validated);
                    if (validated == "true")
                    {
                        await context.PostAsync(msg);
                        context.Done($"Thanks {authResult.UserName}. You are now in. ");
                        UserData.AccessToken = authResult.AccessToken;
                        UserData.UserName = authResult.UserName;
                        UserData.bValidated = true;
                        
                        //context.Wait(MessageReceivedAsync);
                    }
                }
                catch
                {
                    context.Done($"I'm sorry but something went wrong while authenticating.");
                }

            }
        }   
    }


 }


