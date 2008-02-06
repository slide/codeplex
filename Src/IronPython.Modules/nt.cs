/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Microsoft Public License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Public License, please send an email to 
 * ironpy@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

#if !SILVERLIGHT // file-system, environment, process, RNGCryptoServiceProvider

using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

using Microsoft.Scripting;
using Microsoft.Scripting.Math;
using Microsoft.Scripting.Hosting;
using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Utils;

using IronPython.Runtime;
using IronPython.Runtime.Operations;
using IronPython.Runtime.Exceptions;
using IronPython.Runtime.Calls;
using IronPython.Runtime.Types;
using Microsoft.Scripting.Runtime;

[assembly: PythonModule("nt", typeof(IronPython.Modules.PythonNT))]
namespace IronPython.Modules {
    public static class PythonNT {
        #region Public API Surface

        [PythonName("abort")]
        public static void Abort() {
            System.Environment.FailFast("IronPython os.abort");
        }


        [PythonName("chdir")]
        public static void ChangeDirectory(string path) {
            if (String.IsNullOrEmpty(path)) throw PythonExceptions.CreateThrowable(PythonExceptions.OSError, "Invalid argument");

            try {
                Directory.SetCurrentDirectory(path);
            } catch (Exception e) {
                throw ToPythonException(e);
            }
        }

        [PythonName("chmod")]
        public static void ChangeMod(string path, int mode) {
            FileInfo fi = new FileInfo(path);
            if ((mode & S_IREAD) != 0) fi.Attributes |= FileAttributes.ReadOnly;
            else if ((mode & S_IWRITE) != 0) fi.Attributes &= ~(FileAttributes.ReadOnly);
        }

        [PythonName("close")]
        public static void CloseFileDescriptor(int fd) {
            PythonFile pf = PythonFileManager.GetFileFromId(fd);
            pf.Close();
        }

        public static object Environment {
            [PythonName("environ")]
            get {
                return new EnvironmentDictionary();
            }
        }

        public static object error = Builtin.OSError;

        [PythonName("_exit")]
        public static void ExitProcess(int code) {
            ScriptDomainManager.CurrentManager.PAL.TerminateScriptExecution(code);
        }

        [PythonName("fdopen")]
        public static object FileForDescriptor(CodeContext context, int fd) {
            return FileForDescriptor(context, fd, "r");
        }

        [PythonName("fdopen")]
        public static object FileForDescriptor(CodeContext context, int fd, string mode) {
            return FileForDescriptor(context, fd, mode, 0);
        }

        [PythonName("fdopen")]
        public static object FileForDescriptor(CodeContext context, int fd, string mode, int bufsize) {
            PythonFile pf = PythonFileManager.GetFileFromId(fd);
            return pf;
        }

        [PythonName("fstat")]
        public static object GetFileFStats(int fd) {
            PythonFile pf = PythonFileManager.GetFileFromId(fd);
            return GetFileStats(pf.FileName);
        }

        [PythonName("getcwd")]
        public static string GetCurrentDirectory() {
            return Directory.GetCurrentDirectory();
        }

        [PythonName("getcwdu")]
        public static string GetCurrentDirectoryUnicode() {
            return Directory.GetCurrentDirectory();
        }

        [PythonName("getpid")]
        public static int GetCurrentProcessId() {
            return System.Diagnostics.Process.GetCurrentProcess().Id;
        }

        [PythonName("listdir")]
        public static List ListDirectory(string path) {
            List ret = List.Make();
            try {
                string[] files = Directory.GetFiles(path);
                addBase(files, ret);
                addBase(Directory.GetDirectories(path), ret);
                return ret;
            } catch (Exception e) {
                throw ToPythonException(e);
            }
        }

        // 
        // lstat(path) -> stat result
        // Like stat(path), but do not follow symbolic links.
        // 
        [PythonName("lstat")]
        public static object GetFileLStats(string path) {
            return GetFileStats(path);
        }

        [PythonName("mkdir")]
        public static void MakeDirectory(string path) {
            if (Directory.Exists(path)) throw PythonOps.OSError("directory already exists");

            try {
                Directory.CreateDirectory(path);
            } catch (Exception e) {
                throw ToPythonException(e);
            }
        }

