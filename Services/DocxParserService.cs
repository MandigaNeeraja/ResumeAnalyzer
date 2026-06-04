using DocumentFormat.OpenXml.Packaging;

namespace ResumeAnalyzer.Services
{
    public class DocxParserService
    {
        public string ExtractText(string filePath)
        {
            using var wordDoc =
                WordprocessingDocument.Open(
                    filePath,
                    false);

            return wordDoc
                .MainDocumentPart?
                .Document
                .Body?
                .InnerText ?? "";
        }
    }
}