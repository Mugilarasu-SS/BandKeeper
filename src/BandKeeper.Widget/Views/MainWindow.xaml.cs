using System.Windows;
using BandKeeper.Widget.ViewModels;

namespace BandKeeper.Widget.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new WidgetViewModel();
    }
}
