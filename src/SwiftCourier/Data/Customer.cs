﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SwiftCourier.Data
{
    public partial class Customer
    {
        public Customer()
        {
            Bookings = new HashSet<Booking>();
        }

        public int Id { get; set; }
        public string Address { get; set; }
        public string ContactNumber { get; set; }
        public string EmailAddress { get; set; }
        public string Name { get; set; }
        public bool TaxExempted { get; set; }

        public string Username { get; set; }
        public string Password { get; set; }

        public virtual ICollection<Booking> Bookings { get; set; }
    }
}
