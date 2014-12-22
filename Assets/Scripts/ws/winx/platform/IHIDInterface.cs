//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.17929
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
using System;
using UnityEngine;
using ws.winx.devices;
using System.Collections.Generic;

namespace ws.winx.platform
{
		public interface IHIDInterface:IDisposable
		{
			event EventHandler<DeviceEventArgs<int>> DeviceDisconnectEvent;
			event EventHandler<DeviceEventArgs<IDevice>> DeviceConnectEvent;
			


			IDriver defaultDriver{get;set;}
            Dictionary<int, HIDDevice> Generics{get;}

            /// <summary>
            /// Reading by use of OS default driver (Win-WINMMDriver, Osx-OSXDriver...
            /// </summary>
            /// <param name="pid"></param>
            /// <returns></returns>
			HIDReport ReadDefault(int pid);
            HIDReport ReadBuffered(int pid);
            void Read(int pid,HIDDevice.ReadCallback callback);
            void Read(int pid, HIDDevice.ReadCallback callback,int timeout);
            void Write(object data, int device, HIDDevice.WriteCallback callback, int timeout);
            void Write(object data, int device,HIDDevice.WriteCallback callback);
            void Write(object data, int device);
			bool Contains(int pid);
			void AddDriver(IDriver driver);
			void Update();
			void Enumerate();
		    

		}
}

