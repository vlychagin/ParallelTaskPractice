using System.Collections.ObjectModel;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Newtonsoft.Json;
using ParallelTaskPractice.Models;

namespace ParallelTaskPractice.ViewModels;

public partial class AppViewModel
{

    // При помощи WebClient загрузите файл с этой ссылки
    // https://github.com/twbs/bootstrap/releases/download/v4.6.1/bootstrap-4.6.1-examples.zip
    // (это CSS-фреймворк Bootstrap).
    private void DownloadByWebClientExec(object o) {
        // класс для загрузки файлов из интернет
        var webClient = new WebClient();

        // адрес для скачивания
        var uri = "https://github.com/twbs/bootstrap/releases/download/v4.6.1/bootstrap-4.6.1-examples.zip";

        // получим имя локального файла из uri: строка после последнего / 
        // т.е. последний элемент массива строк 
        var fileName = uri.Split('/', StringSplitOptions.RemoveEmptyEntries)[^1];

        // полное имя локального файла с путем для сохраенния
        var path = @$"..\..\..\App_Data\{fileName}";

        HostWindow.TblStatus.Text = $"Начата загрузка с URI {uri}";

        // простейшая команда загрузки файла, используем URI, имя локального файла
        webClient.DownloadFile(uri, path);

        HostWindow.TblResult.Text = $"\nФайл {fileName} загружен, скачано, байт: ${new FileInfo(path).Length}";
        HostWindow.TblStatus.Text = $"Завершена загрузка с URI {uri}";
    } // DownloadByWebClientExec


    // Скачать файл при помощи WebRequest и распарсить по заданию
    private void DownloadByWebRequestExec(object o) {
        string uri = "https://www.newtonsoft.com/json";
        HostWindow.TblResult.Text = $"Начата загрузка страницы {uri}\n";

        // 1. Создать и отправить серверу запрос
        WebRequest webRequest = WebRequest.Create(uri);

        // 2. ждать ответ сервера, получить ссылку на ответ сервера
        WebResponse webResponse = webRequest.GetResponse();

        var sb = new StringBuilder();

        // 3. получение данных от сервера, обработка данных
        using (var srd = new StreamReader(webResponse.GetResponseStream())) {
            // чтение данных от сервера
            string str;
            while ((str = srd.ReadLine()!) != null) {
                sb.AppendLine(str);
            } // while
        } // using srd

        // 4. закрыть соединение
        webResponse.Close();
        HostWindow.TblResult.Text += $"Завершена загрузка страницы {uri}\n";

        // Главная страница обычно имеет имя index.html
        var fileName = "index.html";
        var path = $"../../../App_Data/{fileName}";
        var page = sb.ToString();
        File.WriteAllText(path, page, Encoding.UTF8);
        HostWindow.TblResult.Text += $"Загруженная страница сохранена в файл {path}\n\n"; 

        // Подсчитаем количество символов < и > в загруженной странице
        int lts = page.Count(c => c == '<');
        int gts = page.Count(c => c == '>');
        HostWindow.TblResult.Text += $$"""
             На странице найдено символов '<': {{lts}}
             На странице найдено символов '>': {{gts}}
             Всего символов '<' и '>': {{lts + gts}}
             """;
    } // DownloadByWebRequestExec


    // Скачать курс валют через WebAPI ЦРБ, формат JSON
    private void DownloadValutesExec(object o) {
        var uri = "https://www.cbr-xml-daily.ru/daily_json.js";

        // запрос к серверу
        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);

        // синхронное получение ответа, блокирующий вызов
        HttpWebResponse response = (HttpWebResponse)request.GetResponse();

        using var srd = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
        string str;
        var sbJson = new StringBuilder();
        while ((str = srd.ReadLine()!) != null) {
            sbJson.AppendLine(str);
        } // while

        // Задать имя для файла JSON с курсом валют
        var fileName = uri.Split('/', StringSplitOptions.RemoveEmptyEntries)[^1] + "on";
        var path = $"../../../App_Data/{fileName}";
        var json = sbJson.ToString();
        File.WriteAllText(path, json, Encoding.UTF8);

        Valutes = ValuteApi.FromJson(json).Valutes.Values.ToList();

        HostWindow.TblStatus.Text = $"Загружен курс валют ЦРБ, файл {path}";
        HostWindow.TbcMain.SelectedIndex = 1;
        
        HostWindow.TblTask2Header.FontWeight = FontWeights.Bold;
        HostWindow.TblTask1Header.FontWeight = 
        HostWindow.TblTask2HeaderFilter.FontWeight = 
        HostWindow.TblTask2HeaderOrderByValueDesc.FontWeight =
        HostWindow.TblTask2HeaderOrderByName.FontWeight = FontWeights.Normal;

