using Microsoft.CognitiveServices.Speech;
using static System.Console;
using System.Threading.Tasks;

namespace Apex.RaspberryPiMachineLearning.Helpers
{
    public static class SpeechHelper
    {
        private static readonly SpeechConfig config = SpeechConfig.FromSubscription("505c02237d774dc6a5488e6f67f70d0d", "northeurope");

        public static async Task Listen()
        {
            // Creates a speech recognizer.
            using var recognizer = new SpeechRecognizer(config);
            
            WriteLine("Say something...");

            // Starts speech recognition, and returns after a single utterance is recognized. The end of a
            // single utterance is determined by listening for silence at the end or until a maximum of 15
            // seconds of audio is processed.  The task returns the recognition text as result. 
            // Note: Since RecognizeOnceAsync() returns only a single utterance, it is suitable only for single
            // shot recognition like command or query. 
            // For long-running multi-utterance recognition, use StartContinuousRecognitionAsync() instead.
            var result = await recognizer.RecognizeOnceAsync();

            // Checks result.
            if (result.Reason == ResultReason.RecognizedSpeech)
            {
                WriteLine($"We recognized: {result.Text}");
            }
            else if (result.Reason == ResultReason.NoMatch)
            {
                WriteLine($"NOMATCH: Speech could not be recognized.");
            }
            else if (result.Reason == ResultReason.Canceled)
            {
                var cancellation = CancellationDetails.FromResult(result);
                ConsoleHelper.PrintCancellationReason(cancellation.Reason.ToString());
                if (cancellation.Reason == CancellationReason.Error)
                {
                    ConsoleHelper.PrintCancellationError(cancellation.ErrorCode.ToString(), cancellation.ErrorDetails);
                }
            }
        } 

        public static async Task Speak(string text)
        {
            // Creates a speech synthesizer using the default speaker as audio output.
            using var synthesizer = new SpeechSynthesizer(config);

            using var result = await synthesizer.SpeakTextAsync(text);
            if (result.Reason == ResultReason.SynthesizingAudioCompleted)
            {
                WriteLine($"Speech synthesized to speaker for text [{text}]");
            }
            else if (result.Reason == ResultReason.Canceled)
            {
                var cancellation = SpeechSynthesisCancellationDetails.FromResult(result);
                ConsoleHelper.PrintCancellationReason(cancellation.Reason.ToString());
                if (cancellation.Reason == CancellationReason.Error)
                {
                    ConsoleHelper.PrintCancellationError(cancellation.ErrorCode.ToString(), cancellation.ErrorDetails);
                }
            }
        }
    }
}
