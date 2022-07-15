using System;
using System.Collections.Generic;
using Discord.WebSocket;

namespace bot
{
    internal class DialogueBase : IDialogue
    {
        public void PingUser(ulong userId)
        {
            _userId = userId;
        }

        public void AppendResponse(string response)
        {
            _outBuffer += response;
        }

        public DialogueStatus Update(SocketMessage msg)
        {
            if (msg.Content == "terminate")
            {
                msg.Channel.SendMessageAsync((_userId != 0 ? $"<@!{_userId}> " : "") + "Terminating dialogue");
                return DialogueStatus.Finished;
            }

            // This is here in case the dialogue does not get terminated
            // right after finishing/encountering an error.
            if (_currentState == "error" || _currentState == "final") return DialogueStatus.Error;

            if (_transitionFunc.TryGetValue(_currentState, out var func))
            {
                _currentState = func(_currentState, msg);
            }
            else
            {
                msg.Channel.SendMessageAsync("The dialogue is broken, please report this issue to the author.");
            }

            if (_outBuffer != "")
            {
                msg.Channel.SendMessageAsync((_userId != 0 ? $"<@!{_userId}> " : "") + _outBuffer);
                _outBuffer = "";
            }

            if (_currentState == "final") return DialogueStatus.Finished;
            if (_currentState == "error") return DialogueStatus.Error;

            return DialogueStatus.Continue;
        }

        public void AddTransition(string startState, Func<string, SocketMessage, string> transition)
        {
            _transitionFunc[startState] = transition;
        }

        protected string _currentState = "start";
        string _outBuffer = "";
        ulong _userId = 0;
        Dictionary<string, Func<string, SocketMessage, string>> _transitionFunc = new();
    }
}
