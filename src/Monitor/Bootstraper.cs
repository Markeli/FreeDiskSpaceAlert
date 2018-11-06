using Topshelf;

namespace Monitor
{
    public class Bootstraper : ServiceControl
    {
        public bool Start(HostControl hostControl)
        {
            //throw new System.NotImplementedException();
            return true;
        }

        public bool Stop(HostControl hostControl)
        {
            return true;
        }
    }
}