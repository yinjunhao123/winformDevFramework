using SqlSugar;

namespace WinformDevFramework.IRepository.UnitOfWork
{
    public interface IUnitOfWork
    {
        SqlSugarScope GetDbClient();
        void BeginTran();
        void CommitTran();
        void RollbackTran();
    }
}
