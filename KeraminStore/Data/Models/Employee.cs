using System;
using System.Text.RegularExpressions;

namespace KeraminStore
{
    static class Employee
    {
        public static string CheckEmployeeFullName(string fullName, string emptyFullName, string invalidSymbol, string invalidLength)
        {
            if (fullName == string.Empty) return emptyFullName;
            else
            {
                if (fullName.Length >= 2 && fullName.Length <= 50)
                {
                    char[] fullNameArray = fullName.ToCharArray();
                    for (int i = 0; i < fullNameArray.Length; i++)
                    {
                        if (!char.IsLetter(fullNameArray[i]) && fullNameArray[i] != '-') return invalidSymbol;
                    }
                }
                else return invalidLength;
                return fullName;
            }
        }

        public static string CheckEmployeeLogin(string login, string emptyLogin, string invalidSymbol, string invalidLength)
        {
            if (login == string.Empty) return emptyLogin;
            else
            {
                if (login.Length >= 3 && login.Length <= 30)
                {
                    char[] loginArray = login.ToCharArray();
                    for (int i = 0; i < loginArray.Length; i++)
                    {
                        if (!char.IsLetter(loginArray[i]) && !char.IsDigit(loginArray[i]) && loginArray[i] != '_') return invalidSymbol;
                    }
                }
                else return invalidLength;
                return login;
            }
        }

        public static string CheckEmployeePassword(string password, string emptyPassword, string invalidPassword)
        {
            if (password == string.Empty) return emptyPassword;
            else
            {
                Regex regex = new Regex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)[a-zA-Z\d]{6,30}$");
                if (!regex.IsMatch(password)) return invalidPassword;
                return password;
            }
        }

        public static string CheckEmployeeBirthdayDate(DateTime birthdayDate, string uncorrectAge)
        {
            int year = DateTime.Now.Year - birthdayDate.Year;
            if (DateTime.Now.Month < birthdayDate.Month || (DateTime.Now.Month == birthdayDate.Month && DateTime.Now.Day < birthdayDate.Day)) year--;
            if (year < 18) return uncorrectAge;
            return birthdayDate.ToShortDateString();
        }
    }
}