using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Debricked.Models.Constants
{
    internal enum CLProjectTypeEnum
    {
        Unknown = -1,
        CSProj = 0, //C#
        FsProj = 1, //F#
        VbProj = 2, //VB
        DcProj = 3, //Docker compose
        NjsProj = 4, //node js
        PyProj = 5, //python
        EsProj = 6, //javascript
    }
}
