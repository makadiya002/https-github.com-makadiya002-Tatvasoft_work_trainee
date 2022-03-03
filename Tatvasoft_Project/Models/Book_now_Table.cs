using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tatvasoft_Project.Models
{
    public class Book_now_Table
    {
        public int ID { get; set; }
        public string Zipcode { get; set; }
        public DateTime Booking_date { get; set; }
        public string Booking_time { get; set; }
        public string Booking_duration { get; set; }
        public int Cabinate { get; set; }
        public int Fridge { get; set; }
        public int Oven { get; set; }
        public int Laundry { get; set; }
        public int Windows { get; set; }
        public string Suggestion { get; set; }
        public bool Pets { get; set; }
        public string Street { get; set; }
        public string House_number { get; set; }
        public string PLZ { get; set; }
        public string Location { get; set; }
        public string Phone { get; set; }
        public int to_check { get; set; }
        public float Total_cost { get; set; }
        public float Discounted_cost { get; set; }
        public int? Status { get; set; }
        public string SP_Name { get; set; }
        public float SP_Ratings { get; set; }
        public int? SP_ID { get; set; }
    }
}
