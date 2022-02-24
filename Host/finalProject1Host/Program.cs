using System;
using System.Text;
using Intel.Dal;
using System.Net.Sockets;
using System.Net;
using System.Linq;

namespace finalProject1Host
{
    class Program
    {
        static string UserID = null;
        static string userPassword = null;
        static byte[] userAuthority = new byte[1];
        

        static void Main(string[] args)
    {
#if AMULET
        // When compiled for Amulet the Jhi.DisableDllValidation flag is set to true 
		// in order to load the JHI.dll without DLL verification.
        // This is done because the JHI.dll is not in the regular JHI installation folder, 
		// and therefore will not be found by the JhiSharp.dll.
        // After disabling the .dll validation, the JHI.dll will be loaded using the Windows search path
		// and not by the JhiSharp.dll (see http://msdn.microsoft.com/en-us/library/7d83bc18(v=vs.100).aspx for 
		// details on the search path that is used by Windows to locate a DLL) 
        // In this case the JHI.dll will be loaded from the $(OutDir) folder (bin\Amulet by default),
		// which is the directory where the executable module for the current process is located.
        // The JHI.dll was placed in the bin\Amulet folder during project build.
        Jhi.DisableDllValidation = true;
#endif

        Jhi jhi = Jhi.Instance;
        JhiSession session;

        // This is the UUID of this Trusted Application (TA).
        //The UUID is the same value as the applet.id field in the Intel(R) DAL Trusted Application manifest.
        string appletID = "5d0e958b-f595-40c3-8253-9b4a85d020ec";
        // This is the path to the Intel Intel(R) DAL Trusted Application .dalp file that was created by the Intel(R) DAL Eclipse plug-in.
        string appletPath = "L:\\last semester\\finalProject1\\finalProject1\\bin\\finalProject1.dalp";
        //string appletPath = "L:\\last semester\\finalProject1\\finalProject1\\bin\\finalProject1-debug.dalp";


        // Install the Trusted Application
        Console.WriteLine("Installing the applet.");
        jhi.Install(appletID, appletPath);

        // Start a session with the Trusted Application
        byte[] initBuffer = new byte[] { }; // Data to send to the applet onInit function
        Console.WriteLine("Opening a session.");
        jhi.CreateSession(appletID, JHI_SESSION_FLAGS.None, initBuffer, out session);

            //Performs a function according to the user's request
            ActionToPerform(jhi, session);

            // Close the session
            Console.WriteLine("Closing the session.");
            jhi.CloseSession(session);

            //Uninstall the Trusted Application
            Console.WriteLine("Uninstalling the applet.");
            jhi.Uninstall(appletID);

            Console.WriteLine("Press Enter to finish.");
            Console.Read();
        }

        //Receives the command off which action to preform and sends to the right function for handling
        static void ActionToPerform(Jhi jhi, JhiSession session)
        {
            int end = 1;
            int cmdId; // The ID of the command to be performed by the TA
            while (end != 0)
            {
                Console.WriteLine("Please enter the operation you would like to perform: 0 - to exit, 1 - Register, 2 - Login, 3 - Logout, 4 - ResetPassword");
                cmdId = int.Parse(Console.ReadLine()); // receives the operation command the user wants to preform
                switch (cmdId)
                {
                    case 0:
                        end = 0;
                        break;
                    case 1:
                        Register(jhi, session);
                        break;
                    case 2:
                        bool LoggedIn = Login(jhi, session);
                        if (LoggedIn)//if the user logged in successfully to the system

                            LoggedInActions(jhi, session);//actions that can be done only as a logged in user
                        break;
                    case 3:
                        Logout(jhi, session);
                        break;
                    case 4:
                        ResetPassword(jhi, session);
                        break;

                }
            }

        }

