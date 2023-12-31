using System.Globalization;

namespace Library.Service.Helpers
{
    public class InWord
    {
        public static string SpellAmountInIndiaSubConWay(string myNumber, string CUR_NAME, string CUR_DEC_NAME, string strOnly)
        {
            var Dollars = "";
            var Cents = "";
            var Temp = "";
            int DecimalPlace, Count;

            var place = new string[9];
            place[2] = " Thousand ";
            place[3] = " Lakhs ";
            place[4] = " Crore ";

            //String representation of amount.
            myNumber = myNumber.Trim();

            if (ConvNumData(myNumber) != "0")
            {
                myNumber = string.Format("{0:#.00}", double.Parse(ConvNumData(myNumber)));
            }

            //Position of decimal place 0 if none.
            DecimalPlace = myNumber.IndexOf(".");
            //Convert cents and set MyNumber to dollar amount.
            if (DecimalPlace > 0)
            {
                Cents = myNumber.Substring(DecimalPlace + 1);
                Cents = GetTens(Cents);
                myNumber = myNumber.Substring(0, DecimalPlace);
            }
            if (DecimalPlace == 0)
            {
                Cents = myNumber.Substring(DecimalPlace + 1);
                Cents = GetTens(Cents);
                myNumber = myNumber.Substring(0, DecimalPlace);
            }
            Count = 1;
            while (myNumber != "")
            {
                if (Count == 1)
                {
                    if (myNumber.Length > 3)
                    {
                        Temp = myNumber.Substring(myNumber.Length - 3);
                    }
                    else
                    {
                        Temp = myNumber;
                    }
                    Temp = GetHundreds(Temp);
                }
                else if (Count > 1 && Count <= 3)
                {
                    if (myNumber.Length >= 2)
                    {
                        Temp = GetTens(myNumber.Substring(myNumber.Length - 2));
                    }
                    else if (myNumber.Length == 1)
                    {
                        Temp = GetDigit(myNumber);
                    }
                }
                else if (Count >= 4)
                {
                    if (myNumber.Length > 3)
                    {
                        Temp = myNumber.Substring(myNumber.Length - 3);
                    }
                    else
                    {
                        Temp = myNumber;
                    }
                    Temp = GetHundreds(Temp);
                }

                if (Temp != "")
                {
                    Dollars = Temp + place[Count] + Dollars;
                }
                if (Count == 1)
                {
                    if (myNumber.Length > 2)
                    {
                        myNumber = myNumber.Substring(0, myNumber.Length - 3);
                    }
                    else
                    {
                        myNumber = "";
                    }
                }
                else if (Count > 1 && Count <= 3)
                {
                    if (myNumber.Length > 1)
                    {
                        myNumber = myNumber.Substring(0, myNumber.Length - 2);
                    }
                    else
                    {
                        myNumber = "";
                    }
                }
                else if (Count >= 4)
                {
                    if (myNumber.Length > 2)
                    {
                        myNumber = myNumber.Substring(0, myNumber.Length - 3);
                    }
                    else
                    {
                        myNumber = "";
                    }
                }

                Count = Count + 1;
            }

            switch (Dollars)
            {
                case "":
                    Dollars = "";
                    break;

                case "One":
                    Dollars = "One " + " " + CUR_NAME;
                    break;

                default:
                    Dollars = Dollars + " " + CUR_NAME;
                    break;
            }
            switch (Cents)
            {
                case "":
                    Cents = "";
                    break;

                case "One":
                    Cents = " and One " + " " + CUR_DEC_NAME;
                    break;

                default:
                    Cents = " and " + Cents + " " + CUR_DEC_NAME;
                    break;
            }

            return Dollars + Cents + " " + strOnly;
        }

