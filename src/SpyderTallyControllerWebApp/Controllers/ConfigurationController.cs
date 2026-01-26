using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SpyderTallyControllerWebApp.Models;
using SpyderTallyControllerWebApp.Models.Configuration;
using SpyderTallyControllerWebApp.ViewModels;
using System.Text.Json;

namespace SpyderTallyControllerWebApp.Controllers
{
    public class ConfigurationController : Controller
    {
        private readonly IConfigurationRepository configurationRepository;
        private readonly ISpyderRepository spyderRepository;

        public ConfigurationController(IConfigurationRepository configurationRepository
            , ISpyderRepository spyderRepository)
        {
            this.configurationRepository = configurationRepository;
            this.spyderRepository = spyderRepository;
        }

        public async Task<IActionResult> Index()
        {
            var servers = await spyderRepository.GetServersAsync();
            var sourcesByServer = new Dictionary<string, List<string>>();
            foreach(string server in servers)
            {
                sourcesByServer.Add(server, await spyderRepository.GetSourcesAsync(server));
            }

            var deviceConfig = configurationRepository.GetTallyDeviceConfiguration();
            var appConfig = configurationRepository.GetTallyAppConfiguration();
            var tallyConfigurations = new List<TallyConfigurationViewModel>();
            for (int i=0; i<deviceConfig.TallyCount; i++)
            {
                var vm = new TallyConfigurationViewModel() { TallyIndex = i + 1, TallyMode = TallyMode.OnInProgram.ToString() };
                if(appConfig.TallyConfigurations.TryGetValue(i, out var tally))
                {
                    vm.SpyderServerIP = tally.SpyderServerIP; 
                    vm.SpyderSourceName = tally.SpyderSourceName;
                    vm.TallyMode = tally.Mode.ToString();
                }
                tallyConfigurations.Add(vm);
            }

            var viewModel = new ConfigurationViewModel()
            {
                TallyCount = configurationRepository.GetTallyDeviceConfiguration().TallyCount,
                AvailableServers = servers,
                SourcesByServer = sourcesByServer,
                TallyConfigurations = tallyConfigurations,
                TallyModes = GetFriendlyTallyModes().Select(m => new SelectListItem(m.ModeString, m.Mode.ToString())).ToList()
        };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Save(ConfigurationViewModel vm)
        {
            ValidateModel(vm);

            if(ModelState.IsValid)
            {
                //Save model back to configuration repository
                var modes = GetFriendlyTallyModes();
                var config = new TallyAppConfiguration()
                {
                    TallyConfigurations = vm.TallyConfigurations.ToDictionary(
                        t => t.TallyIndex - 1,
                        t => new TallyConfiguration
                        {
                            SpyderServerIP = t.SpyderServerIP,
                            SpyderSourceName = t.SpyderSourceName,
                            Mode = Enum.Parse<TallyMode>(t.TallyMode)
                        })
                };

                await configurationRepository.SetTallyDeviceConfigurationAsync(config);
                return RedirectToAction("Index");
            }
            return View(vm);
        }

        private void ValidateModel(ConfigurationViewModel vm)
        {
            //todo
            Console.WriteLine(JsonSerializer.Serialize(vm, new JsonSerializerOptions() { WriteIndented = true }));
        }

        private List<(TallyMode Mode, string ModeString)> GetFriendlyTallyModes()
        {
            return new List<(TallyMode Mode, string ModeString)>()
            {
                (TallyMode.OnInProgram, "On in Program"),
                (TallyMode.OnInPreview, "On in Preview"),
                (TallyMode.OnInPreviewOrProgram, "On in Preview or Program"),
                (TallyMode.ForceOn, "Force On"),
                (TallyMode.ForceOff, "Force Off")
            };
        }

        [HttpGet]
        public async Task<IActionResult> GetServers()
        {
            var servers = await spyderRepository.GetServersAsync();
            return Json(servers);
        }

        [HttpGet]
        public async Task<IActionResult> GetSources(string serverIP)
        {
            if (string.IsNullOrWhiteSpace(serverIP))
            {
                return Json(new List<string>());
            }

            var sources = await spyderRepository.GetSourcesAsync(serverIP);
            return Json(sources);
        }
    }
}
