﻿#region License
// ====================================================
// Team DOML Copyright(C) 2017 Team DOML
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion

using System;
using System.Text;
using System.IO;
using DOML.Logger;

namespace DOML.IR
{
    /// <summary>
    /// This class writes all the IR instructions.
    /// </summary>
    public class IRWriter : IDisposable
    {
        /// <summary>
        /// The writer.
        /// </summary>
        private TextWriter writer;

        /// <summary>
        /// Creeate an IR writer for a file.
        /// </summary>
        /// <param name="filePath"> The file path. </param>
        /// <param name="append"> Append to file or overwrite. </param>
        public IRWriter(string filePath, bool append) : this(new StreamWriter(File.Exists(filePath)
            ? File.Open(filePath, FileMode.Truncate)
            : new FileStream(filePath, append ? FileMode.Append : FileMode.Create)))
        { }

        /// <summary>
        /// Creates an IR writer to write to a string builder.
        /// </summary>
        /// <param name="resultText"> The builder to write to. </param>
        public IRWriter(StringBuilder resultText) : this(new StringWriter(resultText))
        { }

        /// <summary>
        /// Creates an IR writer to write to any text writer.
        /// </summary>
        /// <param name="writer"> The writer to write to. </param>
        public IRWriter(TextWriter writer) => this.writer = writer;

        /// <summary>
        /// Deconstructor for IR writer.
        /// Means you don't have to dispose of it.
        /// </summary>
        ~IRWriter()
        {
            // Dispose of writer.
            Dispose();
        }

        /// <summary>
        /// Emits all the instructions to a file path.
        /// </summary>
        /// <param name="interpreter"> The interpreter to print all the commands off. </param>
        /// <param name="filePath"> The path to write to. </param>
        /// <param name="append"> Append or overwrite. </param>
        /// <param name="withLineComments"> Write line comments. </param>
        public static void EmitToLocation(Interpreter interpreter, string filePath, bool append, bool withLineComments)
        {
            using (IRWriter writer = new IRWriter(filePath, append))
            {
                writer.Emit(interpreter, withLineComments);
            }
        }

        /// <summary>
        /// Emits all the instructions to a string builder.
        /// </summary>
        /// <param name="interpreter"> The interpreter to print all the commands off. </param>
        /// <param name="builder"> The builder to write to. </param>
        /// <param name="withLineComments"> Write line comments. </param>
        public static void EmitToString(Interpreter interpreter, StringBuilder builder, bool withLineComments)
        {
            using (IRWriter writer = new IRWriter(builder))
            {
                writer.Emit(interpreter, withLineComments);
            }
        }

        /// <summary>
        /// Emits all the instructions to this writer.
        /// </summary>
        /// <param name="interpreter"> The interpreter to print all the commands off. </param>
        /// <param name="withLineComments"> Add line comments. </param>
        public void Emit(Interpreter interpreter, bool withLineComments)
        {
            WriteHeader();

            if (interpreter == null)
                return;

            for (int i = 0; i < interpreter.Instructions.Count; i++)
            {
                WriteInstructionText(interpreter.Instructions[i], withLineComments, i != interpreter.Instructions.Count - 1);
            }

            Log.Info("Emitted IR");
        }

        /// <summary>
        /// Writes the header.
        /// Which consists of a warning that changes won't persist.
        /// </summary>
        public void WriteHeader()
        {
            writer.WriteLine("; This is the resulting bytecode from the file given\n; This bytecode will be overriden if new bytecode is generated.");
        }

