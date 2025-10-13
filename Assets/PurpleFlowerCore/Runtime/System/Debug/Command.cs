using System;
using UnityEngine;

namespace PurpleFlowerCore.PFCDebug
{
    
    public abstract class CommandBase
    {
        public abstract Type ParamType{ get; }
        public abstract void Invoke(string input);
    }
    
    public static class CommandFactory
    {
        
        public static CommandBase CreateCommand(Action action)
        {
            return new Command(action);
        }
        
        public static CommandBase CreateCommand<T>(Action<T> action)
        {
            return CreateCommand(action, typeof(T));
        }
        
        public static CommandBase CreateCommand(object action, Type type)
        {
            if (type == typeof(int))
                return new Command_Int((Action<int>)action);
            if (type == typeof(float))
                return new Command_Float((Action<float>)action);
            if (type == typeof(string))
                return new Command_String((Action<string>)action);
            if (type == typeof(bool))
                return new Command_Bool((Action<bool>)action);
            
            PFCLog.Error("DebugMenu", "Invalid input type: " + type);
            return null;
        }
    }
    
    public class Command : CommandBase
    {
        public Action Action;
        
        public Command(Action action)
        {
            Action = action;
        }
        
        public static implicit operator Command(Action a) => new(a);


        public override Type ParamType => null;

        public override void Invoke(string _)
        {
            Action?.Invoke();
        }
    }
    
    public class Command_Int : CommandBase
    {
        public Action<int> Action;
        
        public Command_Int(Action<int> action)
        {
            Action = action;
        }
        
        public static implicit operator Command_Int(Action<int> a) => new(a);

        public override Type ParamType => typeof(int);

        public override void Invoke(string input)
        {
            if (int.TryParse(input, out int result))
            {
                Action?.Invoke(result);
            }
            else
            {
                PFCLog.Error("DebugMenu", "Invalid input: " + input);
            }
        }
    }
    
    public class Command_Float : CommandBase
    {
        public Action<float> Action;
        
        public Command_Float(Action<float> action)
        {
            Action = action;
        }
        
        public static implicit operator Command_Float(Action<float> a) => new(a);

        public override Type ParamType => typeof(float);

        public override void Invoke(string input)
        {
            if (float.TryParse(input, out float result))
            {
                Action?.Invoke(result);
            }
            else
            {
                PFCLog.Error("DebugMenu", "Invalid input: " + input);
            }
        }
    }
    
    public class Command_String : CommandBase
    {
        public Action<string> Action;
        
        public Command_String(Action<string> action)
        {
            Action = action;
        }
        
        public static implicit operator Command_String(Action<string> a) => new(a);

        public override Type ParamType => typeof(string);

        public override void Invoke(string input)
        {
            Action?.Invoke(input);
        }
    }
    
    public class Command_Bool : CommandBase
    {
        public Action<bool> Action;
        
        public Command_Bool(Action<bool> action)
        {
            Action = action;
        }
        
        public static implicit operator Command_Bool(Action<bool> a) => new(a);

        public override Type ParamType => typeof(bool);

        public override void Invoke(string input)
        {
            if (bool.TryParse(input, out bool result))
            {
                Action?.Invoke(result);
            }
            else
            {
                PFCLog.Error("DebugMenu", "Invalid input: " + input);
            }
        }
    }
    
    public class Command_int_int : CommandBase
    {
        public Action<(int,int)> Action;
        
        public Command_int_int(Action<(int,int)> action)
        {
            Action = action;
        }
        
        public static implicit operator Command_int_int(Action<(int,int)> a) => new(a);
        
        public override Type ParamType => typeof((int,int));
        
        public override void Invoke(string input)
        {
            string[] inputs = input.Split(' ');
            if (inputs.Length == 2 && int.TryParse(inputs[0], out int result1) && int.TryParse(inputs[1], out int result2))
            {
                Action?.Invoke((result1, result2));
            }
            else
            {
                PFCLog.Error("DebugMenu", "Invalid input: " + input);
            }
        }
    }
}