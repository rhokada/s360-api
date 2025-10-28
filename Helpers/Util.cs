using System;
using System.Text;

namespace WebApi.Helpers
{
   public static class Util
   {
      private static String _passwordNumbersAndCharsAllowed = "0123456789@!ABCDEFGHIJKLMNOPQRSTUVXZWY";

      private static int _passwordDigits = 4;

      private static String _passwordNumbersAllowed = "0123456789";

      private static int _firstPasswordDigits = 4;

      public static string CalculateMD5Hash(params string[] input)
      {
         var md5 = System.Security.Cryptography.MD5.Create();

         var inputBytes = System.Text.Encoding.ASCII.GetBytes(String.Join(String.Empty, input));

         var hash = md5.ComputeHash(inputBytes);

         var sb = new StringBuilder();

         for (int i = 0; i < hash.Length; i++)
         {
            sb.Append(hash[i].ToString("X2"));
         }

         return sb.ToString();
      }

      public static string GeneratePassword()
      {
         var stringChars = new char[_passwordDigits];
         var random = new Random();

         for (int i = 0; i < stringChars.Length; i++)
         {
            stringChars[i] = _passwordNumbersAndCharsAllowed[random.Next(_passwordNumbersAndCharsAllowed.Length)];
         }

         return new String(stringChars);
      }

      public static string GenerateNumericPassword()
      {
         var stringChars = new char[_firstPasswordDigits];
         var random = new Random();

         for (int i = 0; i < stringChars.Length; i++)
         {
            stringChars[i] = _passwordNumbersAndCharsAllowed[random.Next(_passwordNumbersAllowed.Length)];
         }

         return new String(stringChars);
      }

      public static bool Between(this DateTime instant, DateTime dtFrom, DateTime dtThru)
      {
         if (dtFrom > dtThru) throw new ArgumentException("Invalid period!");
         bool isBetween = (instant >= dtFrom && instant <= dtThru);
         return isBetween;
      }

      public static bool ValidateCnpj(this String cnpj)
      {
         return !String.IsNullOrEmpty(cnpj) && ValidateCNPJNumbers(cnpj);
      }

      public static Boolean ValidateCNPJNumbers(String cnpj)
      {
         Int32[] digitos, soma, resultado;

         Int32 nrDig;

         String ftmt;

         Boolean[] cnpjOk;

         cnpj = cnpj.Replace("/", "");

         cnpj = cnpj.Replace(".", "");

         cnpj = cnpj.Replace("-", "");

         if (cnpj == "00000000000000")
         {
            return false;
         }

         ftmt = "6543298765432";

         digitos = new Int32[14];

         soma = new Int32[2];

         soma[0] = 0;

         soma[1] = 0;

         resultado = new Int32[2];

         resultado[0] = 0;

         resultado[1] = 0;

         cnpjOk = new Boolean[2];

         cnpjOk[0] = false;

         cnpjOk[1] = false;

         try
         {
            for (nrDig = 0; nrDig < 14; nrDig++)
            {

               digitos[nrDig] = int.Parse(cnpj.Substring(nrDig, 1));

               if (nrDig <= 11)

                  soma[0] += (digitos[nrDig] *

                  int.Parse(ftmt.Substring(nrDig + 1, 1)));

               if (nrDig <= 12)

                  soma[1] += (digitos[nrDig] *

                  int.Parse(ftmt.Substring(nrDig, 1)));
            }

            for (nrDig = 0; nrDig < 2; nrDig++)
            {
               resultado[nrDig] = (soma[nrDig] % 11);

               if ((resultado[nrDig] == 0) || (resultado[nrDig] == 1))
                  cnpjOk[nrDig] = (digitos[12 + nrDig] == 0);
               else
                  cnpjOk[nrDig] = (digitos[12 + nrDig] == (11 - resultado[nrDig]));
            }

            return (cnpjOk[0] && cnpjOk[1]);

         }
         catch
         {
            return false;
         }
      }

   }
}
