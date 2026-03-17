using System;
using System.Text;

public static class RandomStringGenerator
{
    // 使用 [ThreadStatic] 实现线程安全（.NET Standard 2.1 支持）
    [ThreadStatic]
    private static Random _random;
    
    // 默认字符集：数字 + 大小写字母
    private const string DefaultCharset = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

    /// <summary>
    /// 生成指定长度的随机字符串
    /// </summary>
    public static string Generate(int length, string charset = null)
    {
        if (length <= 0)
            throw new ArgumentOutOfRangeException(nameof(length), "长度必须大于0");
        
        charset ??= DefaultCharset;
        if (string.IsNullOrEmpty(charset))
            throw new ArgumentException("字符集不能为空", nameof(charset));
        
        // 获取当前线程的 Random 实例
        var random = _random ??= new Random();
        
        var buffer = new char[length];
        var charsetLength = charset.Length;
        
        for (int i = 0; i < length; i++)
        {
            buffer[i] = charset[random.Next(charsetLength)];
        }
        
        return new string(buffer);
    }
}