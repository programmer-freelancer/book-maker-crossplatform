using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.IO;
using Ionic.Zip;
using System.Diagnostics;
using System.Threading;
using System.Xml;
using Remote_Computer;
using System.Security.AccessControl;
namespace CompressionSample
{
    class ZipUtil
    {
        public string sProjectName, sBookNames, sAuthors, sContents, sWebsites, sEmails, sPhones,sVersions,sourceFile;
        public string AppID="", ClientID="";
        public string[] files;

        public int TypeBook;

        public string platformType = "";
        public bool EnterpriceOutput = true;

        public void CompressFile()
        {
            //int waitTime;

            frmmain.currentCopyFile = "Prepare to create a Android Book...";
            ExportAPK("d");

            if (Directory.Exists(Path.Combine(Application.StartupPath,"tool\\book")) == false)
            {
                frmmain.completeCompile = false;
                MessageBox.Show("کتابساز با خطا مواجه شد.خطا ممکن است از اجرای نرم افزار جاوا باشد.\nلطفا نرم افزار را فعال و یا دوباره نصب کنید","خطا",MessageBoxButtons.OK,MessageBoxIcon.Warning);
                return;
            }

            string xmlPath = Path.Combine(Application.StartupPath , "tool\\book\\AndroidManifest.xml");

            #region Permission
            try
            {
                frmmain.currentCopyFile = "Change Permission Folder";
                Thread.Sleep(300);
                System.IO.DirectoryInfo dInfo = new System.IO.DirectoryInfo(Path.Combine(Application.StartupPath , "tool\\book"));

                // Get a DirectorySecurity object that represents the 
                // current security settings.
                DirectorySecurity dSecurity = dInfo.GetAccessControl();

                // Add the FileSystemAccessRule to the security settings. 
                dSecurity.AddAccessRule(
                    new FileSystemAccessRule(
                        new System.Security.Principal.SecurityIdentifier(
                            System.Security.Principal.WellKnownSidType.BuiltinUsersSid,
                            null
                        ),
                        FileSystemRights.DeleteSubdirectoriesAndFiles,
                    //FileSystemRights.FullControl,
                        AccessControlType.Allow
                    )
                );

                // Set the new access settings.
                dInfo.SetAccessControl(dSecurity);
            }
            catch (Exception e2)
            {
                File.AppendAllText(Application.StartupPath + "\\log.txt", e2.Message + "\r\n");
            }
            #endregion


            try
            {
                foreach (string file in files)
                {
                    try
                    {
                        if (file.EndsWith("Thumbs.db")) continue;
                        File.Copy(file, Path.Combine(Path.Combine(Application.StartupPath ,"tool\\book\\assets"), new FileInfo(file).Name), true);
                        frmmain.currentCopyFile = file;
                    }
                    catch (Exception e1)
                    {
                        frmmain.completeCompile = false;
                        File.AppendAllText(Application.StartupPath + "\\log.txt", e1.Message + "\r\n");
                        return;
                    }
                    Application.DoEvents();
                }
                if (Directory.Exists(Application.StartupPath + "\\" + sProjectName + "\\lib"))
                {
                    string[] Libfile = Directory.GetFiles(Application.StartupPath + "\\" + sProjectName + "\\lib");
                    foreach (string Libfile1 in Libfile)
                    {
                        try
                        {
                            File.Copy(Libfile1, Path.Combine(Application.StartupPath + "\\tool\\book\\assets", new FileInfo(Libfile1).Name), true);
                            frmmain.currentCopyFile = "Copy Library File: " + Libfile1;
                            Thread.Sleep(300);
                        }
                        catch (Exception e1)
                        {
                            File.AppendAllText(Application.StartupPath + "\\log.txt", e1.Message + "\r\n");

                        }
                        Application.DoEvents();
                    }
                }

                if (File.Exists(Path.Combine(Application.StartupPath + "\\" + sProjectName, "icon.png")))
                {
                    File.Copy(Path.Combine(Application.StartupPath + "\\" + sProjectName, "icon.png"), Path.Combine(Application.StartupPath + "\\tool\\book\\res\\drawable", "icon.png"), true);
                    frmmain.currentCopyFile = Application.StartupPath + "\\" + sProjectName + "\\icon.png";
                }

                if (TypeBook == 1)
                {
                    File.WriteAllText(Application.StartupPath + "\\tool\\book\\assets\\short_book","");
                }

                #region write manifest file
                XmlDocument doc = new XmlDocument();
                doc.Load(Path.Combine(Application.StartupPath + "\\tool\\book", "AndroidManifest.xml"));

                if (sVersions.Length < 3) sVersions = "1.0.0";

                XmlNode versionNode = doc.SelectSingleNode(@"manifest");
                versionNode.Attributes["package"].InnerText = "student." + sProjectName;
                versionNode.Attributes["android:versionName"].InnerText = sVersions;
                versionNode.Attributes["android:versionCode"].InnerText = sVersions.Substring(0, 1);

                XmlNode LabelNode1 = doc.SelectSingleNode(@"manifest/application");
                LabelNode1.Attributes["android:label"].InnerText = sBookNames;

                XmlNodeList Activities = doc.SelectSingleNode(@"manifest/application").ChildNodes;
                
                for (int i = 0; i < Activities.Count; i++)
                {
                    XmlNode x1 = Activities[i];
                    if (x1.Name.ToLower() == "activity")
                    {
                        try
                        {
                            x1.Attributes["android:label"].InnerText = sBookNames;
                        }
                        catch (Exception)
                        {

                        }
                    }
                }

                doc.Save(Path.Combine(Application.StartupPath + "\\tool\\book", "AndroidManifest.xml"));
                #endregion

                Directory.Move(Application.StartupPath + "\\tool\\book\\smali\\student\\templatebook", Application.StartupPath + "\\tool\\book\\smali\\student\\" + sProjectName);
                
                frmmain.currentCopyFile = "Change Book Package Name";
                changeAPKPackage(sProjectName);
                frmmain.currentCopyFile = "Starting Compile...";

                ExportAPK("b");

                File.Copy(Application.StartupPath + "\\tool\\book\\dist\\book.apk", Application.StartupPath + "\\tool\\output\\book_out.apk", true);
                Directory.Delete(Application.StartupPath + "\\tool\\book", true);

                SignAPK();
                File.Delete(Application.StartupPath + "\\tool\\output\\book_out.apk");

                //Thread.Sleep(waitTime);
                frmmain.completeCompile = true;
                myClass.clearFolder(Application.StartupPath + "\\tool\\book");

                if (Directory.Exists(Application.StartupPath + "\\tool\\book"))
                    Directory.Delete(Application.StartupPath + "\\tool\\book");
            }
            catch (Exception)
            {
                frmmain.completeCompile = false;
            }
        }

