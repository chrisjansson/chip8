﻿using System;

namespace Cj.Chip8.Cpu
{
    public class Chip8Cpu
    {
        private readonly IDisplay _display;
        private readonly IRandomizer _randomizer;
        private readonly IKeyboard _keyboard;

        public Chip8Cpu(IDisplay display, IRandomizer randomizer, IKeyboard keyboard)
        {
            _display = display;
            _randomizer = randomizer;
            _keyboard = keyboard;

            State = new CpuState();
        }

        public CpuState State { get; set; }

        public void Cls()
        {
            _display.Clear();

            State.ProgramCounter += 2;
        }

        public void Jump(short address)
        {
            State.ProgramCounter = address;
        }

        public void Call(short address)
        {
            State.Stack[State.StackPointer++] = State.ProgramCounter;
            State.ProgramCounter = address;
        }

        public void SeConstant(byte register, byte kk)
        {
            var registerValue = State.V[register];
            var valueToCompare = kk;

            if (registerValue == valueToCompare)
                State.ProgramCounter += 4;
            else
                State.ProgramCounter += 2;
        }

        public void SneConstant(byte register, byte kk)
        {
            var registerValue = State.V[register];
            var valueToCompare = kk;

            if (registerValue != valueToCompare)
                State.ProgramCounter += 4;
            else
                State.ProgramCounter += 2;
        }

        public void Ret()
        {
            State.ProgramCounter = State.Stack[--State.StackPointer];
        }

        public void Se(byte vx, byte vy)
        {
            var left = State.V[vx];
            var right = State.V[vy];

            if (left == right)
                State.ProgramCounter += 4;
            else
                State.ProgramCounter += 2;
        }

        public void LdConstant(byte vx, byte argument)
        {
            State.V[vx] = argument;
            State.ProgramCounter += 2;
        }

        public void AddConstant(byte vx, byte argument)
        {
            State.V[vx] += argument;
            State.ProgramCounter += 2;
        }

        public void Ld(byte vx, byte vy)
        {
            State.V[vx] = State.V[vy];
            State.ProgramCounter += 2;
        }

        public void Or(byte vx, byte vy)
        {
            State.V[vx] = (byte) (State.V[vx] | State.V[vy]);
            State.ProgramCounter += 2;
        }

        public void And(byte vx, byte vy)
        {
            State.V[vx] = (byte)(State.V[vx] & State.V[vy]);
            State.ProgramCounter += 2;
        }

        public void Xor(byte vx, byte vy)
        {
            State.V[vx] = (byte)(State.V[vx] ^ State.V[vy]);
            State.ProgramCounter += 2;
        }

        public void AddCarry(byte vx, byte vy)
        {
            var result = State.V[vx] + State.V[vy];
            State.V[vx] = (byte)(result & 0xFF);
            State.V[0x0F] = (byte) (result > byte.MaxValue ? 1 : 0);

            State.ProgramCounter += 2;
        }

        public void Sub(byte vx, byte vy)
        {
            State.V[0x0F] = (byte) (State.V[vy] > State.V[vx] ? 0 : 1);
            State.V[vx] = (byte) (State.V[vx] - State.V[vy]);

            State.ProgramCounter += 2;
        }

        public void Shr(byte vx)
        {
            State.V[0x0F] = (byte) (State.V[vx] & 0x01);
            State.V[vx] = (byte) (State.V[vx] >> 1);

            State.ProgramCounter += 2;
        }

        public void Subn(int vx, int vy)
        {
            State.V[0x0F] = (byte) (State.V[vx] > State.V[vy] ? 0 : 1);
            State.V[vx] = (byte) (State.V[vy] - State.V[vx]);

            State.ProgramCounter += 2;
        }

        public void Shl(byte vx)
        {
            State.V[0x0F] = (byte) (State.V[vx] >> 7);
            State.V[vx] = (byte) (State.V[vx] << 1);

            State.ProgramCounter += 2;
        }

        public void Sne(byte vx, byte vy)
        {
            var skip = State.V[vx] != State.V[vy];
            State.ProgramCounter += (short)(skip ? 4 : 2);
        }

        public void Ldi(short address)
        {
            State.I = (short) (address & 0x0FFF);
            State.ProgramCounter += 2;  
        }

        public void JumpV0Offset(short address)
        {
            State.ProgramCounter = (short) ((address & 0x0FFF) + State.V[0]);  
        }

        public void Rnd(byte vx, byte kk)
        {
            State.V[vx] = (byte) (_randomizer.GetNext() & kk);
            State.ProgramCounter += 2;
        }

        public void Drw(byte vx, byte vy, byte height)
        {
            var sprite = new byte[height];
            Array.Copy(State.Memory, State.I, sprite, 0, height);

            var ereasedPixels = _display.Draw(State.V[vx], State.V[vy], sprite);

            State.V[0x0F] = ereasedPixels;
            State.ProgramCounter += 2;
        }

        public void Skp(byte vx)
        {
            var key = State.V[vx];
            var isKeyDown = _keyboard.IsKeyDown(key);

            if (isKeyDown)
                State.ProgramCounter += 4;
            else
                State.ProgramCounter += 2;
        }

        public void Skpn(byte vx)
        {
            var key = State.V[vx];
            var isKeyDown = _keyboard.IsKeyDown(key);

            if (isKeyDown)
                State.ProgramCounter += 2;
            else
                State.ProgramCounter += 4;
        }

        public void Dt(byte vx)
        {
            State.V[vx] = State.DelayTimer;
            State.ProgramCounter += 2;
        }

        public void K(byte vx)
        {
            var key = _keyboard.WaitForKeyPress();
            State.V[vx] = key;

            State.ProgramCounter += 2;
        }
    }
}