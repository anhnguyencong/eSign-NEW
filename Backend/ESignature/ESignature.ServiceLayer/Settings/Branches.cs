using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESignature.ServiceLayer.Settings
{
    public class BranchesSetting
    {
        public List<Branch> Branches { get; set; }
    }
    public class Branch
    {
        public string SignerId { get; set; }
        public string FullName { get; set; }
    }
}
