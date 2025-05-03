using System;
using System.Text;
using System.IO;

internal static class JsonUnicodeConverter
{
    private static void Main()
    {
        Console.WriteLine("输入JSON文件所在目录路径：");
        var sourcePath = Console.ReadLine();

        if (!Directory.Exists(sourcePath))
        {
            Console.WriteLine("目录不存在！");
            return;
        }

        var outputPath = Path.Combine(sourcePath, "UnicodeJSON");
        Directory.CreateDirectory(outputPath);

        var jsonFiles = Directory.GetFiles(sourcePath, "*.json", SearchOption.TopDirectoryOnly);

        foreach (var filePath in jsonFiles)
        {
            try
            {
                var originalContent = File.ReadAllText(filePath, Encoding.UTF8);
                
                var convertedContent = new StringBuilder();
                foreach (var c in originalContent)
                {
                    if (c > 0x7F) // 非ASCII字符
                    {
                        convertedContent.Append($"\\u{(int)c:X4}");
                    }
                    else
                    {
                        convertedContent.Append(c);
                    }
                }
                
                var outputFilePath = Path.Combine(
                    outputPath,
                    Path.GetFileName(filePath)
                );
                
                File.WriteAllText(outputFilePath, convertedContent.ToString(), new UTF8Encoding(false));

                Console.WriteLine($"已转换：{Path.GetFileName(filePath)}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"处理 {Path.GetFileName(filePath)} 时出错：{ex.Message}");
            }
        }

        Console.WriteLine("\nJSON文件转换完成！输出目录：" + outputPath);
        Console.ReadLine();
    }
}