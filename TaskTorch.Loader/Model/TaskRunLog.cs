using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServiceStack.DataAnnotations;
using ServiceStack.OrmLite;

namespace TaskTorch.Loader.Model
{
    public class TaskRunLog
    {
        [AutoIncrement]
        [Index(Unique = true)]
        public long Id { get; set; }
        public DateTime Date { get; set; }
        public TaskStatus Statu { get; set; }
        public string Msg { get; set; }
    }
}
