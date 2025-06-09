using System;
using System.Collections.Generic;
using System.Globalization;

namespace CalculatorApp
{
  public static class CalculatorLogic
  {
    // Valuta un’espressione contenente  + - * / ^ ( ) √
    public static bool Evaluate(string expr, out double result)
    {
      try
      {
        var tokens = Tokenize(expr);
        var rpn = ToRpn(tokens);
        result = EvaluateRpn(rpn);
        return true;
      }
      catch
      {
        result = double.NaN;
        return false;
      }
    }

    // Tokenizzazione
    private static List<string> Tokenize(string expr)
    {
      var tokens = new List<string>();
      int i = 0;
      while (i < expr.Length)
      {
        char c = expr[i];
        if (char.IsDigit(c) || c == '.')
        {
          string num = "";
          while (i < expr.Length && (char.IsDigit(expr[i]) || expr[i] == '.')) { num += expr[i]; i++; }
          tokens.Add(num);
          continue;
        }
        if (char.IsLetter(c))
        {
          string id = "";
          while (i < expr.Length && char.IsLetterOrDigit(expr[i])) { id += expr[i]; i++; }          // <── ora accetta anche cifre
          tokens.Add(id);
          continue;
        }
        if ("+-*/^()√".Contains(c)) { tokens.Add(c.ToString()); i++; continue; }
        i++;
      }
      return tokens;
    }

    // Shunting-yard → Reverse Polish Notation
    private static Queue<string> ToRpn(List<string> tokens)
    {
      var output = new Queue<string>();
      var ops = new Stack<string>();
      int Prec(string op) => op switch
      {
        "√" or "sin" or "cos" or "tan" or "asin" or "acos" or "atan" or "deg2rad" or "rad2deg" or "abs" => 4,
        "^" => 3,
        "*" or "/" => 2,
        "+" or "-" => 1,
        _ => 0
      };
      bool Right(string op) => op is "^" or "√" or "sin" or "cos" or "tan" or "asin" or "acos" or "atan" or "deg2rad" or "rad2deg" or "abs";
      bool Func(string op) => Right(op);
      foreach (var t in tokens)
      {
        if (double.TryParse(t, out _)) { output.Enqueue(t); continue; }
        if (t == "pi" || t == "e") { output.Enqueue(t); continue; }
        if (Func(t)) { ops.Push(t); continue; }
        if ("+-*/^".Contains(t))
        {
          while (ops.Count > 0 && Prec(ops.Peek()) > 0 && (Prec(ops.Peek()) > Prec(t) || (Prec(ops.Peek()) == Prec(t) && !Right(t)))) output.Enqueue(ops.Pop());
          ops.Push(t);
          continue;
        }
        if (t == "(") { ops.Push(t); continue; }
        if (t == ")")
        {
          while (ops.Count > 0 && ops.Peek() != "(") output.Enqueue(ops.Pop());
          if (ops.Count == 0) throw new Exception("Parentesi errata");
          ops.Pop();
          if (ops.Count > 0 && Func(ops.Peek())) output.Enqueue(ops.Pop());
        }
      }
      while (ops.Count > 0)
      {
        if (ops.Peek() is "(" or ")") throw new Exception("Parentesi errata");
        output.Enqueue(ops.Pop());
      }
      return output;
    }
    private static double EvaluateRpn(Queue<string> rpn)
    {
      var st = new Stack<double>();
      while (rpn.Count > 0)
      {
        string tk = rpn.Dequeue();
        if (double.TryParse(tk, NumberStyles.Any, CultureInfo.InvariantCulture, out double n)) { st.Push(n); continue; }
        if (tk == "pi") { st.Push(Math.PI); continue; }
        if (tk == "e") { st.Push(Math.E); continue; }
        if (tk == "√" || tk == "sin" || tk == "cos" || tk == "tan" || tk == "asin" || tk == "acos" || tk == "atan" || tk == "deg2rad" || tk == "rad2deg" || tk == "abs")
        {
          double a = st.Pop();
          st.Push(tk switch
          {
            "√" => Math.Sqrt(a),
            "sin" => Math.Sin(a),
            "cos" => Math.Cos(a),
            "tan" => Math.Tan(a),
            "asin" => Math.Asin(a),
            "acos" => Math.Acos(a),
            "atan" => Math.Atan(a),
            "deg2rad" => a * Math.PI / 180.0,
            "rad2deg" => a * 180.0 / Math.PI,
            "abs" => Math.Abs(a),
            _ => throw new InvalidOperationException()
          });
          continue;
        }
        double b = st.Pop();
        double a2 = st.Pop();
        st.Push(tk switch
        {
          "+" => a2 + b,
          "-" => a2 - b,
          "*" => a2 * b,
          "/" => b != 0 ? a2 / b : throw new DivideByZeroException(),
          "^" => Math.Pow(a2, b),
          _ => throw new InvalidOperationException()
        });
      }
      if (st.Count != 1) throw new Exception("Espressione non valida");
      return st.Pop();
    }
  }
}