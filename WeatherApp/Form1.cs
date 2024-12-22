using System;
// ���������� ����������, ����������� ��� ������ � �����������, �������� ��������, HTTP-���������,
// JSON-���������� � ������� Windows Forms.
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
    // ������� ����� ����������, ������� ��������� ���������������� �����������
    public partial class Form1 : Form
    {
        // API ���� ��� ������� � OpenWeatherMap
        private readonly string apiKey = "b30921ec23d5c88c96c22616fb2f3933";

        // URL ��� ��������� ������ � ������
        private readonly string apiUrl = "https://api.openweathermap.org/data/2.5/weather";

        // ���� � �����, ����������� ������ � �������
        private readonly string cityPath = "C:\\Users\\CYBORG\\source\\repos\\lab09\\lab09\\files\\city.txt";

        // ������ �������� �������
        private readonly List<City> _cities = new List<City>();

        // ����������� �����, ���������� ��� �� ��������
        public Form1()
        {
            InitializeComponent(); // ������������� ����������� �����
            LoadCitiesAsync(cityPath); // ����������� �������� ������� �� �����
        }

        // ����� ��� ����������� �������� ������ � �������
        private async Task LoadCitiesAsync(string filePath)
        {
            try
            {
                // ������ ���� ����� �� �����
                var cities = File.ReadAllLines(filePath);

                foreach (var city in cities)
                {
                    // ��������� ������ �� �������� ������ � ����������
                    var parts = city.Split('\t');
                    if (parts.Length == 2)
                    {
                        var name = parts[0];
                        var coord = parts[1].Replace(" ", "").Split(',');

                        // ����������� ���������� � �������� ������
                        var latitude = Convert.ToDouble(coord[0].Replace(".", ","));
                        var longitude = Convert.ToDouble(coord[1].Replace(".", ","));

                        // ������� ������ City � ��������� � ������
                        var info = new City(name, latitude, longitude);
                        _cities.Add(info);
                    }
                }

                // ��������� ���������� ������ ComboBox ������� �� ������ �������
                CityComboBox.DataSource = _cities;
            }
            catch (Exception ex)
            {
                // ���������� ��������� �� ������, ���� ���-�� ����� �� ���
                MessageBox.Show($"Error loading cities: {ex.Message}");
            }
        }

        // ���������� ������� ������� �� ������ "Get Weather"
        private async void GetWeatherButton_Click(object sender, EventArgs e)
        {
            // ���������, ������ �� ����� � ���������� ������
            if (CityComboBox.SelectedItem is City selectedCity)
            {
                try
                {
                    // �������� ������ � ������ ��� ���������� ������
                    var weather = await FetchWeatherAsync(apiUrl, selectedCity);

                    if (weather != null)
                    {
                        // ���� ������ ������� ��������, ���������� �� � ��������� ����
                        ResulttextBox.Text = weather.ToString();
                    }
                    else
                    {
                        // ���� ������ �� ������� ��������, ������� ���������
                        MessageBox.Show("Failed fetching weather data. Try again later.");
                    }
                }
                catch (Exception ex)
                {
                    // ������������ ���������� � ������� ��������� �� ������
                    MessageBox.Show($"Error occurred: {ex.Message}");
                }
            }
            else
            {
                // ���� ����� �� ������, ������ ������������ ������� ���
                MessageBox.Show("Please choose a city");
            }
        }

        // ����� ��� ��������� ������ � ������ � API
        private async Task<Weather> FetchWeatherAsync(string URL, City city)
        {
            try
            {
                // ������� HTTP-������ ��� �������� ��������
                HttpClient client = new HttpClient
                {
                    BaseAddress = new Uri(URL)
                };

                // ������� ��������� ������� � ������, ��� ������� ����� � ������� JSON
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                // ��������� URL � ����������� (���������� ������, API-���� � ������� ���������)
                var urlParameters = $"?lat={city.Latitude}&lon={city.Longitude}&appid={apiKey}&units=metric";
                var fullUrl = URL + urlParameters;

                // ���������� GET-������ � API
                var response = await client.GetAsync(fullUrl);

                if (response.IsSuccessStatusCode)
                {
                    // ���� ������ �������, ������ ���������� ������ ��� ������
                    var responseString = await response.Content.ReadAsStringAsync();

                    // ������ ������ JSON � ������
                    var json = JsonObject.Parse(responseString);

                    // ��������� ����������� ������ � ������� ������ Weather
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
                    // ���� ������ ���������� � �������, ������� ���������
                    MessageBox.Show($"API error: {response.StatusCode}");
                }
                return null;
            }
            catch (Exception ex)
            {
                // ������������ ���������� � ������� ��������� �� ������
                MessageBox.Show($"Failed fetching weather data: {ex.Message}");
                return null;
            }
        }
    }

    // ����� ��� ������������� ������
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
            return Name; // ���������� �������� ������ � ���������� ������
        }
    }

    // ����� ��� ������������� ������ � ������
    public class Weather
    {
        public string Country { get; set; }
        public string Name { get; set; }
        public double Temp { get; set; }
        public string Description { get; set; }

        // ����������� � �����������
        public Weather(string country, string name, double temp, string description)
        {
            Country = country;
            Name = name;
            Temp = temp;
            Description = description;
        }

        // ������ ����������� ��� ��������
        public Weather()
        {
        }

        public override string ToString()
        {
            // ��������������� ����� ������ � ������
            return $"Country: {Country}, City: {Name}, Temperature: {Temp} �C, Description: {Description}";
        }
    }
}
