using System;
using System.Collections.Generic;

namespace HumanSDK.VO.Resp
{
    public class CompleteJobResp
    {
    }

    public class CompleteProduceingJobResp
    {
        public String nextJobId { get; set; } = string.Empty;

        public String nextJobName { get; set; } = string.Empty;
    }
}