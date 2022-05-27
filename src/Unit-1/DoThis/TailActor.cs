using System.IO;
using System.Text;
using Akka.Actor;

namespace WinTail;

public class TailActor : UntypedActor
{
    private readonly string _filePath;
    private readonly FileObserver _observer;
    private readonly IActorRef _reporterActor;
    private readonly Stream _fileStream;
    private readonly StreamReader _fileStreamReader;


    public TailActor(IActorRef reporterActor, string filePath)
    {
        _reporterActor = reporterActor;
        _filePath = filePath;
        
        // start watching the file for changes.
        _observer = new FileObserver(Self, Path.GetFullPath(_filePath));
        _observer.Start();
        
        // Open the file stream with shared read/write permissions
        // (so file can be written to while open)
        _fileStream = new FileStream(Path.GetFullPath(_filePath), FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        _fileStreamReader = new StreamReader(_fileStream, Encoding.UTF8);

        // read the initial contents of the file and send it to console as first msg
        var text = _fileStreamReader.ReadToEnd();
        Self.Tell(new InitialRead(_filePath, text));
    }

    protected override void OnReceive(object message)
    {
        if (message is FileWrite)
        {
            var text = _fileStreamReader.ReadToEnd();
            if (!string.IsNullOrEmpty(text))
            {
                _reporterActor.Tell(text);
            }
        }
        else if (message is FileError fileError)
        {
            _reporterActor.Tell($"Tail error: {fileError.Reason}");
        }
        else if (message is InitialRead initialRead)
        {
            _reporterActor.Tell(initialRead.Text);
        }
    }

    public class FileWrite
    {
        public FileWrite(string fileName)
        {
            FileName = fileName;
        }

        public string FileName { get; }
    }

    public class FileError
    {
        public FileError(string fileName, string reason)
        {
            FileName = fileName;
            Reason = reason;
        }

        public string FileName { get; }

        public string Reason { get; }
    }

    public class InitialRead
    {
        public InitialRead(string fileName, string text)
        {
            FileName = fileName;
            Text = text;
        }

        public string FileName { get; }

        public string Text { get; }
    }
}
