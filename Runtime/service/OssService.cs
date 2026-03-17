// OssService.cs

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Aliyun.OSS;
using Aliyun.OSS.Common;
using Lwscope;
using Lwscope.Project;
using UnityEngine;

namespace HumanSdk.Core.Service
{
    public class OssService
    {
        private sealed class ResumableUploadProgressState
        {
            public int Finished;
            public long TransferredBytes;
            public long TotalBytes;
        }

        private readonly string _accessKeyId;
        private readonly string _accessKeySecret;
        private readonly string _endpoint;
        private readonly string _bucketName;
        private readonly string _regionId;

        private OssService(string accessKeyId, string accessKeySecret, string endpoint, string bucketName, string region)
        {
            _accessKeyId = accessKeyId ?? throw new ArgumentNullException(nameof(accessKeyId));
            _accessKeySecret = accessKeySecret ?? throw new ArgumentNullException(nameof(accessKeySecret));
            _endpoint = endpoint ?? throw new ArgumentNullException(nameof(endpoint));
            _bucketName = bucketName ?? throw new ArgumentNullException(nameof(bucketName));
            _regionId = region ?? throw new ArgumentNullException(nameof(region));
        }

        public static OssService CreateOssService(string region, string ak, string sk, bool accelerate)
        {
            string regionStr = region.EndsWith("-test") ? region.Substring(0, region.Length - 5) : region;
            // string endPoint = "oss-accelerate.aliyuncs.com";
            string endPoint = "oss-cn-shanghai.aliyuncs.com";
            string bucketName = "lw-human-case";
            if (region.Contains("test"))
            {
                bucketName = "lw-human-case-test";
                endPoint = "oss-cn-shanghai.aliyuncs.com";
            }
            else if (accelerate)
            {
                endPoint = "oss-accelerate.aliyuncs.com";
            }

            return new OssService(ak, sk, endPoint, bucketName, regionStr);
        }

        /// <summary>
        /// 上传本地文件到 OSS
        /// </summary>
        public async Task<string> UploadFileAsync(string localFilePath, string jobId, string fileName)
        {
            Console.WriteLine($"UploadFileAsync: start");
            if (!File.Exists(localFilePath))
                throw new FileNotFoundException("本地文件不存在", localFilePath);

            var client = new OssClient(_endpoint, _accessKeyId, _accessKeySecret);

            string objectKey = "cn-shanghai/video-capture/" + jobId + "/" + RandomStringGenerator.Generate(10) + "/" + fileName;
            Console.WriteLine($"object-key: {objectKey}");
            try
            {
                using var fileStream = File.OpenRead(localFilePath);
                await Task.Run(() => client.PutObject(_bucketName, objectKey, fileStream));
                return objectKey;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"OSS 上传失败: {ex.Message}", ex);
            }
        }

