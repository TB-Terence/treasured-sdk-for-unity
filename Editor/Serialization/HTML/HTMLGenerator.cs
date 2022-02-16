using System.IO;
using System.Text.RegularExpressions;

namespace Treasured.UnitySdk
{
    internal class HTMLGenerator
    {
        public const string TemplateFileRelativePath = "Packages/com.treasured.unitysdk/Resources/template.html";
        
        private static Regex s_regex = new Regex(@"([^A-z0-9])(#CODE#){1}([^A-z0-9])");

        public static void Generate(string outputPath, string replacement)
        {
            string templatePath = Path.GetFullPath(TemplateFileRelativePath);
            if (!File.Exists(templatePath))
            {
                throw new FileNotFoundException("HTML template file not found.");
            }
            string template = File.ReadAllText(templatePath);
            string result = s_regex.Replace(template, $"$1{replacement}$3");
            File.WriteAllText(outputPath, result);
        }
    }
}