        [PythonName("mkdir")]
        public static void MakeDirectory(string path, int mode) {
            if (Directory.Exists(path)) throw PythonOps.OSError("directory already exists");
            // we ignore mode

            try {
                Directory.CreateDirectory(path);
            } catch (Exception e) {
                throw ToPythonException(e);
            }
        }

        [PythonName("open")]
        public static object Open(CodeContext context, string filename, int flag) {
            return Open(context, filename, flag, 0777);
        }

        [PythonName("open")]
        public static object Open(CodeContext/*!*/ context, string filename, int flag, int mode) {
            try {
                FileStream fs = File.Open(filename, FileModeFromFlags(flag), FileAccessFromFlags(flag));

                string mode2;
                if (fs.CanRead && fs.CanWrite) mode2 = "w+";
                else if (fs.CanWrite) mode2 = "w";
                else mode2 = "r";

                return PythonFileManager.AddToStrongMapping(PythonFile.Create(context, fs, filename, mode2, false));
            } catch (Exception e) {
                throw ToPythonException(e);
            }
        }

        [PythonName("popen")]
        public static PythonFile OpenPipedCommand(CodeContext context, string command) {
            return OpenPipedCommand(context, command, "r");
        }

        [PythonName("popen")]
        public static PythonFile OpenPipedCommand(CodeContext context, string command, string mode) {
            return OpenPipedCommand(context, command, mode, 4096);
        }

        [PythonName("popen")]
        public static PythonFile OpenPipedCommand(CodeContext context, string command, string mode, int bufsize) {
            if (String.IsNullOrEmpty(mode)) mode = "r";
            ProcessStartInfo psi = GetProcessInfo(command);
            psi.CreateNoWindow = true;  // ipyw shouldn't create a new console window
            Process p;
            PythonFile res;

            try {
                switch (mode) {
                    case "r":
                        psi.RedirectStandardOutput = true;
                        p = Process.Start(psi);

                        res = new POpenFile(context, command, p, p.StandardOutput.BaseStream, "r");
                        break;
                    case "w":
                        psi.RedirectStandardInput = true;
                        p = Process.Start(psi);

                        res = new POpenFile(context, command, p, p.StandardInput.BaseStream, "w");
                        break;
                    default:
                        throw PythonOps.ValueError("expected 'r' or 'w' for mode, got {0}", mode);
                }
            } catch (Exception e) {
                throw ToPythonException(e);
            }

            return res;
        }

        [PythonName("popen2")]
        public static PythonTuple OpenPipedCommandBoth(CodeContext context, string command) {
            return OpenPipedCommandBoth(context, command, "t");
        }

        [PythonName("popen2")]
        public static PythonTuple OpenPipedCommandBoth(CodeContext context, string command, string mode) {
            return OpenPipedCommandBoth(context, command, "t", 4096);
        }

        [PythonName("popen2")]
        public static PythonTuple OpenPipedCommandBoth(CodeContext context, string command, string mode, int bufsize) {
            if (String.IsNullOrEmpty(mode)) mode = "t";
            if (mode != "t" && mode != "b") throw PythonOps.ValueError("mode must be 't' or 'b' (default is t)");
            if (mode == "t") mode = String.Empty;

            try {
                ProcessStartInfo psi = GetProcessInfo(command);
                psi.RedirectStandardInput = true;
                psi.RedirectStandardOutput = true;
                psi.CreateNoWindow = true; // ipyw shouldn't create a new console window
                Process p = Process.Start(psi);

                return PythonTuple.MakeTuple(new POpenFile(context, command, p, p.StandardInput.BaseStream, "w" + mode),
                        new POpenFile(context, command, p, p.StandardOutput.BaseStream, "r" + mode));
            } catch (Exception e) {
                throw ToPythonException(e);
            }
        }

        [PythonName("popen3")]
        public static PythonTuple OpenPipedCommandAll(CodeContext context, string command) {
            return OpenPipedCommandAll(context, command, "t");
        }

        [PythonName("popen3")]
        public static PythonTuple OpenPipedCommandAll(CodeContext context, string command, string mode) {
            return OpenPipedCommandAll(context, command, "t", 4096);
        }

