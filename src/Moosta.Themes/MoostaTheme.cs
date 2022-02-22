using MudBlazor;

namespace Moosta.Themes
{
    public class MoostaTheme : MudTheme
    {
        private const string _primaryBlue = "#4FA1FF";
        private const string _secondaryBlue = "#1A3656";
        public MoostaTheme()
        {
            Palette = new Palette()
            {
                Primary = _primaryBlue,
                AppbarBackground = _primaryBlue
            };

            Shadows = new Shadow();
            Typography = new Typography();
            LayoutProperties = new LayoutProperties();
            ZIndex = new ZIndex();
        }
    }
}