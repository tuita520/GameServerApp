using System.Collections.Generic;

namespace GameServerApp
{
    class RoleManager
    {
        #region 单例
        private static RoleManager instance;
        private static object _lock = new object();

        public static RoleManager Instance
        {
            get
            {
                lock (_lock)
                {
                    if (instance == null)
                    {
                        instance = new RoleManager();
                    }
                }
                return instance;
            }
        }

        private RoleManager()
        {

        }
        #endregion

        /// <summary>
        /// List集合，存储所有Role
        /// </summary>
        private List<Role> m_AllRole = new List<Role>();

        public List<Role> AllRole
        {
            get
            {
                return m_AllRole;
            }
        }
    }
}