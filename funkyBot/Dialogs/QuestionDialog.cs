using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using funkyBot.APIs;
using funkyBot.Objects;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;

namespace funkyBot.Dialogs
{
    [Serializable]
    public class QuestionDialog : IDialog<string>
    {
        private LuisResult luisResult;

        public async Task StartAsync(IDialogContext context)
        {
            await context.PostAsync("Was möchtest du wissen, oder schick mir was?");

            context.Wait(this.MessageReceivedAsync);
        }

        public async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            IMessageActivity activity = await argument;
            if (activity.Attachments != null && activity.Attachments.Count > 0)
            {
                await context.PostAsync("Ich habe attachments gefunden");

                byte[] bytes = await this.getContentFromAttachment(activity, activity.Attachments[0]);

                string url = await AzureFunctionAPI.SaveBlob(bytes);

                HeroCard heroCard = new HeroCard()
                    {
                        Images = new List<CardImage>()
                            {
                                new CardImage(url, alt: url),
                            },
                        Text = url
                    };
                IMessageActivity response = context.MakeMessage();
                response.Attachments = new List<Attachment>();
                response.Attachments.Add(heroCard.ToAttachment());

                await context.PostAsync(response);

                context.Done("Fertig");
                return;
            }
            string TextToLuis = activity.Text;

            this.luisResult = await LuisApi.GetLuisResult(TextToLuis);

            await this.HandleLuisMessage(context);
        }

        private async Task<byte[]> getContentFromAttachment(IActivity activity, Attachment attachment)
        {
            using (ConnectorClient connectorClient = new ConnectorClient(new Uri(activity.ServiceUrl)))
            {
                string token = await ((MicrosoftAppCredentials)connectorClient.Credentials).GetTokenAsync();
                Uri uri = new Uri(attachment.ContentUrl);
                using (HttpClient httpClient = new HttpClient())
                {
                    if (uri.Host.EndsWith("skype.com") && uri.Scheme == "https")
                    {
                        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/octet-stream"));
                    }
                    else
                    {
                        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(attachment.ContentType));
                    }
                    return await httpClient.GetByteArrayAsync(uri);
                }
            }
        }

        private Task HandleLuisMessage(IDialogContext context)
        {
            string responseText;

            switch (this.luisResult.topScoringIntent.intent)
            {
                case "ProductInfo":
                    responseText = "Funky, das supertolle Erfassungssystem #FreedomManufaktur #SuperTE #isso";
                    break;
                case "CompanyInfo":
                    responseText = "freedom manufaktur, besucht uns unter https://freedom-manufaktur.com";
                    break;
                default:
                    responseText = "Geht nicht #fail";
                    break;
            }

            context.Done(responseText);

            return Task.CompletedTask;
        }
    }
}