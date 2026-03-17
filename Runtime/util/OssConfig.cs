public class AppSettings
{
    public OssConfig OssConfig { get; set; }
}

public class OssConfig
{
    public string AccessKeyId { get; set; } = string.Empty;
    public string AccessKeySecret { get; set; } = string.Empty;
    public string Endpoint { get; set; } = string.Empty;
    public string BucketName { get; set; } = string.Empty;
}