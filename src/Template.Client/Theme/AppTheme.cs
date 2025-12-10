using MudBlazor;

namespace Template.Client.Theme;

public static class AppTheme
{
    public static MudTheme DefaultTheme = new()
    {
        PaletteLight = new PaletteLight()
        {
            Primary = "#594AE2",
            Secondary = "#FF4081",
            AppbarBackground = "#594AE2",
        },
        PaletteDark = new PaletteDark()
        {
            Primary = "#776be7",
            Secondary = "#FF4081",
            AppbarBackground = "#1b1f22",
            Background = "#1b1f22",
            Surface = "#212529",
            DrawerBackground = "#15171a",
        }
    };
}
