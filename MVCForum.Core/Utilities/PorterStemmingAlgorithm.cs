using System;
using System.Runtime.InteropServices;

namespace MVCForum.Utilities 
{

    #region NOTES
    /*

	   Porter stemmer in CSharp, based on the Java port. The original paper is in

		   Porter, 1980, An algorithm for suffix stripping, Program, Vol. 14,
		   no. 3, pp 130-137,

	   See also http://www.tartarus.org/~martin/PorterStemmer

	   History:

	   Release 1

	   Bug 1 (reported by Gonzalo Parra 16/10/99) fixed as marked below.
	   The words 'aed', 'eed', 'oed' leave k at 'a' for step 3, and b[k-1]
	   is then out outside the bounds of b.

	   Release 2

	   Similarly,

	   Bug 2 (reported by Steve Dyrdahl 22/2/00) fixed as marked below.
	   'ion' by itself leaves j = -1 in the test for 'ion' in step 5, and
	   b[j] is then outside the bounds of b.

	   Release 3

	   Considerably revised 4/9/00 in the light of many helpful suggestions
	   from Brian Goetz of Quiotix Corporation (brian@quiotix.com).

	   Release 4
	   
	   This revision allows the Porter Stemmer Algorithm to be exported via the
	   .NET Framework. To facilate its use via .NET, the following commands need to be
	   issued to the operating system to register the component so that it can be
	   imported into .Net compatible languages, such as Delphi.NET, Visual Basic.NET,
	   Visual C++.NET, etc. 
	   
	   1. Create a stong name: 		
			sn -k Keyfile.snk  
	   2. Compile the C# class, which creates an assembly PorterStemmerAlgorithm.dll
			csc /t:library PorterStemmerAlgorithm.cs
	   3. Register the dll with the Windows Registry 
		  and so expose the interface to COM Clients via the type library 
		  ( PorterStemmerAlgorithm.tlb will be created)
			regasm /tlb PorterStemmerAlgorithm.dll
	   4. Load the component in the Global Assembly Cache
			gacutil -i PorterStemmerAlgorithm.dll
		
	   Note: You must have the .Net Studio installed.
	   
	   Once this process is performed you should be able to import the class 
	   via the appropiate mechanism in the language that you are using.
	   
	   i.e in Delphi 7 .NET this is simply a matter of selecting: 
			Project | Import Type Libary
	   And then selecting Porter stemmer in CSharp Version 1.4"!
	   
	   Cheers Leif
	
	*/

    /**
      * Stemmer, implementing the Porter Stemming Algorithm
      *
      * The Stemmer class transforms a word into its root form.  The input
      * word can be provided a character at time (by calling add()), or at once
      * by calling one of the various stem(something) methods.
      */
    
    #endregion

	public interface IStemmerInterface 
	{
		string StemTerm( string s );
	}


	public class PorterStemmer : IStemmerInterface
	{
		private char[] _b;
		private int _i,     /* offset into b */
			_iEnd, /* offset to end of stemmed word */
			_j, _k;

	    private const int INC = 200;
	    /* unit of size whereby b is increased */
		
		public PorterStemmer() 
		{
			_b = new char[INC];
			_i = 0;
			_iEnd = 0;
		}

		/* Implementation of the .NET interface - added as part of realease 4 (Leif) */
		public string StemTerm( string s )
		{
			SetTerm( s );
			stem();
			return getTerm();
		}

		/*
			SetTerm and GetTerm have been simply added to ease the 
			interface with other lanaguages. They replace the add functions 
			and toString function. This was done because the original functions stored
			all stemmed words (and each time a new woprd was added, the buffer would be
			re-copied each time, making it quite slow). Now, The class interface 
			that is provided simply accepts a term and returns its stem, 
			instead of storing all stemmed words.
			(Leif)
		*/

		
		
		void SetTerm( string s)
		{
			_i = s.Length;
			var new_b = new char[_i];
			for (int c = 0; c < _i; c++)
			new_b[c] = s[c];

			_b  = new_b;		

		}

		public string getTerm()
		{
			return new String(_b, 0, _iEnd);
		}


		/* Old interface to the class - left for posterity. However, it is not
		 * used when accessing the class via .NET (Leif)*/

		/**
		 * Add a character to the word being stemmed.  When you are finished
		 * adding characters, you can call stem(void) to stem the word.
		 */

		public void add(char ch) 
		{
			if (_i == _b.Length) 
			{
				var new_b = new char[_i+INC];
				for (var c = 0; c < _i; c++)
					new_b[c] = _b[c];
				_b = new_b;
			}
			_b[_i++] = ch;
		}


		/** Adds wLen characters to the word being stemmed contained in a portion
		 * of a char[] array. This is like repeated calls of add(char ch), but
		 * faster.
		 */

