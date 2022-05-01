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

            var atenVS0801HB = _devices.OfType<AtenVS0801HB>().ToList();
            AddCloudInterface(() => new AtenVS0801HBCloudInterface(atenVS0801HB), deviceCloudInterfaces);

            var apcAP8959EU3 = _devices.OfType<ApcAP8959EU3>().First();
            AddCloudInterface(() => new ApcAP8959EU3CloudInterface(apcAP8959EU3), deviceCloudInterfaces);

            var extronDSC301HD = _devices.OfType<ExtronDSC301HD>().First();
            AddCloudInterface(() => new ExtronDSC301HDCloudInterface(extronDSC301HD), deviceCloudInterfaces);

            var sonySimpleIP = _devices.OfType<SonySimpleIP>().First();
            AddCloudInterface(() => new SonySimpleIPCloudInterface(sonySimpleIP), deviceCloudInterfaces);

            var extronMVX44VGA = _devices.OfType<ExtronMVX44VGA>().First();
            AddCloudInterface(() => new ExtronMVX44VGACloudInterface(extronMVX44VGA), deviceCloudInterfaces);

            var ossc = _devices.OfType<OSSC>().First();
            AddCloudInterface(() => new OSSCCloudInterface(ossc), deviceCloudInterfaces);

            var retroTink5xPro = _devices.OfType<RetroTink5xPro>().First();
            AddCloudInterface(() => new RetroTink5xProCloudInterface(retroTink5xPro), deviceCloudInterfaces);

            //All devices are added to the command processor, as that can call functionality from any device
            AddCloudInterface(() => new CommandProcessorInterface(_devices), deviceCloudInterfaces);

            return deviceCloudInterfaces;
        }

        public static void AddCloudInterface<T>(Func<T> creator, List<DeviceCloudInterface> list) where T : DeviceCloudInterface
        {
            T cloudInterface = creator();
            list.Add(cloudInterface);
        }

        public static List<object> CreateControllableDevices(JToken deviceTypesJson)
        {
            var serialBlasters = new List<SerialBlaster>();

            var devices = new List<object>();

            //For each device
            foreach (var deviceTypeJson in deviceTypesJson)
            {
                string deviceTypeAsString = ((JProperty)deviceTypeJson).Name;

                //Get the constructor and parameters for the class were going to create (e.g AtenVS0801H)
                Type deviceType = Type.GetType($"ControllableDevice.{deviceTypeAsString}, ControllableDevice");
                var constructors = deviceType.GetConstructors();

                foreach (var constructor in constructors)
                {
                    var parameterInfos = constructor.GetParameters();

                    // For each instance of the device settings
                    foreach (var deviceSettingsJson in deviceTypeJson.First())
                    {
                        var parameters = new List<object>();

                        //Look for special serialBlasterIndex parameter
                        Int64? serialBlasterIndex = (Int64?)((JValue)deviceSettingsJson["serialBlasterIndex"])?.Value;
                        bool hasSerialBlasterIndex = serialBlasterIndex != null;

                        //For each paramter, either manager the cast or assignment explicitly by type, or rely on an explicit cast
                        foreach (var parameterInfo in parameterInfos)
                        {
                            Type type = parameterInfo.ParameterType;
                            string value = ((JValue)deviceSettingsJson[parameterInfo.Name])?.Value.ToString();

                            if (value != null || type == typeof(SerialBlaster))
                            {
                                switch (type)
                                {
                                    case Type _ when type == typeof(SerialBlaster):
                                        Debug.Assert(hasSerialBlasterIndex, $"SerialBlaster is required for consruction of {deviceType.Name}, but no serialBlasterIndex parameter has been provided.");
                                        Debug.Assert(serialBlasters.Count >= serialBlasterIndex + 1, $"A SerialBlaster index of value {serialBlasterIndex} has been requested but does not exist.");
                                        parameters.Add(serialBlasters[(int)serialBlasterIndex]);
                                        break;
                                    case Type _ when type == typeof(IPAddress):
                                        parameters.Add(IPAddress.Parse(value));
                                        break;
                                    case Type _ when type == typeof(PhysicalAddress):
                                        parameters.Add(PhysicalAddress.Parse(value));
                                        break;
                                    default:
                                        //Ignore special serialBlasterIndex parameter
                                        if (parameterInfo.Name == "serialBlasterIndex") break;

                                        //Default is to rely on an explicit case
                                        parameters.Add(Convert.ChangeType(value, parameterInfo.ParameterType));
                                        break;
                                }
                            }
                        }

                        try
                        {
                            var device = constructor.Invoke(parameters.ToArray());

                            if (device.GetType() == typeof(SerialBlaster))
                            {
                                serialBlasters.Add((SerialBlaster)device);
                            }

                            devices.Add(device);
                        }
                        catch
                        {
                            break;
                        }
                    }
                }
            }

            return devices;
        }
    }
}
