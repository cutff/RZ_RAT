using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace helper
{
    [ComVisible(true)]
    public class FileManger
    {
        public string encode(object obj)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                new BinaryFormatter().Serialize(ms, obj);
                return Convert.ToBase64String(ms.ToArray());
            }
        }
        [Serializable()]
        public class files : ISerializable
        {
            public string path, name, size, type;
            public files() { }
            public files(SerializationInfo info, StreamingContext ctxt)
            {
                path = (string)info.GetValue("path", typeof(string));
                name = (string)info.GetValue("name", typeof(string));
                size = (string)info.GetValue("size", typeof(string));
                type = (string)info.GetValue("type", typeof(string));
            }

            public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
            {
                info.AddValue("path", path);
                info.AddValue("name", name);
                info.AddValue("size", size);
                info.AddValue("type", type);
            }
        }
        public string getDrivers()
        {
            StringBuilder SB = new StringBuilder();
            foreach (DriveInfo DI in DriveInfo.GetDrives())
            {
                SB.Append(DI.Name);
                SB.Append("||");
            }
            return SB.ToString();
        }
        public string getFilesByPath(string path)
        {
            List<files> listoffiles = new List<files>();
            files backpath = new files();
            backpath.path = path.Replace("\\" + path.Split('\\').Last(), "");
            backpath.name = "...";
            backpath.size = "0";
            backpath.type = "folder";
            listoffiles.Add(backpath);
           
            foreach (string FI in Directory.GetDirectories(path))
            {
                try
                {
                    files n = new files();
                    n.path = Path.Combine(path, FI);
                    n.name = Path.GetFileName(FI);
                    n.size = " ";
                    n.type = "folder";
                    listoffiles.Add(n);
                }
                catch { }
            }
            foreach (string FI in Directory.GetFiles(path))
            {
                try
                {
                    files n = new files();
                    n.path = Path.Combine(path, FI);
                    n.name = Path.GetFileName(FI);
                    n.size = new FileInfo(FI).Length.ToString();
                    n.type = "file";
                    listoffiles.Add(n);
                }
                catch { }
            }
            return encode(listoffiles);
        }
    }
}
