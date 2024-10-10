using System.Windows;
using ParallelTaskPractice.ViewModels;

namespace ParallelTaskPractice.Windows;
/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private AppViewModel _appViewModel;

    public MainWindow() {
        InitializeComponent();

        // привязка данных окна к классу ApplicationViewModel 
        // реализация связи между View и ViewModel
        _appViewModel = new AppViewModel(this);
        DataContext = _appViewModel;
    } // MainWindow
} // class MainWindow
