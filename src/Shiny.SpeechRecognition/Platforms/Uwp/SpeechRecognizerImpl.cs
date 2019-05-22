﻿using System;
using System.Threading.Tasks;
using System.Reactive.Linq;
using Windows.Foundation;
using Windows.Media.SpeechRecognition;
using WinSpeechRecognizer = Windows.Media.SpeechRecognition.SpeechRecognizer;


namespace Shiny.SpeechRecognition
{
    public class SpeechRecognizerImpl : AbstractSpeechRecognizer
    {
        // TODO: appmanifest
        public override Task<AccessState> RequestAccess() => Task.FromResult(AccessState.Available);
            //Permissions.IsInMainfest("speech")


        public override IObservable<string> ListenUntilPause() => Observable.FromAsync(async ct =>
        {
            var speech = new WinSpeechRecognizer();
            await speech.CompileConstraintsAsync();
            this.ListenSubject.OnNext(true);
            var result = await speech.RecognizeAsync();
            this.ListenSubject.OnNext(false);

            return result.Text;
        });


        public override IObservable<string> ContinuousDictation() => Observable.Create<string>(async ob =>
        {
            var speech = new WinSpeechRecognizer();
            await speech.CompileConstraintsAsync();

            var handler = new TypedEventHandler<SpeechContinuousRecognitionSession, SpeechContinuousRecognitionResultGeneratedEventArgs>((sender, args) =>
                ob.OnNext(args.Result.Text)
            );
            speech.ContinuousRecognitionSession.ResultGenerated += handler;
            await speech.ContinuousRecognitionSession.StartAsync();
            this.ListenSubject.OnNext(true);

            return () =>
            {
                //speech.ContinuousRecognitionSession.StopAsync();
                speech.ContinuousRecognitionSession.ResultGenerated -= handler;
                this.ListenSubject.OnNext(false);
                speech.Dispose();
            };
        });


        //        //{
        //        //    var grammar = new SpeechRecognitionTopicConstraint(SpeechRecognitionScenario.Dictation, "webSearch");
        //        //    speech.UIOptions.AudiblePrompt = "Say what you want to search for...";
        //        //    speech.UIOptions.ExampleText = @"Ex. &#39;weather for London&#39;";
        //        //    speech.Constraints.Add(webSearchGrammar);
        //        //}
        //        //speech.ContinuousRecognitionSession.AutoStopSilenceTimeout = TimeSpan.FromDays(1)
    }
}