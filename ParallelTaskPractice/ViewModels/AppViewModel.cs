using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using ParallelTaskPractice.Infrastructure;
using ParallelTaskPractice.Models;
using ParallelTaskPractice.Windows;

namespace ParallelTaskPractice.ViewModels;

public partial class AppViewModel: INotifyPropertyChanged
{
    public MainWindow HostWindow { get; set; }

    // коллекция курсов валют ЦРБ
    public List<Valute> Valutes { get; set; } = new();

    #region команды
    // Скачать файл при помощи WebClient
    public RelayCommand DownloadByWebClientCommand { get; set; }

    // Скачать файл при помощи WebRequest и распарсить по заданию
    public RelayCommand DownloadByWebRequestCommand { get; set; }

    // Скачать курс валют с сайта ЦРБ
    public RelayCommand DownloadValutesCommand { get; set; }

    // О приложении
    public RelayCommand AboutCommand { get; set; }

    // Выход
    public RelayCommand ExitCommand { get; set; }

    #endregion

    public AppViewModel(MainWindow hostWindow) {
        HostWindow = hostWindow;

        DownloadByWebClientCommand = new(DownloadByWebClientExec);
        DownloadByWebRequestCommand = new(DownloadByWebRequestExec);

        // Скачать курс валют с сайта ЦРБ
        DownloadValutesCommand = new(DownloadValutesExec);

        // Показать окно сведений о программе
        AboutCommand = new(o => { });

        ExitCommand = new(o => Application.Current.Shutdown(0));
    } // AppViewModel

    #region реализация интерфейса INotifyPropertyChanged
    // событие, зажигающееся при изменении свойства
    public event PropertyChangedEventHandler? PropertyChanged;
    public void OnPropertyChanged([CallerMemberName] string prop = "") {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    } // OnPropertyChanged
    #endregion

}