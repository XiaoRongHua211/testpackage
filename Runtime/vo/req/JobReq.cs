
using System;
using System.Collections.Generic;

namespace HumanSDK.VO.Req
{
  public class JobReq
  {
    public string projectUuid { get; set; }
    public string jobType { get; set; }
    public int taskCount { get; set; }

    public string batchName { get; set; }

    public string taskName { get; set; }

    public bool needQrCode { get; set; }

    public bool preferUnfinished { get; set; }

    public bool needHumanCase { get; set; }
  }
}
