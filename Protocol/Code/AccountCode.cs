using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Protocol.Code
{
    /// <summary>
    /// 账户的子操作码
    /// </summary>
    public class AccountCode
    {
        public const int Register_CREQ = 0;
        public const int Register_SRES = 1;
        public const int Login = 2;
    }
}
