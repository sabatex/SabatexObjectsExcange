namespace sabatex.ObjectsExchange.Models
{
#if NET3_5 || NETSTANDARD2_0

    using System;
    public class Login
    {
        public Guid ClientId { get; set; }

        public string Password { get; set; }
    }
#endif

}