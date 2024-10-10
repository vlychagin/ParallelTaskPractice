using System.Collections.ObjectModel;
using System.IO;
using System.Net;
using System.Text;
using System.Windows;
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
        HostWindow.TblTask1Header.FontWeight = FontWeights.Normal;
        HostWindow.TblTask2Header.FontWeight = FontWeights.Bold;
        HostWindow.DtgValutes.ItemsSource = new ObservableCollection<Valute>(Valutes);
    } // DownloadValutesExec
} // class AppViewModel
