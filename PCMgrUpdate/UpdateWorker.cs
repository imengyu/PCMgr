using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.IO.Compression;
using System.Windows.Forms;
using System.Linq;
using System.Collections.Generic;

namespace PCMgrUpdate
{
    class UpdateWorker
    {
        public bool Updateing { get; private set; }
        public bool CanCancel { get; private set; }
        public bool IsAutoCheck { get; private set; }
        public bool IsIgnoreCheck { get; private set; }
        public bool IsFix { get; set; }
        public bool IsInstall { get; set; }
        public bool IsUpdateUpdater { get; private set; }
        public bool IsFinishUpr { get; private set; }
        public bool IsGetMD5 { get; private set; }

        public void RunUpdate(FormMain main)
        {
            formMain = main;
            if (!IsGetMD5)
            {
                runnerThread = new Thread(Runner);
                runnerThread.Start();
            }
        }
        public void CancelUpdate()
        {
            if (runnerThread.IsAlive)
                runnerThread.Abort();
        }
        public void ReadAgrs(string[] agrs)
        {
            if (agrs.Length >= 1 && File.Exists(agrs[0]))
            {
                string md5 = GetMD5HashFromFile(agrs[0]);
                new FormA(md5, agrs[0]).ShowDialog();
                IsGetMD5 = true;
                Environment.Exit(0);
                return;
            }
            if (agrs.Length == 1 && agrs[0] == "fix")
                IsFix = true;
            if (agrs.Contains("auto"))
                IsAutoCheck = true;
            if (agrs.Contains("local"))
                updatePath = "http://localhost/softs/pcmgr/";
            if (agrs.Contains("ignorecheck"))
                IsIgnoreCheck = true;
            if (agrs.Contains("finish-updater-self"))
                IsFinishUpr = true;
            if (agrs.Contains("update_updater"))
            {
                IsUpdateUpdater = true;
                if (agrs.Length >= 2)
                    updateUpdaterTarget = agrs[1];
            }
        }

        private string updateUpdaterTarget = "";
        private string installFails = "";
        private bool installHasFail = false;
        private string updatePath = "http://www.imyzc.com/softs/pcmgr/";
        private Thread runnerThread = null;
        private FormMain formMain;

        private string GetCurrentFilePath()
        {
            return Application.StartupPath + "\\" + System.Diagnostics.Process.GetCurrentProcess().ProcessName + ".exe";
        }
        private void HideMain() { formMain.Invoke(new Action(delegate { formMain.Hide(); })); }
        private void CloseMain() { formMain.Invoke(new Action(delegate { formMain.Close(); })); }
        private void AnnyInstallFails() { if (!installHasFail) installHasFail = true; }

