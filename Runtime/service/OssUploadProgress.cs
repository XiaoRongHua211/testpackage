using System;

namespace HumanSdk.Core.Service
{
    public sealed class OssUploadProgress
    {
        /// <summary>
        /// 当前已传输字节数
        /// </summary>
        public long TransferredBytes { get; private set; }

        /// <summary>
        /// 总字节数
        /// </summary>
        public long TotalBytes { get; private set; }

        /// <summary>
        /// 当前上传进度，范围 0~1
        /// </summary>
        public float Progress { get; private set; }

        /// <summary>
        /// 上传是否已完成
        /// </summary>
        public bool IsCompleted { get; private set; }

        internal OssUploadProgress Update(long transferredBytes, long totalBytes, bool isCompleted = false)
        {
            TransferredBytes = Math.Max(0, transferredBytes);
            TotalBytes = Math.Max(0, totalBytes);
            IsCompleted = isCompleted;

            if (isCompleted)
            {
                Progress = 1f;
            }
            else if (TotalBytes <= 0)
            {
                Progress = 0f;
            }
            else
            {
                Progress = Math.Max(0f, Math.Min(1f, (float)TransferredBytes / TotalBytes));
            }

            return this;
        }
    }
}