        /// <summary>
        /// Writes the text for an instruction.
        /// </summary>
        /// <param name="instruction"> The instruction. </param>
        /// <param name="withComment"> Write a description comment.</param>
        /// <param name="includeComma"> Include a comma, or not.  Only done if <paramref name="withComment"/> is false. </param>
        /// <remarks> Spaces out instructions to provide nice indentation and spacing. </remarks>
        public void WriteInstructionText(Instruction instruction, bool withComment, bool includeComma)
        {
            if (withComment)
            {
                writer.WriteLine("{0:D2} {1,-50} ; {2,10}", instruction.OpCode, GetParameterText(instruction).Trim(), GetCommentEmit(instruction));
            }
            else
            {
                writer.Write("{0} {1}{2}", instruction.OpCode, GetParameterText(instruction), includeComma ? ", " : " ");
            }
        }

        /// <summary>
        /// Gets the comment emit for a specific instruction.
        /// </summary>
        /// <param name="instruction"> The instruction. </param>
        /// <returns> The corresponding comment. </returns>
        public string GetCommentEmit(Instruction instruction)
        {
            switch ((Opcodes)instruction.OpCode)
            {
            case Opcodes.NOP:
                return $"Deliberate void instruction";
            case Opcodes.COMMENT:
                return $"USER COMMENT";
            case Opcodes.MAKE_SPACE:
                return $"Reserves {GetParameterText(instruction)} space" + ((int)instruction.Parameter != 1 ? "s" : "") + " on the stack.";
            case Opcodes.MAKE_REG:
                return $"Reserves {GetParameterText(instruction)} space" + ((int)instruction.Parameter != 1 ? "s" : "") + " on the register.";

            case Opcodes.SET:
                return $"Runs the {GetParameterText(instruction)} function";
            case Opcodes.COPY:
                return $"Copies top value {GetParameterText(instruction)} time" + ((int)instruction.Parameter != 1 ? "s" : "") + ", aka a peek and push";
            case Opcodes.REG_OBJ:
                return $"Registers top object to index {GetParameterText(instruction)} after popping it off the stack";
            case Opcodes.UNREG_OBJ:
                return $"Unregisters object at index {GetParameterText(instruction)}";

            case Opcodes.PUSH_OBJ:
                return $"Pushes object in register ID: {GetParameterText(instruction)} onto the stack";
            case Opcodes.PUSH_INT:
                return $"Pushes long integer {GetParameterText(instruction)} onto the stack";
            case Opcodes.PUSH_NUM:
                return $"Pushes double {GetParameterText(instruction)} onto the stack";
            case Opcodes.PUSH_STR:
                return $"Pushes string {GetParameterText(instruction)} onto the stack";
            case Opcodes.PUSH_BOOL:
                return $"Pushes boolean {GetParameterText(instruction)} onto the stack";
            case Opcodes.PUSH:
                return $"Performs an unsafe push, pushing {GetParameterText(instruction)} onto the stack regardless of its type";
            case Opcodes.CALL:
                return $"Performs a getter call on {GetParameterText(instruction)} and pushes the values onto the stack";
            case Opcodes.NEW:
                return $"Performs a constructor call on {GetParameterText(instruction)} and pushes the new object onto the stack";
            case Opcodes.POP:
                return $"Pops top object off the stack {GetParameterText(instruction)} time" + ((int)instruction.Parameter != 1 ? "s" : "");

            default:
                Log.Error("Can't get comment emit on this instruction.");
                return string.Empty;
            }
        }

        /// <summary>
        /// This gets the parameter text for an instruction
        /// </summary>
        /// <param name="instruction"> The instruction. </param>
        /// <returns> The corresponding text. </returns>
        private string GetParameterText(Instruction instruction)
        {
            switch ((Opcodes)instruction.OpCode)
            {
            case Opcodes.PUSH_STR:
            case Opcodes.COMMENT:
                return '"' + instruction.Parameter.ToString() + '"';
            case Opcodes.SET:
            case Opcodes.NEW:
            case Opcodes.CALL:
                return instruction.Parameter.ToString().Split(' ')[1];
            default:
                return instruction.Parameter?.ToString() ?? "0";
            }
        }

        /// <summary>
        /// Dispose of the writer.
        /// </summary>
        public void Dispose()
        {
            writer.Dispose();
        }
    }
}