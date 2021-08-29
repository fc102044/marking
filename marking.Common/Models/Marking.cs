using System;
namespace marking.Common.Models
{
    public class Marking
    {
        public int IdEmpleo { get; set; }
        public DateTime DateTimeInOrOut { get; set; }
        public int Tipo { get; set; }
        public bool consolidated { get; set; }
    }
}
