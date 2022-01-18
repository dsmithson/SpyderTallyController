namespace SpyderTallyControllerWebApp.Models
{
    public interface IRelayRepository
    {
        event EventHandler RelayStatusChanged;

        bool[] GetRelayStatus();

        bool GetRelayStatus(int index);

        void SetRelayStatus(int relayIndex, bool value);
    }
}
