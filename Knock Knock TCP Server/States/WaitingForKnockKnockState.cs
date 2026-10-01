using System;
using System.Collections.Generic;
using System.Text;
using Knock_Knock_TCP_Server;

namespace Knock_Knock_TCP_Server.States
{
    internal class WaitingForKnockKnockState : IKnockKnockState
    {
        public void Handle(Knock_KnockServer server)
        {
            string? message = server.ReadMessage ();

            if (message?.ToLower() == "knock knock!")
            {
                server.SendMessage ("Who's there?");
                server.SetState (new WaitningForSetupState ());
            }
        }
    }
}
