using BZNParser.Battlezone;
using BZNParser.Tokenizer;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Linq.Expressions;
using System.Numerics;
using System.Reflection;
using System.Reflection.Emit;
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
            this.internalType2 = default!;
        }

        public DualModeValue(T2 value)
        {
            this.internalType2 = value;
            this.fromType2 = true;
            this.internalType1 = default!;
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
                euler.DisableMalformationAutoFix();

                try
                {
                    tok = reader.ReadToken();
                    if (tok == null || !tok.Validate(null, BinaryFieldType.DATA_FLOAT)) throw new Exception("Failed to parse euler's FLOAT");
                    tok.ApplySingle(euler, x => x.Mass);

                    tok = reader.ReadToken();
                    if (tok == null || !tok.Validate(null, BinaryFieldType.DATA_FLOAT)) throw new Exception("Failed to parse euler's FLOAT");
                    tok.ApplySingle(euler, x => x.MassInv);

                    tok = reader.ReadToken();
                    if (tok == null || !tok.Validate(null, BinaryFieldType.DATA_FLOAT)) throw new Exception("Failed to parse euler's FLOAT");
                    tok.ApplySingle(euler, x => x.VMag);

                    tok = reader.ReadToken();
                    if (tok == null || !tok.Validate(null, BinaryFieldType.DATA_FLOAT)) throw new Exception("Failed to parse euler's FLOAT");
                    tok.ApplySingle(euler, x => x.VMagInv);

                    tok = reader.ReadToken();
                    if (tok == null || !tok.Validate(null, BinaryFieldType.DATA_FLOAT)) throw new Exception("Failed to parse euler's FLOAT");
                    tok.ApplySingle(euler, x => x.I);

                    tok = reader.ReadToken();
                    if (tok == null || !tok.Validate(null, BinaryFieldType.DATA_FLOAT)) throw new Exception("Failed to parse euler's FLOAT");
                    tok.ApplySingle(euler, x => x.IInv);

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
                finally
                {
                    euler.EnableMalformationAutoFix();
                }
            }

            tok = reader.ReadToken();
            if (tok == null || !tok.Validate(name, BinaryFieldType.DATA_UNKNOWN))
                throw new Exception($"Failed to parse {name}");

            Euler value = tok.GetEuler(index);
            value.DisableMalformationAutoFix();

            try
            {
                // store the value into the property if possible
                if (parent != null && propInfo != null)
                    propInfo.SetValue(parent, value);

                // we can't process anything, so just serve the euler as is
                if (parent == null || propInfo == null)
                    return value;
            
                IBZNToken subTok;
                subTok = tok.GetSubToken(index, 0); subTok.ApplySingle(value, x => x.Mass   ); if (subTok.GetRawName() != @" mass"     ) { value.Malformations.AddIncorrectName<Euler, float>(x => x.Mass    , subTok.GetRawName()); }
                subTok = tok.GetSubToken(index, 1); subTok.ApplySingle(value, x => x.MassInv); if (subTok.GetRawName() != @" mass_inv" ) { value.Malformations.AddIncorrectName<Euler, float>(x => x.MassInv , subTok.GetRawName()); }
                subTok = tok.GetSubToken(index, 2); subTok.ApplySingle(value, x => x.VMag   ); if (subTok.GetRawName() != @" v_mag"    ) { value.Malformations.AddIncorrectName<Euler, float>(x => x.VMag    , subTok.GetRawName()); }
                subTok = tok.GetSubToken(index, 3); subTok.ApplySingle(value, x => x.VMagInv); if (subTok.GetRawName() != @" v_mag_inv") { value.Malformations.AddIncorrectName<Euler, float>(x => x.VMagInv , subTok.GetRawName()); }
                subTok = tok.GetSubToken(index, 4); subTok.ApplySingle(value, x => x.I      ); if (subTok.GetRawName() != @" I"        ) { value.Malformations.AddIncorrectName<Euler, float>(x => x.I       , subTok.GetRawName()); }
                subTok = tok.GetSubToken(index, 5); subTok.ApplySingle(value, x => x.IInv   ); if (subTok.GetRawName() != @" k_i"      ) { value.Malformations.AddIncorrectName<Euler, float>(x => x.IInv    , subTok.GetRawName()); }
                subTok = tok.GetSubToken(index, 6); subTok.ApplyVector3D(value, x => x.v    ); if (subTok.GetRawName() != @" v"        ) { value.Malformations.AddIncorrectName<Euler, Vector3D>(x => x.v    , subTok.GetRawName()); }
                subTok = tok.GetSubToken(index, 7); subTok.ApplyVector3D(value, x => x.omega); if (subTok.GetRawName() != @" omega"    ) { value.Malformations.AddIncorrectName<Euler, Vector3D>(x => x.omega, subTok.GetRawName()); }
                subTok = tok.GetSubToken(index, 8); subTok.ApplyVector3D(value, x => x.Accel); if (subTok.GetRawName() != @" Accel"    ) { value.Malformations.AddIncorrectName<Euler, Vector3D>(x => x.Accel, subTok.GetRawName()); }

                return value;
            }
            finally
            {
                value.EnableMalformationAutoFix();
            }
        }
    }

    public class Vector3D : IMalformable
    {
        private readonly IMalformable.MalformationManager _malformationManager;
        public IMalformable.MalformationManager Malformations => _malformationManager;
        public Vector3D()
        {
            this._malformationManager = new IMalformable.MalformationManager(this);
            this.x = 0;
            this.y = 0;
            this.z = 0;
            DisableMalformationAutoFix();
        }
        public Vector3D(float x, float y, float z)
        {
            this._malformationManager = new IMalformable.MalformationManager(this);
            this.x = x;
            this.y = y;
            this.z = z;
            EnableMalformationAutoFix();
        }
        public void ClearMalformations()
        {
            Malformations.Clear();
        }
        private bool blockAutoFixMalformations = false;
        public void DisableMalformationAutoFix()
        {
            blockAutoFixMalformations = true;
        }
        public void EnableMalformationAutoFix()
        {
            blockAutoFixMalformations = false;
        }

        public float X
        {
            get { return x; }
            set
            {
                if (!blockAutoFixMalformations && value != x)
                    Malformations.Clear<Vector3D, float>((x) => x.X);
                x = value;
            }
        }
        public float Y
        {
            get { return y; }
            set
            {
                if (!blockAutoFixMalformations && value != y)
                    Malformations.Clear<Vector3D, float>((x) => x.Y);
                y = value;
            }
        }
        public float Z
        {
            get { return z; }
            set
            {
                if (!blockAutoFixMalformations && value != z)
                    Malformations.Clear<Vector3D, float>((x) => x.Z);
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
        public Vector2D()
        {
            this._malformationManager = new IMalformable.MalformationManager(this);
            this.x = 0;
            this.z = 0;
            DisableMalformationAutoFix();
        }
        public Vector2D(float x, float z)
        {
            this._malformationManager = new IMalformable.MalformationManager(this);
            this.x = x;
            this.z = z;
            EnableMalformationAutoFix();
        }
        public void ClearMalformations()
        {
            Malformations.Clear();
        }
        private bool blockAutoFixMalformations = false;
        public void DisableMalformationAutoFix()
        {
            blockAutoFixMalformations = true;
        }
        public void EnableMalformationAutoFix()
        {
            blockAutoFixMalformations = false;
        }



        public float X
        {
            get { return x; }
            set
            {
                if (!blockAutoFixMalformations && value != x)
                    Malformations.Clear<Vector2D, float>((x) => x.X);
                x = value;
            }
        }
        public float Z
        {
            get { return z; }
            set
            {
                if (!blockAutoFixMalformations && value != z)
                    Malformations.Clear<Vector2D, float>((x) => x.Z);
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
        public Euler()
        {
            this._malformationManager = new IMalformable.MalformationManager(this);
            this.mass = 0;
            this.mass_inv = 0;
            this.i = 0;
            this.i_inv = 0;
            this.v_mag = 0;
            this.v_mag_inv = 0;
            EnableMalformationAutoFix();
        }
        public void ClearMalformations()
        {
            v.ClearMalformations();
            omega.ClearMalformations();
            Accel.ClearMalformations();
            Alpha.ClearMalformations();
            Pos.ClearMalformations();
            Malformations.Clear();
        }
        private bool blockAutoFixMalformations = false;
        public void DisableMalformationAutoFix()
        {
            v.DisableMalformationAutoFix();
            omega.DisableMalformationAutoFix();
            Accel.DisableMalformationAutoFix();
            Alpha.DisableMalformationAutoFix();
            Pos.DisableMalformationAutoFix();
            blockAutoFixMalformations = true;
        }
        public void EnableMalformationAutoFix()
        {
            v.EnableMalformationAutoFix();
            omega.EnableMalformationAutoFix();
            Accel.EnableMalformationAutoFix();
            Alpha.EnableMalformationAutoFix();
            Pos.EnableMalformationAutoFix();
            blockAutoFixMalformations = false;
        }

        public const float EPSILON = 1.0e-4f;
        public const float HUGE_NUMBER = 1.0e30f;

        //public Quaternion Att { get; set; }

        public Vector3D v { get; set; }
        public Vector3D omega { get; set; }
        public Vector3D Accel { get; set; }

        public Vector3D Alpha { get; set; }
        public Vector3D Pos { get; set; }

        public float Mass {
            get { return mass; }
            set
            {
                if (!blockAutoFixMalformations && value != mass)
                    Malformations.Clear<Euler, float>((x) => x.Mass);
                mass = value;
            }
        }
        public float MassInv {
            get { return mass_inv; }
            set
            {
                if (!blockAutoFixMalformations && value != mass_inv)
                    Malformations.Clear<Euler, float>((x) => x.MassInv);
                mass_inv = value;
            }
        }

        public float I {
            get { return i; }
            set
            {
                if (!blockAutoFixMalformations && value != i)
                    Malformations.Clear<Euler, float>((x) => x.I);
                i = value;
            }
        }
        public float IInv {
            get { return i_inv; }
            set
            {
                if (!blockAutoFixMalformations && value != i_inv)
                    Malformations.Clear<Euler, float>((x) => x.IInv);
                i_inv = value;
            }
        }

        public float VMag {
            get { return v_mag; }
            set
            {
                if (!blockAutoFixMalformations && value != v_mag)
                    Malformations.Clear<Euler, float>((x) => x.VMag);
                v_mag = value;
            }
        }
        public float VMagInv
        {
            get { return v_mag_inv; }
            set
            {
                if (!blockAutoFixMalformations && value != v_mag_inv)
                    Malformations.Clear<Euler, float>((x) => x.VMagInv);
                v_mag_inv = value;
            }
        }

        private float mass;
        private float mass_inv;
        private float i;
        private float i_inv;
        private float v_mag;
        private float v_mag_inv;

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
            MassInv = HUGE_NUMBER;
            IInv = HUGE_NUMBER;
            if (Mass > EPSILON)
            {
                MassInv = 1.0f / Mass;
                IInv = 1.0f / I;
            }
            else
            {
                MassInv = HUGE_NUMBER;
                IInv = HUGE_NUMBER;
            }
        }


        // Reads the 'v' member, builds the 'v_mag' and 'v_mag_inv' members
        public void CalcVMag()
        {
            VMag = v.Magnitude();
            VMagInv = (VMag == 0.0f) ? HUGE_NUMBER : 1.0f / VMag;
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
        public Matrix()
        {
            this._malformationManager = new IMalformable.MalformationManager(this);
            this.rightx = 1;
            this.righty = 0;
            this.rightz = 0;
            this.rightw = 0;
            this.upx = 0;
            this.upy = 1;
            this.upz = 0;
            this.upw = 0;
            this.frontx = 0;
            this.fronty = 0;
            this.frontz = 1;
            this.frontw = 0;
            this.positx = 0;
            this.posity = 0;
            this.positz = 0;
            this.positw = 1;
        }
        public Matrix(Vector3D pos)
        {
            this._malformationManager = new IMalformable.MalformationManager(this);
            this.rightx = 1;
            this.righty = 0;
            this.rightz = 0;
            this.rightw = 0;
            this.upx = 0;
            this.upy = 1;
            this.upz = 0;
            this.upw = 0;
            this.frontx = 0;
            this.fronty = 0;
            this.frontz = 1;
            this.frontw = 0;
            this.positx = pos.X;
            this.posity = pos.Y;
            this.positz = pos.Z;
            this.positw = 1;
        }
        public void ClearMalformations()
        {
            Malformations.Clear();
            junk = 0;
        }
        private bool blockAutoFixMalformations = false;
        public void DisableMalformationAutoFix()
        {
            blockAutoFixMalformations = true;
        }
        public void EnableMalformationAutoFix()
        {
            blockAutoFixMalformations = false;
        }


        public float RightX {
            get { return rightx; }
            set
            {
                if (!blockAutoFixMalformations && value != rightx)
                    Malformations.Clear<Matrix, float>((x) => x.RightX);
                rightx = value;
            }
        }
        public float RightY {
            get { return righty; }
            set
            {
                if (!blockAutoFixMalformations && value != righty)
                    Malformations.Clear<Matrix, float>((x) => x.RightY);
                righty = value;
            }
        }
        public float RightZ {
            get { return rightz; }
            set
            {
                if (!blockAutoFixMalformations && value != rightz)
                    Malformations.Clear<Matrix, float>((x) => x.RightZ);
                rightz = value;
            }
        }
        public float RightW {
            get { return rightw; }
            set
            {
                if (!blockAutoFixMalformations && value != rightw)
                    Malformations.Clear<Matrix, float>((x) => x.RightW);
                rightw = value;
            }
        }
        public float UpX {
            get { return upx; }
            set
            {
                if (!blockAutoFixMalformations && value != upx)
                    Malformations.Clear<Matrix, float>((x) => x.UpX);
                upx = value;
            }
        }
        public float UpY {
            get { return upy; }
            set
            {
                if (!blockAutoFixMalformations && value != upy)
                    Malformations.Clear<Matrix, float>((x) => x.UpY);
                upy = value;
            }
        }
        public float UpZ {
            get { return upz; }
            set
            {
                if (!blockAutoFixMalformations && value != upz)
                    Malformations.Clear<Matrix, float>((x) => x.UpZ);
                upz = value;
            }
        }
        public float UpW {
            get { return upw; }
            set
            {
                if (!blockAutoFixMalformations && value != upw)
                    Malformations.Clear<Matrix, float>((x) => x.UpW);
                upw = value;
            }
        }
        public float FrontX {
            get { return frontx; }
            set
            {
                if (!blockAutoFixMalformations && value != frontx)
                    Malformations.Clear<Matrix, float>((x) => x.FrontX);
                frontx = value;
            }
        }
        public float FrontY {
            get { return fronty; }
            set
            {
                if (!blockAutoFixMalformations && value != fronty)
                    Malformations.Clear<Matrix, float>((x) => x.FrontY);
                fronty = value;
            }
        }
        public float FrontZ {
            get { return frontz; }
            set
            {
                if (!blockAutoFixMalformations && value != frontz)
                    Malformations.Clear<Matrix, float>((x) => x.FrontZ);
                frontz = value;
            }
        }
        public float FrontW {
            get { return frontw; }
            set
            {
                if (!blockAutoFixMalformations && value != frontw)
                    Malformations.Clear<Matrix, float>((x) => x.FrontW);
                frontw = value;
            }
        }
        public UInt32 junk { get; set; } // used only in double-posit mode
        public double PositX {
            get { return positx; }
            set
            {
                if (!blockAutoFixMalformations && value != positx)
                    Malformations.Clear<Matrix, double>((x) => x.PositX);
                positx = value;
            }
        }
        public double PositY {
            get { return posity; }
            set
            {
                if (!blockAutoFixMalformations && value != posity)
                    Malformations.Clear<Matrix, double>((x) => x.PositY);
                posity = value;
            }
        }
        public double PositZ {
            get { return positz; }
            set
            {
                if (!blockAutoFixMalformations && value != positz)
                    Malformations.Clear<Matrix, double>((x) => x.PositZ);
                positz = value;
            }
        }
        public double PositW {
            get { return positw; }
            set
            {
                if (!blockAutoFixMalformations && value != positw)
                    Malformations.Clear<Matrix, double>((x) => x.PositW);
                positw = value;
            }
        }

        private float rightx;
        private float righty;
        private float rightz;
        private float rightw;
        private float upx;
        private float upy;
        private float upz;
        private float upw;
        private float frontx;
        private float fronty;
        private float frontz;
        private float frontw;
        private double positx;
        private double posity;
        private double positz;
        private double positw;
    }

    static class MatrixExtension
    {
        public static Matrix ReadMatrix<T>(this BZNStreamReader reader, string name, T? parent, Expression<Func<T, Matrix>> property, int index = 0) where T : IMalformable
        {
            PropertyInfo? propInfo = null;
            if (property != null && property.Body is MemberExpression member && member.Member is PropertyInfo propInfo_)
                propInfo = propInfo_;

            IBZNToken? tok;
            tok = reader.ReadToken();
            if (tok == null || !tok.Validate(name, BinaryFieldType.DATA_MAT3D))
                throw new Exception($"Failed to parse {name}/MAT3D");

            Matrix value = tok.GetMatrix(index);
            value.DisableMalformationAutoFix();
            try
            {
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
                subTok = tok.GetSubToken(index,  0); subTok.ApplySingle(value, x => x.RightX); if (subTok.GetRawName() != @"  right.x") { value.Malformations.AddIncorrectName<Matrix, float>(x => x.RightX, subTok.GetRawName()); }
                subTok = tok.GetSubToken(index,  1); subTok.ApplySingle(value, x => x.RightY); if (subTok.GetRawName() != @"  right.y") { value.Malformations.AddIncorrectName<Matrix, float>(x => x.RightY, subTok.GetRawName()); }
                subTok = tok.GetSubToken(index,  2); subTok.ApplySingle(value, x => x.RightZ); if (subTok.GetRawName() != @"  right.z") { value.Malformations.AddIncorrectName<Matrix, float>(x => x.RightZ, subTok.GetRawName()); }
                subTok = tok.GetSubToken(index,  3); subTok.ApplySingle(value, x => x.UpX   ); if (subTok.GetRawName() != @"  up.x"   ) { value.Malformations.AddIncorrectName<Matrix, float>(x => x.UpX, subTok.GetRawName()); }
                subTok = tok.GetSubToken(index,  4); subTok.ApplySingle(value, x => x.UpY   ); if (subTok.GetRawName() != @"  up.y"   ) { value.Malformations.AddIncorrectName<Matrix, float>(x => x.UpY, subTok.GetRawName()); }
                subTok = tok.GetSubToken(index,  5); subTok.ApplySingle(value, x => x.UpZ   ); if (subTok.GetRawName() != @"  up.z"   ) { value.Malformations.AddIncorrectName<Matrix, float>(x => x.UpZ, subTok.GetRawName()); }
                subTok = tok.GetSubToken(index,  6); subTok.ApplySingle(value, x => x.FrontX); if (subTok.GetRawName() != @"  front.x") { value.Malformations.AddIncorrectName<Matrix, float>(x => x.FrontX, subTok.GetRawName()); }
                subTok = tok.GetSubToken(index,  7); subTok.ApplySingle(value, x => x.FrontY); if (subTok.GetRawName() != @"  front.y") { value.Malformations.AddIncorrectName<Matrix, float>(x => x.FrontY, subTok.GetRawName()); }
                subTok = tok.GetSubToken(index,  8); subTok.ApplySingle(value, x => x.FrontZ); if (subTok.GetRawName() != @"  front.z") { value.Malformations.AddIncorrectName<Matrix, float>(x => x.FrontZ, subTok.GetRawName()); }
                subTok = tok.GetSubToken(index,  9); subTok.ApplySingle(value, x => x.PositX); if (subTok.GetRawName() != @"  posit.x") { value.Malformations.AddIncorrectName<Matrix, double>(x => x.PositX, subTok.GetRawName()); }
                subTok = tok.GetSubToken(index, 10); subTok.ApplySingle(value, x => x.PositY); if (subTok.GetRawName() != @"  posit.y") { value.Malformations.AddIncorrectName<Matrix, double>(x => x.PositY, subTok.GetRawName()); }
                subTok = tok.GetSubToken(index, 11); subTok.ApplySingle(value, x => x.PositZ); if (subTok.GetRawName() != @"  posit.z") { value.Malformations.AddIncorrectName<Matrix, double>(x => x.PositZ, subTok.GetRawName()); }

                return value;
            }
            finally
            {
                value.EnableMalformationAutoFix();
            }
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
            value.DisableMalformationAutoFix();
            try
            {
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
                subTok = tok.GetSubToken(index, 0); subTok.ApplySingle(value, x => x.RightX); if (subTok.GetRawName() != @"  right_x") { value.Malformations.AddIncorrectName<Matrix, float>(x => x.RightX, subTok.GetRawName()); }
                subTok = tok.GetSubToken(index, 1); subTok.ApplySingle(value, x => x.RightY); if (subTok.GetRawName() != @"  right_y") { value.Malformations.AddIncorrectName<Matrix, float>(x => x.RightY, subTok.GetRawName()); }
                subTok = tok.GetSubToken(index, 2); subTok.ApplySingle(value, x => x.RightZ); if (subTok.GetRawName() != @"  right_z") { value.Malformations.AddIncorrectName<Matrix, float>(x => x.RightZ, subTok.GetRawName()); }
                subTok = tok.GetSubToken(index, 3); subTok.ApplySingle(value, x => x.UpX); if (subTok.GetRawName() != @"  up_x") { value.Malformations.AddIncorrectName<Matrix, float>(x => x.UpX, subTok.GetRawName()); }
                subTok = tok.GetSubToken(index, 4); subTok.ApplySingle(value, x => x.UpY); if (subTok.GetRawName() != @"  up_y") { value.Malformations.AddIncorrectName<Matrix, float>(x => x.UpY, subTok.GetRawName()); }
                subTok = tok.GetSubToken(index, 5); subTok.ApplySingle(value, x => x.UpZ); if (subTok.GetRawName() != @"  up_z") { value.Malformations.AddIncorrectName<Matrix, float>(x => x.UpZ, subTok.GetRawName()); }
                subTok = tok.GetSubToken(index, 6); subTok.ApplySingle(value, x => x.FrontX); if (subTok.GetRawName() != @"  front_x") { value.Malformations.AddIncorrectName<Matrix, float>(x => x.FrontX, subTok.GetRawName()); }
                subTok = tok.GetSubToken(index, 7); subTok.ApplySingle(value, x => x.FrontY); if (subTok.GetRawName() != @"  front_y") { value.Malformations.AddIncorrectName<Matrix, float>(x => x.FrontY, subTok.GetRawName()); }
                subTok = tok.GetSubToken(index, 8); subTok.ApplySingle(value, x => x.FrontZ); if (subTok.GetRawName() != @"  front_z") { value.Malformations.AddIncorrectName<Matrix, float>(x => x.FrontZ, subTok.GetRawName()); }

                //if (reader.Format == BZNFormat.Battlezone && reader.Version >= 0) // BREADCRUMB VER_BIGPOSIT
                //{
                //    subTok = tok.GetSubToken(index,  9); subTok.ReadDouble(value, x => x.positx); if (subTok.GetRawName() != @"  posit_x") { value.Malformations.AddIncorrectName<Matrix, double>(x => x.positx, subTok.GetRawName()); }
                //    subTok = tok.GetSubToken(index, 10); subTok.ReadDouble(value, x => x.posity); if (subTok.GetRawName() != @"  posit_y") { value.Malformations.AddIncorrectName<Matrix, double>(x => x.posity, subTok.GetRawName()); }
                //    subTok = tok.GetSubToken(index, 11); subTok.ReadDouble(value, x => x.positz); if (subTok.GetRawName() != @"  posit_z") { value.Malformations.AddIncorrectName<Matrix, double>(x => x.positz, subTok.GetRawName()); }
                //}
                //else
                {
                    subTok = tok.GetSubToken(index, 9); subTok.ApplySingle(value, x => x.PositX); if (subTok.GetRawName() != @"  posit_x") { value.Malformations.AddIncorrectName<Matrix, double>(x => x.PositX, subTok.GetRawName()); }
                    subTok = tok.GetSubToken(index, 10); subTok.ApplySingle(value, x => x.PositY); if (subTok.GetRawName() != @"  posit_y") { value.Malformations.AddIncorrectName<Matrix, double>(x => x.PositY, subTok.GetRawName()); }
                    subTok = tok.GetSubToken(index, 11); subTok.ApplySingle(value, x => x.PositZ); if (subTok.GetRawName() != @"  posit_z") { value.Malformations.AddIncorrectName<Matrix, double>(x => x.PositZ, subTok.GetRawName()); }
                }

                return value;
            }
            finally
            {
                value.EnableMalformationAutoFix();
            }
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
        public static string CorrectName(this IMalformable.MalformationManager malformations, string name)
        {
            //if (preserveMalformations)
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
