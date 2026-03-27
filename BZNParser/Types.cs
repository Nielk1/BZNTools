using BZNParser.Battlezone;
using BZNParser.Tokenizer;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Numerics;
using System.Reflection;
using System.Reflection.PortableExecutable;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.X86;
using System.Security.Cryptography;
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

    public class SizedString : IMalformable
    {
        private readonly IMalformable.MalformationManager _malformationManager;
        public IMalformable.MalformationManager Malformations => _malformationManager;
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        public SizedString()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        {
            this._malformationManager = new IMalformable.MalformationManager(this);
        }

        public UInt32? Size { get; set; }
        public string Value { get; set; }

        public override string ToString()
        {
            return Value;
        }
    }
    static class OtherTypeExtenions // clean this up later
    {
        /*public static Vector3D ReadVector3D<T>(this BZNStreamReader reader, string name, T? parent, Expression<Func<T, Vector3D>>? property, int index = 0) where T : IMalformable
        {
            PropertyInfo? propInfo = null;
            if (property != null && property.Body is MemberExpression member && member.Member is PropertyInfo propInfo_)
                propInfo = propInfo_;

            IBZNToken? tok;
            tok = reader.ReadToken();
            if (tok == null || !tok.Validate(name, BinaryFieldType.DATA_VEC3D))
                throw new Exception($"Failed to parse {name}/VEC3D");

            Vector3D value = tok.GetVector3D(index);

            // store the value into the property if possible
            if (parent != null && propInfo != null)
                propInfo.SetValue(parent, value);

            // we can't process anything, so just serve the matrix as is
            if (parent == null || propInfo == null)
                return value;

            // binary doesn't have subtokens, it's just a blob of data
            if (tok.IsBinary)
                return value;

            IBZNToken subTok;
            subTok = tok.GetSubToken(index, 0); subTok.ReadSingle(value, x => x.X); if (subTok.GetRawName() != @"  x") { value.Malformations.AddIncorrectName<Matrix, float>(x => x.X, subTok.GetRawName()); }
            subTok = tok.GetSubToken(index, 1); subTok.ReadSingle(value, x => x.Y); if (subTok.GetRawName() != @"  y") { value.Malformations.AddIncorrectName<Matrix, float>(x => x.Y, subTok.GetRawName()); }
            subTok = tok.GetSubToken(index, 2); subTok.ReadSingle(value, x => x.Z); if (subTok.GetRawName() != @"  z") { value.Malformations.AddIncorrectName<Matrix, float>(x => x.Z, subTok.GetRawName()); }

            return value;
        }
        public static Vector2D ReadVector2D<T>(this BZNStreamReader reader, string name, T? parent, Expression<Func<T, Vector2D>>? property, int index = 0) where T : IMalformable
        {
            PropertyInfo? propInfo = null;
            if (property != null && property.Body is MemberExpression member && member.Member is PropertyInfo propInfo_)
                propInfo = propInfo_;

            IBZNToken? tok;
            tok = reader.ReadToken();
            if (tok == null || !tok.Validate(name, BinaryFieldType.DATA_VEC2D))
                throw new Exception($"Failed to parse {name}/VEC2D");

            Vector2D value = tok.GetVector2D(index);

            // store the value into the property if possible
            if (parent != null && propInfo != null)
                propInfo.SetValue(parent, value);

            // we can't process anything, so just serve the matrix as is
            if (parent == null || propInfo == null)
                return value;

            // binary doesn't have subtokens, it's just a blob of data
            if (tok.IsBinary)
                return value;

            IBZNToken subTok;
            subTok = tok.GetSubToken(index, 0); subTok.ReadSingle(value, x => x.X); if (subTok.GetRawName() != @"  x") { value.Malformations.AddIncorrectName<Matrix, float>(x => x.X, subTok.GetRawName()); }
            subTok = tok.GetSubToken(index, 1); subTok.ReadSingle(value, x => x.Z); if (subTok.GetRawName() != @"  z") { value.Malformations.AddIncorrectName<Matrix, float>(x => x.Z, subTok.GetRawName()); }

            return value;
        }*/
        public static Euler ReadEuler<T>(this BZNStreamReader reader, string name, T? parent, Expression<Func<T, Euler>>? property, int index = 0) where T : IMalformable
        {
            PropertyInfo? propInfo = null;
            if (property != null && property.Body is MemberExpression member && member.Member is PropertyInfo propInfo_)
                propInfo = propInfo_;

            IBZNToken? tok;

            if (reader.InBinary)
            {
                Euler euler = new Euler();

                tok = reader.ReadToken();
                if (tok == null || !tok.Validate(null, BinaryFieldType.DATA_FLOAT)) throw new Exception("Failed to parse euler's FLOAT");
                tok.ApplySingle(euler, x => x.mass);

                tok = reader.ReadToken();
                if (tok == null || !tok.Validate(null, BinaryFieldType.DATA_FLOAT)) throw new Exception("Failed to parse euler's FLOAT");
                tok.ApplySingle(euler, x => x.mass_inv);

                tok = reader.ReadToken();
                if (tok == null || !tok.Validate(null, BinaryFieldType.DATA_FLOAT)) throw new Exception("Failed to parse euler's FLOAT");
                tok.ApplySingle(euler, x => x.v_mag);

                tok = reader.ReadToken();
                if (tok == null || !tok.Validate(null, BinaryFieldType.DATA_FLOAT)) throw new Exception("Failed to parse euler's FLOAT");
                tok.ApplySingle(euler, x => x.v_mag_inv);

                tok = reader.ReadToken();
                if (tok == null || !tok.Validate(null, BinaryFieldType.DATA_FLOAT)) throw new Exception("Failed to parse euler's FLOAT");
                tok.ApplySingle(euler, x => x.I);

                tok = reader.ReadToken();
                if (tok == null || !tok.Validate(null, BinaryFieldType.DATA_FLOAT)) throw new Exception("Failed to parse euler's FLOAT");
                tok.ApplySingle(euler, x => x.I_inv);

                tok = reader.ReadToken();
                if (tok == null || !tok.Validate(null, BinaryFieldType.DATA_VEC3D)) throw new Exception("Failed to parse euler's VEC3D");
                tok.ApplyVector3D(euler, x => x.v);

                tok = reader.ReadToken();
                if (tok == null || !tok.Validate(null, BinaryFieldType.DATA_VEC3D)) throw new Exception("Failed to parse euler's VEC3D");
                tok.ApplyVector3D(euler, x => x.omega);

                tok = reader.ReadToken();
                if (tok == null || !tok.Validate(null, BinaryFieldType.DATA_VEC3D)) throw new Exception("Failed to parse euler's VEC3D");
                tok.ApplyVector3D(euler, x => x.Accel);

                // store the value into the property if possible
                if (parent != null && propInfo != null)
                    propInfo.SetValue(parent, euler);

                // we can't process anything, so just serve the euler as is
                if (parent == null || propInfo == null)
                    return euler;

                return euler;
            }

            tok = reader.ReadToken();
            if (tok == null || !tok.Validate(name, BinaryFieldType.DATA_UNKNOWN))
                throw new Exception($"Failed to parse {name}");

            Euler value = tok.GetEuler(index);

            // store the value into the property if possible
            if (parent != null && propInfo != null)
                propInfo.SetValue(parent, value);

            // we can't process anything, so just serve the euler as is
            if (parent == null || propInfo == null)
                return value;
            
            IBZNToken subTok;
            subTok = tok.GetSubToken(index, 0); subTok.ApplySingle(value, x => x.mass     ); if (subTok.GetRawName() != @" mass"     ) { value.Malformations.AddIncorrectName<Euler, float>(x => x.mass     , subTok.GetRawName()); }
            subTok = tok.GetSubToken(index, 1); subTok.ApplySingle(value, x => x.mass_inv ); if (subTok.GetRawName() != @" mass_inv" ) { value.Malformations.AddIncorrectName<Euler, float>(x => x.mass_inv , subTok.GetRawName()); }
            subTok = tok.GetSubToken(index, 2); subTok.ApplySingle(value, x => x.v_mag    ); if (subTok.GetRawName() != @" v_mag"    ) { value.Malformations.AddIncorrectName<Euler, float>(x => x.v_mag    , subTok.GetRawName()); }
            subTok = tok.GetSubToken(index, 3); subTok.ApplySingle(value, x => x.v_mag_inv); if (subTok.GetRawName() != @" v_mag_inv") { value.Malformations.AddIncorrectName<Euler, float>(x => x.v_mag_inv, subTok.GetRawName()); }
            subTok = tok.GetSubToken(index, 4); subTok.ApplySingle(value, x => x.I        ); if (subTok.GetRawName() != @" I"        ) { value.Malformations.AddIncorrectName<Euler, float>(x => x.I        , subTok.GetRawName()); }
            subTok = tok.GetSubToken(index, 5); subTok.ApplySingle(value, x => x.I_inv    ); if (subTok.GetRawName() != @" k_i"      ) { value.Malformations.AddIncorrectName<Euler, float>(x => x.I_inv    , subTok.GetRawName()); }
            subTok = tok.GetSubToken(index, 6); subTok.ApplyVector3D(value, x => x.v      ); if (subTok.GetRawName() != @" v"        ) { value.Malformations.AddIncorrectName<Euler, Vector3D>(x => x.v     , subTok.GetRawName()); }
            subTok = tok.GetSubToken(index, 7); subTok.ApplyVector3D(value, x => x.omega  ); if (subTok.GetRawName() != @" omega"    ) { value.Malformations.AddIncorrectName<Euler, Vector3D>(x => x.omega , subTok.GetRawName()); }
            subTok = tok.GetSubToken(index, 8); subTok.ApplyVector3D(value, x => x.Accel  ); if (subTok.GetRawName() != @" Accel"    ) { value.Malformations.AddIncorrectName<Euler, Vector3D>(x => x.Accel , subTok.GetRawName()); }

            return value;
        }
    }
    static class SizedStringExtension
    {
        /// <summary>
        /// Read a normal chars string unless BZ2 and version > 1128
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="reader"></param>
        /// <param name="name"></param>
        /// <param name="parent"></param>
        /// <param name="property"></param>
        /// <param name="destinationIndex">We already read index 0 of the Token, but this is what index we're writing to</param>
        /// <exception cref="Exception"></exception>
        public static (string stored, string raw) ReadSizedString<T, TProp>(this BZNStreamReader reader, string name, T? parent, Expression<Func<T, TProp?>>? property, int destinationIndex = 0) where T : IMalformable
        {
            PropertyInfo? propInfo = null;
            if (property != null && property.Body is MemberExpression member && member.Member is PropertyInfo propInfo_)
                propInfo = propInfo_;

            //SizedString? value = null;
            //if (parent != null)
            //    value = (SizedString?)(propInfo?.GetValue(parent));
            //if (value == null && propInfo != null)
            //    value = new SizedString();
            //if (propInfo != null)
            //{
            //    value = new SizedString();
            //    if (parent != null)
            //        propInfo.SetValue(parent, value);
            //}

            SizedString? value = new SizedString();
            TProp setVal = default!;
            bool did = false;
            if (typeof(TProp) == typeof(SizedString) || Nullable.GetUnderlyingType(typeof(TProp)) == typeof(SizedString))
            {
                setVal = (TProp)(object)value;
                did = true;
            }
            else if (typeof(TProp).IsArray && typeof(TProp).GetElementType() == typeof(SizedString))
            {
                SizedString[]? arr = (SizedString[]?)propInfo?.GetValue(parent);
                if (arr != null && destinationIndex >= 0 && destinationIndex < arr.Length)
                {
                    arr[destinationIndex] = value;
                    setVal = (TProp)(object)arr;
                    did = true;
                }
            }

            IBZNToken? tok;
            if (reader.InBinary)
            {
                // this only happens on BZ2/BZCC BZNs over version 1128s
                if (reader.Format == BZNFormat.Battlezone2 && reader.Version > 1128)
                {
                    // TODO this might be a compressed number so do figure that out
                    tok = reader.ReadToken();
                    if (tok == null || !tok.Validate(null, BinaryFieldType.DATA_CHAR) || tok.GetCount() > 1)
                        throw new Exception($"Failed to parse {name}/CHAR");
                    (_, byte size) = tok.ApplyUInt8(value, x => x.Size);

                    if (size > 0) // descision based on raw value, not cleaned
                    {
                        tok = reader.ReadToken();
                        if (tok == null || !tok.Validate(name, BinaryFieldType.DATA_CHAR))
                            throw new Exception($"Failed to parse {name}/CHAR");
                        (string stored_, string raw_) = tok.ApplyChars(value, x => x.Value);

                        // this works when the property is a SizedString, but not a SizedString[] where we want to apply it instead to SizedString[destinationIndex]
                        if (propInfo != null && parent != null && did)
                            propInfo.SetValue(parent, setVal);

                        return (stored_, raw_);
                    }

                    // this works when the property is a SizedString, but not a SizedString[] where we want to apply it instead to SizedString[destinationIndex]
                    if (propInfo != null && parent != null && did)
                        propInfo.SetValue(parent, setVal);

                    return (null, null);
                }
            }
            tok = reader.ReadToken();
            if (tok == null || !tok.Validate(name, BinaryFieldType.DATA_CHAR))
                throw new Exception($"Failed to parse {name}/CHAR");
            (string stored, string raw) = tok.ApplyChars(value, x => x.Value);

            if (propInfo != null && parent != null && did)
                propInfo.SetValue(parent, setVal);

            return (stored, raw);
        }

        public static (string stored, string raw) ReadGameObjectClass_BZ2<T, TProp>(this BZNStreamReader reader, SaveType saveType, string name, T? parent, Expression<Func<T, TProp>>? property, int index = 0) where T : IMalformable
        {
            if (reader.Version < 1145)
            {
                //return reader.ReadSizedString_BZ2_1145(name, 16, malformations);
                return reader.ReadSizedString(name, parent, property, index);
            }
            else
            {
                if (saveType == SaveType.LOCKSTEP)
                {
                    throw new NotImplementedException();
                }
                else
                {
                    return reader.ReadSizedString(name, parent, property, index);
                }
            }
        }

        public static void WriteSizedString<T, TProp>(this BZNStreamWriter writer, string name, T parent, Expression<Func<T, TProp>> property, Func<TProp, SizedString>? convert = null)
        {
            TProp wrappedValue = BZNStreamWriter.ExtractPropertyValue(parent, property);
            SizedString value;

            if (convert != null)
            {
                value = convert(wrappedValue);
            }
            //else if (typeof(TProp).IsArray && typeof(TProp).GetElementType() == typeof(SizedString))
            //{
            //    values = (SizedString[])(object)propValue;
            //}
            else if (typeof(TProp) == typeof(SizedString))
            {
                value = (SizedString)(object)wrappedValue;
            }
            else
            {
                throw new NotImplementedException("Property type not handled");
            }

            if (writer.InBinary)
            {
                if (writer.Format == BZNFormat.Battlezone2 && writer.Version > 1128)
                {
                    (byte size, _) = writer.WriteUInt8(null, value, x => x.Size);
                    if (size > 0)
                        writer.WriteChars(name, value, x => x.Value);
                    return;
                }
            }
            writer.WriteChars(name, value, x => x.Value);
        }

        /// <summary>
        /// Sized path name string
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="reader"></param>
        /// <param name="name"></param>
        /// <param name="parent"></param>
        /// <param name="property"></param>
        /// <param name="index"></param>
        /// <exception cref="Exception"></exception>
        public static (string stored, string raw) ReadSizedStringType2<T>(this BZNStreamReader reader, string name, T? parent, Expression<Func<T, SizedString>>? property, int index = 0) where T : IMalformable
        {
            PropertyInfo? propInfo = null;
            if (property != null && property.Body is MemberExpression member && member.Member is PropertyInfo propInfo_)
                propInfo = propInfo_;

            SizedString? value = null;
            if (parent != null)
                value = (SizedString?)(propInfo?.GetValue(parent));
            if (value == null && propInfo != null)
                value = new SizedString();
            if (propInfo != null)
            {
                value = new SizedString();
                if (parent != null)
                    propInfo.SetValue(parent, value);
            }

            IBZNToken? tok;
            // TODO this might be a compressed number so do figure that out
            tok = reader.ReadToken();
            if (tok == null || !tok.Validate("size", BinaryFieldType.DATA_LONG) || tok.GetCount() > 1)
                throw new Exception($"Failed to parse size/LONG");
            (_, UInt32 size) = tok.ApplyUInt32(value, x => x.Size);

            if (size > 0) // descision based on raw value, not cleanedsssss
            {
                tok = reader.ReadToken();
                if (tok == null || !tok.Validate(name, BinaryFieldType.DATA_CHAR))
                    throw new Exception($"Failed to parse {name}/CHAR");
                return tok.ApplyChars(value, x => x.Value);
            }
            return (null, null);
        }
        public static void WriteSizedStringType2<T>(this BZNStreamWriter writer, string name, T parent, Expression<Func<T, SizedString>> property)
        {
            SizedString wrappedValue = BZNStreamWriter.ExtractPropertyValue(parent, property);

            (UInt32 size, _) = writer.WriteUInt32("size", wrappedValue, x => x.Size);
            if (size > 0)
                writer.WriteChars(name, wrappedValue, x => x.Value);
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

        public Quaternion Att { get; set; }

        public Vector3D v { get; set; }
        public Vector3D omega { get; set; }
        public Vector3D Accel { get; set; }

        public Vector3D Alpha { get; set; }
        public Vector3D Pos { get; set; }

        public float mass { get; set; }
        public float mass_inv { get; set; }

        public float I { get; set; }
        public float I_inv { get; set; }

        public float v_mag { get; set; }
        public float v_mag_inv { get; set; }

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


        public float rightx { get; set; }
        public float righty { get; set; }
        public float rightz { get; set; }
        public float rightw { get; set; }
        public float upx { get; set; }
        public float upy { get; set; }
        public float upz { get; set; }
        public float upw { get; set; }
        public float frontx { get; set; }
        public float fronty { get; set; }
        public float frontz { get; set; }
        public float frontw { get; set; }
        public UInt32 junk { get; set; } // used only in double-posit mode
        public double positx { get; set; }
        public double posity { get; set; }
        public double positz { get; set; }
        public double positw { get; set; }
    }

    static class MatrixExtension
    {
        public static Matrix ReadMatrix<T>(this BZNStreamReader reader, string name, T? parent, Expression<Func<T, Matrix>>? property, int index = 0) where T : IMalformable
        {
            PropertyInfo? propInfo = null;
            if (property != null && property.Body is MemberExpression member && member.Member is PropertyInfo propInfo_)
                propInfo = propInfo_;


            IBZNToken? tok;
            tok = reader.ReadToken();
            if (tok == null || !tok.Validate(name, BinaryFieldType.DATA_MAT3D))
                throw new Exception($"Failed to parse {name}/MAT3D");

            Matrix value = tok.GetMatrix(index);

            // store the value into the property if possible
            if (parent != null && propInfo != null)
                propInfo.SetValue(parent, value);

            // we can't process anything, so just serve the matrix as is
            if (parent == null || propInfo == null)
                return value;

            // binary doesn't have subtokens, it's just a blob of data
            if (tok.IsBinary)
                return value;

            // we have stuff to play with, so lets re-read the values back into the matrix again, but this time pass through our validation function
            IBZNToken subTok;
            subTok = tok.GetSubToken(index,  0); subTok.ApplySingle(value, x => x.rightx); if (subTok.GetRawName() != @"  right.x") { value.Malformations.AddIncorrectName<Matrix, float>(x => x.rightx, subTok.GetRawName()); }
            subTok = tok.GetSubToken(index,  1); subTok.ApplySingle(value, x => x.righty); if (subTok.GetRawName() != @"  right.y") { value.Malformations.AddIncorrectName<Matrix, float>(x => x.righty, subTok.GetRawName()); }
            subTok = tok.GetSubToken(index,  2); subTok.ApplySingle(value, x => x.rightz); if (subTok.GetRawName() != @"  right.z") { value.Malformations.AddIncorrectName<Matrix, float>(x => x.rightz, subTok.GetRawName()); }
            subTok = tok.GetSubToken(index,  3); subTok.ApplySingle(value, x => x.upx   ); if (subTok.GetRawName() != @"  up.x"   ) { value.Malformations.AddIncorrectName<Matrix, float>(x => x.upx   , subTok.GetRawName()); }
            subTok = tok.GetSubToken(index,  4); subTok.ApplySingle(value, x => x.upy   ); if (subTok.GetRawName() != @"  up.y"   ) { value.Malformations.AddIncorrectName<Matrix, float>(x => x.upy   , subTok.GetRawName()); }
            subTok = tok.GetSubToken(index,  5); subTok.ApplySingle(value, x => x.upz   ); if (subTok.GetRawName() != @"  up.z"   ) { value.Malformations.AddIncorrectName<Matrix, float>(x => x.upz   , subTok.GetRawName()); }
            subTok = tok.GetSubToken(index,  6); subTok.ApplySingle(value, x => x.frontx); if (subTok.GetRawName() != @"  front.x") { value.Malformations.AddIncorrectName<Matrix, float>(x => x.frontx, subTok.GetRawName()); }
            subTok = tok.GetSubToken(index,  7); subTok.ApplySingle(value, x => x.fronty); if (subTok.GetRawName() != @"  front.y") { value.Malformations.AddIncorrectName<Matrix, float>(x => x.fronty, subTok.GetRawName()); }
            subTok = tok.GetSubToken(index,  8); subTok.ApplySingle(value, x => x.frontz); if (subTok.GetRawName() != @"  front.z") { value.Malformations.AddIncorrectName<Matrix, float>(x => x.frontz, subTok.GetRawName()); }
            subTok = tok.GetSubToken(index,  9); subTok.ApplySingle(value, x => x.positx); if (subTok.GetRawName() != @"  posit.x") { value.Malformations.AddIncorrectName<Matrix, double>(x => x.positx, subTok.GetRawName()); }
            subTok = tok.GetSubToken(index, 10); subTok.ApplySingle(value, x => x.posity); if (subTok.GetRawName() != @"  posit.y") { value.Malformations.AddIncorrectName<Matrix, double>(x => x.posity, subTok.GetRawName()); }
            subTok = tok.GetSubToken(index, 11); subTok.ApplySingle(value, x => x.positz); if (subTok.GetRawName() != @"  posit.z") { value.Malformations.AddIncorrectName<Matrix, double>(x => x.positz, subTok.GetRawName()); }

            return value;
        }
        public static Matrix ReadMatrixOld<T>(this BZNStreamReader reader, string name, T? parent, Expression<Func<T, Matrix>>? property, int index = 0) where T : IMalformable
        {
            PropertyInfo? propInfo = null;
            if (property != null && property.Body is MemberExpression member && member.Member is PropertyInfo propInfo_)
                propInfo = propInfo_;


            IBZNToken? tok;
            tok = reader.ReadToken();
            if (tok == null || !tok.Validate(name, BinaryFieldType.DATA_MAT3DOLD))
                throw new Exception($"Failed to parse {name}/MAT3DOLD");

            Matrix value = tok.GetMatrixOld();

            // store the value into the property if possible
            if (parent != null && propInfo != null)
                propInfo.SetValue(parent, value);

            // we can't process anything, so just serve the matrix as is
            if (parent == null || propInfo == null)
                return value;

            // binary doesn't have subtokens, it's just a blob of data
            if (tok.IsBinary)
                return value;

            // we have stuff to play with, so lets re-read the values back into the matrix again, but this time pass through our validation function
            IBZNToken subTok;
            subTok = tok.GetSubToken(index, 0); subTok.ApplySingle(value, x => x.rightx); if (subTok.GetRawName() != @"  right_x") { value.Malformations.AddIncorrectName<Matrix, float>(x => x.rightx, subTok.GetRawName()); }
            subTok = tok.GetSubToken(index, 1); subTok.ApplySingle(value, x => x.righty); if (subTok.GetRawName() != @"  right_y") { value.Malformations.AddIncorrectName<Matrix, float>(x => x.righty, subTok.GetRawName()); }
            subTok = tok.GetSubToken(index, 2); subTok.ApplySingle(value, x => x.rightz); if (subTok.GetRawName() != @"  right_z") { value.Malformations.AddIncorrectName<Matrix, float>(x => x.rightz, subTok.GetRawName()); }
            subTok = tok.GetSubToken(index, 3); subTok.ApplySingle(value, x => x.upx   ); if (subTok.GetRawName() != @"  up_x"   ) { value.Malformations.AddIncorrectName<Matrix, float>(x => x.upx   , subTok.GetRawName()); }
            subTok = tok.GetSubToken(index, 4); subTok.ApplySingle(value, x => x.upy   ); if (subTok.GetRawName() != @"  up_y"   ) { value.Malformations.AddIncorrectName<Matrix, float>(x => x.upy   , subTok.GetRawName()); }
            subTok = tok.GetSubToken(index, 5); subTok.ApplySingle(value, x => x.upz   ); if (subTok.GetRawName() != @"  up_z"   ) { value.Malformations.AddIncorrectName<Matrix, float>(x => x.upz   , subTok.GetRawName()); }
            subTok = tok.GetSubToken(index, 6); subTok.ApplySingle(value, x => x.frontx); if (subTok.GetRawName() != @"  front_x") { value.Malformations.AddIncorrectName<Matrix, float>(x => x.frontx, subTok.GetRawName()); }
            subTok = tok.GetSubToken(index, 7); subTok.ApplySingle(value, x => x.fronty); if (subTok.GetRawName() != @"  front_y") { value.Malformations.AddIncorrectName<Matrix, float>(x => x.fronty, subTok.GetRawName()); }
            subTok = tok.GetSubToken(index, 8); subTok.ApplySingle(value, x => x.frontz); if (subTok.GetRawName() != @"  front_z") { value.Malformations.AddIncorrectName<Matrix, float>(x => x.frontz, subTok.GetRawName()); }

            //if (reader.Format == BZNFormat.Battlezone && reader.Version >= 0) // BREADCRUMB VER_BIGPOSIT
            //{
            //    subTok = tok.GetSubToken(index,  9); subTok.ReadDouble(value, x => x.positx); if (subTok.GetRawName() != @"  posit_x") { value.Malformations.AddIncorrectName<Matrix, double>(x => x.positx, subTok.GetRawName()); }
            //    subTok = tok.GetSubToken(index, 10); subTok.ReadDouble(value, x => x.posity); if (subTok.GetRawName() != @"  posit_y") { value.Malformations.AddIncorrectName<Matrix, double>(x => x.posity, subTok.GetRawName()); }
            //    subTok = tok.GetSubToken(index, 11); subTok.ReadDouble(value, x => x.positz); if (subTok.GetRawName() != @"  posit_z") { value.Malformations.AddIncorrectName<Matrix, double>(x => x.positz, subTok.GetRawName()); }
            //}
            //else
            {
                subTok = tok.GetSubToken(index,  9); subTok.ApplySingle(value, x => x.positx); if (subTok.GetRawName() != @"  posit_x") { value.Malformations.AddIncorrectName<Matrix, double>(x => x.positx, subTok.GetRawName()); }
                subTok = tok.GetSubToken(index, 10); subTok.ApplySingle(value, x => x.posity); if (subTok.GetRawName() != @"  posit_y") { value.Malformations.AddIncorrectName<Matrix, double>(x => x.posity, subTok.GetRawName()); }
                subTok = tok.GetSubToken(index, 11); subTok.ApplySingle(value, x => x.positz); if (subTok.GetRawName() != @"  posit_z") { value.Malformations.AddIncorrectName<Matrix, double>(x => x.positz, subTok.GetRawName()); }
            }

            return value;
        }
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
        [Obsolete]
        public static void CheckMalformationsMatrix(this IBZNToken tok, IMalformable.MalformationManager malformations, FloatTextFormat floatFormat, int index = 0)
        {
            if (tok.IsBinary)
            {
                // check for malformations in the binary data and add to the token's malformation manager if found
            }
            else
            {
//                IBZNToken subTok;
//                subTok = tok.GetSubToken(index,  0); if (subTok.GetRawName() != @"  right_x") { malformations.AddIncorrectName(@"  right_x", subTok.GetRawName()); } subTok.CheckMalformationsSingle(@"  right_x", malformations, floatFormat);
//                subTok = tok.GetSubToken(index,  1); if (subTok.GetRawName() != @"  right_y") { malformations.AddIncorrectName(@"  right_y", subTok.GetRawName()); } subTok.CheckMalformationsSingle(@"  right_y", malformations, floatFormat);
//                subTok = tok.GetSubToken(index,  2); if (subTok.GetRawName() != @"  right_z") { malformations.AddIncorrectName(@"  right_z", subTok.GetRawName()); } subTok.CheckMalformationsSingle(@"  right_z", malformations, floatFormat);
//                subTok = tok.GetSubToken(index,  3); if (subTok.GetRawName() != @"  up_x"   ) { malformations.AddIncorrectName(@"  up_x"   , subTok.GetRawName()); } subTok.CheckMalformationsSingle(@"  up_x"   , malformations, floatFormat);
//                subTok = tok.GetSubToken(index,  4); if (subTok.GetRawName() != @"  up_y"   ) { malformations.AddIncorrectName(@"  up_y"   , subTok.GetRawName()); } subTok.CheckMalformationsSingle(@"  up_y"   , malformations, floatFormat);
//                subTok = tok.GetSubToken(index,  5); if (subTok.GetRawName() != @"  up_z"   ) { malformations.AddIncorrectName(@"  up_z"   , subTok.GetRawName()); } subTok.CheckMalformationsSingle(@"  up_z"   , malformations, floatFormat);
//                subTok = tok.GetSubToken(index,  6); if (subTok.GetRawName() != @"  front_x") { malformations.AddIncorrectName(@"  front_x", subTok.GetRawName()); } subTok.CheckMalformationsSingle(@"  front_x", malformations, floatFormat);
//                subTok = tok.GetSubToken(index,  7); if (subTok.GetRawName() != @"  front_y") { malformations.AddIncorrectName(@"  front_y", subTok.GetRawName()); } subTok.CheckMalformationsSingle(@"  front_y", malformations, floatFormat);
//                subTok = tok.GetSubToken(index,  8); if (subTok.GetRawName() != @"  front_z") { malformations.AddIncorrectName(@"  front_z", subTok.GetRawName()); } subTok.CheckMalformationsSingle(@"  front_z", malformations, floatFormat);
//                subTok = tok.GetSubToken(index,  9); if (subTok.GetRawName() != @"  posit_x") { malformations.AddIncorrectName(@"  posit_x", subTok.GetRawName()); } subTok.CheckMalformationsSingle(@"  posit_x", malformations, floatFormat);
//                subTok = tok.GetSubToken(index, 10); if (subTok.GetRawName() != @"  posit_y") { malformations.AddIncorrectName(@"  posit_y", subTok.GetRawName()); } subTok.CheckMalformationsSingle(@"  posit_y", malformations, floatFormat);
//                subTok = tok.GetSubToken(index, 11); if (subTok.GetRawName() != @"  posit_z") { malformations.AddIncorrectName(@"  posit_z", subTok.GetRawName()); } subTok.CheckMalformationsSingle(@"  posit_z", malformations, floatFormat);
            }
        }
        [Obsolete]
        public static void CheckMalformationsEuler(this IBZNToken tok, Euler euler, FloatTextFormat floatFormat, int index = 0)
        {
            if (tok.IsBinary)
            {
                // check for malformations in the binary data and add to the token's malformation manager if found
            }
            else
            {
//                IBZNToken subTok;
//                subTok = tok.GetSubToken(index, 0); if (subTok.GetRawName() != @" mass"     ) { euler.Malformations.AddIncorrectName(@" mass"     , subTok.GetRawName()); } subTok.CheckMalformationsSingle(@" mass"     , euler.Malformations, floatFormat);
//                subTok = tok.GetSubToken(index, 1); if (subTok.GetRawName() != @" mass_inv" ) { euler.Malformations.AddIncorrectName(@" mass_inv" , subTok.GetRawName()); } subTok.CheckMalformationsSingle(@" mass_inv" , euler.Malformations, floatFormat);
//                subTok = tok.GetSubToken(index, 2); if (subTok.GetRawName() != @" v_mag"    ) { euler.Malformations.AddIncorrectName(@" v_mag"    , subTok.GetRawName()); } subTok.CheckMalformationsSingle(@" v_mag"    , euler.Malformations, floatFormat);
//                subTok = tok.GetSubToken(index, 3); if (subTok.GetRawName() != @" v_mag_inv") { euler.Malformations.AddIncorrectName(@" v_mag_inv", subTok.GetRawName()); } subTok.CheckMalformationsSingle(@" v_mag_inv", euler.Malformations, floatFormat);
//                subTok = tok.GetSubToken(index, 4); if (subTok.GetRawName() != @" I"        ) { euler.Malformations.AddIncorrectName(@" I"        , subTok.GetRawName()); } subTok.CheckMalformationsSingle(@" I"        , euler.Malformations, floatFormat);
//                subTok = tok.GetSubToken(index, 5); if (subTok.GetRawName() != @" k_i"      ) { euler.Malformations.AddIncorrectName(@" k_i"      , subTok.GetRawName()); } subTok.CheckMalformationsSingle(@" k_i"      , euler.Malformations, floatFormat);
//                subTok = tok.GetSubToken(index, 6); if (subTok.GetRawName() != @" v"        ) { euler.Malformations.AddIncorrectName(@" v"        , subTok.GetRawName()); } subTok.CheckMalformationsVector3D(euler.v.Malformations    , floatFormat);
//                subTok = tok.GetSubToken(index, 7); if (subTok.GetRawName() != @" omega"    ) { euler.Malformations.AddIncorrectName(@" omega"    , subTok.GetRawName()); } subTok.CheckMalformationsVector3D(euler.omega.Malformations, floatFormat);
//                subTok = tok.GetSubToken(index, 8); if (subTok.GetRawName() != @" Accel"    ) { euler.Malformations.AddIncorrectName(@" Accel"    , subTok.GetRawName()); } subTok.CheckMalformationsVector3D(euler.Accel.Malformations, floatFormat);
            }
        }

        [Obsolete]
        public static void CheckMalformationsVector2D(this IBZNToken tok, IMalformable.MalformationManager malformations, FloatTextFormat floatFormat, int index = 0)
        {
            if (tok.IsBinary)
            {
                // check for malformations in the binary data and add to the token's malformation manager if found
            }
            else
            {
//                IBZNToken subTok;
//                subTok = tok.GetSubToken(index, 0); if (subTok.GetRawName() != @"  x") { malformations.AddIncorrectName(@"  x", subTok.GetRawName()); } subTok.CheckMalformationsSingle("  x", malformations, floatFormat);
//                subTok = tok.GetSubToken(index, 1); if (subTok.GetRawName() != @"  z") { malformations.AddIncorrectName(@"  z", subTok.GetRawName()); } subTok.CheckMalformationsSingle("  z", malformations, floatFormat);
            }
        }

        [Obsolete]
        public static void CheckMalformationsVector3D(this IBZNToken tok, IMalformable.MalformationManager malformations, FloatTextFormat floatFormat, int index = 0)
        {
            if (tok.IsBinary)
            {
                // check for malformations in the binary data and add to the token's malformation manager if found
            }
            else
            {
//                IBZNToken subTok;
//                subTok = tok.GetSubToken(index, 0); if (subTok.GetRawName() != @"  x") { malformations.AddIncorrectName(@"  x", subTok.GetRawName()); } subTok.CheckMalformationsSingle("  x", malformations, floatFormat);
//                subTok = tok.GetSubToken(index, 1); if (subTok.GetRawName() != @"  y") { malformations.AddIncorrectName(@"  y", subTok.GetRawName()); } subTok.CheckMalformationsSingle("  y", malformations, floatFormat);
//                subTok = tok.GetSubToken(index, 2); if (subTok.GetRawName() != @"  z") { malformations.AddIncorrectName(@"  z", subTok.GetRawName()); } subTok.CheckMalformationsSingle("  z", malformations, floatFormat);
            }
        }

        [Obsolete]
        public static void CheckMalformationsSingle(this IBZNToken tok, string name, IMalformable.MalformationManager malformations, FloatTextFormat floatFormat, int index = 0)
        {
            if (tok.IsBinary)
            {
                // check for malformations in the binary data and add to the token's malformation manager if found
            }
            else
            {
//                if (SingleExtension.GetFloatTextFormat(tok.GetString(index)) != floatFormat || tok.GetSingle(index).ToBZNString(floatFormat) != tok.GetString(index))
//                    malformations.AddIncorrectTextParse(name, tok.GetString(index));
            }
        }
        [Obsolete]
        public static void CheckMalformationsBool(this IBZNToken tok, string name, IMalformable.MalformationManager malformations, int index = 0)
        {
            if (tok.IsBinary)
            {
                // check for malformations in the binary data and add to the token's malformation manager if found
            }
            else
            {
//                if (tok.GetBoolean(index).ToString().ToLowerInvariant() != tok.GetString(index))
//                    malformations.AddIncorrectTextParse(name, tok.GetString(index));
            }
        }

        [Obsolete]
        public static string CorrectName(this IMalformable.MalformationManager malformations, bool preserveMalformations, string name)
        {
            if (preserveMalformations)
            {
//                var malIncorrectName = malformations.GetMalformations(Malformation.INCORRECT_NAME, name);
//                if (malIncorrectName.Any())
//                {
//                    var mal = malIncorrectName.First();
//                    name = (string)mal.Fields[0];
//                }
            }
            return name;
        }
    }
}
