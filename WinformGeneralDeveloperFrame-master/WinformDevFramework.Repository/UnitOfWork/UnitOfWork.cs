using Dm.Config;
using SqlSugar;
using SqlSugar.IOC;
using WinformDevFramework.IRepository.UnitOfWork;

namespace WinformDevFramework.Repository.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ISqlSugarClient _sqlSugarClient;

        //public UnitOfWork(ISqlSugarClient sqlSugarClient)
        //{
        //    _sqlSugarClient = sqlSugarClient;
        //}
        public UnitOfWork()
        {
            _sqlSugarClient = DbScoped.SugarScope;
        }

        /// <summary>
        ///     获取DB，保证唯一性
        /// </summary>
        /// <returns></returns>
        public SqlSugarScope GetDbClient()
        {
            // 必须要as，后边会用到切换数据库操作
            return _sqlSugarClient as SqlSugarScope;
        }

        public void BeginTran()
        {
            GetDbClient().BeginTran();
        }

        public void CommitTran()
        {
            try
            {
                GetDbClient().CommitTran(); //
            }
            catch (Exception ex)
            {
                GetDbClient().RollbackTran();
                throw;
            }
        }

        public void RollbackTran()
        {
            GetDbClient().RollbackTran();
        }
    }
}
