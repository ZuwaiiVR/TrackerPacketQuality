using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HidLibrary;
using System.Threading;
namespace TrackerPacketQuality
{
   public class Tracker_info : IEquatable<Tracker_info>
    {
        public HidDevice _device;
        private Thread m_Thread;
        private Thread hid_Thread;
        public int ID = new int();
        public int package_quality { get; set; }
        private string dev_path = "";
        private int totalpackages_s = 0;
        private int totalpackages_t = 0;
        
        public void stopThread()
        {
            m_Thread.Abort();
        }
        public bool Equals(Tracker_info other)
        {
            if (other == null) return false;
            return true;
        }
        public void init(string device_path,int id)
        {
            dev_path = device_path;
            ID = id;
            hid_Thread = new Thread(() => h_thread(device_path)) ;
            hid_Thread.Start();
            m_Thread = new Thread(ThreadLoop);
            m_Thread.Start();
        }

        private void h_thread(string device_path)
        {
            _device = HidDevices.GetDevice(device_path);
            _device.OpenDevice();
            _device.MonitorDeviceEvents = true;
            _device.Removed += DeviceRemovedHandler;
            _device.ReadReport(OnReport);
        }

        private void OnReport(HidReport report)
        {
            totalpackages_s++;
            _device.ReadReport(OnReport);
        }

        private void DeviceRemovedHandler()
        {
            Console.WriteLine(String.Format("Device {0} removed.\n Please restart to refresh.",ID));
            m_Thread.Abort();
            package_quality = -1;
        }
        private void ThreadLoop()
        {
            while (m_Thread != null)
            {
                totalpackages_t = totalpackages_s;
                totalpackages_s = 0;
                package_quality = totalpackages_t;  
                Thread.Sleep(1000);
            }
        }
    }


}
