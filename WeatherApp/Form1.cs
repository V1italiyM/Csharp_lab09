using System;
// Ïîäêëþ÷àåì áèáëèîòåêè, íåîáõîäèìûå äëÿ ðàáîòû ñ êîëëåêöèÿìè, ôàéëîâîé ñèñòåìîé, HTTP-çàïðîñàìè,
// JSON-îáðàáîòêîé è ôîðìàìè Windows Forms.
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
    // Ãëàâíàÿ ôîðìà ïðèëîæåíèÿ, êîòîðàÿ óïðàâëÿåò ïîëüçîâàòåëüñêèì èíòåðôåéñîì
    public partial class Form1 : Form
    {
        // API êëþ÷ äëÿ äîñòóïà ê OpenWeatherMap
        private readonly string apiKey = "";

        // URL äëÿ ïîëó÷åíèÿ äàííûõ î ïîãîäå
        private readonly string apiUrl = "https://api.openweathermap.org/data/2.5/weather";

        // Ïóòü ê ôàéëó, ñîäåðæàùåìó äàííûå î ãîðîäàõ
        private readonly string cityPath = "C:\\Users\\CYBORG\\source\\repos\\lab09\\lab09\\files\\city.txt";

        // Ñïèñîê îáúåêòîâ ãîðîäîâ
        private readonly List<City> _cities = new List<City>();

        // Êîíñòðóêòîð ôîðìû, âûçûâàåòñÿ ïðè åå ñîçäàíèè
        public Form1()
        {
            InitializeComponent(); // Èíèöèàëèçàöèÿ êîìïîíåíòîâ ôîðìû
            LoadCitiesAsync(cityPath); // Àñèíõðîííàÿ çàãðóçêà ãîðîäîâ èç ôàéëà
        }

        // Ìåòîä äëÿ àñèíõðîííîé çàãðóçêè äàííûõ î ãîðîäàõ
        private async Task LoadCitiesAsync(string filePath)
        {
            try
            {
                // ×òåíèå âñåõ ñòðîê èç ôàéëà
                var cities = File.ReadAllLines(filePath);

                foreach (var city in cities)
                {
                    // Ðàçäåëÿåì ñòðîêó íà íàçâàíèå ãîðîäà è êîîðäèíàòû
                    var parts = city.Split('\t');
                    if (parts.Length == 2)
                    {
                        var name = parts[0];
                        var coord = parts[1].Replace(" ", "").Split(',');

                        // Ïðåîáðàçóåì êîîðäèíàòû â ÷èñëîâîé ôîðìàò
                        var latitude = Convert.ToDouble(coord[0].Replace(".", ","));
                        var longitude = Convert.ToDouble(coord[1].Replace(".", ","));

                        // Ñîçäàåì îáúåêò City è äîáàâëÿåì â ñïèñîê
                        var info = new City(name, latitude, longitude);
                        _cities.Add(info);
                    }
                }

                // Çàïîëíÿåì âûïàäàþùèé ñïèñîê ComboBox äàííûìè èç ñïèñêà ãîðîäîâ
                CityComboBox.DataSource = _cities;
            }
            catch (Exception ex)
            {
                // Îòîáðàæàåì ñîîáùåíèå îá îøèáêå, åñëè ÷òî-òî ïîøëî íå òàê
                MessageBox.Show($"Error loading cities: {ex.Message}");
            }
        }

        // Îáðàáîò÷èê ñîáûòèÿ íàæàòèÿ íà êíîïêó "Get Weather"
        private async void GetWeatherButton_Click(object sender, EventArgs e)
        {
            // Ïðîâåðÿåì, âûáðàí ëè ãîðîä â âûïàäàþùåì ñïèñêå
            if (CityComboBox.SelectedItem is City selectedCity)
            {
                try
                {
                    // Ïîëó÷àåì äàííûå î ïîãîäå äëÿ âûáðàííîãî ãîðîäà
                    var weather = await FetchWeatherAsync(apiUrl, selectedCity);

                    if (weather != null)
                    {
                        // Åñëè äàííûå óñïåøíî ïîëó÷åíû, îòîáðàæàåì èõ â òåêñòîâîì ïîëå
                        ResulttextBox.Text = weather.ToString();
                    }
                    else
                    {
                        // Åñëè äàííûå íå óäàëîñü ïîëó÷èòü, âûâîäèì ñîîáùåíèå
                        MessageBox.Show("Failed fetching weather data. Try again later.");
                    }
                }
                catch (Exception ex)
                {
                    // Îáðàáàòûâàåì èñêëþ÷åíèÿ è âûâîäèì ñîîáùåíèå îá îøèáêå
                    MessageBox.Show($"Error occurred: {ex.Message}");
                }
            }
            else
            {
                // Åñëè ãîðîä íå âûáðàí, ïðîñèì ïîëüçîâàòåëÿ âûáðàòü åãî
                MessageBox.Show("Please choose a city");
            }
        }

        // Ìåòîä äëÿ ïîëó÷åíèÿ äàííûõ î ïîãîäå ñ API
        private async Task<Weather> FetchWeatherAsync(string URL, City city)
        {
            try
            {
                // Ñîçäàåì HTTP-êëèåíò äëÿ îòïðàâêè çàïðîñîâ
                HttpClient client = new HttpClient
                {
                    BaseAddress = new Uri(URL)
                };

                // Î÷èùàåì çàãîëîâêè êëèåíòà è çàäàåì, ÷òî îæèäàåì îòâåò â ôîðìàòå JSON
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                // Ôîðìèðóåì URL ñ ïàðàìåòðàìè (êîîðäèíàòû ãîðîäà, API-êëþ÷ è åäèíèöû èçìåðåíèÿ)
                var urlParameters = $"?lat={city.Latitude}&lon={city.Longitude}&appid={apiKey}&units=metric";
                var fullUrl = URL + urlParameters;

                // Îòïðàâëÿåì GET-çàïðîñ ê API
                var response = await client.GetAsync(fullUrl);

                if (response.IsSuccessStatusCode)
                {
                    // Åñëè çàïðîñ óñïåøåí, ÷èòàåì ñîäåðæèìîå îòâåòà êàê ñòðîêó
                    var responseString = await response.Content.ReadAsStringAsync();

                    // Ïàðñèì ñòðîêó JSON â îáúåêò
                    var json = JsonObject.Parse(responseString);

                    // Èçâëåêàåì íåîáõîäèìûå äàííûå è ñîçäàåì îáúåêò Weather
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
                    // Åñëè çàïðîñ çàâåðøèëñÿ ñ îøèáêîé, âûâîäèì ñîîáùåíèå
                    MessageBox.Show($"API error: {response.StatusCode}");
                }
                return null;
            }
            catch (Exception ex)
            {
                // Îáðàáàòûâàåì èñêëþ÷åíèÿ è âûâîäèì ñîîáùåíèå îá îøèáêå
                MessageBox.Show($"Failed fetching weather data: {ex.Message}");
                return null;
            }
        }
    }

    // Êëàññ äëÿ ïðåäñòàâëåíèÿ ãîðîäà
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
            return Name; // Îòîáðàæàåì íàçâàíèå ãîðîäà â âûïàäàþùåì ñïèñêå
        }
    }

    // Êëàññ äëÿ ïðåäñòàâëåíèÿ äàííûõ î ïîãîäå
    public class Weather
    {
        public string Country { get; set; }
        public string Name { get; set; }
        public double Temp { get; set; }
        public string Description { get; set; }

        // Êîíñòðóêòîð ñ ïàðàìåòðàìè
        public Weather(string country, string name, double temp, string description)
        {
            Country = country;
            Name = name;
            Temp = temp;
            Description = description;
        }

        // Ïóñòîé êîíñòðóêòîð äëÿ óäîáñòâà
        public Weather()
        {
        }

        public override string ToString()
        {
            // Ôîðìàòèðîâàííûé âûâîä äàííûõ î ïîãîäå
            return $"Country: {Country}, City: {Name}, Temperature: {Temp} °C, Description: {Description}";
        }
    }
}