        private void Runner()
        {
            if (IsUpdateUpdater) UpdateInstallUpdater();
            else if (IsFinishUpr) { UpdateInstallUpdaterEnd(); UpdateFinish(); }
            else if (IsIgnoreCheck) Update();
            else if (IsInstall)
            {
                if (MessageBox.Show("您希望立即开始在线安装 PC Manager 吗？", "疑问 - PC Manager 更新工具", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    Update();
            }
            else if (IsFix)
            {
                if (MessageBox.Show("您希望立即开始修复 PC Manager 吗？", "疑问 - PC Manager 更新工具", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    Update();
            }
            else
            {
                string latest_status = CheckLatest();
                if (latest_status == "latest") ShowLatest();
                else if (latest_status == "newupdate")
                {
                    UpdateSetStatus("已检测到更新");
                    UpdateSetPrecentText("");
                    if (MessageBox.Show(IsAutoCheck ? "PC Manager 有新的版本了，现在是否更新？" : "检测到新的版本，是否立即更新？", "疑问 - PC Manager 更新工具", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                        Update();
                }
            }
            CloseMain();
        }

        private string CheckLatest()
        {
            UpdateSetPrecentText("正在获取更新");
            try
            {
                var request = (HttpWebRequest)WebRequest.Create(updatePath + "?checkupdate=" + formMain.CurrentVersion);
                request.UserAgent = "PC Manager Client Updater";
                var response = (HttpWebResponse)request.GetResponse();
                return new StreamReader(response.GetResponseStream()).ReadToEnd();
            }
            catch (WebException e)
            {
                if (!IsAutoCheck) ShowUpdateError("无法获取更新：" + e.Message, null);
            }
            catch (Exception e)
            {
                if (!IsAutoCheck) ShowUpdateError("无法获取更新。", e);
            }
            return "failed";
        }
        private string CheckNewPath()
        {
            UpdateSetPrecentText("正在获取更新");
            try
            {
#if _X64_
                var request = (HttpWebRequest)WebRequest.Create(updatePath + "?getupdate=x64");
#else
                var request = (HttpWebRequest)WebRequest.Create(updatePath + "?getupdate=x86");
#endif
                request.UserAgent = "PC Manager Client Updater";
                var response = (HttpWebResponse)request.GetResponse();
                return new StreamReader(response.GetResponseStream()).ReadToEnd();
            }
            catch (WebException e)
            {
                ShowUpdateError("获取更新失败：" + e.Message, null);
            }
            catch (Exception e)
            {
                ShowUpdateError("无法获取更新。", e);
            }
            return "failed";
        }
        private string CheckNewMD5()
        {
            UpdateSetPrecentText("正在获取更新信息");
            try
            {
#if _X64_
                var request = (HttpWebRequest)WebRequest.Create(updatePath + "?getupdate_md5=x64");
#else
                var request = (HttpWebRequest)WebRequest.Create(updatePath + "?getupdate_md5=x86");
#endif
                request.UserAgent = "PC Manager Client Updater";
                var response = (HttpWebResponse)request.GetResponse();
                return new StreamReader(response.GetResponseStream()).ReadToEnd();
            }
            catch (Exception e)
            {
                ShowUpdateError("无法获取更新。", e);
            }
            return "failed";
        }

        private void UpdateSetStatus(string s) { formMain.SetMainStatus(s); }
        private void UpdateSetPrecentText(string s) { formMain.SetMainPrecentText(s); }
        private void UpdateSetProgress(int progress, ProgressBarStyle style = ProgressBarStyle.Blocks) { formMain.SetMainProgress(progress, style); }

        private void Update()
        {
            string temp_dir = Application.StartupPath + "\\update";
            string temp_path = temp_dir + "\\update.zip";

            if (!UpdateCreateTempDir(temp_dir))
                return;
            if (!UpdateDownload(temp_path))
                return;
            if (!UpdateInstall(temp_path, temp_dir))
                return;
            UpdateFinish();
        }
        private bool UpdateCreateTempDir(string temp_dir)
        {
            if (Directory.Exists(temp_dir)) return true;
            try
            {
                Directory.CreateDirectory(temp_dir);
                return true;
            }
            catch (UnauthorizedAccessException)
            {
                if (NativeMethods.MIsRunasAdmin()) ShowUpdateError("无法创建临时目录，请尝试在其他目录安装本软件，您可以移动安装程序到其他目录来进行在线安装。", null);
                else
                {
                    bool userCanceled = false;
                    NativeMethods.MAppRebotAdmin3("ignorecheck", ref userCanceled);
                    if (userCanceled) ShowUpdateError("无法创建临时目录，请尝试以管理员权限运行本安装程序。", null);
                }
            }
            catch(PathTooLongException)
            {
                ShowUpdateError("安装路径过长（超过了260个字符），请尝试在其他目录安装本软件，您可以移动安装程序到其他目录来进行在线安装。", null);
            }
            return false;
        }
        private bool UpdateDownload(string temp_path)
        {
            UpdateSetStatus("正在更新......");

            string md5 = CheckNewMD5();
            if (md5 == "failed") return false;
            if (File.Exists(temp_path))
            {
                UpdateSetPrecentText("更新下载完成");

                //Check md5
                string old_temp_md5 = GetMD5HashFromFile(temp_path);
                if (old_temp_md5 == "failed") return false;
                if (old_temp_md5 != md5)
                {
                    try { File.Delete(temp_path); }
                    catch(Exception e) {
                        ShowUpdateError("删除旧更新包错误", e);
                        return false;
                    }
                    goto DWONLOAD;
                }
                return true;
            }
            
            DWONLOAD:
            //GetPath & md5
            string newpath = CheckNewPath();
            if (newpath == "failed" )
                return false;
            if (newpath == "notsupport")
            {
                MessageBox.Show("在线更新服务现在不可用", "提示 - PC Manager 更新工具", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            string path = updatePath + newpath;


            UpdateSetPrecentText("");

            //Download Package
            if (DownloadFile(path, temp_path, UpdateDownloadCallback))
                UpdateSetPrecentText("更新下载完成");
            else
            {
                UpdateSetPrecentText("更新下载失败");
                return false;
            }

            //Check md5
            string temp_md5 = GetMD5HashFromFile(temp_path);
            if (temp_md5 == "failed")
                return false;
            if (temp_md5 != md5)
            {
                try { File.Delete(temp_path); }
                catch { }
                ShowUpdateError("解析更新包错误", null);
                return false;
            }

            return true;
        }
        private bool UpdateInstall(string temp_path, string temp_dir)
        {
            try
            {
                if (!NativeMethods.MIsRunasAdmin()) {
                    bool userCanceled = false;
                    NativeMethods.MAppRebotAdmin3("ignorecheck", ref userCanceled);
                    if (userCanceled)
                    {
                        userCanceled = false;
                        ShowUpdateError("无法继续安装，请尝试以管理员权限运行本安装程序。", null);
                        NativeMethods.MAppRebotAdmin3("ignorecheck", ref userCanceled);
                        if (userCanceled)
                            ShowUpdateError("无法继续安装，请尝试以管理员权限运行本安装程序。", null);
                    }
                    return false;
                }
                NativeMethods.FreeLibrary(NativeMethods.MAppGetCoreModulHandle());
            }
            catch
            {

            }

            UpdateSetStatus("正在安装......");

            bool del_old_success = false;
            bool need_update_updater = false;
            string updater_temp_path = "";

            try
            {
                int current = 0, all = 0;

                FileStream file = new FileStream(temp_path, FileMode.Open, FileAccess.Read);
                ZipArchive zip = new ZipArchive(file);
                all = zip.Entries.Count;
                foreach (ZipArchiveEntry entry in zip.Entries)
                {
                    string file_name = Path.GetFileName(entry.FullName);
                    string file_path = Application.StartupPath + "\\" + entry.FullName;

                    del_old_success = false;

                    UpdateSetPrecentText("写入文件 " + file_name);
                    UpdateSetProgress(current / all * 100, ProgressBarStyle.Blocks);
#if _X64_
                    if (entry.FullName == "PCMgrUpdate64.exe")
#else
                    if (entry.FullName == "PCMgrUpdate.exe")
#endif
                    {
                        try
                        {
                            file_path = temp_dir + "\\" + file_name;
                            updater_temp_path = file_path;
                            entry.ExtractToFile(file_path);
                            need_update_updater = true;
                            if (GetMD5HashFromFile(file_path) == GetMD5HashFromFile(GetCurrentFilePath()))
                                need_update_updater = false;
                        }
                        catch (UnauthorizedAccessException) { installFails += file_path + "[拒绝访问]\n"; }
                        catch (PathTooLongException) { installFails += file_path + "[路径过长]\n"; }
                        catch (Exception e)
                        {
                            installFails += file_path + "[" + e.Message + "]" + e.StackTrace + "\n";
                        }
                        continue;
                    }
                    if (entry.FullName.EndsWith("/"))
                    {
                        string dir_path = Application.StartupPath + "\\" + entry.FullName.Replace("//","\\");
                        try
                        {
                            if (!Directory.Exists(dir_path))
                                Directory.CreateDirectory(dir_path);
                        }
                        catch (UnauthorizedAccessException) { installFails += file_path + "[拒绝访问]\n"; }
                        catch (PathTooLongException) { installFails += file_path + "[路径过长]\n"; }
                        catch (Exception e)
                        {
                            installFails += file_path + "[" + e.Message + "]" + e.StackTrace + "\n";
                        }
                        continue;
                    }

                    //Del old
                    if (File.Exists(file_path))
                    {
                        try
                        {
                            File.Delete(file_path);
                            del_old_success = true;
                        }
                        catch (UnauthorizedAccessException) { installFails += file_path + "[拒绝访问]\n"; }
                        catch (PathTooLongException) { installFails += file_path + "[路径过长]\n"; }
                        catch (Exception e)
                        {
                            installFails += file_path + "[" + e.Message + "]" + e.StackTrace + "\n";
                            AnnyInstallFails();
                        }
                    }
                    else del_old_success = true;
                    //del_old_success
                    if (del_old_success)
                    {
                        try
                        {
                            entry.ExtractToFile(file_path);
                        }
                        catch (UnauthorizedAccessException) { installFails += file_path + "[拒绝访问]\n"; }
                        catch (PathTooLongException) { installFails += file_path + "[路径过长]\n"; }
                        catch (Exception e)
                        {
                            installFails += file_path + "[" + e.Message + "]" + e.StackTrace + "\n";
                            AnnyInstallFails();
                        }

                    }
                    current++;
                }
                zip.Dispose();
                file.Close();

                UpdateSetPrecentText("正在验证");

                if (installHasFail)
                {
                    if (MessageBox.Show("安装未能成功完成，这可能是因为权限的问题，请重新安装试试。或者将错误日志报告给我们。点击“是”查看安装错误日志。", "错误 - PC Manager 更新工具 ", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                        new FormErrLog(installFails).ShowDialog(formMain);
                    return false;
                }

                if (need_update_updater)
                {
                    System.Diagnostics.Process.Start(updater_temp_path,
                        "update_updater " + GetCurrentFilePath());
                    return false;
                }

                return true;
            }
            catch (Exception e)
            {
                ShowUpdateError("安装时发生错误", e);
            }
            return false;
        }
        private bool UpdateInstallUpdater()
        {
            UpdateSetStatus("正在更新安装程序...");
            Thread.Sleep(5000);
            if (File.Exists(updateUpdaterTarget))
            {
                try
                {
                    File.Delete(updateUpdaterTarget);
                }
                catch (Exception e)
                {
                    ShowUpdateError("删除旧更新程序时发生错误", e);
                    return false;
                }
            }
            try
            {
                File.Copy(GetCurrentFilePath(), updateUpdaterTarget, true);
            }
            catch (Exception e)
            {
                ShowUpdateError("复制更新程序时发生错误", e);
                return false;
            }
            System.Diagnostics.Process.Start(Path.GetDirectoryName(updateUpdaterTarget), "finish-updater-self");

            return true;
        }
        private bool UpdateInstallUpdaterEnd()
        {
            string temp_upr = Application.StartupPath + "\\update\\" + System.Diagnostics.Process.GetCurrentProcess().ProcessName + ".exe";
            if (File.Exists(temp_upr))
            {
                try  { File.Delete(temp_upr);  }
                catch { return false; }
            }
            return true;
        }
        private void UpdateFinish()
        {
            HideMain();
            if (MessageBox.Show("安装完成！您希望立即启动软件吗", "PC Manager 更新工具 ", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
            {
#if _X64_
                System.Diagnostics.Process.Start(Application.StartupPath + "\\PCMgr64.exe");
#else
                System.Diagnostics.Process.Start(Application.StartupPath + "\\PCMgr32.exe");
#endif
            }
        }

        private void UpdateDownloadCallback(float progress)
        {
            UpdateSetProgress((int)progress, ProgressBarStyle.Blocks);
            UpdateSetPrecentText("正在下载更新 " + progress.ToString("0.0") + "%");
        }

        private void ShowUpdateError(string s, Exception e)
        {
            if (e != null) MessageBox.Show(s + "\n错误详细信息：\n" + e.ToString(), "更新错误 - PC Manager 更新工具", MessageBoxButtons.OK, MessageBoxIcon.Error);
            else MessageBox.Show(s, "更新错误 - PC Manager 更新工具", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        private void ShowLatest()
        {
            if (!IsAutoCheck)
            {
                HideMain();
                MessageBox.Show("您的 PC Manager 已经是最新的版本。", "提示 - PC Manager 更新工具");
            }
        }

        public delegate void DownloadFileCallback(float progress);
        public bool DownloadFile(string URL, string filename, DownloadFileCallback downloadFileCallback)
        {
            float percent = 0;
            try
            {
                System.Net.HttpWebRequest Myrq = (System.Net.HttpWebRequest)System.Net.HttpWebRequest.Create(URL);
                Myrq.UserAgent = "PC Manager Client Updater";
                System.Net.HttpWebResponse myrp = (System.Net.HttpWebResponse)Myrq.GetResponse();
                long totalBytes = myrp.ContentLength;
                System.IO.Stream st = myrp.GetResponseStream();
                System.IO.Stream so = new System.IO.FileStream(filename, System.IO.FileMode.Create);
                long totalDownloadedByte = 0;
                byte[] by = new byte[1024];
                int osize = st.Read(by, 0, (int)by.Length);
                while (osize > 0)
                {
                    totalDownloadedByte = osize + totalDownloadedByte;
                    System.Windows.Forms.Application.DoEvents();
                    so.Write(by, 0, osize);
                    osize = st.Read(by, 0, (int)by.Length);

                    percent = (float)totalDownloadedByte / (float)totalBytes * 100;
                    downloadFileCallback(percent);
                    Application.DoEvents(); //必须加注这句代码，否则label1将因为循环执行太快而来不及显示信息
                }
                so.Close();
                st.Close();
                return true;
            }
            catch (Exception e)
            {
                ShowUpdateError("下载更新错误", e);
                return false;
            }
        }
        private string GetMD5HashFromFile(string fileName)
        {
            try
            {
                FileStream file = new FileStream(fileName, FileMode.Open, FileAccess.Read);
                System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
                byte[] retVal = md5.ComputeHash(file);
                file.Close();

                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < retVal.Length; i++)
                {
                    sb.Append(retVal[i].ToString("x2"));
                }
                return sb.ToString();
            }
            catch (Exception ex)
            {
                ShowUpdateError("GetMD5HashFromFile() fail" , ex);
                return "failed";
            }
        }
    }
}
