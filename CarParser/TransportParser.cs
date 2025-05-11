using ClosedXML.Excel;
using DocumentFormat.OpenXml.Bibliography;
using Microsoft.AspNetCore.WebUtilities;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO.Compression;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using OpenQA.Selenium.Chrome;
using System.Windows.Forms;
using Svg;
using System.Drawing.Imaging;

namespace CarParser
{
    public class TransportParser : Form
    {
        IWebDriver driver;
        string transportDownloadFilesPath = "";
        string trasportLinkType = "";
        string transportModelTypes = "";

        readonly Dictionary<string, string> transportImages = new Dictionary<string, string>();
        string DirPath = "";
        int currentRow = 1; // Свойство для отслеживания текущей строки

        private Form1 mainForm;

        public async Task StartParsing(Form1 mainForm, string transportType, string trasportLinkType, string transportModelType)
        {
            this.mainForm = mainForm;
            this.trasportLinkType = trasportLinkType;
            this.transportModelTypes = transportModelType;

            transportDownloadFilesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "PdfFiles", transportType);
            DirPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Transports", "Images", transportType);

            try
            {
                transportImages.Clear();
                currentRow = 1;

                await Task.Run(async () =>
                {
                    if (driver == null)
                    {
                        ChromeOptions options = new ChromeOptions();
                        options.AddArgument("--headless"); // Запуск в безголовом режиме
                        options.AddArgument("--disable-gpu"); // Отключает GPU для улучшения производительности в headless режиме
                        options.AddUserProfilePreference("download.default_directory", transportDownloadFilesPath);
                        options.AddUserProfilePreference("download.prompt_for_download", false);
                        options.AddUserProfilePreference("download.directory_upgrade", true);
                        options.AddUserProfilePreference("safebrowsing.enabled", true);

                        ChromeDriverService service = ChromeDriverService.CreateDefaultService();
                        service.HideCommandPromptWindow = true;

                        // Инициализация драйвера с указанием удаленного URL
                        driver = new ChromeDriver(service, options);
                    }

                    HashSet<string> carLinks;
                    HashSet<string> carDescriptionsLinks = new HashSet<string>();
                    carLinks = SearchCarModelsInMain();

                    // Создаем новый Excel файл
                    using (XLWorkbook workbook = new XLWorkbook())
                    {
                        IXLWorksheet worksheet = workbook.Worksheets.Add($"{transportType} Descriptions");
                        foreach (var carLink in carLinks)
                        {
                            carDescriptionsLinks.Clear(); // Очищаем коллекцию перед каждой итерацией
                            await SearchSubTransports(carLink, carDescriptionsLinks);
                            foreach (var carDescriptionLink in carDescriptionsLinks)
                            {
                                Console.WriteLine(carDescriptionLink);
                                await CarDescriptionParser(driver, carDescriptionLink, worksheet);
                            }
                        }

                        await DownloadTransportImagesAsync(transportImages);

                        // Автоматическое подстраивание ширины столбцов
                        AutoFitColumns(worksheet);
                        // Сохраняем файл
                        workbook.SaveAs(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Transports", transportType, $"{transportType}Descriptions.xlsx"));
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
                string modelGroupId = "", makeId = "", modificationId = "", fuelType = "";
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
                List<string> modificationIdsData = new List<string>();
                List<string> modificationFuelTypes = new List<string>();

                Regex repairManualRegex = new Regex(@"https:\/\/www\.workshopdata\.com\/touch\/site\/layout\/repairManuals\?modelId=\w+&groupId=\w+&storyId=\w+(&altView=true&extraInfo=true)?");

                Regex jackLocationRegex = new Regex(@"^\/touch\/site\/layout\/jackingPoints\?modelId=d_\d+");
                Regex diagnosticConnectorRegex = new Regex(@"^\/touch\/site\/layout\/eobdConnectorLocations\?modelId=d_\d+");

                var idLocationDivs = driver.FindElements(By.ClassName("id-location-div"));

                foreach (var div in idLocationDivs)
                {
                    var idLocationBtns = div.FindElements(By.TagName("a"));

                    foreach (var btn in idLocationBtns)
                    {
                        string href = btn.GetAttribute("href");
                        string dataUrl = btn.GetAttribute("data-url");

                        UpdateUI($"HREF: {href} DataURL: {dataUrl}");

                        if (!string.IsNullOrEmpty(dataUrl) && jackLocationRegex.IsMatch(dataUrl))
                        {
                            UpdateUI($"({makeName})({name}) {btn.Text} https://www.workshopdata.com{dataUrl}l");
                            transportImages.Add($"({makeName})({name}) {btn.Text}", "https://www.workshopdata.com" + dataUrl);
                        }
                        if (!string.IsNullOrEmpty(dataUrl) && diagnosticConnectorRegex.IsMatch(dataUrl))
                        {
                            UpdateUI($"({makeName})({name}) {btn.Text} https://www.workshopdata.com{dataUrl}");
                            transportImages.Add($"({makeName})({name}) {btn.Text}", "https://www.workshopdata.com" + dataUrl);
                        }

                        if (!string.IsNullOrEmpty(href) && repairManualRegex.IsMatch(href))
                        {
                            UpdateUI($"({makeName})({name}) {btn.Text} {href}");
                            transportImages.Add($"({makeName})({name})  {btn.Text}", href);
                        }
                    }
                }

                // Проход по строкам
                foreach (IWebElement row in rows)
                {
                    string rawId = row.GetAttribute("data-url");
                    modificationId = ExtractTypeId(rawId);
                    string fuelClassType = row.GetAttribute("class");

                    switch (fuelClassType)
                    {
                        case "petrol":
                            fuelType = "Бензин";
                            break;

                        case "diesel":
                            fuelType = "Дизель";
                            break;

                        case "cng":
                            fuelType = "Природный газ";
                            break;

                        case "hydrogen":
                            fuelType = "Водород";
                            break;

                        case "electric":
                            fuelType = "Электроэнергия";
                            break;

                        case "hybrid":
                            fuelType = "Гибрид";
                            break;

                        case "":
                            fuelType = "Не указано";
                            break;
                    }

                    UpdateUI($"Id марки: {makeId}, Id модели: {modelGroupId}, Id модификации: {modificationId}, Тип топлива модификации: {fuelType}");

                    // Получение ячеек (<td>) в текущей строке
                    IList<IWebElement> cells = row.FindElements(By.TagName("td"));

                    // Создание списка для хранения данных текущей строки
                    List<string> rowData = new List<string>();

                    // Проход по ячейкам и извлечение текста
                    foreach (IWebElement cell in cells)
                    {
                        rowData.Add(cell.Text.Trim());
                    }
                    modificationFuelTypes.Add(fuelType);
                    modificationIdsData.Add(modificationId);

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
                    worksheet.Cell(currentRow, lastI + 4).Value = "Тип топлива";
                    worksheet.Cell(currentRow, lastI + 5).Value = "Id марки";
                    worksheet.Cell(currentRow, lastI + 6).Value = "Id модели";
                    worksheet.Cell(currentRow, lastI + 7).Value = "Id модификации";
                    worksheet.Cell(currentRow, lastI + 8).Value = "Путь до изображения марки";
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
                    worksheet.Cell(currentRow + i, lastJ + 4).Value = fuelType;
                    worksheet.Cell(currentRow + i, lastJ + 5).Value = makeId;
                    worksheet.Cell(currentRow + i, lastJ + 6).Value = modelGroupId;
                    worksheet.Cell(currentRow + i, lastJ + 7).Value = modificationIdsData[i];
                    worksheet.Cell(currentRow + i, lastJ + 8).Value = Path.Combine(DirPath, makeName.ToUpper(), $"{makeName}.png");
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

        private async Task SearchSubTransports(string carLink, HashSet<string> carDescriptionsLinks)
        {
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));

            await driver.Navigate().GoToUrlAsync(carLink);

            UpdateUI("Ожидание загрузки всех ссылок...");
            wait.Until(d => d.FindElements(By.TagName("a")).Count > 0);

            UpdateUI("Поиск всех элементов <a>...");
            var links = driver.FindElements(By.TagName("a"));

            Regex descriptionRegex = new Regex($@"^https:\/\/www\.workshopdata\.com\/touch\/site\/layout\/{transportModelTypes}\?modelGroupId=dg_\d+&makeId=m_\d+");

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
                        transportImages.Add(imgName, imgUrl);
                    }

                    if (!string.IsNullOrEmpty(href) && href != "#" && descriptionRegex.IsMatch(href))
                    {
                        carDescriptionsLinks.Add(href);
                    }
                }
            }
        }

        private HashSet<string> SearchCarModelsInMain()
        {
            HashSet<string> uniqueLinks = new HashSet<string>();
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));