        //Allows a new worker in the hospital to register to the system
        static void Register(Jhi jhi, JhiSession session)
        {
            // Send and Receive data to/from the Trusted Application
            byte[] sendBuff = new byte[16]; // A message to send to the TA
            byte[] recvBuff = new byte[4]; // old nothing
            byte[] userAuthority1 = new byte[1];
            int responseCode = 0; // The return value that the TA provides using the IntelApplet.setResponseCode method
            Console.WriteLine("Please Enter your authority level: (choose number between 1-3 when 3 is highest) ");
            userAuthority1[0] = Convert.ToByte(int.Parse(Console.ReadLine()));
            string send_buff = getUserId() + getUserPassword(); //size: 1+9+6
            sendBuff = UTF32Encoding.UTF8.GetBytes(send_buff);
            sendBuff = userAuthority1.Concat(sendBuff).ToArray();//encodes the data we want to send to applet 
            jhi.SendAndRecv2(session, 1, sendBuff, ref recvBuff, out responseCode);
            if (responseCode == 1)
            {
                Console.WriteLine("please enter the path to your fingerPrint: ");
                string path = Console.ReadLine(); // @"L:\last semester\finalProject1\register_fingerPrint.jpg";
                byte[] path1 = new byte[4];
                path1 = Encoding.ASCII.GetBytes(path);
                byte[] response = createSocket(3,null, path1);
                string response1 = Encoding.ASCII.GetString(response).Substring(0,1);
                if(response1=="1")
                    Console.Out.WriteLine("Registration was completed succesfully");
            }
            else
                Console.Out.WriteLine("you are already registered!"); 

        }

        //Allows an already registered user to login to his account
        static bool Login(Jhi jhi, JhiSession session)
        {
            // Send and Receive data to/from the Trusted Application
            byte[] sendBuff = new byte[16]; // A message to send to the TA
            byte[] recvBuff = new byte[4]; // A buffer to hold the output data from the TA
            Console.WriteLine("Please Enter your ID:");
            string UserID = Console.ReadLine();
            Console.WriteLine("Please Enter your password:");
            string userPassword = Console.ReadLine();
            int responseCode = 0; // The return value that the TA provides using the IntelApplet.setResponseCode method
            string send_buff = UserID + userPassword;//the message were gonna send is the id and the password of the user
            sendBuff = UTF32Encoding.UTF8.GetBytes(send_buff); // A message to send to the TA
            jhi.SendAndRecv2(session, 2, sendBuff, ref recvBuff, out responseCode);//sends the password and the user id to applet  where it will be verified

            if (responseCode == 1)
            {
                Console.Out.WriteLine("your password is valid!");
                Program.userAuthority[0] = recvBuff[0];

                Console.Out.WriteLine("Please enter your fingerprint - path:");
                //first match -meant to succeed
                string path = Console.ReadLine(); //@"L:\last semester\finalProject1\good_fingerPrint.jpg"  @"L:\last semester\finalProject1\bad_fingerPrint.jpg"
                byte[] path1 = new byte[4];
                path1 = Encoding.ASCII.GetBytes(path);
                byte[] response = createSocket(4, null,path1);
                string response1 = Encoding.ASCII.GetString(response).Substring(0, 1);
                if (response1 == "1")
                {
                    Console.WriteLine("your fingerprint match!");
                    Console.Out.WriteLine("The user was registered successfully");
                    return true;
                }
                else
                {
                    Console.WriteLine("your fingerprint doesnt match!");
                    Console.Out.WriteLine("registretion faild");
                    return true;
                }
            }

            
            else if (responseCode == 0)
            {
                Console.Out.WriteLine("your password is incorrect");
                return false;

            }

            else
            {
                Console.Out.WriteLine("you are logged in already"); //appletלשים לב להתאים את זה ב
                return false;

            }
        }


        //logs out as long as the user is already logged in
        static void Logout(Jhi jhi, JhiSession session)
        {
            // Send and Receive data to/from the Trusted Application
            byte[] sendBuff = new byte[8]; // A message to send to the TA
            byte[] recvBuff = new byte[2000]; // A buffer to hold the output data from the TA
            int responseCode = 0; // The return value that the TA provides using the IntelApplet.setResponseCode method
                                  // string send_buff = getUserId() + getUserPassword();//the message were gonna send is the id and the password of the user
                                  //sendBuff = UTF32Encoding.UTF8.GetBytes(send_buff); // A message to send to the TA
            jhi.SendAndRecv2(session, 3, sendBuff, ref recvBuff, out responseCode);
            if (responseCode == 1)
                Console.Out.WriteLine("Logout completed succesfully");
            else
                Console.Out.WriteLine("cannot logout");

        }

