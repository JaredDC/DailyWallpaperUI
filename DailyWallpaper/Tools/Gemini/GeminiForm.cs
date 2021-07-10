﻿using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.IO;
using System.Windows.Forms;
using DailyWallpaper.Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.VisualBasic.FileIO;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using DailyWallpaper.Tools;
using System.Diagnostics;
using System.Collections;
using static DailyWallpaper.Tools.Gemini;
using System.Drawing;
using System.Security.Cryptography;
// using System.Linq;

namespace DailyWallpaper
{
    public partial class GeminiForm : Form, IDisposable
    {
        private Gemini gemini;
        private TextBoxCons _console;
        private string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        private CancellationTokenSource _source = null;
        private bool deletePermanently = false;

        private bool scanRes = false;
        private List<string> folderFilter;
        private string regexFilter;
        private Regex regex;

        private List<string> targetFolder1History = new List<string>();
        private List<string> targetFolder2History = new List<string>();
        private List<string> filesList1;
        private List<string> filesList2;

        private List<GeminiFileStruct> geminiFileStructList1;
        private List<GeminiFileStruct> geminiFileStructList2;
        private long minimumFileLimit = 0;
        private List<Task> _tasks = new List<Task>();
        private Mutex _mutex;
        private Mutex _mutexPb;
        private ListViewColumnSorter lvwColumnSorter;
        private List<string> deleteList;
        private List<GeminiFileStruct> geminiFileStructListForLV = new List<GeminiFileStruct>();
        private Color themeColor = Color.FromArgb(250, 234, 192);
        GeminiCompareMode m_comparemode = GeminiCompareMode.NameAndSize;
        private enum FilterMode : int
        {
            REGEX_FIND,
            REGEX_PROTECT,
            GEN_FIND,
            GEN_PROTECT,
        }
        private FilterMode filterMode;
        public GeminiForm()
        {
            InitializeComponent();
            targetFolder1TextBox.KeyDown += targetFolder1_KeyDown;
            targetFolder2TextBox.KeyDown += targetFolder2_KeyDown;
            folderFilterTextBox.KeyDown += folderFilterTextBox_KeyDown;
            Icon = Properties.Resources.GE32X32;
            gemini = new Gemini();
            _console = new TextBoxCons(new ConsWriter(tbConsole));

            // System.Windows.Forms.TextBox.CheckForIllegalCrossThreadCalls = false;

            // init targetfolder 1&2
            targetFolder1TextBox.Text = desktopPath;
            targetFolder2TextBox.Text = "";
            var init = gemini.ini.Read("TargetFolder1", "Gemini");
            if (Directory.Exists(init))
            {
                UpdateTextAndIniFile("TargetFolder1", init,
                    targetFolder1History, targetFolder1TextBox, updateIni: false);
            }

            /*init = gemini.ini.Read("TargetFolder2", "Gemini");
            if (Directory.Exists(init))
            {
                UpdateTextAndIniFile("TargetFolder2", init,
                    targetFolder2History, targetFolder2TextBox, updateIni: false);
            }*/

            btnStop.Enabled = false;
            // default: send to RecycleBin
            deleteOrRecycleBin.Checked = false;

            // auto delete empty folder after remove.
            cleanEmptyFoldersToolStripMenuItem.Checked = true;

            MaximizeBox = false;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            SetUpFilterModeAndRegClick();
            // _console.WriteLine("You could always TYPE help in folder filter textbox and press ENTER.");
            InitFileSameMode();
            filesList1 = new List<string>();
            filesList2 = new List<string>();
            geminiFileStructList1 = new List<GeminiFileStruct>();
            geminiFileStructList2 = new List<GeminiFileStruct>();
            _mutex = new Mutex();
            _mutexPb = new Mutex();

            // Create an instance of a ListView column sorter and assign it
            // to the ListView control.
            lvwColumnSorter = new ListViewColumnSorter();
            resultListView.ListViewItemSorter = lvwColumnSorter;
        }

        /// <summary>
        /// bind to tbTargetFolderHistory
        /// </summary>
        private void SetUpFilterModeAndRegClick()
        {
            var fimode = gemini.ini.Read("FilterMode", "Gemini");
            if (!string.IsNullOrEmpty(fimode))
            {
                if (fimode.Equals("GEN_PROTECT"))
                {
                    regexCheckBox.Checked = false;
                    modeCheckBox.Checked = false;
                    filterMode = FilterMode.GEN_PROTECT;
                }
                else if (fimode.Equals("REGEX_PROTECT"))
                {
                    regexCheckBox.Checked = true;
                    modeCheckBox.Checked = false;
                    filterMode = FilterMode.REGEX_PROTECT;
                }
                else if (fimode.Equals("REGEX_FIND"))
                {
                    regexCheckBox.Checked = true;
                    modeCheckBox.Checked = true;
                    filterMode = FilterMode.REGEX_FIND;
                }
                else
                {
                    regexCheckBox.Checked = false;
                    modeCheckBox.Checked = true;
                    filterMode = FilterMode.GEN_FIND;
                }
            }
            else
            {
                regexCheckBox.Checked = false;
                modeCheckBox.Checked = true;
                filterMode = FilterMode.GEN_FIND;
            }
            UpdateFilterExampleText(filterMode);
            regexCheckBox.Click += new EventHandler(regexCheckBox_Click);
            modeCheckBox.Click += new EventHandler(modeCheckBox_Click);
        }
        private void BindHistory(System.Windows.Forms.TextBox tb, List<string> list)
        {
            tb.AutoCompleteCustomSource.Clear();
            tb.AutoCompleteCustomSource.AddRange(list.ToArray());
            tb.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            tb.AutoCompleteSource = AutoCompleteSource.CustomSource;
        }
        private void btnSelectTargetFolder1_Click(object sender, EventArgs e)
        {
            SelectFolder("TargetFolder1", targetFolder1TextBox,
                targetFolder1History);
        }

        private void btnSelectTargetFolder2_Click(object sender, EventArgs e)
        {
            SelectFolder("TargetFolder2", targetFolder2TextBox,
            targetFolder2History);
        }

        private void SelectFolder(string keyInIni, System.Windows.Forms.TextBox tx,
            List<string> targetFolderHistory)
        {
            using (var dialog = new CommonOpenFileDialog())
            {
                if (Directory.Exists(tx.Text))
                {
                    dialog.InitialDirectory = tx.Text;
                }
                else
                {
                    dialog.InitialDirectory = desktopPath;
                }
                dialog.IsFolderPicker = true;
                dialog.EnsurePathExists = true;
                dialog.Multiselect = false;
                dialog.Title = "Select Target Folders";

                // maybe add some log
                if (dialog.ShowDialog() == CommonFileDialogResult.Ok && !string.IsNullOrEmpty(dialog.FileName))
                {
                    var path = dialog.FileName;
                    if (!UpdateTextAndIniFile(keyInIni, path, targetFolderHistory, tx))
                    {
                        return;
                    }
                    tx.Text = path;
                }
            }
        }

