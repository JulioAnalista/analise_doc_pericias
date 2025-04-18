using GabIA.ENT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GabIA.WPF.Models
{
    public class Singleton
    {
        private static Singleton instance = null;
        public ProcessoCSV ProcessoAtivo { get; set; }

        private Singleton()
        {
        }

        public static Singleton Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Singleton();
                }
                return instance;
            }
        }
        public class SharedResources
        {
            public static ProcessoCSV ProcessoAtivo { get; set; }
        }

    }

}