		public void add(char[] w, int wLen) 
		{
			if (_i+wLen >= _b.Length) 
			{
				var new_b = new char[_i+wLen+INC];
				for (int c = 0; c < _i; c++)
					new_b[c] = _b[c];
				_b = new_b;
			}
			for (int c = 0; c < wLen; c++)
				_b[_i++] = w[c];
		}

		/**
		 * After a word has been stemmed, it can be retrieved by toString(),
		 * or a reference to the internal buffer can be retrieved by getResultBuffer
		 * and getResultLength (which is generally more efficient.)
		 */
		public override string ToString() 
		{
			return new String(_b,0,_iEnd);
		}

		/**
		 * Returns the length of the word resulting from the stemming process.
		 */
		public int GetResultLength() 
		{
			return _iEnd;
		}

		/**
		 * Returns a reference to a character buffer containing the results of
		 * the stemming process.  You also need to consult getResultLength()
		 * to determine the length of the result.
		 */
		public char[] GetResultBuffer() 
		{
			return _b;
		}

		/* cons(i) is true <=> b[i] is a consonant. */
		private bool cons(int i) 
		{
			switch (_b[i]) 
			{
				case 'a': case 'e': case 'i': case 'o': case 'u': return false;
				case 'y': return (i==0) || !cons(i-1);
				default: return true;
			}
		}

		/* m() measures the number of consonant sequences between 0 and j. if c is
		   a consonant sequence and v a vowel sequence, and <..> indicates arbitrary
		   presence,

			  <c><v>       gives 0
			  <c>vc<v>     gives 1
			  <c>vcvc<v>   gives 2
			  <c>vcvcvc<v> gives 3
			  ....
		*/
		private int m() 
		{
			int n = 0;
			int i = 0;
			while(true) 
			{
				if (i > _j) return n;
				if (! cons(i)) break; i++;
			}
			i++;
			while(true) 
			{
				while(true) 
				{
					if (i > _j) return n;
					if (cons(i)) break;
					i++;
				}
				i++;
				n++;
				while(true) 
				{
					if (i > _j) return n;
					if (! cons(i)) break;
					i++;
				}
				i++;
			}
		}

		/* vowelinstem() is true <=> 0,...j contains a vowel */
		private bool vowelinstem() 
		{
			int i;
			for (i = 0; i <= _j; i++)
				if (! cons(i))
					return true;
			return false;
		}

		/* doublec(j) is true <=> j,(j-1) contain a double consonant. */
		private bool doublec(int j) 
		{
			if (j < 1)
				return false;
			if (_b[j] != _b[j-1])
				return false;
			return cons(j);
		}

		/* cvc(i) is true <=> i-2,i-1,i has the form consonant - vowel - consonant
		   and also if the second c is not w,x or y. this is used when trying to
		   restore an e at the end of a short word. e.g.

			  cav(e), lov(e), hop(e), crim(e), but
			  snow, box, tray.

		*/
		private bool cvc(int i) 
		{
			if (i < 2 || !cons(i) || cons(i-1) || !cons(i-2))
				return false;
			int ch = _b[i];
			if (ch == 'w' || ch == 'x' || ch == 'y')
				return false;
			return true;
		}

		private bool ends(String s) 
		{
			int l = s.Length;
			int o = _k-l+1;
			if (o < 0)
				return false;
			char[] sc = s.ToCharArray();
			for (int i = 0; i < l; i++)
				if (_b[o+i] != sc[i])
					return false;
			_j = _k-l;
			return true;
		}

		/* setto(s) sets (j+1),...k to the characters in the string s, readjusting
		   k. */
		private void setto(String s) 
		{
			int l = s.Length;
			int o = _j+1;
			char[] sc = s.ToCharArray();
			for (int i = 0; i < l; i++)
				_b[o+i] = sc[i];
			_k = _j+l;
		}

		/* r(s) is used further down. */
		private void r(String s) 
		{
			if (m() > 0)
				setto(s);
		}

		/* step1() gets rid of plurals and -ed or -ing. e.g.
			   caresses  ->  caress
			   ponies    ->  poni
			   ties      ->  ti
			   caress    ->  caress
			   cats      ->  cat

			   feed      ->  feed
			   agreed    ->  agree
			   disabled  ->  disable

			   matting   ->  mat
			   mating    ->  mate
			   meeting   ->  meet
			   milling   ->  mill
			   messing   ->  mess

			   meetings  ->  meet

		*/

		private void step1() 
		{
			if (_b[_k] == 's') 
			{
				if (ends("sses"))
					_k -= 2;
				else if (ends("ies"))
					setto("i");
				else if (_b[_k-1] != 's')
					_k--;
			}
			if (ends("eed")) 
			{
				if (m() > 0)
					_k--;
			} 
			else if ((ends("ed") || ends("ing")) && vowelinstem()) 
			{
				_k = _j;
				if (ends("at"))
					setto("ate");
				else if (ends("bl"))
					setto("ble");
				else if (ends("iz"))
					setto("ize");
				else if (doublec(_k)) 
				{
					_k--;
					int ch = _b[_k];
					if (ch == 'l' || ch == 's' || ch == 'z')
						_k++;
				}
				else if (m() == 1 && cvc(_k)) setto("e");
			}
		}

