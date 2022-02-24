using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace projectServer
{
    class Patient
    {
        KeyValuePair<int, int> id;
        KeyValuePair<int, byte[]> firstName;
        KeyValuePair<int, byte[]> lastName;
        KeyValuePair<int, byte[]> sex;
        KeyValuePair<int, byte[]> phoneNumber;
        KeyValuePair<int, byte[]> age;
        KeyValuePair<int, byte[]> height;
        KeyValuePair<int, byte[]> weight;
        KeyValuePair<int, byte[]> lastBloodTest;
        KeyValuePair<int, byte[]> medicalHistory;
        KeyValuePair<int, byte[]> lastBloodPressure;

        public KeyValuePair<int, int> Id
        {
            get
            {
                return id;
            }

            set
            {
                id = value;
            }
        }

        public KeyValuePair<int, byte[]> FirstName
        {
            get
            {
                return firstName;
            }

            set
            {
                firstName = value;
            }
        }

        public KeyValuePair<int, byte[]> LastName
        {
            get
            {
                return lastName;
            }

            set
            {
                lastName = value;
            }
        }

        public KeyValuePair<int, byte[]> Sex
        {
            get
            {
                return sex;
            }

            set
            {
                sex = value;
            }
        }

        public KeyValuePair<int, byte[]> PhoneNumber
        {
            get
            {
                return phoneNumber;
            }

            set
            {
                phoneNumber = value;
            }
        }

        public KeyValuePair<int, byte[]> Age
        {
            get
            {
                return age;
            }

            set
            {
                age = value;
            }
        }

        public KeyValuePair<int, byte[]> Height
        {
            get
            {
                return height;
            }

            set
            {
                height = value;
            }
        }

        public KeyValuePair<int, byte[]> Weight
        {
            get
            {
                return weight;
            }

            set
            {
                weight = value;
            }
        }

        public KeyValuePair<int, byte[]> LastBloodTest
        {
            get
            {
                return lastBloodTest;
            }

            set
            {
                lastBloodTest = value;
            }
        }

        public KeyValuePair<int, byte[]> MedicalHistory
        {
            get
            {
                return medicalHistory;
            }

            set
            {
                medicalHistory = value;
            }
        }

        public KeyValuePair<int, byte[]> LastBloodPressure
        {
            get
            {
                return lastBloodPressure;
            }

            set
            {
                lastBloodPressure = value;
            }
        }



        public Patient(int id, byte[] firstName, byte[] lastName, byte[] sex, byte[] phoneNumber, byte[] age, byte[] Height1, byte[] weight, byte[] lastBloodTest, byte[] medicalHistory, byte[] lastBloodPressure)
        {
            Id = new KeyValuePair<int, int>(1, id);
            FirstName = new KeyValuePair<int, byte[]>(1, firstName);
            LastName = new KeyValuePair<int, byte[]>(1, lastName);
            Sex = new KeyValuePair<int, byte[]>(1, sex);
            PhoneNumber = new KeyValuePair<int, byte[]>(1, phoneNumber);
            Age = new KeyValuePair<int, byte[]>(1, age);
            Height = new KeyValuePair<int, byte[]>(2, Height1);
            Weight = new KeyValuePair<int, byte[]>(2, weight);
            LastBloodTest = new KeyValuePair<int, byte[]>(3, lastBloodTest);
            MedicalHistory = new KeyValuePair<int, byte[]>(3, medicalHistory);
            LastBloodPressure = new KeyValuePair<int, byte[]>(3, lastBloodPressure);
        }

    }
}
