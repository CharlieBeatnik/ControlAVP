using System;

namespace ControllableDeviceTypes
{
    namespace ApcAP8959EU3Types
    {
        public class Outlet
        {
            public enum PowerState
            {
                On,
                Off
            }

            public int Id { get; set; }
            public string Name { get; set; }
            public PowerState State { get; set; }
            public bool Pending { get; set; }
            public float Watts { get; set; }
            public float Amps { get; set; }
        }
    }
}
