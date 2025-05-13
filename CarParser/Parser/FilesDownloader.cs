using DocumentFormat.OpenXml.Bibliography;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.IO;
using System.Drawing;
using System.IO.Compression;
using Svg;
using System.Drawing.Imaging;
using OpenQA.Selenium.Chrome;

namespace CarParser.Parser
{
    public class FilesParser
    {
        private IWebDriver Driver { get; }
        private readonly string DirPath;
        private readonly string CacheFolderPath;

        public FilesParser(IWebDriver driver, string cacheFolderPath, string folderName)
        {
            DirPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), folderName);
            Driver = driver;

            CacheFolderPath = cacheFolderPath;

            Directory.CreateDirectory(CacheFolderPath);
        }

        public void ExportImageFile(string ImageName, string ImageURL)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    Directory.CreateDirectory(DirPath);

                    UpdateUI($"Название изображения: {ImageName}, Ссылка на изображение: {ImageURL}");

                    // Извлекаем имя бренда из имени файла
                    string brandName = ImageName.Split(')')[0].Trim('(', ' ');

                    string brandDirPath = Path.Combine(DirPath, brandName.ToUpper());

                    Directory.CreateDirectory(brandDirPath);

                    // Очищаем имя файла от недопустимых символов
                    string safeFileName = Regex.Replace(ImageName, @"[\\/:*?""<>|]", "_");

                    Regex jackLocationRegex = new Regex(@"^https:\/\/www\.workshopdata\.com\/touch\/site\/layout\/jackingPoints\?modelId=d_\d+");
                    Regex diagnosticConnectorRegex = new Regex(@"^https:\/\/www\.workshopdata\.com\/touch\/site\/layout\/eobdConnectorLocations\?modelId=d_\d+");
                    Regex repairManualRegex = new Regex(@"https:\/\/www\.workshopdata\.com\/touch\/site\/layout\/repairManuals\?modelId=\w+&groupId=\w+&storyId=\w+(&altView=true&extraInfo=true)?");

                    if (!string.IsNullOrEmpty(ImageURL) && (jackLocationRegex.IsMatch(ImageURL) || diagnosticConnectorRegex.IsMatch(ImageURL)))
                    {
                        Driver.Navigate().GoToUrl(ImageURL);

                        WebDriverWait wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(30));

                        string className = "";

                        if (jackLocationRegex.IsMatch(ImageURL))
                        {
                            className = "jackingPointsImg";
                        }
                        else
                        {
                            className = "eobdConnectorImg";
                        }

                        wait.Until(d => d.FindElements(By.ClassName(className)));

                        var imgs = Driver.FindElements(By.ClassName(className));

                        foreach (var img in imgs)
                        {
                            ImageURL = img.GetAttribute("src");
                        }
                    }

                    if (!string.IsNullOrEmpty(ImageURL) && repairManualRegex.IsMatch(ImageURL))
                    {
                        try
                        {
                            int index = ImageURL.IndexOf("repairManuals");
                            string imgDownloadUrl = "";

                            if (index != -1)
                            {
                                imgDownloadUrl = ImageURL.Substring(0, index + "repairManuals".Length) + "Print" + ImageURL.Substring(index + "repairManuals".Length);
                            }

                            Driver.Navigate().GoToUrl(imgDownloadUrl);

                            string[] files = Directory.GetFiles(CacheFolderPath);
                            string downloadedFilePath = "";

                            if (files.Length > 0)
                            {
                                downloadedFilePath = files[files.Length - 1];
                            }
                            else
                            {
                                UpdateUI("В каталоге нет файлов для обработки.");
                                return;
                            }

                            while (downloadedFilePath.Contains(".tmp") || downloadedFilePath.Contains(".crdownload"))
                            {
                                files = Directory.GetFiles(CacheFolderPath);
                                if (files.Length > 0)
                                {
                                    downloadedFilePath = files[files.Length - 1];
                                }
                                else
                                {
                                    UpdateUI("В каталоге нет файлов для обработки.");
                                    return;
                                }

                                Task.Delay(3000);
                            }

                            // Переименование файла
                            string newFileName = safeFileName + ".pdf";
                            string newFilePath = Path.Combine(brandDirPath, newFileName);
                            UpdateUI("Путь до скачаного PDF файла: " + downloadedFilePath);
                            File.Move(downloadedFilePath, newFilePath);

                            return;
                        }
                        catch (Exception ex) { UpdateUI(ex.Message); }
                    }

                    ConvertGZIP(client, ImageURL, brandDirPath, safeFileName);
                }
            }
            catch (Exception ex)
            {
                UpdateUI($"Произошла ошибка: {ex.Message}");
            }

            UpdateUI("Скачивание и конвертирование изображений завершено");
        }

        private static void ConvertGZIP(HttpClient client, string imgUrl, string brandDirPath, string safeFileName)
        {
            try
            {
                // Отправляем запрос для получения изображения
                HttpResponseMessage response = client.GetAsync(imgUrl).GetAwaiter().GetResult();

                // Убедимся, что запрос был успешным
                response.EnsureSuccessStatusCode();
                string debugMsg = response.IsSuccessStatusCode ? "Запрос был успешен" : "Запрос не был успешен";
                UpdateUI(debugMsg);

                // Читаем содержимое ответа как массив байтов
                byte[] imageData = response.Content.ReadAsByteArrayAsync().GetAwaiter().GetResult();

                // Записываем массив байтов в файл
                string destinationGzipPath = Path.Combine(brandDirPath, safeFileName + ".svg.gzip");
                File.WriteAllBytes(destinationGzipPath, imageData);

                // Распаковываем GZIP файл
                string destinationSvgPath = Path.Combine(brandDirPath, safeFileName + ".svg");
                using (FileStream gzipStream = new FileStream(destinationGzipPath, FileMode.Open))
                using (GZipStream decompressionStream = new GZipStream(gzipStream, CompressionMode.Decompress))
                using (FileStream outputStream = new FileStream(destinationSvgPath, FileMode.Create))
                {
                    decompressionStream.CopyToAsync(outputStream).GetAwaiter().GetResult();
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
            catch (InvalidDataException ex) { Console.WriteLine("Недопустимые данные: " + ex.Message); }
            catch (Exception ex) { Console.WriteLine("Неизвестная ошибка: " + ex.Message); }
        }

        private static void UpdateUI(string message)
        {
            Console.WriteLine($"[{DateTime.Now}] " + message);
        }
    }
}
