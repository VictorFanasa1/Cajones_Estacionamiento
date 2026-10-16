using System;
using System.Linq;
using System.Security.Cryptography;

namespace Estacionamiento.Helpers
{
    public static class PasswordHelper
    {
        // Genera una contraseña aleatoria con diversidad de caracteres (sin hashear)
        public static string GenerarPassword(int length = 12)
        {
            if (length < 8) length = 8;

            const string upper = "ABCDEFGHJKLMNPQRSTUVWXYZ"; // sin I/O para evitar confusión
            const string lower = "abcdefghijkmnopqrstuvwxyz"; // sin l
            const string digits = "23456789"; // sin 0/1
            const string symbols = "!@$%&*?-_#";
            string all = upper + lower + digits + symbols;

            var pwd = new char[length];

            // Garantizar al menos uno de cada grupo
            pwd[0] = upper[GetIndex(upper.Length)];
            pwd[1] = lower[GetIndex(lower.Length)];
            pwd[2] = digits[GetIndex(digits.Length)];
            pwd[3] = symbols[GetIndex(symbols.Length)];

            for (int i = 4; i < length; i++)
            {
                pwd[i] = all[GetIndex(all.Length)];
            }

            // Mezclar de forma aleatoria
            return new string(pwd.OrderBy(_ => GetUInt()).ToArray());
        }

        private static int GetIndex(int max)
        {
            using (var rng = RandomNumberGenerator.Create())
            {
                var bytes = new byte[4];
                rng.GetBytes(bytes);
                int value = BitConverter.ToInt32(bytes, 0) & int.MaxValue;
                return value % max;
            }
        }

        private static uint GetUInt()
        {
            using (var rng = RandomNumberGenerator.Create())
            {
                var bytes = new byte[4];
                rng.GetBytes(bytes);
                return BitConverter.ToUInt32(bytes, 0);
            }
        }
    }
}