		/* step2() turns terminal y to i when there is another vowel in the stem. */
		private void step2() 
		{
			if (ends("y") && vowelinstem())
				_b[_k] = 'i';
		}

		/* step3() maps double suffices to single ones. so -ization ( = -ize plus
		   -ation) maps to -ize etc. note that the string before the suffix must give
		   m() > 0. */
		private void step3() 
		{
			if (_k == 0)
				return;
			
			/* For Bug 1 */
			switch (_b[_k-1]) 
			{
				case 'a':
					if (ends("ational")) { r("ate"); break; }
					if (ends("tional")) { r("tion"); break; }
					break;
				case 'c':
					if (ends("enci")) { r("ence"); break; }
					if (ends("anci")) { r("ance"); break; }
					break;
				case 'e':
					if (ends("izer")) { r("ize"); break; }
					break;
				case 'l':
					if (ends("bli")) { r("ble"); break; }
					if (ends("alli")) { r("al"); break; }
					if (ends("entli")) { r("ent"); break; }
					if (ends("eli")) { r("e"); break; }
					if (ends("ousli")) { r("ous"); break; }
					break;
				case 'o':
					if (ends("ization")) { r("ize"); break; }
					if (ends("ation")) { r("ate"); break; }
					if (ends("ator")) { r("ate"); break; }
					break;
				case 's':
					if (ends("alism")) { r("al"); break; }
					if (ends("iveness")) { r("ive"); break; }
					if (ends("fulness")) { r("ful"); break; }
					if (ends("ousness")) { r("ous"); break; }
					break;
				case 't':
					if (ends("aliti")) { r("al"); break; }
					if (ends("iviti")) { r("ive"); break; }
					if (ends("biliti")) { r("ble"); break; }
					break;
				case 'g':
					if (ends("logi")) { r("log"); break; }
					break;
				default :
					break;
			}
		}

		/* step4() deals with -ic-, -full, -ness etc. similar strategy to step3. */
		private void step4() 
		{
			switch (_b[_k]) 
			{
				case 'e':
					if (ends("icate")) { r("ic"); break; }
					if (ends("ative")) { r(""); break; }
					if (ends("alize")) { r("al"); break; }
					break;
				case 'i':
					if (ends("iciti")) { r("ic"); break; }
					break;
				case 'l':
					if (ends("ical")) { r("ic"); break; }
					if (ends("ful")) { r(""); break; }
					break;
				case 's':
					if (ends("ness")) { r(""); break; }
					break;
			}
		}

		/* step5() takes off -ant, -ence etc., in context <c>vcvc<v>. */
		private void step5() 
		{
			if (_k == 0)
				return;

			/* for Bug 1 */
			switch ( _b[_k-1] ) 
			{
				case 'a':
					if (ends("al")) break; return;
				case 'c':
					if (ends("ance")) break;
					if (ends("ence")) break; return;
				case 'e':
					if (ends("er")) break; return;
				case 'i':
					if (ends("ic")) break; return;
				case 'l':
					if (ends("able")) break;
					if (ends("ible")) break; return;
				case 'n':
					if (ends("ant")) break;
					if (ends("ement")) break;
					if (ends("ment")) break;
					/* element etc. not stripped before the m */
					if (ends("ent")) break; return;
				case 'o':
					if (ends("ion") && _j >= 0 && (_b[_j] == 's' || _b[_j] == 't')) break;
					/* j >= 0 fixes Bug 2 */
					if (ends("ou")) break; return;
					/* takes care of -ous */
				case 's':
					if (ends("ism")) break; return;
				case 't':
					if (ends("ate")) break;
					if (ends("iti")) break; return;
				case 'u':
					if (ends("ous")) break; return;
				case 'v':
					if (ends("ive")) break; return;
				case 'z':
					if (ends("ize")) break; return;
				default:
					return;
			}
			if (m() > 1)
				_k = _j;
		}

		/* step6() removes a final -e if m() > 1. */
		private void step6() 
		{
			_j = _k;
			
			if (_b[_k] == 'e') 
			{
				int a = m();
				if (a > 1 || a == 1 && !cvc(_k-1))
					_k--;
			}
			if (_b[_k] == 'l' && doublec(_k) && m() > 1)
				_k--;
		}

		/** Stem the word placed into the Stemmer buffer through calls to add().
		 * Returns true if the stemming process resulted in a word different
		 * from the input.  You can retrieve the result with
		 * getResultLength()/getResultBuffer() or toString().
		 */
		public void stem() 
		{
			_k = _i - 1;
			if (_k > 1) 
			{
				step1();
				step2();
				step3();
				step4();
				step5();
				step6();
			}
			_iEnd = _k+1;
			_i = 0;
		}


	}
}