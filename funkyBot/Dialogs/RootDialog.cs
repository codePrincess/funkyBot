using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using funkyBot.APIs;
using funkyBot.Objects;
using Microsoft.Bot.Builder.Dialogs;

namespace funkyBot.Dialogs
{
    [Serializable]
    public class RootDialog : IDialog<object>
    {
        private const string Selectedtrekk = "selectedTrekk";
        private const string Trekklist = "TrekkList";

        public Task StartAsync(IDialogContext context)
        {
            context.Wait(this.MessageReceivedAsync);

            return Task.CompletedTask;
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            await this.SendWelcomeMessageAsync(context);
        }

        private async Task SendWelcomeMessageAsync(IDialogContext context)
        {
            string welcomeMessage = "Hallöchen, ick bin deen Trekka. Was willste machen?";

            List<Trekk> trekkList = await AzureFunctionAPI.GetAllTrekksAsync();

            List<string> options = new List<string>();

            if (trekkList.Count == 0)
            {
                // Just offer new trekk
                options.Add("Neuer Trekk");
                options.Add("Frag mich was");
            }
            else if (trekkList.Count == 1)
            {
                // Offer new trekk, stopp trekk or display trekk
                options.Add("Neuer Trekk");
                options.Add("Trekk auswählen");

                context.PrivateConversationData.SetValue(Selectedtrekk, trekkList.First());
                options.Add("Aktuellen Trekk stoppen");

                options.Add("Frag mich was");
            }
            else
            {
                // Offer new trekk and trekk selection
                options.Add("Neuer Trekk");
                options.Add("Trekk auswählen");
                welcomeMessage += $" Du hast gerade {trekkList.Count} am laufen";

                options.Add("Frag mich was");
            }

            context.PrivateConversationData.SetValue(Trekklist, trekkList);

            PromptDialog.Choice(
                context: context,
                options: options,
                prompt: welcomeMessage,
                retry: "Hab ick nich verstanden.",
                promptStyle: PromptStyle.Auto,
                attempts: 2,
                resume: this.welcomeMessageResumeAfterAsync);
        }

        private async Task welcomeMessageResumeAfterAsync(IDialogContext context, IAwaitable<string> result)
        {
            string choice = await result;

            switch (choice)
            {
                case "Neuer Trekk":
                    context.Call(new NewTrekkDialog(), this.NewTrekkDialogResumeAfter);
                    break;
                case "Frag mich was":
                    context.Call(new QuestionDialog(), this.QuestionDialogResumeAfter);
                    break;
                case "Trekk anzeigen":
                    break;
                case "Aktuellen Trekk stoppen":
                    await this.actionOnSelectedTrekkAsync(context, new AwaitableFromItem<string>("Stopp"));
                    return;

                case "Trekk auswählen":
                    List<Trekk> trekks;
                    if (!context.PrivateConversationData.TryGetValue<List<Trekk>>(Trekklist, out trekks))
                    {
                        context.Fail(new Exception("Missing trekks"));
                    }
                    PromptDialog.Choice(
                        context: context,
                        options: trekks.Select(t => t.DisplayName),
                        prompt: "Bitte trekk auswählen",
                        promptStyle: PromptStyle.Auto,
                        attempts: 1,
                        resume: this.selectedTrekkAsync);

                    break;
                default:
                    break;
            }
        }

        private async Task selectedTrekkAsync(IDialogContext context, IAwaitable<string> result)
        {
            string choice = await result;
            List<Trekk> trekks;
            if (!context.PrivateConversationData.TryGetValue<List<Trekk>>(Trekklist, out trekks))
            {
                context.Fail(new Exception("Missing trekks"));
            }

            Trekk trekk = trekks.First(t => t.DisplayName == choice);
            context.PrivateConversationData.SetValue(Selectedtrekk, trekk);

            PromptDialog.Choice(
                context: context,
                options: new string[] { "Stopp", "So lassen" },
                prompt: "Was willst du tun?",
                promptStyle: PromptStyle.Auto,
                attempts: 1,
                resume: this.actionOnSelectedTrekkAsync);
        }

        private async Task actionOnSelectedTrekkAsync(IDialogContext context, IAwaitable<string> result)
        {
            string choice = await result;
            if (choice == "Stopp")
            {
                Trekk trekk;
                if (!context.PrivateConversationData.TryGetValue<Trekk>(Selectedtrekk, out trekk))
                {
                    context.Fail(new Exception("Missing trekk"));
                }
                await AzureFunctionAPI.StopTrekk(trekk.Id);

                await context.PostAsync("Ok, hab ich gestoppt");
            }

            await this.SendWelcomeMessageAsync(context);
        }

        private async Task QuestionDialogResumeAfter(IDialogContext context, IAwaitable<string> result)
        {
            string response = await result;

            await context.PostAsync(response);

            await this.SendWelcomeMessageAsync(context);
        }

        private async Task NewTrekkDialogResumeAfter(IDialogContext context, IAwaitable<StartObject> result)
        {
            StartObject startObject = await result;

            // Save new trekk
            await AzureFunctionAPI.StartTrekk(startObject);

            await context.PostAsync("Neuer Trekk wurde angelegt.");

            await this.SendWelcomeMessageAsync(context);
        }
    }
}