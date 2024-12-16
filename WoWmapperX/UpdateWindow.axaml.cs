using Avalonia;
using Avalonia.Controls;
using System.Text.RegularExpressions;
using System.Text;
using Avalonia.Input;
using System;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO.Compression;
using System.IO;
using System.Net.Http;
using Avalonia.Controls.ApplicationLifetimes;
using WoWmapperX.AvaloniaImpl;
using System.Linq;

namespace WoWmapperX;

public partial class UpdateWindow : Window
{
    private UpdateRelease _release; 

    #region "Simple Markdown parser"
    [GeneratedRegex(@"\*\*(.*?)\*\*", RegexOptions.Compiled)]
    private static partial Regex BoldRegex();

    [GeneratedRegex(@"\*(.*?)\*", RegexOptions.Compiled)]
    private static partial Regex ItalicRegex();

    [GeneratedRegex(@"\[(.*?)\]\((.*?)\)", RegexOptions.Compiled)]
    private static partial Regex LinkRegex();  

    private string ParseBody(string markdownText)
    {
        StringBuilder plainText = new StringBuilder();
        var lines = markdownText.Split(new[] { '\r', '\n' });
        bool ignoreBlock = false;

        foreach (var line in lines)
        {
            var processedLine = line.Trim();

            if (processedLine == "<!-- ignore -->")
            {
                ignoreBlock = true;
                continue;
            }

            if (processedLine == "<!-- endignore -->")
            {
                ignoreBlock = false;
                continue;
            }

            if (ignoreBlock)
            {
                continue;
            }

            if (processedLine.StartsWith("- "))
            {
                processedLine = $"• {processedLine.Substring(2)}";
            }

            processedLine = BoldRegex().Replace(processedLine, "$1");
            processedLine = ItalicRegex().Replace(processedLine, "$1");
            processedLine = LinkRegex().Replace(processedLine, "$1");

            plainText.AppendLine(processedLine);
        }

        return plainText.ToString();
    }
    #endregion

    public UpdateWindow(UpdateRelease release)
    {
        InitializeComponent();

        _release = release;

        this.TitleBar.WindowIcon.IsVisible = false;
        this.TitleBar.SystemChromeTitle.Margin = new Thickness(10, 0, 0, 0);
        this.TitleBar.SystemChromeTitle.Text = this.Title;
        this.TitleBar.SettingsButton.IsVisible = false;


        TextReleaseTitle.Text = _release.Name;
        TextReleaseNotes.Text = ParseBody(_release.Body);
    }


    public async void ButtonDownload_Click(object sender, TappedEventArgs e)
    {
        PanelButton.IsVisible = false;
        GridProgress.IsVisible = true;

        await DownloadUpdate();
    }

    private async Task DownloadUpdate()
    {
        try
        { 
            if (File.Exists("_update.zip"))
                File.Delete("_update.zip");

            using var httpClient = new HttpClient();
            using var response = await httpClient.GetAsync(_release.DownloadURL, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();

            long totalBytes = response.Content.Headers.ContentLength ?? 0;
            long downloadedBytes = 0;

            using (var fileStream = new FileStream("_update.zip", FileMode.Create, FileAccess.Write, FileShare.None))
            using (var stream = await response.Content.ReadAsStreamAsync())
            {
                byte[] buffer = new byte[8192];
                int bytesRead;
                while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    await fileStream.WriteAsync(buffer, 0, bytesRead);
                    downloadedBytes += bytesRead;

                    // Report progress
                    double progress = (double)downloadedBytes / totalBytes * 100;
                    ProgressDownload.Value = progress;
                }
            }


            using (var updateZip = ZipFile.OpenRead("_update.zip"))
            {
                var updater = updateZip.Entries.First(entry => entry.Name == "WoWmapperX_Updater.exe");
                updater.ExtractToFile("WoWmapperX_Updater.exe", true);
            } 

            // Start downloaded updater
            Process.Start("WoWmapperX_Updater.exe", "_update.zip");


            ((IClassicDesktopStyleApplicationLifetime)Avalonia.Application.Current.ApplicationLifetime).Shutdown(0);
        }
        catch (Exception ex)
        {
            await MessageBox.Show(ex.Message, "Update error");
            Close();
        }
    }

    private void ButtonCancel_Click(object sender, TappedEventArgs e)
    { 
        Close();
    }


}