using PLCBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinformDevFramework.IServices.PLCBasic
{
    public interface IPlcDataCollectService
    {
        void Start();
        void Stop();
        bool IsRunning { get; }
        int CollectIntervalMs { get; set; }
    }
}
