//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.17929
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
#if UNITY_STANDALONE_WIN || UNITY_EDITOR
using System;
using UnityEngine;
using System.Runtime.InteropServices;
using ws.winx.devices;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using System.Text;


namespace ws.winx.platform.windows
{
    public class WinHIDInterface : IHIDInterface
    {




        #region Fields
        private List<IDriver> __drivers;// = new List<IJoystickDriver>();


        private IDriver __defaultJoystickDriver;

       

     

        delegate IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);
        private WndProc m_wnd_proc_delegate;

        private const int ERROR_CLASS_ALREADY_EXISTS = 1410;

        //GUID_DEVINTERFACE_HID	Class GUID{4D1E55B2-F16F-11CF-88CB-001111000030}
        private static readonly Guid GUID_DEVINTERFACE_HID = new Guid("4D1E55B2-F16F-11CF-88CB-001111000030"); // HID devices

        public IntPtr receiverWindowHandle;




        

      
        private static IntPtr notificationHandle;
        private Dictionary<int, HIDDevice> __Generics;

     
      
		public event EventHandler<DeviceEventArgs<int>> DeviceDisconnectEvent;
		public event EventHandler<DeviceEventArgs<IDevice>> DeviceConnectEvent;




        #endregion


        #region IHIDInterface implementation


		public HIDReport ReadDefault(int pid){
            return this.__Generics[pid].ReadDefault();
	    }

        public HIDReport ReadBuffered(int pid)
        {
            return __Generics[pid].ReadBuffered();
        }

        public void Read(int pid,HIDDevice.ReadCallback callback,int timeout)
        {
            this.__Generics[pid].Read(callback,timeout);

        }




        public void Read(int pid, HIDDevice.ReadCallback callback)
        {
            this.__Generics[pid].Read(callback, 0);

        }

        public void Write(object data, int pid)
        {
            this.__Generics[pid].Write(data);
        }

        public void Write(object data, int pid, HIDDevice.WriteCallback callback)
        {
            this.__Generics[pid].Write(data, callback, 0);
        }

        public void Write(object data, int pid, HIDDevice.WriteCallback callback,int timeout)
        {
            this.__Generics[pid].Write(data,callback,timeout);
        }


        /// <summary>
        /// add or get default driver (Overall driver for unhanlded devices by other specialized driver)
        /// </summary>
        public IDriver defaultDriver
        {
            get { if (__defaultJoystickDriver == null) { __defaultJoystickDriver = new WinMMDriver(); } return __defaultJoystickDriver; }
            set { __defaultJoystickDriver = value; 
				if(value is ws.winx.drivers.UnityDriver){
					Debug.LogWarning("UnityDriver set as default driver.\n Warring:Unity doesn't make distinction between triggers/axis/pow and axes happen to be mapped on different Joysticks#, doesn't support plug&play and often return weired raw results");
				}
			
			}

        }




       

        public Dictionary<int, HIDDevice> Generics
        {
            get { return __Generics; }
        }




        public void Update()
        {
            Enumerate();
        }

        #endregion

        //public static readonly Guid GUID_DEVCLASS_HIDCLASS = new Guid(0x745a17a0, 0x74d3, 0x11d0, 0xb6, 0xfe, 0x00, 0xa0, 0xc9, 0x0f, 0x57, 0xda);
        //public static readonly Guid GUID_DEVCLASS_USB = new Guid(0x36fc9e60, 0xc465, 0x11cf, 0x80, 0x56, 0x44, 0x45, 0x53, 0x54, 0x00, 0x00);



        //private static readonly Guid GuidDevinterfaceUSBDevice = new Guid("A5DCBF10-6530-11D2-901F-00C04FB951ED"); // USB devices




        #region Constructor
        public WinHIDInterface(List<IDriver> drivers)
        {
            __drivers = drivers;
           
            __Generics = new Dictionary<int, HIDDevice>();

      
          
        }
        #endregion


        // Specify what you want to happen when the Elapsed event is raised.
        //private void enumerateTimedEvent(object source, ElapsedEventArgs e)
        //{
        //    Update();

        //}