            try
            {
                UpdateUI($"Переход на страницу https://www.workshopdata.com/touch/site/layout/makes{trasportLinkType}");

                driver.Navigate().GoToUrlAsync($"https://www.workshopdata.com/touch/site/layout/makes{trasportLinkType}");
                IWebElement demoButton = driver.FindElement(By.XPath("//input[@value='Демо']"));

                demoButton.Click();

                driver.Navigate().GoToUrlAsync($"https://www.workshopdata.com/touch/site/layout/makes{trasportLinkType}");

                UpdateUI("Ожидание загрузки всех ссылок...");
                wait.Until(d => d.FindElements(By.TagName("a")).Count > 0);

                var imgNameElements = driver.FindElements(By.ClassName("tile"));
                var imgUrlElements = driver.FindElements(By.ClassName("make-logo-big"));

                UpdateUI($"Всего картинок: {imgUrlElements.Count}, всего имён: {imgNameElements.Count}");

                for (int i = 0; i < imgNameElements.Count; i++)
                {
                    string imgName = imgNameElements[i].Text;
                    string imgUrl = imgUrlElements[i].GetAttribute("src");
                    if (!string.IsNullOrEmpty(imgName) && !string.IsNullOrEmpty(imgUrl) && !transportImages.ContainsKey(imgName))
                    {
                        transportImages.Add(imgName, imgUrl);
                        UpdateUI($"Номер изображения: {i + 1}, Название изображения: {imgName}, Адрес изображения: {imgUrl}");
                    }
                    else if (transportImages.ContainsKey(imgName))
                    {
                        UpdateUI($"Пропущено изображение с повторяющимся названием: {imgName}");
                    }
                }

                UpdateUI("Поиск всех элементов <a>...");

                var links = driver.FindElements(By.TagName("a"));

                Regex regex = new Regex($@"^https:\/\/www\.workshopdata\.com\/touch\/site\/layout\/model{trasportLinkType}\?makeId=m_\d+");

                UpdateUI("Сбор уникальных ссылок...");

                UpdateUI("Вывод уникальных ссылок:");

                foreach (var link in links)
                {
                    string href = link.GetAttribute("href");

                    if (!string.IsNullOrEmpty(href) && href != "#" && regex.IsMatch(href))
                    {
                        uniqueLinks.Add(href);
                        UpdateUI(href);
                    }
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

        private async Task DownloadTransportImagesAsync(Dictionary<string, string> carModelsImages)
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

                        UpdateUI($"Image URL: {imgUrl}");

                        Regex jackLocationRegex = new Regex(@"^https:\/\/www\.workshopdata\.com\/touch\/site\/layout\/jackingPoints\?modelId=d_\d+");
                        Regex diagnosticConnectorRegex = new Regex(@"^https:\/\/www\.workshopdata\.com\/touch\/site\/layout\/eobdConnectorLocations\?modelId=d_\d+");
                        Regex repairManualRegex = new Regex(@"https:\/\/www\.workshopdata\.com\/touch\/site\/layout\/repairManuals\?modelId=\w+&groupId=\w+&storyId=\w+(&altView=true&extraInfo=true)?");

                        if (!string.IsNullOrEmpty(imgUrl) && (jackLocationRegex.IsMatch(imgUrl) || diagnosticConnectorRegex.IsMatch(imgUrl)))
                        {
                            await driver.Navigate().GoToUrlAsync(imgUrl);

                            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));

                            string className = "";

                            if (jackLocationRegex.IsMatch(imgUrl))
                            {
                                className = "jackingPointsImg";
                            }
                            else
                            {
                                className = "eobdConnectorImg";
                            }

                            wait.Until(d => d.FindElements(By.ClassName(className)));

                            var imgs = driver.FindElements(By.ClassName(className));

                            foreach (var img in imgs)
                            {
                                imgUrl = img.GetAttribute("src");
                            }
                        }

