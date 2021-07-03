﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Threading;
using System.Security.Cryptography;
using System.IO;
using System.Diagnostics;
using System.Windows.Forms;

namespace DailyWallpaper.HashCalc
{
    class HashCalc
    {
        public string help;
        public string filePath;
        public ProgressBar hashProgressBar;

        private Progress<long> totalProgess;
        public List<string> hashList;
        public List<Task> tasks;

        public HashCalc()
        {
            help = "You can drag FILE to picture/hash panel, drag Signature file to console.";
            // totalHashProgess = new ProgressImpl();
            void ProgressAction(long i)
            {
                // readTotal += i;
                hashProgressBar.Invoke(new Action(() =>
                {
                    hashProgressBar.Value = (int)i;
                }));
            }
            totalProgess = new Progress<long>(ProgressAction);
            tasks = new List<Task>();
            hashList = new List<string>();
        }

        /* new ProgressImpl(i => hashProgressBar.Invoke(new Action(() =>
         {
             hashProgressBar.Value = i;
         }));*/
        /* FUCK THE LAMBA.
         * new Progress<int>(i =>
         * {
         *     hashProgressBar.Invoke(new Action(() =>
         *     {
         *         hashProgressBar.Value = i;
         *     }));
         * });*/

        public void CalcCRC32(string path, Action<bool, string, string, string> action, CancellationToken token)
        {

        }
        public void CalcCRC64(string path, Action<bool, string, string, string> action, CancellationToken token)
        {

        }
        public void CalcMD5(string path, Action<bool, string, string, string> action, CancellationToken token)
        {
            Task.Run(() => ComputeHashAsync(action,
                "MD5:    ", MD5.Create(), path, token, totalProgess));
        }
        public void CalcSHA1(string path, Action<bool, string, string, string> action, CancellationToken token)
        {
            Task.Run(async () => await ComputeHashAsync(action,
                "SHA1:   ", SHA1.Create(), path, token, totalProgess));
        }
        public void CalcSHA256(string path, Action<bool, string, string, string> action, CancellationToken token)
        {
            Task.Run(async () => await ComputeHashAsync(action,
                "SHA256: ", SHA256.Create(), path, token, totalProgess));
        }


        /*

         byte[] bytes;
        using (var hash = MD5.Create())
        {
            using (var fs = new FileStream(f, FileMode.Open))
            {
                bytes = await hash.ComputeHashAsync(fs,
                    progress: new Progress<long>(i =>
                    {
                        progressBar1.Invoke(new Action(() =>
                        {
                            progressBar1.Value = i;
                        }));
                    }));
                MessageBox.Show(BitConverter.ToString(bytes).Replace("-", string.Empty));
            }
        }
         */
        /*try
        {
            var s = new CancellationTokenSource();
            s.CancelAfter(1000);
            byte[] bytes;
            using (var hash = MD5.Create())
            {
                using (var fs = new FileStream(f, FileMode.Open))
                {
                    bytes = await hash.ComputeHashAsync(fs,
                        cancelToken: s.Token,
                        progress: new Progress<long>(i =>
                        {
                            progressBar1.Invoke(new Action(() =>
                            {
                                progressBar1.Value = i;
                            }));
                        }));

                    MessageBox.Show(BitConverter.ToString(bytes).Replace("-", string.Empty));
                }
            }
        }
        catch (OperationCanceledException)
        {
            MessageBox.Show("Operation canceled.");
        }*/
        /*
         var f = Path.Combine(Application.StartupPath, "temp.log");
        File.Delete(f);
        using (var fs = new FileStream(f, FileMode.Create))
        {
            fs.Seek(1L * 1024 * 1024 * 1024, SeekOrigin.Begin);
            fs.WriteByte(0);
            fs.Close();
        }
         */

        /// <summary>
        /// https://stackoverflow.com/questions/53965380/report-hash-progress
        /// http://www.alexandre-gomes.com/?p=144
        /// Extension Methods (C# Programming Guide)
        /// https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/extension-methods
        /// </summary>

