//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.Data;
//using System.Diagnostics;


//internal sealed class ExcludeCharsAttribute : ValidationAttribute
//{
//    private string _Chars;
//    public ExcludeCharsAttribute(string Chars)
//    {
//        _Chars = Chars;
//    }

//    public override bool IsValid(object value)
//    {
//        bool result = true;
//        char[] X = _Chars.ToCharArray();
//        string Data = value.ToString();
//        if (!string.IsNullOrEmpty(Data))
//        {
//            for (int i = 0; i <= X.Length - 1; i++)
//            {
//                if (Data.Contains(X[i].ToString()))
//                {
//                    result = false;
//                }
//            }
//        }
//        return result;
//    }

//    public override string FormatErrorMessage(string name)
//    {
//        return string.Format(ErrorMessage, name, _Chars);
//    } 
//}

