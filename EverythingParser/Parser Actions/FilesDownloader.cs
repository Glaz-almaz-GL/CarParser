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
        public static Dictionary<string, string> extensionToDirectoryMap { get; private set; } = new Dictionary<string, string>
        {
            { "Images", ".jpeg;.jpg;.png;.gif;.bmp;.tiff;.tif;.psd;.raw;.svg;.svgz;.webp;.heif;.heic;.avif;.cr2;.nef;.arw;.dng;.crw;.tga;.ico;.pcx;.pbm;.pgm;.ppm;.dds;.exr;.hdr;.jxr;.pxr" },
            { "Videos", ".mp4;.avi;.mkv;.mov;.flv;.wmv;.mpeg;.mpg;.3gp;.webm;.m4v;.ts;.mts;.vob;.rmvb;.ogv" },
            { "Texts", ".txt;.rtf;.md;.log;.csv;.tsv;.ini;.cfg;.conf;.properties;.sql;.html;.htm;.xml;.json" },
            { "Audios", ".mp3;.wav;.aac;.flac;.ogg;.wma;.m4a;.alac;.mid;.midi;.amr;.opus;.ape;.wv" },
            { "Archives", ".zip;.rar;.7z;.tar;.gz;.tgz;.bz2;.tbz2;.xz;.txz;.iso;.cab;.jar;.war;.ear" },
            { "Documents", ".doc;.docx;.xls;.xlsx;.ppt;.pptx;.pdf;.odt;.ods;.odp;.rtf;.txt;.html;.htm;.xml;.json;.csv;.md" },
            { "Models", ".obj;.fbx;.stl;.dae;.glb;.gltf;.3ds;.max;.blend;.ma;.mb;.c4d;.lwo;.lws;.ply" },
            { "Databases", ".db;.sqlite;.mdb;.accdb;.sql;.bak;.fdb;.dbf;.ldf;.mdf;.frm;.myd;.myi;.ibd;.sdf;.fdbk" },
            { "WebFiles", ".html;.htm;.css;.js;.php;.asp;.aspx;.jsp;.py;.rb;.pl;.go;.java;.ts;.vue;.jsx;.tsx;.svelte;.json;.xml;.yml;.yaml;.md;.markdown;.txt;.htaccess" },
            { "Executables", ".exe;.dll;.so;.dylib;.a;.lib;.class;.pyc;.pyo;.bat;.sh;.cmd;.msi;.app;.ipa;.apk;.dex;.ipa;.aab;.swf;.bin;.out;.com;.cgi;.pl;.py;.rb;.php;.jsp;.asp;.aspx" },
            { "Fonts", ".ttf;.otf;.woff;.woff2;.eot;.pfa;.pfb;.pfm;.afm" },
            { "Spreadsheets", ".xls;.xlsx;.xlsm;.xlsb;.ods;.csv;.tsv;.numbers" },
            { "Presentations", ".ppt;.pptx;.pptm;.odp;.key" },
            { "Ebooks", ".epub;.mobi;.azw;.azw3;.fb2;.ibooks;.djvu;.pdf" },
            { "Torrents", ".torrent" },
            { "VirtualMachines", ".vmdk;.vdi;.vhd;.vhdx;.ova;.ovf" },
            { "CAD", ".dwg;.dxf;.dgn;.stp;.step;.iges;.igs;.sat" },
            { "GIS", ".shp;.kml;.kmz;.gpx;.geojson;.mif;.tab" },
            { "Backups", ".bak;.backup;.bkp;.old;.tmp" },
            { "Configs", ".ini;.cfg;.conf;.properties;.yml;.yaml;.toml;.env" },
            { "SourceCode", ".c;.cpp;.h;.hpp;.cs;.vb;.vbnet;.java;.js;.ts;.go;.swift;.php;.py;.rb;.pl;.sh;.bat;.cmd;.perl;.ruby;.bash;.fish;.rust;.dart;.typescript;.javascript;.scala;.kotlin;.groovy;.r;.matlab;.octave;.hs;.lua;.erlang;.elixir;.haskell;.coffeescript;.sass;.scss;.less;.vue;.jsx;.tsx;.svelte;.asm;.cmake;.make;.gradle;.ninja;.dockerfile;.sql;.graphql;.proto;.rst;.md;.markdown;.tex;.latex;.asciidoc;.pug;.haml;.erb;.jade;.blade;.twig;.mustache;.handlebars;.ejs;.liquid;.razor;.asp;.aspx;.jsp;.asp;.aspx;.jsp;.jspx;.cfm;.cfml;.tpl;.blade;.blade.php;.blade.html;.blade.htm;.blade.xml;.blade.json;.blade.md;.blade.markdown;.blade.tex;.blade.latex;.blade.asciidoc;.blade.pug;.blade.haml;.blade.erb;.blade.jade;.blade.blade;.blade.twig;.blade.mustache;.blade.handlebars;.blade.ejs;.blade.liquid;.blade.razor" }
        };

        public string ParserName { get; private set; }
        public static string DirPath { get; private set; }
        public string DownloadCacheFolderPath { get; private set; }

        public FilesDownloader(string parserName, string downloadFolderPath, string folderName)
        {
            ParserName = parserName;
            DirPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), $"Parser ({ParserName})", folderName);
            DownloadCacheFolderPath = downloadFolderPath;

            Directory.CreateDirectory(DownloadCacheFolderPath);
        }

        public static void DownloadFile(string fileUrl, string fileName = "")
        {
            string fileNameToUse = !string.IsNullOrEmpty(fileName) ? fileName : GetFileNameFromUrl(fileUrl);
            string fileExtension = GetFileExtensionFromUrl(fileUrl);

            string newDirPath = DirPath;

            foreach (var kvp in extensionToDirectoryMap)
            {
                string[] extensions = kvp.Value.Split(';');
                if (extensions.Contains(fileExtension))
                {
                    newDirPath = Path.Combine(newDirPath, kvp.Key);
                    break;
                }
            }

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    Directory.CreateDirectory(newDirPath);
                    string safeFileName = GetSafeFileName(fileNameToUse);

                    if (fileExtension.Equals(".svgz", StringComparison.OrdinalIgnoreCase))
                    {
                        string destinationGzipPath = Path.Combine(newDirPath, safeFileName + ".svg.gzip");
                        string destinationSvgPath = Path.Combine(newDirPath, safeFileName + ".svg");
                        string destinationPngPath = Path.Combine(newDirPath, safeFileName + ".png");
                        ConvertGZIP(client, fileUrl, destinationGzipPath, destinationSvgPath, destinationPngPath);
                    }
                    else
                    {
                        string destinationFilePath = Path.Combine(newDirPath, safeFileName + fileExtension);
                        DownloadRegularFile(client, fileUrl, destinationFilePath);
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
                HttpResponseMessage response = client.GetAsync(imgUrl).GetAwaiter().GetResult();
                response.EnsureSuccessStatusCode();

                byte[] imageData = response.Content.ReadAsByteArrayAsync().GetAwaiter().GetResult();
                File.WriteAllBytes(destinationGzipPath, imageData);

                using (FileStream gzipStream = new FileStream(destinationGzipPath, FileMode.Open))
                using (GZipStream decompressionStream = new GZipStream(gzipStream, CompressionMode.Decompress))
                using (FileStream outputStream = new FileStream(destinationSvgPath, FileMode.Create))
                {
                    decompressionStream.CopyToAsync(outputStream).GetAwaiter().GetResult();
                }

                SvgDocument svgDoc = SvgDocument.Open(destinationSvgPath);
                using (Bitmap bitmap = svgDoc.Draw())
                {
                    bitmap.Save(destinationPngPath, ImageFormat.Png);
                }

                UpdateUI($"Конвертация {destinationSvgPath} to {destinationPngPath}");

                File.Delete(destinationGzipPath);
                File.Delete(destinationSvgPath);
            }
            catch (InvalidDataException ex)
            {
                UpdateUI($"Log (FilesDownloader.cs): Недопустимые данные: {ex.Message}");
            }
            catch (Exception ex)
            {
                UpdateUI($"Log (FilesDownloader.cs): Неизвестная ошибка: {ex.Message}");
            }
        }

        private static void DownloadRegularFile(HttpClient client, string fileUrl, string destinationFilePath)
        {
            try
            {
                HttpResponseMessage response = client.GetAsync(fileUrl).GetAwaiter().GetResult();
                response.EnsureSuccessStatusCode();

                byte[] fileData = response.Content.ReadAsByteArrayAsync().GetAwaiter().GetResult();
                File.WriteAllBytes(destinationFilePath, fileData);

                UpdateUI($"Файл скачан: {destinationFilePath}");
            }
            catch (Exception ex)
            {
                UpdateUI($"Log (FilesDownloader.cs): Ошибка при скачивании файла: {ex.Message}");
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
                string prefix = parts[0];
                string suffix = parts[parts.Length - 1];
                safeFileName = $"{prefix} ({suffix})";
            }
            else
            {
                safeFileName = parts[0];
            }

            // Очищаем имя файла от недопустимых символов
            safeFileName = Regex.Replace(safeFileName, @"[\\/:*?""<>|]", "_");

            UpdateUI($"Log (FilesDownloader.cs): Безопасное название изображения: {safeFileName}");

            return safeFileName;
        }

        private static string GetFileExtensionFromUrl(string url)
        {
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