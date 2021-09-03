using System.IO;
namespace JamesFrowen.SimpleCodeGen
{
    public sealed class CreateFromTemplate
    {
        readonly string template;
        string output;

        public CreateFromTemplate(string templatePath)
        {
            template = File.ReadAllText(templatePath);
            output = template;
        }

        public void Replace(string oldValue, string newValue)
        {
            output = output.Replace(oldValue, newValue);
        }
        public void Replace(string oldValue, object newValue)
        {
            output = output.Replace(oldValue, newValue.ToString());
        }

        public void WriteToFile(string path)
        {
            File.WriteAllText(path, output);
            // reset output to template after writing
            output = template;
        }
    }
}
