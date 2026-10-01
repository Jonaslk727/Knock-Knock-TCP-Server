using System;
using System.Collections.Generic;
using System.Text;

namespace Knock_Knock_TCP_Server.States
{
    internal class WaitingForJokeState : IKnockKnockState
    {
        public void Handle(Knock_KnockServer server)
        {
            string? message = server.ReadMessage ();
            if (!string.IsNullOrEmpty (message))
            {
                server.SendMessage ($"Ha! {message}");
                server.SetState (new FinishedState ());
            }
        }
    }
}
