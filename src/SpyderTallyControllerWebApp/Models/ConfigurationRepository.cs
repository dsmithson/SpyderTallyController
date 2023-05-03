using SpyderTallyControllerWebApp.Models.Configuration;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SpyderTallyControllerWebApp.Models
{
    public class ConfigurationRepository : IConfigurationRepository
    {
        public const string DeviceConfigurationFileName = "deviceConfig.json";
        public const string AppConfigurationFileName = "appConfig.json";

        private TallyAppConfiguration appConfiguration;
        private TallyDeviceConfiguration deviceConfiguration;

        public event EventHandler<TallyAppConfiguration> TallyAppConfigurationChanged;

        public ConfigurationRepository()
        {
            //Load or create device config
            deviceConfiguration = Load<TallyDeviceConfiguration>(DeviceConfigurationFileName);
            if (deviceConfiguration == null)
            {
                //Write default device configuration
                deviceConfiguration = new TallyDeviceConfiguration()
                {
                    TallyCount = 4,
                    TallyGpioPinAssignments = new Dictionary<int, int>()
                    {
                        { 0, 4 },
                        { 1, 27 },
                        { 2, 22 },
                        { 3, 23 }
                    }
                };
                Save(GetFullFilePath(DeviceConfigurationFileName), deviceConfiguration);
            }

            //Load or create app config
            appConfiguration = Load<TallyAppConfiguration>(AppConfigurationFileName);
            if (appConfiguration == null)
            {
                appConfiguration = new TallyAppConfiguration()
                {
                    TallyConfigurations = new Dictionary<int, TallyConfiguration>()
                    {
                        { 0, new TallyConfiguration() { Mode = TallyMode.OnInProgram, SpyderServerIP = "172.16.1.100", SpyderSourceName = "Source 1" } }
                    }
                };
                Save(GetFullFilePath(AppConfigurationFileName), appConfiguration);
            }
        }

        private string GetFullFilePath(string fileName)
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);
        }

        private T Load<T>(string fileName)
        {
            string configFile = GetFullFilePath(fileName);
            if (File.Exists(configFile))
            {
                Console.WriteLine("Loading file: " + configFile);
                using var stream = File.OpenRead(configFile);
                return JsonSerializer.Deserialize<T>(stream, new JsonSerializerOptions()
                {
                    Converters =
                    {
                        new JsonStringEnumConverter()
                    }
                });
            }
            else
            {
                Console.WriteLine("File not found: " + configFile);
            }
            return default;
        }

        private void Save<T>(string fileName, T value)
        {
            string configFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);
            using var stream = File.Create(configFile);
            JsonSerializer.Serialize(stream, value, new JsonSerializerOptions()
            {
                WriteIndented = true,
                Converters =
                {
                    new JsonStringEnumConverter()
                }
            });
        }

        private async Task SaveAsync<T>(string fileName, T value)
        {
            string configFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);
            using var stream = File.Create(configFile);
            await JsonSerializer.SerializeAsync(stream, value, new JsonSerializerOptions()
            {
                WriteIndented = true,
                Converters =
                {
                    new JsonStringEnumConverter()
                }
            });
        }

        public TallyAppConfiguration GetTallyAppConfiguration()
        {
            return appConfiguration;
        }

        public TallyDeviceConfiguration GetTallyDeviceConfiguration()
        {
            return deviceConfiguration;
        }

        public async Task SetTallyDeviceConfigurationAsync(TallyAppConfiguration config)
        {
            if (config == null)
                return;

            await SaveAsync(AppConfigurationFileName, config);

            //Store config locally
            this.appConfiguration = config;

            //Notify consumers of change
            TallyAppConfigurationChanged?.Invoke(this, config);
        }
    }
}
