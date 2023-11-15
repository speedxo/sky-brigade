using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Horizon.Content.Processors;

/// <summary>
/// Helper class to pre-process shader source, adding support for some preprocessor directives.
/// Currently #include directives are implemented, with caching; as well as special #import shorthands such as fastRandom for convenient one-liners.
/// </summary>
/// <seealso cref="System.IDisposable" />
public class ShaderDirectiveProcessor : IDisposable
{
    private Dictionary<string, Func<string, string, string>> _directiveBindings = new();
    public Dictionary<string, string> fileSources = new();
    private bool disposedValue;

    /// <summary>
    /// Initializes a new instance of the <see cref="ShaderDirectiveProcessor"/> class.
    /// </summary>
    public ShaderDirectiveProcessor()
    {
        /* Bind our source generators. */
        _directiveBindings.Add("include", IncludeDirectiveProcessor);
        _directiveBindings.Add("import", ImportDirectiveProcessor);
    }

    /// <summary>
    /// handles special convenient imports for commonly used code.
    /// </summary>
    /// <param name="arg">The argument.</param>
    /// <returns>The code snippet as request, string.Empty if failed.</returns>
    private string ImportDirectiveProcessor(string file, string arg)
    {
        return arg switch
        {
            "fastRandom"
                => @"
float rand(vec2 co){
    return fract(sin(dot(co, vec2(12.9898, 78.233))) * 43758.5453);
}",
            _ => string.Empty
        };
    }

    /// <summary>
    /// Handles the include C directive.
    /// </summary>
    /// <param name="arg">The file to include.</param>
    /// <returns></returns>
    /// <exception cref="System.IO.FileNotFoundException">Referenced include directive file {arg} not found!</exception>
    private string IncludeDirectiveProcessor(string file, string arg)
    {
        if (file.CompareTo(string.Empty) == 0)
            return string.Empty;

        var path = Path.GetDirectoryName(file) ?? string.Empty;
        var target = Path.Combine(path, arg);

        if (path is null || path.CompareTo(string.Empty) == 0 || !File.Exists(target))
        {
            // TODO: implement logging and error handling.
            throw new FileNotFoundException(
                $"Referenced include directive file {arg} not found!"
            );
        }

        // basic caching implementation
        if (!fileSources.ContainsKey(arg))
            fileSources.Add(arg, File.ReadAllText(target).Trim());

        return fileSources[arg];
    }

    /// <summary>Scans any input files for supported directives and process them accordingly. </summary>
    /// <param name="file"></param>
    /// <exception cref="FileNotFoundException" />
    public string ProcessFile(in string file)
    {
        if (!File.Exists(file))
        {
            /* TODO: implement logging and error handling. */
            throw new FileNotFoundException(
                $"Input file {file} not found! Please supply a valid shader source file."
            );
        }

        return ProcessSource(file, File.ReadAllLines(file));
    }

    /// <summary>Scans any input files for supported directives and process them accordingly. </summary>
    /// <param name="file">The file.</param>
    /// <param name="lines">The lines.</param>
    /// <returns></returns>
    public string ProcessSource(in string file, in IEnumerable<string> lines)
    {
        /* These are _fast_ with strings. */
        StringBuilder source = new StringBuilder();

        /* Iterate over the source. */
        foreach (var rawLine in lines)
        {
            /* We can do this because it will store a loose ref to rawLine,
            /* the value would only be copied if we modify the string, which we dont.
            */
            string line = rawLine;

            foreach (var (identifier, generator) in _directiveBindings)
            {
                if (rawLine.StartsWith($"#{identifier}"))
                {
                    int q0 = rawLine.IndexOf('"') + 1;
                    int q1 = rawLine.LastIndexOf('"');
                    string argument = rawLine.Substring(q0, q1 - q0);

                    line = generator.Invoke(file, argument);
                }
            }
            source.AppendLine(line);
        }
        return source.ToString();
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                // TODO: dispose managed state (managed objects)
                fileSources.Clear();
            }

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null
            disposedValue = true;
        }
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}