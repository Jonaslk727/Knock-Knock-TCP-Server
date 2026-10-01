using System;
using System.Collections.Generic;
using System.Text;
using Knock_Knock_TCP_Server;

namespace Knock_Knock_TCP_Server.States
{
    internal class WaitningForSetupState : IKnockKnockState
    {

        public void Handle(Knock_KnockServer server)
        {
            string? message = server.ReadMessage ();

            if (!string.IsNullOrEmpty(message))
            {
                server.SendMessage ($"{message}, who?");
                server.SetState (new WaitingForJokeState ());
            }
        }
    }
}
