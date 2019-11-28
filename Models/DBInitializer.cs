using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OpdrachtAPI.Models
{
    public class DBInitializer
    {
        public static void Initialize(WebpollContext context)
        {
            context.Database.EnsureCreated();

            if (context.Users.Any())
            {
                return;
            }

            context.Users.AddRange(
                            new User { UserName = "test", Password = "test", FirstName = "Bart", LastName = "Ooms", Email = "test@hotmail.com" }
                                 );
            context.Vriend.AddRange(
                            new Vriend { Email = "test@hotmail.com", UserName = "test"  }
                                );
            context.SaveChanges();


        }
    }
}
