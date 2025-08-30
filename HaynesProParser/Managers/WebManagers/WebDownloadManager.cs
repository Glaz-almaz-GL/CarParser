using Svg;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace HaynesProParser.Managers.WebManagers
{
    public static class WebDownloadManager
    {
        public static bool SortByFolders { get; set; }

        public static Dictionary<string, string> ExtensionToDirectoryMap { get; } = new Dictionary<string, string>
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
            { "SourceCodes", ".c;.cpp;.h;.hpp;.cs;.vb;.vbnet;.java;.js;.ts;.go;.swift;.php;.py;.rb;.pl;.sh;.bat;.cmd;.perl;.ruby;.bash;.fish;.rust;.dart;.typescript;.javascript;.scala;.kotlin;.groovy;.r;.matlab;.octave;.hs;.lua;.erlang;.elixir;.haskell;.coffeescript;.sass;.scss;.less;.vue;.jsx;.tsx;.svelte;.asm;.cmake;.make;.gradle;.ninja;.dockerfile;.sql;.graphql;.proto;.rst;.md;.markdown;.tex;.latex;.asciidoc;.pug;.haml;.erb;.jade;.blade;.twig;.mustache;.handlebars;.ejs;.liquid;.razor;.asp;.aspx;.jsp;.asp;.aspx;.jsp;.jspx;.cfm;.cfml;.tpl;.blade;.blade.php;.blade.html;.blade.htm;.blade.xml;.blade.json;.blade.md;.blade.markdown;.blade.tex;.blade.latex;.blade.asciidoc;.blade.pug;.blade.haml;.blade.erb;.blade.jade;.blade.blade;.blade.twig;.blade.mustache;.blade.handlebars;.blade.ejs;.blade.liquid;.blade.razor" }
        };

        public static async Task DownloadFileAsync(string fileUrl, string fileName = "", string fileExtension = "", string[] folderNames = null, IReadOnlyCollection<OpenQA.Selenium.Cookie> cookies = null)
        {
            string cookieHeader = "";
            if (cookies != null)
            {
                cookieHeader = string.Join("; ", cookies.Select(c => $"{c.Name}={c.Value}"));
            }

            string fileNameToUse = string.IsNullOrEmpty(fileName) ? GetFileNameFromUrl(fileUrl) : fileName;
            string fileExtensionToUse = string.IsNullOrEmpty(fileExtension) ? GetFileExtensionFromUrl(fileUrl) : fileExtension;

            string newDirPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "HaynesPro Parser", Path.Combine(folderNames));

            if (SortByFolders)
            {
                newDirPath = GetExtensionType(fileExtensionToUse, newDirPath);
            }

            if (File.Exists(Path.Combine(newDirPath, fileNameToUse + fileExtensionToUse)))
            {
                return;
            }

            try
            {
                using HttpClient client = new();

                if (!string.IsNullOrEmpty(cookieHeader))
                {
                    client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36...");
                    client.DefaultRequestHeaders.Add("Cookie", cookieHeader);
                    client.DefaultRequestHeaders.Referrer = new Uri("https://www.workshopdata.com/");
                }

                Directory.CreateDirectory(newDirPath);
                string safeFileName = GetSafeFileName(fileNameToUse);

                if (fileExtensionToUse.Equals(".svgz", StringComparison.OrdinalIgnoreCase))
                {
                    string destinationGzipPath = Path.Combine(newDirPath, safeFileName + ".svg.gzip");
                    string destinationSvgPath = Path.Combine(newDirPath, safeFileName + ".svg");
                    string destinationPngPath = Path.Combine(newDirPath, safeFileName + ".png");
                    await ConvertGZIPAsync(client, fileUrl, destinationGzipPath, destinationSvgPath, destinationPngPath);
                }
                else
                {
                    string destinationFilePath = Path.Combine(newDirPath, safeFileName + fileExtensionToUse);
                    await DownloadRegularFileAsync(client, fileUrl, destinationFilePath);
                }
            }
            catch (Exception ex)
            {
                UpdateUI($"Log (FilesDownloader.cs): Произошла ошибка: {ex.Message}");
            }

            UpdateUI("Log (FilesDownloader.cs): Скачивание и конвертирование изображений завершено");
        }

        private static string GetExtensionType(string extension, string newDirPath)
        {
            foreach (KeyValuePair<string, string> kvp in ExtensionToDirectoryMap)
            {
                string[] extensions = kvp.Value.Split(';');
                if (extensions.Contains(extension))
                {
                    newDirPath = Path.Combine(newDirPath, kvp.Key);
                    return newDirPath;
                }
            }

            return "Unknowns";
        }

        private static async Task ConvertGZIPAsync(HttpClient client, string imgUrl, string destinationGzipPath, string destinationSvgPath, string destinationPngPath)
        {
            try
            {
                HttpResponseMessage response = client.GetAsync(imgUrl).GetAwaiter().GetResult();
                response.EnsureSuccessStatusCode();

                byte[] imageData = response.Content.ReadAsByteArrayAsync().GetAwaiter().GetResult();
                await File.WriteAllBytesAsync(destinationGzipPath, imageData);

                await using (FileStream gzipStream = new FileStream(destinationGzipPath, FileMode.Open))
                await using (GZipStream decompressionStream = new GZipStream(gzipStream, CompressionMode.Decompress))
                await using (FileStream outputStream = new FileStream(destinationSvgPath, FileMode.Create))
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

        private static async Task DownloadRegularFileAsync(HttpClient client, string fileUrl, string destinationFilePath)
        {
            try
            {
                HttpResponseMessage response = client.GetAsync(fileUrl).GetAwaiter().GetResult();
                response.EnsureSuccessStatusCode();

                byte[] fileData = response.Content.ReadAsByteArrayAsync().GetAwaiter().GetResult();
                await File.WriteAllBytesAsync(destinationFilePath, fileData);

                UpdateUI($"Файл скачан: {destinationFilePath}");
            }
            catch (Exception ex)
            {
                UpdateUI($"Log (FilesDownloader.cs): Ошибка при скачивании файла: {ex.Message}");
            }
        }

        public static string GetSafeFileName(string fileName)
        {
            char[] invalidChars = Path.GetInvalidFileNameChars();
            return string.Concat(fileName.Split(invalidChars));
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
            Debug.WriteLine($"[{DateTime.Now}]: {message}");
        }
    }
}
