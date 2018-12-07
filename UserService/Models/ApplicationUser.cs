using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace UserService.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string Name
        {
            get; set;
        }

        public string Surname
        {
            get; set;
        }

        public string Middlename
        {
            get; set;
        }

        public string FullName
        {
            get {
                return string.Join(" ", Surname, Name, Middlename);
            }
        }

        public override string UserName
        {
            get
            {
                return Email;
            }
        }
    }
}
