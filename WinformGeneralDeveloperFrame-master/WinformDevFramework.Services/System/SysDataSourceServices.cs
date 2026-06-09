using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinformDevFramework.IRepository.System;
using WinformDevFramework.IRepository.UnitOfWork;
using WinformDevFramework.IServices.System;


namespace WinformDevFramework.Services.System
{
    public class SysDataSourceServices:BaseServices<sysDataSource>, ISysDataSourceServices
    {
        private readonly ISysDataSourceRepository _dal;
        private readonly IUnitOfWork _unitOfWork;

        public SysDataSourceServices(IUnitOfWork unitOfWork, ISysDataSourceRepository dal)
        {
            this._dal = dal;
            base.BaseDal = dal;
            _unitOfWork = unitOfWork;
        }
    }
}
