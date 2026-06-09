using Microsoft.Data.SqlClient;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinformDevFramework.Basic.IRepository;
using WinformDevFramework.Models.Basic;
using WinformDevFramework.Repository;


namespace WinformDevFramework.Basic.Repository
{
    public class MD_ProductModelRepository : BaseRepository<MD_ProductModel>, IMD_ProductModelRepository
    {
        public MD_ProductModelRepository(ISqlSugarClient sqlSugar) : base(sqlSugar)
        {

        }
    }
}