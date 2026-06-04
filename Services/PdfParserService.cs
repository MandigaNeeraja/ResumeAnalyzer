using UglyToad.PdfPig;

namespace ResumeAnalyzer.Services
{
    public class PdfParserService
    {
        public string ExtractText(string filePath)
        {
            using var document =
                PdfDocument.Open(filePath);

            string text = "";

            foreach (var page in document.GetPages())
            {
                text += page.Text;
            }

            return text;
        }
    }
}