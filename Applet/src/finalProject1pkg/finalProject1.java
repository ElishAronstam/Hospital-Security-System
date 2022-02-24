package finalProject1pkg;

import com.intel.util.*;
import com.intel.crypto.Random;
//
// Implementation of DAL Trusted Application: Hospital 
//
// **************************************************************************************************
// NOTE:  This default Trusted Application implementation is intended for DAL API Level 7 and above
// **************************************************************************************************
import com.intel.crypto.SymmetricBlockCipherAlg;
import com.intel.langutil.ArrayUtils;
import java.lang.Object;
import java.lang.String;

public class finalProject1 extends IntelApplet {
	//static variables
			static SymmetricBlockCipherAlg instance = null;
			static Boolean ISREGISTER =false;
		    static Boolean ISLOGIN =false;
		    static Boolean KEY_GENERETED =false;
		    static byte[] USER_AUTHORITY = new byte[1];

	/**
	 * This method will be called by the VM when a new session is opened to the Trusted Application 
	 * and this Trusted Application instance is being created to handle the new session.
	 * This method cannot provide response data and therefore calling
	 * setResponse or setResponseCode methods from it will throw a NullPointerException.
	 * 
	 * @param	request	the input data sent to the Trusted Application during session creation
	 * 
	 * @return	APPLET_SUCCESS if the operation was processed successfully, 
	 * 		any other error status code otherwise (note that all error codes will be
	 * 		treated similarly by the VM by sending "cancel" error code to the SW application).
	 */
		    public int onInit(byte[] request) {
				DebugPrint.printString("Hello, DAL!");
				return APPLET_SUCCESS;
			}
			
			/**
			 * This method will be called by the VM to handle a command sent to this
			 * Trusted Application instance.
			 * 
			 * @param	commandId	the command ID (Trusted Application specific) 
			 * @param	request		the input data for this command 
			 * @return	the return value should not be used by the applet
			 */
			public int invokeCommand(int commandId, byte[] data) {
				if(KEY_GENERETED==false)
				{	
					generateSymmetricKey();
			    	KEY_GENERETED = true;
			    }
				
				DebugPrint.printString("Received command Id: " + commandId + ". ");
				if(data != null)
				{
					DebugPrint.printString("Received buffer/data:");
					DebugPrint.printBuffer(data);
					switch (commandId)
					{
			            case 1:
			                Register(data);
			                break;
			            case 2:
			                Login(data);
			                break;
			            case 3:
			                Logout(data);//make sure to send the user_id also on the other side
			                break;
			            case 4:
			             	ResetPassword(data);
			             	break;
			            case 5:
			            	encryptData(data);
			            	break;
			            case 6:
			            	decryptData(data);

					}
				}
			
				return APPLET_SUCCESS;
			}
			
			public void generateSymmetricKey() 
			{
				DebugPrint.printString("generating Symmetric Key!!" + FlashStorage.getFlashDataSize(1));
				short keyIndex = 0; 
			    short keyLength = 16;
			    
				if(FlashStorage.getFlashDataSize(1)==0) //in case it is the first time that the program runs
				{	
					DebugPrint.printString("creating new simetric key");
			        DebugPrint.printString("generating instance");
				    instance = SymmetricBlockCipherAlg.create(SymmetricBlockCipherAlg.ALG_TYPE_AES_ECB);
				    byte[] keyArray = new byte[16];
				    DebugPrint.printString("random");
				    Random.getRandomBytes(keyArray, (short) 0, (short) 16);
				    DebugPrint.printString("set key");
				    instance.setKey(keyArray, keyIndex, keyLength);
				    //save array to memory
				    DebugPrint.printString("saving key");
				    FlashStorage.writeFlashData(1, keyArray, 0, keyArray.length);
			    }
				else
				{			  
				    DebugPrint.printString("restore simetric key from the flash storage");
			        byte[] destKey = new byte[16];
				    DebugPrint.printString("read flash data");
			        FlashStorage.readFlashData(1,destKey, 0);
				    DebugPrint.printString("generating instance");
			        instance = SymmetricBlockCipherAlg.create(SymmetricBlockCipherAlg.ALG_TYPE_AES_ECB);
				    DebugPrint.printString("set key");
			        instance.setKey(destKey,keyIndex,keyLength);
			        //sets from memory
			        DebugPrint.printString("saved SymmetricBlockCipher instance from memory to instance");
			    }

			}
			
			
			
