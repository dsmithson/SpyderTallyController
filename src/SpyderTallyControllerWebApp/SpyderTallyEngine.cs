using Spyder.Client;
using Spyder.Client.Net.Notifications;
using Spyder.Client.Net.DrawingData;
using SpyderTallyControllerWebApp.Models;
using SpyderTallyControllerWebApp.Models.Configuration;

namespace SpyderTallyControllerWebApp
{
    public class SpyderTallyEngine
    {
        private readonly IConfigurationRepository configurationRepository;
        private readonly ISpyderRepository spyderRepository;
        private readonly IRelayRepository relayRepository;

        private readonly TallyDeviceConfiguration deviceConfig;
        private TallyAppConfiguration appConfig;

        public SpyderTallyEngine(IConfigurationRepository configurationRepository, 
            IRelayRepository relayRepository,
            ISpyderRepository spyderRepository)
        {
            this.configurationRepository = configurationRepository;
            this.appConfig = configurationRepository.GetTallyAppConfiguration();
            this.deviceConfig = configurationRepository.GetTallyDeviceConfiguration();
            this.configurationRepository.TallyAppConfigurationChanged += ConfigurationRepository_TallyAppConfigurationChanged;

            this.relayRepository = relayRepository;

            this.spyderRepository = spyderRepository;
            spyderRepository.DrawingDataReceived += SpyderRepository_DrawingDataReceived;
        }

        private void ConfigurationRepository_TallyAppConfigurationChanged(object sender, TallyAppConfiguration e)
        {
            //Update status of tallies in case any of the force on / force off values have changed
            this.appConfig = e;
            UpdateTallies(null, null);
        }

        private void SpyderRepository_DrawingDataReceived(object sender, DrawingDataReceivedEventArgs e)
        {
            UpdateTallies(e.ServerIP, e.DrawingData);
        }

        private void UpdateTallies(string serverIP, DrawingData data)
        {
            if (appConfig == null || deviceConfig == null)
                return;

            //Determine target relay state
            int tallyCount = deviceConfig.TallyCount;
            for (int i = 0; i < tallyCount; i++)
            {
                if (appConfig.TallyConfigurations.TryGetValue(i, out TallyConfiguration tallyConfig))
                {
                    bool? curr = GetTallyState(serverIP, data, tallyConfig);
                    if (curr != null)
                    {
                        //Update relay if changed
                        bool last = relayRepository.GetRelayStatus(i);
                        if (last != curr)
                        {
                            relayRepository.SetRelayStatus(i, curr.Value);
                        }
                    }
                }
            }
        }

        private static bool? GetTallyState(string serverIP, DrawingData data, TallyConfiguration tally)
        {
            if (tally.Mode == TallyMode.ForceOn)
            {
                return true;
            }
            else if (tally.Mode == TallyMode.ForceOff)
            {
                return false;
            }
            else if (tally.SpyderServerIP == serverIP && data != null)
            {
                var isPresent = new Func<DrawingKeyFrame, bool, bool, bool>(
                    (dkf, inPgm, inPvw) =>
                    {
                        if (dkf.Source == tally.SpyderSourceName && dkf.IsVisible && dkf.IsWithinPixelSpace)
                        {
                            var ps = data.GetPixelSpace(dkf.PixelSpaceID);
                            if (ps != null && ((ps.Scale == 1f && inPgm) || (ps.Scale != 1f && inPvw)))
                            {
                                return true;
                            }
                        }
                        return false;
                    });

                if (tally.Mode == TallyMode.OnInProgram)
                {
                    bool inPgm = data.DrawingKeyFrames.Values.Any((dkf) => isPresent(dkf, true, false));
                    return inPgm;
                }
                else if(tally.Mode == TallyMode.OnInPreview)
                {
                    bool inPvw = data.DrawingKeyFrames.Values
                        .Concat(data.PreviewDrawingKeyFrames.Values)
                        .Any((dkf) => isPresent(dkf, false, true));

                    return inPvw;
                }
                else if(tally.Mode == TallyMode.OnInPreviewOrProgram)
                {
                    bool inPvwOrPgm = data.DrawingKeyFrames.Values
                        .Concat(data.PreviewDrawingKeyFrames.Values)
                        .Any((dkf) => isPresent(dkf, true, true));

                    return inPvwOrPgm;
                }
            }
            return null;
        }
    }
}
