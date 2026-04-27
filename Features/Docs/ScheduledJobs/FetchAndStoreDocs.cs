using Coravel.Invocable;
using Microsoft.Extensions.Options;
using ZdcReference.Features.Docs.Models;
using ZdcReference.Features.Docs.Repositories;
using System.Net.Http.Json;

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
            var zoaApiUrl = appSettings.CurrentValue.Urls.ZoaDocumentsApiEndpoint;
            if (!string.IsNullOrWhiteSpace(zoaApiUrl))
            {
                logger.LogInformation("Fetching ZOA docs from {url}", zoaApiUrl);
                var fetchedDocCategories = await httpClient.GetFromJsonAsync<List<ZoaDocumentCategory>>(zoaApiUrl);
                if (fetchedDocCategories is not null)
                {
                    compiledDocCategories.AddRange(fetchedDocCategories.Select(c => c.ToGenericDocumentCategory()));
                }
                else
                {
                    logger.LogInformation("Fetched ZOA documents null or zero");
                }
            }

            var vzdcApiUrl = appSettings.CurrentValue.Urls.VzdcDocumentsApiEndpoint;
            if (!string.IsNullOrWhiteSpace(vzdcApiUrl))
            {
                logger.LogInformation("Fetching VZDC docs from {url}", vzdcApiUrl);
                var vzdcDocs = await httpClient.GetFromJsonAsync<List<VzdcDocument>>(vzdcApiUrl);
                if (vzdcDocs is not null && vzdcDocs.Count > 0)
                {
                    compiledDocCategories.Add(new DocumentCategory
                    {
                        Name = "ZDC Publications",
                        Documents = vzdcDocs.Select(d => new Document(d.Name, d.Url)).ToList()
                    });
                }
                else
                {
                    logger.LogInformation("Fetched VZDC documents null or zero");
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