
using System;
using System.Collections.Generic;

namespace HumanSDK.VO.Req
{
  public class ListHumanTaskReq
  {
    public string projectUuid { get; set; }

    public int page { get; set; }
    public int pageSize { get; set; }
  }
}
