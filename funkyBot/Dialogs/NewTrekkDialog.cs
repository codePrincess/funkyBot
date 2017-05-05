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
    public class NewTrekkDialog : IDialog<StartObject>
    {
        private List<TrekkTemplate> _templates = new List<TrekkTemplate>();

        private List<TrekkActionParameter> _actionParameters = new List<TrekkActionParameter>();

        private readonly StartObject _startObject = new StartObject();

        private string _selection = "";

        public Task StartAsync(IDialogContext context)
        {
            this._templates = AzureFunctionAPI.GetTrekkTemplates();

            List<string> options = this._templates.Select(x => x.DisplayName).ToList();

            PromptDialog.Choice(
                context: context,
                options: options,
                resume: this.DisplayNameResumeAfter,
                promptStyle: PromptStyle.Auto,
                prompt: "Wähle deinen Trekk aus.",
                retry: "Bitte einen Trekk aus der Liste wählen.",
                attempts: 2
            );

            return Task.CompletedTask;
        }

        private async Task DisplayNameResumeAfter(IDialogContext context, IAwaitable<string> result)
        {
            // Get the users selected track type
            this._selection = await result;

            // Get the TrekkTemplateId based on the unique DisplayName of the TrekkTemplate
            int id = this._templates.Where(x => x.DisplayName == this._selection).Select(x => x.Id).FirstOrDefault();

            // Set command to Start
            string actionCommand = "Start";

            // Get the ActionId needed to get the ActionParameters
            int actionId = AzureFunctionAPI.GetActionId(id, actionCommand);

            // Get the ActionParameters
            this._actionParameters = AzureFunctionAPI.GetActionParameters(actionId);

            await this.PerformDataGathering(context);
        }

        private Task PerformDataGathering(IDialogContext context)
        {
            if (this._actionParameters.Count == 0)
            {
                this._startObject.DisplayText = this._selection;
                context.Done(this._startObject);
            }
            else
            {
                if (this._actionParameters.First().Type == "SelectResource")
                {
                    int resourceCategoryId = this._actionParameters.First().ResourceCategoryId;

                    PromptDialog.Choice(
                        context: context,
                        options: AzureFunctionAPI.GetResources(resourceCategoryId),
                        resume: this.SelectResourceResumeAfter,
                        promptStyle: PromptStyle.Auto,
                        prompt: "Wähle dein Nummernschild.",
                        retry: "Bitte wähle eines der Nummernshilder aus der Liste.",
                        attempts: 2
                    );
                }
                else
                {
                    PromptDialog.Number(
                        context: context,
                        resume: this.ResourcePropertyResumeAfter,
                        prompt: "Gib den Kilometerstand an.",
                        retry: "Die Eingabe wurde nicht erkannt, bitte einen valide Kilometerzahl ohne Zeichen eingeben.",
                        attempts: 2
                    );
                }
            }

            return Task.CompletedTask;
        }

        private async Task SelectResourceResumeAfter(IDialogContext context, IAwaitable<string> result)
        {
            string licenseNumber = await result;

            this._startObject.LicenseNumber = licenseNumber;

            this._actionParameters.Remove(this._actionParameters.First());

            await this.PerformDataGathering(context);
        }

        private async Task ResourcePropertyResumeAfter(IDialogContext context, IAwaitable<long> result)
        {
            long km = await result;

            this._startObject.Km = (int)km;

            this._actionParameters.Remove(this._actionParameters.First());

            await this.PerformDataGathering(context);
        }
    }
}