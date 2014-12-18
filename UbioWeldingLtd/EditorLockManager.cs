using System;
using System.Collections.Generic;

namespace UbioWeldingLtd
{
	public static class EditorLockManager
	{

		public class EditorLock
		{
			private bool _save;
			private bool _exit;
			private bool _load;
			private string _key;

			public EditorLock(bool save, bool exit, bool load, string key)
			{
				_save = save;
				_exit = exit;
				_load = load;
				_key = key;
			}

			public bool LockSave
			{
				get { return _save; }
				set { _save = value; }
			}

			public bool lockExit
			{
				get { return _exit; }
				set { _exit = value; }
			}

			public bool lockLoad
			{
				get { return _load; }
				set { _load = value; }
			}

			public string lockKey
			{
				get { return _key; }
				set { _key = value; }
			}
		}

		private static List<EditorLock> _activeLocks = new List<EditorLock>();


		/// <summary>
		/// locks the editor keys for the given key
		/// </summary>
		/// <param name="loadButton"></param>
		/// <param name="exitButton"></param>
		/// <param name="saveButton"></param>
		/// <param name="lockKey"></param>
		public static void lockEditor(bool loadButton, bool exitButton, bool saveButton, string lockKey)
		{
			if (!isLockKeyActive(lockKey))
			{
				EditorLogic.fetch.Lock(loadButton, exitButton, saveButton, lockKey);
				_activeLocks.Add(new EditorLock(loadButton, exitButton, loadButton, lockKey));
			}
		}


		/// <summary>
		/// unlocks the editor for the entered key
		/// </summary>
		/// <param name="lockKey"></param>
		public static void unlockEditor(string lockKey)
		{
			if (isLockKeyActive(lockKey))
			{
				EditorLogic.fetch.Unlock(lockKey);

				for (int i = 0; i < _activeLocks.Count; i++)
				{
					if (_activeLocks[i].lockKey == lockKey)
					{
						_activeLocks.RemoveAt(i);
						return;
					}
				}
			}
		}


		/// <summary>
		/// returns the info about the current lockstatus
		/// </summary>
		/// <returns></returns>
		public static bool isEditorLocked()
		{
			return _activeLocks.Count > 0 ? true : false;
		}


		/// <summary>
		/// provides all the keys that are currently in use
		/// </summary>
		/// <returns></returns>
		public static string[] getActiveLockKeys()
		{
			string[] locks = new string[_activeLocks.Count];
			for (int i = 0; i < locks.Length; i++)
			{
				locks[i] = _activeLocks[i].lockKey;
			}
			return locks;
		}


		/// <summary>
		/// provides the binary information if the key is already in use
		/// </summary>
		/// <param name="lockKey"></param>
		/// <returns></returns>
		public static bool isLockKeyActive(string lockKey)
		{
			foreach (EditorLock l in _activeLocks)
			{
				if (l.lockKey == lockKey)
				{
					return true;
				}
			}
			return false;
		}


		/// <summary>
		/// provides the information if the main buttons of the editor are locked
		/// </summary>
		/// <returns></returns>
		public static bool isEditorSoftlocked()
		{
			foreach (EditorLock l in _activeLocks)
			{
				if (l.LockSave && l.lockExit && l.lockLoad)
				{
					return true;
				}
			}
			return false;
		}


		/// <summary>
		/// resets the editorlocks to a clean state
		/// </summary>
		public static void resetEditorLocks()
		{
			_activeLocks.Clear();
		}

	}
}
