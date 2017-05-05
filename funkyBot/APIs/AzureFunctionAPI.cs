using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using funkyBot.Objects;
using Newtonsoft.Json;

namespace funkyBot.APIs
{
    [Serializable]
    public static class AzureFunctionAPI
    {
        private static readonly string _listTrekskUrl = "https://funkyfunc.azurewebsites.net/api/listTrekks?code=";
        private static readonly string _saveTrekkUrl = "https://funkyfunc.azurewebsites.net/api/startTrekk?code=";
        private static readonly string _stopTrekkUrl = "https://funkyfunc.azurewebsites.net/api/stopTrekk?code=";
        private static readonly string _saveBlobUrl = "https://funkyfunc.azurewebsites.net/api/saveBlob?code=";

        private static readonly List<TrekkTemplate> _trekkTemplates = new List<TrekkTemplate>
            {
                new TrekkTemplate { Id = 1, DisplayName = "Fahrt" },
                new TrekkTemplate { Id = 2, DisplayName = "Arbeitslohn" }
            };

        private static readonly List<TrekkAction> _actions = new List<TrekkAction>
            {
                new TrekkAction { Id = 1, TrekkTemplateId = 1, DisplayName = "Start", StartsTrekk = true, StopsTrekk = false },
                new TrekkAction { Id = 2, TrekkTemplateId = 1, DisplayName = "Stoppen", StartsTrekk = false, StopsTrekk = true }
            };

        private static readonly List<TrekkActionParameter> _actionParameters = new List<TrekkActionParameter>
            {
                new TrekkActionParameter { Id = 1, ActionId = 1, DisplayName = "Auto auswählen", Type = "SelectResource", ResourceCategoryId = 1 },
                new TrekkActionParameter { Id = 2, ActionId = 1, DisplayName = "Kilometerstand eintragen", Type = "ResourceProperty", ResourceCategoryId = 1 }
            };

        private static readonly List<TrekkRessources> _resources = new List<TrekkRessources>
            {
                new TrekkRessources { Id = 1, DisplayName = "F-CB 1899", RessourceCategoryId = 1 },
                new TrekkRessources { Id = 2, DisplayName = "AC-AP 1337", RessourceCategoryId = 1 }
            };

        internal static async Task StartTrekk(StartObject s)
        {
            StartTrekkInput input = new StartTrekkInput()
                {
                    DisplayText = s.DisplayText,
                    Km = s.Km,
                    LicenseNumber = s.LicenseNumber
                };

            using (HttpClient h = new HttpClient())
            {
                HttpResponseMessage response = await h.PostAsJsonAsync(_saveTrekkUrl, input);
                response.EnsureSuccessStatusCode();
            }
        }

        internal static async Task StopTrekk(string id)
        {
            StopTrekkInput input = new StopTrekkInput()
                {
                    Id = id,
                };

            using (HttpClient h = new HttpClient())
            {
                HttpResponseMessage response = await h.PostAsJsonAsync(_stopTrekkUrl, input);
                response.EnsureSuccessStatusCode();
            }
        }

        internal static async Task<List<Trekk>> GetAllTrekksAsync()
        {
            List<Trekk> trekks = new List<Trekk>();
            using (HttpClient h = new HttpClient())
            {
                string response = await h.GetStringAsync(_listTrekskUrl);
                ListTrekksOutput muh2 = JsonConvert.DeserializeObject<ListTrekksOutput>(response);

                foreach (Dictionary<string, object> pair in muh2.Data)
                {
                    trekks.Add(new Trekk()
                        {
                            Id = (string)pair["id"],
                            DisplayName = (string)pair["displayText"]
                        });
                }
            }
            return trekks;
        }

        internal static List<TrekkTemplate> GetTrekkTemplates()
        {
            return _trekkTemplates;
        }

        internal static int GetActionId(int id, string actionCommand)
        {
            int actionId = _actions.Where(x => x.DisplayName == actionCommand && x.TrekkTemplateId == id).Select(x => x.Id).FirstOrDefault();

            return actionId;
        }

        internal static List<TrekkActionParameter> GetActionParameters(int actionId)
        {
            List<TrekkActionParameter> actionParameterList = _actionParameters.Where(x => x.ActionId == actionId).ToList();

            return actionParameterList;
        }

        internal static List<string> GetResources(int resourceCategoryId)
        {
            List<string> selectedResources = _resources.Where(x => x.RessourceCategoryId == resourceCategoryId).Select(x => x.DisplayName).ToList();

            return selectedResources;
        }

        internal static async Task<string> SaveBlob(byte[] bytes)
        {
            using (HttpClient h = new HttpClient())
            {
                HttpResponseMessage response = await h.PostAsync($"{_saveBlobUrl}&imgName={Guid.NewGuid():N}.png", new ByteArrayContent(bytes));
                response.EnsureSuccessStatusCode();

                string url = await response.Content.ReadAsStringAsync();
                return url;
            }
        }
    }
}