using SpyderTallyControllerWebApp.Models.Configuration;

namespace SpyderTallyControllerWebApp.Models
{
    public interface IConfigurationRepository
    {
        event EventHandler<TallyAppConfiguration> TallyAppConfigurationChanged;

        TallyAppConfiguration GetTallyAppConfiguration();

        TallyDeviceConfiguration GetTallyDeviceConfiguration();

        Task SetTallyDeviceConfigurationAsync(TallyAppConfiguration config);
    }
}
