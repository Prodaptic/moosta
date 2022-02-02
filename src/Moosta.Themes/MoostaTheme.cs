using MudBlazor;

namespace Moosta.Themes
{
    public class MoostaTheme : MudTheme
    {
        private const string _primaryOrange = "#FF914D";
        public MoostaTheme()
        {
            Palette = new Palette()
            {
                Primary = _primaryOrange,
                AppbarBackground = _primaryOrange
            };

            Shadows = new Shadow();
            Typography = new Typography();
            LayoutProperties = new LayoutProperties();
            ZIndex = new ZIndex();
        }
    }
}