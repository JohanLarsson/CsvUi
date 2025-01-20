using System.Collections.Immutable;

namespace CsvUi;

public static class Csv
{
    public static ImmutableArray<ImmutableArray<string>> Read(ReadOnlySpan<char> csv, char separator)
    {
        var builder = ImmutableArray.CreateBuilder<ImmutableArray<string>>();
        var headers = ReadFields(ref csv, separator);
        builder.Add(headers);
        while (!csv.IsEmpty)
        {
            var row = ReadFields(ref csv, separator);
            if (row.Length == 0)
            {
                continue;
            }

            if (row.Length != headers.Length)
            {
                throw new FormatException($"expected {headers.Length} values, was {row.Length} at row {builder.Count + 1}");
            }

            builder.Add(row);
        }

        return builder.ToImmutable();
    }

    private static ImmutableArray<string> ReadFields(ref ReadOnlySpan<char> csv, char separator)
    {
        if (ReadLine(ref csv) is { IsEmpty: false } line)
        {
            var builder = ImmutableArray.CreateBuilder<string>();
            var e = line.Split(separator);
            while (e.MoveNext())
            {
                builder.Add(line[e.Current].Trim('"').ToString());
            }

            return builder.ToImmutable();
        }

        return [];
    }

    private static ReadOnlySpan<char> ReadLine(ref ReadOnlySpan<char> csv)
    {
        if (csv.IsEmpty)
        {
            return [];
        }

        for (var i = 0; i < csv.Length; i++)
        {
            switch (csv[i])
            {
                case '\r'
                    when i < csv.Length - 1 &&
                         csv[i + 1] == '\n':
                    {
                        var line = csv[..i];
                        csv = csv[(i + 2)..];
                        return line;
                    }

                case '\r' or '\n':
                    {
                        var line = csv[..i];
                        csv = csv[(i + 1)..];
                        return line;
                    }
            }
        }

        var temp = csv;
        csv = [];
        return temp;
    }
}
