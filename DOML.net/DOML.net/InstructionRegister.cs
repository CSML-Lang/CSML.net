﻿using System;
using System.Collections.Generic;
using System.Text;
using DOML.Logger;

namespace DOML.ByteCode
{
    /// <summary>
    /// A register of all the instructions.
    /// </summary>
    public static class InstructionRegister
    {
        /// <summary>
        /// These represent the actions to run.
        /// </summary>
        internal static Dictionary<string, Action<InterpreterRuntime>> Actions { get; } = new Dictionary<string, Action<InterpreterRuntime>>();

        /// <summary>
        /// Expected size of operations.
        /// Either amount of parameters or amount of return variables.
        /// </summary>
        internal static Dictionary<string, int> SizeOf { get; } = new Dictionary<string, int>();


        /// <summary>
        /// Register a getter function.
        /// </summary>
        /// <param name="name"> Function name. </param>
        /// <param name="forObject"> The object name this refers to. </param>
        /// <param name="returnCount"> The amount of return variables. </param>
        /// <param name="func"> The function. </param>
        public static void RegisterGetter(string name, string forObject, int returnCount, Action<InterpreterRuntime> func) => RegisterActionAndSizeOf($"get {forObject}::{name}", returnCount, func);

        /// <summary>
        /// Register a setter function.
        /// </summary>
        /// <param name="name"> Function name. </param>
        /// <param name="forObject"> The object name this refers to. </param>
        /// <param name="parameterCount"> The amount of parameter variables. </param>
        /// <param name="func"> The function. </param>
        public static void RegisterSetter(string name, string forObject, int parameterCount, Action<InterpreterRuntime> func) => RegisterActionAndSizeOf($"set {forObject}::{name}", parameterCount, func);

        /// <summary>
        /// Unregisters a getter function.
        /// </summary>
        /// <param name="name"> Function name. </param>
        /// <param name="forObject"> The object name this refers to. </param>
        public static void UnRegisterGetter(string name, string forObject) => UnRegisterActionAndSizeOf($"get {forObject}::{name}");

        /// <summary>
        /// Unregisters a setter function.
        /// </summary>
        /// <param name="name"> Function name. </param>
        /// <param name="forObject"> The object name this refers to. </param>
        public static void UnRegisterSetter(string name, string forObject) => UnRegisterActionAndSizeOf($"set {forObject}::{name}");

        /// <summary>
        /// Registers a constructor.
        /// </summary>
        /// <param name="name"> Function name. </param>
        /// <param name="func"> The function. </param>
        public static void RegisterConstructor(string name, Action<InterpreterRuntime> func)
        {
            name = "new " + name;
            if (Actions.ContainsKey(name) == false)
            {
                Actions.Add(name, func);
            }
            else
            {
                Log.Warning($"Action already exists for {name}, will set to the one provided");
                Actions[name] = func;
            }
        }

        /// <summary>
        /// Unregisters a constructor.
        /// </summary>
        /// <param name="name"> Function name. </param>
        public static void UnRegisterConstructor(string name)
        {
            Actions.Remove("new " + name);
        }

        /// <summary>
        /// Clears all instructions.
        /// Doesn't include system made ones.
        /// </summary>
        public static void ClearInstructions()
        {
            Actions.Clear();
        }

        /// <summary>
        /// Unregisters action for name and sizeof operators.
        /// </summary>
        /// <param name="name"> The name to unregister. </param>
        private static void UnRegisterActionAndSizeOf(string name)
        {
            Actions.Remove(name);
            SizeOf.Remove(name);
        }

        /// <summary>
        /// Registers action and sizeof.
        /// </summary>
        /// <param name="name"> The function name. </param>
        /// <param name="amount"> The amount of sizeofs. </param>
        /// <param name="func"> The function. </param>
        private static void RegisterActionAndSizeOf(string name, int amount, Action<InterpreterRuntime> func)
        {
            if (Actions.ContainsKey(name) == false)
            {
                Actions.Add(name, func);
            }
            else
            {
                Log.Warning($"Action already exists for {name}, will set to the one provided");
                Actions[name] = func;
            }

            if (SizeOf.ContainsKey(name))
            {
                Log.Warning($"SizeOf already exists for {name}, will choose the higher variant.");
                if (SizeOf[name] < amount)
                    SizeOf[name] = amount;
            }
            else
            {
                SizeOf.Add(name, amount);
            }
        }
    }
}