using System.Runtime.CompilerServices;
using CommandLine;

[assembly: InternalsVisibleTo("FileCompiler.Tests")]
namespace FileCompiler;
internal static class Program
{
    static void Main(string[] args)
    {
        Parser.Default.ParseArguments<Options>(args)
            .WithParsed(FileCompiler.Run);
    }
}