namespace sabatex.ObjectsExchange.Models
{
#if NET3_5
    using System;
    public class Login
    {
        public Guid ClientId { get; set; }

        public string Password { get; set; }
    }
#endif

}