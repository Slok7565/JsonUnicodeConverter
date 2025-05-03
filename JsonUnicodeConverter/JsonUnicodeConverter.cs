using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;

namespace JsonUnicodeConverter;

internal static class JsonCryptoProcessor
{
    private enum ProcessMode
    {
        Encrypt,
        Decrypt 
    }

    private static void Main()
    {
        Console.WriteLine("请选择模式 (E=加密, D=解密):");
        var modeInput = Console.ReadLine()?.Trim().ToUpper();
        
        var mode = modeInput == "D" ? ProcessMode.Decrypt : ProcessMode.Encrypt;
        var modeName = mode == ProcessMode.Encrypt ? "加密" : "解密";

        Console.WriteLine($"\n请输入要{modeName}的JSON目录路径：");
        var sourcePath = Console.ReadLine();

        if (!Directory.Exists(sourcePath))
        {
            Console.WriteLine("目录不存在！");
            return;
        }
        
        var outputPath = Path.Combine(sourcePath, 
            mode == ProcessMode.Encrypt ? "EncryptedJSON" : "DecryptedJSON");
        Directory.CreateDirectory(outputPath);
        
        var jsonFiles = Directory.GetFiles(sourcePath, "*.json");
        foreach (var filePath in jsonFiles)
        {
            try
            {
                var json = JObject.Parse(File.ReadAllText(filePath));
                ProcessJson(json, mode);
                
                var outputFile = Path.Combine(outputPath, Path.GetFileName(filePath));
                File.WriteAllText(outputFile, json.ToString(), new UTF8Encoding(false));
                
                Console.WriteLine($"已{modeName}：{Path.GetFileName(filePath)}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"处理失败 [{Path.GetFileName(filePath)}]: {ex.Message}");
            }
        }

        Console.WriteLine($"\n{modeName}完成！输出目录：{outputPath}");
        Console.ReadLine();
    }

    private static void ProcessJson(JToken token, ProcessMode mode)
    {
        switch (token.Type)
        {
            case JTokenType.Object:
                foreach (var prop in token.Children<JProperty>().ToList())
                {
                    ProcessJson(prop.Value, mode); // 保持键名不变
                }
                
                break;
            case JTokenType.Array:
                foreach (var child in token.Children().ToList())
                {
                    ProcessJson(child, mode);
                }
                
                break;
            case JTokenType.String:
                var currentValue = token.Value<string>();
                if (currentValue != null)
                {
                    var processedValue = mode == ProcessMode.Encrypt 
                        ? EncryptValue(currentValue) 
                        : DecryptValue(currentValue);
                    token.Replace(processedValue);
                }

                break;
        }
    }

    #region 加密
 
    private static string EncryptValue(string plainText)
    {
        var unicodeStr = ConvertToUnicodeEscape(plainText);
        return Base64Encode(unicodeStr);
    }

    private static string ConvertToUnicodeEscape(string input)
    {
        var sb = new StringBuilder();
        foreach (var c in input)
        {
            sb.Append(c > 0x7F ? $"\\u{(int)c:X4}" : c.ToString());
        }
        
        return sb.ToString();
    }

    private static string Base64Encode(string plainText)
    {
        var bytes = Encoding.UTF8.GetBytes(plainText);
        return Convert.ToBase64String(bytes);
    }
    
    #endregion

    #region 解密
    
    private static string DecryptValue(string cipherText)
    {
        try
        {
            var base64Decoded = Base64Decode(cipherText);
            return ParseUnicodeEscapes(base64Decoded);
        }
        catch
        {
            return cipherText;
        }
    }

    private static string Base64Decode(string cipherText)
    {
        var bytes = Convert.FromBase64String(cipherText);
        return Encoding.UTF8.GetString(bytes);
    }

    private static string ParseUnicodeEscapes(string input)
    {
        return Regex.Replace(input, @"\\u([0-9A-Fa-f]{4})", match =>
        {
            try
            {
                return ((char)Convert.ToInt32(match.Groups[1].Value, 16)).ToString();
            }
            catch
            {
                return match.Value;
            }
        });
    }
  
    #endregion
}