        [PythonName("popen3")]
        public static PythonTuple OpenPipedCommandAll(CodeContext context, string command, string mode, int bufsize) {
            if (String.IsNullOrEmpty(mode)) mode = "t";
            if (mode != "t" && mode != "b") throw PythonOps.ValueError("mode must be 't' or 'b' (default is t)");
            if (mode == "t") mode = String.Empty;

            try {
                ProcessStartInfo psi = GetProcessInfo(command);
                psi.RedirectStandardInput = true;
                psi.RedirectStandardOutput = true;
                psi.RedirectStandardError = true;
                psi.CreateNoWindow = true; // ipyw shouldn't create a new console window
                Process p = Process.Start(psi);

                return PythonTuple.MakeTuple(new POpenFile(context, command, p, p.StandardInput.BaseStream, "w" + mode),
                        new POpenFile(context, command, p, p.StandardOutput.BaseStream, "r" + mode),
                        new POpenFile(context, command, p, p.StandardError.BaseStream, "r+" + mode));
            } catch (Exception e) {
                throw ToPythonException(e);
            }
        }


        [PythonName("putenv")]
        public static void PutEnvironment(string varname, string value) {
            try {
                System.Environment.SetEnvironmentVariable(varname, value);
            } catch (Exception e) {
                throw ToPythonException(e);
            }
        }

        [PythonName("read")]
        public static string ReadFromFileDescriptor(int fd, int buffersize) {
            try {
                PythonFile pf = PythonFileManager.GetFileFromId(fd);
                return pf.Read();
            } catch (Exception e) {
                throw ToPythonException(e);
            }
        }

        [PythonName("remove")]
        public static void RemoveFile(string path) {
            try {
                UnlinkFile(path);
            } catch (Exception e) {
                throw ToPythonException(e);
            }
        }

        [PythonName("rename")]
        public static void Rename(string src, string dst) {
            try {
                Directory.Move(src, dst);
            } catch (Exception e) {
                throw ToPythonException(e);
            }
        }

        [PythonName("rmdir")]
        public static void RemoveDirectory(string path) {
            try {
                Directory.Delete(path);
            } catch (Exception e) {
                throw ToPythonException(e);
            }
        }

        [PythonName("spawnl")]
        public static object SpawnProcess(int mode, string path, params object[] args) {
            return SpawnProcessImpl(MakeProcess(), mode, path, args);
        }

        [PythonName("spawnle")]
        public static object SpawnProcessWithParamsArgsAndEnvironment(int mode, string path, params object[] args) {
            if (args.Length < 1) {
                throw PythonOps.TypeError("spawnle() takes at least three arguments ({0} given)", 2 + args.Length);
            }

            object env = args[args.Length - 1];
            object[] slicedArgs = ArrayUtils.RemoveFirst(args);

            Process process = MakeProcess();
            SetEnvironment(process.StartInfo.EnvironmentVariables, env);

            return SpawnProcessImpl(process, mode, path, slicedArgs);
        }

        [PythonName("spawnv")]
        public static object SpawnProcess(int mode, string path, object args) {
            return SpawnProcessImpl(MakeProcess(), mode, path, args);
        }

        [PythonName("spawnve")]
        public static object SpawnProcess(int mode, string path, object args, object env) {
            Process process = MakeProcess();
            SetEnvironment(process.StartInfo.EnvironmentVariables, env);

            return SpawnProcessImpl(process, mode, path, args);
        }

        private static Process MakeProcess() {
            try {
                return new Process();
            } catch (Exception e) {
                throw ToPythonException(e);
            }
        }

        private static object SpawnProcessImpl(Process process, int mode, string path, object args) {
            try {
                process.StartInfo.Arguments = ArgumentsToString(args);
                process.StartInfo.FileName = path;
                process.StartInfo.UseShellExecute = false;
            } catch (Exception e) {
                throw ToPythonException(e);
            }

            if (!process.Start()) {
                throw PythonOps.OSError("Cannot start process: {0}", path);
            }
            if (mode == (int)P_WAIT) {
                process.WaitForExit();
                int exitCode = process.ExitCode;
                process.Close();
                return exitCode;
            } else {
                return process.Id;
            }
        }

