using BZNParser.Tokenizer;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Numerics;
using System.Reflection.PortableExecutable;
using System.Runtime.Intrinsics.X86;
using System.Text;
using static BZNParser.Tokenizer.BZNStreamReader;

namespace BZNParser
{
    // note, there are some oddities with passing these around, might need to swap to classes unless this lets me use sizeof

    public class DualModeValue<T1, T2>
    {
        public T1 ValueType1
        {
            get
            {
                if (fromType2)
                    return internalType2 is not null
                        ? (T1)Convert.ChangeType(internalType2, typeof(T1))
                        : (T1)Activator.CreateInstance(typeof(T1))!;
                else
                    return internalType1;
            }
            set
            {
                if (fromType2 || !EqualityComparer<T1>.Default.Equals(value, internalType1))
                {
                    // reset malformations
                }
                internalType1 = value;
                fromType2 = false;
            }
        }

        public T2 ValueType2
        {
            get
            {
                if (fromType2)
                    return internalType2;
                else
                    return internalType1 is not null
                        ? (T2)Convert.ChangeType(internalType1, typeof(T2))
                        : (T2)Activator.CreateInstance(typeof(T2))!;
            }
            set
            {
                if (!fromType2 || !EqualityComparer<T2>.Default.Equals(value, internalType2))
                {
                    // reset malformations
                }
                internalType2 = value;
                fromType2 = true;
            }
        }

        public dynamic Value
        {
            get
            {
                if (fromType2)
                    return internalType2!;
                else
                    return internalType1!;
            }
            set
            {
                if (value is T1)
                {
                    internalType1 = (T1)value;
                    fromType2 = false;
                }
                else if (value is T2)
                {
                    internalType2 = (T2)value;
                    fromType2 = true;
                }
                else
                {
                    throw new ArgumentException($"Value must be of type {typeof(T1)} or {typeof(T2)}");
                }
            }
        }

        public T Get<T>()
        {
            if (typeof(T) == typeof(T1))
            {
                return (T)(object)ValueType1!;
            }
            if (typeof(T) == typeof(T2))
            {
                return (T)(object)ValueType2!;
            }
            else
            {
                throw new ArgumentException($"Type parameter must be {typeof(T1)} or {typeof(T2)}");
            }
        }


        private T1 internalType1;
        private T2 internalType2;
        private bool fromType2;

        public DualModeValue(T1 value)
        {
            this.internalType1 = value;
            this.fromType2 = false;
        }

        public DualModeValue(T2 value)
        {
            this.internalType2 = value;
            this.fromType2 = true;
        }
    }



    public class Vector3D : IMalformable
    {
        private readonly IMalformable.MalformationManager _malformationManager;
        public IMalformable.MalformationManager Malformations => _malformationManager;
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        public Vector3D()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        {
            this._malformationManager = new IMalformable.MalformationManager(this);
        }


        public float X
        {
            get { return x; }
            set
            {
                if (value != x)
                {
                    // reset malformations
                }
                x = value;
            }
        }
        public float Y
        {
            get { return y; }
            set
            {
                if (value != y)
                {
                    // reset malformations
                }
                y = value;
            }
        }
        public float Z
        {
            get { return z; }
            set
            {
                if (value != z)
                {
                    // reset malformations
                }
                z = value;
            }
        }

        private float x;
        private float y;
        private float z;

        internal float Magnitude()
        {
            return (float)Math.Sqrt(x * x + y * y + z * z);
        }
    }

    public class Vector2D : IMalformable
    {
        private readonly IMalformable.MalformationManager _malformationManager;
        public IMalformable.MalformationManager Malformations => _malformationManager;
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        public Vector2D()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        {
            this._malformationManager = new IMalformable.MalformationManager(this);
        }



        public float X
        {
            get { return x; }
            set
            {
                if (value != x)
                {
                    // reset malformations
                }
                x = value;
            }
        }
        public float Z
        {
            get { return z; }
            set
            {
                if (value != z)
                {
                    // reset malformations
                }
                z = value;
            }
        }

        private float x;
        private float z;
    }

    public class Euler : IMalformable
    {
        private readonly IMalformable.MalformationManager _malformationManager;
        public IMalformable.MalformationManager Malformations => _malformationManager;
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        public Euler()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        {
            this._malformationManager = new IMalformable.MalformationManager(this);
        }

