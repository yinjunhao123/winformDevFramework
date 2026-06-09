using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinformDevFramework.Repository
{
    internal static class SqlSugarExtensions
    {
        internal static ISugarQueryable<T> WithNoLockOrNot<T>(this ISugarQueryable<T> query, bool @lock = false)
        {
            if (@lock)
            {
                query = query.With(SqlWith.NoLock);
            }

            return query;
        }
    }
}
