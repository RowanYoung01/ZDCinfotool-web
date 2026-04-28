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
                    compiledDocCategories.AddRange(CategorizeVzdcDocuments(vzdcDocs));
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

                // If the URL is relative (starts with /), the file is already in wwwroot — no fetch needed
                if (doc.Url.StartsWith('/'))
                {
                    logger.LogInformation("Skipping fetch for local PDF at {path}", localPdfPath);
                    continue;
                }

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
        if (url.StartsWith('/'))
            return Path.GetFileName(url);
        var uri = new Uri(url);
        return Path.GetFileName(uri.AbsolutePath);
    }

    private static List<DocumentCategory> CategorizeVzdcDocuments(List<VzdcDocument> docs)
    {
        var categoryRules = new List<(string Name, Func<VzdcDocument, bool> Match)>
        {
            ("General Policy & Facility Administration",
                d => d.Name.StartsWith("vZDC-A-")),

            // Process bulletins before Tower SOPs to avoid "Tower Responsibilities" mismatch
            ("Controller Bulletins",
                d => d.Name.StartsWith("vZDC-") && d.Name.Contains("-B-")),

            ("Charts",
                d => d.Name.StartsWith("vZDC-C-") || d.Name.StartsWith("vZDC-PCT-C-")),

            ("Quick Reference Job Aids",
                d => d.Name.StartsWith("vZDC-Q-")),

            ("Enroute Standard Operating Procedures & Reference",
                d => d.Name.StartsWith("vZDC-ZDC-P-") || d.Name.Contains("Deconsolidation")),

            ("ATC Tower Standard Operating Procedures & Reference",
                d => d.Name.Contains(" Tower") && !d.Name.Contains("ATCT/TRACON")),

            ("ATCT/TRACON Standard Operating Procedures & Reference",
                d => d.Name.Contains("ATCT/TRACON")),

            ("TRACON Standard Operating Procedures & Reference",
                d => d.Name.StartsWith("PCT ")),

            ("Letters of Agreement",
                d => d.Name.StartsWith("ZBW |") || d.Name.StartsWith("ZID |") ||
                     d.Name.StartsWith("ZJX |") || d.Name.StartsWith("ZNY |") ||
                     d.Name.StartsWith("ZOB |") || d.Name.StartsWith("ZTL |") ||
                     d.Name.StartsWith("PCT-ZNY |") || d.Name.StartsWith("PHL-PCT |") ||
                     d.Name.StartsWith("USNv |")),

            ("Washington Special Flight Rules Area (SFRA)",
                d => d.Name.Contains("SFRA")),

            ("vATIS",
                d => d.Name.Contains("ATIS")),

            ("vVSCS",
                d => d.Name.Contains("VSCS") || d.Name.StartsWith("User Guide")),
        };

        var assigned = new HashSet<string>();
        var result = new List<DocumentCategory>();

        foreach (var (name, match) in categoryRules)
        {
            var matched = docs.Where(d => !assigned.Contains(d.Key) && match(d)).ToList();
            if (matched.Count == 0) continue;

            result.Add(new DocumentCategory
            {
                Name = name,
                Documents = matched.Select(d => new Document(d.Name, d.Url)).ToList()
            });
            foreach (var d in matched)
                assigned.Add(d.Key);
        }

        var unmatched = docs.Where(d => !assigned.Contains(d.Key)).ToList();
        if (unmatched.Count > 0)
        {
            result.Add(new DocumentCategory
            {
                Name = "ZDC Publications",
                Documents = unmatched.Select(d => new Document(d.Name, d.Url)).ToList()
            });
        }

        return result;
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