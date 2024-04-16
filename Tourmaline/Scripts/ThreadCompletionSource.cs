using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tourmaline.Scripts
{
    public class ThreadCompletionSource
    {
        private readonly object tcsLock = new();
        public bool Finished { get; set; } = false;

        public void Finish()
        {
            lock (tcsLock)
            {
                Finished = true;
            }
        }
        public async Task Wait()
        {
            while (true)
            {
                bool finished;
                lock (tcsLock)
                {
                    finished = Finished;
                }

                if (finished == true) break;
                await Task.Delay(50);
            }
        }
        public bool IsFinished()
        {
            lock (tcsLock)
            {
                return Finished;
            }
        }
    }
}
