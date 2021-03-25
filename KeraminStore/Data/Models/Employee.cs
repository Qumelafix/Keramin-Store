using System;
using System.Data.SqlClient;
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

        public static string CheckEmployeePasportNumber(string pasportNumber, string emptyPasportNumber, string invalidSymbol, string invalidLength)
        {
            if (pasportNumber == string.Empty) return emptyPasportNumber;
            else
            {
                if (pasportNumber.Length == 9)
                {
                    if (!Regex.IsMatch(pasportNumber, @"([AB|BM|HB|KH|MP|MC|KB]\d{7})")) return invalidSymbol;
                }
                else return invalidLength;
                return pasportNumber;
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

        public static string CheckEmployeePassword(string password, string emptyPassword, string invalidSymbol, string invalidLength)
        {
            if (password == string.Empty) return emptyPassword;
            else
            {
                if (password.Length >= 6 && password.Length <= 30)
                {
                    char[] passwordArray = password.ToCharArray();
                    for (int i = 0; i < passwordArray.Length; i++)
                    {
                        if (!char.IsLetter(passwordArray[i]) && !char.IsDigit(passwordArray[i]) && passwordArray[i] != '_' && passwordArray[i] != '*') return invalidSymbol;
                    }
                }
                else return invalidLength;
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