        public static string SpellAmountInIndiaSubConWayNew(string myNumber, string CUR_NAME, string CUR_DEC_NAME)
        {
            var Dollars = "";
            var Cents = "";
            var Temp = "";
            int DecimalPlace, Count;

            var place = new string[9];
            place[2] = " Thousand ";
            place[3] = " Lakhs ";
            place[4] = " Crore ";

            //String representation of amount.
            myNumber = myNumber.Trim();

            if (ConvNumData(myNumber) != "0")
            {
                myNumber = string.Format("{0:#.00}", double.Parse(ConvNumData(myNumber)));
            }

            //Position of decimal place 0 if none.
            DecimalPlace = myNumber.IndexOf(".");
            //Convert cents and set MyNumber to dollar amount.
            if (DecimalPlace > 0)
            {
                Cents = myNumber.Substring(DecimalPlace + 1);
                Cents = GetTens(Cents);
                myNumber = myNumber.Substring(0, DecimalPlace);
            }
            if (DecimalPlace == 0)
            {
                Cents = myNumber.Substring(DecimalPlace + 1);
                Cents = GetTens(Cents);
                myNumber = myNumber.Substring(0, DecimalPlace);
            }
            Count = 1;
            while (myNumber != "")
            {
                if (Count == 1)
                {
                    if (myNumber.Length > 3)
                    {
                        Temp = myNumber.Substring(myNumber.Length - 3);
                    }
                    else
                    {
                        Temp = myNumber;
                    }
                    Temp = GetHundreds(Temp);
                }
                else if (Count > 1 && Count <= 3)
                {
                    if (myNumber.Length >= 2)
                    {
                        Temp = GetTens(myNumber.Substring(myNumber.Length - 2));
                    }
                    else if (myNumber.Length == 1)
                    {
                        Temp = GetDigit(myNumber);
                    }
                }
                else if (Count >= 4)
                {
                    if (myNumber.Length > 3)
                    {
                        Temp = myNumber.Substring(myNumber.Length - 3);
                    }
                    else
                    {
                        Temp = myNumber;
                    }
                    Temp = GetHundreds(Temp);
                }

                if (Temp != "")
                {
                    Dollars = Temp + place[Count] + Dollars;
                }
                if (Count == 1)
                {
                    if (myNumber.Length > 2)
                    {
                        myNumber = myNumber.Substring(0, myNumber.Length - 3);
                    }
                    else
                    {
                        myNumber = "";
                    }
                }
                else if (Count > 1 && Count <= 3)
                {
                    if (myNumber.Length > 1)
                    {
                        myNumber = myNumber.Substring(0, myNumber.Length - 2);
                    }
                    else
                    {
                        myNumber = "";
                    }
                }
                else if (Count >= 4)
                {
                    if (myNumber.Length > 2)
                    {
                        myNumber = myNumber.Substring(0, myNumber.Length - 3);
                    }
                    else
                    {
                        myNumber = "";
                    }
                }

                Count = Count + 1;
            }

            switch (Dollars)
            {
                case "":
                    Dollars = "";
                    break;

                case "One":
                    Dollars = "One " + " " + CUR_NAME;
                    break;

                default:
                    Dollars = Dollars + " " + CUR_NAME;
                    break;
            }
            switch (Cents)
            {
                case "":
                    Cents = "";
                    break;

                case "One":
                    Cents = " and One " + " " + CUR_DEC_NAME;
                    break;

                default:
                    Cents = " and " + Cents + " " + CUR_DEC_NAME;
                    break;
            }

            return Dollars + Cents;
        }

        public static string SpellAmountInIntlWay(string MyNumber, string CUR_NAME, string CUR_DEC_NAME, string strOnly)
        {
            string Dollars = "", Cents = "", Temp = "";
            int DecimalPlace, Count;

            string[] place = new string[9];
            place[2] = " Thousand ";
            place[3] = " Million ";
            place[4] = " Billion ";
            place[5] = " Trillion ";
            place[6] = " Quadrillion ";

            //String representation of amount.
            MyNumber = MyNumber.Trim();

            if (ConvNumData(MyNumber) != "0")
            {
                MyNumber = string.Format("{0:#.00}", double.Parse(ConvNumData(MyNumber)));
            }

            //Position of decimal place 0 if none.
            DecimalPlace = MyNumber.IndexOf(".");
            //Convert cents and set MyNumber to dollar amount.
            if (DecimalPlace > 0)
            {
                Cents = MyNumber.Substring(DecimalPlace + 1);
                Cents = GetTens(Cents);
                MyNumber = MyNumber.Substring(0, DecimalPlace);
            }
            if (DecimalPlace == 0)
            {
                Cents = MyNumber.Substring(DecimalPlace + 1);
                Cents = GetTens(Cents);
                MyNumber = MyNumber.Substring(0, DecimalPlace);
            }
            Count = 1;
            while (MyNumber != "")
            {
                if (MyNumber.Length > 3)
                {
                    Temp = MyNumber.Substring(MyNumber.Length - 3);
                }
                else
                {
                    Temp = MyNumber;
                }
                Temp = GetHundreds(Temp);

                if (Temp != "")
                {
                    Dollars = Temp + place[Count] + Dollars;
                }
                if (MyNumber.Length > 3)
                {
                    MyNumber = MyNumber.Substring(0, MyNumber.Length - 3);
                }
                else
                {
                    MyNumber = "";
                }

                Count = Count + 1;
            }

            switch (Dollars)
            {
                case "":
                    Dollars = "";
                    //Dollars = "Zero " + " " + CUR_NAME;
                    break;

                case "One":
                    Dollars = "One " + " " + CUR_NAME;
                    break;

                default:
                    Dollars = Dollars + " " + CUR_NAME;
                    break;
            }
            switch (Cents)
            {
                case "":
                    Cents = "";
                    //Cents = " and Zero " + " " + CUR_DEC_NAME;
                    break;

                case "One":
                    Cents = " and One " + " " + CUR_DEC_NAME;
                    break;

                default:
                    Cents = " and " + Cents + " " + CUR_DEC_NAME;
                    break;
            }

            return Dollars + Cents + " " + strOnly;
        } // end of function

