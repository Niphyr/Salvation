using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Salvation.Core.Profile;
using Salvation.Core;
using Salvation.Core.Constants;
using System.Runtime.CompilerServices;
using Salvation.Core.Models;
using Salvation.Core.Interfaces.Constants;
using Salvation.Core.Interfaces.Models;
using Salvation.Core.State;

namespace Salvation.Api
{
    public class ProcessModel
    {
        private readonly IConstantsService _constantsService;
        private readonly IModellingService _modellingService;

        public ProcessModel(IConstantsService constantService, IModellingService modellingService)
        {
            this._constantsService = constantService;
            this._modellingService = modellingService;
        }

        [FunctionName("ProcessModel")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest request,
            ILogger log, ExecutionContext context)
        {
            // Parse the incoming profile
            BaseProfile profile;
            try
            {
                string requestBody = await new StreamReader(request.Body).ReadToEndAsync();
                profile = JsonConvert.DeserializeObject<BaseProfile>(requestBody);
            }
            catch (Exception ex)
            {
                log.LogError("Unable to process request body, wrong format?", ex);
                return new BadRequestResult();
            }

            if (profile == null)
            {
                log.LogError("Profile needs to be provided");
                return new BadRequestResult();
            }

            log.LogInformation("Processing a new profile: {0}", JsonConvert.SerializeObject(profile));

            // Load the profile into the model and return the results
            try
            {
                _constantsService.SetDefaultDirectory(context.FunctionAppDirectory);

                var sw = new StatWeightGenerator(_constantsService);

                var effectiveHealingStatWeights = sw.Generate(profile, 100,
                    StatWeightGenerator.StatWeightType.EffectiveHealing);

                var rawHealingStatWeights = sw.Generate(profile, 100,
                    StatWeightGenerator.StatWeightType.RawHealing);

                //--------------
                GameState state = new GameState();
                state.Constants = _constantsService.LoadConstantsFromFile();
                state.Profile = profile;
                var results = _modellingService.GetResults(state);

                //------------------------------
                return new JsonResult(new
                {
                    ModelResults = results,
                    StatWeightsEffective = effectiveHealingStatWeights,
                    StatWeightsRaw = rawHealingStatWeights,
                    State = state
                });
            }
            catch(Exception ex)
            {
                log.LogError(ex, $"Unable to process model");
                return new BadRequestResult();
            }
        }
    }
}
