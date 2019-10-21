using ControllableDevice;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ControlRelay
{
    internal static class ControlRelayInitialisation
    {
        public static List<DeviceCloudInterface> CreateDeviceCloudInterfaces(List<object> _devices)
        {
            List<DeviceCloudInterface> deviceCloudInterfaces = new List<DeviceCloudInterface>();

            var atenVS0801H = _devices.OfType<AtenVS0801H>().ToList();
            AddCloudInterface(() => new AtenVS0801HCloudInterface(atenVS0801H), deviceCloudInterfaces);

            var apcAP8959EU3 = _devices.OfType<ApcAP8959EU3>().First();
            AddCloudInterface(() => new ApcAP8959EU3CloudInterface(apcAP8959EU3), deviceCloudInterfaces);

            var extronDSC301HD = _devices.OfType<ExtronDSC301HD>().First();
            AddCloudInterface(() => new ExtronDSC301HDCloudInterface(extronDSC301HD), deviceCloudInterfaces);

            var sonyKDL60W855 = _devices.OfType<SonyKDL60W855>().First();
            AddCloudInterface(() => new SonyKDL60W855CloudInterface(sonyKDL60W855), deviceCloudInterfaces);

            var extronMVX44VGA = _devices.OfType<ExtronMVX44VGA>().First();
            AddCloudInterface(() => new ExtronMVX44VGACloudInterface(extronMVX44VGA), deviceCloudInterfaces);

            var ossc = _devices.OfType<OSSC>().First();
            AddCloudInterface(() => new OSSCCloudInterface(ossc), deviceCloudInterfaces);

            return deviceCloudInterfaces;
        }

        public static void AddCloudInterface<T>(Func<T> creator, List<DeviceCloudInterface> list) where T : DeviceCloudInterface
        {
            try
            {
                T cloudInterface = creator();
                list.Add(cloudInterface);
            }
            catch (Exception)
            {

            }
        }

        public static List<object> CreateControllableDevices(JToken deviceTypesJson)
        {
            SerialBlaster serialBlaster = null;

            var devices = new List<object>();

            //For each device
            foreach (var deviceTypeJson in deviceTypesJson)
            {
                string deviceTypeAsString = ((JProperty)deviceTypeJson).Name;

                //Get the constructor and parameters for the class were going to create (e.g AtenVS0801H)
                Type deviceType = Type.GetType($"ControllableDevice.{deviceTypeAsString}, ControllableDevice");
                var constructor = deviceType.GetConstructors()[0];
                var parameterInfos = constructor.GetParameters();

                // For each instance of the device settings
                foreach (var deviceSettingsJson in deviceTypeJson.First())
                {
                    var parameters = new List<object>();

                    //For each paramter, either manager the cast or assignment explicitly by type, or rely on an explicit cast
                    foreach (var parameterInfo in parameterInfos)
                    {
                        Type type = parameterInfo.ParameterType;
                        string value = ((JValue)deviceSettingsJson[parameterInfo.Name])?.Value.ToString();

                        switch (type)
                        {
                            case Type _ when type == typeof(SerialBlaster):
                                Debug.Assert(serialBlaster != null, $"SerialBlaster has not been initialised but is required for construciton of {deviceType.Name}.");
                                parameters.Add(serialBlaster);
                                break;
                            case Type _ when type == typeof(IPAddress):
                                parameters.Add(IPAddress.Parse(value));
                                break;
                            case Type _ when type == typeof(PhysicalAddress):
                                parameters.Add(PhysicalAddress.Parse(value));
                                break;
                            default:
                                //Else rely on an explicit case
                                parameters.Add(Convert.ChangeType(value, parameterInfo.ParameterType));
                                break;
                        }
                    }
                    
                    var device = constructor.Invoke(parameters.ToArray());

                    if(device.GetType() == typeof(SerialBlaster))
                    {
                        serialBlaster = (SerialBlaster)device;
                    }

                    devices.Add(device);
                }
            }

            return devices;
        }
    }
}
