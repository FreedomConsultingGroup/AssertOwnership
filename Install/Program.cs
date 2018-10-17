using System;
using System.Windows.Forms;
using System.IO;

namespace FCG.AssertOwnership.Install
{
    class Install
    {
        static string destPath, sourcePath;

        // Copies all files in the Files directory to the specified folder, creating directories that do not exist
        static void Main(string[] args)
        {
            Console.Out.Write("Enter the name of the directory you wish to install the application to. If the directory does not exist, it will be created" + Environment.NewLine + "Path: ");
            destPath = Console.ReadLine();

            sourcePath = Application.StartupPath;
            if (!Directory.Exists(destPath))
            {
                Directory.CreateDirectory(destPath);
            }

            Copy(Path.Combine(sourcePath, "Files"), null);
        }

        static void Copy(string path, string subfolder)
        {
            string filename;
            foreach (string file in Directory.GetFiles(path))
            {
                filename = Path.GetFileName(file);

                if(subfolder == null)
                {
                    if (!Directory.Exists(destPath))
                    {
                        Directory.CreateDirectory(destPath);
                    }
                    File.Copy(file, Path.Combine(destPath, filename));
                }
                else
                {
                    if (!Directory.Exists(Path.Combine(destPath, subfolder)))
                    {
                        Directory.CreateDirectory(Path.Combine(destPath, subfolder));
                    }
                    File.Copy(file, Path.Combine(destPath, subfolder, filename));
                }
            }
            foreach(string directory in Directory.GetDirectories(path))
            {
                string sub = directory.Replace(sourcePath + "\\Files\\", "");
                Copy(directory, sub);
            }
        }
    }
}
