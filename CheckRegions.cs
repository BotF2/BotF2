// PSEUDOCODE / PLAN:
// - This small console tool scans a C# source file and matches #region / #endregion directives.
// - For each line in the file:
//     - If line contains a #region directive, push an entry (name, lineNumber) onto a stack.
//     - If line contains a #endregion directive, pop the stack if not empty.
//         - If stack empty when #endregion encountered -> report an unmatched #endregion with the line number.
//         - If popped region name is present, report a matched pair (optional).
// - At end of file:
//     - If stack not empty -> report each remaining unmatched #region with its name and line number.
// - Exit with code 0 if everything matched, non-zero otherwise.
// - This helps find the missing #endregion causing CS1038 so you can open the file and insert the missing directive
//   at or near the reported location.
//
// USAGE:
//   dotnet run --project Tools (or compile and run the .cs file)
//   Or: csc CheckRegions.cs && CheckRegions.exe <path-to-file>
//   If no path provided, tool will prompt for one.
//
// The tool only analyzes raw directives; it does not parse conditional compilation or string/comment exclusions.
// Use it to locate where a #region was left open or where an extra #endregion exists,
// then fix the C# file by adding/removing the matching directive accordingly.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

class CheckRegions
{
    static int Main(string[] args)
    {
        string path;
        if (args.Length >= 1)
        {
            path = args[0];
        }
        else
        {
            Console.Write("Path to C# file to check: ");
            path = Console.ReadLine()?.Trim().Trim('"') ?? "";
        }

        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
        {
            Console.Error.WriteLine("File not found. Provide a valid path to a C# source file.");
            return 2;
        }

        try
        {
            var lines = File.ReadAllLines(path);
            var regionStack = new Stack<(string name, int line)>();
            var unmatchedEndRegions = new List<int>();
            var regionRegex = new Regex(@"^\s*#\s*region\b(.*)$", RegexOptions.Compiled);
            var endRegionRegex = new Regex(@"^\s*#\s*endregion\b(.*)$", RegexOptions.Compiled);

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];

                // naive skip of lines in string literals is not implemented; this handles most common cases.
                var mRegion = regionRegex.Match(line);
                if (mRegion.Success)
                {
                    string name = mRegion.Groups[1].Value.Trim();
                    regionStack.Push((name, i + 1));
                    continue;
                }

                var mEnd = endRegionRegex.Match(line);
                if (mEnd.Success)
                {
                    if (regionStack.Count == 0)
                    {
                        unmatchedEndRegions.Add(i + 1);
                    }
                    else
                    {
                        regionStack.Pop();
                    }
                }
            }

            bool ok = true;

            if (unmatchedEndRegions.Count > 0)
            {
                ok = false;
                Console.WriteLine("Found #endregion without matching #region at lines:");
                foreach (var ln in unmatchedEndRegions)
                {
                    Console.WriteLine("  - line " + ln);
                }
            }

            if (regionStack.Count > 0)
            {
                ok = false;
                Console.WriteLine("Found unclosed #region(s) (missing #endregion). Open regions:");
                foreach (var item in regionStack.Reverse())
                {
                    var displayName = string.IsNullOrEmpty(item.name) ? "<no name>" : item.name;
                    Console.WriteLine($"  - \"{displayName}\" opened at line {item.line}");
                }
            }

            if (ok)
            {
                Console.WriteLine("All #region / #endregion directives are balanced in: " + path);
                return 0;
            }
            else
            {
                Console.WriteLine();
                Console.WriteLine("To fix CS1038 (#endregion directive expected):");
                Console.WriteLine(" - Open the file at the listed line(s) and add the missing '#endregion' for each unmatched '#region'.");
                Console.WriteLine(" - If you see '#endregion' listed as unmatched, remove the extra one or ensure a matching '#region' exists.");
                return 1;
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("Error while checking file: " + ex.Message);
            return 3;
        }
    }
}