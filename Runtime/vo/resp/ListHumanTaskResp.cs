
using System;
using System.Collections.Generic;

namespace HumanSDK.VO.Req
{
  public class ListHumanTaskResp
  {
    public List<HumanTaskItem> data { get; set; } = new List<HumanTaskItem>();
  }

  public class HumanTaskItem
  {
    public string name { get; set; } = string.Empty;
    public string description { get; set; } = string.Empty;
    public int totalNum { get; set; }
    public int pendingNum { get; set; }

    public string type { get; set; } = string.Empty;

     public Dictionary<string, string> metadata { get; set; } = new Dictionary<string, string>();
  }
}
