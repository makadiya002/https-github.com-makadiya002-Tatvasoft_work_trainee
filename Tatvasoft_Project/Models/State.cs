﻿using System;
using System.Collections.Generic;

#nullable disable

namespace Tatvasoft_Project.Models
{
    public partial class State
    {
        public State()
        {
            Cities = new HashSet<City>();
        }

        public int Id { get; set; }
        public string StateName { get; set; }

        public virtual ICollection<City> Cities { get; set; }
    }
}
