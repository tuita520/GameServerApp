using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServerApp
{
    /// <summary>
    /// 角色类
    /// </summary>
    public class Role
    {
        /// <summary>
        /// 持有ClientSocket对象
        /// </summary>
        public ClientSocket m_ClientSocket { get; set; }
    }
}
