using System.Collections.Generic;

namespace AoC.Utils.IntCode
{
    public struct Instruction
    {
        public int OpCode { get; }

        public List<ParameterMode> Modes { get; }

        public Instruction(int opCode, List<ParameterMode> modes)
        {
            OpCode = opCode;
            Modes = modes;
        }

        public ParameterMode GetParameterMode(int idx)
        {
            return Modes.Count > idx ? Modes[idx] : ParameterMode.Position;
        }

        public static Instruction Parse(long value)
        {
            var opCode = (int)(value % 100);
            var modes = new List<ParameterMode>();
            var p = value / 100;
            while (p != 0)
            {
                modes.Add((ParameterMode)(p % 10));
                p = p / 10;
            }
            return new Instruction(opCode, modes);
        }
    }
}
