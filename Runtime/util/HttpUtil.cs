// HttpUtil.cs
using Newtonsoft.Json;
using System.Text;
using System.Net.Http;
using System;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;



public static class HttpUtil
{
    private static readonly HttpClient _httpClient = new HttpClient();


    public static TimeSpan Timeout
    {
        get => _httpClient.Timeout;
        set => _httpClient.Timeout = value;
    }

    /// <summary>
    /// 添加全局默认请求头（如 Authorization、User-Agent 等）
    /// </summary>
    public static void AddDefaultHeader(string name, string value)
    {
        _httpClient.DefaultRequestHeaders.Add(name, value);
    }

    // ================ GET ================

    /// <summary>
    /// 发送 GET 请求，返回反序列化后的对象
    /// </summary>
    public static async Task<T> GetAsync<T>(string url, CancellationToken ct = default)
    {
        try
        {
            // return await _httpClient.GetFromJsonAsync<T>(url, ct);
            var response = await _httpClient.GetAsync(url, ct);
            response.EnsureSuccessStatusCode();
            var jsonString = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<T>(jsonString);
            return result;
        }
        catch (Exception ex)
        {
            throw new HttpRequestException($"GET 请求失败: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 发送 GET 请求，返回原始字符串
    /// </summary>
    public static async Task<string> GetStringAsync(string url, CancellationToken ct = default)
    {
        try
        {

            var response = await _httpClient.GetAsync(url, ct);
            response.EnsureSuccessStatusCode();
            var jsonString = await response.Content.ReadAsStringAsync();
            return jsonString;
        }
        catch (Exception ex)
        {
            throw new HttpRequestException($"GET 请求失败: {ex.Message}", ex);
        }
    }

    // ================ POST ================

    /// <summary>
    /// 发送 JSON POST 请求，自动序列化请求体，返回反序列化响应
    /// </summary>
    public static async Task<TResponse> PostJsonAsync<TRequest, TResponse>(
        string url,
        TRequest data,
        Dictionary<string, string> headers = null,
        CancellationToken ct = default)
    {
        using var request = CreateJsonRequest(HttpMethod.Post, url, data, headers);
        using var response = await _httpClient.SendAsync(request, ct);
        await EnsureSuccessAsync(response);
        var jsonString = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<TResponse>(jsonString);
        return result;
    }

    /// <summary>
    /// 发送 JSON POST 请求，返回原始字符串响应
    /// </summary>
    public static async Task<string> PostJsonAsStringAsync<TRequest>(
        string url,
        TRequest data,
        Dictionary<string, string> headers = null,
        CancellationToken ct = default)
    {
        using var request = CreateJsonRequest(HttpMethod.Post, url, data, headers);
        using var response = await _httpClient.SendAsync(request, ct);
        await EnsureSuccessAsync(response);
        var jsonString = await response.Content.ReadAsStringAsync();
        return jsonString;
    }

    // ================ PUT ================

    public static async Task<TResponse> PutJsonAsync<TRequest, TResponse>(
        string url,
        TRequest data,
        Dictionary<string, string> headers = null,
        CancellationToken ct = default)
    {
        using var request = CreateJsonRequest(HttpMethod.Put, url, data, headers);
        using var response = await _httpClient.SendAsync(request, ct);
        await EnsureSuccessAsync(response);
        var jsonString = await response.Content.ReadAsStringAsync();
        var result = JsonConvert.DeserializeObject<TResponse>(jsonString);
        return result;
    }

    // ================ DELETE ================

    public static async Task DeleteAsync(string url, CancellationToken ct = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Delete, url);
        using var response = await _httpClient.SendAsync(request, ct);
        await EnsureSuccessAsync(response);
    }

    // ================ 内部辅助方法 ================

    private static HttpRequestMessage CreateJsonRequest<T>(
        HttpMethod method,
        string url,
        T data,
        Dictionary<string, string> headers)
    {
        string json = JsonConvert.SerializeObject(data);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var request = new HttpRequestMessage(method, url) { Content = content };

        if (headers != null)
        {
            foreach (var (key, value) in headers)
                request.Headers.Add(key, value);
        }

        return request;
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response)
    {
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException(
                $"HTTP {response.StatusCode}: {response.ReasonPhrase}. Response: {errorContent}");
        }
    }
}