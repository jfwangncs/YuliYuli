using System.Threading;
using System.Threading.Tasks;

namespace YuliYuli.Models
{
    public class DownLoadTask
    {
        public string Key { get; set; }
        public Task VideoTask { get; set; }

        public CancellationTokenSource Token { get; set; }

        public ManualResetEvent ResetEvent { get; set; }
    }
}
