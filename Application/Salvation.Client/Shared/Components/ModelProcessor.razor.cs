﻿using BlazorApplicationInsights;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Http;
using Microsoft.JSInterop;
using Newtonsoft.Json;
using Salvation.Core.Constants;
using Salvation.Core.Profile.Model;
using Salvation.Core.ViewModel;
using System.Net.Http.Json;
using System.Text.Json;

namespace Salvation.Client.Shared.Components
{
    public partial class ModelProcessor : ComponentBase
    {
        [Inject]
        protected IHttpClientFactory? _httpClientFactory { get; set; }
        [Inject]
        protected IConfiguration? _configuration { get; set; }

        [Inject] 
        protected IApplicationInsights? _appInsights { get; set; }

        private static readonly int holyPriestSpecId = 257;
        private static readonly string wowheadItemLinkPrefix = "//wowhead.com/item=";
        private static readonly string wowheadSpellPrefix = "//wowhead.com/spell=";

        // Loading of default profile
        private static readonly string defaultProfileEndpoint = "DefaultProfile";
        private PlayerProfileViewModel? data = default;
        private string errorMessage = string.Empty;
        private bool loadingData = true;

        private string searchString = "";
        private string advancedSearchString = "";

        // Talent viewer
        private Dictionary<int, int> selectedTalents = new Dictionary<int, int>()
        {
            { 10060, 1 },
        };

        // Loading of results
        private static readonly string processModelEndpoint = "ProcessModel";
        private ModellingResultsViewModel? modellingResults = null;
        private bool loadingResults = false;

        protected override async Task OnInitializedAsync()
        {
            await GetDefaultProfile();
        }

        private async Task GenerateResults()
        {
            loadingResults = true;

            if (_httpClientFactory == null)
                throw new NullReferenceException("Web client was not initialised");

            if (_appInsights == null)
                throw new NullReferenceException("App insights logging was not initialised");

            var client = _httpClientFactory.CreateClient("Api");

            try
            {
                var response = await client.PostAsJsonAsync(processModelEndpoint, data);

                if (response.IsSuccessStatusCode)
                {
                    modellingResults = await response.Content.ReadFromJsonAsync<ModellingResultsViewModel>();

                    //var jsonOptions = new JsonSerializerOptions
                    //{
                    //    PropertyNameCaseInsensitive = true,
                    //};
                    
                    //modellingResults = await JsonConvert.DeserializeObject<ModellingResultsViewModel>(responseStream, jsonOptions);
                    //modellingResults = await JsonSerializer.DeserializeAsync<ModellingResultsViewModel>(responseStream, jsonOptions);

                    loadingResults = false;
                }
                else
                {
                    loadingResults = false;
                }
            }
            catch (HttpRequestException ex)
            {
                Error error = new()
                {
                    Message = ex.Message,
                    Stack = ex.StackTrace
                };
                await _appInsights.TrackException(error);

                loadingResults = false;
            }
            catch (InvalidOperationException ex)
            {
                Error error = new()
                {
                    Message = ex.Message,
                    Stack = ex.StackTrace
                };
                await _appInsights.TrackException(error);

                loadingResults = false;
            }
        }

        private async Task GetDefaultProfile()
        {
            loadingData = true;
            errorMessage = "";

            if (_httpClientFactory == null)
                throw new NullReferenceException("Web client was not initialised");

            if (_appInsights == null)
                throw new NullReferenceException("App insights logging was not initialised");

            var client = _httpClientFactory.CreateClient("Api");

            try
            {
                var response = await client.GetAsync($"{defaultProfileEndpoint}?specid={holyPriestSpecId}");

                if (response.IsSuccessStatusCode)
                {
                    using var responseStream = await response.Content.ReadAsStreamAsync();

                    var jsonOptions = new System.Text.Json.JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                    };

                    data = await System.Text.Json.JsonSerializer.DeserializeAsync<PlayerProfileViewModel>(responseStream, jsonOptions);

                    loadingData = false;
                }
                else
                {
                    errorMessage = "Unable to generate default profile.";
                    loadingData = false;
                }
            }
            catch (HttpRequestException ex)
            {
                Error error = new()
                {
                    Message = ex.Message,
                    Stack = ex.StackTrace
                };
                await _appInsights.TrackException(error);

                errorMessage = "Unable to generate default profile.";
                loadingData = false;
            }
            catch(InvalidOperationException ex)
            {
                Error error = new()
                {
                    Message = ex.Message,
                    Stack = ex.StackTrace
                };
                await _appInsights.TrackException(error);

                errorMessage = $"Unable to generate default profile.";
                loadingData = false;
            }
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await JS.InvokeVoidAsync("updateWowheadTooltips");
        }

        public string GenerateWowheadItemLink(Item item)
        {
            // Link is in the format item={id}?bonus={bonus}:{bonus}&ilvl={ilvl}&spec={specid}
            var link = wowheadItemLinkPrefix;

            link += $"{item.ItemId}";

            return link;
        }

        public string GenerateWowheadSpellLink(int spellId)
        {
            var link = wowheadSpellPrefix;

            link += $"{spellId}";

            return link;
        }

        public string GenerateWowheadSpellLink(uint spellId)
        {
            return GenerateWowheadSpellLink((int)spellId);
        }

        /// <summary>
        /// Filters the playstyle list based on the search text
        /// </summary>
        private Func<CastProfileViewModel, bool> playstyleFilter => x =>
        {
            if (string.IsNullOrWhiteSpace(searchString))
                return true;

            if (x.SpellId.ToString().Contains(searchString, StringComparison.OrdinalIgnoreCase))
                return true;

            if (x.Description.Contains(searchString, StringComparison.OrdinalIgnoreCase))
                return true;

            return false;
        };

        /// <summary>
        /// Filters the advanced list based on the search text
        /// </summary>
        private Func<AdvancedSettingsViewModel, bool> advancedFilter => x =>
        {
            if (string.IsNullOrWhiteSpace(advancedSearchString))
                return true;

            if (x.SpellId.ToString().Contains(advancedSearchString, StringComparison.OrdinalIgnoreCase))
                return true;

            if (x.Name.Contains(advancedSearchString, StringComparison.OrdinalIgnoreCase))
                return true;

            return false;
        };
    }
}
