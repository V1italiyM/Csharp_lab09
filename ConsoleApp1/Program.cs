// Указываем путь к файлу, содержащему данные о городах
string cityPath = "C:\\Users\\CYBORG\\source\\repos\\lab09\\lab09\\files\\city.txt";

// Асинхронный метод для загрузки данных о городах из файла
async Task LoadCitiesAsync(string filePath)
{
    // Попытка чтения данных из файла
    try
    {
        // Считываем все строки из файла в массив строк
        var cities = File.ReadAllLines(filePath);

        // Проходим по каждой строке файла
        foreach (var city in cities)
        {
            // Разделяем строку на части, используя табуляцию (\t) в качестве разделителя
            var parts = city.Split('\t');

            // Берем первую часть строки — название города
            var name = parts[0];

            // Берем вторую часть строки — координаты (широта и долгота),
            // и разделяем их по запятой с пробелом
            var coord = parts[1].Split(", ");

            // Для отладки выводим долготу на экран
            Console.WriteLine($"'{coord[1]}'");

            // Конвертируем широту из строки в число, заменяя точку на запятую
            // (это нужно для правильной обработки чисел в некоторых локалях)
            Console.WriteLine($"Hey ----{Convert.ToDouble(coord[0].Replace('.', ','))}");

            // Аналогично для долготы
            Console.WriteLine($"Hey ----{Convert.ToDouble(coord[1].Replace('.', ','))}");

            // Выводим всю строку целиком для отладки
            Console.WriteLine(city);

            // Выводим имя города и координаты
            Console.WriteLine(name, coord);

            // Преобразуем широту и долготу в числа с плавающей запятой
            var latitude = Convert.ToDouble(coord[0].Trim());
            var longitude = Convert.ToDouble(coord[1].Trim());

            // Выводим название города и его координаты
            Console.WriteLine(name, latitude, longitude);
        }
    }
    // Обрабатываем исключения, если они возникнут при чтении файла или обработке данных
    catch (Exception ex)
    {
        Console.WriteLine($"Error loading cities: {ex.Message}");
    }
}

// Вызываем метод для загрузки данных о городах, передавая путь к файлу
LoadCitiesAsync(cityPath);
