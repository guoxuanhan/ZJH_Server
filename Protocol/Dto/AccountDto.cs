﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Protocol.Dto
{
    /// <summary>
    /// 账号传输模型
    /// </summary>
    [Serializable]
    public class AccountDto
    {
        public string userName;
        public string passwork;

        public AccountDto(string userName, string passwork)
        {
            this.userName = userName;
            this.passwork = passwork;
        }

        public void Change(string userName, string passwork)
        {
            this.userName = userName;
            this.passwork = passwork;
        }
    }
}
