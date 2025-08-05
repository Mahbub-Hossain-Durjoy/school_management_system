using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace project
{
    public class Assignment
    {
        public int Id { get; set; }
        public int? TeacherId { get; set; }
        public int ClassRoomId { get; set; }
        public int SubjectId { get; set; }


        public Teacher Teacher { get; set; }

        public ClassRoom ClassRoom { get; set; }
        public Subject Subject { get; set; }
        public ICollection<Grade> Grades { get; set; }

    }

    
}
