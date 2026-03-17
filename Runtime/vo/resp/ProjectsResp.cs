using System;
using System.Collections.Generic;

namespace HumanSDK.VO.Resp
{
    public class ProjectsResp
    {
        public List<ProjectItem> Data { get; set; } = new List<ProjectItem>();
    }

    public class ProjectItem
    {
        public string uuid { get; set; } = string.Empty;
        public string name { get; set; } = string.Empty;
    }
}