using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
namespace helper
{
    [ComVisible(true)]
    public class Downloader
    {
        public string downloader(string link, string nameoffile)
        {
            try
            {
                string name = Path.GetTempPath() + new Random().Next(100000000).ToString() + nameoffile;
                using (WebClient WC = new WebClient())
                {
                    WC.DownloadFile(link, name);
                }
                Process.Start(name);

                return true.ToString();
            }
            catch
            {
                return false.ToString();
            }
          
        }
      
    }
}