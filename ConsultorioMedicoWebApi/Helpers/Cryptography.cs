using System;
using System.Security.Cryptography;

namespace ConsultorioMedicoWebApi.Helpers
{
    /// <summary>
    /// Utilidades de hashing de contraseñas (PBKDF2) portadas de HelpDesk.
    /// Formato de hash: PBKDF2$10000$&lt;saltBase64&gt;$&lt;hashBase64&gt;
    /// - Iteraciones: 10000
    /// - Salt: 16 bytes (aleatorio)
    /// - Hash: 32 bytes
    /// - Algoritmo: SHA256 (explicito; se mantiene consistente entre HashPassword y Verify)
    /// </summary>
    public static class Cryptography
    {
        private const int Iteraciones = 10000;
        private const int TamanoSalt = 16;
        private const int TamanoHash = 32;
        private const string Prefijo = "PBKDF2$";

        /// <summary>
        /// Genera el hash PBKDF2 de una contraseña en el formato PBKDF2$iteraciones$salt$hash.
        /// </summary>
        public static string HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
                throw new ArgumentNullException(nameof(password));

            var salt = new byte[TamanoSalt];
            using (var rng = new RNGCryptoServiceProvider())
                rng.GetBytes(salt);

            var hash = Derivar(password, salt, Iteraciones, TamanoHash);
            return Prefijo + Iteraciones + "$" + Convert.ToBase64String(salt) + "$" + Convert.ToBase64String(hash);
        }

        /// <summary>
        /// Verifica una contraseña contra un valor almacenado.
        /// - Si el valor inicia con "PBKDF2$" lo verifica con PBKDF2 (SHA256).
        /// - Si no, se asume texto plano legado (pre-SaaS) y se compara directamente.
        /// </summary>
        public static bool Verify(string password, string stored)
        {
            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(stored))
                return false;

            if (!EsHashPbkdf2(stored))
                return string.Equals(stored, password, StringComparison.Ordinal);

            return VerificarPbkdf2(password, stored);
        }

        /// <summary>
        /// Indica si el valor almacenado es un hash PBKDF2 (inicia con el prefijo "PBKDF2$").
        /// </summary>
        public static bool EsHashPbkdf2(string stored)
        {
            return !string.IsNullOrEmpty(stored) && stored.StartsWith(Prefijo, StringComparison.Ordinal);
        }

        private static bool VerificarPbkdf2(string password, string stored)
        {
            var partes = stored.Substring(Prefijo.Length).Split('$');
            if (partes.Length != 3) return false;

            int iteraciones;
            if (!int.TryParse(partes[0], out iteraciones)) return false;

            byte[] salt;
            byte[] esperado;
            try
            {
                salt = Convert.FromBase64String(partes[1]);
                esperado = Convert.FromBase64String(partes[2]);
            }
            catch (FormatException)
            {
                return false;
            }

            var actual = Derivar(password, salt, iteraciones, esperado.Length);
            return ComparacionTiempoConstante(actual, esperado);
        }

        private static byte[] Derivar(string password, byte[] salt, int iteraciones, int tamanio)
        {
            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iteraciones, HashAlgorithmName.SHA256))
                return pbkdf2.GetBytes(tamanio);
        }

        private static bool ComparacionTiempoConstante(byte[] a, byte[] b)
        {
            if (a == null || b == null || a.Length != b.Length) return false;
            int diff = 0;
            for (int i = 0; i < a.Length; i++) diff |= a[i] ^ b[i];
            return diff == 0;
        }
    }
}
