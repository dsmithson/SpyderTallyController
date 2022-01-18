namespace SpyderTallyControllerWebApp.Models.Configuration
{
    /// <summary>
    /// Wraps the device configuration file.  Contains properties tied to the physical setup for the device
    /// </summary>
    public class TallyDeviceConfiguration
    {
        /// <summary>
        /// Number of tallies (Relays) in the environment
        /// </summary>
        public int TallyCount { get; set; } = 8;

        /// <summary>
        /// GPIO pin assignments, keyed by tally (relay) index
        /// </summary>
        public Dictionary<int, int> TallyGpioPinAssignments { get; set; } = new Dictionary<int, int>();
    }
}
