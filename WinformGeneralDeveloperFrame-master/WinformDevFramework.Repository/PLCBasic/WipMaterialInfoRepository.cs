using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinformDevFramework.IRepository;
using WinformDevFramework.Repository;
using PLCBasic;
using PLCBasic.IRepository;

namespace PLCBasic.Repository
{
    public class WipMaterialInfoRepository : BaseRepository<WipMaterialInfo>, IWipMaterialInfoRepository
    {
        public WipMaterialInfoRepository(ISqlSugarClient sqlSugar) : base(sqlSugar)
        {
        }
    }
}