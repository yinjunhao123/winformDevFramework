using WinformDevFramework.IRepository;
using SqlSugar;

namespace WinformDevFramework.Repository
{
    public class GenericRepository<T> : BaseRepository<T>, IBaseRepository<T> where T : class, new()
    {
        public GenericRepository(ISqlSugarClient sqlSugar) : base(sqlSugar)
        {
        }
    }
}