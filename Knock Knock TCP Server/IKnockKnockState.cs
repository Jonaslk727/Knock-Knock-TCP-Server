
using System;
using System.Collections.Generic;
using System.Text;

namespace Knock_Knock_TCP_Server
{
    internal interface IKnockKnockState
    {
        void Handle(Knock_KnockServer server);

    }
}
