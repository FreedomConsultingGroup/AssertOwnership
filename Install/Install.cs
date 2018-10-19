using System;
using System.Windows.Forms;
using System.IO;
using System.Xml;

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

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load("Install.xml");
            XmlElement root = xmlDoc.DocumentElement;

            Copy(Path.Combine(sourcePath, "Files"), "", root.FirstChild);
            SetVars(root.LastChild);
        }

        static void Copy(string path, string subfolder, XmlNode folder)
        {
            string filename;
            if(folder.Attributes != null)
            {
                subfolder = Path.Combine(subfolder, folder.Attributes["name"].Value);
            }
            XmlNodeList nodes = folder.SelectNodes("File");
            foreach(XmlNode node in nodes)
            {
                filename = node.FirstChild.InnerText;

                if (!Directory.Exists(Path.Combine(destPath, subfolder)))
                {
                    Directory.CreateDirectory(Path.Combine(destPath, subfolder));
                }
                try
                {
                    File.Copy(Path.Combine(path, filename), Path.Combine(destPath, subfolder, filename));
                }
                catch (FileNotFoundException e)
                {
                    throw e;
                }
            }

            nodes = folder.SelectNodes("Folder");
            foreach(XmlNode node in nodes)
            {
                Copy(path, subfolder, node);
            }
        }

        static void SetVars(XmlNode varRoot)
        {
            foreach (XmlNode node in varRoot.SelectNodes("Variable"))
            {
                string name = node.Attributes["name"].Value;
                string value = node.InnerText.Replace("%INSTALL_DIRECTORY%", destPath).Replace(@"\\", @"\");
                Environment.SetEnvironmentVariable(name, value);
            }
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            startInfo.FileName = "cmd.exe";
            startInfo.Arguments = "/C iisreset /RESTART";
            process.StartInfo = startInfo;
            process.Start();
        }
    }
}
