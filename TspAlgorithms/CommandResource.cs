using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GUIwpf
{
    public class CommandResource
    {
        private string _command = null;
        private object _lock = new object();


        public string GetCommand()
        {
            lock (_lock)
            {
                while (_command == null)
                    Monitor.Wait(_lock);
            }

            String ret = _command;
            _command = null;
            return ret;
        }

        public void SetCommand(string command)
        {
            lock( _lock)
            {
                _command = command;
                Monitor.Pulse(_lock);
            }
        }
    }
}
