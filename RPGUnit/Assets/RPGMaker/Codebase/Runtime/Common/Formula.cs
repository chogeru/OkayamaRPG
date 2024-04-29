#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace RPGMaker.Codebase.Runtime.Common
{
    [CustomEditor(typeof(Formula))]
    public class FormulaInspector : Editor
    {
        public override void OnInspectorGUI() {
            if (GUILayout.Button("Parse", GUILayout.Width(100)))
            {
                var g = target as Formula;
                Eval(g.formula);
            }

            base.OnInspectorGUI();
        }

        private static void Eval(string formula) {
            formula = formula.Replace("num", "10");

            var list = new List<char>(formula);

            list.RemoveAll(x => x == ' ');

            var c = list.ToArray();

            var node = Parse(c);

            var result = Eval(node);
        }

        private static bool IsNumber(char c) {
            return char.IsDigit(c) || c == 'x' || c == 'X' || c == '#';
        }

        private static double Eval(Node node) {
            List<string> ns;
            List<char> os;

            LexicalAnalysis(node.formula, out ns, out os);

            var numbers = new List<double>();
            {
                var child = 0;
                for (var i = 0; i < ns.Count; i++)
                {
                    var num = 0.0;

                    switch (ns[i])
                    {
                        case "#":
                            num = Eval(node.childs[child++]);
                            break;
                        default:
                            double.TryParse(ns[i], out num);
                            break;
                    }

                    numbers.Add(num);
                }
            }

            {
                for (var i = 0; i < os.Count;)
                    switch (os[i])
                    {
                        case '*':
                        {
                            var left = numbers[i];
                            var right = numbers[i + 1];
                            numbers[i] = left * right;
                            numbers.RemoveAt(i + 1);
                            os.RemoveAt(i);
                        }
                            break;
                        case '/':
                        {
                            var left = numbers[i];
                            var right = numbers[i + 1];
                            numbers[i] = left / right;
                            numbers.RemoveAt(i + 1);
                            os.RemoveAt(i);
                        }
                            break;
                        default:
                            i++;
                            break;
                    }
            }

            var total = numbers[0];
            {
                for (var i = 0; i < os.Count; i++)
                    switch (os[i])
                    {
                        case '+':
                            total += numbers[i + 1];
                            break;
                        case '-':
                            total -= numbers[i + 1];
                            break;
                    }
            }

            return total;
        }

        private static void LexicalAnalysis(string str, out List<string> ns, out List<char> os) {
            ns = new List<string>();
            os = new List<char>();

            var text = "";
            for (var i = 0; i < str.Length; i++)
                switch (str[i])
                {
                    case '+':
                    case '-':
                    case '*':
                    case '/':
                        ns.Add(text);
                        os.Add(str[i]);
                        text = "";
                        break;
                    default:
                        if (IsNumber(str[i]) || str[i] == '.')
                        {
                            text += str[i];
                            if (i == str.Length - 1)
                            {
                                ns.Add(text);
                                text = "";
                            }
                        }

                        break;
                }
        }

        private static Node Parse(char[] c) {
            var root = new Node();
            var target = root;

            for (var i = 0; i < c.Length; i++)
                switch (c[i])
                {
                    case '(':
                    {
                        target.formula += "#";

                        var node = new Node();
                        target.Add(node);
                        target = node;
                    }
                        break;
                    case ')':
                    {
                        target = target.parent;
                    }
                        break;
                    default:
                        target.formula += c[i];
                        break;
                }

            return root;
        }

        public class Node
        {
            public List<Node> childs  = new List<Node>();
            public string     formula = "";

            public Node parent { get; private set; }

            public void Add(Node node) {
                node.parent = this;
                childs.Add(node);
            }

            public void Log() {
                foreach (var child in childs) child.Log();
            }
        }
    }

    public class Formula : MonoBehaviour
    {
        public string formula = "1+2*(3+4)+5*(6*(7+8)+(9+10))";
    }
#endif

#if UNITY_EDITOR
    public class FormulaNodeWindow : EditorWindow
    {
        public static FormulaInspector.Node RootNode;

        private static int id;

        protected void OnGUI() {
            BeginWindows();
            if (RootNode != null)
            {
                id = 0;
                Draw(RootNode, new Vector2(200, 200));
            }

            EndWindows();
        }

        private static Rect GetWindowRect(Vector2 pos) {
            const int SizeX = 120;
            const int SizeY = 45;
            var window = new Rect(pos, new Vector2(SizeX, SizeY));
            return window;
        }

        public void Draw(FormulaInspector.Node node, Vector2 position) {
            var window = GetWindowRect(position);
            GUI.Window(id++, window, DrawNodeWindow, node.formula);

            var left = position + new Vector2(-100, 100);
            var right = position + new Vector2(100, 100);
            var center = position + new Vector2(0, 100);
            var n = node.childs.Count;

            if (n == 1)
            {
                var childPos = center;
                var childWindow = GetWindowRect(childPos);

                DrawNodeLine(window, childWindow);
                Draw(node.childs[0], childPos);
            }
            else if (n > 1)
            {
                for (var i = 0; i < n; i++)
                {
                    float t = i / (n - 1);
                    var childPos = Vector2.Lerp(left, right, t);
                    var childWindow = GetWindowRect(childPos);

                    DrawNodeLine(window, childWindow);
                    Draw(node.childs[i], childPos);
                }
            }
        }

        private void DrawNodeWindow(int id) {
            GUI.DragWindow();
        }

        private static void DrawNodeLine(Rect start, Rect end) {
            var startPos = new Vector3(start.x + start.width / 2, start.y + start.height / 2, 0);
            var endPos = new Vector3(end.x + end.width / 2, end.y + end.height / 2, 0);
            var shadowCol = new Color(0, 0, 0, 0.06f);

            Handles.DrawLine(startPos, endPos);
        }
    }
}

#endif