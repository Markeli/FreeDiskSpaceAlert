using System;
using System.IO;

namespace Monitor
{
    internal class MonitoringTrigger
    {
        public string DeviceName { get; }
        
        public double ThresholdValue { get; }
        
        public TriggerMode Mode { get; }
        
        public MeasurementUnit EventUnit { get; }

        public MonitoringTrigger(
            string deviceName, 
            double thresholdValue, 
            TriggerMode mode, 
            MeasurementUnit eventUnit)
        {
            DeviceName = deviceName;
            ThresholdValue = thresholdValue;
            Mode = mode;
            EventUnit = eventUnit;
        }

        public bool IsTriggered(DriveInfo info)
        {
            if (info == null) throw new ArgumentNullException(nameof(info));
            if (String.Equals(info.Name, DeviceName, StringComparison.InvariantCultureIgnoreCase)) return false;

            switch (Mode)
            {
                case TriggerMode.Accuracy:
                    return info.AvailableFreeSpace < ThresholdValue;
                case TriggerMode.Percentile:
                    return (info.AvailableFreeSpace - info.TotalSize) / 100 < ThresholdValue;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}