        public const float EPSILON = 1.0e-4f;
        public const float HUGE_NUMBER = 1.0e30f;

        public Quaternion Att;

        public Vector3D v;
        public Vector3D omega;
        public Vector3D Accel;

        public Vector3D Alpha;
        public Vector3D Pos;

        public float mass;
        public float mass_inv;

        public float I;
        public float I_inv;

        public float v_mag;
        public float v_mag_inv;

        public void InitLoadSave()
        {
            v = new Vector3D();
            omega = new Vector3D();
            Accel = new Vector3D();
            Alpha = new Vector3D();
        }

        // Reads the 'mass' member, builds mass_inv and I_inv fields
        public void CalcMassIInv()
        {
            mass_inv = HUGE_NUMBER;
            I_inv = HUGE_NUMBER;
            if (mass > EPSILON)
            {
                mass_inv = 1.0f / mass;
                I_inv = 1.0f / I;
            }
            else
            {
                mass_inv = HUGE_NUMBER;
                I_inv = HUGE_NUMBER;
            }
        }


        // Reads the 'v' member, builds the 'v_mag' and 'v_mag_inv' members
        public void CalcVMag()
        {
            v_mag = v.Magnitude();
            v_mag_inv = (v_mag == 0.0f) ? HUGE_NUMBER : 1.0f / v_mag;
        }
    }

    /*public struct MatrixOld
    {
        public Vector3D right;
        public Vector3D up;
        public Vector3D front;
        public Vector3D posit;
    }*/

    public class Matrix : IMalformable
    {
        private readonly IMalformable.MalformationManager _malformationManager;
        public IMalformable.MalformationManager Malformations => _malformationManager;
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        public Matrix()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        {
            this._malformationManager = new IMalformable.MalformationManager(this);
        }


        public float rightx;
        public float righty;
        public float rightz;
        public float rightw;
        public float upx;
        public float upy;
        public float upz;
        public float upw;
        public float frontx;
        public float fronty;
        public float frontz;
        public float frontw;
        public UInt32 junk; // used only in double-posit mode
        public double positx;
        public double posity;
        public double positz;
        public double positw;
    }

    public struct Quaternion
    {
    }
    public enum BinaryFieldType : byte
    {
        DATA_VOID = 0, //0x00
        DATA_BOOL = 1, //0x01
        DATA_CHAR = 2, //0x02
        DATA_SHORT = 3, //0x03
        DATA_LONG = 4, //0x04
        DATA_FLOAT = 5, //0x05
        DATA_DOUBLE = 6, //0x06
        DATA_ID = 7, //0x07
        DATA_PTR = 8, //0x08
        DATA_VEC3D = 9, //0x09
        DATA_VEC2D = 10,//0x0A
        DATA_MAT3DOLD = 11,//0x0B
        DATA_MAT3D = 12,//0x0C
        DATA_STRING = 13,//0x0D
        DATA_QUAT = 14, //0x0E

        DATA_UNKNOWN = 255
    }

    public enum BZNFormat
    {
        Battlezone,
        Battlezone2,
        BattlezoneN64,
        StarTrekArmada,
        StarTrekArmada2,
    }


