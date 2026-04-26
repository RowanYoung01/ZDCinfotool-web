using Coravel.Invocable;
using Microsoft.Extensions.Options;
using ZdcReference.Features.Docs.Models;
using ZdcReference.Features.Docs.Repositories;

namespace ZdcReference.Features.Docs.ScheduledJobs;

public class FetchAndStoreDocs(
    ILogger<FetchAndStoreDocs> logger, 
    HttpClient httpClient, 
    IWebHostEnvironment webHostEnvironment, 
    IOptionsMonitor<AppSettings> appSettings, 
    DocumentRepository documentRepository) : IInvocable
{
    public async Task Invoke()
    {
        List<DocumentCategory> compiledDocCategories = [];

        try
        {
            var apiUrl = appSettings.CurrentValue.Urls.ZoaDocumentsApiEndpoint;
            if (!string.IsNullOrWhiteSpace(apiUrl))
            {
                logger.LogInformation("Fetching ZDC docs from {url}", apiUrl);
                var fetchedDocCategories = await httpClient.GetFromJsonAsync<List<ZoaDocumentCategory>>(apiUrl);
                if (fetchedDocCategories is not null)
                {
                    compiledDocCategories.AddRange(fetchedDocCategories.Select(c => c.ToGenericDocumentCategory()));
                }
                else
                {
                    logger.LogInformation("Fetched ZDC documents null or zero");
                }
            }

            var customDocCategories = appSettings.CurrentValue.CustomDocuments;
            compiledDocCategories.AddRange(customDocCategories.Select(c => c.ToGenericDocumentCategory()));

            logger.LogInformation("Successfully fetched ZDC and custom docs");
        }
        catch (Exception e)
        {
            logger.LogError("Error while fetching ZDC docs: {ex}", e.ToString());
        }

        var tasks = new List<Task>();
        foreach (var category in compiledDocCategories)
        {
            foreach (var doc in category.Documents)
            {
                var pdfName = GetPdfNameFromUrl(doc.Url);
                var localPdfPath = Path.ChangeExtension(Path.Combine(PdfFolderPath, pdfName), ".pdf");

                // Always write new file
                try
                {
                    var task = WriteRemotePdfToLocal(doc.Url, localPdfPath);
                    tasks.Add(task);
                    logger.LogInformation("Found pdf at {url} and writing at {path}", doc.Url, localPdfPath);
                }
                catch (Exception e)
                {
                    logger.LogWarning("Could not write PDF from {url} at {path}. Error: {e}", doc.Url, localPdfPath, e);
                }
            }
        }

        await Task.WhenAll(tasks);
        documentRepository.ClearAllDocumentCategories();
        documentRepository.AddDocumentCategories(compiledDocCategories);
    }

    private string PdfFolderPath => Path.Combine(webHostEnvironment.WebRootPath, appSettings.CurrentValue.DocumentsPdfPath);

    private static string GetPdfNameFromUrl(string url)
    {
        var uri = new Uri(url);
        return Path.GetFileName(uri.AbsolutePath);
    }

    private async Task WriteRemotePdfToLocal(string url, string path)
    {
        try
        {
            await using var pdfStream = await httpClient.GetStreamAsync(url);

            var dirPath = Path.GetDirectoryName(path);
            if (dirPath is not null && !Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }

            await using var pdfNewFile = File.Create(path);
            await pdfStream.CopyToAsync(pdfNewFile);
            logger.LogInformation("Wrote new PDF to {path}", path);
        }
        catch (Exception e)
        {
            logger.LogError("Error while fetching PDF from {url}: {ex}", url, e);
        }

    }
}