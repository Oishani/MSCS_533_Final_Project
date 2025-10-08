using Microsoft.Maui.Controls;

namespace GeoHeatmap;

public partial class App : Application
{
    public App(MapPage mapPage)
    {
        InitializeComponent();
        MainPage = new NavigationPage(mapPage);
    }
}