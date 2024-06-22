using System.Text;
using System.Text.RegularExpressions;

namespace FileCompiler;

internal sealed class FileCompiler
{
    private readonly Options _options;
    private readonly Regex _fileRegex;
    private readonly Regex _lineRegex;
    private readonly Regex _paramRegex;
    
    internal static void Run(Options options)
    {
        FileCompiler fileCompiler = new FileCompiler(options);
        fileCompiler.Run();
    }

    internal FileCompiler(Options options)
    {
        _options = options;
        _fileRegex = new Regex(_options.FileRegex, RegexOptions.Compiled);
        if (!_fileRegex.GetGroupNames().Contains("command"))
        {
            throw new Exception("File regex must have a group called command.");
        }
        _lineRegex = new Regex(_options.LineRegex, RegexOptions.Compiled);
        if (!_lineRegex.GetGroupNames().Contains("command"))
        {
            throw new Exception("Line regex must have a group called command.");
        }
        _paramRegex = new Regex(_options.ParamRegex, RegexOptions.Compiled);
        if (!_paramRegex.GetGroupNames().Contains("param"))
        {
            throw new Exception("Param regex must have a group called param.");
        }
    }

    internal void Run()
    {
        if (_options.Delete && Directory.Exists(_options.Out))
        {
            Directory.Delete(_options.Out, true);
        }
        UpdateAllFiles();
        
        // if (_options.Watch)
        // {
        //     using FileSystemWatcher watcher = new FileSystemWatcher(_options.Input);
        //     watcher.NotifyFilter = NotifyFilters.FileName |
        //                            NotifyFilters.DirectoryName |
        //                            NotifyFilters.Attributes |
        //                            NotifyFilters.Size |
        //                            NotifyFilters.LastWrite |
        //                            NotifyFilters.LastAccess |
        //                            NotifyFilters.CreationTime |
        //                            NotifyFilters.Security;
        //     watcher.IncludeSubdirectories = true;
        //     watcher.EnableRaisingEvents = true;
        //     watcher.Changed += OnWatcher;
        //     watcher.Created += OnWatcher;
        //     watcher.Deleted += OnWatcher;
        //     watcher.Renamed += OnWatcher;
        //     while (Console.ReadKey().KeyChar != 'q') ;
        // }
    }

    // private int i = 0;
    // private void OnWatcher(object sender, FileSystemEventArgs args)
    // {
    //     Console.WriteLine(args.FullPath);
    //     Console.WriteLine(i++);
    //     Console.WriteLine("Press 'q' to exit.");
    //     Console.WriteLine("Change detected, recompiling.");
    //     switch (args.ChangeType)
    //     {
    //         case WatcherChangeTypes.Created:
    //             UpdateSingleFile(args.FullPath);
    //             break;
    //         case WatcherChangeTypes.Deleted:
    //             File.Delete(GetOutPath(args.FullPath));
    //             break;
    //         case WatcherChangeTypes.Changed:
    //             UpdateSingleFile(args.FullPath);
    //             break;
    //         case WatcherChangeTypes.Renamed:
    //             File.Delete(GetOutPath(((RenamedEventArgs)args).OldFullPath));
    //             UpdateSingleFile(args.FullPath);
    //             break;
    //         case WatcherChangeTypes.All:
    //             break;
    //         default:
    //             throw new ArgumentOutOfRangeException();
    //     }
    //     Console.WriteLine("Compilation done!");
    // }

    internal void UpdateAllFiles()
    {
        foreach (string file in Directory.GetFiles(_options.Input, "*", SearchOption.AllDirectories))
        {
            UpdateSingleFile(file);
        }
    }

    internal void UpdateSingleFile(string inPath)
    {
        string outPath = GetOutPath(inPath);
        string outDir = new FileInfo(outPath).DirectoryName!;
        if (File.Exists(outPath))
        {
            File.Delete(outPath);
        }

        IEnumerable<string> lines = File.ReadLines(inPath).Select(l =>
        {
            l = _fileRegex.Replace(l, m => ExpandCommand(inPath, m));
            l = _lineRegex.Replace(l, m => ExpandCommand(inPath, m).ReplaceLineEndings(""));
            return l;
        });

        if (!Directory.Exists(outDir))
        {
            Directory.CreateDirectory(outDir);
        }
        File.WriteAllText(outPath, string.Join(Environment.NewLine, lines));
    }

    internal string GetOutPath(string inPath)
    {
        string relativePath = Path.GetRelativePath(_options.Input, inPath);
        if (_options.Extension != null)
        {
            relativePath = Path.GetFileNameWithoutExtension(relativePath);
            relativePath += _options.Extension;
        }
        string outPath = Path.Combine(_options.Out, relativePath);
        return outPath;
    }

    internal string ExpandCommand(string inPath, Match match)
    {
        string inDir = new FileInfo(inPath).DirectoryName!;
        if (match.Groups["command"].Captures.Count != 1)
        {
            Console.Error.WriteLine($"No command name found: {inPath}:{match.Index}.");
            return match.Value;
        }

        string command = match.Groups["command"].Captures.First().Value;
        string[] args = match.Groups["param"].Captures.Select(c => c.Value).ToArray();

        return LoadFile(Path.Combine(inDir, command), args);
    }

    internal string LoadFile(string path, string[] args)
    {
        IEnumerable<string> lines = File.ReadAllLines(path).Select(l => _paramRegex.Replace(l, match =>
        {
            if (match.Groups["param"].Captures.Count != 1)
            {
                Console.Error.WriteLine($"No parameter index found: {path}#{match.Index}");
                return match.Value;
            }

            if (!int.TryParse(match.Groups.Values.Skip(1).First().Value, out int paramIndex))
            {
                Console.Error.WriteLine("Param was not a valid integer.");
                return match.Value;
            }

            return args[paramIndex];
        }));

        return string.Join(Environment.NewLine, lines);
    }
}