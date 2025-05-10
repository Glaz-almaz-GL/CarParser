using ClosedXML.Excel;
using Microsoft.AspNetCore.WebUtilities;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using Svg;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Security.AccessControl;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;

namespace CarParser
{
    public partial class Form1 : Form
    {
        IWebDriver driver;
        Dictionary<string, string> carBrendImages = new Dictionary<string, string>();
        string DirPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Images");
        int currentRow = 1; // Свойство для отслеживания текущей строки


        public Form1()
        {
            InitializeComponent();
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            try
            {
                await Task.Run(async () =>
                {
                    ChromeOptions options = new ChromeOptions();
                    options.AddArgument("--headless"); // Запуск в безголовом режиме
                    options.AddArgument("--disable-gpu"); // Отключает GPU для улучшения производительности в headless режиме
                    ChromeDriverService service = ChromeDriverService.CreateDefaultService();
                    service.HideCommandPromptWindow = true;

                    // Инициализация драйвера с указанием удаленного URL
                    driver = new ChromeDriver(service, options);
                    HashSet<string> carLinks;
                    HashSet<string> carDescriptionsLinks = new HashSet<string>();
                    carLinks = SearchCarModelsInMain();

                    // Создаем новый Excel файл
                    using (XLWorkbook workbook = new XLWorkbook())
                    {
                        IXLWorksheet worksheet = workbook.Worksheets.Add("Car Descriptions");
                        foreach (var carLink in carLinks)
                        {
                            carDescriptionsLinks.Clear(); // Очищаем коллекцию перед каждой итерацией
                            await SearchSubCars(carLink, carDescriptionsLinks);
                            foreach (var carDescriptionLink in carDescriptionsLinks)
                            {
                                Console.WriteLine(carDescriptionLink);
                                await CarDescriptionParser(driver, carDescriptionLink, worksheet);
                            }
                        }

                        await DownloadBrendImagesAsync(carBrendImages);

                        // Автоматическое подстраивание ширины столбцов
                        AutoFitColumns(worksheet);
                        // Сохраняем файл
                        workbook.SaveAs("CarDescriptions.xlsx");
                    }
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private async Task CarDescriptionParser(IWebDriver driver, string carDescriptionLink, IXLWorksheet worksheet)
        {
            try
            {
                // Navigate to the car description page
                await driver.Navigate().GoToUrlAsync(carDescriptionLink);
                // Wait for the table to load
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));
                wait.Until(d => d.FindElement(By.TagName("table")));
                var ids = ParseModelAndMakeIds(carDescriptionLink);
                string modelGroupId = "", makeId = "", modificationId = "";
                makeId = ids.makeId;
                modelGroupId = ids.modelGroupId;
                // Поиск контейнера .info-holder
                IWebElement infoHolder = driver.FindElement(By.ClassName("info-holder"));
                // Извлечение названия из <h2>
                IWebElement carNameElement = infoHolder.FindElement(By.TagName("h2"));
                string name = carNameElement.Text.Trim();
                IWebElement makeNameElement = driver.FindElement(By.ClassName("makeName"));
                string makeName = makeNameElement.Text.Trim();
                // Получение таблицы
                IWebElement table = driver.FindElement(By.TagName("table"));
                // Получение всех строк таблицы
                IList<IWebElement> rows = table.FindElements(By.TagName("tr"));
                // Инициализация списка для хранения данных
                List<List<string>> data = new List<List<string>>();
                List<string> modificationsIdData = new List<string>();
                // Проход по строкам
                foreach (IWebElement row in rows)
                {
                    string rawId = row.GetAttribute("data-url");
                    modificationId = ExtractTypeId(rawId);
                    UpdateUI($"Id марки: {makeId}, Id модели: {modelGroupId}, Id модификации: {modificationId}");
                    // Получение ячеек (<td>) в текущей строке
                    IList<IWebElement> cells = row.FindElements(By.TagName("td"));
                    // Создание списка для хранения данных текущей строки
                    List<string> rowData = new List<string>();
                    // Проход по ячейкам и извлечение текста
                    foreach (IWebElement cell in cells)
                    {
                        rowData.Add(cell.Text.Trim());
                    }
                    modificationsIdData.Add(modificationId);
                    // Добавление данных строки в общий список
                    data.Add(rowData);
                }
                // Запись данных в Excel
                if (currentRow == 1)
                {
                    // Если это первая строка, записываем заголовки
                    var headerRow = rows[0].FindElements(By.TagName("th"));
                    int lastI = 0;
                    for (int i = 0; i < headerRow.Count; i++)
                    {
                        worksheet.Cell(currentRow, i + 3).Value = headerRow[i].Text.Trim(); // Смещаем на 1 колонку вправо
                        lastI = i;
                    }
                    // Добавляем заголовок для нового столбца: "Модель"
                    worksheet.Cell(currentRow, 1).Value = "Изготовитель";
                    worksheet.Cell(currentRow, 2).Value = "Модель";
                    worksheet.Cell(currentRow, lastI + 4).Value = "Id марки";
                    worksheet.Cell(currentRow, lastI + 5).Value = "Id модели";
                    worksheet.Cell(currentRow, lastI + 6).Value = "Id модификации";
                    worksheet.Cell(currentRow, lastI + 7).Value = "Путь до изображения марки";
                    currentRow++;
                }
                for (int i = 1; i < data.Count; i++)
                {
                    // Добавляем название в первую колонку
                    worksheet.Cell(currentRow + i, 1).Value = makeName;
                    worksheet.Cell(currentRow + i, 2).Value = name;
                    int lastJ = 0;
                    for (int j = 0; j < data[i].Count; j++)
                    {
                        worksheet.Cell(currentRow + i, j + 3).Value = data[i][j]; // Смещаем на 1 колонку вправо
                        lastJ = j;
                    }
                    worksheet.Cell(currentRow + i, lastJ + 4).Value = makeId;
                    worksheet.Cell(currentRow + i, lastJ + 5).Value = modelGroupId;
                    worksheet.Cell(currentRow + i, lastJ + 6).Value = modificationsIdData[i];
                    worksheet.Cell(currentRow + i, lastJ + 7).Value = Path.Combine(DirPath, makeName.ToUpper(), $"{makeName}.ico");
                }
                currentRow += data.Count;
            }
            catch (NoSuchElementException ex)
            {
                UpdateUI($"Error: Element not found - {ex.Message}");
            }
            catch (TimeoutException ex)
            {
                UpdateUI($"Error: Timeout waiting for element - {ex.Message}");
            }
            catch (WebDriverException ex)
            {
                UpdateUI($"WebDriver Error: {ex.Message}");
            }
            catch (Exception ex)
            {
                UpdateUI($"Unknown Error: {ex.Message}");
            }
        }

        private string ExtractTypeId(string url)
        {
            // Создаем UriBuilder из строки URL
            UriBuilder uriBuilder = new UriBuilder("https://www.workshopdata.com/" + url);
            Uri uri = uriBuilder.Uri;

            // Получаем коллекцию параметров запроса
            var query = HttpUtility.ParseQueryString(uri.Query);

            // Извлекаем значение параметра typeId
            return query["typeId"];
        }

        private async Task SearchSubCars(string carLink, HashSet<string> carDescriptionsLinks)
        {
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));

            await driver.Navigate().GoToUrlAsync(carLink);

            UpdateUI("Ожидание загрузки всех ссылок...");
            wait.Until(d => d.FindElements(By.TagName("a")).Count > 0);

            UpdateUI("Поиск всех элементов <a>...");
            var links = driver.FindElements(By.TagName("a"));

            Regex regex = new Regex(@"^https:\/\/www\.workshopdata\.com\/touch\/site\/layout\/modelTypes\?modelGroupId=dg_\d+&makeId=m_\d+");

            foreach (var link in links)
            {
                string href = link.GetAttribute("href");
                var carImgElements = link.FindElements(By.ClassName("model-group-image"));

                foreach (var img in carImgElements)
                {
                    var imgNameElement = link.FindElement(By.TagName("p"));

                    IWebElement makeNameElement = driver.FindElement(By.ClassName("makeName"));
                    string makeName = makeNameElement.Text.Trim();

                    string imgName = $"({makeName}) {imgNameElement.Text}";
                    string imgUrl = img.GetAttribute("src");
                    UpdateUI($"CarImageName: {imgName}, CarImageUrl: {imgUrl}");

                    if (!string.IsNullOrEmpty(imgName) && !string.IsNullOrEmpty(imgUrl))
                    {
                        carBrendImages.Add(imgName, imgUrl);
                    }
                }


                if (!string.IsNullOrEmpty(href) && href != "#" && regex.IsMatch(href))
                {
                    carDescriptionsLinks.Add(href);
                }
            }

            //foreach (var carDescriptionLink in carDescriptionsLinks)
            //{
            //    UpdateUI(carDescriptionLink);
            //}
        }

