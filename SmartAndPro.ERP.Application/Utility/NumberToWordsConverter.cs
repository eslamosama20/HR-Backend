
using System;
using System.Globalization;
using System.Text;
using Humanizer;
using Tafqeet;

namespace SmartAndPro.ERP.Application.Utility;

public static class NumberToWordsConverter
{
    public static string ToEnglishWords(decimal amount)
    {
        var integerPart = (int)amount;
        var decimalPart = (int)((amount - integerPart) * 100);

        var result = integerPart.ToWords(); // From Humanizer

        if (decimalPart > 0)
        {
            result += $" and {decimalPart.ToWords()} Halalas";
        }

        return result;
    }

    public static string ToArabicWords(decimal number)
    {
        var tafqeet = new Tafqeet.Tafqeet(TafqeetCurrency.SAR);
        var result = tafqeet.ToArabicWords(number);
        return result;
    }
}
