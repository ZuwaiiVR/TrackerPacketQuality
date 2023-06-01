using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HidLibrary;
using System.Threading;
namespace TrackerPacketQuality
{
    class Program
    {
        private const int Vid_valve = 0x28DE;  //Valve Product
        private const int Pid_valve = 0x2102; // Valve Radio / Tundra McDongle / NRF52840 Dongles
        private const int Pid_htc = 0x2101; // Watchman NRF21UL01

        static bool exit = true;
        private static Thread m_Thread;

        static List<Tracker_info> T_info = new List<Tracker_info>();

        static void Main(string[] args)
        {
            var devices = HidDevices.Enumerate(Vid_valve);
            List<HidDevice> _hidDev = new List<HidDevice>();
            _hidDev = devices.ToList();
            int dev_count = 0;
            foreach (var list in _hidDev) //list all devices from product
            {
                if (list.Attributes.ProductId == Pid_valve || list.Attributes.ProductId == Pid_htc)
                {
                    list.ReadProduct(out byte[] _product);
                    list.ReadSerialNumber(out byte[] _serial);
                    Console.Write(String.Format("{0} \t{1} \n", Encoding.Unicode.GetString(_product).Trim((char)0x00), Encoding.Unicode.GetString(_serial).Trim((char)0x00)));
                    // Console.WriteLine(_hidDev.ToList().IndexOf(list));
                    T_info.Add(new Tracker_info());
                    T_info[dev_count].init(list.DevicePath, dev_count); // _hidDev.ToList().IndexOf(list)
                    dev_count++;
                }
            }

            Console.WriteLine(String.Format("\nFound {0} dongles!", T_info.Count));
            Console.WriteLine("q to exit.\n");
            m_Thread = new Thread(ThreadMainLoop);
            m_Thread.Start();
            while (exit)
            {
                switch (Console.Read())
                {
                    case 'q':
                        m_Thread.Abort();
                        foreach (var th in T_info)
                        {
                            th.stopThread();
                        }
                        Thread.Sleep(400);
                        exit = false;
                        break;
                }
            }
        }
        private static void ThreadMainLoop()
        {
            Console.Write(DateTime.Now.ToString("HH:mm:ss") + " |");
            foreach (var trackerinfo in T_info)
            {
                Console.Write(String.Format("ID{1}\t", DateTime.Now, trackerinfo.ID));
            }
            Console.WriteLine("\n");
            
            while (m_Thread != null)
            {
                Console.Write(DateTime.Now.ToString("HH:mm:ss") + " |");
                int totalpackages = 0;
                foreach (var trackerinfo in T_info)
                {
                    totalpackages += trackerinfo.package_quality;
                    Console.Write(trackerinfo.package_quality + "\t");
                }
                Console.WriteLine(" ");
                WriteTotal("Total Packages=" + totalpackages + " pps");
                Thread.Sleep(1000);
            }
        }

        protected static void WriteTotal(string s)
        {
            int origRow = Console.CursorTop;
            int origCol = Console.CursorLeft;
            int width = Console.WindowWidth;
            int x = 10;
            x = x % width;
            try
            {
                Console.SetCursorPosition(x, origRow);
                Console.Write(s);
            }
            catch (ArgumentOutOfRangeException e)
            {

            }
            finally
            {
                try
                {
                    Console.SetCursorPosition(origCol, origRow);
                }
                catch (ArgumentOutOfRangeException e)
                {
                }
            }

        }
    }
}
