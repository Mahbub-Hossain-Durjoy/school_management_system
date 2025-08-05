using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace project
{
    public class Grade
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public int AssignmentId { get; set; }
        public Term Term { get; set; }
        public double GradeValue { get; set; }

        public Student Student { get; set; }
        public Assignment Assignment { get; set; }
    }
}
