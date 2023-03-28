using System.Security.Cryptography;
using System.Text;

namespace ObjectsExchange.Services
{
    public class ApiConfig
    {
        /// <summary>
        /// Life time seconds
        /// </summary>
        public int TokensLifeTime { get; set; } = 3600; 
        public string HashKey { get; set; } = "ajcewewi%^(F#|}9327nx=-23hdxsa5vcx<>_ d";
        public string HashPassword(string password)
        {
            var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(HashKey));
            return Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(password)));

        }
    }
}
