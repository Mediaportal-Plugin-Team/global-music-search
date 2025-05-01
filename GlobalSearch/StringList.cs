/* TStringList port for .net
 * 
 * This version is originally created in 2003 by Sebastian Moleski  (Email: s.moleski[at]tcu.edu)
 * This version is adapted for GlobalSerach by d-fader at may 28, 2007.
 * 
 * Bugfixes:
 * - DelimitedText (and thus CommaText) didn't work, fixed.
 * 
 * Changes:
 * - Changed TStrings and TStringList to resp. Strings and StringList (following .net naming conventions)
 * - Changed namespace to MediaPortal.Search
 */

using System;
using System.Collections;
using System.Globalization;

namespace MediaPortal.Search {

	internal class TStringItem {
		public string FString;
		public object FObject;
		
		public TStringItem(string AString, object AObject) {
			FString = AString;
			FObject = AObject;
		}
	}
	
	public delegate int StringListSortCompare(StringList AList, int AIndex1,
		int AIndex2);
		
	public enum TDuplicates {
		Ignore, Accept, Error
	};

	public class StringList : Strings {
	// private
		private ArrayList FList = new ArrayList();
		private bool FSorted;
		private TDuplicates FDuplicates;
		private bool FCaseSensitive;
		private const string SListIndexError = "Index {0} out of bounds.";
		private const string SSortedListError = "Can't insert into sorted list.";
		
		
		private void ExchangeItems(int AIndex1, int AIndex2) {
			Object temp = FList[AIndex1];
			FList[AIndex1] = FList[AIndex2];
			FList[AIndex2] = temp;
		}
		
		private void Grow() {
			int delta, cap = FList.Capacity;
			if (cap > 64)
				delta = cap / 4;
			else if (cap > 8)
				delta = 16;
			else
				delta = 4;
				
			FList.Capacity = cap + delta;
		}
		
		private void QuickSort(int ALeft, int ARight, StringListSortCompare ACompare) {
			int i, j, p;
			do {
				i = ALeft;
				j = ARight;
				p = (ALeft + ARight) / 2;
				do {
					while (ACompare(this, i, p) < 0)
						++i;
						
					while (ACompare(this, j, p) > 0)
						--j;
						
					if (i <= j) {
						ExchangeItems(i, j);
						if (p == i)
							p = j;
						else if (p == j)
							p = i;
						++i;
						--j;
					}
				} while (i <= j);
				
				if (ALeft < j)
					QuickSort(ALeft, j, ACompare);
					
				ALeft = i;
			} while (i < ARight);
		}
		
		private void SetSorted(bool AValue) {
			if (FSorted != AValue) {
				if (AValue)
					Sort();
					
				FSorted = AValue;
			}
		}
		
		private void SetCaseInsensitive(bool AValue) {
			if (AValue != FCaseSensitive) {
				FCaseSensitive = AValue;
				
				if (Sorted)
					Sort();
			}
		}
		
	// protected interface
		protected virtual void Changed() {
			if (FUpdateCount == 0 && OnChange != null)
				OnChange(this, null);
		}
		
		protected virtual void Changing() {
			if (FUpdateCount == 0 && OnChanging != null)
				OnChanging(this, null);
		}
		
		protected override string Get(int AIndex) {
			if (AIndex < 0 || AIndex > FList.Count)
				Error(SListIndexError, AIndex);
			
			return (FList[AIndex] as TStringItem).FString;
		}
		
		protected override int GetCapacity() {
			return FList.Capacity;
		}
		
		protected override int GetCount() {
			return FList.Count;
		}
		
		protected override object GetObject(int AIndex) {
			if (AIndex < 0 || AIndex >= FList.Count)
				Error(SListIndexError, AIndex);
				
			return (FList[AIndex] as TStringItem).FObject;
		}
		
		protected override void Put(int AIndex, string AValue) {
			if (Sorted)
				Error(SSortedListError, 0);
				
			if (AIndex < 0 || AIndex >= FList.Count)
				Error(SListIndexError, AIndex);
				
			Changing();
			(FList[AIndex] as TStringItem).FString = AValue;
			Changed();
		}
		
		protected override void PutObject(int AIndex, object AObject) {
			if (AIndex < 0 || AIndex >= FList.Count)
				Error(SListIndexError, AIndex);
			
			Changing();
			(FList[AIndex] as TStringItem).FObject = AObject;
			Changed();
		}
		
		protected override void SetCapacity(int AValue) {	
			FList.Capacity = AValue;
		}
		
