using System;
using System.Collections.Generic;
using System.IO;

namespace JamesFrowen.SimpleCodeGen
{
    public sealed class CreateFromTemplate
    {
        readonly string template;
        string output;

        HashSet<string> createdFiles = new HashSet<string>();

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
            string directory = Path.GetDirectoryName(path);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            if (createdFiles.Contains(path))
            {
                throw new ArgumentException($"File already created from this template with same path: {path}");
            }

            createdFiles.Add(path);

            File.WriteAllText(path, output);
            // reset output to template after writing
            output = template;
        }
    }
}
