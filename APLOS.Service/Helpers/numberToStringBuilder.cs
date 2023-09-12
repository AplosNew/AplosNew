using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;

namespace numberToString
{
    public class numberToStringBuilder
    {
        /*
       this program has been created by  : tarek talukder.
                                    email: tarek_uttara@yahoo.com
         **/

        public numberToStringBuilder()
        {

        }
        private static bool isInt(string num)
        {

            /**check whether a value is integer or not returns true if integer, 
                                                 * false if floating or string containing alpahnumeric**/
            bool isInt;
            short number;
            try
            {
                isInt = System.Int16.TryParse(num, out number);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {

            }
            return isInt;
        }
        private string oneToNinetyNine(int number, int rank, Dictionary<int, string> dicNumber, Dictionary<int, string> index)
        {
            string textToNumber = "";



            textToNumber += dicNumber[number];


            if (textToNumber != "")
                textToNumber += index[rank];

            return textToNumber;
        }
        public  string strnumberToString(string numberToConvert)
        {
            //numberToConvert = System.Math.Abs(numberToConvert);
            string currencyWord = " ";// " টাকা ";
            string fractionWord = " দশমিক";//" পঁয়সা ";
            numberToConvert = makeNumberAndBasicValidations(numberToConvert);//System.Convert.ToDecimal(numberToConvert.ToString("F2"));//converting the number to decimal format 188.00
            string[] numberWithPrecision = numberToConvert.Split('.'); //breaking decimal part
            string decimalPart = numberWithPrecision[0];//integer part
            string FloatingPart = numberWithPrecision[1];//floating point part

            if (numberToConvert == "")
                return "";
            if (decimalPart.Length > 126)
                return "";

            Dictionary<int, string> dicIndex = new Dictionary<int, string>();

            #region index
            dicIndex.Add(0, "");
            dicIndex.Add(1, "শত ");
            dicIndex.Add(2, " হাজার ");
            dicIndex.Add(3, " লক্ষ ");
            dicIndex.Add(4, " কোটি ");
            dicIndex.Add(5, " বিলিওন ");
            dicIndex.Add(6, " ট্রিলিওন ");

            dicIndex.Add(7, " কোয়াড্রিলিওন ");
            dicIndex.Add(8, " কুইন্টিলিওন ");
            dicIndex.Add(9, " সেক্সটিলিওন ");
            dicIndex.Add(10, " সেপ্টিলিওন ");
            dicIndex.Add(11, " অক্টিলিওন ");
            dicIndex.Add(12, " ননিলিওন ");

            dicIndex.Add(13, " ডেসিলিওন ");
            dicIndex.Add(14, " আনডেসিলিওন ");
            dicIndex.Add(15, " ডুয়োডেসিলিওন ");
            dicIndex.Add(16, " ট্রিডেসিলিওন ");
            dicIndex.Add(17, " কুয়াট্টুওরডেসিলিওন ");
            dicIndex.Add(18, " কুইনডেসিলিওন ");
            dicIndex.Add(19, " সেক্সডেসিলিওন ");
            dicIndex.Add(20, " সেপ্টেনডেসিলিওন ");
            dicIndex.Add(21, " অক্টোডেসিলিওন ");
            dicIndex.Add(22, " নভেমডেসিলিওন ");

            dicIndex.Add(23, " ভিজিন্টিলিওন ");
            dicIndex.Add(24, " আনভিজিন্টিলিওন ");
            dicIndex.Add(25, " ডুয়োভিজিন্টিলিওন ");
            dicIndex.Add(26, " ট্রিভিজিন্টিলিওন ");
            dicIndex.Add(27, " কোয়াট্টরভিজিন্টিলিওন ");
            dicIndex.Add(28, " কুইনকোয়াভিজিন্টিলিওন ");
            dicIndex.Add(29, " সেসভিজিন্টিলিওন ");
            dicIndex.Add(30, " সেপ্টেমভিজিন্টিলিওন ");
            dicIndex.Add(31, " অক্টোভিজিন্টিলিওন ");
            dicIndex.Add(32, " নভেমভিজিন্টিলিওন ");


            dicIndex.Add(33, " ভিট্রিজিন্টিলিওন ");
            dicIndex.Add(34, " আনভিট্রিজিন্টিলিওন ");
            dicIndex.Add(35, " ডুয়োভিট্রিজিন্টিলিওন ");
            dicIndex.Add(36, " ট্রিভিট্রিজিন্টিলিওন ");
            dicIndex.Add(37, " কোয়াট্টরভিট্রিজিন্টিলিওন ");
            dicIndex.Add(38, " কুইনকোয়াভিট্রিজিন্টিলিওন ");
            dicIndex.Add(39, " সেসভিট্রিজিন্টিলিওন ");
            dicIndex.Add(40, " সেপ্টেমভিট্রিজিন্টিলিওন ");
            dicIndex.Add(41, " অক্টোভিট্রিজিন্টিলিওন ");
            dicIndex.Add(42, " নভেমভিট্রিজিন্টিলিওন ");


            dicIndex.Add(43, " কোয়াড্রাগজিন্টিলিওন ");


            #endregion index

            Dictionary<int, string> dicNumber = new Dictionary<int, string>();

            #region raw number
            dicNumber.Add(0, "");
            dicNumber.Add(1, "এক");
            dicNumber.Add(2, "দুই");
            dicNumber.Add(3, "তিন");
            dicNumber.Add(4, "চার");
            dicNumber.Add(5, "পাঁচ");
            dicNumber.Add(6, "ছয়");
            dicNumber.Add(7, "সাত");
            dicNumber.Add(8, "আট");
            dicNumber.Add(9, "নয়");

            dicNumber.Add(10, "দশ");
            dicNumber.Add(11, "এগারো");
            dicNumber.Add(12, "বারো");
            dicNumber.Add(13, "তেরো");
            dicNumber.Add(14, "চৌদ্দ");
            dicNumber.Add(15, "পনেরো");
            dicNumber.Add(16, "ষোল");
            dicNumber.Add(17, "সতেরো");
            dicNumber.Add(18, "আঠারো");
            dicNumber.Add(19, "ঊনিশ");

            dicNumber.Add(20, "বিশ");
            dicNumber.Add(21, "একুশ");
            dicNumber.Add(22, "বাইশ");
            dicNumber.Add(23, "তেইশ");
            dicNumber.Add(24, "চব্বিশ");
            dicNumber.Add(25, "পঁচিশ");
            dicNumber.Add(26, "ছাব্বিশ");
            dicNumber.Add(27, "সাতাশ");
            dicNumber.Add(28, "আটাশ");
            dicNumber.Add(29, "ঊনত্রিশ");

            dicNumber.Add(30, "ত্রিশ");
            dicNumber.Add(31, "একত্রিশ");
            dicNumber.Add(32, "বত্রিশ");
            dicNumber.Add(33, "তেত্রিশ");
            dicNumber.Add(34, "চৌত্রিশ");
            dicNumber.Add(35, "পঁয়ত্রিশ");
            dicNumber.Add(36, "ছত্রিশ");
            dicNumber.Add(37, "সাঁইত্রিস");
            dicNumber.Add(38, "আটত্রিশ");
            dicNumber.Add(39, "ঊনচল্লিশ");

            dicNumber.Add(40, "চল্লিশ");
            dicNumber.Add(41, "একচল্লিশ");
            dicNumber.Add(42, "বিয়াল্লিশ");
            dicNumber.Add(43, "তেতাল্লিশ");
            dicNumber.Add(44, "চুয়াল্লিশ");
            dicNumber.Add(45, "পঁয়তাল্লিশ");
            dicNumber.Add(46, "ছেচল্লিশ");
            dicNumber.Add(47, "সাতচল্লিশ");
            dicNumber.Add(48, "আটচল্লিশ");
            dicNumber.Add(49, "ঊনপঞ্চাশ");



            dicNumber.Add(50, "পঞ্চাশ");
            dicNumber.Add(51, "একান্ন");
            dicNumber.Add(52, "বায়ান্ন");
            dicNumber.Add(53, "তিপ্পান্ন");
            dicNumber.Add(54, "চুয়ান্ন");
            dicNumber.Add(55, "পঞ্চান্ন");
            dicNumber.Add(56, "ছাপ্পান্ন");
            dicNumber.Add(57, "সাতান্ন");
            dicNumber.Add(58, "আটান্ন");
            dicNumber.Add(59, "ঊনষাট");

            dicNumber.Add(60, "ষাট");
            dicNumber.Add(61, "একষট্টি");
            dicNumber.Add(62, "বাষট্টি");
            dicNumber.Add(63, "তেষট্টি");
            dicNumber.Add(64, "চৌষট্টি");
            dicNumber.Add(65, "পয়ষট্টি");
            dicNumber.Add(66, "ছেষট্টি");
            dicNumber.Add(67, "সাতষট্টি");
            dicNumber.Add(68, "আটষট্টি");
            dicNumber.Add(69, "ঊনসত্তর");

            dicNumber.Add(70, "সত্তর");
            dicNumber.Add(71, "একাত্তর");
            dicNumber.Add(72, "বাহাত্তর");
            dicNumber.Add(73, "তিয়াত্তর");
            dicNumber.Add(74, "চুয়াত্তর");
            dicNumber.Add(75, "পঁচাত্তর");
            dicNumber.Add(76, "ছিয়াত্তর");
            dicNumber.Add(77, "সাতাত্তর");
            dicNumber.Add(78, "আটাত্তর");
            dicNumber.Add(79, "ঊনআশি");

            dicNumber.Add(80, "আশি");
            dicNumber.Add(81, "একাশি");
            dicNumber.Add(82, "বিরাশি");
            dicNumber.Add(83, "তিরাশি");
            dicNumber.Add(84, "চুরাশি");
            dicNumber.Add(85, "পঁচাশি");
            dicNumber.Add(86, "ছিয়াশি");
            dicNumber.Add(87, "সাতাশি");
            dicNumber.Add(88, "আটাশি");
            dicNumber.Add(89, "উননব্বই");

            dicNumber.Add(90, "নব্বই");
            dicNumber.Add(91, "একানব্বই");
            dicNumber.Add(92, "বিরানব্বই");
            dicNumber.Add(93, "তিরানব্বই");
            dicNumber.Add(94, "চুরানব্বই");
            dicNumber.Add(95, "পঁচানব্বই");
            dicNumber.Add(96, "ছিয়ানব্বই");
            dicNumber.Add(97, "সাতানব্বই");
            dicNumber.Add(98, "আটানব্বই");
            dicNumber.Add(99, "নিরানব্বই");
            #endregion raw number

            Dictionary<int, int> dicRank = new Dictionary<int, int>();//numberindex, rank

            #region Rank matrix
            dicRank.Add(0, 0);//Unit
            dicRank.Add(2, 1);//hundreds
            dicRank.Add(3, 2);//thousand
            dicRank.Add(5, 3);//lakh
            dicRank.Add(7, 4);//koti


            dicRank.Add(9, 5);//billoin
            dicRank.Add(12, 6);//trillion
            dicRank.Add(15, 7);
            dicRank.Add(18, 8);
            dicRank.Add(21, 9);
            dicRank.Add(24, 10);
            dicRank.Add(27, 11);
            dicRank.Add(30, 12);
            dicRank.Add(33, 13);
            dicRank.Add(36, 14);
            dicRank.Add(39, 15);
            dicRank.Add(42, 16);
            dicRank.Add(45, 17);
            dicRank.Add(48, 18);
            dicRank.Add(51, 19);
            dicRank.Add(54, 20);
            dicRank.Add(57, 21);
            dicRank.Add(60, 22);
            dicRank.Add(63, 23);

            dicRank.Add(66, 24);
            dicRank.Add(69, 25);
            dicRank.Add(72, 26);
            dicRank.Add(75, 27);
            dicRank.Add(78, 28);
            dicRank.Add(81, 29);
            dicRank.Add(84, 30);
            dicRank.Add(87, 31);
            dicRank.Add(90, 32);
            dicRank.Add(93, 33);
            dicRank.Add(96, 34);
            dicRank.Add(99, 35);
            dicRank.Add(102, 36);
            dicRank.Add(105, 37);
            dicRank.Add(108, 38);
            dicRank.Add(111, 39);
            dicRank.Add(114, 40);
            dicRank.Add(117, 41);
            dicRank.Add(120, 42);
            dicRank.Add(123, 43);

            #endregion Rank matrix

            Int16 valueToConvert = 0;
            string words = "";

            string tempDigits = "";
            decimalPart = flipNumber(decimalPart);//reversing the number eg. 345=543
            for (int i = 0; i < decimalPart.Length; i++)
            {
                if (i < 9)
                {
                    #region 0 to 99 koti
                    if (dicRank.ContainsKey(i) == true)//the rank dictionary contains starting point
                    {

                        if (dicRank[i] == 1)//hundred
                        {
                            valueToConvert = System.Int16.Parse(decimalPart.Substring(i, 1));
                        }

                        else
                        {
                            if (decimalPart.Substring(i).Length > 1)
                            {
                                tempDigits = decimalPart.Substring(i, 2);//flip the two digit
                                valueToConvert = Convert.ToInt16(flipNumber(tempDigits.ToString()));
                            }
                            else
                                valueToConvert = System.Int16.Parse(decimalPart.Substring(i, 1));


                        }
                        words = oneToNinetyNine(valueToConvert, (dicRank[i]), dicNumber, dicIndex) + words;
                    }
                    #endregion 0 to 99 koti
                }
                else
                {
                    #region billion to onword
                    if (dicRank.ContainsKey(i) == true)//the rank dictionary contains starting point
                    {
                        string threeDecimalWords = "";

                        string rankName = dicIndex[dicRank[i]].ToString();
                        string NewdecimalPart = decimalPart.Substring(i);
                        if (NewdecimalPart.Length > 2)
                            NewdecimalPart = NewdecimalPart.Substring(0, 3);//each tree digits represent a rank
                        for (int k = 0; k < NewdecimalPart.Length; k++)
                        {
                            if (dicRank.ContainsKey(k) == true)//the rank dictionary contains starting point
                            {
                                if (dicRank[k] == 1)//hundred
                                {
                                    valueToConvert = System.Int16.Parse(NewdecimalPart.Substring(k, 1));
                                }
                                else
                                {
                                    if (NewdecimalPart.Substring(k).Length > 1)
                                    {
                                        tempDigits = NewdecimalPart.Substring(k, 2);//flip the two digit
                                        valueToConvert = Convert.ToInt16(flipNumber(tempDigits.ToString()));
                                    }
                                    else
                                        valueToConvert = System.Int16.Parse(NewdecimalPart.Substring(k, 1));
                                }
                                threeDecimalWords = oneToNinetyNine(valueToConvert, (dicRank[k]), dicNumber, dicIndex) + threeDecimalWords;
                            }
                        }
                        if (threeDecimalWords != "")
                            words = threeDecimalWords + rankName + words;

                    }
                    #endregion billion to onword
                }
            }
            if (words != "")
                words += currencyWord;
            if (oneToNinetyNine(System.Int16.Parse(FloatingPart), 0, dicNumber, dicIndex) != "")
            {
                //words = words + oneToNinetyNine(System.Int16.Parse(FloatingPart), 0, dicNumber, dicIndex) + fractionWord;
                for (int f = 0; f < FloatingPart.Length; f++)
                {
                    fractionWord = fractionWord + " " + oneToNinetyNine(System.Int16.Parse(FloatingPart[f].ToString()), 0, dicNumber, dicIndex);
                }
                words = words + fractionWord;
            }


            return words;
        }
        public string flipNumber(string number)
        {
            string flipped = "";
            for (int i = (number.Length) - 1; i > -1; i--)
            {
                flipped += number.Substring(i, 1);
            }
            return flipped;
        }
        private string makeNumberAndBasicValidations(string numberToConvert)
        {
            numberToConvert = charUnicode(numberToConvert);
            numberToConvert = numberToConvert.Replace("-", "");
            numberToConvert = numberToConvert.Replace(",", "");
            //validate is every character is numeric
            try
            {
                string num = numberToConvert.Replace(".", "");

                if (num.Length == 0)
                    return "";

                for (int i = 0; i < num.Length; i++)
                {
                    if (isInt(num.Substring(i, 1)) == false)
                        return "";
                }

                //checking whether more than one decimal point used(eg. 9234.3455.995)
                int dotCount = 0;
                for (int i = 0; i < numberToConvert.Length; i++)
                {
                    if (numberToConvert.Substring(i, 1) == ".")
                        dotCount++;

                    if (dotCount > 1)
                        return "";
                }

                if (dotCount == 0)//full integer number
                    numberToConvert += ".00";//add extra decimal to the number

                if (dotCount == 1)
                {
                    if (numberToConvert.EndsWith(".") == true)//full integer with (.) eg. (8984.)
                        numberToConvert += "00";
                    if (numberToConvert.StartsWith(".") == true)//full floating number without zero (.343)
                        numberToConvert = "00" + numberToConvert;


                    string[] splitNumber = numberToConvert.Split('.');

                    if (splitNumber[1].Length > 1)
                        numberToConvert = splitNumber[0] + "." + splitNumber[1].Substring(0, 2);
                    else if (splitNumber[1].Length == 1)
                        numberToConvert = numberToConvert + "0";
                }

            }
            catch (Exception)
            {

            }
            finally
            {

            }

            return numberToConvert;

        }
        private string charUnicode(string s)
        {
            StringCollection dic = new StringCollection();

            dic.Add("০");
            dic.Add("১");
            dic.Add("২");
            dic.Add("৩");
            dic.Add("৪");
            dic.Add("৫");
            dic.Add("৬");
            dic.Add("৭");
            dic.Add("৮");
            dic.Add("৯");

            string ret = "";
            foreach (char c in s)
            {
                if (dic.Contains(c.ToString()))
                {
                    ret+= dic.IndexOf(c.ToString());
                }
                else
                {
                    ret+= c.ToString();
                }


            }

            return ret;

        
        }
    }
}
