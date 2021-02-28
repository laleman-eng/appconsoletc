using System;
using System.Collections.Generic;
using System.Text;

namespace AppConsoleTC
{
   public class Param
    {
        public string Url { get; set; }

        public int dias { get; set; }

        public string  stringConection { get; set; }

        public string database { get; set; }

        public string collection { get; set; }

        public List<Series> Series { get; set; }
    }

    public class Series
    {
        public string serie { get; set; }
        public string descripcion { get; set; }
        public string codigo { get; set; }
    }

}