        private void UpdateCheckedInDelGFL(List<GeminiFileStruct> gfl, List<string> delList, GeminiFileStruct item)
        {
            item.Checked = false;
            foreach (var it in delList)
            {
                if (item.fullPath.ToLower().Equals(it.ToLower()))
                {
                    item.Checked = true;
                }
            }
            gfl.Add(item);
        }
        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (deleteList == null)
            {
                _console.WriteLine($"\r\n!!! You should ANALYZE first.");
                return;
            }
            try
            {
                var taskDel = Task.Run(() => {
                    // from deleteList update geminiFileStructListForLV'bp -- delGflChecked
                    var deleteListEx = new List<string>();
                    foreach (var item in deleteList)
                    {
                        if (File.Exists(item))
                        {
                            deleteListEx.Add(item);
                        }
                    }
                    deleteList = deleteListEx;
                    _console.WriteLine($"\r\n=== You have selected {deleteList.Count} file(s).");
                    if (deleteList.Count < 1)
                    {
                        return;
                    }
                    var delGflChecked = new List<GeminiFileStruct>();
                    foreach (var item in geminiFileStructListForLV)
                    {
                        UpdateCheckedInDelGFL(delGflChecked, deleteList, item);
                    }
                
                    /*
                        * HOW COULD I PASS Anonymous Types ???
                        * Func<TSource, TKey> keySelector;
                        * var v2 = new { hash = "10086", size = 10086};
                        */
                    var delGflGrp = GeminiFileStructList2IEnumerableGroup(delGflChecked, m_comparemode);
                    int k = 0;
                    foreach (var item in delGflGrp)
                    {
                        // Prevent all files in the group from being deleted
                        if (!deleteAllSelectedToolStripMenuItem.Checked)
                        {
                            if ((from i in item
                                    where i.Checked == true
                                    select i).Count().Equals(item.Count))
                            {
                                k++;
                                _console.WriteLine($"![{k}] Prevent all files in the group from being deleted.");
                                continue;
                            }
                        }

                        foreach (var it in item)
                        {
                            if (it.Checked)
                            {
                                _console.WriteLine($"...... Delete file: {it.fullPath}");
                                FileSystem.DeleteFile(it.fullPath, UIOption.OnlyErrorDialogs,
                                                deleteOrRecycleBin.Checked ?
                                                RecycleOption.DeletePermanently : RecycleOption.SendToRecycleBin,
                                                UICancelOption.DoNothing);
                            }
                        }                    
                    }
                    _console.WriteLine($">>> Delete Finished.");
                
                    // clean non-existent file in geminiFileStructListForLV
                    // cleanUpButton do UpdateLVCheckedAndDelList(geminiFileStructListForLV);
                    cleanUpButton.PerformClick();
                    }, _source.Token);
                _tasks.Add(taskDel);
                // taskDel.Wait();
                // await taskDel;
            }
            catch (UnauthorizedAccessException) { }
            catch (FileNotFoundException) { }
            catch (Exception ex)
            {
                _console.WriteLine($"!!! Error occur when deleting files: {ex.Message}");
            }
        }

        private static string GetTimeStringMsOrS(TimeSpan t)
        {
            string hashCostTime;
            if (t.TotalSeconds > 1)
            {
                hashCostTime = t.TotalSeconds.ToString("f2") + "s";
            }
            else
            {
                hashCostTime = t.TotalMilliseconds.ToString("f3") + "ms";
            }
            return hashCostTime;
        }
        private GeminiCompareMode SetCompareMode()
        {
            GeminiCompareMode mode = GeminiCompareMode.NameAndSize;

            if (fileSHA1CheckBox.Checked || fileMD5CheckBox.Checked)
            {
                mode = GeminiCompareMode.HASH;
            } 
            else if (fileNameCheckBox.Checked)
            {
                mode = GeminiCompareMode.NameAndSize;
            }
            else if (fileExtNameCheckBox.Checked)
            {
                mode = GeminiCompareMode.ExtAndSize;
            }
            return mode;
        }
        private async void StartAnalyze(bool delete = false)
        {
            _source = new CancellationTokenSource();
            var token = _source.Token;
            btnStop.Enabled = true;
            btnAnalyze.Enabled = false;
            geminiProgressBar.Visible = true;
            var limit = SetMinimumFileLimit();
            try
            {
                var _task = Task.Run(() =>
                {
                    var timer = new Stopwatch();
                    timer.Start();
                        // Get all files from folder1/2
                        bool fld1 = false;
                    bool fld2 = false;
                    var t1 = targetFolder1TextBox.Text;
                    var t2 = targetFolder2TextBox.Text;

                    if (!string.IsNullOrEmpty(t2) && Directory.Exists(t2))
                    {
                        fld2 = true;
                        RecurseScanDir(t2, ref filesList2, token);
                        _console.WriteLine($">>> Found {filesList2.Count} file(s) in: {t2}");
                    }

                    if (!string.IsNullOrEmpty(t1) && Directory.Exists(t1) && !t1.Equals(t2))
                    {
                        fld1 = true;
                        RecurseScanDir(t1, ref filesList1, token);
                        _console.WriteLine($">>> Found {filesList1.Count} file(s) in: {t1}");
                    }

                    if (!fld2 && !fld1)
                    {
                        _console.WriteLine("!!! Two folder invalid.");
                        return;
                    }

                        // get files info exclude HASH.(FASTER) 
                    FileList2GeminiFileStructList(filesList1, ref geminiFileStructList1, token);
                    FileList2GeminiFileStructList(filesList2, ref geminiFileStructList2, token);

                        // compare folders and themselves, return duplicated files list.
                    _console.WriteLine(">>> Start Fast Compare...");
                   
                    m_comparemode = SetCompareMode();
                    var sameListNoDup = ComparerTwoFolderGetList(geminiFileStructList1,
                            geminiFileStructList2, m_comparemode, limit, token, geminiProgressBar).Result;
                    _console.WriteLine(">>> Fast Compare finished...");
                    // group by size

                    if (fileMD5CheckBox.Checked || fileSHA1CheckBox.Checked)
                    {
                        _console.WriteLine($">>> Update HASH for {sameListNoDup.Count:N0} file(s)...");
                        sameListNoDup = 
                            UpdateHashInGeminiFileStructList(sameListNoDup).Result;
                        _console.WriteLine(">>> Update HASH finished.");
                    }

                    // Regroup List.
                    _console.WriteLine(">>> Group List...");
                    geminiFileStructListForLV = ListReGrpAndReColor(sameListNoDup, m_comparemode, token);
                    
                    _console.WriteLine(">>> Update to ListView...");
                    UpdateListView(geminiFileStructListForLV, token);

                    timer.Stop();
                    string hashCostTime = GetTimeStringMsOrS(timer.Elapsed);
                    _console.WriteLine($">>> Cost time: {hashCostTime}");

                }, _source.Token);

                _tasks.Add(_task);
                await _task;
                // No Error, filesList is usable
                scanRes = true;
            }
            catch (OperationCanceledException e)
            {
                scanRes = false;
                _console.WriteLine($"\r\n>>> OperationCanceledException: {e.Message}");
            }
            catch (AggregateException e)
            {
                _console.WriteLine($"\r\n>>> AggregateException[Cancel exception]: {e.Message}");
            }
            catch (Exception e)
            {
                scanRes = false;
                // _console.WriteLine($"\r\n RecurseScanDir throw exception message: {e.Message}");
                _console.WriteLine($"\r\n RecurseScanDir throw exception message: {e}");
                _console.WriteLine($"\r\n#----^^^  PLEASE CHECK, TRY TO CONTACT ME WITH THIS LOG.  ^^^----#");
            }
            finally
            {
                geminiProgressBar.Visible = false;
                _console.WriteLine(">>> Analyse is over.");
            }
            btnAnalyze.Enabled = true;

        }

