using System.Xml;

namespace MicMuter.Code
{
    internal class ShortcutConfig
    {
        private string _configPath = "";

        public ShortcutConfig(string configPath)
        {
            _configPath = configPath;

            if (System.IO.File.Exists(_configPath) == false)
            {
                CreateBlankConfigFile();
            }
        }

        private void CreateBlankConfigFile()
        {
            XmlDocument xDoc = new XmlDocument();

            XmlDeclaration xDec = xDoc.CreateXmlDeclaration("1.0", "", "");

            XmlElement xRoot = xDoc.CreateElement("root");
            XmlElement xShortcut = xDoc.CreateElement("shortcutkey");
            xRoot.AppendChild(xShortcut);

            xDoc.AppendChild(xDec);
            xDoc.AppendChild(xRoot);

            xDoc.Save(_configPath);
        }

        public string GetShortcutKeyValue()
        {
            XmlDocument xDoc = new XmlDocument();
            xDoc.Load(_configPath);
            return xDoc.LastChild["shortcutkey"].InnerText;
        }

        public void SetShortcutKeyValue(string value)
        {
            XmlDocument xDoc = new XmlDocument();
            xDoc.Load(_configPath);
            xDoc.LastChild["shortcutkey"].InnerText = value;
            xDoc.Save(_configPath);
        }
    }
}
