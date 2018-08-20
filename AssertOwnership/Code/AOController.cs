using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FCG.AssertOwnership
{
    public interface AOController
    {
        void Defer(HttpContext context, string[] path, int index);
    }
}