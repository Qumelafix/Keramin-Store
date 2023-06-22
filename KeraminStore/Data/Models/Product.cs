using System.Text.RegularExpressions;

namespace KeraminStore.Data.Models
{
    public static class Product
    {
        public static string CheckProductName(string productName, string emptyName, string invalidSymbol, string invalidLength)
        {
            if (productName == string.Empty) return emptyName;
            else
            {
                if (productName.Length >= 10 && productName.Length <= 100)
                {
                    char[] productNameArray = productName.ToCharArray();
                    for (int i = 0; i < productNameArray.Length; i++)
                    {
                        if (!char.IsLetterOrDigit(productNameArray[i]) && productNameArray[i] != ' ' && productNameArray[i] != ',' && productNameArray[i] != '.' && productNameArray[i] != '-') return invalidSymbol;
                    }
                }
                else return invalidLength;
                return productName;
            }
        }

        public static string CheckProductWidth (string productWidth, string emptyProductWidth, string wrongValue, string invalidSymbols)
        {
            if (productWidth == string.Empty) return emptyProductWidth;
            else
            {
                double width = 0;
                bool isNum = double.TryParse(productWidth, out width);
                if (isNum)
                {
                    if (width < 9 || width > 600) return wrongValue;
                }
                else return invalidSymbols;
                return productWidth;
            }
        }

        public static string CheckProductLenght(string productLenght, string emptyProductLenght, string wrongValue, string invalidSymbols)
        {
            if (productLenght == string.Empty) return emptyProductLenght;
            else
            {
                double lenght = 0;
                bool isNum = double.TryParse(productLenght, out lenght);
                if (isNum)
                {
                    if (lenght < 98 || lenght > 1200) return wrongValue;
                }
                else return invalidSymbols;
                return productLenght;
            }
        }

        public static string CheckProductArticle(string productArticle, string emptyAricle, string invalidValue)
        {
            if (productArticle == string.Empty) return emptyAricle;
            else
            {
                Regex regex = new Regex(@"^[A-Z]{3}\d{8}$");
                if (productArticle.Length == 11)
                {
                    if (!regex.IsMatch(productArticle)) return invalidValue;
                }
                else return invalidValue;
                return productArticle;
            }
        }

        public static string CheckProductCostOrWeight(string productCostOrWeight, string wrongValue, string invalidSymbols)
        {            
            double cost = 0;
            bool isNum = double.TryParse(productCostOrWeight, out cost);
            if (isNum)
            {
                if (cost < 0 || cost > 100) return wrongValue;
            }
            else return invalidSymbols;
            return productCostOrWeight;
        }

        public static string CheckCountInBox(string countInBox, string emptyCountInBox, string wrongValue, string invalidSymbols)
        {
            if (countInBox == string.Empty) return emptyCountInBox;
            else
            {
                int count = 0;
                bool isNum = int.TryParse(countInBox, out count);
                if (isNum)
                {
                    if (count <= 0) return wrongValue;
                }
                else return invalidSymbols;
                return countInBox;
            }
        }

        public static string CheckProductDescription(string productDescription, string invalidLenght)
        {
            if (productDescription.Length > 300) return invalidLenght;
            else return productDescription;
        }

        public static string CheckCostForSearch(string costStr, string wrongValue, string invalidSymbols)
        {
            int cost = 0;
            bool isNum = int.TryParse(costStr, out cost);
            if (isNum)
            {
                if (cost < 0) return wrongValue;
            }
            else return invalidSymbols;
            return costStr;
        }
    }
}