		protected override void SetUpdateState(bool AUpdating) {
			if (AUpdating)
				Changing();
			else
				Changed();
		}
		
		protected override int CompareStrings(string s1, string s2) {
			return String.Compare(s1, s2, !FCaseSensitive);
		}
		
		protected virtual void InsertItem(int AIndex, string AValue, object AObject) {
			Changing();
			
			if (FList.Count == FList.Capacity)
				Grow();
				
			FList.Insert(AIndex, new TStringItem(AValue, AObject));
			
			Changed();
		}
				
	// public interface
		~StringList() {
			OnChange = null;
			OnChanging = null;
			FList.Clear();
			SetCapacity(0);
		}
	
		public override object Clone() {
			StringList sl = new StringList();
			sl.Assign(this);
			return sl;
		}
		
		public override void CopyTo(Array a, int i) {
			FList.CopyTo(a, i);
		}	

		public override int Add(String AValue) {
			return AddObject(AValue, null);
		}
		
		public override int AddObject(string AValue, object AObject) {
			int result;
			
			if (!FSorted)
				result = FList.Count;
			else if (Find(AValue, out result))
				switch (Duplicates) {
					case TDuplicates.Ignore: 
						return result;
					case TDuplicates.Error: 
						Error("Duplicate string.", 0);
						return result;
				}

			InsertItem(result, AValue, AObject);
			
			return result;
		}
		
		public override void Clear() {
			if (FList.Count != 0) {
				Changing();
				FList.Clear();
				SetCapacity(0);
				Changed();
			}
		}
		
		public override void Delete(int AIndex) {
			if (AIndex < 0 || AIndex >= FList.Count)
				Error(SListIndexError, AIndex);
			
			Changing();
			FList.RemoveAt(AIndex);
			Changed();
		}
		
		public override void Exchange(int AIndex1, int AIndex2) {
			if (AIndex1 < 0 || AIndex1 >= FList.Count)
				Error(SListIndexError, AIndex1);
				
			if (AIndex2 < 0 || AIndex2 >= FList.Count)
				Error(SListIndexError, AIndex2);
				
			Changing();
			ExchangeItems(AIndex1, AIndex2);
			Changed();
		}
		
		public virtual bool Find(String AValue, out int AIndex) {
			bool result = false;
			int lo = 0;
			int hi = FList.Count - 1;
			while (lo <= hi) {
				int i = (lo + hi) / 2;
				int cmp = CompareStrings((FList[i] as TStringItem).FString, AValue);
				if (cmp < 0)
					lo = i + 1;
				else {
					hi = i - 1;
					if (cmp == 0) {
						result = false;
						if (Duplicates != TDuplicates.Accept)
							lo = i;
					}
				}
			}
			AIndex = lo;
			return result;
		}
		
		public override int IndexOf(string AValue) {
			if (!Sorted)
				return base.IndexOf(AValue);
			else {
				int result;
				return Find(AValue, out result) ? result : -1;
			}
		}
		
		public override void Insert(int AIndex, string AValue) {
			InsertObject(AIndex, AValue, null);
		}
		
		public override void InsertObject(int AIndex, string AValue, object AObject) {
			if (Sorted)
				Error(SSortedListError, 0);
				
			if (AIndex < 0 || AIndex >= FList.Count)
				Error(SListIndexError, AIndex);
				
			InsertItem(AIndex, AValue, AObject);
		}
		
		private static int StringListCompareStrings(StringList AList, int AIndex1,
			int AIndex2) {
			return AList.CompareStrings((AList.FList[AIndex1] as TStringItem).FString,
				(AList.FList[AIndex2] as TStringItem).FString);
		}
		
		public virtual void Sort() {
			CustomSort(new StringListSortCompare(StringListCompareStrings));
		}
		
		public virtual void CustomSort(StringListSortCompare ACompare) {
			if (!Sorted && FList.Count > 1) {
				Changing();
				QuickSort(0, FList.Count - 1, ACompare);
				Changed();
			}
		}
		
		public TDuplicates Duplicates {
			get { return FDuplicates; }
			set { FDuplicates = value; }
		}
		
		public bool Sorted {
			get { return FSorted; }
			set { SetSorted(value); }
		}
		
		public bool CaseSensitive {
			get { return FCaseSensitive; }
			set { FCaseSensitive = value; }
		}
		
		public event EventHandler OnChange;
		public event EventHandler OnChanging;
	}
}