        /// <summary>
        /// Registers a window to receive notifications when USB devices are plugged or unplugged.
        /// </summary>
        /// <param name="windowHandle">Handle to the window receiving notifications.</param>
        public void RegisterHIDDeviceNotification(IntPtr windowHandle)
        {
            Native.DEV_BROADCAST_DEVICEINTERFACE dbi = new Native.DEV_BROADCAST_DEVICEINTERFACE
            {
                dbcc_size = 0,

                dbcc_devicetype = (int)Native.DBT_DEVTYP_DEVICEINTERFACE,

                dbcc_reserved = 0,

                dbcc_classguid = GUID_DEVINTERFACE_HID.ToByteArray()

            };




            dbi.dbcc_size = Marshal.SizeOf(dbi);
            IntPtr buffer = Marshal.AllocHGlobal(dbi.dbcc_size);
            Marshal.StructureToPtr(dbi, buffer, true);

            notificationHandle = Native.RegisterDeviceNotification(windowHandle, buffer, 0);


        }


        /// <summary>
        /// Creates window that would receive plug in/out device events
        /// </summary>
        /// <returns></returns>
        IntPtr CreateReceiverWnd()
        {

            IntPtr wndHnd = IntPtr.Zero;
            m_wnd_proc_delegate = CustomWndProc;

            // Create WNDCLASS
            Native.WNDCLASS wind_class = new Native.WNDCLASS();
            wind_class.lpszClassName = "InputManager Device Change Notification Reciver Wnd";
            wind_class.lpfnWndProc = System.Runtime.InteropServices.Marshal.GetFunctionPointerForDelegate(m_wnd_proc_delegate);

            UInt16 class_atom = Native.RegisterClassW(ref wind_class);

            int last_error = System.Runtime.InteropServices.Marshal.GetLastWin32Error();

            if (class_atom == 0 && last_error != ERROR_CLASS_ALREADY_EXISTS)
            {
                Exception e = new System.Exception("Could not register window class");

                UnityEngine.Debug.LogException(e);

                return IntPtr.Zero;
            }


            try
            {
                // Create window
                wndHnd = Native.CreateWindowExW(
                    0,
                    wind_class.lpszClassName,
                    String.Empty,
                    0,
                    0,
                    0,
                    0,
                    0,
                    IntPtr.Zero,
                    IntPtr.Zero,
                    IntPtr.Zero,
                    IntPtr.Zero
                    );
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogException(e);
            }


            return wndHnd;

        }

        /// <summary>
        /// Custom receiver window procedure where WM_MESSAGES are handled (WM_DEVICECHANGE)
        /// </summary>
        /// <param name="hWnd"></param>
        /// <param name="msg"></param>
        /// <param name="wParam"></param>
        /// <param name="lParam"></param>
        /// <returns></returns>
        protected IntPtr CustomWndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
        {
            int devType = 0;

            if (msg == Native.WM_DEVICECHANGE)
            {

                if (lParam != IntPtr.Zero)
                    devType = Marshal.ReadInt32(lParam, 4);

                switch ((int)wParam)
                {
                    case Native.DBT_DEVICEREMOVECOMPLETE:

                        if (devType == Native.DBT_DEVTYP_DEVICEINTERFACE)
                        {
                            try
                            {
                               
                                HIDDevice hidDevice = CreateHIDDeviceFrom(PointerToDevicePath(lParam));

                               
							if (this.Generics.ContainsKey(hidDevice.PID))
                                {
                                    
                                   
                                    this.Generics.Remove(hidDevice.PID);

								this.DeviceDisconnectEvent(this,new DeviceEventArgs<int>(hidDevice.PID));

                                    UnityEngine.Debug.Log("WinHIDInterface: " + hidDevice.Name + " Removed");
                                }



                            }
                            catch (Exception e)
                            {
                                UnityEngine.Debug.LogException(e);
                            }
                        }







                        break;
                    case Native.DBT_DEVICEARRIVAL:
                        if (devType == Native.DBT_DEVTYP_DEVICEINTERFACE)
                        {
                            try
                            {



                                HIDDevice hidDevice = CreateHIDDeviceFrom(PointerToDevicePath(lParam));

                                if (!Generics.ContainsKey(hidDevice.PID))
                                {
                                    // string name = ReadRegKey(Native.HKEY_CURRENT_USER, @"SYSTEM\CurrentControlSet\Control\MediaProperties\PrivateProperties\Joystick\OEM\VID_" + hidDevice.VID.ToString("X4") + "&PID_" + hidDevice.PID.ToString("X4"), Native.REGSTR_VAL_JOYOEMNAME);

                                    UnityEngine.Debug.Log("WinHIDInterface: " + hidDevice.Name + " Connected");

                                    ResolveDevice(hidDevice);
                                }
                                else
                                {
                                    UnityEngine.Debug.Log("WinHIDInterface: " + hidDevice.Name + " Already Connected.");
                                }
                            }
                            catch (Exception e)
                            {
                                UnityEngine.Debug.LogException(e);
                            }
                        }

                        break;
                }
            }

            return Native.DefWindowProcW(hWnd, msg, wParam, lParam);
        }