        private HashSet<string> SearchCarModelsInMain()
        {
            HashSet<string> uniqueLinks = new HashSet<string>();
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));

            try
            {
                UpdateUI("Переход на страницу...");

                driver.Navigate().GoToUrlAsync("https://www.workshopdata.com/touch/site/layout/makesOverview ");

                IWebElement demoButton = driver.FindElement(By.XPath("//input[@value='Демо']"));

                demoButton.Click();

                UpdateUI("Ожидание загрузки всех ссылок...");
                wait.Until(d => d.FindElements(By.TagName("a")).Count > 0);

                var imgNameElements = driver.FindElements(By.ClassName("tile"));
                var imgUrlElements = driver.FindElements(By.ClassName("make-logo-big"));

                UpdateUI($"Всего картинок: {imgUrlElements.Count}, всего имён: {imgNameElements.Count}");

                for (int i = 0; i < imgNameElements.Count; i++)
                {
                    string imgName = imgNameElements[i].Text;
                    string imgUrl = imgUrlElements[i].GetAttribute("src");
                    if (!string.IsNullOrEmpty(imgName) && !string.IsNullOrEmpty(imgUrl) && !carBrendImages.ContainsKey(imgName))
                    {
                        carBrendImages.Add(imgName, imgUrl);
                        UpdateUI($"Номер изображения: {i + 1}, Название изображения: {imgName}, Адрес изображения: {imgUrl}");
                    }
                    else
                    {
                        UpdateUI($"Пропущено изображение с повторяющимся названием: {imgName}");
                    }
                }

                UpdateUI("Поиск всех элементов <a>...");

                var links = driver.FindElements(By.TagName("a"));

                Regex regex = new Regex(@"^https:\/\/www\.workshopdata\.com\/touch\/site\/layout\/modelOverview\?makeId=m_\d+#allowed$");

                UpdateUI("Сбор уникальных ссылок...");

                foreach (var link in links)
                {
                    string href = link.GetAttribute("href");
                    if (!string.IsNullOrEmpty(href) && href != "#" && regex.IsMatch(href))
                    {
                        uniqueLinks.Add(href);
                    }
                }

                UpdateUI("Вывод уникальных ссылок:");

                foreach (var uniqueLink in uniqueLinks)
                {
                    UpdateUI(uniqueLink);
                }
            }
            catch (NoSuchElementException ex)
            {
                UpdateUI($"Ошибка: Элемент не найден - {ex.Message}");
            }
            catch (TimeoutException ex)
            {
                UpdateUI($"Ошибка: Таймаут ожидания - {ex.Message}");
            }
            catch (WebDriverException ex)
            {
                UpdateUI($"Ошибка WebDriver: {ex.Message}");
            }
            catch (Exception ex)
            {
                UpdateUI($"Неизвестная ошибка: {ex.Message}");
            }