        private async Task<List<GeminiFileStruct>> ComparerTwoFolderGetList(List<GeminiFileStruct> l1,
            List<GeminiFileStruct> l2, GeminiCompareMode mode, long limit = 0, CancellationToken token = default,
            System.Windows.Forms.ProgressBar pb = null)
        {

            if (ignoreFileCheckBox.Checked)
            {
                var limited =
                    from i in l1
                    where i.size > limit
                    select i;
                l1 = limited.ToList();
            }

            if (ignoreFileCheckBox.Checked)
            {
                var limited =
                    from i in l2
                    where i.size > limit
                    select i;
                l2 = limited.ToList();
            }

            var sameList = new List<GeminiFileStruct>();
            void retAction(bool res, List<GeminiFileStruct> ret)
            {
                _mutex.WaitOne();
                if (res && ret.Count > 1)
                {
                    sameList.AddRange(ret);
                }
                _mutex.ReleaseMutex();
            }
            var cnt1 = l1.Count;
            var cnt2 = l2.Count;
            long totalCmpCnt = cnt1 * cnt1 + cnt1 * cnt2 + cnt2 * cnt2;
            _console.WriteLine($">>> folder1: {cnt1:N0}");
            _console.WriteLine($">>> folder2: {cnt2:N0}");
            _console.WriteLine($">>> about {totalCmpCnt:N0} times (x1*x1+x2*x2+x1*x2)...");
            
            double percent = 0.0;
            void ProgressAction(long i) // percent in file.
            {
                // FIX ERROR: System.InvalidOperationException
                _mutexPb.WaitOne();
                percent += (double)i / totalCmpCnt * 100;
                var percentInt = (int)percent;
                if (percentInt > 99)
                    percentInt = 100;
/*                if (pb.IsHandleCreated)
                {
                    pb.Invoke(new Action(() =>
                    {*/
                        // pb.Value = percentInt;
                        SetProgressMessage(percentInt, pb);

/*                    }));
                }*/
                _mutexPb.ReleaseMutex();
            }
            var totalProgess = new Progress<long>(ProgressAction);
            await Task.Run(async () => await ComparerTwoList(l1,
                l1, mode, token, retAction, totalProgess));
            await Task.Run(async () => await ComparerTwoList(l2,
                l2, mode, token, retAction, totalProgess));
            await Task.Run(async () => await ComparerTwoList(l1,
                l2, mode, token, retAction, totalProgess));


            return sameList.Distinct().ToList();
        }

        private delegate void GeminiFileStructToListViewDelegate(
            List<GeminiFileStruct> gfL, CancellationToken token);
        
        private void UpdateListView(List<GeminiFileStruct> gfL, 
            CancellationToken token)
        {
            if (InvokeRequired)
            {
                var f = new GeminiFileStructToListViewDelegate(UpdateListView);
                Invoke(f, new object[] { gfL, token});
            }
            else
            {
                resultListView.Items.Clear();
                if (gfL.Count > 0)
                {
                    foreach (var gf in gfL)
                    {
                        if (token.IsCancellationRequested)
                        {
                            token.ThrowIfCancellationRequested();
                        }
                        var item = new System.Windows.Forms.ListViewItem();
                        // var item = new System.Windows.Forms.ListViewItem(" ");
                        item.BackColor = gf.color;
                        AddSubItem(item, "name", gf.name);
                        AddSubItem(item, "lastMtime", gf.lastMtime);
                        AddSubItem(item, "extName", gf.extName);
                        AddSubItem(item, "sizeStr", gf.sizeStr);
                        AddSubItem(item, "dir", gf.dir);
                        AddSubItem(item, "HASH", gf.hash ?? "");
                        AddSubItem(item, "fullPath", gf.fullPath);
                        AddSubItem(item, "size", gf.size.ToString());
                        resultListView.Items.Add(item);
                    }
                    SetText(summaryTextBox, $"Summay: Found {gfL.Count:N0} duplicate files.", themeColor);
                }
                else
                {
                    SetText(summaryTextBox, $"Summay: Found No duplicate files.", Color.ForestGreen);
                }
            }
        }

        private async Task UpdateHash(List<GeminiFileStruct> gfL, 
            GeminiFileStruct gf)
        {
            /*void ProgressActionD(double i) // percent in file.
            {
                _mutexPb.WaitOne();
                var percentInt = (int)i;
                if (percentInt > 100)
                    percentInt = 100;
                SetProgressMessage(percentInt, pb);
                _mutexPb.ReleaseMutex();
            }
            var totalProgessDouble = new Progress<double>(ProgressActionD);*/
            // DONT REPORT HERE.

            if (fileMD5CheckBox.Checked)
            {
                void getRes(bool res, string who, string md5, string costTimeOrMsg)
                {
                    if (res)
                    {
                        gf.hash = md5;
                    }
                }
                await ComputeHashAsync(
                    MD5.Create(), gf.fullPath, _source.Token, "MD5", getRes);
            }
            else if (fileSHA1CheckBox.Checked)
            {
                void getRes(bool res, string who, string sha1, string costTimeOrMsg)
                {
                    if (res)
                    {
                        gf.hash = sha1;
                    }
                }
                await ComputeHashAsync(
                    SHA1.Create(), gf.fullPath, _source.Token, "SHA1", getRes);
            }
            gfL.Add(gf);
        }

        private IEnumerable<List<GeminiFileStruct>> GeminiFileStructList2IEnumerableGroup(
            List<GeminiFileStruct> gfl, GeminiCompareMode mode)
        {
            IEnumerable<List<GeminiFileStruct>> duplicateGrp = null;
            if (mode == GeminiCompareMode.HASH)
            {
                duplicateGrp =
                from i in gfl
                where File.Exists((i.fullPath))
                group i by i.hash into grp
                where grp.Count() > 1
                select grp.ToList();
            }
            else if (mode == GeminiCompareMode.ExtAndSize)
            {
                duplicateGrp =
                   from i in gfl
                   where File.Exists((i.fullPath))
                   group i by new { i.size, i.extName } into grp
                   where grp.Count() > 1
                   select grp.ToList();
            }
            else
            {
                duplicateGrp =
                   from i in gfl
                   where File.Exists((i.fullPath))
                   group i by i.size into grp
                   where grp.Count() > 1
                   select grp.ToList();
            }
            return duplicateGrp;
        }

        private List<GeminiFileStruct> ListReGrpAndReColor(List<GeminiFileStruct> gfl, GeminiCompareMode mode, 
            CancellationToken token)
        {
            var duplicateGrp = GeminiFileStructList2IEnumerableGroup(gfl, mode);
            var tmpHash = new List<GeminiFileStruct>();
            void UpdateColorForLV(List<GeminiFileStruct> tmpL, GeminiFileStruct tmp, Color c)
            {
                tmp.color = c;
                tmpL.Add(tmp);
            }
            int j = 0;
            foreach (var item in duplicateGrp)
            {
                j++;
                var color = Color.White;
                if (j % 2 == 1)
                {
                    color = themeColor;
                }
                foreach (var it in item)
                {
                    if (token.IsCancellationRequested)
                    {
                        token.ThrowIfCancellationRequested();
                    }
                    UpdateColorForLV(tmpHash, it, color);
                }
            }
            return tmpHash;
        }
        private async Task<List<GeminiFileStruct>> UpdateHashInGeminiFileStructList(
            List<GeminiFileStruct> gfL)
        {
            var tmp = new List<GeminiFileStruct>();
            int i = 0;
            foreach (var it in gfL)
            {
                i++;
                await UpdateHash(tmp, it);
                SetProgressMessage((int)((double)i / gfL.Count * 100), geminiProgressBar);
            }
            return tmp;
        }
        public static void AddSubItem(System.Windows.Forms.ListViewItem i, string name, string text)
        {
            i.SubItems.Add(new System.Windows.Forms.ListViewItem.ListViewSubItem() 
                { Name = name, Text = text });
        }

        private void AddGroupTitleToListView()
        {

        }

