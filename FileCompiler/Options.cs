using CommandLine;

namespace FileCompiler;

internal sealed class Options
{
    [Value(0, Default = "./", HelpText = "The input directory of the files")]
    public string Input { get; set; } = "./";

    [Option('o', "output-dir", Default = "./Output", HelpText = "The output directory of the files.")]
    public string Out { get; set; } = "./Output";

    [Option('d', "delete", Default = true, HelpText = "Delete the output directory before writing.")]
    public bool Delete { get; set; } = true;

    // [Option('w', "watch", Default = false, HelpText = "If set the files will be watched for changes.")]
    // public bool Watch { get; set; } = false;

    [Option('e', "extension", Default = null, HelpText = "A new file extension for the files")]
    public string? Extension { get; set; } = null;

    [Option("line", Default = "\\$<(?<command>\\S+)(?:\\s(?:(?:\"(?<param>.*)\")|(?<param>\\S+)))*>", HelpText = "The regex for inserting a file on a single line.")]
    public string LineRegex { get; set; } = "";
    
    [Option("file", Default = "\\${(?<command>\\S+)(?:\\s(?:(?:\"(?<param>.*)\")|(?<param>\\S+)))*}", HelpText = "The regex for inserting a file.")]
    public string FileRegex { get; set; } = "";

    [Option("param", Default = "\\$\\[(?<param>\\d+)\\]", HelpText = "The regex for inserting a parameter.")]
    public string ParamRegex { get; set; } = "\\${(?<param>\\d)+}";

    [Option('m', "methods-dir", Default = null, HelpText = "A directory of globally useable method files.")]
    public string? Methods { get; set; } = null;
}