        /// <summary>
        /// Copy elements from a Python mapping of dict environment variables to a StringDictionary.
        /// </summary>
        private static void SetEnvironment(System.Collections.Specialized.StringDictionary currentEnvironment, object newEnvironment) {
            PythonDictionary env = newEnvironment as PythonDictionary;
            if (env == null) {
                throw PythonOps.TypeError("env argument must be a dict");
            }

            currentEnvironment.Clear();

            string strKey, strValue;
            foreach (object key in env.Keys) {
                if (!Converter.TryConvertToString(key, out strKey)) {
                    throw PythonOps.TypeError("env dict contains a non-string key");
                }
                if (!Converter.TryConvertToString(env[key], out strValue)) {
                    throw PythonOps.TypeError("env dict contains a non-string value");
                }
                currentEnvironment[strKey] = strValue;
            }
        }

        /// <summary>
        /// Convert a sequence of args to a string suitable for using to spawn a process.
        /// </summary>
        private static string ArgumentsToString(object args) {
            IEnumerator argsEnumerator;
            System.Text.StringBuilder sb = null;
            if (!Converter.TryConvertToIEnumerator(args, out argsEnumerator)) {
                throw PythonOps.TypeError("args parameter must be sequence, not {0}", DynamicHelpers.GetPythonType(args));
            }

            bool space = false;
            try {
                // skip the first element, which is the name of the command being run
                argsEnumerator.MoveNext();
                while (argsEnumerator.MoveNext()) {
                    if (sb == null) sb = new System.Text.StringBuilder(); // lazy creation
                    string strarg = PythonOps.ToString(argsEnumerator.Current);
                    if (space) {
                        sb.Append(' ');
                    }
                    if (strarg.IndexOf(' ') != -1) {
                        sb.Append('"');
                        sb.Append(strarg);
                        sb.Append('"');
                    } else {
                        sb.Append(strarg);
                    }
                    space = true;
                }
            } finally {
                IDisposable disposable = argsEnumerator as IDisposable;
                if (disposable != null) disposable.Dispose();
            }

            if (sb == null) return "";
            return sb.ToString();
        }

        [PythonName("startfile")]
        public static void StartFile(string filename) {
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            process.StartInfo.FileName = filename;
            process.StartInfo.UseShellExecute = true;
            process.Start();
        }


        [PythonType("stat_result")]
        public class StatResult : ISequence {
            // !!! We should convert this to be a subclass of Tuple (so that it implements
            // the whole tuple protocol) or at least use a Tuple for internal storage rather
            // than converting back and forth all the time. We also need to support constructors
            // with 11-13 arguments.

            public static object n_fields = (object)13;
            public static object n_sequence_fields = (object)10;
            public static object n_unnamed_fields = (object)3;

            internal BigInteger mode, ino, dev, nlink, uid, gid, size, atime, mtime, ctime;

            internal StatResult() {
            }

            public StatResult(ISequence statResult, [DefaultParameterValue(null)]object dict) {
                // dict is allowed by CPython's stat_result, but doesn't seem to do anything, so we ignore it here.

                if (statResult.GetLength() != 10) {
                    throw PythonOps.TypeError("stat_result() takes a 10-sequence ({0}-sequence given)", statResult.GetLength());
                }

                this.mode = Converter.ConvertToBigInteger(statResult[0]);
                this.ino = Converter.ConvertToBigInteger(statResult[1]);
                this.dev = Converter.ConvertToBigInteger(statResult[2]);
                this.nlink = Converter.ConvertToBigInteger(statResult[3]);
                this.uid = Converter.ConvertToBigInteger(statResult[4]);
                this.gid = Converter.ConvertToBigInteger(statResult[5]);
                this.size = Converter.ConvertToBigInteger(statResult[6]);
                this.atime = Converter.ConvertToBigInteger(statResult[7]);
                this.mtime = Converter.ConvertToBigInteger(statResult[8]);
                this.ctime = Converter.ConvertToBigInteger(statResult[9]);
            }

            public object StatATime {
                [PythonName("st_atime")]
                get {
                    return atime;
                }
            }
            public object StatCTime {
                [PythonName("st_ctime")]
                get {
                    return ctime;
                }
            }
            public object StatDev {
                [PythonName("st_dev")]
                get {
                    return dev;
                }
            }
            public object StatGid {
                [PythonName("st_gid")]
                get {
                    return gid;
                }
            }
            public object StatIno {
                [PythonName("st_ino")]
                get {
                    return ino;
                }
            }
            public object StatMode {
                [PythonName("st_mode")]
                get {
                    return mode;
                }
            }
            public object StatMTime {
                [PythonName("st_mtime")]
                get {
                    return mtime;
                }
            }
            public object StatNLink {
                [PythonName("st_nlink")]
                get {
                    return nlink;
                }
            }
            public object StatSize {
                [PythonName("st_size")]
                get {
                    return size;
                }
            }
            public object StatUid {
                [PythonName("st_uid")]
                get {
                    return uid;
                }
            }