        private XmlAttribute AddAttribute(XmlDocument xd1, string name,string value)
        {
            XmlAttribute xa1 = xd1.CreateAttribute(name);
            xa1.Value = value;
            return xa1;
        }
		
        public void CrossPlatform()
        {
            //int waitTime;

            frmmain.currentCopyFile = "Create a Cross Platform Book...";
 
            if (Directory.Exists(Path.Combine(Application.StartupPath, "tool\\cross\\book")) == false)
            {
                frmmain.completeCompile = false;
                MessageBox.Show("فایل های مربوط به ساخت کتاب وجود ندارد.لطفا دوباره کتابساز را نصب کنید", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                myClass.clearFolder(Path.Combine(Application.StartupPath, "tool\\cross\\book\\Files"));

                if (Directory.Exists(Path.Combine(Application.StartupPath, "tool\\cross\\book\\Files")))
                    Directory.Delete(Path.Combine(Application.StartupPath, "tool\\cross\\book\\Files"));

                DirectoryCopy(Path.Combine(Application.StartupPath, "tool\\cross\\Files"), Path.Combine(Application.StartupPath, "tool\\cross\\book\\Files"), true);
            }
            catch
            {
                frmmain.completeCompile = false;
            }
            
            try
            {
                foreach (string file in files)
                {
                    try
                    {
                        if (file.EndsWith("Thumbs.db")) continue;
                        File.Copy(file, Path.Combine(Path.Combine(Application.StartupPath, "tool\\cross\\book\\Files"), new FileInfo(file).Name), true);
                        frmmain.currentCopyFile = file;
                    }
                    catch (Exception e1)
                    {
                        File.AppendAllText(Application.StartupPath + "\\log.txt", e1.Message + "\r\n");

                    }
                    Application.DoEvents();
                }

                if (Directory.Exists(Application.StartupPath + "\\" + sProjectName + "\\lib"))
                {
                    string[] Libfile = Directory.GetFiles(Application.StartupPath + "\\" + sProjectName + "\\lib");
                    foreach (string Libfile1 in Libfile)
                    {
                        try
                        {
                            File.Copy(Libfile1, Path.Combine(Application.StartupPath + "\\tool\\cross\\book\\Files", new FileInfo(Libfile1).Name), true);
                            frmmain.currentCopyFile = "Copy Library File: " + Libfile1;
                            Thread.Sleep(300);
                        }
                        catch (Exception e1)
                        {
                            File.AppendAllText(Application.StartupPath + "\\log.txt", e1.Message + "\r\n");

                        }
                        Application.DoEvents();
                    }
                }

                if (File.Exists(Path.Combine(Application.StartupPath + "\\" + sProjectName, "icon.png")))
                {
                    File.Copy(Path.Combine(Application.StartupPath + "\\" + sProjectName, "icon.png"), Path.Combine(Application.StartupPath + "\\tool\\cross\\book\\Files", "icon.png"), true);
                    frmmain.currentCopyFile = Application.StartupPath + "\\" + sProjectName + "\\icon.png";
                }
                
                frmmain.currentCopyFile = "Creating Jar file...";

                using (ZipFile zip = new ZipFile())
                {
                    zip.AlternateEncoding = Encoding.UTF8;  // utf-8
                    zip.AddDirectory(Application.StartupPath + "\\tool\\cross\\book");
                    zip.Comment = "This zip was created at " + System.DateTime.Now.ToString("G");
                    zip.Save(sourceFile);
                    
                    File.WriteAllText(Path.GetDirectoryName(sourceFile) + "\\windows.bat", "java -jar " + Path.GetFileName(sourceFile));
                    File.WriteAllText(Path.GetDirectoryName(sourceFile) + "\\linux.sh", "#!/bin/bash\njava -jar " + Path.GetFileName(sourceFile));
                    File.WriteAllText(Path.GetDirectoryName(sourceFile) + "\\mac.sh", "#!/bin/sh\r\njava -jar " + Path.GetFileName(sourceFile));
                  
                    frmmain.completeCompile = true;
                    myClass.clearFolder(Path.Combine(Application.StartupPath, "tool\\cross\\book\\Files"));
                }


            }
            catch (Exception)
            {
                frmmain.completeCompile = false;
            }
        }

        private void extractImage(string projectNames,bool isAndroid)
        {
            System.Data.DataTable d1 = new System.Data.DataTable();
            db db1 = new db(projectNames);
            d1 = db1.getRow("SELECT sBody FROM tbl_app");
            for(int i = 0 ; i<= d1.Rows.Count-1 ; i++)
            {
                myClass.GetImagesInHTMLString(d1.Rows[i][0].ToString(),Application.StartupPath,isAndroid);
            }
        }

        private void changeAPKPackage(string packageName)
        {
            string sPath = "\\tool\\book\\smali\\student\\" + packageName;
            string[] files = Directory.GetFiles(Application.StartupPath + sPath);
            foreach(string file in files)
            {
                myClass.ReplaceInFile(file, "templatebook", packageName);
                if (file.ToLower().IndexOf("main") > -1)
                {
                    myClass.ReplaceInFile(file, "PushAppID", AppID);
                    myClass.ReplaceInFile(file, "PushClientID", ClientID);
                }
            }
            string[] files1 = Directory.GetFiles(Application.StartupPath + sPath + "\\designerscripts");
            foreach (string file1 in files1)
            {
                myClass.ReplaceInFile(file1, "templatebook", packageName);
            }
        }

        public static void ExportAPK(string command)
        {
            int ExitCode;
            ProcessStartInfo ProcessInfo;
            Process process;
            string java = Microsoft.Win32.Registry.GetValue("HKEY_CURRENT_USER\\Student", "Java", "").ToString();

            if (command == "b")
                ProcessInfo = new ProcessStartInfo(java, " -jar apktool.jar " + command + " book");
            else
                ProcessInfo = new ProcessStartInfo(java, " -jar apktool.jar " + command + " book.apk");

            try
            {
                ProcessInfo.CreateNoWindow = true;
                ProcessInfo.UseShellExecute = false;
                ProcessInfo.WorkingDirectory = Application.StartupPath + "\\tool";
                // *** Redirect the output ***
                ProcessInfo.RedirectStandardError = true;
                ProcessInfo.RedirectStandardOutput = true;
                process = Process.Start(ProcessInfo);
                process.WaitForExit();
                ExitCode = process.ExitCode;
                process.Close();
            }
            catch (Exception) {
                MessageBox.Show("متاسفانه کتاب با مشکل مواجه شده است\nممکن است نرم افزار جاوا به درستی نصب نشده باشد","خطا",MessageBoxButtons.OK,MessageBoxIcon.Asterisk);
            }
        }

        public static void SignAPK()
        {
            int ExitCode;
            ProcessStartInfo ProcessInfo;
            Process process;
            string java = Microsoft.Win32.Registry.GetValue("HKEY_CURRENT_USER\\Student", "Java", "").ToString();

            ProcessInfo = new ProcessStartInfo(java, " -jar \"signapk.jar\" certificate.pem key.pk8 output\\book_out.apk output\\book.apk");
            ProcessInfo.CreateNoWindow = true;
            ProcessInfo.UseShellExecute = false;
            ProcessInfo.WorkingDirectory = Application.StartupPath + "\\tool";
            // *** Redirect the output ***
            ProcessInfo.RedirectStandardError = true;
            ProcessInfo.RedirectStandardOutput = true;
            process = Process.Start(ProcessInfo);
            process.WaitForExit();
            ExitCode = process.ExitCode;
            process.Close();

        }

        public static bool GrantAccess(string fullPath)
        {
            try
            {
                DirectoryInfo dInfo = new DirectoryInfo(fullPath);
                DirectorySecurity dSecurity = dInfo.GetAccessControl();
                dSecurity.AddAccessRule(new FileSystemAccessRule(new System.Security.Principal.SecurityIdentifier(System.Security.Principal.WellKnownSidType.WorldSid, null), FileSystemRights.FullControl, InheritanceFlags.ObjectInherit | InheritanceFlags.ContainerInherit, PropagationFlags.NoPropagateInherit, AccessControlType.Allow));
                dInfo.SetAccessControl(dSecurity);
                return true;
            }
            catch(Exception)
            {
                return false;
            }
            
        }

        public static void DirectoryCopy(
        string sourceDirName, string destDirName, bool copySubDirs)
        {
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);
            DirectoryInfo[] dirs = dir.GetDirectories();

            // If the source directory does not exist, throw an exception.
            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            // If the destination directory does not exist, create it.
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }


            // Get the file contents of the directory to copy.
            FileInfo[] files = dir.GetFiles();

            foreach (FileInfo file in files)
            {
                // Create the path to the new copy of the file.
                string temppath = Path.Combine(destDirName, file.Name);

                // Copy the file.
                file.CopyTo(temppath, false);
            }

            // If copySubDirs is true, copy the subdirectories.
            if (copySubDirs)
            {

                foreach (DirectoryInfo subdir in dirs)
                {
                    // Create the subdirectory.
                    string temppath = Path.Combine(destDirName, subdir.Name);

                    // Copy the subdirectories.
                    DirectoryCopy(subdir.FullName, temppath, copySubDirs);
                }
            }
        }
    }
}