            return uniqueLinks;
        }

        public static (string modelGroupId, string makeId) ParseModelAndMakeIds(string url)
        {
            Uri uri = new Uri(url);
            var query = QueryHelpers.ParseQuery(uri.Query);

            string modelGroupId = query["modelGroupId"].ToString();
            string makeId = query["makeId"].ToString();

            return (modelGroupId, makeId);
        }

        private static void AutoFitColumns(IXLWorksheet worksheet)
        {
            worksheet.ColumnsUsed().AdjustToContents();
        }

        private async Task DownloadBrendImagesAsync(Dictionary<string, string> carModelsImages)
        {
            UpdateUI($"Скачивание и конвертирование изображений марок начато.\nКол - во изображений: {carModelsImages.Count}");
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    Directory.CreateDirectory(DirPath);
                    Process.Start(DirPath);
                    int i = 0;
                    foreach (var kvp in carModelsImages)
                    {
                        i++;
                        UpdateUI($"Номер изображения: {i}");
                        string imgName = kvp.Key;
                        string imgUrl = kvp.Value;

                        // Извлекаем имя бренда из имени файла
                        string brandName = imgName.Split(')')[0].Trim('(', ' ');
                        string brandDirPath = Path.Combine(DirPath, brandName.ToUpper());
                        Directory.CreateDirectory(brandDirPath);

                        // Очищаем имя файла от недопустимых символов
                        string safeFileName = Regex.Replace(imgName, @"[\\/:*?""<>|]", "_");

                        // Отправляем запрос для получения изображения
                        HttpResponseMessage response = await client.GetAsync(imgUrl);

                        // Убедимся, что запрос был успешным
                        response.EnsureSuccessStatusCode();
                        string debugMsg = response.IsSuccessStatusCode ? "Запрос был успешен" : "Запрос не был успешен";
                        UpdateUI(debugMsg);

                        // Читаем содержимое ответа как массив байтов
                        byte[] imageData = await response.Content.ReadAsByteArrayAsync();

                        // Записываем массив байтов в файл
                        string destinationGzipPath = Path.Combine(brandDirPath, safeFileName + ".svg.gzip");
                        File.WriteAllBytes(destinationGzipPath, imageData);

                        // Распаковываем GZIP файл
                        string destinationSvgPath = Path.Combine(brandDirPath, safeFileName + ".svg");
                        using (FileStream gzipStream = new FileStream(destinationGzipPath, FileMode.Open))
                        using (GZipStream decompressionStream = new GZipStream(gzipStream, CompressionMode.Decompress))
                        using (FileStream outputStream = new FileStream(destinationSvgPath, FileMode.Create))
                        {
                            await decompressionStream.CopyToAsync(outputStream);
                        }

                        // Конвертируем SVG файл в PNG
                        string destinationPngPath = Path.Combine(brandDirPath, safeFileName + ".png");
                        SvgDocument svgDoc = SvgDocument.Open(destinationSvgPath);

                        using (Bitmap bitmap = svgDoc.Draw())
                        {
                            bitmap.Save(destinationPngPath, ImageFormat.Png);
                        }
                        UpdateUI($"Конвертация {destinationSvgPath} to {destinationPngPath}");
                        // Удаляем временные файлы
                        File.Delete(destinationGzipPath);
                        File.Delete(destinationSvgPath);

                    }
                }
            }
            catch (Exception ex)
            {
                UpdateUI($"Произошла ошибка: {ex.Message}");
            }
            UpdateUI("Скачивание и конвертирование изображений марок завершено");
        }

        private void SetFileAttributes(string path, FileAttributes attributes)
        {
            File.SetAttributes(path, attributes);
        }

        private void UpdateUI(string message)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<string>(UpdateUI), message);
            }
            else
            {
                message = $"[{DateTime.Now}] " + message;

                // Обновление UI здесь
                Console.WriteLine(message);
                textBox1.AppendText(message + Environment.NewLine);
            }
        }
    }
}
