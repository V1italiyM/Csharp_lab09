using System;
// Подключаем библиотеки, необходимые для работы с коллекциями, файловой системой, HTTP-запросами,
// JSON-обработкой и формами Windows Forms.
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WeatherApp
{
    // Главная форма приложения, которая управляет пользовательским интерфейсом
    public partial class Form1 : Form
    {
        // API ключ для доступа к OpenWeatherMap
        private readonly string apiKey = "b30921ec23d5c88c96c22616fb2f3933";

        // URL для получения данных о погоде
        private readonly string apiUrl = "https://api.openweathermap.org/data/2.5/weather";

        // Путь к файлу, содержащему данные о городах
        private readonly string cityPath = "C:\\Users\\CYBORG\\source\\repos\\lab09\\lab09\\files\\city.txt";

        // Список объектов городов
        private readonly List<City> _cities = new List<City>();

        // Конструктор формы, вызывается при ее создании
        public Form1()
        {
            InitializeComponent(); // Инициализация компонентов формы
            LoadCitiesAsync(cityPath); // Асинхронная загрузка городов из файла
        }

        // Метод для асинхронной загрузки данных о городах
        private async Task LoadCitiesAsync(string filePath)
        {
            try
            {
                // Чтение всех строк из файла
                var cities = File.ReadAllLines(filePath);

                foreach (var city in cities)
                {
                    // Разделяем строку на название города и координаты
                    var parts = city.Split('\t');
                    if (parts.Length == 2)
                    {
                        var name = parts[0];
                        var coord = parts[1].Replace(" ", "").Split(',');

                        // Преобразуем координаты в числовой формат
                        var latitude = Convert.ToDouble(coord[0].Replace(".", ","));
                        var longitude = Convert.ToDouble(coord[1].Replace(".", ","));

                        // Создаем объект City и добавляем в список
                        var info = new City(name, latitude, longitude);
                        _cities.Add(info);
                    }
                }

                // Заполняем выпадающий список ComboBox данными из списка городов
                CityComboBox.DataSource = _cities;
            }
            catch (Exception ex)
            {
                // Отображаем сообщение об ошибке, если что-то пошло не так
                MessageBox.Show($"Error loading cities: {ex.Message}");
            }
        }

        // Обработчик события нажатия на кнопку "Get Weather"
        private async void GetWeatherButton_Click(object sender, EventArgs e)
        {
            // Проверяем, выбран ли город в выпадающем списке
            if (CityComboBox.SelectedItem is City selectedCity)
            {
                try
                {
                    // Получаем данные о погоде для выбранного города
                    var weather = await FetchWeatherAsync(apiUrl, selectedCity);

                    if (weather != null)
                    {
                        // Если данные успешно получены, отображаем их в текстовом поле
                        ResulttextBox.Text = weather.ToString();
                    }
                    else
                    {
                        // Если данные не удалось получить, выводим сообщение
                        MessageBox.Show("Failed fetching weather data. Try again later.");
                    }
                }
                catch (Exception ex)
                {
                    // Обрабатываем исключения и выводим сообщение об ошибке
                    MessageBox.Show($"Error occurred: {ex.Message}");
                }
            }
            else
            {
                // Если город не выбран, просим пользователя выбрать его
                MessageBox.Show("Please choose a city");
            }
        }

        // Метод для получения данных о погоде с API
        private async Task<Weather> FetchWeatherAsync(string URL, City city)
        {
            try
            {
                // Создаем HTTP-клиент для отправки запросов
                HttpClient client = new HttpClient
                {
                    BaseAddress = new Uri(URL)
                };

                // Очищаем заголовки клиента и задаем, что ожидаем ответ в формате JSON
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                // Формируем URL с параметрами (координаты города, API-ключ и единицы измерения)
                var urlParameters = $"?lat={city.Latitude}&lon={city.Longitude}&appid={apiKey}&units=metric";
                var fullUrl = URL + urlParameters;

                // Отправляем GET-запрос к API
                var response = await client.GetAsync(fullUrl);

                if (response.IsSuccessStatusCode)
                {
                    // Если запрос успешен, читаем содержимое ответа как строку
                    var responseString = await response.Content.ReadAsStringAsync();

                    // Парсим строку JSON в объект
                    var json = JsonObject.Parse(responseString);

                    // Извлекаем необходимые данные и создаем объект Weather
                    Weather res = new Weather
                    {
                        Country = (string)json["sys"]["country"],
                        Name = (string)json["name"],
                        Temp = (double)json["main"]["temp"],
                        Description = (string)json["weather"][0]["main"]
                    };

                    return res;
                }
                else
                {
                    // Если запрос завершился с ошибкой, выводим сообщение
                    MessageBox.Show($"API error: {response.StatusCode}");
                }
                return null;
            }
            catch (Exception ex)
            {
                // Обрабатываем исключения и выводим сообщение об ошибке
                MessageBox.Show($"Failed fetching weather data: {ex.Message}");
                return null;
            }
        }
    }

    // Класс для представления города
    public class City
    {
        public string Name { get; }
        public double Latitude { get; }
        public double Longitude { get; }

        public City(string name, double latitude, double longitude)
        {
            Name = name;
            Latitude = latitude;
            Longitude = longitude;
        }

        public override string ToString()
        {
            return Name; // Отображаем название города в выпадающем списке
        }
    }

    // Класс для представления данных о погоде
    public class Weather
    {
        public string Country { get; set; }
        public string Name { get; set; }
        public double Temp { get; set; }
        public string Description { get; set; }

        // Конструктор с параметрами
        public Weather(string country, string name, double temp, string description)
        {
            Country = country;
            Name = name;
            Temp = temp;
            Description = description;
        }

        // Пустой конструктор для удобства
        public Weather()
        {
        }

        public override string ToString()
        {
            // Форматированный вывод данных о погоде
            return $"Country: {Country}, City: {Name}, Temperature: {Temp} °C, Description: {Description}";
        }
    }
}
