using System;
using System.Text;
using System.Collections.Generic;

namespace EliCDavis.RecordAndPlay.Util
{
    /// <summary>
    /// Various string formatting utilities.
    /// </summary>
    public static class FormattingUtil
    {


        /// <summary>
        /// This method encodes strings. For instance, single quotation marks and double quotation marks are included as \' and \" in the encoded string.
        /// </summary>
        /// <remarks>
        /// https://github.com/mono/mono/blob/master/mcs/class/System.Web/System.Web/HttpUtility.cs
        /// Authors:
        ///   Patrik Torstensson (Patrik.Torstensson@labs2.com)
        ///   Wictor Wilén (decode/encode functions) (wictor@ibizkit.se)
        ///   Tim Coleman (tim@timcoleman.com)
        ///   Gonzalo Paniagua Javier (gonzalo@ximian.com)
        /// Copyright (C) 2005-2010 Novell, Inc (http://www.novell.com)
        /// Permission is hereby granted, free of charge, to any person obtaining
        /// a copy of this software and associated documentation files (the
        /// "Software"), to deal in the Software without restriction, including
        /// without limitation the rights to use, copy, modify, merge, publish,
        /// distribute, sublicense, and/or sell copies of the Software, and to
        /// permit persons to whom the Software is furnished to do so, subject to
        /// the following conditions:
        ///  
        /// The above copyright notice and this permission notice shall be
        /// included in all copies or substantial portions of the Software.
        /// </remarks>
        /// <param name="value">A string to encode.</param>
        /// <returns>An encoded string.</returns>
        public static string JavaScriptStringEncode(string value)
        {
            return JavaScriptStringEncode(value, false);
        }


        /// <summary>
        /// This method encodes strings. For instance, single quotation marks and double quotation marks are included as \' and \" in the encoded string.
        /// </summary>
        /// <remarks>
        /// https://github.com/mono/mono/blob/master/mcs/class/System.Web/System.Web/HttpUtility.cs
        /// Authors:
        ///   Patrik Torstensson (Patrik.Torstensson@labs2.com)
        ///   Wictor Wilén (decode/encode functions) (wictor@ibizkit.se)
        ///   Tim Coleman (tim@timcoleman.com)
        ///   Gonzalo Paniagua Javier (gonzalo@ximian.com)
        /// Copyright (C) 2005-2010 Novell, Inc (http://www.novell.com)
        /// Permission is hereby granted, free of charge, to any person obtaining
        /// a copy of this software and associated documentation files (the
        /// "Software"), to deal in the Software without restriction, including
        /// without limitation the rights to use, copy, modify, merge, publish,
        /// distribute, sublicense, and/or sell copies of the Software, and to
        /// permit persons to whom the Software is furnished to do so, subject to
        /// the following conditions:
        ///  
        /// The above copyright notice and this permission notice shall be
        /// included in all copies or substantial portions of the Software.
        /// </remarks>
        /// <param name="value">A string to encode.</param>
        /// <param name="addDoubleQuotes">A value that indicates whether double quotation marks will be included around the encoded string.</param>
        /// <returns>An encoded string.</returns>
        public static string JavaScriptStringEncode(string value, bool addDoubleQuotes)
        {
            if (String.IsNullOrEmpty(value))
                return addDoubleQuotes ? "\"\"" : String.Empty;

            int len = value.Length;
            bool needEncode = false;
            char c;
            for (int i = 0; i < len; i++)
            {
                c = value[i];

                if (c >= 0 && c <= 31 || c == 34 || c == 39 || c == 60 || c == 62 || c == 92)
                {
                    needEncode = true;
                    break;
                }
            }

            if (!needEncode)
                return addDoubleQuotes ? "\"" + value + "\"" : value;

            var sb = new StringBuilder();
            if (addDoubleQuotes)
                sb.Append('"');

            for (int i = 0; i < len; i++)
            {
                c = value[i];
                if (c >= 0 && c <= 7 || c == 11 || c >= 14 && c <= 31 || c == 39 || c == 60 || c == 62)
                    sb.AppendFormat("\\u{0:x4}", (int)c);
                else switch ((int)c)
                    {
                        case 8:
                            sb.Append("\\b");
                            break;

                        case 9:
                            sb.Append("\\t");
                            break;

                        case 10:
                            sb.Append("\\n");
                            break;

                        case 12:
                            sb.Append("\\f");
                            break;

                        case 13:
                            sb.Append("\\r");
                            break;

                        case 34:
                            sb.Append("\\\"");
                            break;

                        case 92:
                            sb.Append("\\\\");
                            break;

                        default:
                            sb.Append(c);
                            break;
                    }
            }

            if (addDoubleQuotes)
                sb.Append('"');

            return sb.ToString();
        }

        /// <summary>
        /// Converts a dictionary into a JSON string representation
        /// </summary>
        /// <param name="dict">A dictionary to convert</param>
        /// <returns>A dictionary as a JSON string</returns>
        public static string ToJSON(Dictionary<string, string> dict)
        {
            StringBuilder json = new StringBuilder("{");

            if (dict != null)
            {
                foreach (var keypair in dict)
                {
                    json.AppendFormat("\"{0}\": \"{1}\",", JavaScriptStringEncode(keypair.Key), JavaScriptStringEncode(keypair.Value));
                }
                if (dict.Count > 0)
                {
                    json.Remove(json.Length - 1, 1);
                }
            }

            json.Append("}");

            return json.ToString();
        }

        /// <summary>
        /// Turn a string into a CSV cell output.
        /// </summary>
        /// <param name="str">string to format.</param>
        /// <returns>The CSV cell formatted string.</returns>
        public static string StringToCSVCell(string str)
        {
            bool mustQuote = (str.Contains(",") || str.Contains("\"") || str.Contains("\r") || str.Contains("\n"));
            if (mustQuote)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("\"");
                foreach (char nextChar in str)
                {
                    sb.Append(nextChar);
                    if (nextChar == '"')
                        sb.Append("\"");
                }
                sb.Append("\"");
                return sb.ToString();
            }

            return str;
        }
    }
}