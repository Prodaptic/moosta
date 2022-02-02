using System;

namespace Moosta.Core
{
    public static class DateTimeTools
    {
        public static readonly DateTime epoch = new DateTime(1970,1,1);
        public static int ToEpoch(this DateTime dateTime)
        {
            return (int)(dateTime.ToUniversalTime() - epoch).TotalSeconds;
        }
    }
}
