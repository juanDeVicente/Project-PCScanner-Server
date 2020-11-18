using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Server_Linux.src.security
{
	class SecurityModule
	{
        private string passwordHash;
        private List<IPAddress> adressesLogged = new List<IPAddress>();
		public SecurityModule()
		{
            Console.WriteLine("Introduce a password:");
			PutPassword();
		}
		private void PutPassword()
		{
            var input = string.Empty;
            ConsoleKey key;
            do
            {
                var keyInfo = Console.ReadKey(intercept: true);
                key = keyInfo.Key;

                if (key == ConsoleKey.Backspace && input.Length > 0)
                {
                    Console.Write("\b \b");
                    input.Remove(input.Length - 1, 1);
                }
                else if (!char.IsControl(keyInfo.KeyChar))
                {
                    Console.Write("*");
                    input += keyInfo.KeyChar;
                }
            } while (key != ConsoleKey.Enter);
            using (var sha256Hash = SHA256.Create())
            {
                passwordHash = GetHash(sha256Hash, input);
            }
            Console.WriteLine();
        }
        private string GetHash(HashAlgorithm hashAlgorithm, string input)
        {
            byte[] data = hashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(input));
            var sBuilder = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            return sBuilder.ToString();
        }

        public bool VerifyHash(string input, IPAddress address)
        {
            if (IsIpLogged(address))
                return true;
            using (var sha256Hash = SHA256.Create())
            {
                var hashOfInput = GetHash(sha256Hash, input);
                StringComparer comparer = StringComparer.OrdinalIgnoreCase;
                if (comparer.Compare(hashOfInput, passwordHash) == 0)
                {
                    adressesLogged.Add(address);
                    return true;
                }
            }
            return false;
        }
        public bool IsIpLogged(IPAddress address)
		{
            return adressesLogged.Contains(address);
		}

    }
}
