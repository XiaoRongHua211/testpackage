using System;
using System.Collections.Generic;

namespace HumanSDK.VO.Req
{
    public class CompleteJobReq
    {
        public string jobId { get; set; } = string.Empty;

        public bool passed { get; set; }

        public string region { get; set; } = string.Empty;

        public float videoSeconds { get; set; }

        public float videoFps { get; set; }

        public string env_num { get; set; }
        public string scene_num { get; set; }

        public List<HumanFileCreate> humanFileCreates { get; set; } = new List<HumanFileCreate>();
    }


    public class HumanFileCreate
    {
        public string fileName { get; set; } = string.Empty;

        public string filePath { get; set; } = string.Empty;

        public string fileType { get; set; } = string.Empty;

        public Int32 fileSize { get; set; } = 0;

        public int bucketType { get; set; } = 0;

        public string bucketName { get; set; } = string.Empty;
    }
}