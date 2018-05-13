using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace helper
{
    [ComVisible(true)]
    public class ProcessManger
    {
        public string encode(object obj)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                new BinaryFormatter().Serialize(ms, obj);
                return Convert.ToBase64String(ms.ToArray());
            }
        }
        public object decode(string base64String)
        {
            byte[] bytes = Convert.FromBase64String(base64String);
            using (MemoryStream ms = new MemoryStream(bytes, 0, bytes.Length))
            {
                ms.Write(bytes, 0, bytes.Length);
                ms.Position = 0;
                return new BinaryFormatter().Deserialize(ms);
            }
        }
        [Serializable()]
        public class process : ISerializable
        {
            public string name = null;
            public string pid = null;
            public process(){}
            public process(SerializationInfo info, StreamingContext ctxt)
            {
                name = (string)info.GetValue("name", typeof(string));
                pid = (String)info.GetValue("pid", typeof(string));
            }

            public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
            {
                info.AddValue("name", name);
                info.AddValue("pid", pid);
            }
        }
        public string getAllProcess()
        {
            List<process> listprocess = new List<process>();
            foreach (Process Process1 in Process.GetProcesses())
            {
                process PS = new process();
                PS.name = Process1.ProcessName;
                PS.pid = Process1.Id.ToString();
                listprocess.Add(PS);
            }
            return encode(listprocess);
        }
        public string killprocessByName(string nameOfProcess)
        {
            if (nameOfProcess.ToUpper().Contains(".EXE"))
                nameOfProcess = Path.GetFileNameWithoutExtension(nameOfProcess);
            foreach (var process in Process.GetProcessesByName(nameOfProcess))
            {
                process.Kill();
            }
            int i = 0;
            foreach (var process in Process.GetProcessesByName(nameOfProcess))
            {
                i++;
            }
            if (i == 0)
            { return true.ToString(); }
            else { return false.ToString(); }

        }
    }
}

