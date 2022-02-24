using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using SourceAFIS;
using System.IO;

namespace projectServer
{
    class Program
    {
        static FingerprintTemplate probe = null;
        static FingerprintTemplate candidate = null;
        static InformationArchive patientsDataBase = new InformationArchive();

        static void Main(string[] args)
        {
           // fingerprint();
            createSocket();
        }


        static void createSocket()
        {
            IPHostEntry host = Dns.GetHostEntry("localhost");
            IPAddress ipAddress = host.AddressList[0];
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 11000);
            try
            {
                // Create a Socket that will use Tcp protocol
                Socket listener = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                // A Socket must be associated with an endpoint using the Bind method
                listener.Bind(localEndPoint);
                // Specify how many requests a Socket can listen before it gives Server busy response.
                // We will listen 10 requests at a time
                listener.Listen(10);
                while (true)
                {
                    Console.WriteLine("Waiting for a connection...");
                    Socket handler = listener.Accept();

                    // Incoming data from the client.
                    string data = null;
                    byte[] bytes = null;
                    bytes = new byte[1024];
                    int bytesRec = handler.Receive(bytes);
                    data = Encoding.ASCII.GetString(bytes, 0, bytesRec);
                   // Console.WriteLine("Text received : {0}");
                    String[] words = data.Split(' ');
                    int cmd = int.Parse(words[0]);
                    string path = data.Substring(2,data.Length-2);
                    if (cmd == 1)//patient information is requested
                    {
                        int userId = int.Parse(words[1]);
                        string requestedItem = words[2];
                        HandleRequest(userId, requestedItem, handler);
                    }
                    else if (cmd==0) //patient information need to be saved
                    {
                        int userId = int.Parse(words[1]);
                        savePatientInformation(userId, bytes, bytesRec, handler);
                    }
                    else if (cmd == 3)
                    {
                        register(handler, path);//subString();
                    }
                    else
                    {
                        login(handler,path);
                    }
                    handler.Shutdown(SocketShutdown.Both);
                    handler.Close();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private static void login(Socket handler, string path)
        {
            var options = new FingerprintImageOptions { Dpi = 500 };
            candidate = new FingerprintTemplate(new FingerprintImage(200, 200, File.ReadAllBytes(path), options));
            double score = new FingerprintMatcher(probe).Match(candidate);
            double threshold = 40;
            bool matches = score >= threshold;
            if (matches == true)
            {
                handler.Send(Encoding.ASCII.GetBytes("1"));
            }
            else
            {
               Console.WriteLine("no match:(");
            }
           
        }

        private static void register(Socket handler,string path)
        {
            var options = new FingerprintImageOptions { Dpi = 500 };

            probe = new FingerprintTemplate( new FingerprintImage(200, 200, File.ReadAllBytes(path), options));
            handler.Send(Encoding.ASCII.GetBytes("1"));
        }


        static void HandleRequest(int userId, string requestedItem, Socket handler)
        {
            byte[] information = patientsDataBase.findItem(userId, requestedItem);
            handler.Send(information);
        }


        static void savePatientInformation(int userId, byte[] patientInformation, int length, Socket handler)
        {

            int[] indexes = new int[11];
            int[] len = new int[10];
            int j = 0;
            for(int i = 2; i<length; i++)
            {
                if (patientInformation[i] == 32)
                {
                    indexes[j] = i;
                    j++;
                }


            }
            indexes[10] = length;
            for(int i = 0; i < 10; i++)
            {
                len[i] = indexes[i + 1] - indexes[i] - 1;
            }
            
            int Id = userId;
            byte[] FirstName = SubArray(patientInformation,indexes[0]+1,len[0]);
            byte[] LastName = SubArray(patientInformation, indexes[1] + 1, len[1]);
            byte[] Sex = SubArray(patientInformation, indexes[2] + 1, len[2]);//changed to string from char
            byte[] PhoneNumber = SubArray(patientInformation, indexes[3] + 1, len[3]);
            byte[] Age = SubArray(patientInformation, indexes[4] + 1, len[4]);
            byte[] Height = SubArray(patientInformation, indexes[5] + 1, len[5]);
            byte[] Weight = SubArray(patientInformation, indexes[6] + 1, len[6]);
            byte[] LastBloodTest = SubArray(patientInformation, indexes[7] + 1, len[7]);
            byte[] MedicalHistory = SubArray(patientInformation, indexes[8] + 1, len[8]);
            byte[] LastBloodPressure = SubArray(patientInformation, indexes[9] + 1, len[9]);
            patientsDataBase.addPatient(Id, FirstName, LastName, Sex, PhoneNumber, Age, Height, Weight, LastBloodTest, MedicalHistory, LastBloodPressure);
            handler.Send(Encoding.ASCII.GetBytes("The patient data was added successfuly"));

           
        }

        static byte[] SubArray(byte[] array, int offset, int length)
        {
            byte[] result = new byte[length];
            Array.Copy(array, offset, result, 0, length);
            return result;
        }

        static void fingerprint()
        {
            var options = new FingerprintImageOptions { Dpi = 500 };

            var probe = new FingerprintTemplate(
                new FingerprintImage(
                    200, 200, File.ReadAllBytes(@"C:\Users\Owner\Documents\Visual Studio 2015\Projects\HospitalHost\HospitalHost\registered_FP.jpg"), options));
            //first match -meant to succeed
            var candidate =
                new FingerprintTemplate(new FingerprintImage(
                    200, 200, File.ReadAllBytes(@"C:\Users\Owner\Documents\Visual Studio 2015\Projects\HospitalHost\HospitalHost\input_good.jpg"), options));
            double score = new FingerprintMatcher(probe).Match(candidate);
            double threshold = 40;
            bool matches = score >= threshold;
            if (matches == true)
            {
                Console.WriteLine("Theres a match!");
            }
            else
            {
                Console.WriteLine("no match:(");
            }
            //second match-not succeed
            var candidate2 =
             new FingerprintTemplate(new FingerprintImage(
                 200, 200, File.ReadAllBytes(@"C:\Users\Owner\Documents\Visual Studio 2015\Projects\HospitalHost\HospitalHost\input_bad.jpg"), options));
            double score2 = new FingerprintMatcher(probe).Match(candidate2);
            bool matches2 = score2 >= threshold;
            if (matches2 == true)
            {
                Console.WriteLine("Theres a match!");
            }
            else
            {
                Console.WriteLine("no match:(");
            }
        }
    }



}

