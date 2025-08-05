using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace project
{
    public class Student
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int ClassRoomId { get; set; }

        public ClassRoom ClassRoom { get; set; }
        public ICollection<Grade> Grades { get; set; }
    }
}
