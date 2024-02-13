using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Debricked.Helpers
{
    internal class EventBurstHelper
    {
        private Action _action;
        private long _delay;
        private long lockObj = 1;
        private Task _delayedTask = null;
        public EventBurstHelper(Action action, long msDelay)
        {
            _action = action;
            _delay = msDelay;
        }

        public void Execute()
        {
            if(Interlocked.Exchange(ref lockObj, 0) == 1) 
            {
                _delayedTask = Task.Delay(TimeSpan.FromMilliseconds(_delay)).ContinueWith(execute, TaskScheduler.Default);
            }
        }

        private void execute(Task _)
        {
            Interlocked.Exchange(ref lockObj, 1);
            _action();
        }

        public Task CompleteAsync()
        {
            return _delayedTask ?? Task.FromResult(-1);
        }

    }
}