            public override string ToString() {
                return MakeTuple().ToString();
            }

            [PythonName("__reduce__")]
            public PythonTuple Reduce() {
                PythonDictionary timeDict = new PythonDictionary(3);
                timeDict["st_atime"] = StatATime;
                timeDict["st_ctime"] = StatCTime;
                timeDict["st_mtime"] = StatMTime;

                return PythonTuple.MakeTuple(
                    DynamicHelpers.GetPythonTypeFromType(typeof(StatResult)),
                    PythonTuple.MakeTuple(MakeTuple(), timeDict)
                );
            }

            #region ISequence Members

            //public object AddSequence(object other) {
            //    return MakeTuple().AddSequence(other);
            //}

            //public object MultiplySequence(object count) {
            //    return MakeTuple().MultiplySequence(count);
            //}

            public object this[int index] {
                get {
                    return MakeTuple()[index];
                }
            }

            public object this[Slice slice] {
                get {
                    return MakeTuple()[slice];
                }
            }

            public object GetSlice(int start, int stop) {
                return MakeTuple().GetSlice(start, stop);
            }

            #endregion

            #region IPythonContainer Members

            public int GetLength() {
                return MakeTuple().Count;
            }

            public bool ContainsValue(object item) {
                return MakeTuple().ContainsValue(item);
            }

            #endregion

            private PythonTuple MakeTuple() {
                return PythonTuple.MakeTuple(
                    StatMode,
                    StatIno,
                    StatDev,
                    StatNLink,
                    StatUid,
                    StatGid,
                    StatSize,
                    StatATime,
                    StatMTime,
                    StatCTime
                );
            }

            #region Object overrides

            public override bool Equals(object obj) {
                if (obj is StatResult) {
                    return MakeTuple().Equals(((StatResult)obj).MakeTuple());
                } else {
                    return MakeTuple().Equals(obj);
                }

            }

            public override int GetHashCode() {
                return MakeTuple().GetHashCode();
            }

            #endregion
        }

        [Documentation("stat(path) -> stat result\nGathers statistics about the specified file or directory")]
        [PythonName("stat")]
        public static object GetFileStats(string path) {
            if (path == null) throw PythonOps.TypeError("expected string, got NoneType");

            StatResult sr = new StatResult();

            try {
                sr.atime = (long)Directory.GetLastAccessTime(path).Subtract(DateTime.MinValue).TotalSeconds;
                sr.ctime = (long)Directory.GetCreationTime(path).Subtract(DateTime.MinValue).TotalSeconds;
                sr.mtime = (long)Directory.GetLastWriteTime(path).Subtract(DateTime.MinValue).TotalSeconds;

                if (Directory.Exists(path)) {
                    sr.mode = 0x4000;
                } else if (File.Exists(path)) {
                    FileInfo fi = new FileInfo(path);
                    sr.size = fi.Length;
                    sr.mode = 0x8000; //@TODO - Set other valid mode types (S_IFCHR, S_IFBLK, S_IFIFO, S_IFLNK, S_IFSOCK) (to the degree that they apply)
                } else {
                    throw new IOException("file does not exist");
                }
            } catch (ArgumentException) {
                throw PythonOps.OSError("The path is invalid: {0}", path);
            } catch (Exception e) {
                throw ToPythonException(e);
            }

            return sr;
        }

        [PythonName("tempnam")]
        public static string GetTemporaryFileName() {
            return GetTemporaryFileName(null);
        }

        [PythonName("tempnam")]
        public static string GetTemporaryFileName(string dir) {
            return GetTemporaryFileName(null, null);
        }

        [PythonName("tempnam")]
        public static string GetTemporaryFileName(string dir, string prefix) {
            try {
                if (dir == null) dir = Path.GetTempPath();
                else dir = Path.GetDirectoryName(dir);

                return Path.GetFullPath(Path.Combine(dir, prefix ?? String.Empty) + Path.GetRandomFileName());
            } catch (Exception e) {
                throw ToPythonException(e);
            }
        }