    public static class MalformationExtensions
    {
        public static void CheckMalformationsMatrix(this IBZNToken tok, IMalformable.MalformationManager malformations, FloatTextFormat floatFormat, int index = 0)
        {
            if (tok.IsBinary)
            {
                // check for malformations in the binary data and add to the token's malformation manager if found
            }
            else
            {
                IBZNToken subTok;
                subTok = tok.GetSubToken(index,  0); if (subTok.GetRawName() != @"  right_x") { malformations.AddIncorrectName(@"  right_x", subTok.GetRawName()); } subTok.CheckMalformationsSingle(@"  right_x", malformations, floatFormat);
                subTok = tok.GetSubToken(index,  1); if (subTok.GetRawName() != @"  right_y") { malformations.AddIncorrectName(@"  right_y", subTok.GetRawName()); } subTok.CheckMalformationsSingle(@"  right_y", malformations, floatFormat);
                subTok = tok.GetSubToken(index,  2); if (subTok.GetRawName() != @"  right_z") { malformations.AddIncorrectName(@"  right_z", subTok.GetRawName()); } subTok.CheckMalformationsSingle(@"  right_z", malformations, floatFormat);
                subTok = tok.GetSubToken(index,  3); if (subTok.GetRawName() != @"  up_x"   ) { malformations.AddIncorrectName(@"  up_x"   , subTok.GetRawName()); } subTok.CheckMalformationsSingle(@"  up_x"   , malformations, floatFormat);
                subTok = tok.GetSubToken(index,  4); if (subTok.GetRawName() != @"  up_y"   ) { malformations.AddIncorrectName(@"  up_y"   , subTok.GetRawName()); } subTok.CheckMalformationsSingle(@"  up_y"   , malformations, floatFormat);
                subTok = tok.GetSubToken(index,  5); if (subTok.GetRawName() != @"  up_z"   ) { malformations.AddIncorrectName(@"  up_z"   , subTok.GetRawName()); } subTok.CheckMalformationsSingle(@"  up_z"   , malformations, floatFormat);
                subTok = tok.GetSubToken(index,  6); if (subTok.GetRawName() != @"  front_x") { malformations.AddIncorrectName(@"  front_x", subTok.GetRawName()); } subTok.CheckMalformationsSingle(@"  front_x", malformations, floatFormat);
                subTok = tok.GetSubToken(index,  7); if (subTok.GetRawName() != @"  front_y") { malformations.AddIncorrectName(@"  front_y", subTok.GetRawName()); } subTok.CheckMalformationsSingle(@"  front_y", malformations, floatFormat);
                subTok = tok.GetSubToken(index,  8); if (subTok.GetRawName() != @"  front_z") { malformations.AddIncorrectName(@"  front_z", subTok.GetRawName()); } subTok.CheckMalformationsSingle(@"  front_z", malformations, floatFormat);
                subTok = tok.GetSubToken(index,  9); if (subTok.GetRawName() != @"  posit_x") { malformations.AddIncorrectName(@"  posit_x", subTok.GetRawName()); } subTok.CheckMalformationsSingle(@"  posit_x", malformations, floatFormat);
                subTok = tok.GetSubToken(index, 10); if (subTok.GetRawName() != @"  posit_y") { malformations.AddIncorrectName(@"  posit_y", subTok.GetRawName()); } subTok.CheckMalformationsSingle(@"  posit_y", malformations, floatFormat);
                subTok = tok.GetSubToken(index, 11); if (subTok.GetRawName() != @"  posit_z") { malformations.AddIncorrectName(@"  posit_z", subTok.GetRawName()); } subTok.CheckMalformationsSingle(@"  posit_z", malformations, floatFormat);
            }
        }
        public static void CheckMalformationsEuler(this IBZNToken tok, Euler euler, FloatTextFormat floatFormat, int index = 0)
        {
            if (tok.IsBinary)
            {
                // check for malformations in the binary data and add to the token's malformation manager if found
            }
            else
            {
                IBZNToken subTok;
                subTok = tok.GetSubToken(index, 0); if (subTok.GetRawName() != @" mass"     ) { euler.Malformations.AddIncorrectName(@" mass"     , subTok.GetRawName()); } subTok.CheckMalformationsSingle(@" mass"     , euler.Malformations, floatFormat);
                subTok = tok.GetSubToken(index, 1); if (subTok.GetRawName() != @" mass_inv" ) { euler.Malformations.AddIncorrectName(@" mass_inv" , subTok.GetRawName()); } subTok.CheckMalformationsSingle(@" mass_inv" , euler.Malformations, floatFormat);
                subTok = tok.GetSubToken(index, 2); if (subTok.GetRawName() != @" v_mag"    ) { euler.Malformations.AddIncorrectName(@" v_mag"    , subTok.GetRawName()); } subTok.CheckMalformationsSingle(@" v_mag"    , euler.Malformations, floatFormat);
                subTok = tok.GetSubToken(index, 3); if (subTok.GetRawName() != @" v_mag_inv") { euler.Malformations.AddIncorrectName(@" v_mag_inv", subTok.GetRawName()); } subTok.CheckMalformationsSingle(@" v_mag_inv", euler.Malformations, floatFormat);
                subTok = tok.GetSubToken(index, 4); if (subTok.GetRawName() != @" I"        ) { euler.Malformations.AddIncorrectName(@" I"        , subTok.GetRawName()); } subTok.CheckMalformationsSingle(@" I"        , euler.Malformations, floatFormat);
                subTok = tok.GetSubToken(index, 5); if (subTok.GetRawName() != @" k_i"      ) { euler.Malformations.AddIncorrectName(@" k_i"      , subTok.GetRawName()); } subTok.CheckMalformationsSingle(@" k_i"      , euler.Malformations, floatFormat);
                subTok = tok.GetSubToken(index, 6); if (subTok.GetRawName() != @" v"        ) { euler.Malformations.AddIncorrectName(@" v"        , subTok.GetRawName()); } subTok.CheckMalformationsVector3D(euler.v.Malformations    , floatFormat);
                subTok = tok.GetSubToken(index, 7); if (subTok.GetRawName() != @" omega"    ) { euler.Malformations.AddIncorrectName(@" omega"    , subTok.GetRawName()); } subTok.CheckMalformationsVector3D(euler.omega.Malformations, floatFormat);
                subTok = tok.GetSubToken(index, 8); if (subTok.GetRawName() != @" Accel"    ) { euler.Malformations.AddIncorrectName(@" Accel"    , subTok.GetRawName()); } subTok.CheckMalformationsVector3D(euler.Accel.Malformations, floatFormat);
            }
        }

