using System;
using System.IO;

namespace FreeSpaceAlert
{
    internal class AlertTrigger
    {
        public string DeviceName { get; }
        
        public double ThresholdValueInBytes { get; }
        
        public TriggerMode Mode { get; }
        
        public MeasurementUnit EventUnit { get; }

        public AlertTrigger(
            string deviceName, 
            double thresholdValueInBytes, 
            TriggerMode mode, 
            MeasurementUnit eventUnit)
        {
            if (String.IsNullOrWhiteSpace(deviceName)) throw new ArgumentNullException($"Specify {nameof(deviceName)}");
            if (thresholdValueInBytes <= 0) throw new ArgumentException($"{nameof(thresholdValueInBytes)} should be greater than zero");
            
            DeviceName = deviceName.EndsWith(":\\")
                ? deviceName
                : $"{deviceName}:\\";
            ThresholdValueInBytes = thresholdValueInBytes;
            Mode = mode;
            if (mode == TriggerMode.Accuracy)
            {
                var temp = new DiskSize(thresholdValueInBytes, eventUnit);
                ThresholdValueInBytes = temp.ConvertTo(MeasurementUnit.Byte).Size;
            }
            EventUnit = eventUnit;
        }

        public bool IsTriggered(DriveInfo info)
        {
            if (info == null) throw new ArgumentNullException(nameof(info));
            if (!String.Equals(info.Name, DeviceName, StringComparison.InvariantCultureIgnoreCase)) return false;

            switch (Mode)
            {
                case TriggerMode.Accuracy:
                    return info.AvailableFreeSpace < ThresholdValueInBytes;
                case TriggerMode.Percentile:
                    return (double)info.AvailableFreeSpace / info.TotalSize < ThresholdValueInBytes;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}