        /// <summary>
        /// hashAlgorithm = SHA1.Create(), progress: progressbar
        /// </summary>
        /// <param name="hashAlgorithm"></param>
        /// <param name="stream"></param>
        /// <param name="cancelToken"></param>
        /// <param name="progress"></param>
        /// <param name="bufferSize"></param>
        /// <returns></returns>
        public static async Task ComputeHashAsync(Action<bool, string, string, string> action,
            string who, HashAlgorithm hashAlgorithm, string path,
            CancellationToken cancelToken = default(CancellationToken),
            IProgress<long> progress = null, int bufferSize = 1024 * 1024 * 10)
        {
            try
            {
                using (var stream = new FileInfo(path).OpenRead())
                {
                    var timer = new Stopwatch();
                    timer.Start();
                    stream.Position = 0;
                    byte[] readAheadBuffer, buffer;
                    int readAheadBytesRead, bytesRead;
                    long totalBytesRead = 0;
                    var size = stream.Length;
                    readAheadBuffer = new byte[bufferSize];
                    readAheadBytesRead = await stream.ReadAsync(readAheadBuffer, 0,
                        readAheadBuffer.Length, cancelToken);
                    totalBytesRead += readAheadBytesRead;
                    do
                    {
                        bytesRead = readAheadBytesRead;
                        buffer = readAheadBuffer;
                        readAheadBuffer = new byte[bufferSize];
                        readAheadBytesRead = await stream.ReadAsync(readAheadBuffer, 0,
                            readAheadBuffer.Length, cancelToken);
                        totalBytesRead += readAheadBytesRead;

                        if (readAheadBytesRead == 0)
                            hashAlgorithm.TransformFinalBlock(buffer, 0, bytesRead);
                        else
                            hashAlgorithm.TransformBlock(buffer, 0, bytesRead, buffer, 0);
                        if (progress != null)
                            progress.Report((int)((double)totalBytesRead / size * 100));
                        if (cancelToken.IsCancellationRequested)
                            cancelToken.ThrowIfCancellationRequested();
                    } while (readAheadBytesRead != 0);
                    timer.Stop();
                    var hashCostTime = timer.Elapsed.Milliseconds;
                    action(true, $"{who}", GetHash(data: hashAlgorithm.Hash), hashCostTime.ToString() + "ms");
                }
            }
            catch (Exception e)
            {
                action(false, $"{who}", null, e.Message);
            }
        }
            
        // encoding = Encoding.UTF8
        public static string GetHash(HashAlgorithm hashAlgorithm = null, byte[] data = null, 
            string input = null, Encoding encoding = default)
        {
            // Convert the input string to a byte array and compute the hash.
            if (data == null && !string.IsNullOrEmpty(input))
            {
                data = hashAlgorithm.ComputeHash(encoding.GetBytes(input));
            }

            var sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data
            // and format each one as a hexadecimal string.
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("X2"));
            }

            // Return the hexadecimal string.
            return sBuilder.ToString();
        }


    }

    internal class User32TopWindow
    {
        private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        private const UInt32 SWP_NOSIZE = 0x0001;
        private const UInt32 SWP_NOMOVE = 0x0002;
        private const UInt32 TOPMOST_FLAGS = SWP_NOMOVE | SWP_NOSIZE;
        /// <summary>
        /// SetWindowPos(this.Handle, HWND_TOPMOST, 0, 0, 0, 0, TOPMOST_FLAGS);
        /// </summary>
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
        /*
         private void button1_Click(object sender, EventArgs e)
            {
                if (on)
                {
                    button1.Text = "yes on top";
                    IntPtr HwndTopmost = new IntPtr(-1);
                    SetWindowPos(this.Handle, HwndTopmost, 0, 0, 0, 0, TopmostFlags);
                    on = false;
                }
                else
                {
                    button1.Text = "not on top";
                    IntPtr HwndTopmost = new IntPtr(-2);
                    SetWindowPos(this.Handle, HwndTopmost, 0, 0, 0, 0, TopmostFlags);
                    on = true;
                }
            }
         */
    }
}
