using System;
using System.Collections.Generic;

public static class RPNEvaluator
{
    public static int Evaluate(string expression, Dictionary<string, int> variables)
    {
        if (string.IsNullOrWhiteSpace(expression))
            throw new ArgumentException("Expression is empty");

        Stack<int> stack = new Stack<int>();
        string[] tokens = expression.Split(' ');

        foreach (string token in tokens)
        {
            if (int.TryParse(token, out int number))
            {
                stack.Push(number);
            }
            else if (variables.ContainsKey(token))
            {
                stack.Push(variables[token]);
            }
            else
            {
                // Operator - need at least 2 values
                if (stack.Count < 2)
                    throw new Exception("Invalid RPN expression: not enough operands");

                int b = stack.Pop(); // right operand
                int a = stack.Pop(); // left operand

                switch (token)
                {
                    case "+": stack.Push(a + b); break;
                    case "-": stack.Push(a - b); break;
                    case "*": stack.Push(a * b); break;
                    case "/": stack.Push(b != 0 ? a / b : 0); break;
                    case "%": stack.Push(b != 0 ? a % b : 0); break;
                    default: throw new Exception($"Unknown token: {token}");
                }
            }
        }

        if (stack.Count != 1)
            throw new Exception("Invalid RPN expression: stack not reduced to one value");

        return stack.Pop();
    }
}