        HostWindow.DtgValutes.ItemsSource = new ObservableCollection<Valute>(Valutes);
    } // DownloadValutesExec

    private void Task2CommandExec(object o) {
        Task<List<Valute>> taskFilter = new Task<List<Valute>>(Filter);
        taskFilter.ContinueWith(ShowFiltered);

        Task<List<Valute>> taskOrderByValueDesc = new Task<List<Valute>>(OrderByValueDesc);
        taskOrderByValueDesc.ContinueWith(ShowOrderedByValueDesc);

        Task<List<Valute>> taskOrderByName = new Task<List<Valute>>(OrderByName);
        taskOrderByName.ContinueWith(ShowOrderedByName);

        Parallel.Invoke(
            () => taskFilter.Start(),
            () => taskOrderByValueDesc.Start(),
            () => taskOrderByName.Start()
        );
    } // Task2CommandExec

    #region Методы для запуска в задачах

    // получить копию коллекции из файла fileName
    private List<Valute> GetValutesCopy(string fileName) {
        var path = $"../../../App_Data/{fileName}";
        if (!File.Exists(path)) {
            throw new FileNotFoundException($"Нет файла {path}, повторите загрузку с ЦРБ");
        } // if

        Task.Delay(3_000).Wait(3_000);

        var json = File.ReadAllText(path);
        return JsonConvert.DeserializeObject<ValuteApi>(json)!.Valutes.Values.ToList();
    } // GetValutesCopy

    // В копии коллекции найти и отобразить валюты, курс которых выроc
    private List<Valute> Filter() => GetValutesCopy("daily_json.json")
        // .Where(v => v.Previous < v.Value)
        // для проверки выбеорем валюты с курсом в интервале [50, 60]
        .Where(v => 50 <= v.Value && v.Value <= 60)
        .OrderBy(v => v.CharCode)
        .ToList();

    // Упорядочить копию коллекции валют по убыванию значений курса
    private List<Valute> OrderByValueDesc() => GetValutesCopy("daily_json.json")
        .OrderByDescending(v => v.Value)
        .ToList();

    // Упорядочить копию коллекции валют по названиям
    private List<Valute> OrderByName() => GetValutesCopy("daily_json.json")
        .OrderBy(v => v.Name)
        .ToList();
    #endregion

    #region Задачи продолжения для вывода результатов

    // вывод отобранных валют в DataGrid DtgValutesFiltered
    private void ShowFiltered(Task<List<Valute>> task) {
        List<Valute> valutes = task.Result;

        HostWindow.Dispatcher.Invoke(
            () => {
                HostWindow.TbcMain.SelectedIndex = 2;

                HostWindow.TblTask2HeaderFilter.FontWeight = FontWeights.Bold;

                HostWindow.TblTask1Header.FontWeight = 
                HostWindow.TblTask2Header.FontWeight = 
                HostWindow.TblTask2HeaderOrderByValueDesc.FontWeight = 
                HostWindow.TblTask2HeaderOrderByName.FontWeight = FontWeights.Normal;

                HostWindow.DtgValutesFiltered.ItemsSource = new ObservableCollection<Valute>(valutes);
            }, 
            DispatcherPriority.Normal
        );
    }

    private void ShowOrderedByValueDesc(Task<List<Valute>> task) {
        List<Valute> valutes = task.Result;

        HostWindow.Dispatcher.Invoke(
            () => {
                HostWindow.TbcMain.SelectedIndex = 3;

                HostWindow.TblTask2HeaderOrderByValueDesc.FontWeight = FontWeights.Bold;

                HostWindow.TblTask1Header.FontWeight = 
                HostWindow.TblTask2Header.FontWeight =
                HostWindow.TblTask2HeaderFilter.FontWeight =
                HostWindow.TblTask2HeaderOrderByName.FontWeight = FontWeights.Normal;

                HostWindow.DtgValutesOrderByValueDesc.ItemsSource = new ObservableCollection<Valute>(valutes);
            },
            DispatcherPriority.Normal
        );
    }

    private void ShowOrderedByName(Task<List<Valute>> task) {
        List<Valute> valutes = task.Result;

        HostWindow.Dispatcher.Invoke(
            () => {
                HostWindow.TbcMain.SelectedIndex = 3;

                HostWindow.TblTask2HeaderOrderByName.FontWeight = FontWeights.Bold;

                HostWindow.TblTask1Header.FontWeight =
                HostWindow.TblTask2Header.FontWeight =
                HostWindow.TblTask2HeaderFilter.FontWeight =
                HostWindow.TblTask2HeaderOrderByValueDesc.FontWeight = FontWeights.Normal;

                HostWindow.DtgValutesOrderByName.ItemsSource = new ObservableCollection<Valute>(valutes);
            },
            DispatcherPriority.Normal
        );
    }
    #endregion
} // class AppViewModel 