        delegate void SetTextCallBack(System.Windows.Forms.TextBox tb, string text, Color c);
        private void SetText(System.Windows.Forms.TextBox tb, string text, Color c)
        {
            if (summaryTextBox.InvokeRequired)
            {
                SetTextCallBack stcb = new SetTextCallBack(SetText);
                Invoke(stcb, new object[] { tb, text, c});
            }
            else
            {
                tb.Text = text;
                tb.BackColor = c;
            }
        }

        private void btnAnalyze_Click(object sender, EventArgs e)
        {
            btnClear.PerformClick();
            resultListView.Items.Clear();
            geminiProgressBar.Visible = true;
            deleteList = new List<string>();
            SetFolderFilter(folderFilterTextBox.Text, print: true);
            StartAnalyze();
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            if (_source != null)
            {
                _source.Cancel();
                //_source.Dispose();
                //_source = null;
            }
            _console.WriteLine("Stop...");
            btnAnalyze.Enabled = true;
        }
        private void btnClear_Click(object sender, EventArgs e)
        {
            tbConsole.Clear();
            SetText(summaryTextBox, "", SystemColors.Control);
            summaryTextBox.Text = "";
            geminiProgressBar.Value = 0;
        }


        // Thanks to João Angelo
        // https://stackoverflow.com/questions/2811509/c-sharp-remove-all-empty-subdirectories
        /// <summary>
        /// TODO: When folder To much, try to not use Recurse
        /// </summary>
        private void RecurseScanDir(string path, ref List<string> filesList, CancellationToken token)
        {
            //token.ThrowIfCancellationRequested();
            // DO NOT KNOW WHY D: DOESNOT WORK WHILE D:\ WORK.
            filesList = new List<string>();
            if (string.IsNullOrEmpty(path) || !Directory.Exists(path))
            {
                _console.WriteLine("Invalid directory path: {0}", path);
                return;
            }

            // _console.WriteLine($"\r\n#---- Started Analyze Operation ----#\r\n");
            if (scanRes && filesList.Count > 0)
            {
                foreach (var folder in filesList)
                {
                    _console.WriteLine($"found ###  {folder}");
                }
                return;
            }
            if (filterMode == FilterMode.GEN_FIND && folderFilter.Count > 0)
            {
                FindFilesWithFindMode(path, filesList, token, re: false);
            }
            else if (filterMode == FilterMode.REGEX_FIND && regex != null)
            {
                FindFilesWithFindMode(path, filesList, token, re: true);
            }
            else
            {
                FindFilesWithProtectMode(path, filesList, token);
            }
        }

        private bool FolderFilter(string path, FilterMode mode)
        {
            if (mode == FilterMode.GEN_PROTECT)
            {
                if (folderFilter.Count > 0)
                {
                    foreach (var filter in folderFilter)
                    {
                        if (path.Contains(filter))
                        {
                            return true;
                        }
                    }
                }
            }
            if (mode == FilterMode.REGEX_PROTECT)
            {
                if (regex == null)
                {
                    return false;
                }
                if (regex.IsMatch(path))
                {
                    return true;
                }
            }
            if (mode == FilterMode.REGEX_FIND)
            {
                if (regex == null)
                {
                    return false;
                }
                if (!regex.IsMatch(path))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// TEST CASE: 
        /// 1)games 2)D:\games 3)games,Steam\logs 4)D:\games,Steam\logs
        /// </summary>

        private void FindFilesWithFindMode(string path, List<string> filesList, CancellationToken token, bool re = false)
        {
            if (String.IsNullOrEmpty(path))
            {
                throw new ArgumentException("Starting directory is a null reference or an empty string: path");
            }
            try
            {

                foreach (var d in Directory.EnumerateDirectories(path))
                {
                    if (token.IsCancellationRequested)
                    {
                        token.ThrowIfCancellationRequested();
                    }
                    // FUCK THE $RECYCLE.BIN
                    if (d.ToLower().Contains("$RECYCLE.BIN".ToLower()))
                    {
                        continue;
                    }
                    FindFilesWithFindMode(d, filesList, token, re);
                }
                if (re)
                {
                    if (regex.IsMatch(path))
                    {
                        FindFilesInDir(path, filesList, token);
                        return;
                    }
                }
                else
                {
                    foreach (var filter in folderFilter)
                    {
                        if (token.IsCancellationRequested)
                        {
                            token.ThrowIfCancellationRequested();
                        }
                        if (path.Contains(filter))
                        {
                            FindFilesInDir(path, filesList, token);
                            continue;
                        }
                    }
                    return;
                }
            }
            catch (UnauthorizedAccessException) { }
            catch (Exception)
            {
                throw;
            }
        }
        //calcSHA1: fileSHA1CheckBox.Checked,
        // calcMD5: fileMD5CheckBox.Checked

        void FileList2GeminiFileStructList(List<string> filesList,
            ref List<GeminiFileStruct> gList, CancellationToken token)
        {
            gList = new List<GeminiFileStruct>();
            if (filesList.Count > 0)
            {
                _console.WriteLine(">>> Start collecting all files...");
                foreach (var f in filesList)
                {
                    if (token.IsCancellationRequested)
                    {
                        token.ThrowIfCancellationRequested();
                    }
                    gList.Add(Gemini.FillGeminiFileStruct(f));
                }
                _console.WriteLine(">>> All files collected.");
            }
        }

        private void FindFilesInDir(string dir, List<string> filesList, CancellationToken token)
        {
            try
            {
                foreach (var fi in Directory.EnumerateFiles(dir))
                {
                    filesList.Add(fi);
                    // _console.WriteLine($"print >>>  {fi}");
                    if (token.IsCancellationRequested)
                    {
                        token.ThrowIfCancellationRequested();
                    }
                }
            }
            catch (UnauthorizedAccessException) { }
            catch (DirectoryNotFoundException) { }
        }

        private void FindFilesWithProtectMode(string dir, List<string> filesList, CancellationToken token)
        {
            if (String.IsNullOrEmpty(dir))
            {
                throw new ArgumentException("Starting directory is a null reference or an empty string: dir");
            }
            try
            {
                foreach (var d in Directory.EnumerateDirectories(dir))
                {
                    // FUCK THE $RECYCLE.BIN
                    if (d.ToLower().Contains("$RECYCLE.BIN".ToLower()))
                    {
                        continue;
                    }
                    if (FolderFilter(d, filterMode))
                    {
                        continue;
                    }
                    if (token.IsCancellationRequested)
                    {
                        token.ThrowIfCancellationRequested();
                    }
                    FindFilesWithProtectMode(d, filesList, token);
                }
                FindFilesInDir(dir, filesList, token);
            }
            catch (UnauthorizedAccessException) { }
        }

        /// <summary>
        /// check if Controlled Or NotExist
        /// </summary>
        private bool IsControlled(string path, bool print = true)
        {
            if (gemini.IsControlledFolder(path))
            {
                if (print)
                {
                    _console.WriteLine($"\r\nThe folder is CONTROLLED, please re-select:\r\n   {path}");
                    _console.WriteLine("\r\nYou could Type \" list controlled \" in the \r\n" +
                        "\"Folder Filter\" and Type ENTER" +
                        " to see all the controlled folders.");
                }
                return true;
            }
            return false;
        }

        private bool UpdateTextAndIniFile(string keyInIni, string path,
            List<string> targetFolderHistory, System.Windows.Forms.TextBox tx = null,
            bool updateIni = true, bool print = true)
        {
            if (IsControlled(path))
            {
                return false;
            }
            if (!Directory.Exists(path))
            {
                if (print)
                {
                    _console.WriteLine($"\r\nThe {keyInIni} folder dose NOT EXIST, please re-select:\r\n   {path}");
                }
                return false;
            }
            // DirectoryIn
            path = Path.GetFullPath(path);
            if (tx != null)
            {
                tx.Text = path;
                targetFolderHistory.Add(path);
                BindHistory(tx, targetFolderHistory);
            }
            if (updateIni)
            {
                gemini.ini.UpdateIniItem(keyInIni, path, "Gemini");
            }
            if (print)
            {
                _console.WriteLine($"\r\nYou have selected {keyInIni} folder:\r\n  {path}");
            }
            return true;
        }

        /// <summary>
        /// Should TEST
        /// </summary>
        private void folderFilterTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (sender is System.Windows.Forms.TextBox box)
                {
                    IsCmdInTextBox(folderFilterTextBox, box.Text);
                }
            }
        }

