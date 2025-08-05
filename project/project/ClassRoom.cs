using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace project
{
    public class ClassRoom
    {
        public int Id {  get; set; }
        public string Name { get; set; }


        public ICollection<Student> Students { get; set; }
        public ICollection<Assignment> Assignment { get; set; }
    }
}
