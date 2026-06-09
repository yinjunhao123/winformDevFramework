using PLCBasic;
using PLCBasic.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinformDevFramework.IServices.PLCBasic;

namespace WinformDevFramework.Services.PLCBasic
{
    public class PLC_StationRecipeCurrentService : BaseServices<PLC_StationRecipeCurrent>, IPLC_StationRecipeCurrentService
    {
        private readonly IPLC_StationRecipeCurrentRepository _repository;

        public PLC_StationRecipeCurrentService(IPLC_StationRecipeCurrentRepository repository)
        {
            _repository = repository;
            BaseDal = repository;
        }
    }
}
