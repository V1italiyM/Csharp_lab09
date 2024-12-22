using Newtonsoft.Json.Linq; // Библиотека для работы с JSON, используется для парсинга данных.
using System.ComponentModel; // Предоставляет классы для реализации компонентов и их свойств.
using System.Net.Http.Headers; // Для работы с заголовками HTTP-запросов.
using System.Text.Json; // Для сериализации и десериализации JSON.

public class StockData
{
    // Свойства, соответствующие полям JSON-ответа от API.
    public string s { get; set; } // Символ тикера (например, AAPL для Apple).
    public List<double> c { get; set; } // Список закрытых цен.
    public List<double> h { get; set; } // Список максимальных цен.
    public List<double> l { get; set; } // Список минимальных цен.
    public List<double> o { get; set; } // Список открытых цен.
    public List<int> t { get; set; } // Список меток времени.
    public List<int> v { get; set; } // Список объемов торгов.
}

class Market
{
    static readonly Mutex mutex = new Mutex(); // Мьютекс для синхронизации доступа к файлу при записи.

    // Асинхронное чтение данных из файла в список строк.
    static async Task ReadFileAsync(List<string> massive, string filePath)
    {
        using (StreamReader sr = new StreamReader(filePath))
        {
            string line;
            // Считываем файл построчно и добавляем каждую строку в список.
            while ((line = await sr.ReadLineAsync()) != null)
            {
                massive.Add(line);
            }
        }
    }

    // Запись строки в файл с использованием мьютекса.
    static void WriteToFile(string filePath, string text)
    {
        if (!File.Exists(filePath))
        {
            File.Create(filePath); // Создаем файл, если он не существует.
        }
        mutex.WaitOne(); // Захватываем мьютекс для предотвращения параллельной записи.
        try
        {
            File.AppendAllText(filePath, text + Environment.NewLine); // Добавляем текст в файл.
        }
        finally
        {
            mutex.ReleaseMutex(); // Освобождаем мьютекс.
        }
    }

    // Вычисление средней цены на основе максимальных и минимальных цен.
    static double calculateAvgPrice(List<double> highPrice, List<double> lowPrice)
    {
        double totalAvgPrice = 0;
        for (int i = 0; i < highPrice.Count; i++)
        {
            // Среднее арифметическое для каждой пары (максимальная и минимальная цена).
            totalAvgPrice += (highPrice[i] + lowPrice[i]) / 2;
        }
        return totalAvgPrice / highPrice.Count; // Возвращаем общее среднее значение.
    }

    // Получение данных с API и запись средней цены в файл.
    static async Task GetData(HttpClient client, string quote, string startDate, string endDate, string output)
    {
        // Формируем URL запроса.
        string apiKey = "YlJIQkZUMFM3V0c4VFphYmh1RzlJbDVjY2lKbFJJdEdMa2t6U0hYUHBTdz0"; // API-ключ для авторизации.
        string URL = $"https://api.marketdata.app/v1/stocks/candles/D/{quote}/?from={startDate}&to={endDate}&token={apiKey}";

        HttpClient cl = new HttpClient();
        HttpResponseMessage response = cl.GetAsync(URL).Result; // Выполняем запрос к API.

        if (!response.IsSuccessStatusCode)
        {
            // Если запрос завершился с ошибкой, выводим сообщение.
            Console.WriteLine($"Error! Status code: {response.StatusCode}");
        }
        else
        {
            // Парсим JSON-ответ от API.
            var json = await response.Content.ReadAsStringAsync();
            var data = JsonSerializer.Deserialize<StockData>(json);

            if (data != null && data.h != null && data.l != null)
            {
                // Если данные успешно получены, вычисляем среднюю цену.
                double avgPrice = calculateAvgPrice(data.h, data.l);
                string result = $"{quote}:{avgPrice}"; // Формируем строку для записи в файл.
                WriteToFile(output, result); // Записываем результат в файл.
                Console.WriteLine($"Average price for {quote}: {avgPrice}"); // Выводим результат.
            }
            else
            {
                // Если данных недостаточно, выводим сообщение.
                Console.WriteLine($"Not enough data for {quote}");
            }
        }
    }

    // Основной метод программы.
    static async Task Main(string[] args)
    {
        string tickerPath = "C:\\Users\\CYBORG\\source\\repos\\lab09\\lab09\\files\\ticker.txt"; // Путь к файлу с тикерами.
        string outputPath = "C:\\Users\\CYBORG\\source\\repos\\lab09\\lab09\\files\\output.txt"; // Путь к файлу для записи результата.

        List<string> ticker = []; // Список для хранения тикеров.
        await ReadFileAsync(ticker, tickerPath); // Считываем тикеры из файла.

        // Формируем даты для запросов (прошлый год).
        DateTime endDateTime = DateTime.Now;
        string endDate = endDateTime.AddMonths(-1).ToString("yyyy-MM-dd"); // Конечная дата — месяц назад.
        string startDate = endDateTime.AddYears(-1).AddMonths(1).ToString("yyyy-MM-dd"); // Начальная дата — год назад.

        Console.WriteLine($"Start Date: {startDate}"); // Вывод начальной даты.
        Console.WriteLine($"End Date: {endDate}"); // Вывод конечной даты.

        HttpClient client = new HttpClient(); // Инициализация клиента HTTP.
        client.DefaultRequestHeaders.Clear(); // Очистка заголовков запроса.
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json")); // Устанавливаем тип контента.

        List<Task> tasks = new List<Task>(); // Список задач для параллельных запросов.

        // Для каждого тикера формируем запрос к API.
        foreach (var quote in ticker)
        {
            tasks.Add(GetData(client, quote, startDate, endDate, outputPath)); // Добавляем задачу в список.
        }
        await Task.WhenAll(tasks); // Ожидаем завершения всех задач.
    }
}