using System.Device.Gpio;
using SpyderTallyControllerWebApp.Models.Configuration;

namespace SpyderTallyControllerWebApp.Models
{
    public class RelayRepository : IRelayRepository
    {
        private readonly bool[] relayStatus;
        private readonly TallyDeviceConfiguration deviceConfiguration;
        private readonly GpioController gpioController;

        public event EventHandler RelayStatusChanged;

        private IConfigurationRepository configurationRepository;

        public RelayRepository(IConfigurationRepository configurationRepository)
        {
            this.configurationRepository = configurationRepository;
            this.deviceConfiguration = configurationRepository.GetTallyDeviceConfiguration() ?? new TallyDeviceConfiguration();

            relayStatus = new bool[deviceConfiguration.TallyCount];

            //Initialize GPIO pins
            gpioController = new GpioController();
            foreach(var gpioPin in deviceConfiguration.TallyGpioPinAssignments)
            {
                gpioController.OpenPin(gpioPin.Value, PinMode.Output, PinValue.Low);
            }
        }

        public bool[] GetRelayStatus()
        {
            return relayStatus;
        }

        public bool GetRelayStatus(int index)
        {
            if (index < 0 || index >= relayStatus.Length)
                return false;

            return relayStatus[index];
        }

        public void SetRelayStatus(int relayIndex, bool value)
        {
            if (relayIndex < 0 || relayIndex >= relayStatus.Length || relayStatus[relayIndex] == value)
                return;

            //Lookup GPIO pin for given relay index
            if(deviceConfiguration.TallyGpioPinAssignments.TryGetValue(relayIndex, out int pinAssignment))
            {
                //Write hardware
                gpioController.Write(pinAssignment, value ? PinValue.High : PinValue.Low);

                //Update internal state and fire event
                relayStatus[relayIndex] = value;
                RelayStatusChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
