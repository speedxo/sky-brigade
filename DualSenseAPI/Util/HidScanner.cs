using System.Collections.Generic;
using System.Threading.Tasks;

using Device.Net;
using HidSharp;
//using Hid.Net.Windows;

namespace DualSenseAPI.Util
{
    /// <summary>
    /// Utilities to scann for DualSense controllers on HID.
    /// </summary>
    internal class HidScanner
    {
        private static HidScanner? _instance = null;
        /// <summary>
        /// Singleton HidScanner instance.
        /// </summary>
        internal static HidScanner Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new HidScanner();
                }
                return _instance;
            }
        }
        /// <summary>
        /// Lists connected devices.
        /// </summary>
        /// <returns>An enumerable of connected devices.</returns>
        public static IEnumerable<HidDevice> ListDevices()
        { 
            return DeviceList.Local.GetHidDevices(1356, 3302);
        }   
    }
}