        public static void CheckMalformationsVector2D(this IBZNToken tok, IMalformable.MalformationManager malformations, FloatTextFormat floatFormat, int index = 0)
        {
            if (tok.IsBinary)
            {
                // check for malformations in the binary data and add to the token's malformation manager if found
            }
            else
            {
                IBZNToken subTok;
                subTok = tok.GetSubToken(index, 0); if (subTok.GetRawName() != @"  x") { malformations.AddIncorrectName(@"  x", subTok.GetRawName()); } subTok.CheckMalformationsSingle("  x", malformations, floatFormat);
                subTok = tok.GetSubToken(index, 1); if (subTok.GetRawName() != @"  z") { malformations.AddIncorrectName(@"  z", subTok.GetRawName()); } subTok.CheckMalformationsSingle("  z", malformations, floatFormat);
            }
        }

        public static void CheckMalformationsVector3D(this IBZNToken tok, IMalformable.MalformationManager malformations, FloatTextFormat floatFormat, int index = 0)
        {
            if (tok.IsBinary)
            {
                // check for malformations in the binary data and add to the token's malformation manager if found
            }
            else
            {
                IBZNToken subTok;
                subTok = tok.GetSubToken(index, 0); if (subTok.GetRawName() != @"  x") { malformations.AddIncorrectName(@"  x", subTok.GetRawName()); } subTok.CheckMalformationsSingle("  x", malformations, floatFormat);
                subTok = tok.GetSubToken(index, 1); if (subTok.GetRawName() != @"  y") { malformations.AddIncorrectName(@"  y", subTok.GetRawName()); } subTok.CheckMalformationsSingle("  y", malformations, floatFormat);
                subTok = tok.GetSubToken(index, 2); if (subTok.GetRawName() != @"  z") { malformations.AddIncorrectName(@"  z", subTok.GetRawName()); } subTok.CheckMalformationsSingle("  z", malformations, floatFormat);
            }
        }

        public static void CheckMalformationsSingle(this IBZNToken tok, string name, IMalformable.MalformationManager malformations, FloatTextFormat floatFormat, int index = 0)
        {
            if (tok.IsBinary)
            {
                // check for malformations in the binary data and add to the token's malformation manager if found
            }
            else
            {
                if (SingleExtension.GetFloatTextFormat(tok.GetString(index)) != floatFormat || tok.GetSingle(index).ToBZNString(floatFormat) != tok.GetString(index))
                    malformations.AddIncorrectTextParse(name, tok.GetString(index));
            }
        }
        public static void CheckMalformationsBool(this IBZNToken tok, string name, IMalformable.MalformationManager malformations, int index = 0)
        {
            if (tok.IsBinary)
            {
                // check for malformations in the binary data and add to the token's malformation manager if found
            }
            else
            {
                if (tok.GetBoolean(index).ToString().ToLowerInvariant() != tok.GetString(index))
                    malformations.AddIncorrectTextParse(name, tok.GetString(index));
            }
        }

        public static string CorrectName(this IMalformable.MalformationManager malformations, bool preserveMalformations, string name)
        {
            if (preserveMalformations)
            {
                var malIncorrectName = malformations.GetMalformations(Malformation.INCORRECT_NAME, name);
                if (malIncorrectName.Any())
                {
                    var mal = malIncorrectName.First();
                    name = (string)mal.Fields[0];
                }
            }
            return name;
        }
    }
}
