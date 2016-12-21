using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bluetooth
{
    public class ReceivedData_EventArgs : EventArgs
    {
        private ArrayList al;

        public ArrayList ArrayL
        {
            get { return al; }
            set { al = value; }
        }
        

        public ReceivedData_EventArgs(params object[] list)
        {
            al = new ArrayList(); 
            al.Add(list);
        }
    }
}
