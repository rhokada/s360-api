using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApi.Entities
{
    public class CustomerSGP
    {
        public int? UserId { get; set; }
        public int? ProfileId { get; set; }
        public string Name { get; set; }
        public int? CompanyId { get; set; }
        public string Email { get; set; }
        
    }
}
