namespace sabatex.ObjectsExchange.Models
{
 #if NET6_0_OR_GREATER
    public class Login
    {
        public Guid ClientId { get; set; }
        public string Password { get; set; } = default!;
    }
#else
   using System;
   public class Login
    {
        public Guid ClientId { get; set; }
        public string Password { get; set; }
    }
#endif

}
