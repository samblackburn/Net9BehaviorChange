// ReSharper disable InconsistentNaming
namespace Net9BehaviorChange;

public abstract class Program
{
    private static readonly int[] s_daysToMonth =
    [
        0,
        31,
        59,
        90,
        120,
        151,
        181,
        212,
        243,
        273,
        304,
        334,
        365
    ];

    private static readonly int[] s_daysToMonthLeapYear =
    [
        0,
        31,
        60,
        91,
        121,
        152,
        182,
        213,
        244,
        274,
        305,
        335,
        366
    ];

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
        var dateTime2 = SS2008DateTime2(data, (byte)(prec - 7), (byte)(len - 2), scale);
        Console.WriteLine($"\tResult: {dateTime2.m_Time.m_Hour}:{dateTime2.m_Time.m_Minute}:{dateTime2.m_Time.m_Second}.{dateTime2.m_Time.m_SubSecond}");
    }

    private static SqlServer2008DateTime2 SS2008DateTime2(
        byte[] data,
        byte precision,
        byte length,
        byte scale)
    {
        var dateTime2 = new SqlServer2008DateTime2
        {
            m_Time = SS2008Time(data, precision - 11, length - 3, scale)
        };
        var dateData = new byte[3];
        Buffer.BlockCopy(data, length - 3, dateData, 0, dateData.Length);
        dateTime2.m_Date = DateDateTime(dateData);
        return dateTime2;
    }

    private static SqlServer2008Date DateDateTime(byte[] data)
    {
        var intValue = BitConverter.ToUInt16(data, 0) | (data[2] << 16);

        const int quadCenturyYearCycleLength = 146097; // roughly 365.25 * 400
        var yearCycleOffset = intValue / quadCenturyYearCycleLength;
        intValue -= yearCycleOffset * quadCenturyYearCycleLength;

        const int approxDaysIn100Years = 36524;
        var centuriesSinceYearCycleStart = intValue / approxDaysIn100Years;
        if (centuriesSinceYearCycleStart == 4)
        {
            centuriesSinceYearCycleStart = 3;
        }

        intValue -= centuriesSinceYearCycleStart * approxDaysIn100Years;

        const int daysInFourYears = 1461; // 365.25 * 4
        var quadYearsSinceCentury = intValue / daysInFourYears;
        intValue -= quadYearsSinceCentury * daysInFourYears;

        const int daysInNonLeapYear = 365;
        var yearsSinceQuadYear = intValue / daysInNonLeapYear;
        if (yearsSinceQuadYear == 4)
        {
            yearsSinceQuadYear = 3;
        }

        intValue -= yearsSinceQuadYear * daysInNonLeapYear;

        var date = new SqlServer2008Date
        {
            m_Year = yearCycleOffset * 400
                     + centuriesSinceYearCycleStart * 100
                     + quadYearsSinceCentury * 4
                     + yearsSinceQuadYear
                     + 1
        };

        var isLeapYear = yearsSinceQuadYear == 3
                         && (quadYearsSinceCentury != 24 || centuriesSinceYearCycleStart == 3);
        var numArray = isLeapYear ? s_daysToMonthLeapYear : s_daysToMonth;
        var index = intValue >> 6;
        while (intValue >= numArray[index])
        {
            index++;
        }

        date.m_Month = (sbyte)index;
        date.m_Day = (sbyte)(intValue - numArray[index - 1] + 1);
        return date;
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

    private struct SqlServer2008DateTime2
    {
        internal SqlServer2008Time m_Time;
        internal SqlServer2008Date m_Date;
    }

    private class SqlServer2008Time
    {
        internal sbyte m_Hour, m_Minute, m_Second;
        internal int m_SubSecond;
    }

    private class SqlServer2008Date
    {
        internal sbyte m_Day, m_Month;
        internal int m_Year;
    }
}