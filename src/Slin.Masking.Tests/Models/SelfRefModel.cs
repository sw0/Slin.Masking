using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Slin.Masking.Tests.Models
{
    public class SelfRefModel
    {
        public int Id { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public SelfRefModel Friend { get; set; }
    }
}
