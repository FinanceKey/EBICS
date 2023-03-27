/*	
 * 	
 *  This file is part of libfintx.
 *  
 *  Copyright (C) 2016 - 2021 Torsten Klinger
 * 	E-Mail: torsten.klinger@googlemail.com
 *  
 *  This program is free software; you can redistribute it and/or
 *  modify it under the terms of the GNU Lesser General Public
 *  License as published by the Free Software Foundation; either
 *  version 3 of the License, or (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
 *  Lesser General Public License for more details.
 *
 *  You should have received a copy of the GNU Lesser General Public License
 *  along with this program; if not, write to the Free Software Foundation,
 *  Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
 * 	
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using libfintx.FinTS.Camt;
using libfintx.Globals;
using libfintx.Logger.Log;

namespace libfintx.FinTS
{
    public static partial class Helper
    {
        /// <summary>
        /// Regex pattern for HIRMG/HIRMS messages.
        /// </summary>
        private const string PatternResultMessage = @"(\d{4}):.*?:(.+)";

        /// <summary>
        /// Combine byte arrays
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        public static byte[] CombineByteArrays(byte[] first, byte[] second)
        {
            byte[] ret = new byte[first.Length + second.Length];

            Buffer.BlockCopy(first, 0, ret, 0, first.Length);
            Buffer.BlockCopy(second, 0, ret, first.Length, second.Length);

            return ret;
        }

        /// <summary>
        /// Encode to Base64
        /// </summary>
        /// <param name="toEncode"></param>
        /// <returns></returns>
        public static string EncodeTo64(string toEncode)
        {
            byte[] toEncodeAsBytes = Encoding.ASCII.GetBytes(toEncode);
            string returnValue = Convert.ToBase64String(toEncodeAsBytes);

            return returnValue;
        }

        /// <summary>
        /// Decode from Base64
        /// </summary>
        /// <param name="encodedData"></param>
        /// <returns></returns>
        public static string DecodeFrom64(string encodedData)
        {
            byte[] encodedDataAsBytes = Convert.FromBase64String(encodedData);
            string returnValue = Encoding.ASCII.GetString(encodedDataAsBytes);

            return returnValue;
        }

        /// <summary>
        /// Decode from Base64 default
        /// </summary>
        /// <param name="encodedData"></param>
        /// <returns></returns>
        public static string DecodeFrom64EncodingDefault(string encodedData)
        {
            byte[] encodedDataAsBytes = Convert.FromBase64String(encodedData);
            string returnValue = Encoding.GetEncoding("ISO-8859-1").GetString(encodedDataAsBytes);

            return returnValue;
        }

        /// <summary>
        /// Encrypt -> HNVSD
        /// </summary>
        /// <param name="Segments"></param>
        /// <returns></returns>
        public static string Encrypt(string Segments)
        {
            return "HNVSD:999:1+@" + Segments.Length + "@" + Segments + "'";
        }

        /// <summary>
        /// Extract value from string
        /// </summary>
        /// <param name="StrSource"></param>
        /// <param name="StrStart"></param>
        /// <param name="StrEnd"></param>
        /// <returns></returns>
        public static string Parse_String(string StrSource, string StrStart, string StrEnd)
        {
            int Start, End;

            if (StrSource.Contains(StrStart) && StrSource.Contains(StrEnd))
            {
                Start = StrSource.IndexOf(StrStart, 0) + StrStart.Length;
                End = StrSource.IndexOf(StrEnd, Start);

                return StrSource.Substring(Start, End - Start);
            }
            else
            {
                return string.Empty;
            }
        }



        internal static string Parse_Transactions_Startpoint(string bankCode)
        {
            return Regex.Match(bankCode, @"\+3040::[^:]+:(?<startpoint>[^'\+:]+)['\+:]").Groups["startpoint"].Value;
        }

        public static List<string> Parse_TANMedium(string BankCode)
        {
            List<string> result = new List<string>();

            // HITAB:5:4:3+0+A:1:::::::::::Handy::::::::+A:2:::::::::::iPhone Abid::::::::
            // HITAB:4:4:3+0+M:1:::::::::::mT?:MFN1:********0340'
            // HITAB:5:4:3+0+M:2:::::::::::Unregistriert 1::01514/654321::::::+M:1:::::::::::Handy:*********4321:::::::
            // HITAB:4:4:3+0+M:1:::::::::::mT?:MFN1:********0340+G:1:SO?:iPhone:00:::::::::SO?:iPhone''

            // For easier matching, replace '?:' by some special character
            BankCode = BankCode.Replace("?:", @"\");

            foreach (Match match in Regex.Matches(BankCode, @"\+[AGMS]:[012]:(?<Kartennummer>[^:]*):(?<Kartenfolgenummer>[^:]*):+(?<Bezeichnung>[^+:]+)"))
            {
                result.Add(match.Groups["Bezeichnung"].Value.Replace(@"\", "?:"));
            }

            return result;
        }


        /// <summary>
        /// Parse a single bank result message.
        /// </summary>
        /// <param name="BankCodeMessage"></param>
        /// <returns></returns>
        public static HBCIBankMessage Parse_BankCode_Message(string BankCodeMessage)
        {
            var match = Regex.Match(BankCodeMessage, PatternResultMessage);
            if (match.Success)
            {
                var code = match.Groups[1].Value;
                var message = match.Groups[2].Value;

                message = message.Replace("?:", ":");

                return new HBCIBankMessage(code, message);
            }
            return null;
        }

        /// <summary>
        /// Parse bank error codes
        /// </summary>
        /// <param name="BankCode"></param>
        /// <returns>Banks messages with "??" as seperator.</returns>
        public static List<HBCIBankMessage> Parse_BankCode(string BankCode)
        {
            List<HBCIBankMessage> result = new List<HBCIBankMessage>();

            string[] segments = BankCode.Split('\'');
            foreach (var segment in segments)
            {
                if (segment.Contains("HIRMG") || segment.Contains("HIRMS"))
                {
                    string[] messages = segment.Split('+');
                    foreach (var HIRMG_message in messages)
                    {
                        var message = Parse_BankCode_Message(HIRMG_message);
                        if (message != null)
                            result.Add(message);
                    }
                }
            }

            return result;
        }


        /// <summary>
        /// STOP Flicker Code Rendering
        /// </summary>
        public static void RunAfterTimespan(Action action, TimeSpan span)
        {
            Thread.Sleep(span);
            action();
        }


        /// <summary>
        /// Make filename valid
        /// </summary>
        public static string MakeFilenameValid(string value)
        {
            foreach (char c in System.IO.Path.GetInvalidFileNameChars())
            {
                value = value.Replace(c, '_');
            }
            return value.Replace(" ", "_");
        }

        private static string GetBPDDir()
        {
            var dir = FinTsGlobals.ProgramBaseDir;
            return Path.Combine(dir, "BPD");
        }

        private static string GetBPDFile(string dir, int BLZ)
        {
            return Path.Combine(dir, "280_" + BLZ + ".bpd");
        }

        private static string GetUPDDir()
        {
            var dir = FinTsGlobals.ProgramBaseDir;
            return Path.Combine(dir, "UPD");
        }

        private static string GetUPDFile(string dir, int BLZ, string UserID)
        {
            return Path.Combine(dir, "280_" + BLZ + "_" + UserID + ".upd");
        }


    }
}
