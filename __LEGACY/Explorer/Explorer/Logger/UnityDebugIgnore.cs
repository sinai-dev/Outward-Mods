using System.Collections.Generic;

namespace Explorer
{

    //store message and message Color data
    public class IgnoreData
    {
        internal List<string> m_ignoreInDefaultCompiler = new List<string>();

        //Add a list of strings to the ignoreList
        public void AddToIgnore(params string[] _toAdd)
        {
            for (int a = 0; a < _toAdd.Length; a++)
            {
                m_ignoreInDefaultCompiler.Add(_toAdd[a]);
            }
        }

        //Remove a list of strings from the ignoreList
        public void RemoveFromIgnore(params string[] _toRemove)
        {
            for (int a = 0; a < _toRemove.Length; a++)
            {
                m_ignoreInDefaultCompiler.Remove(_toRemove[a]);
            }
        }

    }

}