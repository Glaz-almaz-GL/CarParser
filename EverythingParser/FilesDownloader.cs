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
using System.Windows.Forms;

namespace CarParser.Parser
{
    public class FilesDownloader
    {
        private string ParserName { get; }
        private string DirPath;
        private string DownloadCacheFolderPath { get; }

        public FilesDownloader(string parserName, string downloadFolderPath, string folderName)
        {
            ParserName = parserName;
            DirPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), $"Parser ({ParserName})", folderName, "Images");

            DownloadCacheFolderPath = downloadFolderPath;

            Directory.CreateDirectory(DownloadCacheFolderPath);
        }

        public void DownloadFile(string FileURL, string fileName = "")
        {
            string FileName = fileName;

            if (string.IsNullOrEmpty(fileName))
            {
                FileName = GetFileNameFromUrl(FileURL);
            }

            string fileExtension = GetFileExtensionFromUrl(FileURL);

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    Directory.CreateDirectory(DirPath);

                    string safeFileName = GetSafeFileName(FileName);

                    if (fileExtension == ".svgz")
                    {
                        // Заменяем .svgz на .svg.gzip
                        string destinationGzipPath = Path.Combine(DirPath, safeFileName + ".svg.gzip");
                        string destinationSvgPath = Path.Combine(DirPath, safeFileName + ".svg");
                        string destinationPngPath = Path.Combine(DirPath, safeFileName + ".png");
                        ConvertGZIP(client, FileURL, destinationGzipPath, destinationSvgPath, destinationPngPath);
                    }
                }
            }
            catch (Exception ex)
            {
                UpdateUI($"Log (FilesDownloader.cs): Произошла ошибка: {ex.Message}");
            }

            UpdateUI("Log (FilesDownloader.cs): Скачивание и конвертирование изображений завершено");
        }

        private static void ConvertGZIP(HttpClient client, string imgUrl, string destinationGzipPath, string destinationSvgPath, string destinationPngPath)
        {
            try
            {
                // Отправляем запрос для получения изображения
                HttpResponseMessage response = client.GetAsync(imgUrl).GetAwaiter().GetResult();

                // Убедимся, что запрос был успешным
                response.EnsureSuccessStatusCode();
                string debugMsg = response.IsSuccessStatusCode ? "Запрос был успешен" : "Запрос не был успешен";
                UpdateUI("Log (FilesDownloader.cs): " + debugMsg);

                // Читаем содержимое ответа как массив байтов
                byte[] imageData = response.Content.ReadAsByteArrayAsync().GetAwaiter().GetResult();

                // Записываем массив байтов в файл
                File.WriteAllBytes(destinationGzipPath, imageData);

                // Распаковываем GZIP файл
                using (FileStream gzipStream = new FileStream(destinationGzipPath, FileMode.Open))
                using (GZipStream decompressionStream = new GZipStream(gzipStream, CompressionMode.Decompress))
                using (FileStream outputStream = new FileStream(destinationSvgPath, FileMode.Create))
                {
                    decompressionStream.CopyToAsync(outputStream).GetAwaiter().GetResult();
                }

                // Конвертируем SVG файл в PNG
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
            catch (InvalidDataException ex)
            {
                UpdateUI("Log (FilesDownloader.cs): Недопустимые данные: " + ex.Message);
            }
            catch (Exception ex)
            {
                UpdateUI("Log (FilesDownloader.cs): Неизвестная ошибка: " + ex.Message);
            }
        }

        public static string GetSafeFileName(string fileName)
        {
            UpdateUI($"Log (FilesDownloader.cs): Название изображения: {fileName}");

            // Разделяем название файла на части по слешу
            string[] parts = fileName.Split('/');

            // Формируем безопасное название файла
            string safeFileName;
            if (parts.Length > 1)
            {
                // Если частей больше одной, берем первую часть и добавляем последнюю часть в скобки
                string prefix = parts[0];
                string suffix = parts[parts.Length - 1];
                safeFileName = $"{prefix} ({suffix})";
            }
            else
            {
                // Если частей нет или только одна часть, используем её как есть
                safeFileName = parts[0];
            }

            // Очищаем имя файла от недопустимых символов
            safeFileName = Regex.Replace(safeFileName, @"[\\/:*?""<>|]", "_");

            UpdateUI($"Log (FilesDownloader.cs): Безопасное название изображения: {safeFileName}");

            return safeFileName;
        }

        private static string GetFileExtensionFromUrl(string url)
        {
            Console.WriteLine(url);

            Uri uri = new Uri(url);
            string path = uri.LocalPath;
            string extension = Path.GetExtension(path);
            return extension;
        }

        private static string GetFileNameFromUrl(string url)
        {
            Uri uri = new Uri(url);
            string fileName = Path.GetFileName(uri.LocalPath);
            return fileName;
        }

        private static void UpdateUI(string message)
        {
            Console.WriteLine($"[{DateTime.Now}]: {message}");
        }
    }
}
