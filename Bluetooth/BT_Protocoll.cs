using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bluetooth
{
    public class BT_Protocoll
    {
        public static int FrameLengthOverhead = 3;
        public static int PräambleLength = 2;
        public static int FrameLengthLength = 2;
        public static int CommandLength = 1; 

        public static int StayingAliveLength = 3; 


        public static byte[] PräambleBytes = new byte [] {0xCA, 0xFE};
        public static byte CarriageReturn = 0x0D; 

        public  const byte StayingAliveCommand = 0x02;
        public  const byte StayingAliveAnswer = 0x82; 
    }
}
