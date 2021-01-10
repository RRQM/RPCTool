using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RPCTool
{
    public class AppConfig
    {
        public string DirPath { get; set; }
        public string IP { get; set; }
        public int Port { get; set; }

        private static readonly string path = "App.Config";
        public void Save()
        {
            RRQMXml.XmlTool.SerializeToXmlFile(path,this);
        }

        public static AppConfig Read()
        {
            if (File.Exists(path))
            {
                try
                {
                    return RRQMXml.XmlTool.DeserializeFromXmlFile<AppConfig>(path);
                }
                catch (Exception)
                {

                }
               
            }
            return new AppConfig();
        }
    }
}
