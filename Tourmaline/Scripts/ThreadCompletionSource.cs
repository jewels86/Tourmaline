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
        private bool finished { get; set; } = false;

        public delegate void Handler();
        public event Handler Finished = delegate { };

        public void Finish()
        {
            lock (tcsLock)
            {
                Finished.Invoke();
                finished = true;
            }
        }
        public async Task Wait()
        {
            while (true)
            {
                bool _finished;
                lock (tcsLock)
                {
                    _finished = finished;
                }

                if (_finished == true) break;
                await Task.Delay(50);
            }
        }
        public bool IsFinished()
        {
            lock (tcsLock)
            {
                return finished;
            }
        }
    }
}
