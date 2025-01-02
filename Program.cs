// ReSharper disable InconsistentNaming
namespace Net9BehaviorChange;

public abstract class Program
{
    public static void Main()
    {
        var dateTimeOffsetData = new byte[] { 84, 80, 1, 37, 53, 11, 10, 0};
        GetBinaryDateTimeOffsetString(dateTimeOffsetData, 0, 0);
    }

    private static void GetBinaryDateTimeOffsetString(
        byte[] data,
        byte prec,
        byte scale)
    {
        var precision = (byte)(prec - 7);
        var time = new SqlServer2008Time();
        double subsecond;
        var divider = (ulong)Math.Pow(10, precision - 11 - 9);
        var value = (ulong)(BitConverter.ToUInt16(data, 0) | (data[2] << 16));
        //Console.WriteLine($"value: {value}");

        if (divider == 0)
        {
            subsecond = 0;
            time.m_Second = (sbyte)(value % 60);
        }
        else
        {
            subsecond = (double)(value % divider) / divider;
            time.m_Second = (sbyte)((value /= divider) % 60);
        }

        time.m_Minute = (sbyte)((value /= 60) % 60);
        time.m_Hour = (sbyte)(value / 60 % 60);

        if (scale != 0)
        {
            //Debug.Assert(scale <= 7, "Scale of Time is incorrect should be between 0 and 7");
            subsecond = Math.Round(subsecond, scale);
            subsecond *= divider;
        }

        time.m_SubSecond = (int)Math.Round(subsecond);
        Console.WriteLine($"time: {time.m_Hour}:{time.m_Minute}:{time.m_Second}.{time.m_SubSecond}");
    }

    private class SqlServer2008Time
    {
        internal sbyte m_Hour, m_Minute, m_Second;
        internal int m_SubSecond;
    }
}