        public async Task<string> UploadFileAsync(Stream fileStream, string jobId, string fileName)
        {
            Console.WriteLine($"UploadFileAsync: start");
            if (fileStream == null)
                throw new ArgumentNullException(nameof(fileStream));

            if (string.IsNullOrWhiteSpace(fileName))
                throw new ArgumentException("文件名不能为空", nameof(fileName));

            var client = new OssClient(_endpoint, _accessKeyId, _accessKeySecret);

            string objectKey = this._regionId + "/" + jobId + "/" + RandomStringGenerator.Generate(10) + "/" + fileName;

            Console.WriteLine($"object-key: {objectKey}");
            try
            {
                Stream uploadStream = fileStream;

                if (!fileStream.CanSeek)
                {
                    var memoryStream = new MemoryStream();
                    await fileStream.CopyToAsync(memoryStream);
                    memoryStream.Position = 0;
                    uploadStream = memoryStream;
                }

                await Task.Run(() => { client.PutObject(_bucketName, objectKey, uploadStream); });
                return objectKey;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"OSS 上传失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 上传任务文件到 OSS
        /// </summary>
        /// <param name="localFilePath"></param>
        /// <param name="jobId"></param>
        /// <param name="fileName"></param>
        /// <param name="onSpeedKb">实时上传速度 KB/s</param>
        /// <param name="onDoneObjectKey">成功返回 objectKey</param>
        /// <param name="onError">失败回调</param>
        /// <returns></returns>
        public IEnumerator UploadFile(string localFilePath, string jobId, string fileName, Action<float> onSpeedKb, Action<string> onDoneObjectKey,
            Action<Exception> onError)
        {
            if (!File.Exists(localFilePath))
            {
                onError?.Invoke(new FileNotFoundException("本地文件不存在", localFilePath));
                yield break;
            }

            var client = new OssClient(_endpoint, _accessKeyId, _accessKeySecret);

            string objectKey = "cn-shanghai/video-capture/" + jobId + "/" + RandomStringGenerator.Generate(10) + "/" + fileName;

            bool finished = false;
            Exception exResult = null;

            new Thread(() =>
            {
                try
                {
                    using var fileStream = File.OpenRead(localFilePath);

                    var req = new PutObjectRequest(_bucketName, objectKey, fileStream);

                    var sw = System.Diagnostics.Stopwatch.StartNew();
                    long lastMs = 0;
                    long lastBytes = 0;

                    req.StreamTransferProgress += (sender, args) =>
                    {
                        long nowMs = sw.ElapsedMilliseconds;
                        if (lastMs == 0)
                        {
                            lastMs = nowMs;
                            lastBytes = args.TransferredBytes;
                            return;
                        }

                        long dtMs = nowMs - lastMs;
                        if (dtMs < 100) return;

                        long deltaBytes = args.TransferredBytes - lastBytes;
                        if (deltaBytes < 0) deltaBytes = 0;

                        float speedKb = (deltaBytes / 1024f) / (dtMs / 1000f);

                        lastMs = nowMs;
                        lastBytes = args.TransferredBytes;

                        UnityMainThreadDispatcher.Enqueue(() =>
                        {
                            Debug.Log($"skode1111: SpeedKB={speedKb}");
                            onSpeedKb?.Invoke(speedKb);
                        });
                    };

                    client.PutObject(req);
                }
                catch (OssException oe)
                {
                    exResult = oe;
                }
                catch (ClientException ce)
                {
                    exResult = ce;
                }
                catch (Exception e)
                {
                    exResult = e;
                }
                finally
                {
                    finished = true;
                    UnityMainThreadDispatcher.Enqueue(() => { onSpeedKb?.Invoke(0); });
                }
            }).Start();

            while (!finished)
            {
                yield return null;
            }

            if (exResult != null)
            {
                onError?.Invoke(exResult);
            }
            else
            {
                onDoneObjectKey?.Invoke(objectKey);
            }
        }

        /// <summary>
        /// 断点续传
        /// </summary>
        /// <param name="localFilePath"></param>
        /// <param name="jobId"></param>
        /// <param name="fileName"></param>
        /// <param name="onDoneObjectKey"></param>
        /// <param name="onError"></param>
        /// <param name="onProgressBytes">仅在上传进度快照发生变化时回调上传进度对象，包含当前已传输字节数、总字节数、当前上传进度；上传结束后会再回调一次最终值</param>
        public IEnumerator UploadFileResumable(string localFilePath, string jobId, string fileName, Action<string> onDoneObjectKey,
            Action<Exception> onError, Action<OssUploadProgress> onProgressBytes = null)
        {
            if (!File.Exists(localFilePath))
            {
                onError?.Invoke(new FileNotFoundException("本地文件不存在", localFilePath));
                yield break;
            }

            var client = new OssClient(_endpoint, _accessKeyId, _accessKeySecret);

            string objectKey = "cn-shanghai/video-capture/" + jobId + "/" + RandomStringGenerator.Generate(10) + "/" + fileName;

            Exception exResult = null;
            var progressState = new ResumableUploadProgressState
            {
                TotalBytes = new FileInfo(localFilePath).Length
            };
            var uploadProgress = new OssUploadProgress();
            bool hasReportedProgress = false;
            long lastReportedTransferredBytes = -1;
            long lastReportedTotalBytes = -1;
            bool lastReportedIsCompleted = false;

            void ReportProgress(long transferredBytes, long totalBytes, bool isCompleted = false)
            {
                if (hasReportedProgress &&
                    lastReportedTransferredBytes == transferredBytes &&
                    lastReportedTotalBytes == totalBytes &&
                    lastReportedIsCompleted == isCompleted)
                {
                    return;
                }

                hasReportedProgress = true;
                lastReportedTransferredBytes = transferredBytes;
                lastReportedTotalBytes = totalBytes;
                lastReportedIsCompleted = isCompleted;
                onProgressBytes?.Invoke(uploadProgress.Update(transferredBytes, totalBytes, isCompleted));
            }

            // 指定断点续传记录目录（官方示例使用 checkpointDir）
            string checkpointDir = Path.Combine(Application.persistentDataPath, "oss_checkpoint");
            if (Directory.Exists(checkpointDir) == false)
            {
                Directory.CreateDirectory(checkpointDir);
            }

            new Thread(() =>
            {
                try
                {
                    var request = new UploadObjectRequest(_bucketName, objectKey, localFilePath)
                    {
                        // 指定分片大小（最少 100 KB ~ 最大 5 GB）
                        PartSize = 10 * 1024 * 1024, // 10MB 推荐值

                        // 并行上传分片线程数
                        ParallelThreadCount = 3,

                        // 指定断点续传记录目录
                        CheckpointDir = checkpointDir
                    };

                    // 进度回调
                    request.StreamTransferProgress += (_, args) =>
                    {
                        long transferredBytes = Math.Max(0, args.TransferredBytes);
                        long totalBytes = args.TotalBytes > 0 ? args.TotalBytes : Interlocked.Read(ref progressState.TotalBytes);
                        Interlocked.Exchange(ref progressState.TransferredBytes, transferredBytes);
                        Interlocked.Exchange(ref progressState.TotalBytes, totalBytes);
                    };

                    // 调用官方断点续传接口
                    client.ResumableUploadObject(request);
                }
                catch (OssException oe)
                {
                    exResult = oe;
                }
                catch (ClientException ce)
                {
                    exResult = ce;
                }
                catch (Exception e)
                {
                    exResult = e;
                }
                finally
                {
                    Interlocked.Exchange(ref progressState.Finished, 1);
                }
            }).Start();

            while (Interlocked.CompareExchange(ref progressState.Finished, 0, 0) == 0)
            {
                ReportProgress(Interlocked.Read(ref progressState.TransferredBytes), Interlocked.Read(ref progressState.TotalBytes));
                yield return null;
            }

            long finalTotalBytes = Interlocked.Read(ref progressState.TotalBytes);
            long finalTransferredBytes = Interlocked.Read(ref progressState.TransferredBytes);
            bool isUploadCompleted = exResult == null;
            if (isUploadCompleted)
            {
                finalTransferredBytes = finalTotalBytes;
                Interlocked.Exchange(ref progressState.TransferredBytes, finalTransferredBytes);
            }
            
            ReportProgress(finalTransferredBytes, finalTotalBytes, isUploadCompleted);

            if (exResult != null)
                onError?.Invoke(exResult);
            else
                onDoneObjectKey?.Invoke(objectKey);
        }

        /// <summary>
        /// 上传日志文件到 OSS
        /// </summary>
        /// <param name="localFilePath"></param>
        /// <param name="onSucess">成功返回 localFilePath</param>
        /// <param name="onError">失败回调</param>
        /// <returns></returns>
        public IEnumerator UploadLogFile(string localFilePath, Action<string> onSucess, Action<string, Exception> onError)
        {
            if (!File.Exists(localFilePath))
            {
                onError?.Invoke(localFilePath, new FileNotFoundException("本地文件不存在", localFilePath));
                yield break;
            }

            var client = new OssClient(_endpoint, _accessKeyId, _accessKeySecret);

            var fileName = Path.GetFileName(localFilePath);
            if (TryParseDate(localFilePath, out string fileDate) == false)
            {
                onError?.Invoke(localFilePath, new FileNotFoundException($"解析日期失败：{localFilePath}", localFilePath));
                yield break;
            }

            if (string.IsNullOrEmpty(RuntimeData.Sn) && Application.isEditor == false && Application.platform == RuntimePlatform.Android)
            {
                onError?.Invoke(localFilePath, new Exception("获取设备SN失败为空！"));
                yield break;
            }

            string ossPath = $"Logs/SN-{RuntimeData.Sn}/{fileDate}/{fileName}";

            bool finished = false;
            Exception exResult = null;

            new Thread(() =>
            {
                try
                {
                    using var fileStream = File.OpenRead(localFilePath);
                    var req = new PutObjectRequest(_bucketName, ossPath, fileStream);
                    client.PutObject(req);
                }
                catch (Exception e)
                {
                    exResult = e;
                }
                finally
                {
                    finished = true;
                }
            }).Start();

            while (!finished)
            {
                yield return null;
            }

            if (exResult != null)
            {
                onError?.Invoke(localFilePath, exResult);
            }
            else
            {
                onSucess?.Invoke(localFilePath);
            }
        }

        /// <summary>
        /// 断点续传
        /// </summary>
        /// <param name="localFilePath"></param>
        /// <param name="onSuccess"></param>
        /// <param name="onError"></param>
        /// <returns></returns>
        public IEnumerator UploadLogFileResumable(string localFilePath, Action<string> onSuccess, Action<string, Exception> onError)
        {
            if (!File.Exists(localFilePath))
            {
                onError?.Invoke(localFilePath,
                    new FileNotFoundException("本地文件不存在", localFilePath));
                yield break;
            }

            var client = new OssClient(_endpoint, _accessKeyId, _accessKeySecret);

            var fileName = Path.GetFileName(localFilePath);

            if (!TryParseDate(localFilePath, out string fileDate))
            {
                onError?.Invoke(localFilePath,
                    new Exception($"解析日期失败：{localFilePath}"));
                yield break;
            }

            if (string.IsNullOrEmpty(RuntimeData.Sn) &&
                Application.isEditor == false &&
                Application.platform == RuntimePlatform.Android)
            {
                onError?.Invoke(localFilePath,
                    new Exception("获取设备SN失败为空！"));
                yield break;
            }

            string objectKey = $"Logs/SN-{RuntimeData.Sn}/{fileDate}/{fileName}";

            bool finished = false;
            Exception exResult = null;

            // ⭐ checkpoint 目录（持久化）
            string checkpointDir = Path.Combine(Application.persistentDataPath, "oss_checkpoint");
            if (Directory.Exists(checkpointDir) == false)
            {
                Directory.CreateDirectory(checkpointDir);
            }

            new Thread(() =>
            {
                try
                {
                    // ⭐ 官方断点续传请求
                    var request = new UploadObjectRequest(_bucketName, objectKey, localFilePath)
                    {
                        // 分片大小（日志文件一般不大，可设小一点）
                        PartSize = 10 * 1024 * 1024, // 10MB

                        // 并行线程数
                        ParallelThreadCount = 3,

                        // 启用断点续传
                        CheckpointDir = checkpointDir
                    };

                    // 可选：进度回调（日志一般不需要）
                    request.StreamTransferProgress += (sender, args) =>
                    {
                        // 这里只做简单日志，不计算速度
                        // Debug.Log($"Log Upload: {args.TransferredBytes}/{args.TotalBytes}");
                    };

                    // ⭐ 调用官方断点续传接口
                    client.ResumableUploadObject(request);
                }
                catch (Exception e)
                {
                    exResult = e;
                }
                finally
                {
                    finished = true;
                }
            }).Start();

            while (!finished)
            {
                yield return null;
            }

            if (exResult != null)
            {
                onError?.Invoke(localFilePath, exResult);
            }
            else
            {
                onSuccess?.Invoke(localFilePath);
            }
        }

        /// <summary>
        /// 获取 Bucket 指定目录(prefix) 下的所有文件
        /// 例如 prefix="cn-shanghai/video-capture/job123/"
        /// 返回示例：Logs/SN-/2025-12-30/app_20251230.log
        /// </summary>
        public List<string> GetFileList(string prefix)
        {
            List<string> nameList = new List<string>();
            var client = new OssClient(_endpoint, _accessKeyId, _accessKeySecret);
            var request = new ListObjectsRequest(Server_Oss.Bucket)
            {
                Prefix = prefix
            };

            ObjectListing listing = client.ListObjects(request);
            foreach (var item in listing.ObjectSummaries)
            {
                nameList.Add(item.Key);
            }

            return nameList;
        }

        /// <summary>
        /// 从日志文件名中提取日期，并转成 YYYY-MM-DD
        /// 支持：
        /// app_20251230.log
        /// app_20251230_001.log
        /// </summary>
        private static bool TryParseDate(string fileName, out string formattedDate)
        {
            formattedDate = null;

            var match = Regex.Match(fileName, @"(\d{8})");
            if (!match.Success)
                return false;

            string raw = match.Groups[1].Value;
            if (!DateTime.TryParseExact(raw, "yyyyMMdd", null, System.Globalization.DateTimeStyles.None, out DateTime dt))
            {
                return false;
            }

            formattedDate = dt.ToString("yyyy-MM-dd");
            return true;
        }
    }
}