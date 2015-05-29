using System;
using System.Runtime.InteropServices;

namespace SuperInteger
{		
	[StructLayout(LayoutKind.Sequential)]
	internal struct GMP_mpz_t
	{
		public int _mp_alloc;
		public int _mp_size;
		public IntPtr ptr;
	}
		
	public class Integer
	{
		internal GMP_mpz_t _pointer;

		~Integer()
		{
			mpz_clear(ref _pointer);
		}

		public Integer()
		{
			mpz_init(ref _pointer);
		}

		public Integer(int initial)
		{
			mpz_init (ref _pointer);
			mpz_set_si (ref _pointer, initial);
		}

		public Integer(byte[] input)
		{
			mpz_init (ref _pointer);
			FromBytes (input);
		}

		public void FromBytes(byte[] input)
		{
			var point = Marshal.AllocHGlobal (input.Length);
			Marshal.Copy (input, 0, point, input.Length);
			mpz_import (ref _pointer, (uint)input.Length, -1, 1, 0, 0, point);
			Marshal.FreeHGlobal (point);
		}

		public byte[] ToBytes()
		{
			int count = mpz_sizeinbase (ref _pointer, 256);
			var output = new byte[count];
			var pointer = Marshal.AllocHGlobal (count);

			mpz_export(pointer, IntPtr.Zero, -1, 1, 0, 0, ref _pointer);
			Marshal.Copy (pointer, output, 0, count);
			Marshal.FreeHGlobal (pointer);
			return output;
		}

		public static Integer operator %(Integer op1, Integer mod)
		{
			Integer result = new Integer();
			mpz_mod(ref result._pointer, ref op1._pointer, ref mod._pointer);
			return result;
		}

		public static Integer operator *(Integer op1, Integer op2)
		{
			var result = new Integer();
			mpz_mul(ref result._pointer, ref op1._pointer, ref op2._pointer);
			return result;
		}

		public static Integer operator +(Integer g1, Integer g2)
		{
			var result = new Integer();
			mpz_add(ref result._pointer, ref g1._pointer, ref g2._pointer);
			return result;
		}

		public static Integer operator -(Integer g1, Integer g2)
		{
			var result = new Integer();
			mpz_sub(ref result._pointer, ref g1._pointer, ref g2._pointer);
			return result;
		}

		public static Integer operator /(Integer g1, Integer g2)
		{
			var result = new Integer();
			mpz_tdiv_q(ref result._pointer, ref g1._pointer, ref g2._pointer);
			return result;
		}

		public static bool operator <(Integer op1, Integer op2)
		{
			return mpz_cmp(ref op1._pointer, ref op2._pointer) < 0;
		}

		public static bool operator >(Integer op1, Integer op2)
		{
			return mpz_cmp(ref op1._pointer, ref op2._pointer) > 0;
		}

		public static bool operator >=(Integer op1, Integer op2)
		{
			return mpz_cmp(ref op1._pointer, ref op2._pointer) >= 0;
		}

		public static bool operator <=(Integer op1, Integer op2)
		{
			return mpz_cmp(ref op1._pointer, ref op2._pointer) <= 0;
		}

		public static bool IsEqual (Integer op1, Integer op2)
		{
			return mpz_cmp(ref op1._pointer, ref op2._pointer) == 0;
		}

		public bool IsEqual (Integer op1)
		{
			return mpz_cmp(ref op1._pointer, ref _pointer) == 0;
		}

		public static Integer Sqrt(Integer i)
		{
			var result = new Integer();
			mpz_sqrt(ref result._pointer, ref i._pointer);
			return result;
		}

		public Integer Sqrt()
		{
			return Sqrt(this);
		}

		public static Integer Pow(Integer i, uint j)
		{
			var result = new Integer();
			mpz_pow_ui(ref result._pointer, ref i._pointer, j);
			return result;
		}

		public static Integer Pow(Integer i, int j)
		{
			if (j >= 0) return Pow(i, (uint) j);
			throw new ArgumentOutOfRangeException("j");
		}

		public static Integer Pow(uint bas, uint exp)
		{
			var result = new Integer();
			mpz_ui_pow_ui(ref result._pointer, bas, exp);
			return result;
		}

		public Integer PowMod(Integer exp, Integer mod)
		{
			var result = new Integer();
			mpz_powm(ref result._pointer, ref this._pointer, ref exp._pointer, ref mod._pointer);
			return result;
		}

		public Integer Pow(int j)
		{
			return Pow(this, j);
		}
			
		public static int InverseModulo(Integer result, Integer op1, Integer op2)
		{
			return mpz_invert(ref result._pointer, ref op1._pointer, ref op2._pointer);
		}

		public override string ToString()
		{
			var str = new string(' ', mpz_sizeinbase(ref _pointer, (int) Math.Abs(10)) + 2);
			IntPtr unmanagedString = Marshal.StringToHGlobalAnsi(str); // allocate UNMANAGED space !
			mpz_get_str(unmanagedString, 10, ref _pointer);
			string result = Marshal.PtrToStringAnsi(unmanagedString); // allocate managed string
			Marshal.FreeHGlobal(unmanagedString); // free unmanaged space
			return result;
		}

