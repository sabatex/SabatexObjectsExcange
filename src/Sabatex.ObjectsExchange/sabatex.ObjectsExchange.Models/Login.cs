namespace sabatex.ObjectsExchange.Models
{
    /// <summary>
    /// 
    /// </summary>

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
