// ReSharper disable InconsistentNaming
namespace Net9BehaviorChange;

public abstract class Program
{
    public static void Main()
    {
        var dateTimeOffsetData = new byte[] { 84, 80, 1, 37, 53, 11, 10, 0};
        GetBinaryDateTimeOffsetString(
            dateTimeOffsetData,
            0,
            8,
            0);
    }

    private static void GetBinaryDateTimeOffsetString(
        byte[] data,
        byte prec,
        byte len,
        byte scale)
    {
        SS2008DateTime2(data, (byte)(prec - 7), (byte)(len - 2), scale);
    }

    private static void SS2008DateTime2(
        byte[] data,
        byte precision,
        byte length,
        byte scale)
    {
        var m_Time = SS2008Time(data, precision - 11, length - 3, scale);
        Console.WriteLine($"m_Time: {m_Time.m_Hour}:{m_Time.m_Minute}:{m_Time.m_Second}.{m_Time.m_SubSecond}");
    }

    private static SqlServer2008Time SS2008Time(
        byte[] data,
        int precision,
        int length,
        int scale)
    {
        var time = new SqlServer2008Time();
        double subsecond;
        ulong value;
        var divider = (ulong)Math.Pow(10, precision - 9);

        switch (length)
        {
            case 3:
            {
                value = (ulong)(BitConverter.ToUInt16(data, 0) | (data[2] << 16));
            }
                break;
            case 4:
            {
                value = BitConverter.ToUInt32(data, 0);
            }
                break;
            case 5:
            {
                value = BitConverter.ToUInt32(data, 0) | ((ulong)data[4] << 32);
            }
                break;
            default:
                throw new Exception();
        }

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
        return time;
    }

    private class SqlServer2008Time
    {
        internal sbyte m_Hour, m_Minute, m_Second;
        internal int m_SubSecond;
    }
}