        //allows an already logged in user to change his password
        static void ResetPassword(Jhi jhi, JhiSession session)
        {
            byte[] sendBuff = new byte[8]; // A message to send to the TA
            byte[] recvBuff = new byte[2000]; // A buffer to hold the output data from the TA
            int responseCode = 0; // The return value that the TA provides using the IntelApplet.setResponseCode method
            Console.WriteLine("Enter your new password:");
            string newPass = Console.ReadLine();
            string send_buff = Program.userAuthority + getUserId() + newPass;//the message were gonna send is the id and the password of the user
            sendBuff = UTF32Encoding.UTF8.GetBytes(send_buff); // A message to send to the TA
            jhi.SendAndRecv2(session, 4, sendBuff, ref recvBuff, out responseCode);
            if (responseCode == 1)
                Console.Out.WriteLine("your password changed succesfully");
            else
                Console.Out.WriteLine("the operation fail try to login first");

        }


        //functions that can be performed once a user is logged in and verified
        public static void LoggedInActions(Jhi jhi, JhiSession session)
        {
            Console.WriteLine("Please enter the operation you would like to perform: 0 - exit, 1 - View a patient's data, 2 - Add a new patient to data base");
            int cmdId = int.Parse(Console.ReadLine()); // receives the operation command the user wants to preform
            while (cmdId != 0)
            {
                switch (cmdId)
                {
                    case 0:
                        break;
                    case 1:
                        ViewPatientData(jhi, session);
                        break;
                    case 2:
                        AddPatientData(jhi, session);
                        break;
                }
                Console.WriteLine("Please enter the operation you would like to perform: 0 - exit, 1 - View a patient's data, 2 - Add a new patient to data base");
                cmdId = int.Parse(Console.ReadLine()); // receives the operation command the user wants to preform
            }
          
        }


