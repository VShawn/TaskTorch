using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskLoader
{
    public interface ITaskInterpreter
    {
        //void Excute();
        string ToYmlString();
        bool FromYmlString(string ymlString);
    }
}
