using System.IO;
using Akka.Actor;

namespace WinTail;

public class FileValidationActor : UntypedActor
{
    private readonly IActorRef _consoleWriterActor;
    private readonly IActorRef _tailCoordinatorActor;

    public FileValidationActor(IActorRef consoleWriterActor, IActorRef tailCoordinatorActor)
    {
        _consoleWriterActor = consoleWriterActor;
        _tailCoordinatorActor = tailCoordinatorActor;
    }

    protected override void OnReceive(object message)
    {
        var msg = message as string;

        if (string.IsNullOrEmpty(msg))
        {
            _consoleWriterActor.Tell(new Messages.NullInputError("No input received."));
            
            Sender.Tell(new Messages.ContinueProcessing());
        }
        else
        {
            var valid = IsFileUri(msg);
            if (valid)
            {
                _consoleWriterActor.Tell(new Messages.InputSuccess($"Staring processing for {message}"));
                _tailCoordinatorActor.Tell(new TailCoordinatorActor.StartTail(msg, _consoleWriterActor));
            }
            else
            {
                _consoleWriterActor.Tell(new Messages.ValidationError($"{message} is not an existing URI on disk."));
                
                Sender.Tell(new Messages.ContinueProcessing());
            }
        }

        Sender.Tell(new Messages.ContinueProcessing());
    }


    private bool IsFileUri(string path)
    {
        return File.Exists(path);
    }
}