        /// <summary>
        /// Convert (WM_DEVICECHANGE)WM_MESSAGE pointer to data structure
        /// </summary>
        /// <param name="lParam"></param>
        /// <returns></returns>
        protected string PointerToDevicePath(IntPtr lParam)
        {
            Native.DEV_BROADCAST_DEVICEINTERFACE devBroadcastDeviceInterface =
                               new Native.DEV_BROADCAST_DEVICEINTERFACE();
            Native.DEV_BROADCAST_HDR devBroadcastHeader = new Native.DEV_BROADCAST_HDR();
            Marshal.PtrToStructure(lParam, devBroadcastHeader);

            Int32 stringSize = Convert.ToInt32((devBroadcastHeader.dbch_size - 32) / 2);
            Array.Resize(ref devBroadcastDeviceInterface.dbcc_name, stringSize);
            Marshal.PtrToStructure(lParam, devBroadcastDeviceInterface);
            return new String(devBroadcastDeviceInterface.dbcc_name, 0, stringSize);
        }


        /// <summary>
        /// Unregisters the window for USB device notifications
        /// </summary>
        public static void UnregisterHIDDeviceNotification()
        {
            if (notificationHandle != IntPtr.Zero)
                Native.UnregisterDeviceNotification(notificationHandle);

            notificationHandle = IntPtr.Zero;
        }








        public void Enumerate()
        {

						if (receiverWindowHandle == IntPtr.Zero){

								receiverWindowHandle = CreateReceiverWnd ();
			
							if (receiverWindowHandle != IntPtr.Zero)
									RegisterHIDDeviceNotification (receiverWindowHandle);
						}


            uint deviceCount = 0;
            var deviceSize = (uint)Marshal.SizeOf(typeof(Native.RawInputDeviceList));

            // first call retrieves the number of raw input devices
            var result = Native.GetRawInputDeviceList(
                IntPtr.Zero,
                ref deviceCount,
                deviceSize);



            if ((int)result == -1)
            {
                // call failed, 
                UnityEngine.Debug.LogError("WinHIDInterface failed to enumerate devices");

                return;
            }
            else if (deviceCount == 0)
            {
                // call failed, 
                UnityEngine.Debug.LogError("WinHIDInterface found no HID devices");

                return;
            }






            // allocates memory for an array of Win32.RawInputDeviceList
            IntPtr ptrDeviceList = Marshal.AllocHGlobal((int)(deviceSize * deviceCount));

            result = Native.GetRawInputDeviceList(
                ptrDeviceList,
                ref deviceCount,
                deviceSize);



            if ((int)result != -1)
            {
                Native.RawInputDeviceList rawInputDeviceList;
                // enumerates array of Win32.RawInputDeviceList,
                // and populates array of managed RawInputDevice objects
                for (var index = 0; index < deviceCount; index++)
                {

                    rawInputDeviceList = (Native.RawInputDeviceList)Marshal.PtrToStructure(
                        new IntPtr((ptrDeviceList.ToInt32() +
                                (deviceSize * index))),
                        typeof(Native.RawInputDeviceList));



                    if (rawInputDeviceList.DeviceType == Native.RawInputDeviceType.HumanInterfaceDevice)
                    {
                        HIDDevice hidDevice = CreateHIDDeviceFrom(rawInputDeviceList);

                        if(!__Generics.ContainsKey(hidDevice.PID))
                        ResolveDevice(hidDevice);
                    }

                }
            }

            Marshal.FreeHGlobal(ptrDeviceList);

        }



