# ControlAVP
AVP stands for **Audio**, **Video** & **Power**. ControlAVP was written to allow control of different home A/V equipment using different communication methods. Example equipment includes, but is not limited to:

* Open Source Scan Converter (OSSC) via **IR**
* Aten HDMI Switch via **RS232**
* Sony Television via **JSON RPC**
* APC Power Distribution Unit (PDU) via **SSH**

## Project Status
ControlAVP was written to control the specific equipment in my home setup, but could potentially be extended to suit other home A/V setups. This project has unfortunately not yet been productised and currently requires some software programming skills to build, configure and run.

## Components
### 1. ControlAVP
ControlAVP itself is an ASP.NET Core web application built using C# that is hosted in the [Azure Cloud](https://azure.microsoft.com/en-gb/). It provides a web interface to the equipment with communication being managed by an [Azure Iot Hub](https://azure.microsoft.com/en-gb/services/iot-hub/). This allows the equipment to be controlled from any devices that supports a web browser from anywhere in the world.

### 2. ControlRelay
ControlRelay is a Universal Windows Platform (UWP) application built using C#. It runs using Windows IoT on a RaspberryPi 3 and handles messages that arrive via the Azure IoT Hub. These messages are prosessed and then relayed to the appropriate device via *IR*, *RS232*, *JSON RPC* or *SSH*. 
