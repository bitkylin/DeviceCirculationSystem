using System;

namespace DeviceCirculationSystem.bean
{
    public class NumBelowZeroException:Exception
    {
        public NumBelowZeroException(string str):base(str)
        {
        }
    }
}