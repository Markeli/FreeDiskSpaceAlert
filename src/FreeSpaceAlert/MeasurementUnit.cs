using System;

namespace FreeSpaceAlert
{
    public enum MeasurementUnit
    {
        Byte,
        KB,
        MB,
        GB
    }

    public static class MeasurementUnitExtensions
    {
        public static string ToText(this MeasurementUnit unit)
        {
            switch (unit)
            {
                case MeasurementUnit.Byte:
                    return "bytes";
                case MeasurementUnit.KB:
                    return "KB";
                case MeasurementUnit.MB:
                    return "MB";
                case MeasurementUnit.GB:
                    return "GB";
                default:
                    throw new ArgumentOutOfRangeException(nameof(unit), unit, null);
            }
        }
    }
}