			//Allows a new user to register to our system
			public void Register(byte[] data)//data=authority level+user_id + password //get rid of space we added!!!
			{  
				if(!ISREGISTER) 
				{
					System.arraycopy(data, 0, USER_AUTHORITY, 0, 1);
					FlashStorage.writeFlashData(0, data, 0, data.length);
					ISREGISTER=true;
		        	setResponseCode(1);
			    }
			    else setResponseCode(0);
			}
			
			
			//Allows a user that is already registered to the system to login 
			public void Login(byte[] data)//data=authority + user_id + password //get rid of space we added!!!//check that the response codes corraspond
			{	DebugPrint.printString("logged in"+ ISLOGIN);
				try {
				if(!ISLOGIN) 
				{
					byte[] dest = new byte[100];
					byte[] savedInformation = new byte[100]; 
					byte[] paddeddest = new byte[100];	
					DebugPrint.printString("reading the user details from flash storage " + FlashStorage.getFlashDataSize(0));							
					FlashStorage.readFlashData(0,savedInformation,0); //userAuthority + id + password
					System.arraycopy(savedInformation,1, dest, 0, data.length); //copy the data to padded_dest
					System.arraycopy(data, 0, paddeddest, 0, data.length); //copy the data to padded_dest
					DebugPrint.printString("comparing the user details" );
					boolean f = ArrayUtils.compareByteArray(paddeddest, 0, dest, 0, paddeddest.length);
					if (f==true)
				    {
				        ISLOGIN = true;
						setResponse(USER_AUTHORITY, 0, USER_AUTHORITY.length);
				        setResponseCode(1);
				    }
				    else { setResponseCode(0);}
				}
				else
				{
					DebugPrint.printString("already logged in");
			    	setResponseCode(2);
				}
				}
				catch(Exception e) {
				}
			  }
			
			
			//Allows a user to logout of system as long as hes already registered and logged in//make sure corresponds with  the host
			public void Logout(byte[]data)//data=user id+password
			{
				DebugPrint.printString("logged in"+ ISLOGIN);
				if(ISLOGIN)
				{
					ISLOGIN = false;
					setResponseCode(1);
				}
				else setResponseCode(0);
			 }
				
			
			//will reset password as long as user is logged in 
			public void ResetPassword(byte[] data) //data= authority level + user id and new password
			{
				DebugPrint.printString("logged in"+ ISLOGIN);
			    if(ISLOGIN)
			    {
			    	FlashStorage.eraseFlashData(0);
			    	FlashStorage.writeFlashData(0, data, 0, data.length);
			    	setResponseCode(1);
			    }
			    else setResponseCode(0);
			}
			
			public void encryptData(byte[] data) {	
				DebugPrint.printString("before encryption" + data); 
				byte[] encryptedData = new byte[1000];
				short size = instance.encryptComplete(data,(short)0,(short) data.length, encryptedData,(short)0);
				byte[] myResponse = new byte[size];
				ArrayUtils.copyByteArray(encryptedData,0,myResponse,0,size);
				DebugPrint.printString("after encryption" + encryptedData); 
				setResponse(myResponse, 0, myResponse.length);
				setResponseCode(1);	
				
			}
			
			public void decryptData(byte[] data) {
					DebugPrint.printString("before decryption" + data); 
					byte[] authorityLevel = new byte[1];
					ArrayUtils.copyByteArray(data, 0, authorityLevel,0 , 1);
					byte authorityLevelData = authorityLevel[0];
					//String authorityLevelUser = USER_AUTHORITY.toString();
					byte authorityLevelUser = USER_AUTHORITY[0];
					if(authorityLevelData<=authorityLevelUser)
					{
						byte[] data1 = new byte[data.length-1];
						ArrayUtils.copyByteArray(data, 1, data1 ,0 ,data.length-1);
						byte[] decryptedData = new byte[1000];
						short size = instance.decryptComplete(data1,(short)0,(short) data1.length, decryptedData,(short)0);
						byte[] myResponse = new byte[size];
						try {
						ArrayUtils.copyByteArray(decryptedData,0,myResponse,0,size);}
						catch(Exception e) {
							DebugPrint.printString(e.toString()); 
						}
						DebugPrint.printString("after decryption" + myResponse); 
						setResponse(myResponse, 0, myResponse.length);
						setResponseCode(1);
					}
					else {
						setResponse(data, 0, data.length);
						setResponseCode(0);
					}
					
				
			}
			/**
			 * This method will be called by the VM when the session being handled by
			 * this Trusted Application instance is being closed 
			 * and this Trusted Application instance is about to be removed.
			 * This method cannot provide response data and therefore
			 * calling setResponse or setResponseCode methods from it will throw a NullPointerException.
			 * 
			 * @return APPLET_SUCCESS code (the status code is not used by the VM).
			 */
			public int onClose() {
				DebugPrint.printString("Goodbye, DAL!");
				return APPLET_SUCCESS;
			}
		}
