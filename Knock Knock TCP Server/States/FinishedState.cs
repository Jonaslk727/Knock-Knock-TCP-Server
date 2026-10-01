using System;
using System.Collections.Generic;
using System.Text;

namespace Knock_Knock_TCP_Server.States
{
    internal class FinishedState : IKnockKnockState
    {
        public void Handle(Knock_KnockServer server)
        {
            Console.WriteLine("Finished state reached. Closing connection.");
        }
        
    }
}