        public static string SpellAmountInIntlWay2(string MyNumber, string CUR_NAME, string CUR_DEC_NAME, string strOnly)
        {
            string Dollars = "", Cents = "", Temp = "";
            int DecimalPlace;

            string[] place = new string[9];
            place[2] = " Thousand";
            place[3] = " Million";
            place[4] = " Billion";
            place[5] = " Trillion";
            place[6] = " Quadrillion";

            //String representation of amount.
            MyNumber = MyNumber.Trim();
            if (ConvNumData(MyNumber) != "0")
            {
                MyNumber = string.Format("{0:#.00}", double.Parse(ConvNumData(MyNumber)));
            }

            //Position of decimal place 0 if none.
            DecimalPlace = MyNumber.IndexOf(".");
            //Convert cents and set MyNumber to dollar amount.
            if (DecimalPlace > 0)
            {
                Cents = MyNumber.Substring(DecimalPlace + 1).Trim();
                Dollars = MyNumber.Substring(0, DecimalPlace);

                Temp = IntegerToWords(long.Parse(Cents)) + " " + CUR_DEC_NAME;
            }
            else
            {
                Dollars = MyNumber.Substring(0, MyNumber.Length);
            }
            Temp = IntegerToWords(long.Parse(ConvNumData(Dollars))) + " " + CUR_NAME + " " + Temp + " " + strOnly;
            return Temp;
        }

        public static string IntegerToWords(long inputNum)
        {
            int dig1, dig2, dig3, level = 0, lasttwo, threeDigits;
            string retval = "";
            string x = "";

            string[] ones = { "", "one", "two", "three", "four", "five", "six", "seven", "eight", "nine", "ten", "eleven", "twelve", "thirteen", "fourteen", "fifteen", "sixteen", "seventeen", "eighteen", "nineteen" };
            string[] tens = { "", "ten", "twenty", "thirty", "forty", "fifty", "sixty", "seventy", "eighty", "ninety" };
            string[] thou = { "", "thousand", "million", "billion", "trillion", "quadrillion", "quintillion" };

            var isNegative = false;

            if (inputNum < 0)
            {
                isNegative = true;
                inputNum *= -1;
            }
            if (inputNum == 0)
            { return "zero"; }

            string s = inputNum.ToString();
            while (s.Length > 0)
            {
                // Get the three rightmost characters
                if (s.Length < 3)
                {
                    x = s;
                }
                else
                {
                    x = s.Substring(s.Length - 3, 3);
                }
                // Separate the three digits
                threeDigits = int.Parse(x);
                lasttwo = threeDigits % 100;
                dig1 = threeDigits / 100;
                dig2 = lasttwo / 10;
                dig3 = threeDigits % 10;
                // append a "thousand" where appropriate
                if (level > 0 && dig1 + dig2 + dig3 > 0)
                {
                    retval = thou[level] + " " + retval;
                    retval = retval.Trim();
                }
                // check that the last two digits is not a zero
                if (lasttwo > 0)
                {
                    if (lasttwo < 20) // if less than 20, use "ones" only
                    {
                        retval = ones[lasttwo] + " " + retval;
                    }
                    else // otherwise, use both "tens" and "ones" array
                    {
                        retval = tens[dig2] + " " + ones[dig3] + " " + retval;
                    }
                }    // if a hundreds part is there, translate it
                if (dig1 > 0)
                {
                    retval = ones[dig1] + " hundred " + retval;
                }

                if ((s.Length - 3) > 0)
                {
                    s = s.Substring(0, s.Length - 3);
                }
                else
                {
                    s = "";
                }
                level++;
            }

            //clean up string
            while (retval.IndexOf("  ") > 0)
            {
                retval = retval.Replace("  ", " ");
            }
            retval = retval.Trim();

            if (isNegative)
            {
                retval = "negative " + retval;
            }

            return retval;
        }

