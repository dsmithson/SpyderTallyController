using Microsoft.AspNetCore.Mvc.Rendering;

namespace SpyderTallyControllerWebApp.ViewModels
{
    public class ConfigurationViewModel
    {
        public List<string> AvailableServers { get; set; } = new List<string>();

        public Dictionary<string, List<string>> SourcesByServer = new Dictionary<string, List<string>>();

        public int TallyCount { get; set; }

        public List<TallyConfigurationViewModel> TallyConfigurations { get; set; } = new List<TallyConfigurationViewModel>();

        public List<SelectListItem> TallyModes { get; set; } = new List<SelectListItem>();
    }
}