        private void UpdateREAndModeCheckBox(FilterMode mode)
        {
            if (mode == FilterMode.GEN_FIND)
            {
                regexCheckBox.Checked = false;
                modeCheckBox.Checked = true;
            }
            if (mode == FilterMode.GEN_PROTECT)
            {
                regexCheckBox.Checked = false;
                modeCheckBox.Checked = false;
            }
            if (mode == FilterMode.REGEX_FIND)
            {
                regexCheckBox.Checked = true;
                modeCheckBox.Checked = true;
            }
            if (mode == FilterMode.REGEX_PROTECT)
            {
                regexCheckBox.Checked = true;
                modeCheckBox.Checked = false;
            }
        }
        private void UpdateIniAndTextBox()
        {
            _console.WriteLine($"\r\n >>> FilterMode: {filterMode}");
            UpdateFilterExampleText(filterMode);
            gemini.ini.UpdateIniItem("FilterMode", filterMode.ToString(), "Gemini");
        }
        private bool IsCmdInTextBox(System.Windows.Forms.TextBox box, string cmd)
        {
            cmd = cmd.Trim();

            // command mode
            bool useCommand = false;

            if (cmd.ToLower().Equals("list controlled"))
            {
                _console.WriteLine("\r\nThe following is a list of controlled folders:");
                foreach (var f in gemini.GetAllControlledFolders())
                {
                    _console.WriteLine(f);
                }
                useCommand = true;
            }
            if (cmd.ToLower().Equals("find"))
            {
                useCommand = true;
                FilterMode fimode = filterMode;
                if (regexCheckBox.Checked)
                {
                    if (filterMode == FilterMode.REGEX_FIND)
                    {
                        fimode = FilterMode.REGEX_PROTECT;
                    }
                    if (filterMode == FilterMode.REGEX_PROTECT)
                    {
                        fimode = FilterMode.REGEX_FIND;
                    }
                }
                else
                {
                    if (filterMode == FilterMode.GEN_FIND)
                    {
                        fimode = FilterMode.GEN_PROTECT;
                    }
                    if (filterMode == FilterMode.GEN_PROTECT)
                    {
                        fimode = FilterMode.GEN_FIND;
                    }
                }
                filterMode = fimode;
                UpdateREAndModeCheckBox(filterMode);
            }
            if (cmd.ToLower().Equals("re"))
            {
                useCommand = true;
                FilterMode fimode = filterMode;
                if (modeCheckBox.Checked)
                {
                    if (filterMode == FilterMode.REGEX_FIND)
                    {
                        fimode = FilterMode.GEN_FIND;
                    }
                    if (filterMode == FilterMode.GEN_FIND)
                    {
                        fimode = FilterMode.REGEX_FIND;
                    }
                }
                else
                {
                    if (filterMode == FilterMode.GEN_PROTECT)
                    {
                        fimode = FilterMode.REGEX_PROTECT;
                    }
                    if (filterMode == FilterMode.REGEX_PROTECT)
                    {
                        fimode = FilterMode.GEN_PROTECT;
                    }
                }
                filterMode = fimode;
                UpdateREAndModeCheckBox(filterMode);
            }
            if (cmd.ToLower().Equals("help"))
            {
                _console.WriteLine(gemini.helpString);
                useCommand = true;
            }

            // recover
            if (useCommand)
            {
                box.Text = "";
                return true;
            }
            return false;
        }
        private void targetFolder1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (sender is System.Windows.Forms.TextBox box)
                {
                    var path = box.Text;
                    path = path.Trim();
                    if (!UpdateTextAndIniFile("TargetFolder1", path, targetFolder1History))
                    {
                        return;
                    }
                }
            }
        }

        private void targetFolder2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (sender is System.Windows.Forms.TextBox box)
                {
                    var path = box.Text;
                    path = path.Trim();
                    if (!UpdateTextAndIniFile("TargetFolder2", path, targetFolder2History))
                    {
                        return;
                    }
                }
            }
        }

        private void saveListOrLog2File(bool log = true)
        {
            if (log && tbConsole.Text.Length < 1)
            {
                return;
            }
            if (!log && deleteList.Count < 1)
            {
                return;
            }
            using (var saveFileDialog = new System.Windows.Forms.SaveFileDialog())
            {

                var saveDir = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "LOG");
                if (!Directory.Exists(saveDir))
                {
                    Directory.CreateDirectory(saveDir);
                }
                saveFileDialog.InitialDirectory = saveDir;
                saveFileDialog.Filter = "Txt files (*.txt)|*.txt";
                // saveFileDialog.FilterIndex = 2;
                saveFileDialog.RestoreDirectory = true;
                string t1;
                string t2;
                var f1 = targetFolder1TextBox.Text;
                var f2 = targetFolder2TextBox.Text;
                if (string.IsNullOrEmpty(f1) || !Directory.Exists(f1))
                {
                    t1 = "NONE";
                }
                else
                {
                    t1 = new DirectoryInfo(f1).Name;
                }
                if (string.IsNullOrEmpty(f2) || !Directory.Exists(f2))
                {
                    t2 = "NONE";
                }
                else
                {
                    t2 = new DirectoryInfo(f2).Name;
                }

                var name = t1 + "-" + t2;
                name = name.Replace(":", "_");
                if (!log)
                {
                    saveFileDialog.FileName = "Gemini-list_" + name + "_" +
                                         DateTime.Now.ToString("yyyy-MM-dd_HH-mm") + ".txt";
                }
                else
                {
                    saveFileDialog.FileName = "Gemini-log_" + name + "_" +
                                         DateTime.Now.ToString("yyyy-MM-dd_HH-mm") + ".txt";
                }


                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    using (var stream = saveFileDialog.OpenFile())
                    {
                        // Code to write the stream goes here.
                        byte[] dataAsBytes = null;

                        if (!log)
                        {
                            dataAsBytes = filesList1.SelectMany(s =>
                            System.Text.Encoding.Default.GetBytes(s + Environment.NewLine)).ToArray();
                        }
                        else
                        {
                            dataAsBytes = System.Text.Encoding.Default.GetBytes(tbConsole.Text);
                        }
                        stream.Write(dataAsBytes, 0, dataAsBytes.Length);
                    }
                }
            }

        }

        /// <summary>
        ///             if (!SetFolderFilter(folderFilterTextBox.Text, print: true))
        /// </summary>
        /// <param name="text"></param>
        /// <param name="print"></param>
        /// <returns></returns>
        private bool SetFolderFilter(string text, bool print = false)
        {
            string filter = text;
            folderFilter = new List<string>();
            if (string.IsNullOrEmpty(filter))
            {
                regexFilter = "";
                regex = null;
                _console.WriteLine($">>> Using: {filterMode}, but there is no valid filter value.");
                return true;
            }
            _console.WriteLine($">>> Using: {filterMode}");
            regexFilter = "";
            if (regexCheckBox.Checked)
            {
                regexFilter = filter;
                try
                {
                    regex = new Regex(regexFilter, RegexOptions.IgnoreCase | RegexOptions.Compiled);
                }
                catch (Exception e)
                {
                    _console.WriteLine($"\r\n!!! filter ERROR: {regexFilter} illegal");
                    _console.WriteLine($"\r\n!!! ERROR: {e.Message}");
                    regex = null;
                    return false;
                }
                if (print)
                {
                    _console.WriteLine($"\r\nYou have set the regex filter: \" {regexFilter} \"");
                }
                return true;
            }
            if (filter.Contains("，"))
            {
                if (print) _console.WriteLine("\r\n>>> WARNING: Chinese comma(full-width commas) in the filter <<<\r\n");
            }
            filter = filter.Trim();
            var filterList = filter.Split(',');
            if (filterList.Length < 1)
            {
                return false;
            }
            if (print) _console.WriteLine("\r\nYou have set the following general filter(s):");
            foreach (var ft in filterList)
            {
                if (print) _console.WriteLine($" {ft} ");
                folderFilter.Add(ft);
            }
            return true;
        }

        private void UpdateFilterExampleText(FilterMode mode)
        {
            if (mode == FilterMode.GEN_FIND)
            {
                filterExample.Text = " Using General Find mode";
            }
            if (mode == FilterMode.GEN_PROTECT)
            {
                filterExample.Text = " Using General Protect mode";
            }
            if (mode == FilterMode.REGEX_FIND)
            {
                filterExample.Text = " Using Regex Find mode";
            }
            if (mode == FilterMode.REGEX_PROTECT)
            {
                filterExample.Text = " Using Regex Protect mode";
            }
        }
        private void regexCheckBox_Click(object sender, EventArgs e)
        {
            if (regexCheckBox.Checked)
            {
                if (filterMode == FilterMode.GEN_FIND)
                {
                    filterMode = FilterMode.REGEX_FIND;
                }
                if (filterMode == FilterMode.GEN_PROTECT)
                {
                    filterMode = FilterMode.REGEX_PROTECT;
                }
            }
            else
            {
                if (filterMode == FilterMode.REGEX_PROTECT)
                {
                    filterMode = FilterMode.GEN_PROTECT;
                }
                if (filterMode == FilterMode.REGEX_FIND)
                {
                    filterMode = FilterMode.GEN_FIND;
                }
            }
            UpdateIniAndTextBox();

        }

        private void modeCheckBox_Click(object sender, EventArgs e)
        {
            if (modeCheckBox.Checked)
            {
                if (filterMode == FilterMode.GEN_PROTECT)
                {
                    filterMode = FilterMode.GEN_FIND;
                }
                if (filterMode == FilterMode.REGEX_PROTECT)
                {
                    filterMode = FilterMode.REGEX_FIND;
                }
            }
            else
            {
                if (filterMode == FilterMode.GEN_FIND)
                {
                    filterMode = FilterMode.GEN_PROTECT;
                }
                if (filterMode == FilterMode.REGEX_FIND)
                {
                    filterMode = FilterMode.REGEX_PROTECT;
                }
            }
            UpdateIniAndTextBox();
        }

        private void targetFolder1_DragDrop(object sender, DragEventArgs e)
        {
            targetFolder_DragDrop(sender, e, "TargetFolder1", targetFolder1History,
                targetFolder1TextBox);
        }
        private void targetFolder2_DragDrop(object sender, DragEventArgs e)
        {
            targetFolder_DragDrop(sender, e, "TargetFolder2", targetFolder2History,
                targetFolder2TextBox);
        }

        private void targetFolder_DragDrop(object sender, DragEventArgs e, string keyInIni,
            List<string> targetFolderHistory, System.Windows.Forms.TextBox tx)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] filePaths = (string[])(e.Data.GetData(DataFormats.FileDrop));
                // DirectoryInfo().
                if (filePaths.Length == 1)
                {
                    var path = filePaths[0];
                    if (Directory.Exists(path))
                    {
                        if (Directory.Exists(path))
                        {
                            UpdateTextAndIniFile(keyInIni, path,
                                targetFolderHistory, tx);
                        }
                    }
                }
                else
                {
                    _console.WriteLine("\r\nAttention: File or multiple folders are not allowed!");
                }

            }
        }

        private void targetFolder1_2_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Move;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private void InitFileSameMode()
        {
            // FIX CFG
            fileSizeCheckBox.Checked = true;
            fileSizeCheckBox.Enabled = false;

            // if no ini, just filename and filesize.
            fileNameCheckBox.Checked = true;
            fileExtNameCheckBox.Checked = false;
            fileMD5CheckBox.Checked = false;
            fileSHA1CheckBox.Checked = false;

            ReadFileSameModeFromIni("SameFileName", fileNameCheckBox);
            ReadFileSameModeFromIni("SameFileExtName", fileExtNameCheckBox);
            ReadFileSameModeFromIni("SameFileMD5", fileMD5CheckBox);
            ReadFileSameModeFromIni("SameFileSHA1", fileSHA1CheckBox);

            fileNameCheckBox.Click += fileNameCheckBox_Click;
            fileExtNameCheckBox.Click += fileExtNameCheckBox_Click;
            fileMD5CheckBox.Click += fileMD5CheckBox_Click;
            fileSHA1CheckBox.Click += fileSHA1CheckBox_Click;

            ReadIgnoreFileFromIni();
        }

        private void ReadIgnoreFileFromIni()
        {
            minimumFileLimit = 1024 * 1024; // 1MB
            ignoreFileSizecomboBox.SelectedIndex = 2;
            ignoreFileSizeTextBox.Text = "1";
            ignoreFileCheckBox.Checked = false;
            ignoreFileSizeTextBox.Enabled = false;

            if (gemini.ini.EqualsIgnoreCase("ignoreFileEnabled", "true", "Gemini"))
            {
                ignoreFileCheckBox.Checked = true;
            }

            if (int.TryParse(gemini.ini.Read("ignoreFileIndex", "Gemini"), out int retIndex))
            {
                if (int.TryParse(gemini.ini.Read("ignoreFileTextBox", "Gemini"), out int retNum))
                {
                    minimumFileLimit = retNum * 1024 ^ retIndex;
                    ignoreFileSizecomboBox.SelectedIndex = retIndex;
                    ignoreFileSizeTextBox.Text = retNum.ToString();
                }
            }
            ignoreFileSizeTextBox.Enabled = ignoreFileCheckBox.Checked;

            ignoreFileCheckBox.Click += new EventHandler(ignoreFileCheckBox_Click);
            ignoreFileSizecomboBox.SelectedIndexChanged += new EventHandler(ignoreFileSizecomboBox_SelectedIndexChanged);
        }

        private void ReadFileSameModeFromIni(string key, System.Windows.Forms.CheckBox cb)
        {
            if (gemini.ini.EqualsIgnoreCase(key, "true", "Gemini"))
            {
                cb.Checked = true;
            }

            if (gemini.ini.EqualsIgnoreCase(key, "false", "Gemini"))
            {
                cb.Checked = false;
            }
        }
        private void FileSameModeClick(string key, System.Windows.Forms.CheckBox cb, 
            string conflictKey = null, System.Windows.Forms.CheckBox cbConflict = null)
        {
            if (cb.Checked)
            {
                cbConflict.Checked = false;
            }
            else
            {

            }
            gemini.ini.UpdateIniItem(key, cb.Checked.ToString(), "Gemini");
            gemini.ini.UpdateIniItem(conflictKey, cbConflict.Checked.ToString(), "Gemini");
        }

        private void fileNameCheckBox_Click(object sender, EventArgs e)
        {
            FileSameModeClick("SameFileName", fileNameCheckBox, "SameFileExtName", fileExtNameCheckBox);
        }

        private void fileExtNameCheckBox_Click(object sender, EventArgs e)
        {
            FileSameModeClick("SameFileExtName", fileExtNameCheckBox, "SameFileName", fileNameCheckBox);
        }

        private void fileMD5CheckBox_Click(object sender, EventArgs e)
        {
            FileSameModeClick("SameFileMD5", fileMD5CheckBox, "SameFileSHA1", fileSHA1CheckBox);
        }

        private void fileSHA1CheckBox_Click(object sender, EventArgs e)
        {
            FileSameModeClick("SameFileSHA1", fileSHA1CheckBox, "SameFileMD5", fileMD5CheckBox);
        }


        private void ignoreFileSizecomboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            SetMinimumFileLimit();
        }

        private long SetMinimumFileLimit()
        {
            minimumFileLimit = 0;
            if (ignoreFileCheckBox.Checked)
            {
                ignoreFileSizeTextBox.Enabled = true;
                if (int.TryParse(ignoreFileSizeTextBox.Text, out int ret))
                {
                    switch (ignoreFileSizecomboBox.SelectedIndex)
                    {
                        case 0:
                            minimumFileLimit = ret * 1;
                            break;
                        case 1:
                            minimumFileLimit = ret * 1024;
                            break;
                        case 2:
                            minimumFileLimit = ret * 1024 * 1024;
                            break;
                        case 3:
                            minimumFileLimit = ret * 1024 * 1024 * 1024;
                            break;
                        default:
                            minimumFileLimit = ret * 1024 * 1024;
                            break;
                    }
                }
            }
            else
            {
                ignoreFileSizeTextBox.Enabled = false;
            }
            gemini.ini.UpdateIniItem("ignoreFileEnabled", ignoreFileCheckBox.Checked.ToString(), "Gemini");
            gemini.ini.UpdateIniItem("ignoreFileIndex", ignoreFileSizecomboBox.SelectedIndex.ToString(), "Gemini");
            gemini.ini.UpdateIniItem("ignoreFileTextBox", ignoreFileSizeTextBox.Text, "Gemini");
            return minimumFileLimit;
        }

        private void ignoreFileCheckBox_Click(object sender, EventArgs e)
        {
            SetMinimumFileLimit();
        }

        private void GeminiForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            targetFolder1TextBox.AllowDrop = false;
            targetFolder2TextBox.AllowDrop = false;

            btnStop.PerformClick();
            btnStop.PerformClick();
            btnStop.PerformClick();

            Hide();

            Task.Run(() =>
            {
                // MessageBox.Show("Start WaitAll.");
                Task.WaitAll(_tasks.ToArray());
                // MessageBox.Show("finished.");
                e.Cancel = false;
            }
            );
        }

        private void alwaysOnTopMenu_Click(object sender, EventArgs e)
        {
            //if (alwaysOnTopCheckBox.Checked)
            var it = alwaysOnTopToolStripMenuItem;
            if (!it.Checked)
            {
                it.Checked = true;
                TopMost = true;
            }
            else
            {
                it.Checked = false;
                TopMost = false;
            }
        }
        private void resultListView_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        // https://stackoverflow.com/questions/17746013/how-to-change-order-of-columns-of-listview
        // https://docs.microsoft.com/en-us/troubleshoot/dotnet/csharp/sort-listview-by-column
        private void resultListView_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            // Determine if clicked column is already the column that is being sorted.
            if (e.Column == lvwColumnSorter.SortColumn)
            {
                // Reverse the current sort direction for this column.
                if (lvwColumnSorter.Order == SortOrder.Ascending)
                {
                    lvwColumnSorter.Order = SortOrder.Descending;
                }
                else
                {
                    lvwColumnSorter.Order = SortOrder.Ascending;
                }
            }
            else
            {
                // Set the column number that is to be sorted; default to ascending.
                lvwColumnSorter.SortColumn = e.Column;
                lvwColumnSorter.Order = SortOrder.Ascending;
            }

            // Perform the sort with these new sort options.
            resultListView.Sort();
        }

        private delegate void SetProgressMessageDelegate(int pro, System.Windows.Forms.ProgressBar proBar);
        private void SetProgressMessage(int pro, System.Windows.Forms.ProgressBar proBar)
        {
            if (InvokeRequired)
            {
                if (proBar.IsHandleCreated)
                {
                    SetProgressMessageDelegate setPro = new SetProgressMessageDelegate(SetProgressMessage);
                    Invoke(setPro, new object[] { pro, proBar });
                }
            }
            else
            {
                proBar.Value = Convert.ToInt32(pro);
            }
        }

        private void geminiProgressBar_Click(object sender, EventArgs e)
        {

        }

        private void resultListView_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            // Handles the ItemCheck event. The method uses the CurrentValue
            // property of the ItemCheckEventArgs to retrieve and tally the  
            // price of the menu items selected.  

            var txet = resultListView.Items[e.Index].SubItems["fullPath"].Text;
            if (e.CurrentValue == CheckState.Unchecked)
            {
                deleteList.Add(txet);
                // _console.WriteLine($"Add {txet}");
            }
            else if ((e.CurrentValue == CheckState.Checked))
            {
                try
                {
                    deleteList.Remove(txet);
                }
                catch
                {
                    _console.WriteLine("NO EXIT");
                }
                // _console.WriteLine($"Remove {txet}");
            }
        }

        private void selectAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (resultListView.Items.Count > 1)
            {
                foreach (var item in resultListView.Items)
                {
                    ((System.Windows.Forms.ListViewItem)item).Checked = true;
                }
            }
        }

        private void unselectAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (var item in resultListView.Items)
            {
                ((System.Windows.Forms.ListViewItem)item).Checked = false;
            }
        }

        private void saveLogToFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveListOrLog2File(log: true);
        }

        private void saveResultToFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveListOrLog2File(log: false);
        }

        private void usageToolStripMenuItem_Click(object sender, EventArgs e)
        {
             _console.WriteLine(gemini.helpString);
        }

        private void cleanUpButton_Click(object sender, EventArgs e)
        {
            var taskCleanUp = Task.Run(() => {
                // custQuery is an IEnumerable<IGrouping<string, Customer>>
                if (geminiFileStructListForLV.Count < 1)
                {
                    _console.WriteLine("!!! ANALYZE First.");
                    return;
                }
                GeminiCompareMode mode = SetCompareMode();
                var existList = ListReGrpAndReColor(geminiFileStructListForLV, mode, _source.Token);
                if (existList.Count < geminiFileStructListForLV.Count)
                {
                    _console.WriteLine(
                        $">>> Remove {geminiFileStructListForLV.Count - existList.Count} " +
                        "items from ListView [ nonexistent + non-repeating ].");
                    geminiFileStructListForLV = existList;
                    // RecoverChecked(UpdateListView, geminiFileStructListForLV, mode, _source.Token);
                    UpdateLVCheckedAndDelList(geminiFileStructListForLV);
                    UpdateListView(geminiFileStructListForLV, _source.Token);
                }
                _console.WriteLine(">>> Clean-UP Finished.");
            });
            _tasks.Add(taskCleanUp);
        }
        private void RecoverChecked(GeminiFileStructToListViewDelegate func, 
            List<GeminiFileStruct> gfL, GeminiCompareMode mode, CancellationToken token)
        {
            // foreach (var item in resultListView.CheckedItems) NOT WORK
            var bkp = new List<string>();
            if (deleteList.Count > 1)
            {
                foreach (var item in deleteList)
                {
                    bkp.Add(item);
                }
            }
            func(gfL, token);
            if (bkp.Count > 1)
            {
                foreach (var item in bkp)
                {
                    foreach (var it in resultListView.Items)
                    {
                        var fullPathFromLV = ((System.Windows.Forms.ListViewItem)it).SubItems["fullPath"].Text;
                        if (item.Equals(fullPathFromLV))
                        {
                            ((System.Windows.Forms.ListViewItem)it).Checked = true;
                        }
                    }
                }
            }
            
        }

        private void deleteOrRecycleBin_Click(object sender, EventArgs e)
        {
            if (deleteOrRecycleBin.Checked)
            {
                btnDelete.Text = "Delete";
            }
            else
            {
                btnDelete.Text = "RecycleBin";
            }
        }

        private void reverseElectionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (resultListView.Items.Count > 1)
            {
                foreach (var item in resultListView.Items)
                {
                    var it = (System.Windows.Forms.ListViewItem)item;
                    if (it.Checked)
                    {
                        it.Checked = false;
                    }
                    else
                    {
                        it.Checked = true;
                    }
                }
            }
        }

        private void GeminiFileStructListRE(List<GeminiFileStruct> gfL,
            GeminiFileStruct item, Regex rege, bool find = true)
        {
            item.Checked = !find;
            if (rege.IsMatch(item.fullPath))
            {
                item.Checked = find;
            }
            gfL.Add(item);
        }

        private void GeminiFileStructListGeneral(List<GeminiFileStruct> gfL,
            GeminiFileStruct item, List<string> filter, bool find = true)
        {
            item.Checked = !find;
            foreach (var it in filter)
            {
                if (item.fullPath.ToLower().Contains(it.ToLower()))
                {
                    item.Checked = find;
                    break;
                }
            }
            gfL.Add(item);
        }

        private void UpdateLVCheckedAndDelList(List<GeminiFileStruct> gfl)
        {
            deleteList.Clear();
            if (resultListView.Items.Count > 1)
            {
                foreach (var item in resultListView.Items)
                {
                    var it = (System.Windows.Forms.ListViewItem)item;
                    var fullPathLV = it.SubItems["fullPath"].Text;
                    foreach (var gf in gfl)
                    {
                        var fullPath = gf.fullPath;
                        if (File.Exists(fullPath))
                        {
                            if (fullPathLV.Equals(fullPath))
                            {
                                it.Checked = gf.Checked;
                                if (gf.Checked)
                                {
                                    deleteList.Add(gf.fullPath);
                                }
                            }
                            
                        }
                    }
                }
            }
        }

        private void updateButton_Click(object sender, EventArgs e)
        {
            Task.Run(() => {
                _console.WriteLine($">>> Update start with {filterMode}...");
                SetFolderFilter(folderFilterTextBox.Text, print: true);
                var updatedList = new List<GeminiFileStruct>();
                if (folderFilter.Count > 0)
                {
                    foreach (var item in geminiFileStructListForLV)
                    {
                        GeminiFileStructListGeneral(updatedList, item, folderFilter,
                            find: filterMode == FilterMode.GEN_FIND); // FilterMode.GEN_PROTECT
                    }
                }
                else if (regex != null)
                {
                    foreach (var item in geminiFileStructListForLV)
                    {
                        GeminiFileStructListRE(updatedList, item, regex,
                            find: filterMode == FilterMode.REGEX_FIND); // FilterMode.REGEX_PROTECT
                    }
                }
                geminiFileStructListForLV = ListReGrpAndReColor(updatedList, m_comparemode, _source.Token);
                UpdateListView(geminiFileStructListForLV, _source.Token);
                UpdateLVCheckedAndDelList(geminiFileStructListForLV);

                var cnt =
                    (from i in geminiFileStructListForLV
                     where i.Checked == true
                     select i).Count();
                _console.WriteLine($">>> {filterMode} selectd {cnt:N0} file(s).");
            });
        }

        private void resultListView_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                var focusedItem = resultListView.FocusedItem;
                if (focusedItem == null)
                {
                    return;
                }
                if (focusedItem.SubItems["name"].Bounds.Contains(e.Location))
                {
                    var filePath = focusedItem.SubItems["fullPath"].Text;
                    if (File.Exists(filePath))
                    {
                        ShellContextMenu scm = new ShellContextMenu();
                        FileInfo[] files = new FileInfo[1];
                        files[0] = new FileInfo(filePath);
                        scm.ShowContextMenu(files, Cursor.Position);
                    }
                } else if (focusedItem.SubItems["dir"].Bounds.Contains(e.Location))
                {
                    listViewContextMenuStrip.Show(Cursor.Position);
                }
            }
        }

        /// <summary>
        /// https://stackoverflow.com/questions/17916183/handle-click-on-a-sub-item-of-listview
        /// </summary>
        /*
         private void listView_Click(object sender, EventArgs e)
        {
            Point mousePos = listView.PointToClient(Control.MousePosition);
            ListViewHitTestInfo hitTest = listView.HitTest(mousePos);
            int columnIndex = hitTest.Item.SubItems.IndexOf(hitTest.SubItem);
        }
         */
        private void resultListView_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                var focusedItem = resultListView.FocusedItem;
                if (focusedItem == null)
                {
                    return;
                }
                if (focusedItem.SubItems["dir"].Bounds.Contains(e.Location))
                {
                    var filePath = focusedItem.SubItems["fullPath"].Text;
                    if (File.Exists(filePath))
                    {
                        // combine the arguments together
                        // it doesn't matter if there is a space after ','
                        string argument = "/select, \"" + filePath + "\"";
                        Process.Start("explorer.exe", argument);
                    }
                }
                // THE FIRST ANONYMOUS ITEM MUST USE INDEX, I PERFET SUBITEMS["NAME"]
                else if (focusedItem.SubItems["name"].Bounds.Contains(e.Location))
                {
                    var filePath = focusedItem.SubItems["fullPath"].Text;
                    if (File.Exists(filePath))
                    {
                        // open file.
                        Process.Start(filePath);
                    }
                }
                else
                {
                    // DONOTHING.
                }

                // DOESN'T WORK, HIT.SUBITEM AND HIT.ITEM IS NULL.
                /*Point mousePosition = resultListView.PointToClient(System.Windows.Forms.Control.MousePosition);
                ListViewHitTestInfo hit = resultListView.HitTest(mousePosition);
                // hit.Item.SubItems["fullPath"].Text
                int columnindex = hit.Item.SubItems.IndexOf(hit.SubItem);
                if (resultListView.Columns[columnindex].Name == "dirColumnHeader")
                {
                    var filePath = hit.Item.SubItems["fullPath"].Text;
                    if (File.Exists(filePath))
                    {
                        // combine the arguments together
                        // it doesn't matter if there is a space after ','
                        string argument = "/select, \"" + filePath + "\"";
                        Process.Start("explorer.exe", argument);
                    }
                }*/
            }
        }

        

        private void undoToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void redoToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void openDirectoryToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void copyFullPathToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void deleteAllSelectedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var it = deleteAllSelectedToolStripMenuItem;
            if (it.Checked)
            {
                it.Checked = false;
            }
            else
            {
                it.Checked = true;
            }
        }

        private void ignoreFileSizeTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SetMinimumFileLimit();
            }
        }

        private void ignoreFileSizeTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            // !char.IsControl(e.KeyChar) allow Enter.
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar)) // && (e.KeyChar != '.')
            {
                e.Handled = true;
            }
        }

        private void cleanEmptyFoldersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var it = cleanEmptyFoldersToolStripMenuItem;
            if (it.Checked)
            {
                it.Checked = false;
            }
            else
            {
                it.Checked = true;
            }
        }
    }
}