                        if (!string.IsNullOrEmpty(imgUrl) && repairManualRegex.IsMatch(imgUrl))
                        {
                            try
                            {
                                int index = imgUrl.IndexOf("repairManuals");
                                string imgDownloadUrl = "";

                                if (index != -1)
                                {
                                    imgDownloadUrl = imgUrl.Substring(0, index + "repairManuals".Length) + "Print" + imgUrl.Substring(index + "repairManuals".Length);
                                }

                                await driver.Navigate().GoToUrlAsync(imgDownloadUrl);


                                string[] files = Directory.GetFiles(transportDownloadFilesPath);
                                string downloadedFilePath = files[files.Length - 1];

                                while (downloadedFilePath.Contains(".tmp") || downloadedFilePath.Contains(".crdownload"))
                                {
                                    files = Directory.GetFiles(transportDownloadFilesPath);
                                    downloadedFilePath = files[files.Length - 1];

                                    await Task.Delay(3000);
                                }

                                // Переименование файла
                                string newFileName = safeFileName + ".pdf";
                                string newFilePath = Path.Combine(brandDirPath, newFileName);
                                UpdateUI("Путь до скачаного PDF файла: " + downloadedFilePath);
                                File.Move(downloadedFilePath, newFilePath);
                            }
                            catch (Exception ex) { UpdateUI(ex.Message); }

                            continue;
                        }

                        await ConvertGZIPAsync(client, imgUrl, brandDirPath, safeFileName);
                    }
                }
            }
            catch (Exception ex)
            {
                UpdateUI($"Произошла ошибка: {ex.Message}");
            }
            UpdateUI("Скачивание и конвертирование изображений завершено");
        }

        private async Task ConvertGZIPAsync(HttpClient client, string imgUrl, string brandDirPath, string safeFileName)
        {
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

        private void UpdateUI(string message)
        {
            if (mainForm.IsHandleCreated && mainForm.InvokeRequired)
            {
                mainForm.Invoke(new Action<string>(UpdateUI), message);
            }
            else
            {
                message = $"[{DateTime.Now}] " + message;

                // Обновление UI здесь
                Console.WriteLine(message);
                mainForm.textBox1.AppendText(message + Environment.NewLine);
            }
        }
    }
}
