

using System;
using System.Collections.Generic;

namespace HumanSDK.VO.Req
{
  public class LoginReq
    {
        // 自动属性（带 getter 和 setter）
        public string username { get; set; }
        public string password { get; set; }

        // 构造函数
        public LoginReq(string username, string password)
        {
            this.username = username;
            this.password = password;
        }
    }  
}