        [PythonName("times")]
        public static object GetProcessTimeInfo() {
            System.Diagnostics.Process p = System.Diagnostics.Process.GetCurrentProcess();

            return PythonTuple.MakeTuple(p.UserProcessorTime.TotalSeconds,
                p.PrivilegedProcessorTime.TotalSeconds,
                0,  // child process system time
                0,  // child process os time
                DateTime.Now.Subtract(p.StartTime).TotalSeconds);
        }

        [PythonName("tmpfile")]
        public static PythonFile GetTemporaryFile(CodeContext context) {
            try {
                FileStream sw = new FileStream(Path.GetTempFileName(), FileMode.Open, FileAccess.ReadWrite, FileShare.None, 4096, FileOptions.DeleteOnClose);

                PythonFile res = PythonFile.Create(context, sw, sw.Name, "w+b");
                return res;
            } catch (Exception e) {
                throw ToPythonException(e);
            }
        }

        [PythonName("tmpnam")]
        public static string GetTempPath() {
            return Path.GetTempPath();
        }

        [PythonName("unlink")]
        public static void UnlinkFile(string path) {
            try {
                File.Delete(path);
            } catch (Exception e) {
                throw ToPythonException(e);
            }
        }

        [PythonName("unsetenv")]
        public static void DeleteEnvironmentVariable(string varname) {
            System.Environment.SetEnvironmentVariable(varname, null);
        }

        [PythonName("urandom")]
        public static object GetRandomData(int n) {
            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
            byte[] data = new byte[n];
            rng.GetBytes(data);

            return PythonBinaryReader.PackDataIntoString(data, n);
        }

        [PythonName("utime")]
        public static void SetFileTimes(string path, PythonTuple times) {
            try {
                FileInfo fi = new FileInfo(path);
                if (times == null) {
                    fi.LastAccessTime = DateTime.Now;
                    fi.LastWriteTime = DateTime.Now;
                } else if (times.Count == 2) {
                    DateTime atime = DateTime.MinValue.Add(TimeSpan.FromSeconds(Converter.ConvertToDouble(times[0])));
                    DateTime mtime = DateTime.MinValue.Add(TimeSpan.FromSeconds(Converter.ConvertToDouble(times[1])));

                    fi.LastAccessTime = atime;
                    fi.LastWriteTime = mtime;
                } else {
                    throw PythonOps.TypeError("times value must be a 2-value tuple (atime, mtime)");
                }
            } catch (Exception e) {
                throw ToPythonException(e);
            }
        }

        [PythonName("waitpid")]
        public static PythonTuple WaitForProcess(int pid, object options) {
            System.Diagnostics.Process process = System.Diagnostics.Process.GetProcessById(pid);
            if (process == null) {
                throw PythonOps.OSError("Cannot find process {0}", pid);
            }
            process.WaitForExit();
            return PythonTuple.MakeTuple(pid, process.ExitCode);
        }

        [PythonName("write")]
        public static void WriteToFileDescriptor(int fd, string text) {
            try {
                PythonFile pf = PythonFileManager.GetFileFromId(fd);
                pf.Write(text);
            } catch (Exception e) {
                throw ToPythonException(e);
            }
        }

        public static object O_APPEND = 0x8;
        public static object O_CREAT = 0x100;
        public static object O_TRUNC = 0x200;

        public static object O_EXCL = 0x400;
        public static object O_NOINHERIT = 0x80;

        public static object O_RANDOM = 0x10;
        public static object O_SEQUENTIAL = 0x20;

        public static object O_SHORT_LIVED = 0x1000;
        public static object O_TEMPORARY = 0x40;

        public static object O_WRONLY = 0x1;
        public static object O_RDONLY = 0x0;
        public static object O_RDWR = 0x2;

        public static object O_BINARY = 0x8000;
        public static object O_TEXT = 0x4000;

        public static object P_WAIT = 0;
        public static object P_NOWAIT = 1;
        public static object P_NOWAITO = 3;
        // Not implemented:
        // public static object P_OVERLAY = 2;
        // public static object P_DETACH = 4;

        #endregion

        #region Private implementation details

