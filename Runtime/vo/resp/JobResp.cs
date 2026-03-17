using System;
using System.Collections.Generic;
using System.Collections.Generic;
using System.Data;
using System.IO;
using Aliyun.OSS;

namespace HumanSDK.VO.Resp
{
    public class JobResp
    {
        public List<DataItem> Data { get; set; } = new List<DataItem>();
        public int taskTotal { get; set; }
        public int taskPending { get; set; }
    }

    public class DataItem
    {
        public string Id { get; set; } = string.Empty; //全局唯一，不同表、不同用户之间
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;//任务类型，例如视频采集/图片采集等
        public QrCode qrCode { get; set; } = new QrCode();
        public HumanCase humanCase { get; set; } = new HumanCase();
    }

    public class QrCode
    {
        public string VersionUuid { get; set; } = string.Empty;
        public string FileUrl { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
    }

    public class HumanCase
    {
        public string id { get; set; } = string.Empty;
        public string name { get; set; } = string.Empty;
        public string taskName { get; set; } = string.Empty;
        public string description { get; set; } = string.Empty;
    }
}