        public static byte[] createSocket(int operation, byte[] patient_id, byte[] data) // converted all from string to byte[] need to check if works with server before changing for view patient data
        {

            try
            {
                // Connect to a Remote server
                // Get Host IP Address that is used to establish a connection
                // In this case, we get one IP address of localhost that is IP : 127.0.0.1
                // If a host has multiple addresses, you will get a list of addresses
                IPHostEntry host = Dns.GetHostEntry("localhost");
                IPAddress ipAddress = host.AddressList[0];
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, 11000);

                // Create a TCP/IP  socket.
                Socket sender = new Socket(ipAddress.AddressFamily,
                SocketType.Stream, ProtocolType.Tcp);

                // Connect the socket to the remote endpoint. Catch any errors.
                try
                {

                    // Connect to Remote EndPoint
                    sender.Connect(remoteEP);

                    Console.WriteLine("connecting to serever");
                    byte[] op = Encoding.ASCII.GetBytes(operation.ToString());
                    if (operation == 0) //add new patient 
                    {
                        byte[] serverResponse = new byte[1024]; // message of success
                        byte[] myMsg = op.Concat(patient_id).Concat(data).ToArray();//send the op + id+ data encrypted with spaces between each item
                        // Send the data through the socket.
                        int bytesSent = sender.Send(myMsg);
                        // Receive the response from the remote device.
                        int bytesRec = sender.Receive(serverResponse);
                        byte[] response = new byte[bytesRec];
                        Array.Copy(serverResponse, 0, response, 0, bytesRec);
                        // Release the socket.
                        sender.Shutdown(SocketShutdown.Both);
                        sender.Close();
                        return response;
                    }
                    else if (operation == 1)//view ptient data
                    {
                        byte[] patientData = new byte[1024];   //data that we will recieve from the server with the patients info or message of success
                        byte[] msg = op.Concat(patient_id).Concat(data).ToArray();   //create nessage
                        int bytesSent = sender.Send(msg);  //send the op + id+ data encrypted with spaces between each item
                        //Receive the response from the remote device.
                        int bytesRec = sender.Receive(patientData);
                        //Release the socket.
                        byte[] response = new byte[bytesRec];
                        Array.Copy(patientData, 0, response, 0, bytesRec);
                        sender.Shutdown(SocketShutdown.Both);
                        sender.Close();
                        return response;
                    }
                    
                    else
                    {
                        byte[] msg = op.Concat(Encoding.ASCII.GetBytes(" ")).Concat(data).ToArray();   //create nessage
                        byte[] response = new byte[1024];
                        int bytesSent = sender.Send(msg);  //send the op + id+ data encrypted with spaces between each item
                        int bytesRec = sender.Receive(response);
                        return response;


                    }

                }
                catch (ArgumentNullException ane)
                {
                    Console.WriteLine("ArgumentNullException : {0}", ane.ToString());
                }
                catch (SocketException se)
                {
                    Console.WriteLine("SocketException : {0}", se.ToString());
                }
                catch (Exception e)
                {
                    Console.WriteLine("Unexpected exception : {0}", e.ToString());
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            return null;
        }





        //Allows the user to view data of a specific patient after hes logged in. 
        public static void ViewPatientData(Jhi jhi, JhiSession session)
        {
            Console.WriteLine("Please enter the patient ID you would like to view data for: ");
            string patientId = " " + Console.ReadLine();
            byte[] id = Encoding.ASCII.GetBytes(patientId);
            Console.WriteLine("Please enter the data you would like to view for this patient: id, firstName, lastName, sex, phoneNumber, age, lastHeight, weight, lastBloodPressure, medicalHistory, lastBloodTest ");
            string data = " " + Console.ReadLine();
            byte[] requestedData = Encoding.ASCII.GetBytes(data);
            //send the patient id and the data we want to view for him to the server which will send back to us encrypted
            byte[] response = createSocket(1, id, requestedData);//encrypted response from the server
            byte[] dataAuthority = new byte[1];
            dataAuthority[0] = response[0];//the authority level of the data that is requested.
            byte[] encryptedData = new byte[response.Length-1];
            Array.Copy(response, 1, encryptedData, 0, response.Length - 1);
            string decryptedData = DecryptedData(encryptedData, dataAuthority, jhi, session);
            if (decryptedData == null)
                Console.WriteLine("Couldnt retreive the data you requested because your authority level dosent match");
            else
                Console.WriteLine("The " + data + " of patient: " + patientId + " is: " + decryptedData);
        }



        //Adds a new patient to data base after users logged in
        public static void AddPatientData(Jhi jhi, JhiSession session)
        { //recieve infor for patient from user and encrypt by sending to applet
            Console.WriteLine("Please enter the patient ID you would like add : ");
            string ID = " " + Console.ReadLine();
            byte[] eID = new byte[5];
            eID = Encoding.ASCII.GetBytes(ID);
            //firstName
            Console.WriteLine("Please enter the first name: ");
            string firstName = Console.ReadLine();
            byte[] fi = Encoding.ASCII.GetBytes(firstName);
            byte[] efirstName1 = EncryptInfo(jhi, session, firstName);
            byte[] efirstName = Encoding.ASCII.GetBytes(" ").Concat(efirstName1).ToArray();
            //lastName
            Console.WriteLine("Please enter the last name: ");
            string lastName = Console.ReadLine();
            byte[] elastName1 = EncryptInfo(jhi, session, lastName);
            byte[] elastName = Encoding.ASCII.GetBytes(" ").Concat(elastName1).ToArray();
            // sex 
            Console.WriteLine("Please enter the sex: ");
            string sex = Console.ReadLine();
            byte[] eSex1 = EncryptInfo(jhi, session, sex);
            byte[] eSex = Encoding.ASCII.GetBytes(" ").Concat(eSex1).ToArray();
            Console.WriteLine("Please enter the phone number:");
            //phoneNumber
            string phoneNumber = Console.ReadLine();
            byte[] ePhoneNumber1 = EncryptInfo(jhi, session, phoneNumber);
            byte[] ePhoneNumber = Encoding.ASCII.GetBytes(" ").Concat(ePhoneNumber1).ToArray();
            Console.WriteLine("Please enter the age:");
            //age 
            string age = Console.ReadLine();
            byte[] eAge1 = EncryptInfo(jhi, session, age);
            byte[] eAge = Encoding.ASCII.GetBytes(" ").Concat(eAge1).ToArray();
            Console.WriteLine("Please enter the  height:");
            //lastHeight 
            string lastHeight = Console.ReadLine();
            byte[] elastHeight1 = EncryptInfo(jhi, session, lastHeight);
            byte[] elastHeight = Encoding.ASCII.GetBytes(" ").Concat(elastHeight1).ToArray();
            Console.WriteLine("Please enter the weight:");
            //weight 
            string weight = Console.ReadLine();
            byte[] eWeight1 = EncryptInfo(jhi, session, weight);
            byte[] eWeight = Encoding.ASCII.GetBytes(" ").Concat(eWeight1).ToArray();
            Console.WriteLine("Please enter the last blood test results:");
            //LastBloodTest 
            string LastBloodTest = Console.ReadLine();
            byte[] eLastBloodTest1 = EncryptInfo(jhi, session, LastBloodTest);
            byte[] eLastBloodTest = Encoding.ASCII.GetBytes(" ").Concat(eLastBloodTest1).ToArray();
            Console.WriteLine("Please enter the medical history:");
            //medicalHistory
            string medicalHistory  = Console.ReadLine();
            byte[] emedicalHistory1 = EncryptInfo(jhi, session, medicalHistory);
            byte[] emedicalHistory = Encoding.ASCII.GetBytes(" ").Concat(emedicalHistory1).ToArray();
            Console.WriteLine("Please enter the last blood pressure results:");
            //lastBloodPressure 
            string lastBloodPressure = Console.ReadLine();
            byte[] elastBloodPressure1 = EncryptInfo(jhi, session, lastBloodPressure);
            byte[] elastBloodPressure = Encoding.ASCII.GetBytes(" ").Concat(elastBloodPressure1).ToArray();
            //combine encrypted data into one variable -10 stuff +id that isnt encrypted

            byte[] eInfo = efirstName.Concat(elastName).Concat(eSex).Concat(ePhoneNumber).Concat(eAge).Concat(elastHeight).Concat(eWeight).Concat(eLastBloodTest).Concat(emedicalHistory).Concat(elastBloodPressure).ToArray();
            byte[] response = createSocket(0, eID, eInfo);
            if (response == null)
                Console.WriteLine("Couldnt add the patient to data base");
            else
                Console.WriteLine(UTF32Encoding.UTF8.GetString(response));


        }


        //will recieve a string which contains the patients data and will encrypt it by sending to applet
        public static byte[] EncryptInfo(Jhi jhi, JhiSession session, string patientData)
        {
            Console.WriteLine("encrypting data");
            byte[] recvBuff = new byte[2000]; // A buffer to hold the output data from the TA
            int responseCode = 0;
            byte[] sendBuff = UTF32Encoding.UTF8.GetBytes(patientData);//remember that authority level is only one digit
            jhi.SendAndRecv2(session, 5, createArrayToSend(sendBuff), ref recvBuff, out responseCode);//send the data for encryption to the applet
            if (responseCode == 1)
                return recvBuff;
            else
            {
                Console.WriteLine("Could not encrypt the data ");
                throw new Exception();
            }

        }


        //Send the encrypted data along with authority level to the applet for decryption, if we added a new patient authority level will be empty string
        public static string DecryptedData(byte[] patientData, byte[] authorityLevelData, Jhi jhi, JhiSession session)
        {
            Console.WriteLine("decrypting data");
            byte[] recvBuff = new byte[2000]; // A buffer to hold the output data from the TA
            int responseCode = 0;
            //the applet will send back decrypted data only if the authority level matches
            byte[] sendBuff = authorityLevelData.Concat(createArrayToSend(patientData)).ToArray();//remember that authority level is only one digit
            jhi.SendAndRecv2(session, 6, sendBuff, ref recvBuff, out responseCode);//sends to applett for decryption
            if (responseCode == 1)
                return UTF32Encoding.UTF8.GetString(recvBuff);
            else
                return null;

        }


        //converts size of arrary to muliply with 16 
        public static byte[] createArrayToSend(byte[] data)
        {
            int size = data.Length;
            int a = size / 16;
            int b = size % 16;
            if (b > 0)
                a++;
            byte[] dataToSend = new byte[a * 16];
            Array.Copy(data, dataToSend, size);
            return dataToSend;
        }



        public static string getUserId()
        {
            if (Program.UserID == null)
            {
                Console.WriteLine("Please Enter your ID:");
                Program.UserID = Console.ReadLine();

            }
            return Program.UserID;

        }

        public static string getUserPassword()
        {
            if (Program.userPassword == null)
            {
                Console.WriteLine("Please Enter your password:");
                Program.userPassword = Console.ReadLine();
            }
            return Program.userPassword;
        }
    }
}