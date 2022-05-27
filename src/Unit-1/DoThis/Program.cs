using System;
using Akka.Actor;

namespace WinTail;

#region Program

internal class Program
{
    public static ActorSystem MyActorSystem;

    private static void Main(string[] args)
    {
        // initialize MyActorSystem
        MyActorSystem = ActorSystem.Create("MyActorSystem");

        var consoleWriterProps = Props.Create<ConsoleWriterActor>();
        var consoleWriterActor = MyActorSystem.ActorOf(consoleWriterProps, "consoleWriterActor");
        var tailCoordinatorProps = Props.Create(() => new TailCoordinatorActor());
        var tailCoordinatorActor = MyActorSystem.ActorOf(tailCoordinatorProps, "tailCoordinatorActor");
        var validationActorProps = Props.Create(() => new FileValidationActor(consoleWriterActor, tailCoordinatorActor));
        var validationActor = MyActorSystem.ActorOf(validationActorProps, "validationActor");
        var consoleReaderProps = Props.Create<ConsoleReaderActor>(validationActor);
        var consoleReaderActor = MyActorSystem.ActorOf(consoleReaderProps, "consoleReaderActor");

        // tell console reader to begin
        consoleReaderActor.Tell(ConsoleReaderActor.StartCommand);

        // blocks the main thread from exiting until the actor system is shut down
        MyActorSystem.WhenTerminated.Wait();
    }

    private static void PrintInstructions()
    {
        Console.WriteLine("Write whatever you want into the console!");
        Console.Write("Some lines will appear as");
        Console.ForegroundColor = ConsoleColor.DarkRed;
        Console.Write(" red ");
        Console.ResetColor();
        Console.Write(" and others will appear as");
        Console.ForegroundColor = ConsoleColor.Green;
        Console.Write(" green! ");
        Console.ResetColor();
        Console.WriteLine();
        Console.WriteLine();
        Console.WriteLine("Type 'exit' to quit this application at any time.\n");
    }
}

#endregion
