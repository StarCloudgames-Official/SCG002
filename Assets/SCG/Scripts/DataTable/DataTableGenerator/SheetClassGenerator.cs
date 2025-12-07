using System.IO;
using System.Text;
using UnityEngine;

public static class SheetClassGenerator
{
    public static void GenerateClass(string className, string[] fieldNames, string[] fieldTypes, string outputPath)
    {
        var sb = new StringBuilder();
        sb.AppendLine("using System;");
        sb.AppendLine("using System.Collections.Generic;");
        sb.AppendLine("using UnityEngine;");
        sb.AppendLine("using MemoryPack;");
        sb.AppendLine();
        sb.AppendLine("[MemoryPackable]");
        sb.AppendLine($"public partial class {className}");
        sb.AppendLine("{");

        for (int i = 0; i < fieldNames.Length; i++)
        {
            if (fieldNames[i].Contains("#")) continue;
            string type = ConvertType(fieldTypes[i]);
            sb.AppendLine($"    public {type} {fieldNames[i]};");
        }

        sb.AppendLine("}");

        if (!Directory.Exists(outputPath))
            Directory.CreateDirectory(outputPath);

        var path = Path.Combine(outputPath, className + ".cs");
        File.WriteAllText(path, sb.ToString());
        var logPath = path.Replace('\\', '/');
        Debug.Log("[ClassGen] Generated class: " + logPath);
    }

    private static string ConvertType(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return "string";
        raw = raw.Trim();

        if (raw.StartsWith("enum:", System.StringComparison.OrdinalIgnoreCase))
        {
            string enumName = raw.Substring("enum:".Length).Trim();
            if (string.IsNullOrEmpty(enumName)) return "int";
            return $"DataTableEnum.{enumName}";
        }

        switch (raw)
        {
            case "int": return "int";
            case "long": return "long";
            case "float": return "float";
            case "double": return "double";
            case "bool": return "bool";
            case "string": return "string";
            case "int[]": return "int[]";
            case "long[]": return "long[]";
            case "float[]": return "float[]";
            case "double[]": return "double[]";
            default: return raw;
        }
    }
}
