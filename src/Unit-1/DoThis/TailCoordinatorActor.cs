using System;
using Akka.Actor;

namespace WinTail;

public class TailCoordinatorActor : UntypedActor
{
    protected override void OnReceive(object message)
    {
        if (message is StartTail startTail)
        {
            // Here we are creating the parent/child relationship. The TailActor instance created here
            // is a child of this instance of TailCoordinatorActor
            Context.ActorOf(Props.Create(() => new TailActor(startTail.ReporterActor, startTail.FilePath)));
        }
    }

    protected override SupervisorStrategy SupervisorStrategy()
    {
        return new OneForOneStrategy(10,
            TimeSpan.FromSeconds(30),
            x =>
            {
                return x switch
                {
                    ArithmeticException => Directive.Resume,
                    NotSupportedException => Directive.Stop,
                    _ => Directive.Restart
                };
            });
    }

    public class StartTail
    {
        public StartTail(string filePath, IActorRef reporterActor)
        {
            FilePath = filePath;
            ReporterActor = reporterActor;
        }

        public string FilePath { get; }

        public IActorRef ReporterActor { get; }
    }

    public class StopTail
    {
        public StopTail(string filePath)
        {
            FilePath = filePath;
        }

        public string FilePath { get; }
    }
}