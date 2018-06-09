using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Speech.Recognition;
using System.Text;
using System.Threading.Tasks;

namespace VoiceKeyboard
{
    class Program
    {
        List<GrammarStruct> _grammars = new List<GrammarStruct>();
        //    {
        //        new GrammarStruct() { Grammar = "いち", KeyboardString = "1"},
        //        new GrammarStruct() { Grammar = "に", KeyboardString = "2"},
        //        new GrammarStruct() { Grammar = "3", KeyboardString = "3"},
        //        new GrammarStruct() { Grammar = "し", KeyboardString = "4"},
        //        new GrammarStruct() { Grammar = "よん", KeyboardString = "4"},
        //        new GrammarStruct() { Grammar = "ご", KeyboardString = "5"},
        //        new GrammarStruct() { Grammar = "ろく", KeyboardString = "6"},
        //        new GrammarStruct() { Grammar = "なな", KeyboardString = "7"},
        //        new GrammarStruct() { Grammar = "しち", KeyboardString = "7"},
        //        new GrammarStruct() { Grammar = "はち", KeyboardString = "8"},
        //        new GrammarStruct() { Grammar = "きゅう", KeyboardString = "9"},
        //        new GrammarStruct() { Grammar = "く", KeyboardString = "9"},
        //        new GrammarStruct() { Grammar = "じゅう", KeyboardString = "10"},
        //        new GrammarStruct() { Grammar = "とお", KeyboardString = "10"}
        //};

        static void Main(string[] args)
        {
            Program p = new Program();
            p.Start();
        }

        public void Start()
        {
            for (double d = 0.0; d < 10.0; d += 0.1)
            {
                _grammars.Add(new GrammarStruct() { Grammar = d.ToString(), KeyboardString = d.ToString() });
            }
            var words = GrammarsStruct2GrammarsStringArray(_grammars);
            var choices = new Choices(words);
            var gBuilder = new GrammarBuilder(choices);
            var grammar = new Grammar(gBuilder);

            // .NETの機能で音声認識を行う。
            using (
            SpeechRecognitionEngine recognizeEngine =
              new SpeechRecognitionEngine(
                new CultureInfo("ja-JP")))
            {
                // Create and load a dictation grammar.
                //recognizeEngine.LoadGrammar(new DictationGrammar());
                recognizeEngine.LoadGrammar(grammar);

                // Add a handler for the speech recognized event.
                recognizeEngine.SpeechRecognized += new EventHandler<SpeechRecognizedEventArgs>(recognizer_SpeechRecognized);

                // Configure input to the speech recognizer.
                recognizeEngine.SetInputToDefaultAudioDevice();

                // Start asynchronous, continuous speech recognition.
                recognizeEngine.RecognizeAsync(RecognizeMode.Multiple);

                Console.WriteLine("準備OK");

                // Keep the console window open.
                while (true)
                {
                    Console.ReadLine();
                }
            }
        }

        private string[] GrammarsStruct2GrammarsStringArray(List<GrammarStruct> grammars)
        {
            int numGrammars = grammars.Count;
            string[] output = new string[numGrammars];

            for (int i = 0; i < numGrammars; i++)
            {
                output[i] = grammars[i].Grammar;
            }

            return output;
        }

        private void recognizer_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            Console.WriteLine("Recognized text: " + e.Result.Text);

            // キーボードイベントをエミュレートする。
            for (int i = 0; i < _grammars.Count; i++)
            {
                if (e.Result.Text == _grammars[i].Grammar)
                {
                    string keyboardString = _grammars[i].KeyboardString;
                    foreach (char c in keyboardString)
                    {
                        Win32ApiWrapper.keybd_event((byte)c, 0, 0, (UIntPtr)0);
                    }
                }
            }
        }
    }
}
