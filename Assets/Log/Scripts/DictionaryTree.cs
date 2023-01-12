using System;
using System.Collections;
using System.Collections.Generic;

public interface ITypedBranch : IEnumerable {
	bool TryGetValue(object[] path, int pathIndex, out object found);
	void SetValue(object[] path, int pathIndex, object value);
}

public class DictionaryTree<TYPE, BRANCH> : ITypedBranch, IEnumerable {
	Dictionary<TYPE, BRANCH> _branches = new Dictionary<TYPE, BRANCH>();
	public bool TryGet<LEAF_TYPE>(object[] path, out LEAF_TYPE found) {
		return TryGet<LEAF_TYPE>(path, 0, out found);
	}

	public bool TryGet<LEAF_TYPE>(object[] path, int pathIndex, out LEAF_TYPE found) {
		found = default;
		if (typeof(TYPE).IsAssignableFrom(path[pathIndex].GetType())) {
			bool itsWorking = _branches.TryGetValue((TYPE)path[pathIndex], out BRANCH branch);
			if (!itsWorking) {
				return false;
			}
			if (pathIndex == path.Length - 1) {
				if (typeof(LEAF_TYPE).IsAssignableFrom(typeof(BRANCH))) {
					found = (LEAF_TYPE)(object)branch;
					return true;
				}
				return false;
			}
			return TraverseNextIdentifier(branch, path, pathIndex, out found);
		}
		return false;
	}

	private bool TraverseNextIdentifier<LEAF_TYPE>(BRANCH branch, object[] path, int pathIndex, out LEAF_TYPE found) {
		ITypedBranch typedBranch = branch as ITypedBranch;
		if (typedBranch != null) {
			if (typedBranch.TryGetValue(path, pathIndex + 1, out object output)) {
				found = (LEAF_TYPE)output;
				return true;
			}
		} else {
			throw new Exception("unable to get branch with " + branch.GetType());
		}
		found = default;
		return false;
	}

	public void Set<LEAF_TYPE>(object[] path, LEAF_TYPE value) { Set<LEAF_TYPE>(path, 0, value); }

	public void Set<LEAF_TYPE>(object[] path, int pathIndex, LEAF_TYPE value) {
		bool itsWorking = _branches.TryGetValue((TYPE)path[pathIndex], out BRANCH branch);
		if (pathIndex == path.Length - 1) {
			if (value.GetType().IsAssignableFrom(typeof(BRANCH))) {
				_branches[(TYPE)path[pathIndex]] = (BRANCH)(object)value;
				return;
			}
			throw new System.Exception("expected type assignable to " + typeof(BRANCH) + ", given " + value.GetType());
		}
		if (!itsWorking) {
			branch = (BRANCH)Activator.CreateInstance(typeof(BRANCH), new object[] { });
			_branches[(TYPE)path[pathIndex]] = branch;
		}
		//Debug.Log(branch.GetType());
		ITypedBranch typedBranch = branch as ITypedBranch;
		if (typedBranch != null) {
			typedBranch.SetValue(path, pathIndex + 1, value);
		} else {
			throw new Exception("unable to set branch with " + branch.GetType());
		}
	}

	public bool TryGetValue(object[] path, int pathIndex, out object found) {
		bool result = TryGet(path, pathIndex, out object branch);
		found = branch;
		return result;
	}

	public void SetValue(object[] path, int pathIndex, object value) {
		Set(path, pathIndex, value);
	}

	public IEnumerator GetEnumerator() {
		foreach (KeyValuePair<TYPE,BRANCH> item in _branches) {
			ITypedBranch typedBranch = item.Value as ITypedBranch;
			if (typedBranch != null) {
				IEnumerator enumerator = typedBranch.GetEnumerator();
				while (enumerator.MoveNext()) {
					yield return enumerator.Current;
				}
			} else {
				yield return item.Value;
			}
		}
	}

	public void Clear() {
		_branches.Clear();
	}
}
