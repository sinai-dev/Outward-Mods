using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Reflection;
using Mono.CSharp;
using System.IO;
using HarmonyLib;

/*
 * Credits to ManlyMarco (BepInEx team) for the REPL in RuntimeUnityEditor.
 * This is a 'lite' version of that.
*/

namespace Explorer
{
    public class ConsolePage : MainMenu.WindowPage
    {
        public override string Name { get => "Console"; set => base.Name = value; }

        private ScriptEvaluator _evaluator;
        private readonly StringBuilder _sb = new StringBuilder();

        private string MethodInput = "";
        private string UsingInput = "";

        public static List<string> UsingDirectives;

        private static readonly string[] m_defaultUsing = new string[]
        {
            "System",
            "UnityEngine",
            "System.Linq",
            "System.Collections",
            "System.Collections.Generic",
            "System.Reflection"
        };

        public override void Init()
        {
            try
            {
                //                MethodInput = @"// This is a basic REPL console used to execute a method.
                //// Some common directives are added by default, you can add more below.
                //// If you want to return some output, Debug.Log() it.

                //var ironSword = ResourcesPrefabManager.Instance.GetItemPrefab(2000010);
                //Debug.Log(ironSword.Name);";

                MethodInput = "This console is currently not working, due to lack of System.Reflection.Emit.";

                ResetConsole();
            }
            catch (Exception e)
            {
                Debug.Log($"Error setting up console!\r\nMessage: {e.Message}\r\nStack: {e.StackTrace}");
            }
        }

        public void ResetConsole()
        {
            if (_evaluator != null)
            {
                _evaluator.Dispose();
            }

            _evaluator = new ScriptEvaluator(new StringWriter(_sb)) { InteractiveBaseClass = typeof(REPL) };

            UsingDirectives = new List<string>();
            UsingDirectives.AddRange(m_defaultUsing);
            foreach (string asm in UsingDirectives)
            {
                Evaluate(AsmToUsing(asm));
            }
        }

        public string AsmToUsing(string asm, bool richtext = false)
        {
            if (richtext)
            {
                return $"<color=#569cd6>using</color> {asm};";
            }
            return $"using {asm};";
        }

        public void AddUsing(string asm)
        {
            if (!UsingDirectives.Contains(asm))
            {
                UsingDirectives.Add(asm);
                Evaluate(AsmToUsing(asm));
            }
        }

        public object Evaluate(string str)
        {
            object ret = VoidType.Value;

            _evaluator.Compile(str, out var compiled);

            try
            {
                compiled?.Invoke(ref ret);
            }
            catch (Exception e)
            {
                Debug.LogWarning(e.ToString());
            }

            return ret;
        }


        public override void DrawWindow()
        {
            GUILayout.Label("<b><size=15><color=cyan>REPL Console</color></size></b>");

            GUILayout.Label("Method:");
            MethodInput = GUILayout.TextArea(MethodInput, GUILayout.Height(300));

            if (GUILayout.Button("<color=cyan><b>Execute</b></color>"))
            {
                try
                {
                    MethodInput = MethodInput.Trim();

                    if (!string.IsNullOrEmpty(MethodInput))
                    {
                        var result = Evaluate(MethodInput);

                        if (result != null && !Equals(result, VoidType.Value))
                        {
                            Debug.Log("[Console Output]\r\n" + result.ToString());
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError("Exception compiling!\r\nMessage: " + e.Message + "\r\nStack: " + e.StackTrace);
                }
            }

            GUILayout.Label("<b>Using directives:</b>");
            foreach (var asm in UsingDirectives)
            {
                GUILayout.Label(AsmToUsing(asm, true));
            }
            GUILayout.BeginHorizontal();
            GUILayout.Label("Add namespace:", GUILayout.Width(110));
            UsingInput = GUILayout.TextField(UsingInput, GUILayout.Width(150));
            if (GUILayout.Button("Add", GUILayout.Width(50)))
            {
                AddUsing(UsingInput);
            }
            if (GUILayout.Button("<color=red>Reset</color>"))
            {
                ResetConsole();
            }
            GUILayout.EndHorizontal();
        }

        public override void Update() { }

        private class VoidType
        {
            public static readonly VoidType Value = new VoidType();
            private VoidType() { }
        }

    }

    internal class ScriptEvaluator : Evaluator, IDisposable
    {
        private static readonly HashSet<string> StdLib =
                new HashSet<string>(StringComparer.InvariantCultureIgnoreCase) { "mscorlib", "System.Core", "System", "System.Xml" };

        private readonly TextWriter _logger;

        public ScriptEvaluator(TextWriter logger) : base(BuildContext(logger))
        {
            _logger = logger;

            ImportAppdomainAssemblies(ReferenceAssembly);
            AppDomain.CurrentDomain.AssemblyLoad += OnAssemblyLoad;
        }

        public void Dispose()
        {
            AppDomain.CurrentDomain.AssemblyLoad -= OnAssemblyLoad;
            _logger.Dispose();
        }

        private void OnAssemblyLoad(object sender, AssemblyLoadEventArgs args)
        {
            string name = args.LoadedAssembly.GetName().Name;
            if (StdLib.Contains(name))
                return;
            ReferenceAssembly(args.LoadedAssembly);
        }

        private static CompilerContext BuildContext(TextWriter tw)
        {
            var reporter = new StreamReportPrinter(tw);

            var settings = new CompilerSettings
            {
                Version = LanguageVersion.Experimental,
                GenerateDebugInfo = false,
                StdLib = true,
                Target = Target.Library,
                WarningLevel = 0,
                EnhancedWarnings = false
            };

            return new CompilerContext(settings, reporter);
        }

        private static void ImportAppdomainAssemblies(Action<Assembly> import)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                string name = assembly.GetName().Name;
                if (StdLib.Contains(name))
                    continue;
                import(assembly);
            }
        }
    }
}