        private static string GetHundreds(string myNumber)
        {
            string Result = "";
            if (IsNumeric(myNumber))
            {
                if (ConvNumData(myNumber) == "0")
                {
                    return "";
                }
            }
            else
            {
                return "";
            }

            myNumber = string.Format("{0:000}", long.Parse(myNumber));

            // Convert the hundreds place.
            if (myNumber.Substring(0, 1) != "0")
            {
                Result = GetDigit(myNumber.Substring(0, 1)) + " Hundred ";
            }

            //Convert the tens and ones place.
            if (myNumber.Substring(1, 1) != "0")
            {
                Result = Result + GetTens(myNumber.Substring(1));
            }
            else
            {
                Result = Result + GetDigit(myNumber.Substring(2));
            }
            return Result;
        }

        //Converts a number from 10 to 99 into text. *
        private static string GetTens(string TensText)
        {
            string Result = "";
            string strTemp = "";

            if (TensText.Substring(0, 1) == "1") // If value between 10-19...
            {
                switch (ConvNumData(TensText))
                {
                    case "10":
                        Result = "Ten";
                        break;

                    case "11":
                        Result = "Eleven";
                        break;

                    case "12":
                        Result = "Twelve";
                        break;

                    case "13":
                        Result = "Thirteen";
                        break;

                    case "14":
                        Result = "Fourteen";
                        break;

                    case "15":
                        Result = "Fifteen";
                        break;

                    case "16":
                        Result = "Sixteen";
                        break;

                    case "17":
                        Result = "Seventeen";
                        break;

                    case "18":
                        Result = "Eighteen";
                        break;

                    case "19":
                        Result = "Nineteen";
                        break;

                    default:
                        break;
                }
            }
            else
            {
                // If value between 20-99...
                switch (ConvNumData(TensText.Substring(0, 1)))
                {
                    case "2":
                        Result = "Twenty";
                        break;

                    case "3":
                        Result = "Thirty";
                        break;

                    case "4":
                        Result = "Forty";
                        break;

                    case "5":
                        Result = "Fifty";
                        break;

                    case "6":
                        Result = "Sixty";
                        break;

                    case "7":
                        Result = "Seventy";
                        break;

                    case "8":
                        Result = "Eighty";
                        break;

                    case "9":
                        Result = "Ninety";
                        break;

                    default:
                        break;
                }

                strTemp = GetDigit(TensText.Substring(TensText.Length - 1, 1));

                if (strTemp != "" && Result != "")
                {
                    Result = Result + "-" + GetDigit(TensText.Substring(TensText.Length - 1, 1));  // Retrieve ones place.
                }
                else
                {
                    Result = Result + GetDigit(TensText.Substring(TensText.Length - 1, 1));  //' Retrieve ones place.
                }
            }
            return Result;
        }

        private static string GetDigit(string digit)
        {
            var result = "";
            switch (ConvNumData(digit))
            {
                case "1":
                    result = "One";
                    break;

                case "2":
                    result = "Two";
                    break;

                case "3":
                    result = "Three";
                    break;

                case "4":
                    result = "Four";
                    break;

                case "5":
                    result = "Five";
                    break;

                case "6":
                    result = "Six";
                    break;

                case "7":
                    result = "Seven";
                    break;

                case "8":
                    result = "Eight";
                    break;

                case "9":
                    result = "Nine";
                    break;

                default:
                    result = "";
                    break;
            }
            return result;
        }

        private static bool IsNumeric(string strNumber)
        {
            double d;
            var n = new NumberFormatInfo();
            if (strNumber.Length == 0)
            {
                return false;
            }
            return double.TryParse(strNumber, NumberStyles.Float, n, out d);
        }

        private static string ConvNumData(string strNumber)
        {
            double d;
            var n = new NumberFormatInfo();
            return strNumber.Trim() == "" ? "0" : double.TryParse(strNumber, NumberStyles.Float, n, out d) ? strNumber : "0";
        }
    }
}