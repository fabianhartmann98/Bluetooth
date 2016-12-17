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
        public static int FrameLengthLength = 1;
        public static int CommandLength = 1; 

        public static byte[] PräambleBytes = new byte [] {0xCA, 0xFE};
        public static byte CarriageReturn = 0x0D;

        public static int InitLength = 3;
        public static int InitAnswerLength = 3; 
        public static int StayingAliveLength = 3;
        public static int StayingAliveAnswerLength = 3;
        public static int MeasuredDataLength = 10;            //not fixed yet
        public static int MeasuredDataAnswerLength = 4;
        public static int MotorAdjustingLength = 5;
        public static int MotorAdjustingAnswerLength = 5;
        public static int StatusRequestLength = 3;
        public static int StatusRequestAnswerLength = 4;
        public static int PositionRequestLength = 3;
        public static int PositionRequestAnswerLength = 7;
        public static int MaxGapRequestLength = 3;
        public static int MaxGapRequestAnswerLength = 5;

        public const byte InitCommand = 0x01;
        public const byte InitAnswer = 0x81;
        public const byte StayingAliveCommand = 0x02;
        public const byte StayingAliveAnswer = 0x82;
        public const byte MeasuredDataCommand = 0x04;
        public const byte MeasuredDataAnswer = 0x84;
        public const byte MotorAdjustingCommand = 0x08;
        public const byte MotorAdjustingAnswer = 0x88;
        public const byte StatusRequestCommand = 0x11;
        public const byte StatusRequestAnswer = 0x91;
        public const byte PositionRequestCommand = 0x12;
        public const byte PositionRequestAnswer = 0x92;
        public const byte MaxGapRequestCommand = 0x13;
        public const byte MaxGapRequestAnswer = 0x93;


    }
}
