using System;

namespace Monitor
{
    public struct DiskSize
    {
        private const double BytesInKB = 1024;
        private const double BytesInMB = BytesInKB * 1024;
        private const double BytesInGB = BytesInMB * 1024;
        
        public double Size { get; }
        
        public MeasurementUnit Unit { get; }

        public DiskSize(
            double size, 
            MeasurementUnit unit = MeasurementUnit.Byte)
        {
            Size = size;
            Unit = unit;
        }
        
        public DiskSize ConvertTo(MeasurementUnit targetUnit)
        {
            if (Unit == targetUnit) return this;

            double sizeInBytes;
            switch (Unit)
            {
                case MeasurementUnit.KB:
                    sizeInBytes = Size * BytesInKB;
                    break;
                case MeasurementUnit.MB:
                    sizeInBytes = Size * BytesInMB;
                    break;
                case MeasurementUnit.GB:
                    sizeInBytes = Size * BytesInGB;
                    break;
                default:
                    sizeInBytes = Size;
                    break;
            }

            switch (targetUnit)
            {
                case MeasurementUnit.Byte:
                    return new DiskSize(sizeInBytes);
                case MeasurementUnit.KB:
                    return new DiskSize(sizeInBytes / BytesInKB, MeasurementUnit.KB);
                case MeasurementUnit.MB:
                    return new DiskSize(sizeInBytes / BytesInMB, MeasurementUnit.MB);
                case MeasurementUnit.GB:
                    return new DiskSize(sizeInBytes / BytesInGB, MeasurementUnit.GB);
                default:
                    throw new ArgumentOutOfRangeException(nameof(targetUnit), targetUnit, null);
            }
        }
        
        public override string ToString()
        {
            return $"{Math.Round(Size, 2)} {Unit.ToText()}";
        }
    }
}