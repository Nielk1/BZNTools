using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Numerics;
using System.Text;

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
                    originalValue = null;
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
                    originalValue = null;
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

        public string? OriginalValue => originalValue;

        private T1 internalType1;
        private T2 internalType2;
        private bool fromType2;
        private string? originalValue;

        public DualModeValue(T1 value, string? originalValue = null)
        {
            this.internalType1 = value;
            this.fromType2 = false;
            this.originalValue = originalValue;
        }

        public DualModeValue(T2 value, string? originalValue = null)
        {
            this.internalType2 = value;
            this.fromType2 = true;
            this.originalValue = originalValue;
        }
    }

    public class TextAnnotatedType<T>
    {
        public T Value
        {
            get { return internalValue; }
            set
            {
                if (!EqualityComparer<T>.Default.Equals(value, internalValue))
                    originalValue = null;
                internalValue = value;
            }
        }

        public string? OriginalValue => originalValue;


        private T internalValue;
        public string? originalValue;

        public TextAnnotatedType(T value, string? originalValue)
        {
            this.internalValue = value;
            this.originalValue = originalValue;
        }
    }

    public struct Vector3D
    {
        public float x;
        public float y;
        public float z;

        internal float Magnitude()
        {
            return (float)Math.Sqrt(x * x + y * y + z * z);
        }
    }

    public struct Vector2D
    {
        public float x;
        public float z;
    }

    public struct Euler
    {
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

    public struct Matrix
    {
        public Vector3D right;
        public float rightw;
        public Vector3D up;
        public float upw;
        public Vector3D front;
        public float frontw;
        public Vector3D posit;
        public float positw;
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
}
