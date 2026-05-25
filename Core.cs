using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace uch
{
    internal class Core
    {
        public static uchprEntities2 Context = new uchprEntities2();
        public static Users CurrentUser { get; set; }
        public static Users TempFrozenUser { get; set; }
        public static bool IsAdmin => CurrentUser?.Roles?.Name == "Admin";
        public static bool IsAuthenticated => CurrentUser != null;
        public static bool IsAuthor => CurrentUser?.Roles?.Name == "Author";
    }
}
