namespace SpyderTallyControllerWebApp.Models.Configuration
{
    /// <summary>
    /// Configuration for tallies managed by this app
    /// </summary>
    public class TallyAppConfiguration
    {
        /// <summary>
        /// Configuration for each tally
        /// </summary>
        public Dictionary<int, TallyConfiguration> TallyConfigurations { get; set; } = new Dictionary<int, TallyConfiguration>();
    }
}
