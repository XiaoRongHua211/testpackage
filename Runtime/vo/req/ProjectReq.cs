using System;
using System.Collections.Generic;


namespace HumanSDK.VO.Req
{
  public class ProjectReq
    {
        public Int32 page { get; set; }
    
        public Int32 pageSize { get; set; }

        public SearchRequest searchRequest = new SearchRequest();

        // 构造函数
        public ProjectReq(Int32 page, Int32 pageSize)
        {
            this.page = page;
            this.pageSize = pageSize;
        }
    }  

  public class SearchRequest
    {
        public string projectCategory { get; set; } =  string.Empty;

       public SearchRequest()
       {
        this.projectCategory = "human_data";
       }
    }
}