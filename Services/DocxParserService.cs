using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace ResumeAnalyzer.Services
{
    public class DocxParserService
    {
        public string ExtractText(string filePath)
        {
            var uris = ExtractHyperlinkUris(filePath);
            using var wordDoc = WordprocessingDocument.Open(filePath, false);

            var bodyText = wordDoc.MainDocumentPart?.Document?.Body?.InnerText ?? "";
            if (uris.Count == 0)
                return bodyText;

            return bodyText + " " + string.Join(" ", uris);
        }

        public IReadOnlyList<string> ExtractHyperlinkUris(string filePath)
        {
            var uris = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            using var wordDoc = WordprocessingDocument.Open(filePath, false);
            var mainPart = wordDoc.MainDocumentPart;
            if (mainPart == null)
                return Array.Empty<string>();

            foreach (var rel in mainPart.HyperlinkRelationships)
            {
                var uri = rel.Uri?.ToString();
                if (!string.IsNullOrWhiteSpace(uri))
                    uris.Add(uri);
            }

            var body = mainPart.Document?.Body;
            if (body != null)
            {
                foreach (var hyperlink in body.Descendants<Hyperlink>())
                {
                    var relId = hyperlink.Id?.Value;
                    if (string.IsNullOrWhiteSpace(relId))
                        continue;

                    var rel = mainPart.HyperlinkRelationships
                        .FirstOrDefault(r => r.Id == relId);

                    if (rel != null)
                    {
                        var uri = rel.Uri?.ToString();
                        if (!string.IsNullOrWhiteSpace(uri))
                            uris.Add(uri);
                    }
                }
            }

            return uris.ToList();
        }
    }
}
