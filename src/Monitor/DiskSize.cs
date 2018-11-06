namespace Monitor
{
    public struct DiskSize
    {
        public double Size { get; }
        
        public MeasurementUnit Unit { get; }

        public DiskSize(
            double size, 
            MeasurementUnit unit = MeasurementUnit.Byte)
        {
            Size = size;
            Unit = unit;
        }

        public override string ToString()
        {
            return $"{Size} {Unit.ToText()}";
        }
    }
}