        /// <summary>
        /// Get Value of the Registry Key
        /// </summary>
        /// <param name="rootKey"></param>
        /// <param name="keyPath"></param>
        /// <param name="valueName"></param>
        /// <returns></returns>
        private string ReadRegKey(UIntPtr rootKey, string keyPath, string valueName)
        {
            UIntPtr hKey;

            if (Native.RegOpenKeyEx(rootKey, keyPath, 0, Native.KEY_READ, out hKey) == 0)
            {
                uint size = 1024;
                uint type;
                string keyValue = null;
                StringBuilder keyBuffer = new StringBuilder((int)size);

                if (Native.RegQueryValueEx(hKey, valueName, 0, out type, keyBuffer, ref size) == 0)
                    keyValue = keyBuffer.ToString();

                Native.RegCloseKey(hKey);

                return (keyValue);
            }

            return String.Empty;  // Return null if the value could not be read
        }





        /// <summary>
        /// 
        /// </summary>
        /// <param name="devicePath"></param>
        /// <returns></returns>
        protected HIDDevice CreateHIDDeviceFrom(string devicePath)
        {

            string[] Parts = devicePath.Split('#');

            if (Parts.Length >= 3)
            {
                // string DevType = Parts[0].Substring(Parts[0].IndexOf(@"?\") + 2);//HID
                string DeviceInstanceId = Parts[1];

                String[] VID_PID_Parts = DeviceInstanceId.Split('&');


                //if we need in later code expansion
                // string DeviceUniqueID = Parts[2];//{fas232fafs2345faf}



                // string RegPath = @"SYSTEM\CurrentControlSet\Enum\" + DevType + "\\" + DeviceInstanceId + "\\" + DeviceUniqueID;


                //return ReadRegKey(HKEY_LOCAL_MACHINE, RegPath, "FriendlyName")+ReadRegKey(HKEY_LOCAL_MACHINE, RegPath, "DeviceDesc");





               // string name = ReadRegKey(Native.HKEY_CURRENT_USER, @"SYSTEM\CurrentControlSet\Control\MediaProperties\PrivateProperties\Joystick\OEM\" + DeviceInstanceId, Native.REGSTR_VAL_JOYOEMNAME);

                string name = ReadRegKey(Native.HKEY_CURRENT_USER, @"SYSTEM\CurrentControlSet\Control\MediaProperties\PrivateProperties\Joystick\OEM\" + VID_PID_Parts[0] + "&"+ VID_PID_Parts[1], Native.REGSTR_VAL_JOYOEMNAME);
		




                //!!! deviceHandle set to IntPtr.Zero (think not needed in widows)
                return new GenericHIDDevice(__Generics.Count,Convert.ToInt32(VID_PID_Parts[0].Replace("VID_", ""), 16), Convert.ToInt32(VID_PID_Parts[1].Replace("PID_", ""), 16), IntPtr.Zero, this, devicePath, name);
            }

            return null;
        }


        protected HIDDevice CreateHIDDeviceFrom(Native.RawInputDeviceList rawInputDeviceList)
        {



            Native.DeviceInfo deviceInfo = GetDeviceInfo(rawInputDeviceList.DeviceHandle);
            UnityEngine.Debug.Log("PID:" + deviceInfo.HIDInfo.ProductID + " VID:" + deviceInfo.HIDInfo.VendorID);

            string devicePath = GetDevicePath(rawInputDeviceList.DeviceHandle);

            string name = ReadRegKey(Native.HKEY_CURRENT_USER, @"SYSTEM\CurrentControlSet\Control\MediaProperties\PrivateProperties\Joystick\OEM\" + "VID_" + deviceInfo.HIDInfo.VendorID.ToString("X4") + "&PID_" + deviceInfo.HIDInfo.ProductID.ToString("X4"), Native.REGSTR_VAL_JOYOEMNAME);
		


            return new GenericHIDDevice(__Generics.Count,Convert.ToInt32(deviceInfo.HIDInfo.VendorID), Convert.ToInt32(deviceInfo.HIDInfo.ProductID), rawInputDeviceList.DeviceHandle, this, devicePath, name);

            //this have problems with   
            // return GetHIDDeviceInfo(GetDevicePath(rawInputDeviceList.DeviceHandle));
        }






        private static IntPtr GetDeviceData(IntPtr deviceHandle, Native.RawInputDeviceInfoCommand command)
        {
            uint dataSize = 0;
            var ptrData = IntPtr.Zero;

            Native.GetRawInputDeviceInfo(
                deviceHandle,
                command,
                ptrData,
                ref dataSize);

            if (dataSize == 0) return IntPtr.Zero;

            ptrData = Marshal.AllocHGlobal((int)dataSize);

            var result = Native.GetRawInputDeviceInfo(
                deviceHandle,
                command,
                ptrData,
                ref dataSize);

            if (result == 0)
            {
                Marshal.FreeHGlobal(ptrData);
                return IntPtr.Zero;
            }

            return ptrData;
        }

        private static string GetDevicePath(IntPtr deviceHandle)
        {
            var ptrDeviceName = GetDeviceData(
                deviceHandle,
                Native.RawInputDeviceInfoCommand.DeviceName);

            if (ptrDeviceName == IntPtr.Zero)
            {
                return string.Empty;
            }

            var deviceName = Marshal.PtrToStringAnsi(ptrDeviceName);
            Marshal.FreeHGlobal(ptrDeviceName);
            return deviceName;
        }

        private static Native.DeviceInfo GetDeviceInfo(IntPtr deviceHandle)
        {
            var ptrDeviceInfo = GetDeviceData(
                deviceHandle,
                Native.RawInputDeviceInfoCommand.DeviceInfo);

            if (ptrDeviceInfo == IntPtr.Zero)
            {
                return new Native.DeviceInfo();
            }

            Native.DeviceInfo deviceInfo = (Native.DeviceInfo)Marshal.PtrToStructure(
                ptrDeviceInfo, typeof(Native.DeviceInfo));

            Marshal.FreeHGlobal(ptrDeviceInfo);
            return deviceInfo;
        }




        /// <summary>
        /// Try to attach compatible driver based on device PID and VID
        /// </summary>
		/// <param name="hidDevice"></param>
        protected void ResolveDevice(HIDDevice hidDevice)
        {

            IDevice joyDevice = null;
            
           

            //loop thru drivers and attach the driver to device if compatible
            if (__drivers != null)
                foreach (var driver in __drivers)
                {

                    

                    joyDevice = driver.ResolveDevice(hidDevice);
                    if (joyDevice != null)
                    {
                        joyDevice.Name = hidDevice.Name; 

	                    Generics[hidDevice.PID] = hidDevice;

					    DeviceConnectEvent(this,new DeviceEventArgs<IDevice>(joyDevice));
                   
				

                        Debug.Log("Device"+hidDevice.index+" PID:" + hidDevice.PID + " VID:" + hidDevice.VID + " attached to " + driver.GetType().ToString());

                        break;
                    }
                }

            if (joyDevice == null)
            {//set default driver as resolver if no custom driver match device
                joyDevice = defaultDriver.ResolveDevice(hidDevice);




                if (joyDevice != null)
                {
                    joyDevice.Name = hidDevice.Name;

	                Generics[hidDevice.PID] = hidDevice;

					DeviceConnectEvent(this,new DeviceEventArgs<IDevice>(joyDevice));

				
                   


                    Debug.Log("Device" + hidDevice.index + "  PID:" + hidDevice.PID + " VID:" + hidDevice.VID + " attached to " + __defaultJoystickDriver.GetType().ToString() + " Path:" + hidDevice.DevicePath + " Name:" + joyDevice.Name);

                }
                else
                {
                    Debug.LogWarning("Device PID:" + hidDevice.PID + " VID:" + hidDevice.VID + " not found compatible driver thru WinHIDInterface!");

                }

            }


        }

       










        public void Dispose()
        {
            UnityEngine.Debug.Log("Try to dispose notificationHandle");
            UnregisterHIDDeviceNotification();

            UnityEngine.Debug.Log("Try to dispose receiverWindowHandle");


            if (receiverWindowHandle != IntPtr.Zero)
            {
				try{
               			 UnityEngine.Debug.Log("Destroy Receiver" + Native.DestroyWindow(receiverWindowHandle));

					//TODO test with this (issue when open  close InputMapper in Editor twice
					//Native.PostMessage(new HandleRef(this,this.receiverWindowHandle),Native.WM_CLOSE,IntPtr.Zero,IntPtr.Zero);
               			 receiverWindowHandle = IntPtr.Zero;


				}catch(Exception ex){

					//UnityEngine.Debug.LogException(ex);
					UnityEngine.Debug.LogError(Native.GetLastError());
				}

                //
            }

           

           
            foreach (KeyValuePair<int, HIDDevice> entry in Generics)
            {
                entry.Value.Dispose();
            }

            Generics.Clear();

            if(__drivers!=null)
            __drivers.Clear();

            UnityEngine.Debug.Log("Dispose WinHIDInterface");
        }







      
    }

}

#endif