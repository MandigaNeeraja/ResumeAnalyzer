using System.Text;
using UglyToad.PdfPig;

namespace ResumeAnalyzer.Services
{
    public class PdfParserService
    {
        public string ExtractText(string filePath)
        {
            using var document = PdfDocument.Open(filePath);
            var builder = new StringBuilder();

            foreach (var page in document.GetPages())
            {
                builder.Append(page.Text);

                foreach (var link in page.GetHyperlinks())
                {
                    if (!string.IsNullOrWhiteSpace(link.Uri))
                        builder.Append(' ').Append(link.Uri);

                    if (!string.IsNullOrWhiteSpace(link.Text))
                        builder.Append(' ').Append(link.Text);
                }
            }

            return builder.ToString();
        }

        public IReadOnlyList<string> ExtractHyperlinkUris(string filePath)
        {
            var uris = new List<string>();

            using var document = PdfDocument.Open(filePath);
            foreach (var page in document.GetPages())
            {
                foreach (var link in page.GetHyperlinks())
                {
                    if (!string.IsNullOrWhiteSpace(link.Uri))
                        uris.Add(link.Uri);
                }
            }

            return uris;
        }
    }
}