		public int SizeInBase(int basis)
		{
			return mpz_sizeinbase(ref _pointer, basis);
		}

		[DllImport("gmp", EntryPoint = "__gmpz_init")]
		private static extern void mpz_init(ref GMP_mpz_t value);

		[DllImport("gmp", EntryPoint = "__gmpz_init_set_si")]
		private static extern void mpz_init_set_si(ref GMP_mpz_t value, int v);

		[DllImport("gmp", EntryPoint = "__gmpz_init_set_str")]
		private static extern int mpz_init_set_str(ref GMP_mpz_t rop, IntPtr s, int basis);

		[DllImport("gmp", EntryPoint = "__gmpz_clear")]
		private static extern void mpz_clear(ref GMP_mpz_t src);

		[DllImport("gmp", EntryPoint = "__gmpz_mul_si")]
		private static extern void mpz_mul_si(ref GMP_mpz_t dest, ref GMP_mpz_t src, int val);

		[DllImport("gmp", EntryPoint = "__gmpz_mul")]
		private static extern void mpz_mul(ref GMP_mpz_t dest, ref GMP_mpz_t op1, ref GMP_mpz_t op2);

		[DllImport("gmp", EntryPoint = "__gmpz_add")]
		private static extern void mpz_add(ref GMP_mpz_t dest, ref GMP_mpz_t src, ref GMP_mpz_t src2);

		[DllImport("gmp", EntryPoint = "__gmpz_tdiv_q")]
		private static extern void mpz_tdiv_q(ref GMP_mpz_t dest, ref GMP_mpz_t src, ref GMP_mpz_t src2);

		[DllImport("gmp", EntryPoint = "__gmpz_set")]
		private static extern void mpz_set(ref GMP_mpz_t dest, ref GMP_mpz_t src);

		[DllImport("gmp", EntryPoint = "__gmpz_set_si")]
		private static extern void mpz_set_si(ref GMP_mpz_t src, int value);

		[DllImport("gmp", EntryPoint = "__gmpz_set_str")]
		private static extern int mpz_set_str(ref GMP_mpz_t rop, IntPtr s, int sbase);

		[DllImport("gmp", EntryPoint = "__gmpz_get_si")]
		private static extern int mpz_get_si(ref GMP_mpz_t src);

		[DllImport("gmp", EntryPoint = "__gmpz_get_d")]
		private static extern double mpz_get_d(ref GMP_mpz_t src);

		[DllImport("gmp", EntryPoint = "__gmpz_get_str", CharSet = CharSet.Ansi)]
		private static extern IntPtr mpz_get_str(IntPtr out_string, int _base, ref GMP_mpz_t src);

		[DllImport("gmp", EntryPoint = "__gmpz_sizeinbase")]
		internal static extern int mpz_sizeinbase(ref GMP_mpz_t src, int _base);

		[DllImport("gmp", EntryPoint = "__gmpz_cmp")]
		private static extern int mpz_cmp(ref GMP_mpz_t op1, ref GMP_mpz_t op2);

		[DllImport("gmp", EntryPoint = "__gmpz_cmp_d")]
		private static extern int mpz_cmp_d(ref GMP_mpz_t op1, double op2);

		[DllImport("gmp", EntryPoint = "__gmpz_cmp_si")]
		private static extern int mpz_cmp_si(ref GMP_mpz_t op1, int op2);

		[DllImport("gmp", EntryPoint = "__gmpz_sub")]
		private static extern void mpz_sub(ref GMP_mpz_t rop, ref GMP_mpz_t op1, ref GMP_mpz_t op2);

		[DllImport("gmp", EntryPoint = "__gmpz_sqrt")]
		private static extern void mpz_sqrt(ref GMP_mpz_t rop, ref GMP_mpz_t op);

		[DllImport("gmp", EntryPoint = "__gmpz_pow_ui")]
		private static extern void mpz_pow_ui(ref GMP_mpz_t rop, ref GMP_mpz_t op, uint exp);

		[DllImport("gmp", EntryPoint = "__gmpz_powm")]
		private static extern void mpz_powm(ref GMP_mpz_t rop, ref GMP_mpz_t bas, ref GMP_mpz_t exp, ref GMP_mpz_t mod);

		[DllImport("gmp", EntryPoint = "__gmpz_mod")]
		private static extern void mpz_mod(ref GMP_mpz_t rop, ref GMP_mpz_t op1, ref GMP_mpz_t mod);

		[DllImport("gmp", EntryPoint = "__gmpz_ui_pow_ui")]
		private static extern void mpz_ui_pow_ui(ref GMP_mpz_t rop, uint bas, uint exp);

		[DllImport("gmp", EntryPoint = "__gmpz_invert")]
		private static extern int mpz_invert(ref GMP_mpz_t rop, ref GMP_mpz_t op1, ref GMP_mpz_t op2);

		[DllImport("gmp", EntryPoint="__gmpz_export")]
		private static extern IntPtr mpz_export (IntPtr dataOutput, IntPtr count, int order, int size, int endian, uint nails, ref GMP_mpz_t op);

		[DllImport("gmp", EntryPoint="__gmpz_import")]
		private static extern void mpz_import(ref GMP_mpz_t output, uint count, int order, int size, int endian, uint nails, IntPtr input);
	}
}

