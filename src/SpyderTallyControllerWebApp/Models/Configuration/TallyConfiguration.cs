namespace SpyderTallyControllerWebApp.Models.Configuration
{
    /// <summary>
    /// Configuration associated with a single Tally (Relay)
    /// </summary>
    public class TallyConfiguration
    {
        /// <summary>
        /// Operational mode for the tally
        /// </summary>
        public TallyMode Mode { get; set; }

        /// <summary>
        /// Server IP or Name associated with this tally
        /// </summary>
        public string SpyderServerIP { get; set; }

        /// <summary>
        /// Source name associated with this tally
        /// </summary>
        public string SpyderSourceName { get; set; }
    }
}
