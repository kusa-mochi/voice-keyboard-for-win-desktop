using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Speech.Recognition;
using System.Text;
using System.Threading.Tasks;

namespace VoiceLauncher
{
    class Program
    {
        List<GrammarStruct> _grammars = new List<GrammarStruct>();

        static void Main(string[] args)
        {
            var p = new Program();
            p.Start();
        }

        public void Start()
        {
            // 設定ファイルから音声コマンドの設定情報を取得する。
            using (var fileReader = new StreamReader(Properties.Settings.Default.SettingFilePath))
            {
                string line = "";
                while ((line = fileReader.ReadLine()) != null)
                {
                    string[] splitedLine = line.Split(',');
                    _grammars.Add(
                        new GrammarStruct()
                        {
                            Grammar = splitedLine[0],
                            ExecuteCommand = splitedLine[1],
                            Parameters = splitedLine[2]
                        }
                        );
                }
            }

            // 音声認識エンジンを初期化する。
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

            // 認識した言葉に応じてPCを操作する。
            for (int i = 0; i < _grammars.Count; i++)
            {
                if (e.Result.Text == _grammars[i].Grammar)
                {
                    Process process = new Process();
                    process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    process.StartInfo.FileName = _grammars[i].ExecuteCommand;
                    process.StartInfo.Arguments = _grammars[i].Parameters;
                    process.Start();
                    break;
                }
            }
        }
    }
}
