namespace SpyderTallyControllerWebApp.Models
{
    public enum DisplayMode {  Normal, TwoLineManual }
    public interface IDisplayRepository
    {
        void SetDisplayMode(DisplayMode mode);

        void SetText(string line1, string line2);
    }
}
