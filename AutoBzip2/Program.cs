using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ICSharpCode.SharpZipLib.BZip2;

namespace AutoBzip2
{
    static class Program
    {
        static readonly string worker = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        static readonly string myself = Path.GetFileName(System.Reflection.Assembly.GetExecutingAssembly().Location);

        static void Main()
        {
            Console.Title = "Auto Bzip Compressing   Path: [" + worker + "]";
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("Scanning..." + Environment.NewLine);

            var fail = 0;
            var succ = 0;
            var logs = new List<string>();
            var todo = new List<string>();
            Directory.GetFileSystemEntries(worker, "*.*", SearchOption.AllDirectories)
                .OrderBy(p => p).ToList().ForEach(f =>
                {
                    var fa = File.GetAttributes(f);
                    if (fa.HasFlag(FileAttributes.Directory) || f.Contains(myself) || worker.Equals(Path.GetDirectoryName(f)) || ".bz2".Equals(Path.GetExtension(f)))
                        return;

                    todo.Add(f);
                });

            Directory.CreateDirectory(Path.Combine(worker, "bzip"));

            todo.ForEach(f =>
            {
                try
                {
                    var t = Path.Combine(worker, "bzip", f.RelativePath() + ".bz2");
                    Directory.CreateDirectory(Path.GetDirectoryName(t));
                    var i = new FileStream(f, FileMode.Open);
                    var o = new FileStream(t, FileMode.Create);
                    BZip2.Compress(i, o, true, 9);
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Compressed => {0}", i.Name.RelativePath());
                    succ++;
                }
                catch (Exception e)
                {
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Console.WriteLine("Compression failed => {0} => {1}", f.RelativePath(), e.Message);
                    logs.Add(string.Format("Compression failed => {0} => {1}", f.RelativePath(), e.Message));
                    fail++;
                }
                finally
                {
                    Console.Title = "Auto Bzip Compressing   Path: [" + worker + "]" + "     " + succ + "/" + fail + "/" + todo.Count;
                }
            });

            var list = Path.Combine(worker, "autobz2.log");
            File.WriteAllLines(list, logs.ToArray(), new UTF8Encoding(false));

            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine(Environment.NewLine);
            Console.WriteLine("Total: {0} files | Success: {1} | Failure: {2}", todo.Count, succ, fail);
            Console.ReadKey(true);
        }

        static string RelativePath(this string value)
        {
            return value.Replace(worker, "").Replace('\\', '/').Substring(1);
        }
    }
}
