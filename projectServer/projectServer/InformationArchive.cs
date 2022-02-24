using projectServer;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace projectServer
{
    class InformationArchive
    {
        List<Patient> patients;

        public InformationArchive()
        {
            patients = new List<Patient>();
        }

        public int GetSize()
        {
            return patients.Count();

        }
        public void addPatient (int id, byte[] firstName, byte[] lastName, byte[] sex, byte[] phoneNumber, byte[] age, byte[] lastHeight, byte[] weight, byte[] lastBloodTest, byte[] medicalHistory, byte[] lastBloodPressure)

        {
            patients.Add(new Patient(id, firstName, lastName, sex, phoneNumber, age, lastHeight, weight, lastBloodPressure, medicalHistory, lastBloodPressure));
        }

        public byte[] findItem(int id1, string requestedItem)
        {
            foreach (Patient p in this.patients)
            {
                if (p.Id.Value == id1)
                {
                    switch (requestedItem)
                    {
                       // case "id": return convertItemToSend(p.Id.Key, p.Id.Value);
                        case "firstName": return convertItemToSend(p.FirstName.Key, p.FirstName.Value);
                        case "lastName": return convertItemToSend(p.LastName.Key, p.LastName.Value);
                        case "sex": return convertItemToSend(p.Sex.Key, p.Sex.Value);
                        case "phoneNumber": return convertItemToSend(p.PhoneNumber.Key, p.PhoneNumber.Value);
                        case "age": return convertItemToSend(p.Age.Key, p.Age.Value);
                        case "height": return convertItemToSend(p.Height.Key, p.Height.Value);
                        case "weight": return convertItemToSend(p.Weight.Key, p.Weight.Value);
                        case "lastBloodTest": return convertItemToSend(p.LastBloodTest.Key, p.LastBloodTest.Value);
                        case "medicalHistory": return convertItemToSend(p.MedicalHistory.Key, p.MedicalHistory.Value);
                        case "lastBloodPressure": return convertItemToSend(p.LastBloodPressure.Key, p.LastBloodPressure.Value);
                    }
                }
            }
            return null;
        }

        public byte[] convertItemToSend(int dataAuthority, byte[] data)
        {
            byte[] result = new byte[data.Length +1];
            result[0] = Convert.ToByte(dataAuthority);
            Array.Copy(data, 0, result, 1, data.Length);
            return result;
        }

    }
}
