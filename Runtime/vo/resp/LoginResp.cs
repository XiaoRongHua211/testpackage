
using System;
using System.Collections.Generic;

namespace HumanSDK.VO.Resp
{
    public class LoginResp
    {
        public string token { get; set; }
        public string uuid { get; set; }
        public bool isAdmin { get; set; }
        public string refreshToken { get; set; }

        // 仅用于反序列化或构造
        public LoginResp(
            string token,
            string uuid,
            bool isAdmin,
            string refreshToken)
        {
            this.token = token ?? throw new ArgumentNullException(nameof(token));
            this.uuid = uuid ?? throw new ArgumentNullException(nameof(uuid));
            this.isAdmin = isAdmin;
            this.refreshToken = refreshToken ?? throw new ArgumentNullException(nameof(refreshToken));
        }
        public LoginResp() { }
    }
}

