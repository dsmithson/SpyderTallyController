using Microsoft.AspNetCore.Mvc.Rendering;

namespace SpyderTallyControllerWebApp.ViewModels
{
    public class TallyConfigurationViewModel
    {
        public int TallyIndex { get; set; }

        public string SpyderServerIP { get; set; }

        public string SpyderSourceName { get; set; }

        public string TallyMode { get; set; }
    }
}