        private static Exception ToPythonException(Exception e) {
            if (e is ArgumentException || e is ArgumentNullException || e is ArgumentTypeException) {
                // rethrow reasonable exceptions
                return ExceptionHelpers.UpdateForRethrow(e);
            }

            string message = e.Message;
            int hr = System.Runtime.InteropServices.Marshal.GetHRForException(e);
            if ((hr & ~0xfff) == (unchecked((int)0x80070000))) {
                // Win32 HR, translate HR to Python error code if possible, otherwise
                // report the HR.
                switch (hr & 0xfff) {
                    case ERROR_FILE_EXISTS:
                        hr = PythonErrorNumber.EEXIST;
                        break;
                    case ERROR_ACCESS_DENIED:
                        hr = PythonErrorNumber.EACCES;
                        break;                    
                }
                message = "[Errno " + hr.ToString() + "] " + e.Message;
            }

            return PythonExceptions.CreateThrowable(PythonExceptions.OSError, hr, message);
        }

        private const int ERROR_FILE_EXISTS = 80;
        private const int ERROR_ACCESS_DENIED = 5;

        private const int S_IWRITE = 0x80;
        private const int S_IREAD = 0x100;

        private static void addBase(string[] files, List ret) {
            foreach (string file in files) {
                ret.AddNoLock(Path.GetFileName(file));
            }
        }

        private static FileMode FileModeFromFlags(int flags) {
            if ((flags & (int)O_APPEND) != 0) return FileMode.Append;
            if ((flags & (int)O_CREAT) != 0) return FileMode.CreateNew;
            if ((flags & (int)O_TRUNC) != 0) return FileMode.Truncate;
            return FileMode.Open;
        }

        private static FileAccess FileAccessFromFlags(int flags) {
            if ((flags & (int)O_RDWR) != 0) return FileAccess.ReadWrite;
            if ((flags & (int)O_WRONLY) != 0) return FileAccess.Write;

            return FileAccess.Read;
        }

        [PythonType(typeof(PythonFile))]
        private class POpenFile : PythonFile {
            private Process _process;

            [PythonName("__new__")]
            public static object Make(CodeContext/*!*/ context, string command, Process process, Stream stream, string mode) {
                return new POpenFile(context, command, process, stream, mode);
            }

            internal POpenFile(CodeContext/*!*/ context, string command, Process process, Stream stream, string mode) {
                Initialize(stream, PythonContext.GetContext(context).DefaultEncoding, command, mode);
                this._process = process;
            }

            [PythonName("close")]
            public override object Close() {
                base.Close();

                if (_process.HasExited && _process.ExitCode != 0) {
                    return _process.ExitCode;
                }

                return null;
            }
        }

        private static ProcessStartInfo GetProcessInfo(string command) {
            command = command.Trim();

            string baseCommand = command;
            string args = string.Empty;
            int pos;

            if (command[0] == '\"') {
                for (pos = 1; pos < command.Length; pos++) {
                    if (command[pos] == '\"') {
                        baseCommand = command.Substring(1, pos - 1).Trim();
                        if (pos + 1 < command.Length) {
                            args = command.Substring(pos + 1);
                        }
                        break;
                    }
                }
                if (pos == command.Length)
                    throw PythonOps.ValueError("mismatch quote in command");
            } else {
                pos = command.IndexOf(' ');
                if (pos != -1) {
                    baseCommand = command.Substring(0, pos);
                    // pos won't be the last one
                    args = command.Substring(pos + 1);
                }
            }

            baseCommand = GetCommandFullPath(baseCommand);

            ProcessStartInfo psi = new ProcessStartInfo(baseCommand, args);
            psi.UseShellExecute = false;

            return psi;
        }

        private static string GetCommandFullPath(string command) {
            string fullpath = Path.GetFullPath(command);
            if (File.Exists(fullpath)) return fullpath;

            // TODO: need revisit
            string sysdir = System.Environment.GetFolderPath(System.Environment.SpecialFolder.System);
            foreach (string suffix in new string[] { string.Empty, ".com", ".exe", "cmd", ".bat" }) {
                fullpath = Path.Combine(sysdir, command + suffix);
                if (File.Exists(fullpath)) return fullpath;
            }

            throw PythonOps.WindowsError("The system can not find command '{0}'", command);
        }

        #endregion
    }
}

#endif
