using System;

namespace Monitor
{
    public static class DiskSizeExntensions
    {
        private const double BytesInKB = 1024;
        private const double BytesInMB = BytesInKB * 1024;
        private const double BytesInGB = BytesInMB * 1024;
        
        public static DiskSize ConvertTo(this DiskSize diskSize, MeasurementUnit targetUnit)
        {
            if (diskSize.Unit == targetUnit) return diskSize;

            double sizeInBytes;
            switch (diskSize.Unit)
            {
                case MeasurementUnit.KB:
                    sizeInBytes = diskSize.Size * BytesInKB;
                    break;
                case MeasurementUnit.MB:
                    sizeInBytes = diskSize.Size * BytesInMB;
                    break;
                case MeasurementUnit.GB:
                    sizeInBytes = diskSize.Size * BytesInGB;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
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
    }
}