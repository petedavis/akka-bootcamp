using System;
using Akka.Actor;

namespace WinTail;

/// <summary>
///     Actor responsible for serializing message writes to the console.
///     (write one message at a time, champ :)
/// </summary>
internal class ConsoleWriterActor : UntypedActor
{
    protected override void OnReceive(object message)
    {
        if (message is Messages.InputError inputErrorMsg)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(inputErrorMsg.Reason);
        }
        else if (message is Messages.InputSuccess inputSuccessMsg)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(inputSuccessMsg.Reason);
        }
        else
        {
            Console.WriteLine(message);
        }

        Console.ResetColor();
    }
}
